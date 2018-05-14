/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

namespace RecordWav
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
            this.tbFilename = new System.Windows.Forms.TextBox();
            this.bnRecord = new System.Windows.Forms.Button();
            this.bnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbFilename
            // 
            this.tbFilename.Location = new System.Drawing.Point(12, 22);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.Size = new System.Drawing.Size(268, 20);
            this.tbFilename.TabIndex = 0;
            this.tbFilename.Text = "c:\\testout.wav";
            // 
            // bnRecord
            // 
            this.bnRecord.Location = new System.Drawing.Point(44, 79);
            this.bnRecord.Name = "bnRecord";
            this.bnRecord.Size = new System.Drawing.Size(75, 23);
            this.bnRecord.TabIndex = 2;
            this.bnRecord.Text = "Record";
            this.bnRecord.UseVisualStyleBackColor = true;
            this.bnRecord.Click += new System.EventHandler(this.bnRecord_Click);
            // 
            // bnClose
            // 
            this.bnClose.Enabled = false;
            this.bnClose.Location = new System.Drawing.Point(174, 79);
            this.bnClose.Name = "bnClose";
            this.bnClose.Size = new System.Drawing.Size(75, 23);
            this.bnClose.TabIndex = 4;
            this.bnClose.Text = "Close";
            this.bnClose.UseVisualStyleBackColor = true;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 134);
            this.Controls.Add(this.bnClose);
            this.Controls.Add(this.bnRecord);
            this.Controls.Add(this.tbFilename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "RecordWav";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbFilename;
        private System.Windows.Forms.Button bnRecord;
        private System.Windows.Forms.Button bnClose;
    }
}

