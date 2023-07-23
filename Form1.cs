using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Acrobat;
using AFORMAUTLib;
using WFA_PDF_PhraseFinder.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


//author below is expert on PDF programming
//http://khkonsulting.com/2014/04/extract-pdf-pages-based-content/

namespace WFA_PDF_PhraseFinder
{
    public partial class Form1 : Form
    {

        private int NumPhrases = 5;
        private long TotalPDFPages, TotalMatches;
        public IFields theseFields = null;
        private bool bFormDirty = false;
        private StringCollection scSavedWords;

        private class cPhraseTable
        {
            public bool Select { get; set; }
            public string Phrase { get; set; }
            public string Number { get; set; }
            public int iNumber;
            public int iDupPageCnt;
            public int iLastPage;
            public string strPages;
            public string[] strInSeries;
            public int nFollowing; // number of words to check in sequence

            private int CountWords(string strIn) // count the number of following words that must match
            {
                char[] delimiters = new char[] { ' ', '\r', '\n' };
                strInSeries = strIn.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                int n = strInSeries.Length;
                if (n > 1) return n - 1; // if 2 words then must check one more word
                return 0;
            }
            public void InitPhrase(string aPhrase)
            {
                Select = true;
                Number = " ";
                iNumber = 0;
                strPages = "";
                iDupPageCnt = 0;
                iLastPage = -1;
                Phrase = aPhrase;
                nFollowing = CountWords(aPhrase);
            }
            public void AddPage(int iPage) // do not add the same page twice
            {
                if (strPages == "")
                {
                    strPages = iPage.ToString();
                    iLastPage = iPage;
                }
                else
                {
                    if(iLastPage == iPage)
                    {
                        iDupPageCnt++;
                        return;
                    }
                    strPages += "," + iPage.ToString();
                    iLastPage = iPage;
                }
            }
            public void IncMatch()
            {
                iNumber++;
            }
        }

        List<cPhraseTable> phlist = new List<cPhraseTable>();
        //string[] InitialPhrase = new string[NumPhrases] { " prorated ", " lender & grant ", " lender", " grant ", " contract & school & lunches " };
        string[] InitialPhrase = new string[5] { "and", "address", "make sure", "motherboard", "memory" };
        string[] WorkingPhrases = new string[5];

        private string GetSimpleDate(string sDT)
        {
            //Sun 06/09/2019 23:33:53.18 
            int i = sDT.IndexOf(' ');
            i++;
            int j = sDT.LastIndexOf('.');
            return sDT.Substring(i, j - i);
        }
        
        // must have at least 2 letters
        bool bMatchWord(string word, int iPage,int jMax, ref int jWord, ref bool bError)
        {
            int n;
            string strTemp = "";
            if (word == null) return false;
            if (word.Length < 2) return false;
            if(cbIgnoreCase.Checked)word = word.ToLower();
            for(int i=0; i <NumPhrases;i++)
            {
                if (!phlist[i].Select) continue;
                if(word == WorkingPhrases[i])
                {
                    n = phlist[i].nFollowing;
                    if (n == 0)
                    {
                        phlist[i].IncMatch();
                        phlist[i].AddPage(iPage);
                        return true;
                    }
                    // need to check the following words of the phrase
                    for(int j = 0; j < n; j++)
                    {
                        jWord++;
                        if (jWord == jMax) return false;
                        word = GetThisWord(jWord, jMax, iPage, ref bError);
                        if (bError) return false;
                        if(cbIgnoreCase.Checked)word = word.ToLower();
                        strTemp = phlist[i].strInSeries[j+1];
                        if (cbIgnoreCase.Checked) strTemp = strTemp.ToLower();
                        if (strTemp != word) return false;
                    }
                    phlist[i].IncMatch();
                    phlist[i].AddPage(iPage);
                    return true;
                }
            }
            return false;
        }

        private long GetMatchCount()
        {
            long lCnt = 0;
            for(int i = 0; i < NumPhrases;i++)
            {
                int j = phlist[i].iNumber;
                lCnt += j;
                phlist[i].Number = j.ToString();
            }
            return lCnt;
        }

