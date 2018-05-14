/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

From http://windowsmedianet.sourceforge.net
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using WindowsMediaLib;
using System.Threading;
using System.Runtime.InteropServices;

namespace ReadFromStream
{
    class CReader : COMBase, IWMReaderCallback, IWMReaderCallbackAdvanced
    {
        protected CROStream m_pStream;
        protected AutoResetEvent m_hEvent;
        protected int m_hrAsync;
        protected IWMReader m_pReader;
        protected IWMReaderAdvanced2 m_pReader2;
        protected bool m_EOF;
        protected int []m_SampleCount;

        //------------------------------------------------------------------------------
        // Name: CReader()
        // Desc: Constructor.
        //------------------------------------------------------------------------------
        public CReader()
        {
            m_pStream = null;
            m_hrAsync = 0;

            m_hEvent = new AutoResetEvent(false);

            //
            // Create reader object
            //

            WMUtils.WMCreateReader(IntPtr.Zero, Rights.Playback, out m_pReader);

            //
            // QI for IWMReaderAdvanced2 
            //
            m_pReader2 = (IWMReaderAdvanced2)m_pReader;

            m_pStream = new CROStream();
        }

        //------------------------------------------------------------------------------
        // Name: ~CReader()
        // Desc: Destructor.
        //------------------------------------------------------------------------------
        ~CReader()
        {

            if (m_pReader != null)
            {
                try
                {
                    m_pReader.Close();
                }
                catch { }
                Marshal.ReleaseComObject(m_pReader);
                m_pReader = null;
            }

            if (m_pStream != null)
            {
                m_pStream.Close();
                m_pStream = null;
            }

            if (null != m_hEvent)
            {
                m_hEvent.Close();
                m_hEvent = null;
            }
        }

        //------------------------------------------------------------------------------
        // Name: Open()
        // Desc: Creates the reader and opens the file.
        //------------------------------------------------------------------------------
        public void Open(string ptszFile)
        {
            Close();

            m_hrAsync = 0;

            //
            // Open input file
            //
            m_pStream.Open(ptszFile);

            //
            // initial state is "not set".
            //
            m_hEvent.Reset();

            //
            // Open the stream
            //
            m_pReader2.OpenStream(m_pStream, this, IntPtr.Zero);

            //
            // Wait for the open to finish
            //
            m_hEvent.WaitOne(-1, false);

            int i;
            m_pReader.GetOutputCount(out i);
            m_SampleCount = new int[i];

            // Turn on the user clock.  This will send samples to IWMReaderCallback::OnSample as fast
            // as it can.  If you comment this line out, samples will be sent at the rate specified
            // in the file (ie a 47 second clip will take 47 seconds to process).
            m_pReader2.SetUserProvidedClock(true);

            //
            // Check open operation result
            //
            if (m_hrAsync < 0)
            {
                throw new COMException("Open failed", m_hrAsync);
            }
        }

        //------------------------------------------------------------------------------
        // Name: Start()
        // Desc: Plays the file.
        //------------------------------------------------------------------------------
        public void Start()
        {
            //
            // Start the reader to play the stream normally
            //
            m_hrAsync = 0;
            m_EOF = false;

            m_pReader.Start(0, 0, 1.0f, IntPtr.Zero);

            // Wait for the start to complete
            m_hEvent.WaitOne(-1, false);
            Debug.WriteLine("Playing......");

            //
            // Check results
            //
            if (m_hrAsync < 0)
            {
                throw new COMException("IWMReader playback failed", m_hrAsync);
            }
        }

        // Have we received the End of File status message?
        public bool EOF()
        {
            return m_EOF;
        }

        //------------------------------------------------------------------------------
        // Name: Close()
        // Desc: Closes the reader.
        //------------------------------------------------------------------------------
        public void Close()
        {
            try
            {
                m_pReader.Close();
                m_hEvent.WaitOne(-1, false);
            }
            catch { }

            m_pStream.Close();
        }

        #region IWMReaderCallback Members

        //------------------------------------------------------------------------------
        // Name: OnSample()
        // Desc: Implementation of IWMReaderCallback::OnSample.
        //------------------------------------------------------------------------------
        public void OnSample(int dwOutputNum, long cnsSampleTime, long cnsSampleDuration, SampleFlag dwFlags, INSSBuffer pSample, IntPtr pvContext)
        {
            //
            // Add your code to process samples here
            //
            m_SampleCount[dwOutputNum]++;
        }

        public void OnStatus(Status iStatus, int hr, AttrDataType dwType, IntPtr pValue, IntPtr pvContext)
        {
            Debug.WriteLine(string.Format("Status: {0}", iStatus));

            switch (iStatus)
            {
                case Status.Opened:
                    m_hrAsync = hr;
                    m_hEvent.Set();
                    Debug.WriteLine("Opened the file ...");
                    break;

                case Status.Started:
                    m_hrAsync = hr;
                    m_hEvent.Set();

                    if (hr >= 0)
                    {
                        Debug.WriteLine("Started ...");

                        try
                        {
                            // Ask for all data
                            m_pReader2.DeliverTime(-1);
                        }
                        catch (Exception e)
                        {
                            int lhr = Marshal.GetHRForException(e);
                            m_hrAsync = hr;
                            m_hEvent.Set();
                        }
                    }
                    break;

                case Status.Stopped:
                    m_hrAsync = hr;
                    m_hEvent.Set();
                    Debug.WriteLine("Stopped ...");
                    break;

                case Status.Closed:
                    m_hrAsync = hr;
                    m_hEvent.Set();
                    Debug.WriteLine("Closed the file ...");
                    break;

                case Status.EOF:
                    m_EOF = true;
                    m_hrAsync = hr;
                    m_hEvent.Set();
                    Debug.WriteLine("End of file ...");
                    for (int x = 0; x < m_SampleCount.Length; x++)
                    {
                        Debug.WriteLine(string.Format("{0}: {1}", x, m_SampleCount[x]));
                    }
                    break;
            }
        }

        #endregion


        #region IWMReaderCallbackAdvanced Members

        public void AllocateForOutput(int dwOutputNum, int cbBuffer, out INSSBuffer ppBuffer, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AllocateForStream(short wStreamNum, int cbBuffer, out INSSBuffer ppBuffer, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnOutputPropsChanged(int dwOutputNum, WindowsMediaLib.Defs.AMMediaType pMediaType, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnStreamSample(short wStreamNum, long cnsSampleTime, long cnsSampleDuration, SampleFlag dwFlags, INSSBuffer pSample, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnStreamSelection(short wStreamCount, short[] pStreamNumbers, StreamSelection[] pSelections, IntPtr pvContext)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void OnTime(long cnsCurrentTime, IntPtr pvContext)
        {
            if (!m_EOF)
            {
                // We should never get here.  We requested all data in OnStatus().
            }
        }

        #endregion
    }
}
