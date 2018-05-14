/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

/* Note!  I don't have any files with scripts in them to test that part of
 * the code with.   That code will need writing before it will work.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using WindowsMediaLib;
using WindowsMediaLib.Defs;

namespace WMVCopy
{
    internal struct CScript
    {
        public CScript(string pwszType, string pwszCommand, long cnsSampleTime)
        {
            m_pwszType = pwszType;
            m_pwszParameter = pwszCommand;
            m_cnsTime = cnsSampleTime;
        }

        public string m_pwszType;
        public string m_pwszParameter;
        public long m_cnsTime;
    }

    public class WMVCopy : COMBase, IWMStatusCallback, IWMReaderCallback, IWMReaderCallbackAdvanced, IDisposable
    {
        #region Member Variables

        protected IWMWriter m_pWriter;
        protected IWMWriterAdvanced m_pWriterAdvanced;
        protected IWMHeaderInfo m_pWriterHeaderInfo;
        protected IWMReader m_pReader;
        protected IWMReaderAdvanced m_pReaderAdvanced;
        protected IWMHeaderInfo m_pReaderHeaderInfo;
        protected IWMProfile m_pReaderProfile;

        protected int m_dwStreamCount;
        protected Guid[] m_pguidStreamType;
        protected short[] m_pwStreamNumber;

        protected AutoResetEvent m_hEvent;
        protected int m_hr;
        protected long m_qwReaderTime;
        protected bool m_fEOF;
        protected bool m_fMoveScriptStream;
        protected long m_qwMaxDuration;

        protected long m_qwDuration;
        protected long m_dwProgress;

        List<CScript> m_ScriptList;

        #endregion

        public WMVCopy()
        {
            m_ScriptList = new List<CScript>();
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::Copy()
        // Desc: Copies the input file to the output file. The script stream is moved
        //       to the header if fMoveScriptStream is true.
        //------------------------------------------------------------------------------
        public void Copy(
            string pwszInputFile,
            string pwszOutputFile,
            long qwMaxDuration,
            bool fMoveScriptStream)
        {
            //
            // Initialize the pointers
            //
            m_hEvent = null;
            m_pReader = null;
            m_pReaderAdvanced = null;
            m_pReaderHeaderInfo = null;
            m_pReaderProfile = null;
            m_pWriter = null;
            m_pWriterAdvanced = null;
            m_pWriterHeaderInfo = null;

            m_dwStreamCount = 0;
            m_pguidStreamType = null;
            m_pwStreamNumber = null;

            if (null == pwszInputFile || null == pwszOutputFile)
            {
                throw new COMException("Missing input or output file", E_InvalidArgument);
            }

            m_fMoveScriptStream = fMoveScriptStream;
            m_qwMaxDuration = qwMaxDuration;

            //
            // Event for the asynchronous calls
            //
            m_hEvent = new AutoResetEvent(false);

            //
            // Create the Reader
            //
            CreateReader(pwszInputFile);

            //
            // Get profile information
            //
            GetProfileInfo();

            //
            // Create the Writer
            //
            CreateWriter(pwszOutputFile);

            //
            // Copy all attributes
            //
            CopyAttribute();

            //
            // Copy codec info
            //
            CopyCodecInfo();

            //
            // Copy all scripts in the header
            //
            CopyScriptInHeader();

            //
            // Process samples: read from the reader, and write to the writer
            //

            try
            {
                Process();
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                //
                // Output some common error messages.
                //
                if (NSResults.E_VIDEO_CODEC_NOT_INSTALLED == hr)
                {
                    Console.WriteLine("Processing samples failed: Video codec not installed");
                }
                if (NSResults.E_AUDIO_CODEC_NOT_INSTALLED == hr)
                {
                    Console.WriteLine("Processing samples failed: Audio codec not installed");
                }
                else if (NSResults.E_INVALID_OUTPUT_FORMAT == hr)
                {
                    Console.WriteLine("Processing samples failed: Invalid output format ");
                }
                else if (NSResults.E_VIDEO_CODEC_ERROR == hr)
                {
                    Console.WriteLine("Processing samples failed: An unexpected error occurred with the video codec ");
                }
                else if (NSResults.E_AUDIO_CODEC_ERROR == hr)
                {
                    Console.WriteLine("Processing samples failed: An unexpected error occurred with the audio codec ");
                }
                else
                {
                    Console.WriteLine(string.Format("Processing samples failed: Error (hr=0x{0:x})", hr));
                }
                throw;
            }

            //
            // Copy all scripts in m_ScriptList
            //
            CopyScriptInList(pwszOutputFile);

            //
            // Copy marker information
            //
            CopyMarker(pwszOutputFile);

            Console.WriteLine("Copy finished.");

            //
            // Note: The output file is indexed automatically.
            // You can use IWMWriterFileSink3::SetAutoIndexing(false) to disable
            // auto indexing.
            //

            Dispose();
        }

        public void Dispose()
        {
            //SafeRelease(m_pReaderProfile);
            //SafeRelease(m_pReaderHeaderInfo);
            //SafeRelease(m_pReaderAdvanced);
            SafeRelease(m_pReader);
            //SafeRelease(m_pWriterHeaderInfo);
            //SafeRelease(m_pWriterAdvanced);
            SafeRelease(m_pWriter);

            m_pguidStreamType = null;
            m_pwStreamNumber = null;

            if (m_ScriptList != null)
            {
                m_ScriptList.Clear();
                m_ScriptList = null;
            }

            if (m_hEvent != null)
            {
                m_hEvent.Close();
                m_hEvent = null;
            }
        }

        #region Protected members

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CreateReader()
        // Desc: Creates a reader and opens the source file using this reader.
        //------------------------------------------------------------------------------
        protected void CreateReader(string pwszInputFile)
        {
            Console.WriteLine("Creating the Reader...");

            //
            // Create a reader
            //
            WMUtils.WMCreateReader(IntPtr.Zero, 0, out m_pReader);

            //
            // Get the IWMReaderAdvanced interface
            //
            m_pReaderAdvanced = m_pReader as IWMReaderAdvanced;

            //
            // Get the IWMHeaderInfo interface of the reader
            //
            m_pReaderHeaderInfo = m_pReader as IWMHeaderInfo;

            //
            // Open the reader; use "this" as the callback interface.
            //
            m_pReader.Open(pwszInputFile, this, IntPtr.Zero);

            //
            // Wait until WMT_OPENED status message is received in OnStatus()
            //
            WaitForCompletion();

            //
            // Get the duration of the source file
            //
            short wStreamNumber = 0;
            AttrDataType enumType;
            short cbLength = 8; // sizeof(m_qwDuration);
            byte[] b = new byte[cbLength];

            m_pReaderHeaderInfo.GetAttributeByName(ref wStreamNumber,
                Constants.g_wszWMDuration,
                out enumType,
                b,
                ref cbLength);

            m_qwDuration = BitConverter.ToInt64(b, 0);

            if (m_qwDuration == 0)
            {
                throw new COMException("Duration is zero", E_InvalidArgument);
            }

            //
            // Turn on the user clock
            //
            m_pReaderAdvanced.SetUserProvidedClock(true);

            //
            // Turn on manual stream selection, so we get all streams.
            //
            m_pReaderAdvanced.SetManualStreamSelection(true);
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::GetProfileInfo()
        // Desc: Gets the profile information from the reader.
        //------------------------------------------------------------------------------
        protected void GetProfileInfo()
        {
            IWMStreamConfig pStreamConfig = null;
            IWMMediaProps pMediaProperty = null;

            //
            // Get the profile of the reader
            //
            m_pReaderProfile = m_pReader as IWMProfile;

            //
            // Get stream count
            //
            m_pReaderProfile.GetStreamCount(out m_dwStreamCount);

            //
            // Allocate memory for the stream type array and stream number array
            //
            m_pguidStreamType = new Guid[m_dwStreamCount];
            m_pwStreamNumber = new short[m_dwStreamCount];

            for (int i = 0; i < m_dwStreamCount; i++)
            {
                m_pReaderProfile.GetStream(i, out pStreamConfig);

                try
                {
                    //
                    // Get the stream number of the current stream
                    //
                    pStreamConfig.GetStreamNumber(out m_pwStreamNumber[i]);

                    //
                    // Set the stream to be received in compressed mode
                    //
                    m_pReaderAdvanced.SetReceiveStreamSamples(m_pwStreamNumber[i], true);

                    pMediaProperty = pStreamConfig as IWMMediaProps;

                    //
                    // Get the stream type of the current stream
                    //
                    pMediaProperty.GetType(out m_pguidStreamType[i]);
                }
                finally
                {
                    Marshal.ReleaseComObject(pStreamConfig);
                }
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CreateWriter()
        // Desc: Creates a writer and sets the profile and output of this writer.
        //------------------------------------------------------------------------------
        protected void CreateWriter(string pwszOutputFile)
        {
            int dwInputCount = 0;

            Console.WriteLine("Creating the Writer...");

            //
            // Create a writer
            //
            WMUtils.WMCreateWriter(IntPtr.Zero, out m_pWriter);

            //
            // Get the IWMWriterAdvanced interface of the writer
            //
            m_pWriterAdvanced = m_pWriter as IWMWriterAdvanced;

            //
            // Get the IWMHeaderInfo interface of the writer
            //
            m_pWriterHeaderInfo = m_pWriter as IWMHeaderInfo;

            //
            // Set the profile of the writer to the reader's profile
            //
            m_pWriter.SetProfile(m_pReaderProfile);

            m_pWriter.GetInputCount(out dwInputCount);

            //
            // Set the input property to null so the SDK knows we're going to
            // send compressed samples to the inputs of the writer.
            //
            for (int i = 0; i < dwInputCount; i++)
            {
                m_pWriter.SetInputProps(i, null);
            }

            //
            // Set the output file of the writer
            //
            m_pWriter.SetOutputFilename(pwszOutputFile);
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CopyAttribute()
        // Desc: Copies all the header attributes from the reader file to the writer.
        //------------------------------------------------------------------------------
        protected void CopyAttribute()
        {
            short wStreamNumber;
            short wAttributeCount;
            AttrDataType enumType;
            short cbNameLength = 0;
            short cbValueLength = 0;
            StringBuilder pwszName = null;
            byte[] pbValue = null;

            for (int i = 0; i <= m_dwStreamCount; i++)
            {
                if (i == m_dwStreamCount)
                {
                    //
                    // When the stream number is 0, the attribute is a file-level attribute
                    //
                    wStreamNumber = 0;
                }
                else
                {
                    wStreamNumber = m_pwStreamNumber[i];
                }

                //
                // Get the attribute count
                //
                m_pReaderHeaderInfo.GetAttributeCount(wStreamNumber,
                    out wAttributeCount);
                //
                // Copy all attributes of this stream
                //
                for (short j = 0; j < wAttributeCount; j++)
                {
                    //
                    // Get the attribute name and value length
                    //
                    m_pReaderHeaderInfo.GetAttributeByIndex(j,
                        ref wStreamNumber,
                        null,
                        ref cbNameLength,
                        out enumType,
                        null,
                        ref cbValueLength);
                    pwszName = new StringBuilder(cbNameLength);
                    pbValue = new byte[cbValueLength];

                    //
                    // Get the attribute name and value
                    //
                    m_pReaderHeaderInfo.GetAttributeByIndex(j,
                        ref wStreamNumber,
                        pwszName,
                        ref cbNameLength,
                        out enumType,
                        pbValue,
                        ref cbValueLength);
                    //
                    // Set the attribute for the writer
                    //
                    try
                    {
                        m_pWriterHeaderInfo.SetAttribute(wStreamNumber,
                            pwszName.ToString(),
                            enumType,
                            pbValue,
                            cbValueLength);
                    }
                    catch (Exception e)
                    {
                        int hr = Marshal.GetHRForException(e);
                        if (E_InvalidArgument == hr)
                        {
                            //
                            // Some attributes are read-only; we cannot set them.
                            // They'll be set automatically by the writer.
                            //
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy:CopyCodecInfo()
        // Desc: Copies codec information from the reader header to the writer header.
        //------------------------------------------------------------------------------
        protected void CopyCodecInfo()
        {
            int cCodecInfo;
            StringBuilder pwszName = null;
            StringBuilder pwszDescription = null;
            byte[] pbCodecInfo = null;
            IWMHeaderInfo3 pReaderHeaderInfo3 = null;
            IWMHeaderInfo3 pWriterHeaderInfo3 = null;

            pReaderHeaderInfo3 = m_pReaderHeaderInfo as IWMHeaderInfo3;

            pWriterHeaderInfo3 = m_pWriterHeaderInfo as IWMHeaderInfo3;

            pReaderHeaderInfo3.GetCodecInfoCount(out cCodecInfo);

            for (int i = 0; i < cCodecInfo; i++)
            {
                CodecInfoType enumCodecType;
                short cchName = 0;
                short cchDescription = 0;
                short cbCodecInfo = 0;

                //
                // Get codec info from the source
                //
                pReaderHeaderInfo3.GetCodecInfo(i, ref cchName, null,
                    ref cchDescription, null, out enumCodecType,
                    ref cbCodecInfo, null);

                pwszName = new StringBuilder(cchName);
                pwszDescription = new StringBuilder(cchDescription);
                pbCodecInfo = new byte[cbCodecInfo];

                pReaderHeaderInfo3.GetCodecInfo(i, ref cchName, pwszName,
                    ref cchDescription, pwszDescription, out enumCodecType,
                    ref cbCodecInfo, pbCodecInfo);

                //
                // Add the codec info to the writer
                //
                pWriterHeaderInfo3.AddCodecInfo(pwszName.ToString(),
                    pwszDescription.ToString(),
                    enumCodecType,
                    cbCodecInfo, pbCodecInfo);
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CopyScriptInHeader()
        // Desc: Copies the script in the header of the source file to the header
        //       of the destination file .
        //------------------------------------------------------------------------------
        protected void CopyScriptInHeader()
        {
            short cScript = 0;
            StringBuilder pwszType = null;
            StringBuilder pwszCommand = null;
            long cnsScriptTime = 0;

            m_pReaderHeaderInfo.GetScriptCount(out cScript);

            for (short i = 0; i < cScript; i++)
            {
                short cchTypeLen = 0;
                short cchCommandLen = 0;

                //
                // Get the memory size for this script
                //
                m_pReaderHeaderInfo.GetScript(i,
                    null,
                    ref cchTypeLen,
                    null,
                    ref cchCommandLen,
                    out cnsScriptTime);

                pwszType = new StringBuilder(cchTypeLen);
                pwszCommand = new StringBuilder(cchCommandLen);

                //
                // Get the script
                //
                m_pReaderHeaderInfo.GetScript(i,
                    pwszType,
                    ref cchTypeLen,
                    pwszCommand,
                    ref cchCommandLen,
                    out cnsScriptTime);

                //
                // Add the script to the writer
                //
                m_pWriterHeaderInfo.AddScript(pwszType.ToString(),
                    pwszCommand.ToString(),
                    cnsScriptTime);
            }

        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::Process()
        // Desc: Processes the samples from the reader..
        //------------------------------------------------------------------------------
        protected void Process()
        {
            //
            // Begin writing
            //
            m_pWriter.BeginWriting();

            m_hr = S_Ok;
            m_fEOF = false;
            m_dwProgress = 0;
            m_hEvent.Reset();

            Console.WriteLine("            0%-------20%-------40%-------60%-------80%-------100%");
            Console.Write("Process:    ");

            //
            // Start the reader
            //
            m_pReader.Start(0, 0, 1.0f, IntPtr.Zero);

            //
            // Wait until the reading is finished
            //
            WaitForCompletion();
            Console.WriteLine("");

            //
            // End writing
            //
            m_pWriter.EndWriting();
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CopyMarker()
        // Desc: Copies the markers from the source file to the destination file.
        //------------------------------------------------------------------------------
        protected void CopyMarker(string pwszOutputFile)
        {
            short cMarker = 0;
            IWMMetadataEditor pEditor = null;
            IWMHeaderInfo pWriterHeaderInfo = null;
            StringBuilder pwszMarkerName = null;

            m_pReaderHeaderInfo.GetMarkerCount(out cMarker);

            //
            // Markers can be copied only by the metadata editor.
            // Create an editor
            //
            WMUtils.WMCreateEditor(out pEditor);

            try
            {
                //
                // Open the output using the editor
                //
                pEditor.Open(pwszOutputFile);

                pWriterHeaderInfo = pEditor as IWMHeaderInfo;

                for (short i = 0; i < cMarker; i++)
                {
                    short cchMarkerNameLen = 0;
                    long cnsMarkerTime = 0;

                    //
                    // Get the memory size for this marker
                    //
                    m_pReaderHeaderInfo.GetMarker(i,
                        null,
                        ref cchMarkerNameLen,
                        out cnsMarkerTime);

                    pwszMarkerName = new StringBuilder(cchMarkerNameLen);

                    m_pReaderHeaderInfo.GetMarker(i,
                        pwszMarkerName,
                        ref cchMarkerNameLen,
                        out cnsMarkerTime);
                    //
                    // Add marker to the writer
                    //
                    pWriterHeaderInfo.AddMarker(pwszMarkerName.ToString(),
                        cnsMarkerTime);
                }

                //
                // Close and release the editor
                //
                pEditor.Flush();

                pEditor.Close();
            }
            finally
            {
                Marshal.ReleaseComObject(pEditor);
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::CopyScriptInList()
        // Desc: Copies scripts in m_ScriptList to the header of the writer.
        //------------------------------------------------------------------------------
        protected void CopyScriptInList(string pwszOutputFile)
        {
            IWMMetadataEditor pEditor = null;
            IWMHeaderInfo pWriterHeaderInfo = null;

            //
            // Scripts can be added by the metadata editor.
            // Create an editor
            //
            WMUtils.WMCreateEditor(out pEditor);

            try
            {
                //
                // Open the output using the editor
                //
                pEditor.Open(pwszOutputFile);

                pWriterHeaderInfo = pEditor as IWMHeaderInfo;

                foreach(CScript pScript in m_ScriptList)
                {
                    //
                    // Add the script to the writer
                    //
                    pWriterHeaderInfo.AddScript(pScript.m_pwszType,
                        pScript.m_pwszParameter,
                        pScript.m_cnsTime);
                }

                //
                // Close and release the editor
                //
                pEditor.Flush();

                pEditor.Close();
            }
            finally
            {
                Marshal.ReleaseComObject(pEditor);
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::WaitForCompletion()
        // Desc: Waits until the event is signaled.
        //------------------------------------------------------------------------------
        protected void WaitForCompletion()
        {
            m_hEvent.WaitOne();
        }

        #endregion

        #region Callbacks

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::OnSample()
        // Desc: Implementation of IWMReaderCallback::OnSample.
        //------------------------------------------------------------------------------
        public void OnSample(
            int dwOutputNum,
            long qwSampleTime,
            long qwSampleDuration,
            SampleFlag dwFlags,
            INSSBuffer pSample,
            IntPtr pvContext)
        {
            //
            // The samples are expected in OnStreamSample
            //
            m_hr = E_Unexpected;
            Console.WriteLine(string.Format("Reader Callback: Received a decompressed sample (hr=0x{0:x}).", m_hr));
            m_hEvent.Set();
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::OnStatus()
        // Desc: Implementation of IWMStatusCallback::OnStatus.
        //------------------------------------------------------------------------------
        public void OnStatus(
            Status Status,
            int hr,
            AttrDataType dwType,
            IntPtr pValue,
            IntPtr pvContext)
        {
            //
            // If an error code already exists, just set the event and return.
            //
            if (m_hr < 0)
            {
                m_hEvent.Set();
                return;
            }

            //
            // If an error occurred in the reader, save this error code and set the event.
            //
            if (hr < 0)
            {
                m_hr = hr;
                m_hEvent.Set();
            }

            switch (Status)
            {
                case Status.Opened:
                    Console.WriteLine("Reader Callback: File is opened.");
                    m_hr = S_Ok;
                    m_hEvent.Set();

                    break;

                case Status.Started:
                    //
                    // Ask for 1 second of the stream to be delivered
                    //
                    m_qwReaderTime = 10000000;

                    try
                    {
                        m_pReaderAdvanced.DeliverTime(m_qwReaderTime);
                    }
                    catch (Exception e)
                    {
                        int lhr = Marshal.GetHRForException(e);
                        m_hr = hr;
                        m_hEvent.Set();
                    }

                    break;

                case Status.EOF:
                    m_hr = S_Ok;
                    m_fEOF = true;
                    m_hEvent.Set();

                    break;
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::OnStreamSample()
        // Desc: Implementation of IWMReaderCallbackAdvanced::OnStreamSample.
        //------------------------------------------------------------------------------
        public void OnStreamSample(
            short wStreamNum,
            long cnsSampleTime,
            long cnsSampleDuration,
            SampleFlag dwFlags,
            INSSBuffer pSample,
            IntPtr pvContext)
        {
            bool fMoveScript = false;

            while (m_dwProgress <= cnsSampleTime * 50 / m_qwDuration)
            {
                m_dwProgress++;
                Console.Write("*");
            }

            if (m_qwMaxDuration > 0 && cnsSampleTime > m_qwMaxDuration)
            {
                m_hEvent.Set();
                return;
            }

            if (m_fMoveScriptStream)
            {
                //
                // We may have multiple script streams in this file.
                //
                for (int i = 0; i < m_dwStreamCount; i++)
                {
                    if (m_pwStreamNumber[i] == wStreamNum)
                    {
                        if (MediaType.ScriptCommand == m_pguidStreamType[i])
                        {
                            fMoveScript = true;
                        }

                        break;
                    }
                }
            }

            try
            {
                if (fMoveScript)
                {
                    int dwBufferLength;
                    int nStringLength;
                    IntPtr pwszTypeIP = IntPtr.Zero;
                    StringBuilder pwszType = null;
                    StringBuilder pwszCommand = null;

                    //
                    // Read the buffer and length of the script stream sample
                    //
                    pSample.GetBufferAndLength(out pwszTypeIP, out dwBufferLength);
                    pwszType = new StringBuilder(Marshal.PtrToStringUni(pwszTypeIP)); //todo - this won't work if the string doesn't have a final \0

                    nStringLength = dwBufferLength / 2;
                    if (nStringLength > 0)
                    {

                        //
                        // Get the command string of this script
                        //

                        // Todo - maybe the format of pwszTypeIP is Type\0Command\0 ???

#if false
                        pwszCommand = wcschr(pwszType, null);
                        if (pwszCommand - pwszType < nStringLength - 1)
                        {
                            pwszCommand++;
                        }
                        else
                        {
                            pwszCommand = " ";
                        }
#else
                        pwszCommand = new StringBuilder(" ");
#endif

                        //
                        // Add the script to the script list. We cannot write scripts
                        // directly to the writer after writing has begun.
                        //
                        m_ScriptList.Add(new CScript(pwszType.ToString(), pwszCommand.ToString(), cnsSampleTime));
                    }
                }
                else
                {
                    m_pWriterAdvanced.WriteStreamSample(wStreamNum,
                        cnsSampleTime,
                        0,
                        cnsSampleDuration,
                        dwFlags,
                        pSample);
                }
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                m_hr = hr;
                m_hEvent.Set();
            }
            finally
            {
                Marshal.ReleaseComObject(pSample);
            }
        }

        //------------------------------------------------------------------------------
        // Name: CWMVCopy::OnTime()
        // Desc: Implementation of IWMReaderCallbackAdvanced::OnTime.
        //------------------------------------------------------------------------------
        public void OnTime(
            long qwCurrentTime,
            IntPtr pvContext)
        {
            //
            // Keep asking for 1 second of the stream till EOF
            //
            if (!m_fEOF)
            {
                m_qwReaderTime += 10000000;

                try
                {
                    m_pReaderAdvanced.DeliverTime(m_qwReaderTime);
                }
                catch (Exception e)
                {
                    int hr = Marshal.GetHRForException(e);
                    //
                    // If an error occurred in the reader, save this error code and set the event.
                    //
                    m_hr = hr;
                    m_hEvent.Set();
                }
            }
        }

        //------------------------------------------------------------------------------
        // Implementation of other IWMReaderCallbackAdvanced methods.
        //------------------------------------------------------------------------------
        public void OnStreamSelection(
            short wStreamCount,
            short[] pStreamNumbers,
            StreamSelection[] pSelections,
            IntPtr pvContext)
        {
        }

        public void OnOutputPropsChanged(
            int dwOutputNum,
            AMMediaType pMediaType,
            IntPtr pvContext)
        {
        }

        public void AllocateForOutput(
            int dwOutputNum,
            int cbBuffer,
            out INSSBuffer ppBuffer,
            IntPtr pvContext)
        {
            throw new COMException("AllocateForOutput not implemented", E_NotImplemented);
        }

        public void AllocateForStream(
            short wStreamNum,
            int cbBuffer,
            out INSSBuffer ppBuffer,
            IntPtr pvContext)
        {
            throw new COMException("AllocateForStream not implemented", E_NotImplemented);
        }

        #endregion
    }
}
