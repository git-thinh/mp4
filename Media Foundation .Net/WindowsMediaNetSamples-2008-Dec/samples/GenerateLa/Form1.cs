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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MultiMedia;
using WindowsMediaLib;
using WindowsMediaLib.Defs;
using System.Runtime.InteropServices;

namespace GenerateLa
{
    public partial class Form1 : Form
    {
        private WaveFormatEx m_Format;
        IntPtr m_pWave;
        WBuf m_Buff;

        public Form1()
        {
            InitializeComponent();
            m_Format = new WaveFormatEx();

            m_Format.wFormatTag = 1; // PCM
            m_Format.nChannels = 2; // Stereo
            m_Format.nSamplesPerSec = 44100;
            m_Format.wBitsPerSample = 8;
            m_Format.nBlockAlign = (short)(m_Format.nChannels * (m_Format.wBitsPerSample / 8));
            m_Format.nAvgBytesPerSec = m_Format.nSamplesPerSec * m_Format.nBlockAlign;
            m_Format.cbSize = 0;

            int iRet = waveOut.Open(out m_pWave, 0, m_Format, IntPtr.Zero, IntPtr.Zero, WaveOpenFlags.None);

            // Make a single buffer big enough to hold the entire sound, based on the maxium value on nudDuration
            m_Buff = new WBuf(m_pWave, m_Format.nSamplesPerSec * (int)Math.Ceiling(nudDuration.Maximum / 10) * m_Format.nBlockAlign);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Only going to be visible if a second beep is requested while the
            // first beep is still, umm, beeping
            this.Cursor = Cursors.WaitCursor;

            int iSize = m_Buff.GenerateLa(m_Format, (int)nudDuration.Value, (int)nudFreq.Value);

            // Wait for a previous write to complete
            while (!m_Buff.IsBufferFree())
            {
                System.Threading.Thread.Sleep(1);
            }

            int iRet = waveOut.Write(m_pWave, m_Buff.GetPtr(), iSize);
            waveOut.ThrowExceptionForError(iRet);

            this.Cursor = Cursors.Default;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_Buff.Release(m_pWave);

            int iRet = waveOut.Close(m_pWave);
            waveOut.ThrowExceptionForError(iRet);
        }
    }
    /// <summary>
    /// A class that holds a WAVEHDR to pass to the playback device.  These structures *must*
    /// be pinned, since the playback device will attemt to write to them at the address that
    /// was passed to WaveOutWrite.  If the GC were to move the buffers, bad things would happen.
    /// </summary>
    internal class WBuf
    {
        private WAVEHDR m_Head;
        private int m_MaxLen;
        private GCHandle m_Handle;

        public WBuf(IntPtr waveHandle, int iMaxBuf)
        {
            m_MaxLen = iMaxBuf;
            m_Head = new WAVEHDR(iMaxBuf);
            m_Head.dwBytesRecorded = 0;

            // Since waveOutWrite will continue to the buffer after the call
            // returns, the buffer must be fixed in place.
            m_Handle = GCHandle.Alloc(m_Head, GCHandleType.Pinned);

            int mmr = waveOut.PrepareHeader(waveHandle, m_Head, Marshal.SizeOf(typeof(WAVEHDR)));
            waveOut.ThrowExceptionForError(mmr);

            // Make it easier for IsBufferFree() to determine when buffers are free
            m_Head.dwFlags |= WAVEHDR.WHDR.Done;
        }
        public void Release(IntPtr waveHandle)
        {
            int mmr;

            mmr = waveOut.UnprepareHeader(waveHandle, m_Head, Marshal.SizeOf(typeof(WAVEHDR)));
            waveOut.ThrowExceptionForError(mmr);

            m_Head.Dispose();
            m_Handle.Free();
        }
        public bool IsBufferFree()
        {
            return (m_Head.dwFlags & WAVEHDR.WHDR.Done) != 0;
        }
        public IntPtr GetPtr()
        {
            return m_Handle.AddrOfPinnedObject();
        }

        public static int GenerateLaSize(WaveFormatEx wfx, int iDuration, out int iSamples)
        {
            iSamples = (int)Math.Ceiling((wfx.nSamplesPerSec * iDuration) / 10.0);
            return iSamples * wfx.nBlockAlign;
        }
        public int GenerateLa(WaveFormatEx wfx, int iDuration, double freq)
        {
            Debug.Assert(wfx.wFormatTag == 1, "Illegal wFormatTag");
            Debug.Assert(wfx.wBitsPerSample == 8 || wfx.wBitsPerSample == 16, "Unrecognized wBitsPerSample");
            Debug.Assert(wfx.nChannels == 1 || wfx.nChannels == 2, "Unrecognized nChannels");

            int iSamples;
            int iSize = GenerateLaSize(wfx, iDuration, out iSamples);
            if (iSize > m_MaxLen)
            {
                throw new Exception("Requested duration greater than buffer's max size");
            }
            m_Head.dwBufferLength = iSize;

            int iOffset = 0;
            IntPtr pBuff = m_Head.lpData;

            for (int i = 0; i < iSamples; i++)
            {
                double y = Math.Sin((freq * 2 * Math.PI * i) / wfx.nSamplesPerSec); // tone

                if (wfx.wBitsPerSample == 8)
                {
                    byte sample = (byte)(127.5 * (y + 1.0));
                    Marshal.WriteByte(pBuff, iOffset, sample); // Note: This would perform better using pointers in unsafe mode
                    iOffset++;
                    if (wfx.nChannels == 2)
                    {
                        Marshal.WriteByte(pBuff, iOffset, sample); // Note: This would perform better using pointers in unsafe mode
                        iOffset++;
                    }
                }
                else
                {
                    short sample = (short)((32767.5 * y) - 0.5);
                    Marshal.WriteInt16(pBuff, iOffset, sample); // Note: This would perform better using pointers in unsafe mode
                    iOffset += 2;
                    if (wfx.nChannels == 2)
                    {
                        Marshal.WriteInt16(pBuff, iOffset, sample); // Note: This would perform better using pointers in unsafe mode
                        iOffset += 2;
                    }
                }
            }

            return iSize;
        }
    }
}