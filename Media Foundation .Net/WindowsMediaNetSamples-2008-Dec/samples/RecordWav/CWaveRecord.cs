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
using System.Threading;
using System.Diagnostics;

using WindowsMediaLib;
using WindowsMediaLib.Defs;
using MultiMedia;

namespace RecordWav
{
    class CWaveRecord : IDisposable
    {
        private const short WAVE_FORMAT_PCM = 0x1;
        private const int RECORDINGBUFFERS = 10;
        private const int FORMATBLOCKSIZE = 16;

        private IntPtr m_hDevice;                   // Handle to the waveIn device
        private int m_iDevice;                      // Index of the device
        private IntPtr m_OutputFile;                // Handle to the output file
        private RecordingBuffer[] m_Buffers;        // Buffers for the waveIn device to write to
        private waveIn.WaveInDelegate m_Delegate;   // Callback routine
        private AutoResetEvent m_ProcReady;         // Event used to coordinate callback with thread
        private AutoResetEvent m_Sample;            // Event used to coordinate callback with thread
        private int m_AudioLength;                  // How much audio data in the file
        private WaveFormatEx m_wfe;                 // Format of the audio data
        private bool m_Closing;                     // Set to indicate that the file is being closed
        private int m_DataOffset;                   // Offset in the file where audio data starts
        private bool m_Disposed;                    // Has IDisposable::Dispose been called?

        /// <summary>
        /// Returns an array of recording devices
        /// </summary>
        /// <returns>Capabilities of the devices</returns>
        public static WaveInCaps[] GetDevices()
        {
            int iCount = waveIn.GetNumDevs();
            WaveInCaps[] pwicRet = new WaveInCaps[iCount];

            for (int x = 0; x < iCount; x++)
            {
                pwicRet[x] = new WaveInCaps();
                int mmr = waveIn.GetDevCaps(x, pwicRet[x], Marshal.SizeOf(pwicRet[x]));
                waveOut.ThrowExceptionForError(mmr);
            }

            return pwicRet;
        }

        /// <summary>
        /// Create an instance of the recording class
        /// </summary>
        /// <param name="iDevice">Index of the device to record from (see GetDevices)</param>
        public CWaveRecord(int iDevice)
        {
            m_Disposed = false;
            m_AudioLength = 0;
            m_iDevice = iDevice;
            m_Delegate = new waveIn.WaveInDelegate(WaveInProc);
            m_Sample = new AutoResetEvent(false);
            m_ProcReady = new AutoResetEvent(false);
        }

        ~CWaveRecord()
        {
            m_Disposed = true;
            if (!m_Disposed)
            {
                Close();
            }
        }

        /// <summary>
        /// Open a new file to write recording data to.  If there is an existing file, its
        /// contents will be lost.
        /// </summary>
        /// <param name="sFilename">File name to write to</param>
        /// <param name="channels">1 = mono, 2 = stereo</param>
        /// <param name="bits">8 = 8bit audio, 16 = 16bit audio</param>
        /// <param name="sampleRate">Audio rate (11025, 22050, 44100, 48000 etc)</param>
        public void CreateNew(string sFilename, short channels, short bits, int sampleRate)
        {
            if (!m_Disposed)
            {
                Close();

                try
                {
                    OpenWaveFile(sFilename, MMIOFlags.Create | MMIOFlags.Exclusive | MMIOFlags.Write);
                    CreateWFE(channels, bits, sampleRate);
                    OpenWaveDevice();
                }
                catch
                {
                    Close();

                    throw;
                }

                WriteHeader();
                m_Closing = false;
            }
            else
            {
                throw new Exception("Instance is Disposed");
            }
        }

        /// <summary>
        /// Add recording data to an existing file.  If the file doesn't exist, an error
        /// gets thrown.
        /// </summary>
        /// <param name="sFilename">File to apppend data to</param>
        public void AppendExisting(string sFilename)
        {
            if (!m_Disposed)
            {
                Close();

                try
                {
                    OpenWaveFile(sFilename, MMIOFlags.ReadWrite | MMIOFlags.Exclusive);
                    LoadWFE();
                    OpenWaveDevice();
                }
                catch
                {
                    Close();

                    throw;
                }

                m_Closing = false;
            }
            else
            {
                throw new Exception("Instance is Disposed");
            }
        }

