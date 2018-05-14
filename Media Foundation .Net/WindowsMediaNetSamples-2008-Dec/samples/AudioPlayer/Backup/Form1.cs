/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using WindowsMediaLib;

using MultiMedia;

namespace AudioPlayer
{
    public partial class Form1 : Form
    {
        private const int TENTHSEC = 1000000;

        private enum AUDIOSTATUS
        {
            CLOSED = 0,
            STOP,
            PAUSE,
            PLAY,
            OPENING,
            ACQUIRINGLICENSE,
            INDIVIDUALIZING,
            STOPPING,
            READY,
            BUFFERING,
            LICENSEACQUIRED,
            INDIVIDUALIZED
        }

        private delegate void StatusDelegate(AUDIOSTATUS currentStatus);
        private delegate void TimeDelegate(long cnsTimeElapsed);

        private CAudioPlay m_pAudioplay;
        private AUDIOSTATUS m_Status;
        private string m_Duration;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Mixer.LINE_CHANGE:
                    //Debug.WriteLine(string.Format("Line {0} {1}", m.LParam, m.WParam));
                    ShowVolume();
                    break;
                //case Mixer.CONTROL_CHANGE:
                //    Debug.WriteLine(string.Format("Control {0} {1}", m.LParam, m.WParam));
                //    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        public Form1()
        {
            InitializeComponent();

            SetCurrentStatus(AUDIOSTATUS.READY);

            try
            {
                // While I could do a pulldown to let people select the
                // playback device, I (probably like most people) only 
                // have one sound device on my machine, so I can't test it.
                const int iUseDevice = 0;

                WaveOutCaps[] waves = CAudioPlay.GetDevs();
                tbDevice.Text = waves[iUseDevice].szPname;

                m_pAudioplay = new CAudioPlay(iUseDevice, this);
                m_pAudioplay.StatusChanged += new EventHandler(StatusChanged);
                m_pAudioplay.TimeChanged += new EventHandler(TimeChanged);
            }
            catch (Exception e)
            {
                ShowError(e);
            }
        }

