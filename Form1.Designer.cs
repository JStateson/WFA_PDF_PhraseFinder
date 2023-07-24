namespace WFA_PDF_PhraseFinder
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.gbPDFname = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbStopEarly = new System.Windows.Forms.CheckBox();
            this.tbPdfName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbTotalMatch = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbNumPages = new System.Windows.Forms.TextBox();
            this.gbSearch = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.dgv_phrases = new System.Windows.Forms.DataGridView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnInvert = new System.Windows.Forms.Button();
            this.btnUncheckall = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.cbIgnoreCase = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.tbMatches = new System.Windows.Forms.TextBox();
            this.pbarLoading = new System.Windows.Forms.ProgressBar();
            this.btnRunSearch = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gbPDFname.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gbSearch.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_phrases)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.Location = new System.Drawing.Point(767, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(279, 129);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // gbPDFname
            // 
            this.gbPDFname.Controls.Add(this.groupBox3);
            this.gbPDFname.Controls.Add(this.tbPdfName);
            this.gbPDFname.Controls.Add(this.groupBox2);
            this.gbPDFname.Controls.Add(this.groupBox1);
            this.gbPDFname.Location = new System.Drawing.Point(135, 12);
            this.gbPDFname.Name = "gbPDFname";
            this.gbPDFname.Size = new System.Drawing.Size(509, 167);
            this.gbPDFname.TabIndex = 4;
            this.gbPDFname.TabStop = false;
            this.gbPDFname.Text = " Current PDF";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cbStopEarly);
            this.groupBox3.Location = new System.Drawing.Point(357, 93);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(130, 49);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Stop after 40 pages";
            // 
            // cbStopEarly
            // 
            this.cbStopEarly.AutoSize = true;
            this.cbStopEarly.Checked = true;
            this.cbStopEarly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStopEarly.Location = new System.Drawing.Point(45, 22);
            this.cbStopEarly.Name = "cbStopEarly";
            this.cbStopEarly.Size = new System.Drawing.Size(15, 14);
            this.cbStopEarly.TabIndex = 0;
            this.toolTip1.SetToolTip(this.cbStopEarly, "Used for testing ");
            this.cbStopEarly.UseVisualStyleBackColor = true;
            this.cbStopEarly.CheckedChanged += new System.EventHandler(this.cbStopEarly_CheckedChanged);
            // 
            // tbPdfName
            // 
            this.tbPdfName.Location = new System.Drawing.Point(65, 41);
            this.tbPdfName.Name = "tbPdfName";
            this.tbPdfName.Size = new System.Drawing.Size(406, 20);
            this.tbPdfName.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbTotalMatch);
            this.groupBox2.Location = new System.Drawing.Point(203, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(122, 49);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Total Matches";
            // 
            // tbTotalMatch
            // 
            this.tbTotalMatch.Location = new System.Drawing.Point(31, 19);
            this.tbTotalMatch.Name = "tbTotalMatch";
            this.tbTotalMatch.Size = new System.Drawing.Size(54, 20);
            this.tbTotalMatch.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbNumPages);
            this.groupBox1.Location = new System.Drawing.Point(53, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(122, 49);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Number Pages";
            // 
            // tbNumPages
            // 
            this.tbNumPages.Location = new System.Drawing.Point(31, 19);
            this.tbNumPages.Name = "tbNumPages";
            this.tbNumPages.Size = new System.Drawing.Size(54, 20);
            this.tbNumPages.TabIndex = 2;
            // 
            // gbSearch
            // 
            this.gbSearch.Controls.Add(this.groupBox4);
            this.gbSearch.Controls.Add(this.dgv_phrases);
            this.gbSearch.Controls.Add(this.btnSave);
            this.gbSearch.Controls.Add(this.btnInvert);
            this.gbSearch.Controls.Add(this.btnUncheckall);
            this.gbSearch.Controls.Add(this.btnSelectAll);
            this.gbSearch.Controls.Add(this.btnRemove);
            this.gbSearch.Controls.Add(this.btnAdd);
            this.gbSearch.Controls.Add(this.cbIgnoreCase);
            this.gbSearch.Location = new System.Drawing.Point(48, 198);
            this.gbSearch.Name = "gbSearch";
            this.gbSearch.Size = new System.Drawing.Size(547, 394);
            this.gbSearch.TabIndex = 5;
            this.gbSearch.TabStop = false;
            this.gbSearch.Text = "Phrase Searching";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnImport);
            this.groupBox4.Controls.Add(this.btnExport);
            this.groupBox4.Location = new System.Drawing.Point(17, 290);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(126, 85);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Clipboard / Notepad";
            this.toolTip1.SetToolTip(this.groupBox4, "Copy to;.from cliipboard");
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(8, 56);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 1;
            this.btnImport.Text = "Import";
            this.toolTip1.SetToolTip(this.btnImport, "Open your phrase liost using notepad and then selecte and copy");
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(8, 19);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export";
            this.toolTip1.SetToolTip(this.btnExport, "Brint up notepad and seledct paste");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // dgv_phrases
            // 
            this.dgv_phrases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_phrases.Location = new System.Drawing.Point(171, 19);
            this.dgv_phrases.Name = "dgv_phrases";
            this.dgv_phrases.ShowEditingIcon = false;
            this.dgv_phrases.Size = new System.Drawing.Size(333, 356);
            this.dgv_phrases.TabIndex = 10;
            this.toolTip1.SetToolTip(this.dgv_phrases, " You may edit the text field but be sure tos save your edits");
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(17, 245);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save Phrases";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnInvert
            // 
            this.btnInvert.Location = new System.Drawing.Point(17, 216);
            this.btnInvert.Name = "btnInvert";
            this.btnInvert.Size = new System.Drawing.Size(96, 24);
            this.btnInvert.TabIndex = 8;
            this.btnInvert.Text = "Invert Selection";
            this.btnInvert.UseVisualStyleBackColor = true;
            this.btnInvert.Click += new System.EventHandler(this.btnInvert_Click);
            // 
            // btnUncheckall
            // 
            this.btnUncheckall.Location = new System.Drawing.Point(17, 187);
            this.btnUncheckall.Name = "btnUncheckall";
            this.btnUncheckall.Size = new System.Drawing.Size(96, 23);
            this.btnUncheckall.TabIndex = 7;
            this.btnUncheckall.Text = "Un-check all";
            this.btnUncheckall.UseVisualStyleBackColor = true;
            this.btnUncheckall.Click += new System.EventHandler(this.btnUncheckall_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(17, 158);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(96, 23);
            this.btnSelectAll.TabIndex = 6;
            this.btnSelectAll.Text = "Check All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.BackColor = System.Drawing.Color.LightCoral;
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.Location = new System.Drawing.Point(17, 107);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(112, 23);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "Delete phrase";
            this.toolTip1.SetToolTip(this.btnRemove, "Be sure to click SAVE ");
            this.btnRemove.UseVisualStyleBackColor = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(17, 61);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(88, 23);
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = "Add phrase";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // cbIgnoreCase
            // 
            this.cbIgnoreCase.AutoSize = true;
            this.cbIgnoreCase.Checked = true;
            this.cbIgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIgnoreCase.Location = new System.Drawing.Point(17, 38);
            this.cbIgnoreCase.Name = "cbIgnoreCase";
            this.cbIgnoreCase.Size = new System.Drawing.Size(83, 17);
            this.cbIgnoreCase.TabIndex = 1;
            this.cbIgnoreCase.Text = "Ignore Case";
            this.cbIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1072, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click_1);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // ofd
            // 
            this.ofd.FileName = "ofd";
            // 
            // tbMatches
            // 
            this.tbMatches.Location = new System.Drawing.Point(650, 217);
            this.tbMatches.Multiline = true;
            this.tbMatches.Name = "tbMatches";
            this.tbMatches.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMatches.Size = new System.Drawing.Size(396, 311);
            this.tbMatches.TabIndex = 11;
            // 
            // pbarLoading
            // 
            this.pbarLoading.Location = new System.Drawing.Point(767, 156);
            this.pbarLoading.Name = "pbarLoading";
            this.pbarLoading.Size = new System.Drawing.Size(279, 23);
            this.pbarLoading.TabIndex = 12;
            // 
            // btnRunSearch
            // 
            this.btnRunSearch.Enabled = false;
            this.btnRunSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRunSearch.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.btnRunSearch.Location = new System.Drawing.Point(12, 65);
            this.btnRunSearch.Name = "btnRunSearch";
            this.btnRunSearch.Size = new System.Drawing.Size(96, 23);
            this.btnRunSearch.TabIndex = 13;
            this.btnRunSearch.Text = "Run Search";
            this.toolTip1.SetToolTip(this.btnRunSearch, "Click to start searching");
            this.btnRunSearch.UseVisualStyleBackColor = true;
            this.btnRunSearch.Click += new System.EventHandler(this.btnRunSearch_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1072, 624);
            this.Controls.Add(this.btnRunSearch);
            this.Controls.Add(this.pbarLoading);
            this.Controls.Add(this.tbMatches);
            this.Controls.Add(this.gbSearch);
            this.Controls.Add(this.gbPDFname);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "USDA / FNS";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.gbPDFname.ResumeLayout(false);
            this.gbPDFname.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbSearch.ResumeLayout(false);
            this.gbSearch.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_phrases)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox gbPDFname;
        private System.Windows.Forms.GroupBox gbSearch;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnInvert;
        private System.Windows.Forms.Button btnUncheckall;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.CheckBox cbIgnoreCase;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.DataGridView dgv_phrases;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbTotalMatch;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbNumPages;
        private System.Windows.Forms.TextBox tbPdfName;
        private System.Windows.Forms.TextBox tbMatches;
        private System.Windows.Forms.ProgressBar pbarLoading;
        private System.Windows.Forms.Button btnRunSearch;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox cbStopEarly;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
    }
}