        /// <summary>
        /// Close the file
        /// </summary>
        public void Close()
        {
            if (!m_Disposed)
            {
                m_Closing = true;
                if (m_hDevice != IntPtr.Zero)
                {
                    waveIn.Reset(m_hDevice);  // Does an implicit Stop
                    m_ProcReady.WaitOne(); // Wait for the thread to exit
                }

                // No point in hanging on to the buffers.  The next file
                // we open may use different attributes.
                if (m_Buffers != null)
                {
                    for (int x = 0; x < RECORDINGBUFFERS; x++)
                    {
                        m_Buffers[x].Release(m_hDevice);
                    }

                    m_Buffers = null;
                }

                if (m_hDevice != IntPtr.Zero)
                {
                    waveIn.Close(m_hDevice);
                    m_hDevice = IntPtr.Zero;
                }

                if (m_OutputFile != IntPtr.Zero)
                {
                    WriteHeader();

                    MMIO.Flush(m_OutputFile, MMIOFlushFlags.None);
                    MMIO.Close(m_OutputFile, MMIOCloseFlags.None);
                    m_OutputFile = IntPtr.Zero;
                }
            }
            else
            {
                throw new Exception("Instance is Disposed");
            }
        }

        /// <summary>
        /// Start (or resume) recording.  Note that the file must be opened first.
        /// </summary>
        public void Record()
        {
            if (!m_Disposed)
            {
                MMIO.Seek(m_OutputFile, 0, MMIOSeekFlags.End);

                int mmr = waveIn.Start(m_hDevice);
                waveIn.ThrowExceptionForError(mmr);
            }
            else
            {
                throw new Exception("Instance is Disposed");
            }
        }

        /// <summary>
        /// Pause the recording
        /// </summary>
        public void Pause()
        {
            if (!m_Disposed)
            {
                if (m_hDevice != IntPtr.Zero)
                {
                    int mmr = waveIn.Stop(m_hDevice);
                    waveIn.ThrowExceptionForError(mmr);
                }
                else
                {
                    throw new Exception("Device is not open");
                }
            }
            else
            {
                throw new Exception("Instance is Disposed");
            }
        }

        #region Private methods

        private void OpenWaveFile(string sFilename, MMIOFlags f)
        {
            MMIOINFO minfo = new MMIOINFO();

            m_OutputFile = MMIO.Open(sFilename, minfo, f);
            if (m_OutputFile == IntPtr.Zero)
            {
                MMIO.ThrowExceptionForError(minfo.wErrorRet);
            }
        }

        private void OpenWaveDevice()
        {
            // Create a thread to handle callbacks from the recording device
            Thread t = new Thread(ProcSample);
            t.Start();

            // Wait for thread to start
            m_ProcReady.WaitOne();

            // Open the recording device, pointing to our callback routine.
            int mmr = waveIn.Open(out m_hDevice, m_iDevice, m_wfe, m_Delegate, IntPtr.Zero, WaveOpenFlags.Function);
            waveIn.ThrowExceptionForError(mmr);

            // Initialize buffers and send them to the recording device
            InitBuffers();
        }