        private void TimeChanged(object o, System.EventArgs e)
        {
            CAudioPlay.TimeChangeArgs ta = e as CAudioPlay.TimeChangeArgs;

            object[] obj = new object[1];
            obj[0] = ta.newTime;

            BeginInvoke(new TimeDelegate(SetTime), obj);
        }
        private void StatusChanged(object o, System.EventArgs e)
        {
            AUDIOSTATUS auds;

            CAudioPlay.StatusChangeArgs ca = e as CAudioPlay.StatusChangeArgs;

            switch (ca.newStatus)
            {
                case CAudioPlay.CurStatus.Buffering:
                    auds = AUDIOSTATUS.BUFFERING;
                    break;
                case CAudioPlay.CurStatus.Closed:
                    auds = AUDIOSTATUS.CLOSED;
                    break;
                case CAudioPlay.CurStatus.Play:
                    auds = AUDIOSTATUS.PLAY;
                    break;
                case CAudioPlay.CurStatus.Ready:
                    auds = AUDIOSTATUS.READY;
                    break;
                case CAudioPlay.CurStatus.Stop:
                    auds = AUDIOSTATUS.STOP;
                    break;
                default:
                    MessageBox.Show("Invalid status", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            if (InvokeRequired)
            {
                object[] obj = new object[1];
                obj[0] = auds;

                BeginInvoke(new StatusDelegate(SetCurrentStatus), obj);
            }
            else
            {
                SetCurrentStatus(auds);
            }

        }
        private void SetCurrentStatus(AUDIOSTATUS currentStatus)
        {
            switch (currentStatus)
            {
                case AUDIOSTATUS.READY:

                    tbStatus.Text = "Ready";
                    panel1.Visible = false;

                    bnStop.Enabled = false;
                    bnPause.Enabled = false;
                    bnOpen.Enabled = true;

                    tbFileName.ReadOnly = false;

                    return;

                case AUDIOSTATUS.OPENING:

                    tbStatus.Text = "Opening...";

                    bnStop.Enabled = true;
                    bnPlay.Enabled = false;
                    bnPause.Enabled = false;
                    bnOpen.Enabled = false;
                    tkFilePos.Enabled = false;

                    tbFileName.ReadOnly = true;
                    bnStop.Select();
                    break;

                case AUDIOSTATUS.PLAY:
                    //
                    // Reset the global variable which might have been set while seeking
                    // or stop operation
                    //
                    tbStatus.Text = "Playing...";

                    bnStop.Enabled = true;
                    bnPlay.Enabled = false;
                    bnOpen.Enabled = false;
                    tkFilePos.Enabled = m_pAudioplay.IsSeekable();

                    tbFileName.ReadOnly = true;
                    panel1.Visible = true;

                    if (!m_pAudioplay.IsBroadcast())
                    {
                        bnPause.Enabled = true;
                        bnPause.Select();
                    }
                    else
                    {
                        bnPause.Enabled = false;
                        bnStop.Select();
                    }

                    break;

                case AUDIOSTATUS.PAUSE:

                    tbStatus.Text = "Paused";

                    bnPause.Enabled = false;
                    bnPlay.Enabled = true;
                    tkFilePos.Enabled = false;
                    bnPlay.Select();

                    break;

                case AUDIOSTATUS.CLOSED:

                    tbStatus.Text = "";

                    bnOpen.Enabled = true;
                    tkFilePos.Enabled = false;
                    bnStop.Enabled = false;
                    bnPlay.Enabled = true;

                    tbFileName.ReadOnly = false;
                    tkFilePos.Value = 0;
                    panel1.Visible = false;

                    break;

                case AUDIOSTATUS.STOP:

                    tbStatus.Text = "Stopped";

                    tkFilePos.Value = 0;

                    bnPause.Enabled = false;
                    bnStop.Enabled = false;
                    bnPlay.Enabled = true;
                    bnOpen.Enabled = true;
                    tkFilePos.Enabled = false;

                    tbFileName.ReadOnly = false;
                    bnPlay.Select();

                    SetTime(0);

                    break;

                case AUDIOSTATUS.BUFFERING:

                    tbStatus.Text = "Buffering...";
                    return;

                case AUDIOSTATUS.STOPPING:
                    //
                    // Since we are going to position the trackbar at the beginning,
                    // set the global variable
                    //
                    tbStatus.Text = "Trying to Stop...Please Wait";

                    bnStop.Enabled = false;
                    bnPause.Enabled = false;
                    bnOpen.Enabled = false;
                    tkFilePos.Enabled = false;

                    tbFileName.ReadOnly = true;

                    return;

                case AUDIOSTATUS.ACQUIRINGLICENSE:

                    tbStatus.Text = "Acquiring License...";
                    bnStop.Enabled = true;

                    bnStop.Select();

                    break;

                case AUDIOSTATUS.INDIVIDUALIZING:

                    tbStatus.Text = "Individualizing...";
                    bnStop.Enabled = true;
                    bnStop.Select();

                    break;

                case AUDIOSTATUS.LICENSEACQUIRED:

                    tbStatus.Text = "License acquired";
                    break;

                case AUDIOSTATUS.INDIVIDUALIZED:
                    tbStatus.Text = "Individualization complete";
                    break;

                default:
                    return;
            }

            m_Status = currentStatus;
        }
        private void SetTime(long cnsTimeElapsed)
        {
            string tszTime = CalcTime(cnsTimeElapsed);

            tbDuration.Text = tszTime + m_Duration;

            if (tkFilePos.Enabled)
            {
                tkFilePos.Value = (int)(cnsTimeElapsed / TENTHSEC);
            }
        }
        private string CalcTime(long l)
        {
            string tszTime;

            int dwSeconds = (int)(l / (TENTHSEC * 10));
            int nHours = dwSeconds / 60 / 60;
            dwSeconds %= 3600;
            int nMins = dwSeconds / 60;
            dwSeconds %= 60;

            //
            // Format the string
            //
            if (0 != nHours)
            {
                tszTime = string.Format("{0}:{0:00}:{1:00}", nHours, nMins, dwSeconds);
            }
            else
            {
                tszTime = string.Format("{0:00}:{1:00}", nMins, dwSeconds);
            }

            return tszTime;
        }
        private void OnPlay()
        {
            if (null == m_pAudioplay)
            {
                return;
            }

            //
            // Check previous status of the player
            //
            switch (m_Status)
            {
                case AUDIOSTATUS.PAUSE:
                    //
                    // Player was PAUSEed, now resume it
                    //
                    try
                    {
                        m_pAudioplay.Resume();
                    }
                    catch (Exception e)
                    {
                        ShowError(e);
                    }
                    SetCurrentStatus(AUDIOSTATUS.PLAY);

                    break;

                case AUDIOSTATUS.STOP:
                    //
                    // Player was STOPped, now start it
                    //
                    try
                    {
                        m_pAudioplay.Start(0);
                    }
                    catch (Exception e)
                    {
                        ShowError(e);
                    }
                    SetCurrentStatus(AUDIOSTATUS.OPENING);

                    break;

                case AUDIOSTATUS.CLOSED:
                    //
                    // The play is being called for the current file for the first time.
                    // Start playing the file
                    //
                    SetCurrentStatus(AUDIOSTATUS.OPENING);

                    //
                    // Open the file. We may need to convert the filename string from multibytes to wide characters
                    //
                    try
                    {
                        m_pAudioplay.Open(tbFileName.Text.Trim());

                    }
                    catch (Exception e)
                    {
                        ShowError(e);
                        SetCurrentStatus(AUDIOSTATUS.CLOSED);
                        SetCurrentStatus(AUDIOSTATUS.READY);
                        return;
                    }

                    try
                    {
                        string sRet = null;

                        long lDuration = m_pAudioplay.GetFileDuration();

                        tkFilePos.Value = 0;
                        tkFilePos.Maximum = (int)(lDuration / TENTHSEC); ;

                        m_Duration = " / " + CalcTime(lDuration);
                        SetTime(0);

                        sRet = m_pAudioplay.GetAttributeString(Constants.g_wszWMTitle);
                        if (sRet == null)
                        {
                            sRet = "No Data";
                        }
                        tbClip.Text = sRet;

                        sRet = m_pAudioplay.GetAttributeString(Constants.g_wszWMAuthor);
                        if (sRet == null)
                        {
                            sRet = "No Data";
                        }
                        tbAuthor.Text = sRet;

                        sRet = m_pAudioplay.GetAttributeString(Constants.g_wszWMCopyright);
                        if (sRet == null)
                        {
                            sRet = "No Data";
                        }
                        tbCopyright.Text = sRet;

                        //
                        // Start to play from the beginning
                        //
                        m_pAudioplay.Start(0);
                    }
                    catch (Exception e)
                    {
                        ShowError(e);
                    }
                    break;
            }

            ShowVolume();
        }
        private void ShowVolume()
        {
            int iLeft, iRight;
            m_pAudioplay.GetVolume(out iLeft, out iRight);
            tkVolume.Value = iLeft;
        }
        private void SetVolume(int i)
        {
            m_pAudioplay.SetVolume(i, i);
        }

        private void bnPause_Click(object sender, EventArgs e)
        {
            try
            {
                m_pAudioplay.Pause();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            SetCurrentStatus(AUDIOSTATUS.PAUSE);
        }
        private void bnStop_Click(object sender, EventArgs e)
        {
            SetCurrentStatus(AUDIOSTATUS.STOPPING);

            try
            {
                m_pAudioplay.Stop();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        private void bnPlay_Click(object sender, EventArgs e)
        {
            OnPlay();
            ShowVolume();
            tkVolume.Enabled = true;
        }
        private void bnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = tbFileName.Text;

            DialogResult dr = openFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                tbFileName.Text = openFileDialog1.FileName;
                bnPlay.Select();
            }
        }
        private void tbFileName_TextChanged(object sender, EventArgs e)
        {
            if (tbFileName.Text.Length > 0)
            {
                bnPlay.Enabled = true;
                SetCurrentStatus(AUDIOSTATUS.CLOSED);
            }
            else
            {
                bnPlay.Enabled = false;
            }
            SetCurrentStatus(AUDIOSTATUS.READY);
        }
        private void tkFilePos_Scroll(object sender, EventArgs e)
        {
            if (m_pAudioplay.IsSeekable())
            {
                // 
                // Start the file from the new position
                //
                try
                {
                    m_pAudioplay.Start(tkFilePos.Value * TENTHSEC);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }
        private void tkVolume_Scroll(object sender, EventArgs e)
        {
            SetVolume(tkVolume.Value);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != m_pAudioplay)
            {
                try
                {
                    m_pAudioplay.Dispose();
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
                m_pAudioplay = null;
            }
        }

        private void ShowError(Exception e)
        {
            int hr = Marshal.GetHRForException(e);
            string s = WMError.GetErrorText(hr);

            if (s == null)
            {
                s = e.Message;
            }
            else
            {
                s = string.Format("{0} ({1})", s, e.Message);
            }

            System.Windows.Forms.MessageBox.Show(string.Format("0x{0:x}: {1}", hr, s), "Exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

    }
}