/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using WindowsMediaLib;
using WindowsMediaLib.Defs;

using MultiMedia;

namespace AudioPlayer
{
    class CAudioPlay : COMBase, IWMReaderCallback, IWMReaderCallbackAdvanced, IDisposable
    {
        private const int ONESEC = 10000000;
        private const int MAXBUFFERS = 20;
        private int WAVHDRLEN = Marshal.SizeOf(typeof(WAVEHDR));

        public enum CurStatus
        {
            Closed,
            Ready,
            Stop,
            Play,
            Buffering,
            AcquiringLicense,
            Individualizing
        }

        public class StatusChangeArgs : System.EventArgs
        {
            public CurStatus newStatus;

            /// <summary>
            /// Used to construct an instace of the class.
            /// </summary>
            /// <param name="ec"></param>
            internal StatusChangeArgs(CurStatus sc)
            {
                newStatus = sc;
            }
        }

        public class TimeChangeArgs : System.EventArgs
        {
            public long newTime;

            /// <summary>
            /// Used to construct an instace of the class.
            /// </summary>
            /// <param name="ec"></param>
            internal TimeChangeArgs(long tc)
            {
                newTime = tc;
            }
        }

        #region Members

        public event EventHandler StatusChanged = null;
        public event EventHandler TimeChanged = null;

        private bool m_bIsSeekable;                     // TRUE if the content is seekable
        private bool m_bIsBroadcast;                    // TRUE if playing a broadcast stream
        private short m_dwAudioOutputNum;               // audio output number
        private AutoResetEvent m_hAsyncEvent;           // event handle
        private int m_hrAsync;                          // HRESULT during async operations
        private IntPtr m_hWaveOut;                      // handle to waveout device
        private IWMReader m_pReader;                    // IWMReader pointer
        private IWMHeaderInfo m_pHeaderInfo;            // IWMHeaderInfo pointer
        private long m_cnsFileDuration;                 // content's playback duration
        private WaveFormatEx m_pWfx;                    // waveformat struct
        private int m_MaxSampleSize;
        private long m_OldTime;
        private int m_UseNext;
        private INSBuf[] m_InsBuf;
        private int m_DeviceIndex;
        private WaveCapsFlags m_WaveCaps;
        private IntPtr m_hMixer;
        private Form m_fForm;

        #endregion

        /// <summary>
        /// Create the player
        /// </summary>
        /// <param name="iDevice">Zero based index of the playback device.  See GetDevs for a list of devices.</param>
        public CAudioPlay(int iDevice, Form fForm)
        {
            m_DeviceIndex = iDevice;
            m_fForm = fForm;

            WaveOutCaps woc = new WaveOutCaps();
            int mmr = waveOut.GetDevCaps(iDevice, woc, Marshal.SizeOf(woc));
            waveOut.ThrowExceptionForError(mmr);

            m_WaveCaps = woc.dwSupport;

            m_bIsSeekable = false;
            m_bIsBroadcast = false;
            m_dwAudioOutputNum = -1;
            m_hrAsync = 0;
            m_hWaveOut = IntPtr.Zero;
            m_hMixer = IntPtr.Zero;
            m_pReader = null;
            m_pHeaderInfo = null;
            m_cnsFileDuration = 0;
            m_pWfx = null;
            m_MaxSampleSize = -1;
            m_OldTime = -1;
            m_UseNext = 0;
            m_InsBuf = new INSBuf[MAXBUFFERS];

            // Create an event for asynchronous calls.
            // When code in this class makes a call to an asynchronous
            //  method, it will wait for the event to be set to resume
            //  processing.
            m_hAsyncEvent = new AutoResetEvent(false);

            // Create a reader object, requesting only playback rights.
            WMUtils.WMCreateReader(IntPtr.Zero, Rights.Playback, out m_pReader);
        }

        ~CAudioPlay()
        {
            Dispose();
        }

        public bool IsDisposed()
        {
            return m_pReader == null;
        }

        static public WaveOutCaps[] GetDevs()
        {
            int iCount = waveOut.GetNumDevs();
            WaveOutCaps[] pwocRet = new WaveOutCaps[iCount];

            for (int x = 0; x < iCount; x++)
            {
                pwocRet[x] = new WaveOutCaps();
                int mmr = waveOut.GetDevCaps(x, pwocRet[x], Marshal.SizeOf(pwocRet[x]));
                waveOut.ThrowExceptionForError(mmr);
            }

            return pwocRet;
        }

