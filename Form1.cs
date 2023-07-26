using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Acrobat;
using AFORMAUTLib;
using Microsoft.Win32;
using WFA_PDF_PhraseFinder.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// data mining PDF appplication
// copyright 2023, Joseph Stateson  github/jstateson  

/*
 Program search for phrases in a PDF.  Adobe pro seems to be needed for the app to work
 The searching is done by looking at whole words extracted from a PDF.  The page the word or
 phrase was found on is displayed.  A count of the number of matching phrases is listed.
 This program was written as a concept with the idea that the phrases might be highlighted
 in a new ThisDoc so that the use can then look at the new ThisDoc and quickly locate
 what was written.
notes follow
setpage: C:\Program Files (x86)\Adobe\Acrobat 2015\Acrobat>acrobat /A "page=10" d:\msi-x299-raider.pdf
using itext(C#): https://dev.to/eliotjones/reading-a-pdf-in-c-on-net-core-43ef
get current page https://stackoverflow.com/questions/32450906/get-current-page-in-a-pdf-in-windows-application
set bookmark PDFView.initialBookmark = "page=5";
nice https://csharp.hotexamples.com/examples/-/AcroAVDoc/-/php-acroavdoc-class-examples.html
*/


namespace WFA_PDF_PhraseFinder
{
    public partial class Form1 : Form
    {
        CAcroApp acroApp;
        AcroAVDoc ThisDoc;
        CAcroAVPageView ThisDocView;
        int[] ThisPageList = null;
        int iCurrentPage=0;
        bool bThisDocOpen = false;

        private int NumPhrases = 5;
        private long TotalPDFPages, TotalMatches;
        public IFields theseFields = null;
        private bool bFormDirty = false;
        private StringCollection scSavedWords;
        private class cLocalSettings         // used to restore user settings
        {
            public bool bExitEarly;         // for debugging or demo purpose only examine a limited number of page
            public string strLastFolder;    // where last PDF was obtained
        }

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

            // count the number of following words that must match
            private int CountWords(string strIn)
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
            public void AddPage(int jPage) // do not add the same page twice
            {
                int iPage = jPage + 1;
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

        List<cPhraseTable> phlist = new List<cPhraseTable>();   // table of phrases
        cLocalSettings LocalSettings = new cLocalSettings();    // table of settings


        //string[] InitialPhrase = new string[NumPhrases] { " prorated ", " lender & grant ", " lender", " grant ", " contract & school & lunches " };
        // the above using "&" was not implementable because I was unable to read a line and know that two words were on the
        // same line.  Since the SDK retrieves whole words there is no need for a space before or after a phrase
        string[] InitialPhrase = new string[5] { "and", "address", "make sure", "motherboard", "memory" };
        string[] WorkingPhrases = new string[5]; // same as above but optomises somewhat for case sensitivity

        //show a simple date on the form
        private string GetSimpleDate(string sDT)
        {
            //Sun 06/09/2019 23:33:53.18 
            int i = sDT.IndexOf(' ');
            i++;
            int j = sDT.LastIndexOf('.');
            return sDT.Substring(i, j - i);
        }
        
        // must have at least 2 letters
        bool bMatchWord(string word, int iPage, int jMax, ref int jWord, ref bool bError)
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
                    // workingphrases do not have the "following words" so had to index into phlist
                    // was trying to optimise the matching by not haveing to adjust case all the time
                    // but didnt think this out too clearly so had to use phlist for more than 1 word
                    for(int j = 0; j < n; j++)
                    {
                        jWord++;
                        if (jWord == jMax) return false; // do not read past the end of the page or ThisDoc
                        word = GetThisWord(jWord, jMax, iPage, ref bError); //need to peek for the next word
                        if (bError) return false; // some PDFs are corrupted I discovered
                        if(cbIgnoreCase.Checked)word = word.ToLower();
                        strTemp = phlist[i].strInSeries[j+1]; // the phlist first word was already checked
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

        // count number of words in the phrase
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

        //progress bar
        private void SetPBAR(int p)
        {
            double pbarSlope =  pbarLoading.Maximum * p;
            pbarSlope /= TotalPDFPages;
            pbarLoading.Value = Convert.ToInt32( pbarSlope);
            pbarLoading.Update();
            pbarLoading.Refresh();
            Application.DoEvents();
        }

        //open file needs adobe professional (not always found) in addition to badly formed PDFs
        // i need to give warning if PRO is not on the system
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
                tbPdfName.Text = "missing Adobe DLL or file:" + tbPdfName.Text;
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
                MessageBox.Show("Error in PDF at page " + iCurrentPage);
                bError = true;
                return "";
            }
            return chkWord;
        }

