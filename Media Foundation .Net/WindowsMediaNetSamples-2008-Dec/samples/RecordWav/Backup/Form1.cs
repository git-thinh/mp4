/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using WindowsMediaLib;
using MultiMedia;

namespace RecordWav
{
    public partial class Form1 : Form
    {
        private CWaveRecord m_Wav;
        public Form1()
        {
            InitializeComponent();

            // In theory, you could build a pulldown of recording devices.  My 
            // machine only has 1 device, so I can't try it.
            WaveInCaps[] devs = CWaveRecord.GetDevices();

            // Make an instance of the recording device
            m_Wav = new CWaveRecord(0);
        }

        private void bnRecord_Click(object sender, EventArgs e)
        {

            // If the file isn't open yet
            if (bnRecord.Text == "Record")
            {
                if (!File.Exists(tbFilename.Text))
                {
                    m_Wav.CreateNew(tbFilename.Text, 1, 16, 44100);
                }
                else
                {
                    m_Wav.AppendExisting(tbFilename.Text);
                }
                m_Wav.Record();
                bnRecord.Text = "Pause";
                bnClose.Enabled = true;
                tbFilename.Enabled = false;
            }
            // Pause the recording
            else if (bnRecord.Text == "Pause")
            {
                m_Wav.Pause();
                bnRecord.Text = "Resume";
            }
            // Continue recording to the same file
            else
            {
                m_Wav.Record();
                bnRecord.Text = "Pause";
            }
        }

        // Close the current file
        private void bnClose_Click(object sender, EventArgs e)
        {
            bnRecord.Text = "Record";
            bnClose.Enabled = false;
            tbFilename.Enabled = true;
            m_Wav.Close();
        }
    }
}