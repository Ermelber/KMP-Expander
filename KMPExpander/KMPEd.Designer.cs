namespace KMPExpander
{
    partial class KMPEd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KMPEd));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKMPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKMP = new System.Windows.Forms.OpenFileDialog();
            this.saveKMP = new System.Windows.Forms.SaveFileDialog();
            this.extractbin = new System.Windows.Forms.Button();
            this.injectbin = new System.Windows.Forms.Button();
            this.injectcsv = new System.Windows.Forms.Button();
            this.extractcsv = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.saveBinary = new System.Windows.Forms.SaveFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.filesize_box = new System.Windows.Forms.TextBox();
            this.openBinary = new System.Windows.Forms.OpenFileDialog();
            this.saveCSV = new System.Windows.Forms.SaveFileDialog();
            this.openCSV = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(258, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openKMPToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openKMPToolStripMenuItem
            // 
            this.openKMPToolStripMenuItem.Name = "openKMPToolStripMenuItem";
            this.openKMPToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.openKMPToolStripMenuItem.Text = "Open KMP";
            this.openKMPToolStripMenuItem.Click += new System.EventHandler(this.OpenKMPToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // openKMP
            // 
            this.openKMP.FileName = "course.kmp";
            this.openKMP.Filter = "Mario Kart 7 KMP Files|*.kmp|All files|*.*";
            this.openKMP.Title = "Open a Mario Kart 7 KMP File";
            this.openKMP.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenKMP_FileOk);
            // 
            // saveKMP
            // 
            this.saveKMP.FileName = "course.kmp";
            this.saveKMP.Filter = "Mario Kart 7 KMP Files|*.kmp|All files|*.*";
            this.saveKMP.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog1_FileOk);
            // 
            // extractbin
            // 
            this.extractbin.Enabled = false;
            this.extractbin.Location = new System.Drawing.Point(15, 78);
            this.extractbin.Name = "extractbin";
            this.extractbin.Size = new System.Drawing.Size(111, 23);
            this.extractbin.TabIndex = 2;
            this.extractbin.Text = "Extract binary";
            this.extractbin.UseVisualStyleBackColor = true;
            this.extractbin.Click += new System.EventHandler(this.Extractbin_Click);
            // 
            // injectbin
            // 
            this.injectbin.Enabled = false;
            this.injectbin.Location = new System.Drawing.Point(132, 78);
            this.injectbin.Name = "injectbin";
            this.injectbin.Size = new System.Drawing.Size(111, 23);
            this.injectbin.TabIndex = 3;
            this.injectbin.Text = "Inject binary";
            this.injectbin.UseVisualStyleBackColor = true;
            this.injectbin.Click += new System.EventHandler(this.Injectbin_Click);
            // 
            // injectcsv
            // 
            this.injectcsv.Enabled = false;
            this.injectcsv.Location = new System.Drawing.Point(132, 107);
            this.injectcsv.Name = "injectcsv";
            this.injectcsv.Size = new System.Drawing.Size(111, 23);
            this.injectcsv.TabIndex = 5;
            this.injectcsv.Text = "Inject CSV";
            this.injectcsv.UseVisualStyleBackColor = true;
            this.injectcsv.Click += new System.EventHandler(this.Injectcsv_Click);
            // 
            // extractcsv
            // 
            this.extractcsv.Enabled = false;
            this.extractcsv.Location = new System.Drawing.Point(15, 107);
            this.extractcsv.Name = "extractcsv";
            this.extractcsv.Size = new System.Drawing.Size(111, 23);
            this.extractcsv.TabIndex = 4;
            this.extractcsv.Text = "Extract CSV";
            this.extractcsv.UseVisualStyleBackColor = true;
            this.extractcsv.Click += new System.EventHandler(this.Extractcsv_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Enabled = false;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "KTPT (Kart Point)",
            "ENPT (Enemy Routes)",
            "ENPH (Enemy Routes\' Sections)",
            "ITPT (Item Routes)",
            "ITPH (Item Routes\' Sections)",
            "CKPT (Checkpoints)",
            "CKPH (Checkpoints\' Sections)",
            "GOBJ (Global Objects)",
            "POTI (Routes)",
            "AREA",
            "CAME (Camera)",
            "JGPT(Respawn Points)",
            "CNPT (Cannon Points)",
            "MSPT (Mission Points)",
            "STGI",
            "CORS",
            "GLPT (Glider Points)",
            "GLPH (Glider Points\' Sections)"});
            this.comboBox1.Location = new System.Drawing.Point(15, 51);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(228, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // saveBinary
            // 
            this.saveBinary.Filter = "Binary File|*.bin|All Files|*.*";
            this.saveBinary.Title = "Extract Binary Section";
            this.saveBinary.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveBinary_FileOk);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Select a Section:";
            this.label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(129, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Filesize:";
            this.label2.Visible = false;
            // 
            // filesize_box
            // 
            this.filesize_box.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.filesize_box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.filesize_box.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.filesize_box.Location = new System.Drawing.Point(179, 136);
            this.filesize_box.Name = "filesize_box";
            this.filesize_box.ReadOnly = true;
            this.filesize_box.Size = new System.Drawing.Size(64, 13);
            this.filesize_box.TabIndex = 9;
            this.filesize_box.TextChanged += new System.EventHandler(this.Filesize_box_TextChanged);
            // 
            // openBinary
            // 
            this.openBinary.Filter = "Binary File|*.bin|All Files|*.*";
            this.openBinary.Title = "Open a Section to Inject";
            // 
            // saveCSV
            // 
            this.saveCSV.Filter = "Comma Separated Values|*.csv|Text file|*.txt|All files|*.*";
            this.saveCSV.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveCSV_FileOk);
            // 
            // openCSV
            // 
            this.openCSV.Filter = "Comma Separated Values|*.csv|Text File|*.txt|All files|*.*";
            this.openCSV.Title = "Import a CSV File";
            // 
            // KMPEd
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(258, 157);
            this.Controls.Add(this.filesize_box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.injectcsv);
            this.Controls.Add(this.extractcsv);
            this.Controls.Add(this.injectbin);
            this.Controls.Add(this.extractbin);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "KMPEd";
            this.Text = "KMP Expander " + Ver;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKMPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openKMP;
        private System.Windows.Forms.SaveFileDialog saveKMP;
        private System.Windows.Forms.Button extractbin;
        private System.Windows.Forms.Button injectbin;
        private System.Windows.Forms.Button injectcsv;
        private System.Windows.Forms.Button extractcsv;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.SaveFileDialog saveBinary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox filesize_box;
        private System.Windows.Forms.OpenFileDialog openBinary;
        private System.Windows.Forms.SaveFileDialog saveCSV;
        private System.Windows.Forms.OpenFileDialog openCSV;
    }
}

