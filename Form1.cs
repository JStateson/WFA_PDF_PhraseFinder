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
 The adobe sdk is required. 
 This program was written as a concept with the idea that the phrases might be highlighted
 in a new ThisDoc so that the use can then look at the new This Doc and quickly locate
 what was written.


program was written using window forms app. I used the older frameworks by mistake
https://stackoverflow.com/questions/65304110/difference-between-windows-forms-app-vs-windows-forms-app-net-framework


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
        private CAcroApp acroApp;
        private AcroAVDoc ThisDoc;
        private CAcroAVPageView ThisDocView;
        private int[] ThisPageList = null;
        private int iCurrentPage=0;
        private bool bThisDocOpen = false;

        private int NumPhrases = 5;
        private long TotalPDFPages, TotalMatches;
        private IFields theseFields = null;
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

        private List<cPhraseTable> phlist = new List<cPhraseTable>();   // table of phrases
        private cLocalSettings LocalSettings = new cLocalSettings();    // table of settings


        //string[] InitialPhrase = new string[NumPhrases] { " prorated ", " lender & grant ", " lender", " grant ", " contract & school & lunches " };
        // the above using "&" was not implementable because I was unable to read a line and know that two words were on the
        // same line.  Since the SDK retrieves whole words there is no need for a space before or after a phrase
        private string[] InitialPhrase = new string[5] { "and", "address", "make sure", "motherboard", "memory" };
        private string[] WorkingPhrases = new string[5]; // same as above but optomises somewhat for case sensitivity

        //show a simple date on the form
        private string GetSimpleDate(string sDT)
        {
            //Sun 06/09/2019 23:33:53.18 
            int i = sDT.IndexOf(' ');
            i++;
            int j = sDT.LastIndexOf('.');
            return sDT.Substring(i, j - i);
        }

        /// <summary>
        /// must have at least 2 letters
        /// WorkingPhrases are only single words, not phrases.  they are in the correct case  to do the matching
        /// this is to optimize the search as I thought, perhaps wrongly that only a single word was needed
        /// for actual phrases WorkingPhrase only has the first word the phlist needs to be accessed to
        /// check additional words that make up the phrase. To do this the document must be read to get
        /// the additional words.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="iPage"></param>
        /// <param name="jMax"></param>
        /// <param name="jWord"></param>
        /// <param name="bError"></param>
        /// <returns></returns>
        private bool bMatchWord(string word, int iPage, int jMax, ref int jWord, ref bool bError)
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

        /// <summary>
        /// count number of matches for that were found for each phrase 
        /// and return the total number of matches
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// progress bar
        /// note that DoEvents had the side effect of allowing the use to click on widgets
        /// while the program is running.  One must disable some feature to prevent, for example,
        /// starting a new search before the old search has finished.
        /// </summary>
        /// <param name="p"></param>
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
                tbPdfName.Text = "missing Adobe DLL or bad file:" + tbPdfName.Text;
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

        /// <summary>
        /// Open the PDF using Acrobat (I assume) and position to the current page selected
        /// </summary>
        /// <param name="fileName"></param>
        private void ViewDoc(string fileName)
        {
            ThisDoc = new AcroAVDoc();
            ThisDoc.Open(fileName, "");
            ThisDoc.BringToFront();
            ThisDoc.SetViewMode(2); // PDUseThumbs
            ThisDocView = ThisDoc.GetAVPageView() as CAcroAVPageView;
            //ThisDocView.ZoomTo(1 /*AVZoomFitPage*/, 100); // was in an example app, not sure how useful
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
                    if (bError) return false;
                    if(bMatchWord(chkWord,p,numWords, ref jWord, ref bError))
                    {
                        //found a match and have counted it
                        if(bError) return false;
                    }
                }
            }
            SetPBAR(0);   // clear the progress bar and show results of the pattern search
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
            dgv_phrases.DataSource = phlist.ToArray(); // connect results to the data grid view widget
            bFormDirty = true;
            gbPageCtrl.Visible = TotalMatches > 0;  // show page control group only if matches found
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

        //the phrase list is saved into windows AppData or where windows hides setup stuff
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
            tbPdfName.Text = "Build date: " + GetSimpleDate(Properties.Resources.BuildDate) +
                " (v) 1.0 (c)Stateson";
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
            this.Close(); // may want to prompt user to save changes ???
        }

        /// <summary>
        /// starts the search. The workingphrase (actually just word) has its case pre-set to optimize code
        /// unfortunately that only works good if there are only single words to maltch against and not phrases
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        // the searching is done starting with the InitialPhrase list
        // and then setting up workingphrase and phlist 
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



        /// <summary>
        /// user changed the status of the stop early switch
        /// this can be used to stop reading more pages of a PDF
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbStopEarly_CheckedChanged(object sender, EventArgs e)
        {
            LocalSettings.bExitEarly = cbStopEarly.Checked;
        }

        /// <summary>
        /// Get the list of pages that contains the wanted phrase and update the
        /// numeric up/down widget so the pages can be scrolled
        /// </summary>
        private void GetSelection()
        {
            //DataGridViewRow ThisRow = dgv_phrases.CurrentRow;
            Point ThisRC = dgv_phrases.CurrentCellAddress;
            int iRow = ThisRC.Y;
            int iCol = ThisRC.X;
            ThisPageList = phlist[iRow].strPages.Split(',').Select(int.Parse).ToArray();
            nudPage.Maximum = ThisPageList.Length - 1;
            iCurrentPage = -1;
            tbViewPage.Text = ThisPageList[0].ToString();
            tbViewPage.Visible = ThisPageList.Length > 0;   // only show page number of there are pages
            if (tbViewPage.Visible)
                iCurrentPage = ThisPageList[0];
            nudPage.Visible = ThisPageList.Length > 1;      // only show numeric up/down if more than 1 page
        }



        private void dgv_phrases_Click(object sender, EventArgs e)
        {
            GetSelection();
        }


        private void ViewSelectedPage()
        {
            if (iCurrentPage < 0) return;
            try
            {
                ThisDocView.GoTo(iCurrentPage - 1);
            }
            catch
            {
                // page probably closed by user: they can re-open it
            }

        }


        private void nudPage_ValueChanged(object sender, EventArgs e)
        {
            if (ThisPageList == null) return;
            int iVal = Convert.ToInt32(nudPage.Value);
            iCurrentPage = ThisPageList[iVal];
            tbViewPage.Text = iCurrentPage.ToString();
            if (ThisDocView != null)
            {
                ViewSelectedPage();
            }
        }

        private void btnViewDoc_Click(object sender, EventArgs e)
        {
            ViewDoc(tbPdfName.Text);
        }

    }
}