        /// <summary>
        /// Read the WaveFormatEx from the input file and find the place to start
        /// writing data.
        /// </summary>
        private void LoadWFE()
        {
            MMCKINFO mmckinfoParentIn = new MMCKINFO();
            MMCKINFO mmckinfoSubchunkIn = new MMCKINFO();

            int mm = MMIO.Seek(m_OutputFile, 0, MMIOSeekFlags.Set);
            if (mm < 0)
            {
                throw new Exception("seek failure");
            }

            // Check if this is a wave file
            mmckinfoParentIn.fccType = new FourCC("WAVE");
            MMIOError rc = MMIO.Descend(m_OutputFile, mmckinfoParentIn, null, RiffChunkFlags.FindRiff);
            MMIO.ThrowExceptionForError(rc);

            // Get format info
            mmckinfoSubchunkIn.ckid = new FourCC("fmt ");
            rc = MMIO.Descend(m_OutputFile, mmckinfoSubchunkIn, mmckinfoParentIn, RiffChunkFlags.FindChunk);
            MMIO.ThrowExceptionForError(rc);

            // Read the data format from the file (WaveFormatEx)
            IntPtr ip = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WaveFormatEx)));

            try
            {
                rc = MMIO.Read(m_OutputFile, ip, mmckinfoSubchunkIn.ckSize);
                if (rc < 0)
                {
                    throw new Exception("Read failed");
                }

                m_wfe = new WaveFormatEx();
                Marshal.PtrToStructure(ip, m_wfe);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            rc = MMIO.Ascend(m_OutputFile, mmckinfoSubchunkIn, 0);
            MMIO.ThrowExceptionForError(rc);

            // Find the data subchunk
            mmckinfoSubchunkIn.ckid = new FourCC("data");
            rc = MMIO.Descend(m_OutputFile, mmckinfoSubchunkIn, mmckinfoParentIn, RiffChunkFlags.FindChunk);
            MMIO.ThrowExceptionForError(rc);

            // Here is where data gets written
            m_DataOffset = MMIO.Seek(m_OutputFile, 0, MMIOSeekFlags.Cur);

            // Get the length of the audio
            m_AudioLength = mmckinfoSubchunkIn.ckSize;
        }

        /// <summary>
        /// Prepare buffers for use by the recording device, and send them to the device
        /// </summary>
        private void InitBuffers()
        {
            int mmr;
            int iBufferSize = m_wfe.nAvgBytesPerSec / 4;  // Quarter of a second

            m_Buffers = new RecordingBuffer[RECORDINGBUFFERS];

            for (int x = 0; x < RECORDINGBUFFERS; x++)
            {
                m_Buffers[x] = new RecordingBuffer(iBufferSize, m_hDevice);
                mmr = waveIn.AddBuffer(m_hDevice, m_Buffers[x].GetPtr(), Marshal.SizeOf(typeof(WAVEHDR)));
                waveIn.ThrowExceptionForError(mmr);
            }
        }

        /// <summary>
        /// Re-write the header, updating sizes
        /// </summary>
        private void WriteHeader()
        {
            int rc = MMIO.Seek(m_OutputFile, 0, MMIOSeekFlags.Set);
            if (rc < 0)
            {
                throw new Exception("seek failure");
            }

            WriteWaveHeader();
            WriteFormatBlock();
            WriteDataBlock();
        }

        private void WriteWaveHeader()
        {
            MMCKINFO mmckinfoParentIn = new MMCKINFO();

            mmckinfoParentIn.fccType = new FourCC("WAVE");
            mmckinfoParentIn.ckSize = m_AudioLength + m_DataOffset - 8;
            MMIOError rc = MMIO.CreateChunk(m_OutputFile, mmckinfoParentIn, RiffChunkFlags.CreateRiff);
            MMIO.ThrowExceptionForError(rc);
        }
        private void WriteFormatBlock()
        {
            MMCKINFO mmckinfoParentIn = new MMCKINFO();

            // Get format info
            mmckinfoParentIn.ckid = new FourCC("fmt ");
            mmckinfoParentIn.ckSize = FORMATBLOCKSIZE;
            MMIOError rc = MMIO.CreateChunk(m_OutputFile, mmckinfoParentIn, RiffChunkFlags.None);
            MMIO.ThrowExceptionForError(rc);

            IntPtr ip = Marshal.AllocCoTaskMem(FORMATBLOCKSIZE);

            try
            {
                Marshal.StructureToPtr(m_wfe, ip, false);

                int iBytes = MMIO.Write(m_OutputFile, ip, FORMATBLOCKSIZE);
                if (iBytes < 0)
                {
                    throw new Exception("mmioWrite failed");
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            rc = MMIO.Ascend(m_OutputFile, mmckinfoParentIn, 0);
            MMIO.ThrowExceptionForError(rc);
        }
        private void WriteDataBlock()
        {
            MMCKINFO mmckinfoSubchunkIn = new MMCKINFO();

            mmckinfoSubchunkIn.ckid = new FourCC("data");
            mmckinfoSubchunkIn.ckSize = m_AudioLength;
            MMIOError rc = MMIO.CreateChunk(m_OutputFile, mmckinfoSubchunkIn, 0);
            MMIO.ThrowExceptionForError(rc);
        }

        /// <summary>
        /// Write a sample to the file
        /// </summary>
        /// <param name="wh"></param>
        private void WriteSample(WAVEHDR wh)
        {
            int iBytes = MMIO.Write(m_OutputFile, wh.lpData, wh.dwBytesRecorded);
            if (iBytes < 0)
            {
                throw new Exception("Write Failed");
            }
            m_AudioLength += iBytes;
        }

        /// <summary>
        /// The recording device calls this routine when it is opened, closed, or when it
        /// is done with a buffer.
        /// </summary>
        /// <param name="hwo">Device handle of the recording device</param>
        /// <param name="uMsg">Open, close, or done</param>
        /// <param name="dwInstance">Instance data passed in waveInOpen</param>
        /// <param name="dwParam1">Pointer to the buffer that is "done"</param>
        /// <param name="dwParam2"></param>
        private void WaveInProc(
            IntPtr hwo,
            MIWM uMsg,
            IntPtr dwInstance,
            IntPtr dwParam1,
            IntPtr dwParam2)
        {
            switch (uMsg)
            {
                case MIWM.WIM_OPEN:
                    break;

                case MIWM.WIM_DATA:
                    // Avoid having a second callback fire before the first one is processed
                    m_ProcReady.WaitOne();
                    m_Sample.Set();
                    break;

                case MIWM.WIM_CLOSE:
                    break;
            }
        }

        /// <summary>
        /// Thread to process samples when the recording device is done with them
        /// </summary>
        private void ProcSample()
        {
            int iCount = 0;
            int iNextSample = 0;

            // Notify that thread is started
            m_ProcReady.Set();

            do
            {
                // Notify that we are ready to process samples
                m_ProcReady.Set();

                //  Wait for the callback to receive a sample
                m_Sample.WaitOne();

                if (!m_Closing)
                {
                    WAVEHDR wh = m_Buffers[iNextSample].GetHdr();

                    WriteSample(wh);

                    // Send the buffer back to the recording device for re-use
                    int mmr = waveIn.AddBuffer(m_hDevice, m_Buffers[iNextSample].GetPtr(), Marshal.SizeOf(typeof(WAVEHDR)));
                    waveIn.ThrowExceptionForError(mmr);
                }
                else
                {
                    iCount++;
                }
                // Loop around
                iNextSample = (iNextSample + 1) % RECORDINGBUFFERS;
            } while (iCount < RECORDINGBUFFERS);

            Debug.WriteLine("Thread Exiting");
            m_ProcReady.Set();
        }

        /// <summary>
        /// Create a WaveFormatEx from parameters
        /// </summary>
        /// <param name="channels">1 = mono, 2 = stereo</param>
        /// <param name="bits">8 = 8bit audio, 16 = 16bit audio</param>
        /// <param name="sampleRate">Audio rate (11025, 22050, 44100, 48000 etc)</param>
        private void CreateWFE(short channels, short bits, int sampleRate)
        {
            m_wfe = new WaveFormatEx();

            m_wfe.wFormatTag = WAVE_FORMAT_PCM;
            m_wfe.nChannels = channels;
            m_wfe.nSamplesPerSec = sampleRate;
            m_wfe.wBitsPerSample = bits;
            m_wfe.nBlockAlign = (short)(m_wfe.nChannels * (m_wfe.wBitsPerSample / 8));
            m_wfe.nAvgBytesPerSec = m_wfe.nSamplesPerSec * m_wfe.nBlockAlign;
            m_wfe.cbSize = 0;

            m_DataOffset = 44;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_Disposed = true;
            if (!m_Disposed)
            {
                Close();
                GC.SuppressFinalize(this);

                m_Delegate = null;
                m_wfe = null;

                if (m_ProcReady != null)
                {
                    m_ProcReady.Close();
                    m_ProcReady = null;
                }

                if (m_Sample != null)
                {
                    m_Sample.Close();
                    m_Sample = null;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// A class that holds a WAVEHDR to pass to the recording device.  These structures *must*
    /// be pinned, since the recording device will attemt to write to them at the address that
    /// was passed to AddBuffers.  If the GC were to move the buffers, bad things would happen.
    /// </summary>
    internal class RecordingBuffer
    {
        private GCHandle m_Handle;
        private WAVEHDR m_Head;

        public RecordingBuffer(int iSize, IntPtr ipHandle)
        {
            m_Head = new WAVEHDR(iSize);
            m_Handle = GCHandle.Alloc(m_Head, GCHandleType.Pinned);

            int mmr = waveIn.PrepareHeader(ipHandle, m_Head, Marshal.SizeOf(m_Head));
            waveIn.ThrowExceptionForError(mmr);
        }

        public IntPtr GetPtr()
        {
            return m_Handle.AddrOfPinnedObject();
        }

        public WAVEHDR GetHdr()
        {
            return m_Head;
        }

        /// <summary>
        /// Disconnect a buffer from the recording device.  While I could keep the ipHandle as
        /// a member, by forcing people to pass it, it helps people remember not to close the
        /// device until the buffers have been released.
        /// </summary>
        /// <param name="ipHandle"></param>
        public void Release(IntPtr ipHandle)
        {
            int mmr = waveIn.UnprepareHeader(ipHandle, m_Head, Marshal.SizeOf(m_Head));
            waveIn.ThrowExceptionForError(mmr);
        }
    }
}