        private void SetPBAR(int p)
        {
            double pbarSlope =  pbarLoading.Maximum * p;
            pbarSlope /= TotalPDFPages;
            pbarLoading.Value = Convert.ToInt32( pbarSlope);
            pbarLoading.Update();
            pbarLoading.Refresh();
            Application.DoEvents();
        }


        private void IncrementPBAR()
        {
            pbarLoading.PerformStep();
            pbarLoading.Update();
            pbarLoading.Refresh();
            Application.DoEvents();
        }

        private bool bOpenDocs(string strPath)
        {
            try
            {
                AcroPDDocClass objPages = new AcroPDDocClass();
                objPages.Open(strPath);
                TotalPDFPages = objPages.GetNumPages();
                tbNumPages.Text = TotalPDFPages.ToString();
                objPages.Close();
            }
            catch
            {
                tbPdfName.Text = "bad PDF file:" + tbPdfName.Text;
                return false;
            }
            return true;
        }

        // get the next word in the PDF
        private string GetThisWord(int iCurrent, int iLastWord, int iCurrentPage, ref bool bError)
        {
            string chkWord = "";
            try
            {
                chkWord = theseFields.ExecuteThisJavascript("event.value=this.getPageNthWord(" + iCurrentPage + "," + iCurrent + ", true);");
            }
            catch
            {
                MessageBox.Show(" error in PDF page " + iCurrentPage);
                bError = true;
                return "";
            }
            return chkWord;
        }

        private bool RunSearch()
        {
            string strPath = tbPdfName.Text;
            AcroAVDocClass avDoc = new AcroAVDocClass();
            IAFormApp formApp = new AFormAppClass();
            int jWord; // used to check for following words in a phrase match
            bool bError = false; 
            try
            {
                avDoc.Open(strPath, "Title");
                theseFields = (IFields)formApp.Fields;
            }
            catch
            {
                tbPdfName.Text = "corrupt pdf:" + tbPdfName.Text;
                return false;
            }

            string OutText = "";
            string chkWord = "";
            TotalMatches = 0;
            //            StreamWriter sw = new StreamWriter(@"D:\java\output.txt", false);

            for (int p = 0; p < TotalPDFPages; p++)
            {
                //string strTemp = theseFields.ExecuteThisJavascript("event.value=this.getPageNumWords(" + p + ");");
                //if (strTemp == null) continue;
                jWord = -1;
                SetPBAR(p);
                if (p > 40 && cbStopEarly.Checked) break;
                int numWords = int.Parse(theseFields.ExecuteThisJavascript("event.value=this.getPageNumWords(" + p + ");"));
                for (int i = 0; i < numWords; i++)
                {
                    jWord++;
                    if (jWord == numWords) break;
                    chkWord = GetThisWord(jWord, numWords, p, ref bError);
                    if (bError) return false;
                    //OutText = OutText + " " + chkWord;
                    if(bMatchWord(chkWord,p,numWords, ref jWord, ref bError))
                    {
                        //found a match and have counted it
                        if(bError) return false;
                    }
                }
            }
            for(int i = 0; i < NumPhrases;i++)
            {
                if (phlist[i].iNumber > 0)
                {
                    OutText += ">" + phlist[i].Phrase.ToUpper() + "<    found on following pages\r\n";
                    OutText += phlist[i].strPages + "\r\n";
                    OutText += "Total Duplicate pages: "+ phlist[i].iDupPageCnt + "\r\n\r\n";
                }
            }
            tbMatches.Text = OutText;
            TotalMatches = GetMatchCount();
            tbTotalMatch.Text = TotalMatches.ToString();
            avDoc.Close(0);
            avDoc = null;
            formApp = null;
            dgv_phrases.DataSource = phlist.ToArray();
            bFormDirty = true;
            return true;
        }


        private void FillPhrases()
        {
            cPhraseTable cpt;
            phlist.Clear();
            for (int i = 0; i < NumPhrases; i++)
            {
                cpt = new cPhraseTable();
                cpt.InitPhrase(InitialPhrase[i]);
                phlist.Add(cpt);
            }
            dgv_phrases.DataSource = phlist.ToArray();                    
        }

        private void SaveSettings()
        {
            // should be at AppData\Local\Microsoft\YOUR APPLICATION NAME File name is user.config
            scSavedWords = new StringCollection();
            foreach (string str1 in InitialPhrase)
            {
                scSavedWords.Add(str1);
            }
            Properties.Settings.Default.SearchPhrases = scSavedWords;
            Properties.Settings.Default.Save();
        }

