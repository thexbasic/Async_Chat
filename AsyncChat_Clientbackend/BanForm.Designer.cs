namespace AsyncChat_Clientbackend
{
    partial class BanForm
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
            this.listBox_bans = new System.Windows.Forms.ListBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.entbannenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox_bans
            // 
            this.listBox_bans.ContextMenuStrip = this.contextMenuStrip;
            this.listBox_bans.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_bans.FormattingEnabled = true;
            this.listBox_bans.Location = new System.Drawing.Point(0, 0);
            this.listBox_bans.Name = "listBox_bans";
            this.listBox_bans.Size = new System.Drawing.Size(284, 369);
            this.listBox_bans.TabIndex = 0;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.entbannenToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 48);
            // 
            // entbannenToolStripMenuItem
            // 
            this.entbannenToolStripMenuItem.Name = "entbannenToolStripMenuItem";
            this.entbannenToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.entbannenToolStripMenuItem.Text = "Entbannen";
            this.entbannenToolStripMenuItem.Click += new System.EventHandler(this.entbannenToolStripMenuItem_Click);
            // 
            // BanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 369);
            this.Controls.Add(this.listBox_bans);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "BanForm";
            this.Text = "Bans";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox_bans;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem entbannenToolStripMenuItem;
    }
}