        /// <summary>
        /// Open the file that contains the audio
        /// </summary>
        /// <param name="pwszUrl"></param>
        public void Open(string pwszUrl)
        {
            // Check that the parameter is not NULL and that the reader is initialized.
            if (null == pwszUrl)
            {
                throw new COMException("null url", E_InvalidArgument);
            }

            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            try
            {
                // Close previously opened file, if any.
                Close();
            }
            catch { }

            m_hAsyncEvent.Reset();

            // Open the file with the reader object. This method call also sets
            //  the status callback that the reader will use.
            m_pReader.Open(pwszUrl, this, IntPtr.Zero);

            // Wait for the Open call to complete. The event is set in the OnStatus
            //  callback when the reader reports completion.
            m_hAsyncEvent.WaitOne();

            // Check the HRESULT reported by the reader object to the OnStatus
            //  callback. Most errors in opening files will be reported this way.
            if (Failed(m_hrAsync))
            {
                throw new COMException("Could not open the specified file", m_hrAsync);
            }

            try
            {
                m_pHeaderInfo = (IWMHeaderInfo)m_pReader;

                // Get Seekable, Broadcast & Duration
                RetrieveAttributes();

                // Set the audio ouput number for the current file.
                // Only the first audio output is retrieved, regardless of the
                //  number of audio outputs in the file.
                GetAudioOutput();
            }
            catch
            {
                try
                {
                    Close();
                }
                catch { }
                throw;
            }
        }
        /// <summary>
        /// Close the audio file
        /// </summary>
        public void Close()
        {
            if (!IsDisposed())
            {
                try
                {
                    Stop();
                }
                catch { }

                m_pReader.Close();

                // Wait for the reader to deliver a WMT_CLOSED event to the OnStatus
                //  callback.
                m_hAsyncEvent.WaitOne();
            }

            //
            // Close the wave output device.
            //
            if (m_hWaveOut != IntPtr.Zero)
            {
                for (int x = 0; x < MAXBUFFERS; x++)
                {
                    if (m_InsBuf[x] != null)
                    {
                        m_InsBuf[x].Release(m_hWaveOut);
                    }
                }
                waveOut.Restart(m_hWaveOut); // ignore error
                waveOut.Close(m_hWaveOut); // ignore error

                m_hWaveOut = IntPtr.Zero;
            }

            // Close the mixer
            if (m_hMixer != IntPtr.Zero)
            {
                Mixer.Close(m_hMixer);
                m_hMixer = IntPtr.Zero;
            }
        }
        /// <summary>
        /// Start playing the audio
        /// </summary>
        /// <param name="cnsStart"></param>
        public void Start(long cnsStart)
        {
            //
            // Ensure that a reader object has been instantiated.
            //
            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }
            m_OldTime = -1;