        private void ViewDoc(string fileName)
        {
            ThisDoc = new AcroAVDoc();
            ThisDoc.Open(fileName, "");
            ThisDoc.BringToFront();
            ThisDoc.SetViewMode(2); // PDUseThumbs
            ThisDocView = ThisDoc.GetAVPageView() as CAcroAVPageView;
            //pageView.ZoomTo(1 /*AVZoomFitPage*/, 100);
            ThisDocView.GoTo(iCurrentPage-1);
        }

        // this starts the search.  note that the file is closed after the search
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
            //DocTest(strPath);
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
            SetPBAR(0);
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
            gbPageCtrl.Visible = TotalMatches > 0;  // show page control only if matches found
            return true;
        }

        // fill in the phrase table list from the initial (hard coded) list
        // the hard coded list is automatically replaced by any saved list
        // from windows resource or via the import feature
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

        //the phrase list is saved into windows AppData 
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

        // should have given the form a better name but I suspect it is too late
        /// <summary>
        /// if AppData contains a list of phrases that list is used in place of the hard coded list
        /// if there is no saved list then the hard coded one is saved
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            try
            {
                acroApp = new AcroAppClass();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Adobe PRO or STANDARD is not present");
                this.Close();
            }
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
            GetLocalSettings();
            tbPdfName.Text = "Build date: " + GetSimpleDate(Properties.Resources.BuildDate) + " (v) 1.0 (c)Stateson";
            //AdobePresent();
        }


        /// <summary>
        /// this opens the PDF and it is always closed so no need (?) for a close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.DefaultExt = "*pdf";
            ofd.InitialDirectory = LocalSettings.strLastFolder;
            ofd.Filter = "(Adobe PDF)|*.pdf";

            if (DialogResult.OK != ofd.ShowDialog())
            {
                tbPdfName.Text = "ERROR:no PDF file found";
                btnRunSearch.Enabled = false;
                return;
            }
            tbPdfName.Text = ofd.FileName;
            LocalSettings.strLastFolder = Path.GetDirectoryName(ofd.FileName);
            if(bFormDirty)
            {
                FillPhrases();
                bFormDirty = false;
            }
            // enable the run button if a docuement was loaded
            btnRunSearch.Enabled =  bOpenDocs(tbPdfName.Text);
        }

        //close was not needed as the file is opened and closed after every search
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
            }
            btnRunSearch.Enabled = false;
            RunSearch();
            btnRunSearch.Enabled = true;
        }

        // the phrase list was edited so copy the edits so they can be saved
        // the searching is done using using the InitialPhrase list
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

        // a made up phrase is inserted into phlist and the user
        // needs to edit that and must also save it (if desired)
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

        // any phrases selected are deleted but the user needs to save the edits
        private void btnRemove_Click(object sender, EventArgs e)
        {
            int j=0, n = 0;
            for (int i = 0; i < NumPhrases; i++)
                if (phlist[i].Select)
                    n++;
            DialogResult dialogResult = MessageBox.Show(
                "This operation will delete " + n + " filter phrases.  Are you sure?", 
                "Warning: don't forget to save", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                //remove items from phlist and put into a new InitialPhrase list
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

        // copy the phrases onto the windows clipboard
        private void btnExport_Click(object sender, EventArgs e)
        {
            string strOut = "";
            for (int i = 0; i < NumPhrases; i++)
            {
                strOut += phlist[i].Phrase + "\r\n";
            }
            System.Windows.Forms.Clipboard.SetText(strOut);
        }

        //import phrases from the windows clipboard
        private void btnImport_Click(object sender, EventArgs e)
        {
            string strTemp = System.Windows.Forms.Clipboard.GetText();
            if(strTemp == "")
            {
                MessageBox.Show("Clipboard is empty.  Do an export to see the correct format");
                return;
            }
            string[] strTemps = Regex.Split(strTemp, "\r\n");
            NumPhrases = strTemps.Count();
            InitialPhrase = new string[NumPhrases];
            WorkingPhrases = new string[NumPhrases];
            for(int i = 0; i < NumPhrases;i++)
            {
                InitialPhrase[i] = strTemps[i];
            }
            FillPhrases();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveLocalSettings();
        }

        private void SaveLocalSettings()
        {
            Properties.Settings.Default.lsExitEarly = LocalSettings.bExitEarly;
            Properties.Settings.Default.lsLastFolder = LocalSettings.strLastFolder;
            Properties.Settings.Default.Save();
        }

        private void GetLocalSettings()
        {
            LocalSettings.bExitEarly = Properties.Settings.Default.lsExitEarly;
            LocalSettings.strLastFolder = Properties.Settings.Default.lsLastFolder;
            cbStopEarly.Checked = LocalSettings.bExitEarly;
            if (LocalSettings.strLastFolder == "")
            {
                LocalSettings.strLastFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                SaveLocalSettings();
            }
        }

        private bool AdobePresent()
        {

            RegistryKey adobe = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Adobe");
            if (null == adobe)
            {
                var policies = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Policies");
                if (null == policies)
                    return false ;
                adobe = policies.OpenSubKey("Adobe");
            }
            if (adobe != null)
            {
                RegistryKey acroRead = adobe.OpenSubKey("Adobe Acrobat");
                if (acroRead != null)
                {
                    string[] acroReadVersions = acroRead.GetSubKeyNames();
                    string strTemp = "The following version(s) of Acrobat Reader are installed:\r\n ";
                    foreach (string versionNumber in acroReadVersions)
                    {
                        strTemp += versionNumber + "\r\n";
                    }
                }
                return true;
            }
            return false;
        }

        private void cbStopEarly_CheckedChanged(object sender, EventArgs e)
        {
            LocalSettings.bExitEarly = cbStopEarly.Checked;
        }

        private void GetSelection()
        {
            //DataGridViewRow ThisRow = dgv_phrases.CurrentRow;
            Point ThisRC = dgv_phrases.CurrentCellAddress;
            int iRow = ThisRC.Y;
            int iCol = ThisRC.X;
            ThisPageList = phlist[iRow].strPages.Split(',').Select(int.Parse).ToArray();
            nudPage.Maximum = ThisPageList.Length - 1;
            tbViewPage.Text = ThisPageList[0].ToString();
        }

        private void dgv_phrases_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void dgv_phrases_Click(object sender, EventArgs e)
        {
            GetSelection();
        }



        private void nudPage_ValueChanged(object sender, EventArgs e)
        {
            if (ThisPageList == null) return;
            int iVal = Convert.ToInt32(nudPage.Value);
            iCurrentPage = ThisPageList[iVal];
            tbViewPage.Text = iCurrentPage.ToString();
            if (ThisDocView != null)
            {
                ThisDocView.GoTo(iCurrentPage-1);
            }
        }

        private void btnViewDoc_Click(object sender, EventArgs e)
        {
            ViewDoc(tbPdfName.Text);
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}
