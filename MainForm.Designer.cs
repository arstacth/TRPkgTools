namespace TRpkgTools
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.pathCaptionLabel = new System.Windows.Forms.Label();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.unpackButton = new System.Windows.Forms.Button();
            this.repackButton = new System.Windows.Forms.Button();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.fileLogBox = new System.Windows.Forms.TextBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.besidePkgCheck = new System.Windows.Forms.CheckBox();
            this.debugCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.titleLabel.Location = new System.Drawing.Point(16, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(360, 22);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "TRPkgTools";
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.subtitleLabel.Location = new System.Drawing.Point(16, 34);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(400, 18);
            this.subtitleLabel.TabIndex = 1;
            this.subtitleLabel.Text = "TalesRunner PKG Tools";
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.closeButton.Location = new System.Drawing.Point(480, 8);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(28, 28);
            this.closeButton.TabIndex = 2;
            this.closeButton.TabStop = false;
            this.closeButton.Text = "×";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // pathCaptionLabel
            // 
            this.pathCaptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.pathCaptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.pathCaptionLabel.Location = new System.Drawing.Point(16, 64);
            this.pathCaptionLabel.Name = "pathCaptionLabel";
            this.pathCaptionLabel.Size = new System.Drawing.Size(200, 16);
            this.pathCaptionLabel.TabIndex = 3;
            this.pathCaptionLabel.Text = "File";
            // 
            // besidePkgCheck
            // 
            this.besidePkgCheck.AutoSize = false;
            this.besidePkgCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.besidePkgCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.besidePkgCheck.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.besidePkgCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this.besidePkgCheck.Location = new System.Drawing.Point(16, 156);
            this.besidePkgCheck.Name = "besidePkgCheck";
            this.besidePkgCheck.Size = new System.Drawing.Size(280, 20);
            this.besidePkgCheck.TabIndex = 11;
            this.besidePkgCheck.Text = "Unpack at PKG Location";
            this.besidePkgCheck.UseVisualStyleBackColor = false;
            // 
            // debugCheck
            // 
            this.debugCheck.AutoSize = false;
            this.debugCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.debugCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.debugCheck.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.debugCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this.debugCheck.Location = new System.Drawing.Point(300, 156);
            this.debugCheck.Name = "debugCheck";
            this.debugCheck.Size = new System.Drawing.Size(204, 20);
            this.debugCheck.TabIndex = 12;
            this.debugCheck.Text = "DEBUG";
            this.debugCheck.UseVisualStyleBackColor = false;
            // 
            // pathBox
            // 
            this.pathBox.AllowDrop = true;
            this.pathBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pathBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.pathBox.Location = new System.Drawing.Point(16, 84);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(390, 26);
            this.pathBox.TabIndex = 4;
            this.pathBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.pathBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            // 
            // browseButton
            // 
            this.browseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.browseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.browseButton.Location = new System.Drawing.Point(414, 83);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(90, 28);
            this.browseButton.TabIndex = 5;
            this.browseButton.TabStop = false;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = false;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // unpackButton
            // 
            this.unpackButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(166)))), ((int)(((byte)(255)))));
            this.unpackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.unpackButton.FlatAppearance.BorderSize = 0;
            this.unpackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.unpackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.unpackButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.unpackButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.unpackButton.Location = new System.Drawing.Point(16, 122);
            this.unpackButton.Name = "unpackButton";
            this.unpackButton.Size = new System.Drawing.Size(240, 28);
            this.unpackButton.TabIndex = 6;
            this.unpackButton.Text = "Unpack";
            this.unpackButton.UseVisualStyleBackColor = false;
            this.unpackButton.Click += new System.EventHandler(this.unpackButton_Click);
            // 
            // repackButton
            // 
            this.repackButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(166)))), ((int)(((byte)(255)))));
            this.repackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.repackButton.FlatAppearance.BorderSize = 0;
            this.repackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.repackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.repackButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.repackButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.repackButton.Location = new System.Drawing.Point(264, 122);
            this.repackButton.Name = "repackButton";
            this.repackButton.Size = new System.Drawing.Size(240, 28);
            this.repackButton.TabIndex = 7;
            this.repackButton.Text = "Repack";
            this.repackButton.UseVisualStyleBackColor = false;
            this.repackButton.Click += new System.EventHandler(this.repackButton_Click);
            // 
            // progressPanel
            // 
            this.progressPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.progressPanel.Location = new System.Drawing.Point(16, 182);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(488, 6);
            this.progressPanel.TabIndex = 8;
            this.progressPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.progressPanel_Paint);
            // 
            // fileLogBox
            // 
            this.fileLogBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.fileLogBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileLogBox.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.fileLogBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.fileLogBox.Location = new System.Drawing.Point(16, 196);
            this.fileLogBox.Multiline = true;
            this.fileLogBox.Name = "fileLogBox";
            this.fileLogBox.ReadOnly = true;
            this.fileLogBox.Size = new System.Drawing.Size(488, 132);
            this.fileLogBox.TabIndex = 9;
            this.fileLogBox.TabStop = false;
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.Transparent;
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.statusLabel.Location = new System.Drawing.Point(16, 332);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(488, 14);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(520, 352);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.fileLogBox);
            this.Controls.Add(this.progressPanel);
            this.Controls.Add(this.repackButton);
            this.Controls.Add(this.unpackButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.pathBox);
            this.Controls.Add(this.debugCheck);
            this.Controls.Add(this.besidePkgCheck);
            this.Controls.Add(this.pathCaptionLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.subtitleLabel);
            this.Controls.Add(this.titleLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TRPkgTools";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label subtitleLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label pathCaptionLabel;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button unpackButton;
        private System.Windows.Forms.Button repackButton;
        private System.Windows.Forms.Panel progressPanel;
        private System.Windows.Forms.TextBox fileLogBox;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.CheckBox besidePkgCheck;
        private System.Windows.Forms.CheckBox debugCheck;
    }
}
