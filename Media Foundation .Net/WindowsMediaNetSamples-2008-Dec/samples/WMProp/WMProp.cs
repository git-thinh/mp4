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
using System.Threading;

using WindowsMediaLib;
using WindowsMediaLib.Defs;
using System.Runtime.InteropServices;

namespace WMProp
{
    class CWMProp : IWMReaderCallback
    {
        private AutoResetEvent m_hOpenEvent;
        private AutoResetEvent m_hCloseEvent;
        private bool m_bIsDRM;

        public CWMProp()
        {
            m_hOpenEvent = new AutoResetEvent(false);
            m_hCloseEvent = new AutoResetEvent(false);

            m_bIsDRM = false;
        }
        // Safe_Release
        ///////////////////////////////////////////////////////////////////////////////
        ~CWMProp()
        {
            m_hOpenEvent.Close();
            m_hCloseEvent.Close();
        }

        // Use IWMMetadataEditor interface to access 
        // Protected, Seekable, and Stridable attributes
        ///////////////////////////////////////////////////////////////////////////////
        public void Report(string pwszFileName)
        {
            OpenFileWithEditor(pwszFileName);

            if (!m_bIsDRM)
            {
                OpenFileWithReader(pwszFileName);
            }
            else
            {
                Console.WriteLine("These are the properties available from this app for a DRM file");
            }
        }
        // Use IWMMetadataEditor anf IWMHeaderInfo interfaces to access
        // Protected, Seekable, and Stridable attributes
        ///////////////////////////////////////////////////////////////////////////////
        void OpenFileWithEditor(string pwszFileName)
        {
            bool fProp = false;
            IWMHeaderInfo pHeaderInfo;
            IWMMetadataEditor pIWMEditor;

            WMUtils.WMCreateEditor(out pIWMEditor);

            try
            {
                pIWMEditor.Open(pwszFileName);

                try
                {
                    Console.WriteLine(string.Format("Properties for {0} are:", pwszFileName));

                    pHeaderInfo = (IWMHeaderInfo)pIWMEditor;

                    //
                    //	Check whether file is DRM
                    //

                    GetBoolAttribsFromEditor(pHeaderInfo, Constants.g_wszWMProtected, out m_bIsDRM);
                    if (m_bIsDRM)
                    {
                        Console.WriteLine("Is Protected (DRM): true");
                    }
                    else
                    {
                        Console.WriteLine("Is Protected (DRM): false");
                    }

                    //
                    // Get seekable attribute from content and return true/false
                    //

                    fProp = false;
                    GetBoolAttribsFromEditor(pHeaderInfo, Constants.g_wszWMSeekable, out fProp);
                    if (fProp)
                    {
                        Console.WriteLine("Is Seekable: true");
                    }
                    else
                    {
                        Console.WriteLine("Is Seekable: false");
                    }

                    //
                    // Get stridable attribute from content and return true/false
                    //

                    fProp = false;
                    GetBoolAttribsFromEditor(pHeaderInfo, Constants.g_wszWMStridable, out fProp);
                    if (fProp)
                    {
                        Console.WriteLine("Is Stridable: true");
                    }
                    else
                    {
                        Console.WriteLine("Is Stridable: false");
                    }
                    Console.WriteLine();
                }
                finally
                {
                    pIWMEditor.Close();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(pIWMEditor);
            }
        }
        // Use Reader interface to access profile properties
        ///////////////////////////////////////////////////////////////////////////////
        void OpenFileWithReader(string pwszFileName)
        {
            IWMReader pReader;

            WMUtils.WMCreateReader(IntPtr.Zero, 0, out pReader);

            try
            {
                pReader.Open(pwszFileName, this, IntPtr.Zero);
                m_hOpenEvent.WaitOne();

                try
                {
                    GetPropertiesFromProfile((IWMProfile)pReader);
                }
                finally
                {
                    pReader.Close();
                    m_hCloseEvent.WaitOne();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(pReader);
            }
        }
        // Use IWMMetadataEditor interface to access 
        // StreamNumber attribute
        ///////////////////////////////////////////////////////////////////////////////
        void GetBoolAttribsFromEditor(IWMHeaderInfo pHeaderInfo, string pwszName, out bool pResult)
        {
            AttrDataType wmType;
            short wStreamNum = 0;
            short cbLen = 0;

            pHeaderInfo.GetAttributeByName(ref wStreamNum, pwszName, out wmType, null, ref cbLen);

            byte[] pData = new byte[cbLen];
            pHeaderInfo.GetAttributeByName(ref wStreamNum, pwszName, out wmType, pData, ref cbLen);

            pResult = BitConverter.ToBoolean(pData, 0);
        }
        // Use IWMStreamConfig interface to access
        // number of streams, each stream number, and bitrate
        ///////////////////////////////////////////////////////////////////////////////
        void GetPropertiesFromProfile(IWMProfile pProfile)
        {
            int dwStreamCount = 0;
            pProfile.GetStreamCount(out dwStreamCount);

            Console.WriteLine(string.Format("This Windows Media file has {0} stream(s)", dwStreamCount));

            for (int dwIndex = 0; dwIndex < dwStreamCount; dwIndex++)
            {
                IWMStreamConfig pConfig = null;
                pProfile.GetStream(dwIndex, out pConfig);

                try
                {
                    Guid guid;
                    pConfig.GetStreamType(out guid);
                    if (MediaType.Video == guid)
                    {
                        short wStreamNum = -1;
                        int dwBitrate = -1;

                        try
                        {
                            pConfig.GetStreamNumber(out wStreamNum);
                        }
                        catch { }
                        try
                        {
                            pConfig.GetBitrate(out dwBitrate);
                        }
                        catch { }

                        Console.WriteLine("Video Stream properties:");
                        Console.WriteLine(string.Format("Stream number: {0}", wStreamNum));
                        Console.WriteLine(string.Format("Bitrate: {0} bps", dwBitrate));

                        PrintCodecName(pConfig);
                    }
                    else if (MediaType.Audio == guid)
                    {
                        short wStreamNum = -1;
                        int dwBitrate = -1;

                        try
                        {
                            pConfig.GetStreamNumber(out wStreamNum);
                        }
                        catch { }
                        try
                        {
                            pConfig.GetBitrate(out dwBitrate);
                        }
                        catch { }

                        Console.WriteLine("Audio Stream properties:");
                        Console.WriteLine(string.Format("Stream number: {0}", wStreamNum));
                        Console.WriteLine(string.Format("Bitrate: {0} bps", dwBitrate));

                        PrintCodecName(pConfig);
                    }
                    else if (MediaType.ScriptCommand == guid)
                    {
                        short wStreamNum = -1;
                        int dwBitrate = -1;

                        try
                        {
                            pConfig.GetStreamNumber(out wStreamNum);
                        }
                        catch { }
                        try
                        {
                            pConfig.GetBitrate(out dwBitrate);
                        }
                        catch { }

                        Console.WriteLine("Script Stream properties:");
                        Console.WriteLine(string.Format("Stream number: {0}", wStreamNum));
                        Console.WriteLine(string.Format("Bitrate: {0} bps\n", dwBitrate));
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(pConfig);
                }
            }
        }
        // Use IWMStreamConfig interface to access codec names
        ///////////////////////////////////////////////////////////////////////////////
        void PrintCodecName(IWMStreamConfig pConfig)
        {
            IWMMediaProps pMediaProps = null;
            pMediaProps = (IWMMediaProps)pConfig;

            int cbType = 0;
            pMediaProps.GetMediaType(null, ref cbType);

            AMMediaType mt = new AMMediaType();
            mt.formatSize = cbType;

            pMediaProps.GetMediaType(mt, ref cbType);

            try
            {
                //
                // Audio Codec Names
                //

                if (mt.subType == MediaSubType.WMAudioV9)
                {
                    Console.WriteLine("Codec Name: Windows Media Audio V9");
                }
                else if (mt.subType == MediaSubType.WMAudio_Lossless)
                {
                    Console.WriteLine("Codec Name: Windows Media Audio V9 (Lossless Mode)");
                }
                else if (mt.subType == MediaSubType.WMAudioV7)
                {
                    Console.WriteLine("Codec Name: Windows Media Audio V7/V8");
                }
                else if (mt.subType == MediaSubType.WMSP1)
                {
                    Console.WriteLine("Codec Name: Windows Media Speech Codec V9");
                }
                else if (mt.subType == MediaSubType.WMAudioV2)
                {
                    Console.WriteLine("Codec Name: Windows Media Audio V2");
                }
                else if (mt.subType == MediaSubType.ACELPnet)
                {
                    Console.WriteLine("Codec Name: ACELP.net");
                }

                // Video Codec Names
                //

                else if (mt.subType == MediaSubType.WMV1)
                {
                    Console.WriteLine("Codec Name: Windows Media Video V7");
                }
                else if (mt.subType == MediaSubType.MSS1)
                {
                    Console.WriteLine("Codec Name: Windows Media Screen V7");
                }
                else if (mt.subType == MediaSubType.MSS2)
                {
                    Console.WriteLine("Codec Name: Windows Media Screen V9");
                }
                else if (mt.subType == MediaSubType.WMV2)
                {
                    Console.WriteLine("Codec Name: Windows Media Video V8");
                }
                else if (mt.subType == MediaSubType.WMV3)
                {
                    Console.WriteLine("Codec Name: Windows Media Video V9");
                }
                else if (mt.subType == MediaSubType.MP43)
                {
                    Console.WriteLine("Codec Name: Microsoft MPEG-4 Video Codec V3 ");
                }
                else if (mt.subType == MediaSubType.MP4S)
                {
                    Console.WriteLine("Codec Name: ISO MPEG-4 Video V1");
                }
                else
                {
                    Console.WriteLine("Codec Name: {0}", AMToString.MediaSubTypeToString(mt.subType));
                }
                Console.WriteLine();
            }
            finally
            {
                WMUtils.FreeWMMediaType(mt);
            }
        }

        #region IWMReaderCallback Members

        public void OnSample(int dwOutputNum, long cnsSampleTime, long cnsSampleDuration, SampleFlag dwFlags, INSSBuffer pSample, IntPtr pvContext)
        {
        }

        public void OnStatus(Status iStatus, int hr, AttrDataType dwType, IntPtr pValue, IntPtr pvContext)
        {
            switch (iStatus)
            {
                case Status.Opened:
                    {
                        m_hOpenEvent.Set();
                        break;
                    }

                case Status.Closed:
                    {
                        m_hCloseEvent.Set();
                        break;
                    }
            }
        }

        #endregion
    }
}