            //
            // Configure the wave output device.
            //
            if (IntPtr.Zero != m_hWaveOut)
            {
                int woe = waveOut.Reset(m_hWaveOut);
                waveOut.ThrowExceptionForError(woe);
            }
            else
            {
                int mmr = waveOut.Open(out m_hWaveOut,
                                            m_DeviceIndex,
                                            m_pWfx,
                                            IntPtr.Zero,
                                            IntPtr.Zero,
                                            WaveOpenFlags.Null);
                waveOut.ThrowExceptionForError(mmr);

                // If a form was provided
                if (m_fForm != null)
                {
                    // Ensure volume change events get sent
                    MIXER_OBJECTF flags = MIXER_OBJECTF.CallBack_Window | MIXER_OBJECTF.WaveOut;
                    MMSYSERR rc = Mixer.Open(out m_hMixer, m_DeviceIndex, m_fForm.Handle, IntPtr.Zero, flags);
                    // Not checking for error.  If something goes wrong, rather than fail the call, we
                    // just won't update the volume bar
                }

                for (int x = 0; x < MAXBUFFERS; x++)
                {
                    m_InsBuf[x] = new INSBuf(m_hWaveOut, m_MaxSampleSize);
                }
            }
            m_pReader.Start(cnsStart, 0, 1.0f, IntPtr.Zero);
        }
        public void Stop()
        {
            //
            // Ensure that a reader object has been instantiated.
            //
            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            m_pReader.Stop();

            //
            // Reset the wave output device
            //
            if (IntPtr.Zero != m_hWaveOut)
            {
                int woe = waveOut.Reset(m_hWaveOut);
                waveOut.ThrowExceptionForError(woe);

                //
                // Wait for all audio headers to be unprepared.
                //
                m_hAsyncEvent.WaitOne();
            }
        }
        public void Pause()
        {
            //
            // Sanity check
            //
            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            if (null != m_hWaveOut)
            {
                //
                // Pause the wave output device
                //
                int woe = waveOut.Pause(m_hWaveOut);
                waveOut.ThrowExceptionForError(woe);
            }

            m_pReader.Pause();
        }
        public void Resume()
        {
            //
            // Sanity check
            //
            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            m_pReader.Resume();

            if (IntPtr.Zero != m_hWaveOut)
            {
                //
                // Resume the wave output device
                //
                int woe = waveOut.Restart(m_hWaveOut);
                waveOut.ThrowExceptionForError(woe);
            }
        }
        public long GetFileDuration()
        {
            return (m_cnsFileDuration);
        }
        public bool IsSeekable()
        {
            return (m_bIsSeekable);
        }
        public bool IsBroadcast()
        {
            return (m_bIsBroadcast);
        }
        public string GetAttributeString(string sName)
        {
            string sRet;
            byte[] pbValue = null;

            try
            {
                GetHeaderAttribute(sName, out pbValue);
                sRet = Encoding.Unicode.GetString(pbValue);
            }
            catch
            {
                sRet = null;
            }

            return sRet;
        }

        public void SetVolume(int iLeftVolume, int iRightVolume)
        {
            int iVolume = iLeftVolume | (iRightVolume << 16);

            int mmr = waveOut.SetVolume(m_hWaveOut, iVolume);
            waveOut.ThrowExceptionForError(mmr);
        }
        public void GetVolume(out int iLeftVolume, out int iRightVolume)
        {
            int iRet;

            int mmr = waveOut.GetVolume(m_hWaveOut, out iRet);
            waveOut.ThrowExceptionForError(mmr);

            iLeftVolume = iRet & 0xffff;
            iRightVolume = iRet >> 16;
        }
        public void SetPlaybackRate(int iRate)
        {
            if ((m_WaveCaps & WaveCapsFlags.PlaybackRate) > 0)
            {
                int mmr = waveOut.SetPlaybackRate(m_hWaveOut, iRate);
                waveOut.ThrowExceptionForError(mmr);
            }
            else
            {
                throw new NotSupportedException("This wave device doesn't support PlaybackRate");
            }
        }
        public int GetPlaybackRate()
        {
            int iRet;

            if ((m_WaveCaps & WaveCapsFlags.PlaybackRate) > 0)
            {
                int mmr = waveOut.GetPlaybackRate(m_hWaveOut, out iRet);
                waveOut.ThrowExceptionForError(mmr);
            }
            else
            {
                throw new NotSupportedException("This wave device doesn't support PlaybackRate");
            }

            return iRet;
        }
        public void SetPitch(int iPitch)
        {
            if ((m_WaveCaps & WaveCapsFlags.Pitch) > 0)
            {
                int mmr = waveOut.SetPitch(m_hWaveOut, iPitch);
                waveOut.ThrowExceptionForError(mmr);
            }
            else
            {
                throw new NotSupportedException("This wave device doesn't support Pitch");
            }
        }
        public int GetPitch()
        {
            int iRet = 0;

            if ((m_WaveCaps & WaveCapsFlags.Pitch) > 0)
            {
                int mmr = waveOut.GetPitch(m_hWaveOut, out iRet);
                waveOut.ThrowExceptionForError(mmr);
            }
            else
            {
                throw new NotSupportedException("This wave device doesn't support Pitch");
            }

            return iRet;
        }

