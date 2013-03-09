namespace web_crawler
{
    partial class WebCrawler
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
            this.htmlBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // htmlBox
            // 
            this.htmlBox.Location = new System.Drawing.Point(21, 12);
            this.htmlBox.Name = "htmlBox";
            this.htmlBox.Size = new System.Drawing.Size(384, 226);
            this.htmlBox.TabIndex = 0;
            this.htmlBox.Text = "";
            // 
            // WebCrawler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 262);
            this.Controls.Add(this.htmlBox);
            this.Name = "WebCrawler";
            this.Text = "Web Crawler";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox htmlBox;
    }
}

