/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

namespace AudioPlayer
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
            this.label1 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.bnOpen = new System.Windows.Forms.Button();
            this.bnPlay = new System.Windows.Forms.Button();
            this.bnPause = new System.Windows.Forms.Button();
            this.bnStop = new System.Windows.Forms.Button();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.tkFilePos = new System.Windows.Forms.TrackBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbCopyright = new System.Windows.Forms.TextBox();
            this.tbAuthor = new System.Windows.Forms.TextBox();
            this.tbClip = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDuration = new System.Windows.Forms.TextBox();
            this.tbDevice = new System.Windows.Forms.TextBox();
            this.tkVolume = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tkFilePos)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tkVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Audio File:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Media Files (*.asf, *.wma, *.wmv)|*.asf;*.wma;*.wmv|All files(*.*)|*.*";
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(71, 36);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(146, 20);
            this.tbFileName.TabIndex = 1;
            this.tbFileName.Text = "C:\\so_lesson3c.wmv";
            this.tbFileName.TextChanged += new System.EventHandler(this.tbFileName_TextChanged);
            // 
            // bnOpen
            // 
            this.bnOpen.Location = new System.Drawing.Point(223, 33);
            this.bnOpen.Name = "bnOpen";
            this.bnOpen.Size = new System.Drawing.Size(25, 23);
            this.bnOpen.TabIndex = 2;
            this.bnOpen.Text = "...";
            this.bnOpen.UseVisualStyleBackColor = true;
            this.bnOpen.Click += new System.EventHandler(this.bnOpen_Click);
            // 
            // bnPlay
            // 
            this.bnPlay.Location = new System.Drawing.Point(16, 67);
            this.bnPlay.Name = "bnPlay";
            this.bnPlay.Size = new System.Drawing.Size(53, 23);
            this.bnPlay.TabIndex = 3;
            this.bnPlay.Text = "Play";
            this.bnPlay.UseVisualStyleBackColor = true;
            this.bnPlay.Click += new System.EventHandler(this.bnPlay_Click);
            // 
            // bnPause
            // 
            this.bnPause.Enabled = false;
            this.bnPause.Location = new System.Drawing.Point(104, 67);
            this.bnPause.Name = "bnPause";
            this.bnPause.Size = new System.Drawing.Size(56, 23);
            this.bnPause.TabIndex = 4;
            this.bnPause.Text = "Pause";
            this.bnPause.UseVisualStyleBackColor = true;
            this.bnPause.Click += new System.EventHandler(this.bnPause_Click);
            // 
            // bnStop
            // 
            this.bnStop.Enabled = false;
            this.bnStop.Location = new System.Drawing.Point(197, 67);
            this.bnStop.Name = "bnStop";
            this.bnStop.Size = new System.Drawing.Size(52, 23);
            this.bnStop.TabIndex = 5;
            this.bnStop.Text = "Stop";
            this.bnStop.UseVisualStyleBackColor = true;
            this.bnStop.Click += new System.EventHandler(this.bnStop_Click);
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(6, 94);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(154, 20);
            this.tbStatus.TabIndex = 6;
            // 
            // tkFilePos
            // 
            this.tkFilePos.Enabled = false;
            this.tkFilePos.Location = new System.Drawing.Point(3, 6);
            this.tkFilePos.Maximum = 100;
            this.tkFilePos.Name = "tkFilePos";
            this.tkFilePos.Size = new System.Drawing.Size(230, 50);
            this.tkFilePos.TabIndex = 7;
            this.tkFilePos.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tkFilePos.Scroll += new System.EventHandler(this.tkFilePos_Scroll);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbCopyright);
            this.panel1.Controls.Add(this.tbAuthor);
            this.panel1.Controls.Add(this.tbClip);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tkFilePos);
            this.panel1.Location = new System.Drawing.Point(11, 120);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(236, 119);
            this.panel1.TabIndex = 8;
            this.panel1.Visible = false;
            // 
            // tbCopyright
            // 
            this.tbCopyright.Location = new System.Drawing.Point(76, 91);
            this.tbCopyright.Name = "tbCopyright";
            this.tbCopyright.ReadOnly = true;
            this.tbCopyright.Size = new System.Drawing.Size(157, 20);
            this.tbCopyright.TabIndex = 5;
            // 
            // tbAuthor
            // 
            this.tbAuthor.Location = new System.Drawing.Point(76, 65);
            this.tbAuthor.Name = "tbAuthor";
            this.tbAuthor.ReadOnly = true;
            this.tbAuthor.Size = new System.Drawing.Size(157, 20);
            this.tbAuthor.TabIndex = 4;
            // 
            // tbClip
            // 
            this.tbClip.Location = new System.Drawing.Point(76, 39);
            this.tbClip.Name = "tbClip";
            this.tbClip.ReadOnly = true;
            this.tbClip.Size = new System.Drawing.Size(157, 20);
            this.tbClip.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Copyright";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Author";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Clip";
            // 
            // tbDuration
            // 
            this.tbDuration.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbDuration.Location = new System.Drawing.Point(166, 98);
            this.tbDuration.Name = "tbDuration";
            this.tbDuration.ReadOnly = true;
            this.tbDuration.Size = new System.Drawing.Size(83, 13);
            this.tbDuration.TabIndex = 9;
            this.tbDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbDevice
            // 
            this.tbDevice.Location = new System.Drawing.Point(6, 4);
            this.tbDevice.Name = "tbDevice";
            this.tbDevice.ReadOnly = true;
            this.tbDevice.Size = new System.Drawing.Size(241, 20);
            this.tbDevice.TabIndex = 10;
            // 
            // tkVolume
            // 
            this.tkVolume.Enabled = false;
            this.tkVolume.Location = new System.Drawing.Point(265, 27);
            this.tkVolume.Maximum = 65535;
            this.tkVolume.Name = "tkVolume";
            this.tkVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tkVolume.Size = new System.Drawing.Size(50, 216);
            this.tkVolume.SmallChange = 1024;
            this.tkVolume.TabIndex = 11;
            this.tkVolume.TickFrequency = 4096;
            this.tkVolume.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tkVolume.Scroll += new System.EventHandler(this.tkVolume_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(262, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Volume";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 250);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tkVolume);
            this.Controls.Add(this.tbDevice);
            this.Controls.Add(this.tbDuration);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.bnStop);
            this.Controls.Add(this.bnPause);
            this.Controls.Add(this.bnPlay);
            this.Controls.Add(this.bnOpen);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Audio Player";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.tkFilePos)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tkVolume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Button bnOpen;
        private System.Windows.Forms.Button bnPlay;
        private System.Windows.Forms.Button bnPause;
        private System.Windows.Forms.Button bnStop;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.TrackBar tkFilePos;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbCopyright;
        private System.Windows.Forms.TextBox tbAuthor;
        private System.Windows.Forms.TextBox tbClip;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDuration;
        private System.Windows.Forms.TextBox tbDevice;
        private System.Windows.Forms.TrackBar tkVolume;
        private System.Windows.Forms.Label label5;
    }
}