        private void SetAsyncEvent(int hrAsync)
        {
            m_hrAsync = hrAsync;
            m_hAsyncEvent.Set();
        }
        private void ReportStatusChange(CurStatus cs)
        {
            if (StatusChanged != null)
            {
                StatusChangeArgs sa = new StatusChangeArgs(cs);
                try
                {
                    StatusChanged(this, sa);
                }
                catch { }
            }
        }
        private void GetHeaderAttribute(string pwszName, out byte[] ppbValue)
        {
            AttrDataType wmtType;
            short cbLength = 0;
            short wAnyStream = 0;
            ppbValue = null;

            //
            // Sanity check
            //
            if (null == m_pHeaderInfo)
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            //
            // Get the count of bytes to be allocated for pbValue
            //
            m_pHeaderInfo.GetAttributeByName(ref wAnyStream,
                                                    pwszName,
                                                    out wmtType,
                                                    null,
                                                    ref cbLength);

            ppbValue = new byte[cbLength];

            //
            // Get the actual value
            //
            m_pHeaderInfo.GetAttributeByName(ref wAnyStream,
                                                    pwszName,
                                                    out wmtType,
                                                    ppbValue,
                                                    ref cbLength);
        }
        private void RetrieveAttributes()
        {
            byte[] pbValue = null;

            //
            // Get attribute "Duration"
            //
            try
            {
                GetHeaderAttribute(Constants.g_wszWMDuration, out pbValue);
                m_cnsFileDuration = BitConverter.ToInt64(pbValue, 0);
            }
            catch
            {
                m_cnsFileDuration = 0;
            }

            //
            // Retrieve Seekable attribute
            //
            try
            {
                GetHeaderAttribute(Constants.g_wszWMSeekable, out pbValue);
                m_bIsSeekable = BitConverter.ToBoolean(pbValue, 0);
            }
            catch
            {
                m_bIsSeekable = false;
            }

            //
            // Retrieve Broadcast attribute
            //
            try
            {
                GetHeaderAttribute(Constants.g_wszWMBroadcast, out pbValue);
                m_bIsBroadcast = BitConverter.ToBoolean(pbValue, 0);
            }
            catch
            {
                m_bIsBroadcast = false;
            }

        }
        private void GetAudioOutput()
        {
            int cOutputs = 0;
            short i;
            IWMOutputMediaProps pProps = null;
            AMMediaType pMediaType = null;
            int cbType = 0;

            //
            // Sanity check
            //
            if (IsDisposed())
            {
                throw new COMException("Instance has been Disposed", E_Unexpected);
            }

            //
            // Find out the output count
            //
            m_pReader.GetOutputCount(out cOutputs);

            //
            // Find one audio output.
            // Note: This sample only shows how to handle one audio output.
            //       If there is more than one audio output, the first one will be picked.
            //
            StringBuilder sName = null;
            for (i = 0; i < cOutputs; i++)
            {
                m_pReader.GetOutputProps(i, out pProps);

                try
                {
                    //
                    // Find out the space needed for pMediaType
                    //
                    cbType = 0;
                    pMediaType = null;
                    pProps.GetMediaType(pMediaType, ref cbType);

                    // Get the name of the output we'll be using
                    sName = null;
                    short iName = 0;
                    pProps.GetConnectionName(sName, ref iName);

                    sName = new StringBuilder(iName);
                    pProps.GetConnectionName(sName, ref iName);

                    pMediaType = new AMMediaType();
                    pMediaType.formatSize = cbType - Marshal.SizeOf(typeof(AMMediaType));

                    //
                    // Get the value for MediaType
                    //
                    pProps.GetMediaType(pMediaType, ref cbType);

                    try
                    {
                        if (MediaType.Audio == pMediaType.majorType)
                        {
                            m_pWfx = new WaveFormatEx();
                            Marshal.PtrToStructure(pMediaType.formatPtr, m_pWfx);
                            break;
                        }
                    }
                    finally
                    {
                        WMUtils.FreeWMMediaType(pMediaType);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(pProps);
                }
            }

            if (i == cOutputs)
            {
                throw new COMException("Could not find an audio stream in the specified file", E_Unexpected);
            }

            m_dwAudioOutputNum = i;

            // Only deliver samples for the single output
            SetStreams(sName.ToString());
        }
        private void SetStreams(string sName)
        {
            // First find the mapping between the output name and
            // the stream number
            IWMProfile pProfile = m_pReader as IWMProfile;

            int iCount;
            pProfile.GetStreamCount(out iCount);
            short[] sss = new short[iCount];
            StreamSelection[] ss = new StreamSelection[iCount];

            StringBuilder sSName;

            for (short j = 0; j < iCount; j++)
            {
                IWMStreamConfig pConfig;
                pProfile.GetStream(j, out pConfig);

                try
                {
                    pConfig.GetStreamNumber(out sss[j]);

                    short iSName = 0;
                    sSName = null;
                    pConfig.GetConnectionName(sSName, ref iSName);
                    sSName = new StringBuilder(iSName);
                    pConfig.GetConnectionName(sSName, ref iSName);
                }
                finally
                {
                    Marshal.ReleaseComObject(pConfig);
                }

                // Turn the stream on or off depending on whether
                // this is the one that matches the output we're
                // looking for
                if (sSName.ToString() == sName)
                {
                    ss[j] = StreamSelection.On;
                }
                else
                {
                    ss[j] = StreamSelection.Off;
                }
            }

            // Use the array we've built to specify the streams we want
            IWMReaderAdvanced ra = m_pReader as IWMReaderAdvanced;
            ra.SetStreamsSelected((short)iCount, sss, ss);

            // Learn the maximum sample size that will be send to OnSample
            ra.GetMaxOutputSampleSize(m_dwAudioOutputNum, out m_MaxSampleSize);

            // Have the Samples allocated using IWMReaderCallbackAdvanced::AllocateForOutput
            ra.SetAllocateForOutput(m_dwAudioOutputNum, true);
        }

        #region IWMReaderCallback Members

        public void OnSample(int dwOutputNum, long cnsSampleTime, long cnsSampleDuration, SampleFlag dwFlags, INSSBuffer pSample, IntPtr pvContext)
        {
            try
            {
                // Check the output number of the sample against the stored output number.
                // Because only the first audio output is stored, all other outputs,
                //  regardless of type, will be ignored.  Shouldn't be an issue since 
                // SetStreams turned off delivery of samples for all streams except the
                // one we want
                if (dwOutputNum != m_dwAudioOutputNum)
                {
                    return;
                }

                // The rest of the code in this method uses Windows Multimedia wave
                //  handling functions to play the content.
                //
                IntPtr ip = (pSample as INSBuf).GetPtr();

                try
                {
                    //
                    // Send the sample to the wave output device.
                    //
                    int mmr = waveOut.Write(m_hWaveOut, ip, WAVHDRLEN);
                    waveOut.ThrowExceptionForError(mmr);
                }
                catch
                {
                    //
                    // Stop the player.
                    //
                    Stop();
                    throw;
                }
                if (TimeChanged != null)
                {
                    // Only send time events every 1/10 of a second
                    if (Math.Abs(cnsSampleTime - m_OldTime) > (ONESEC / 10))
                    {
                        m_OldTime = cnsSampleTime;
                        TimeChangeArgs ta = new TimeChangeArgs(cnsSampleTime);
                        try
                        {
                            TimeChanged(this, ta);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                // If we weren't allocating our own samples in IWMReaderCallbackAdvanced::AllocateForOutput,
                // we'd have to free this here.

                //Marshal.ReleaseComObject(pSample);
            }
        }

        public void OnStatus(Status iStatus, int hr, AttrDataType dwType, IntPtr pValue, IntPtr pvContext)
        {
            try
            {
                // This switch checks for the important messages sent by the reader object.
                switch (iStatus)
                {
                    // The reader is finished opening a file.
                    case Status.Opened:

                        //
                        // Set the event if DRM operation is not going on
                        //
#if SUPPORT_DRM
                    if (!m_bProcessingDRMOps)
#endif
                        {
                            SetAsyncEvent(hr);
                        }

                        break;

                    // Playback of the opened file has begun.
                    case Status.Started:

                        ReportStatusChange(CurStatus.Play);
                        break;

                    // The reader is finished closing a file.
                    case Status.Closed:

                        SetAsyncEvent(hr);
                        break;

                    // The previously playing reader has stopped.
                    case Status.Stopped:

                        SetAsyncEvent(hr);
                        ReportStatusChange(CurStatus.Stop);
                        break;

                    // This class reacts to any errors by changing its state to stopped.
                    case Status.Error:
                    case Status.EOF:
                    case Status.MissingCodec:

                        //
                        // Set to STOP when no buffer is left for playback
                        //
                        int i;
                        lock (this)
                        {
                            i = (m_UseNext + (MAXBUFFERS - 1)) % MAXBUFFERS;
                        }

                        while (!m_InsBuf[i].IsBufferFree())
                        {
                            Thread.Sleep(1);
                            lock (this)
                            {
                                i = (m_UseNext + (MAXBUFFERS - 1)) % MAXBUFFERS;
                            }
                        }
                        ReportStatusChange(CurStatus.Stop);

                        break;

                    // The reader has begun buffering.
                    case Status.BufferingStart:

                        ReportStatusChange(CurStatus.Buffering);
                        break;

                    // the reader has completed buffering.
                    case Status.BufferingStop:

                        ReportStatusChange(CurStatus.Play);
                        break;

                    case Status.Locating:
                        break;

#if SUPPORT_DRM
                //
                // License and DRM related messages
                //
                case Status.NoRights:
                case Status.NoRightsEx:
                case Status.NeedsIndividualization:
                case Status.LicenseURLSignatureState:

                    m_bProcessingDRMOps = true;
                    objDRM.OnDRMStatus(iStatus, hr, dwType, pValue, pvContext);
                    break;

                case Status.AcquireLicense:
                case Status.Individualize:

                    objDRM.OnDRMStatus(iStatus, hr, dwType, pValue, pvContext);
                    break;
#endif

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        #endregion

        #region IWMReaderCallbackAdvanced Members

        public void OnStreamSample(short wStreamNum, long cnsSampleTime, long cnsSampleDuration, SampleFlag dwFlags, INSSBuffer pSample, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnTime(long cnsCurrentTime, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnStreamSelection(short wStreamCount, short[] pStreamNumbers, StreamSelection[] pSelections, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnOutputPropsChanged(int dwOutputNum, AMMediaType pMediaType, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AllocateForStream(short wStreamNum, int cbBuffer, out INSSBuffer ppBuffer, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AllocateForOutput(int dwOutputNum, int cbBuffer, out INSSBuffer ppBuffer, IntPtr pvContext)
        {
            int i;
            lock (this)
            {
                i = m_UseNext;
                m_UseNext = (m_UseNext + 1) % MAXBUFFERS;
            }

            ppBuffer = m_InsBuf[i];
            while (!m_InsBuf[i].IsBufferFree())
            {
                Thread.Sleep(1);
            }
            ppBuffer.SetLength(cbBuffer);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch { }
            if (!IsDisposed())
            {
                Marshal.ReleaseComObject(m_pReader);
                m_pReader = null;
                m_pHeaderInfo = null;
            }

            if (IntPtr.Zero != m_hWaveOut)
            {
                waveOut.Close(m_hWaveOut); // Ignore return
                m_hWaveOut = IntPtr.Zero;
            }

            if (IntPtr.Zero != m_hMixer)
            {
                Mixer.Close(m_hMixer);
                m_hMixer = IntPtr.Zero;
            }

            if (null != m_hAsyncEvent)
            {
                m_hAsyncEvent.Close();
                m_hAsyncEvent = null;
            }

            m_pWfx = null;
#if SUPPORT_DRM
            objDRM.Dispose();
            m_pwszURL = null;
#endif
        }

        #endregion
    }

    /// <summary>
    /// A class that holds a WAVEHDR to pass to the playback device.  These structures *must*
    /// be pinned, since the playback device will attemt to write to them at the address that
    /// was passed to WaveOutWrite.  If the GC were to move the buffers, bad things would happen.
    /// </summary>
    internal class INSBuf : INSSBuffer
    {
        private WAVEHDR m_Head;
        private int m_MaxLen;
        private GCHandle m_Handle;

        public INSBuf(IntPtr waveHandle, int iMaxBuf)
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

        #region INSSBuffer Members

        public void GetLength(out int pdwLength)
        {
            lock (this)
            {
                pdwLength = m_Head.dwBufferLength;
            }
        }

        public void SetLength(int dwLength)
        {
            Debug.Assert(dwLength <= m_MaxLen);

            lock (this)
            {
                m_Head.dwBufferLength = dwLength;
            }
        }

        public void GetMaxLength(out int pdwLength)
        {
            pdwLength = m_MaxLen;
        }

        public void GetBuffer(out IntPtr ppdwBuffer)
        {
            ppdwBuffer = m_Head.lpData;
        }

        public void GetBufferAndLength(out IntPtr ppdwBuffer, out int pdwLength)
        {
            lock (this)
            {
                ppdwBuffer = m_Head.lpData;
                pdwLength = m_Head.dwBufferLength;
            }
        }

        #endregion
    }
}