        public Form1()
        {
            InitializeComponent();
            scSavedWords = new StringCollection();
            int n = 0;
            if( null != Properties.Settings.Default.SearchPhrases)
                n = Properties.Settings.Default.SearchPhrases.Count;
            if (n > 0)
            {
                string[] tempArr = new string[Properties.Settings.Default.SearchPhrases.Count];
                Properties.Settings.Default.SearchPhrases.CopyTo(tempArr, 0);
                scSavedWords.AddRange(tempArr);
                InitialPhrase = new string[n];
                WorkingPhrases = new string[n];
                NumPhrases = n;
                for (int i=0; i < n; i++)
                {
                    InitialPhrase[i] = scSavedWords[i];
                }
            }
            else
            {
                SaveSettings();
            }
            FillPhrases();
            tbPdfName.Text = "Build date: " + GetSimpleDate(Properties.Resources.BuildDate) + " (v) 1.0 Stateson";
        }



        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string str_LookHere = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofd = new OpenFileDialog();
            ofd.DefaultExt = "*pdf";
            ofd.InitialDirectory = str_LookHere;
            ofd.Filter = "(Adobe PDF)|*.pdf";

            if (DialogResult.OK != ofd.ShowDialog())
            {
                tbPdfName.Text = "ERROR:no PDF file found";
                btnRunSearch.Enabled = false;
                return;
            }
            tbPdfName.Text = ofd.FileName;
            if(bFormDirty)
            {
                FillPhrases();
                bFormDirty = false;
            }
            btnRunSearch.Enabled =  bOpenDocs(tbPdfName.Text);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NumPhrases; i++)
            {
                dgv_phrases.Rows[i].Cells[0].Value = true;
            }
        }

        private void btnUncheckall_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NumPhrases; i++)
            {
                dgv_phrases.Rows[i].Cells[0].Value = false;
            }
        }

        private void btnInvert_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < NumPhrases; i++)
            {
                dgv_phrases.Rows[i].Cells[0].Value = !(bool)dgv_phrases.Rows[i].Cells[0].Value;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btnRunSearch_Click(object sender, EventArgs e)
        {
            // create the search list used for searching
            if (bFormDirty)
            {
                FillPhrases();
                bFormDirty = false;
            }
            for (int i = 0; i < NumPhrases; i++)
            {
                WorkingPhrases[i] = cbIgnoreCase.Checked ? phlist[i].strInSeries[0].ToLower() : phlist[i].strInSeries[0];
                //InitialPhrase[i].ToLower() : InitialPhrase[i]; 
            }
            RunSearch();
        }

        private void UpdateSettings()
        {
            for (int i = 0; i < NumPhrases; i++)
            {
                string strTemp = phlist[i].Phrase;
                InitialPhrase[i] = strTemp.Trim();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            UpdateSettings();
            SaveSettings();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int n = InitialPhrase.Length + 1;
            string[] OldPhrases = new string[n];
            WorkingPhrases = new string[n];
            for (int i = 0; i < n-1;i++)
                OldPhrases[i] = InitialPhrase[i]; 
            InitialPhrase = new string[n];
            for (int i = 0; i < n-1; i++)
                InitialPhrase[i] = OldPhrases[i];
            InitialPhrase[n - 1] = "change me then SAVE";
            NumPhrases = n;
            FillPhrases();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int j=0, n = 0;
            for (int i = 0; i < NumPhrases; i++)
                if (phlist[i].Select)
                    n++;
            DialogResult dialogResult = MessageBox.Show(
                "This operation will delete " + n + " files.  Are you sure?", 
                "Warning", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                //remove items from phlist
                n = NumPhrases - n; // this many to keep
                WorkingPhrases = new string[n];
                for (int i = 0; i < NumPhrases; i++)
                {
                    if (!phlist[i].Select)
                    {
                        WorkingPhrases[j] = InitialPhrase[i];
                        j++;
                    }
                }
                InitialPhrase = new string[j];
                for(int i = 0;i < j; i++)
                {
                    InitialPhrase[i] = WorkingPhrases[i];
                }
                NumPhrases = j;
                FillPhrases();
            }

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
