/****************************************************************************
  While the underlying library is covered by LGPL or BSD, this sample is
  released as public domain.  It is distributed in the hope that it will
  be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Alt;
using System.Reflection;

// Generic IMFAttributes implementation
// for any class that needs it.
abstract public class MFAttributes : IMFAttributes
{
    #region Members

    const int DEFAULTATTRIBUTECOUNT = 10;

    protected IMFAttributes m_Attribs;

    #endregion

    protected MFAttributes()
    {
        MFError hrthrowonerror = MFExtern.MFCreateAttributes(
            out m_Attribs, DEFAULTATTRIBUTECOUNT);
    }

    #region IMFAttributes

    public HResult GetItem(Guid guidKey, PropVariant pValue)
    {
        return m_Attribs.GetItem(guidKey, pValue);
    }

    public HResult GetItemType(Guid guidKey, out MFAttributeType pType)
    {
        return m_Attribs.GetItemType(guidKey, out pType);
    }

    public HResult CompareItem(Guid guidKey,
        ConstPropVariant Value,
        out bool pbResult)
    {
        return m_Attribs.CompareItem(guidKey, Value, out pbResult);
    }

    public HResult Compare(IMFAttributes pTheirs,
        MFAttributesMatchType MatchType,
        out bool pbResult)
    {
        return m_Attribs.Compare(pTheirs, MatchType, out pbResult);
    }

    public HResult GetUINT32(Guid guidKey, out int punValue)
    {
        return m_Attribs.GetUINT32(guidKey, out punValue);
    }

    public HResult GetUINT64(Guid guidKey, out long punValue)
    {
        return m_Attribs.GetUINT64(guidKey, out punValue);
    }

    public HResult GetDouble(Guid guidKey, out double pfValue)
    {
        return m_Attribs.GetDouble(guidKey, out pfValue);
    }

    public HResult GetGUID(Guid guidKey, out Guid pguidValue)
    {
        return m_Attribs.GetGUID(guidKey, out pguidValue);
    }

    public HResult GetStringLength(Guid guidKey, out int pcchLength)
    {
        return m_Attribs.GetStringLength(guidKey, out pcchLength);
    }

    public HResult GetString(Guid guidKey,
        StringBuilder pwszValue,
        int cchBufSize,
        out int pcchLength)
    {
        return m_Attribs.GetString(guidKey,
            pwszValue,
            cchBufSize,
            out pcchLength);
    }

    public HResult GetAllocatedString(Guid guidKey,
        out string ppwszValue,
        out int pcchLength)
    {
        return m_Attribs.GetAllocatedString(guidKey,
            out ppwszValue, out pcchLength);
    }

    public HResult GetBlobSize(Guid guidKey, out int pcbBlobSize)
    {
        return m_Attribs.GetBlobSize(guidKey, out pcbBlobSize);
    }

    public HResult GetBlob(Guid guidKey,
        byte[] pBuf, int cbBufSize, out int pcbBlobSize)
    {
        return m_Attribs.GetBlob(guidKey, pBuf, cbBufSize, out pcbBlobSize);
    }

    public HResult GetAllocatedBlob(Guid guidKey,
        out IntPtr ip, out int pcbSize)
    {
        return m_Attribs.GetAllocatedBlob(guidKey, out ip, out pcbSize);
    }

    public HResult GetUnknown(Guid guidKey, Guid riid, out object ppv)
    {
        return m_Attribs.GetUnknown(guidKey, riid, out ppv);
    }

    public HResult SetItem(Guid guidKey, ConstPropVariant Value)
    {
        return m_Attribs.SetItem(guidKey, Value);
    }

    public HResult DeleteItem(Guid guidKey)
    {
        return m_Attribs.DeleteItem(guidKey);
    }

    public HResult DeleteAllItems()
    {
        return m_Attribs.DeleteAllItems();
    }

    public HResult SetUINT32(Guid guidKey, int unValue)
    {
        return m_Attribs.SetUINT32(guidKey, unValue);
    }

    public HResult SetUINT64(Guid guidKey, long unValue)
    {
        return m_Attribs.SetUINT64(guidKey, unValue);
    }

    public HResult SetDouble(Guid guidKey, double fValue)
    {
        return m_Attribs.SetDouble(guidKey, fValue);
    }

    public HResult SetGUID(Guid guidKey, Guid guidValue)
    {
        return m_Attribs.SetGUID(guidKey, guidValue);
    }

    public HResult SetString(Guid guidKey, string wszValue)
    {
        return m_Attribs.SetString(guidKey, wszValue);
    }

    public HResult SetBlob(Guid guidKey, byte[] pBuf, int cbBufSize)
    {
        return m_Attribs.SetBlob(guidKey, pBuf, cbBufSize);
    }

    public HResult SetUnknown(Guid guidKey, object pUnknown)
    {
        return m_Attribs.SetUnknown(guidKey, pUnknown);
    }

    public HResult LockStore()
    {
        return m_Attribs.LockStore();
    }

    public HResult UnlockStore()
    {
        return m_Attribs.UnlockStore();
    }

    public HResult GetCount(out int pcItems)
    {
        return m_Attribs.GetCount(out pcItems);
    }

    public HResult GetItemByIndex(int unIndex,
        out Guid pguidKey, PropVariant pValue)
    {
        return m_Attribs.GetItemByIndex(unIndex, out pguidKey, pValue);
    }

    public HResult CopyAllItems(IMFAttributes pDest)
    {
        return m_Attribs.CopyAllItems(pDest);
    }

    #endregion
}

[ComVisible(true),
 Guid("64D5ED5F-DB86-418E-84B9-BA922FDD9338"),
 ClassInterface(ClassInterfaceType.None)]
public sealed class IcySchemeActivator : MFAttributes, IMFActivateImpl
{
    #region Members

    private IMFSchemeHandler m_IcyHandler;
    private object m_Obj = new object();

    #endregion

    #region IMFActivateImpl

    // We can't just return an object.  It must be the specified interface.
    public HResult ActivateObject(Guid riid, out IntPtr ppv)
    {
        HResult hr;

        lock (m_Obj)
        {
            if (m_IcyHandler == null)
                m_IcyHandler = new IcySchemeHandler() as IMFSchemeHandler;

            // Note that if we don't support the requested riid, this is
            // essentially a noop.
            ppv = Marshal.GetIUnknownForObject(m_IcyHandler);
            hr = (HResult)Marshal.QueryInterface(ppv, ref riid, out ppv);

            //Release 1 of the 2 refs we just added.
            Marshal.Release(ppv);
        }

        return hr;
    }

    public HResult DetachObject()
    {
        lock (m_Obj)
        {
            m_IcyHandler = null;
        }

        return HResult.S_OK;
    }

    public HResult ShutdownObject()
    {
        lock (m_Obj)
        {
            if (m_IcyHandler != null)
            {
                // Shut it down
                (m_IcyHandler as IDisposable).Dispose();

                // Reset for next time
                m_IcyHandler = null;
            }
        }

        return HResult.S_OK;
    }

    #endregion
}

// SchemeHandler for IcyScheme.
// Supports cancel.
[ComVisible(true),
 Guid("C2EE4578-2B6B-4896-86AA-FECD11B35D83"),
 ClassInterface(ClassInterfaceType.None)]
public class IcySchemeHandler : IMFSchemeHandler
{
    public IcySchemeHandler()
    {
    }

    public HResult BeginCreateObject(string pwszURL,
        MFResolution dwFlags,
        IPropertyStore pProps,
        out object ppIUnknownCancelCookie,
        IMFAsyncCallback pCallback,
        object pUnkState)
    {
        // The constructor launches an async connect to the url.
        IcyScheme ice = new IcyScheme(pwszURL);

        IMFAsyncResult pResult;
        MFError hrthrowonerror = MFExtern.MFCreateAsyncResult(
            ice, pCallback, pUnkState, out pResult);

        ppIUnknownCancelCookie = ice;

        hrthrowonerror = MFExtern.MFInvokeCallback(pResult);

        return HResult.S_OK;
    }

    public HResult CancelObjectCreation(object pIUnknownCancelCookie)
    {
        // If (somehow) the cookie is not and IcyScheme, this
        // will throw an exception.
        IDisposable icy = (IDisposable)pIUnknownCancelCookie;

        icy.Dispose();

        return HResult.S_OK;
    }

    public HResult EndCreateObject(IMFAsyncResult pResult,
        out MFObjectType pObjectType,
        out object ppObject)
    {
        pObjectType = MFObjectType.ByteStream;

        HResult hr = pResult.GetObject(out ppObject);

        if (MFError.Succeeded(hr))
        {
            IcyScheme icy = (IcyScheme)ppObject;

            // Wait for the HttpWebRequest to finish connecting.  This 
            // includes sending the request, and processing the response
            // headers and caching some data (see m_FirstBlock).
            hr = icy.WaitForConnect();
        }

        return hr;
    }
}

/* Note about threading

I'm not quite sure what I want to do about threading.  In theory I should 
wrap each entry point with a lock().  But do I really need to?  And do I 
really want to?  

    IMFAttributes - Already has protection.

    IDisposable - Dispose() should be callable at any time.  It is 
    effectively a 'cancel' on any TCP operations that might be hung.

    IMFByteStream & IMFAsyncCallback - It would probably make sense here.  
    And we should prevent multiple BeginReads from being active at one time.
    They need to complete sequentially.  Does using the StandardWorkqueue 
    ensure this?

    IMFGetServiceImpl - doesn't seem like it needs it.

    IMFMediaEventGenerator - Calls to this interface mustn't block or be 
    blocked by other calls.

*/

sealed internal class IcyScheme : MFAttributes,
    IDisposable,
    IMFAsyncCallback,
    IMFByteStream,
    IMFGetServiceAlt,
    IMFMediaEventGenerator
{
    #region Members

    private struct ReadParams
    {
        public ReadParams(IntPtr p, int c)
        {
            pb = p;
            cb = c;
        }
        public IntPtr pb;
        public int cb;
    }

    // Html response headers are stored in this IMFAttribute
    public static readonly Guid ICY_HEADERS =
        new Guid(0xF9310F38, 0x7414, 0x4851,
            0xA0, 0x63, 0x9F, 0x1E, 0xCC, 0xD4, 0x92, 0x9F);
    public static readonly Guid BYTEADJUSTMENT =
        new Guid(0xaad0aac0, 0x440c, 0x4fef,
            0xb4, 0x62, 0xf3, 0xaa, 0x62, 0x66, 0xe6, 0x58);

    // Just picking a number.  Looks like 65536 is the normal buffer size,
    // so let's have room for a few.
    private const int DefaultReceiveBufferSize = 65536 * 4;

    // Allow for async loading of stream meta data.
    private delegate void LoadValuesCaller(byte[] s);

    // Mapping from icy HTML headers and in-stream names to WM meta names.
    private readonly Dictionary<string, string> m_FixNames;

    private readonly MetaDataContainer m_MetaData;
    private Uri m_uri;
    private readonly IMFMediaEventQueue m_events;
    private ManualResetEvent m_ConnectEvent;
    private readonly LoadValuesCaller m_caller;

    private byte[] m_FirstBlock;
    private volatile bool m_bShutdown;
    private Stream m_ResponseStream;

    // Current stream position (in bytes).  Note that this isn't the
    // *actual* stream position, since it excludes metadata.
    private long m_Position;

    // Position within the m_FirstBlock.  Used to support seeking.
    private long m_PositionReal;

    // How many bytes have we seen since the last m_MetaInt?
    private int m_SinceLast;

    // Interval at which to expect meta data.  0 means no meta data.
    private int m_MetaInt;

    #endregion

    public IcyScheme(string sUrl)
    {
        m_Position = 0;
        m_SinceLast = 0;
        m_bShutdown = false;
        m_uri = new Uri(sUrl);
        m_ConnectEvent = new ManualResetEvent(false);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_uri);
        ServicePoint point = request.ServicePoint;
        point.ReceiveBufferSize = DefaultReceiveBufferSize * 2;
        request.UserAgent = "NSPlayer/12.00.7601.23471";
        request.Headers.Add("Icy-Metadata:1");

        bool OldUnsafe, UseOldUnsafe; 
        UseOldUnsafe = AllowUnsafeHeaderParsing(true, out OldUnsafe);

        // Parts of BeginGetResponse are still done synchronously,
        // so we do it this way.
        Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse,
                                    request.EndGetResponse,
                                    null)
        .ContinueWith(task =>
        {
            if (UseOldUnsafe)
                AllowUnsafeHeaderParsing(OldUnsafe, out OldUnsafe);

            if (task.Status == TaskStatus.RanToCompletion)
            {
                try
                {
                    WebResponse wr = task.Result;

                    // Turn the headers into metadata, plus store them as an 
                    // IMFAttribute
                    LoadResponseHeaders(wr.Headers);

                    // The only part of the HttpWebRequest that we keep.
                    m_ResponseStream = wr.GetResponseStream();

                    // In order to (pretend  to) support seeking, we save off a block
                    // of data.  Any seeks to the first DefaultReceiveBufferSize bytes
                    // are resolved from this data.
                    m_PositionReal = long.MaxValue;
                    m_FirstBlock = new byte[DefaultReceiveBufferSize];
                    WaitRead(m_FirstBlock, 0, DefaultReceiveBufferSize);
                    m_PositionReal = 0;

                    // Used to tell WaitForConnect that the connection is complete.
                    m_ConnectEvent.Set();
                }
                catch
                {
                    Dispose();
                }
            }
            else
            {
                Dispose();
            }
        });

        // While we are waiting for the connect/cache to complete, finish
        // the constructor.

        m_MetaData = new MetaDataContainer();
        MFError hrthrowonerror = MFExtern.MFCreateEventQueue(out m_events);

        m_FixNames = new Dictionary<string, string>(13);

        // Note that the keys are (intentionally) all lower case.
        m_FixNames["icy-name"] = "WM/RadioStationName";
        m_FixNames["x-audiocast-name"] = "WM/RadioStationName";
        m_FixNames["icy-genre"] = "WM/Genre";
        m_FixNames["x-audiocast-genre"] = "WM/Genre";
        m_FixNames["icy-url"] = "WM/PromotionURL";
        m_FixNames["x-audiocast-url"] = "WM/PromotionURL";
        m_FixNames["x-audiocast-description"] = "Description";
        m_FixNames["x-audiocast-artist"] = "Author";
        m_FixNames["x-audiocast-album"] = "WM/AlbumTitle";
        m_FixNames["x-audiocast-br"] = "BitRate";
        m_FixNames["icy-br"] = "BitRate";
        m_FixNames["streamtitle"] = "Title";
        m_FixNames["streamurl"] = "WM/AudioFileURL";

        hrthrowonerror = SetString(
            MFAttributesClsid.MF_BYTESTREAM_EFFECTIVE_URL,
            m_uri.AbsoluteUri);

        // Use this to make async calls to handle meta data.
        m_caller = new LoadValuesCaller(LoadValues);
    }

    ~IcyScheme()
    {
        Dispose(false);
    }

    #region internal methods

    //  The constructor kicks off an async connect, sends request headers, 
    // waits for a response and caches some data.  Calling this method
    // will block until the class is fully ready to proceed.
    internal HResult WaitForConnect()
    {
        HResult hr;

        if (m_ConnectEvent != null)
        {
            m_ConnectEvent.WaitOne();
            m_ConnectEvent = null;
        }

        if (m_bShutdown)
            hr = HResult.MF_E_OPERATION_CANCELLED;
        else
            hr = HResult.S_OK;

        return hr;
    }

    #endregion

    #region Private methods

    // Turn icy names into WM meta names.  See
    // https://msdn.microsoft.com/en-us/library/windows/desktop/dd743066

    private string FixName(string sKey)
    {
        string rkey;

        bool b = m_FixNames.TryGetValue(sKey, out rkey);
        if (!b)
            rkey = sKey;

        return rkey;
    }

    // Parse strings in Html Header format (icy-genre: Dance) and add them
    // to the meta object.
    private void AddValue(string sKey, string[] sValue)
    {
        // I could add *all* the response headers as metadata, but I think
        // I'll stick to the icy metadata.  The entire response string is
        // available as an IMFAttribute if anyone needs it.

        string sKeyl = sKey.ToLower();

        if (sKeyl.StartsWith("icy-") || sKeyl.StartsWith("x-audiocast-"))
        {
            sKeyl = FixName(sKeyl);

            string s = sValue[0];

            // Deal with headers of the form icy-br:128,128
            if (sValue.Length > 1)
            {
                for (int x = 1; x < sValue.Length; x++)
                {
                    s += "," + sValue[x];
                }
            }

            MFError throwonerror = m_MetaData.SetProperty(
                sKeyl, new PropVariant(s));
        }
    }

    // Strings in icy-metaint format.  This method called asynchronously by
    // DoRead().
    private void LoadValues(byte[] bData)
    {
        // This horrific bit of code is intended to allow for foreign language
        // character sets.  icy provides no way of handling (say) Thai 
        // characters in song titles (and yes, it happens).  But you can 
        // set BYTEADJUSTMENT to 0x0e00 and we'll add it to the bytes here.
        // There's GOT to be a better way...
        int byteadjustment;
        HResult hr = GetUINT32(BYTEADJUSTMENT, out byteadjustment);
        if (MFError.Failed(hr))
            byteadjustment = 0;

        // They're really just bytes.  I could just as well use ASCII here.
        string sData = Encoding.UTF8.GetString(bData);

        // Strings are of the form:
        //      StreamTitle ='something';StreamUrl='somethingelse';
        // Assume no spaces around = or ;
        // Note that there can be (unescaped) embedded ' marks in the
        // something.  Yeah, really.

        int iPos = sData.IndexOf('=');
        while (iPos > 0)
        {
            // I'm not sure there's any standardization about the case of 
            // property names, so I'm forcing them all to lc.
            string sKey = FixName(sData.Substring(0, iPos).Trim().ToLower());

            iPos++; // skip past equal

            // I can't think of a more generic way to search
            // for this.  Since ' can be "embedded" in the
            // string, how do you know where the end of the
            // string is?
            int iEnd = sData.IndexOf("';", iPos);

            // Discard open and close quote
            string sValue = sData.Substring(iPos + 1, iEnd - iPos - 1).Trim();
            StringBuilder value;
            value = new StringBuilder(sValue);

            if (byteadjustment != 0)
                for (int x = 0; x < value.Length; x++)
                    value[x] += (char)byteadjustment;

            // Add (or update) the key/value.
            MFError throwonerror = m_MetaData.SetProperty(
                sKey, new PropVariant(value.ToString()));

            // Skip past ' and ;
            sData = sData.Substring(iEnd + 1 + 1);

            iPos = sData.IndexOf('=');
        }

        // Let anyone who's listening know that there's new metadata.
        SendEvent(MediaEventType.MESourceMetadataChanged);
    }

    // This is the completion part of the async callback invoked
    // in DoRead to process a new meta entry.
    static private void CallbackMethod(IAsyncResult ar)
    {
        // Retrieve the delegate.
        AsyncResult result = (AsyncResult)ar;
        LoadValuesCaller caller = (LoadValuesCaller)result.AsyncDelegate;

        // Call EndInvoke to end the call.
        caller.EndInvoke(ar);
    }

    private void SendEvent(MediaEventType met)
    {
        IMFMediaEvent pEvent;

        MFError throwonhr = MFExtern.MFCreateMediaEvent(
            met,
            Guid.Empty, HResult.S_OK, null, out pEvent);
        throwonhr = m_events.QueueEvent(pEvent);
    }

    // Check for any icy related headers and add them to the metadata.
    private void LoadResponseHeaders(WebHeaderCollection whc)
    {
        MFError hrthrowonerror = SetString(ICY_HEADERS, whc.ToString());

        for (int x = 0; x < whc.Count; x++)
        {
            string s = whc.Keys[x];
            ProcessResponseHeader(s, whc.GetValues(x));
        }
    }

    // As LoadResponseHeaders parses out each header, it passes them here.
    private void ProcessResponseHeader(string sKey, string[] sValue)
    {
        // Add to the meta data if appropriate.
        AddValue(sKey, sValue);

        MFError hrthrowonerror;

        // Handle a couple of special cases.
        if (sKey.IndexOf("content-type",
            StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
            hrthrowonerror = SetString(
                MFAttributesClsid.MF_BYTESTREAM_CONTENT_TYPE, sValue[0]);
        }
        else if (sKey.IndexOf("icy-metaint",
            StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
            m_MetaInt = int.Parse(sValue[0]);
        }
    }

    private void Dispose(bool bDisposing)
    {
        if (!m_bShutdown)
        {
            // Shut down the events
            m_events.Shutdown();

            try
            {
                // This  seems risky in a destructor, but unmananged
                // code could be waiting on this.  Give it our
                // best shot.
                if (m_ConnectEvent != null)
                    m_ConnectEvent.Set();
            }
            catch { }

            // Only do these if we are NOT being called
            // from the destructor.  Otherwise they may
            // already have been destructed.
            if (bDisposing)
            {
                m_bShutdown = true;
                GC.SuppressFinalize(this);

                if (m_ResponseStream != null)
                {
                    m_ResponseStream.Close();
                    m_ResponseStream = null;
                }
            }
        }
    }

    // Block until the requested number of bytes are available.
    private void WaitRead(byte[] ba, int iStartPos, int iSize)
    {
        // Are we still returning data from our first block?
        if (m_PositionReal < DefaultReceiveBufferSize)
        {
            int iUse = Math.Min(
                (int)(DefaultReceiveBufferSize - m_PositionReal), 
                iSize);

            Array.Copy(m_FirstBlock, m_PositionReal, ba, iStartPos, iUse);
            iSize -= iUse;
            iStartPos += iUse;
            m_PositionReal += iUse;

            // Once we really start reading data, there's no going back.
            if (m_PositionReal >= DefaultReceiveBufferSize)
            {
                m_FirstBlock = null;
            }
        }

        while (iSize > 0)
        {
            // Note that Read can return less than iSize bytes, EVEN IF
            // there are there are more bytes in the receive buffer.
            int read = m_ResponseStream.Read(ba, iStartPos, iSize);

            iStartPos += read;
            iSize -= read;
        }
    }

    private int WaitByte()
    {
        byte[] b = new byte[1];

        WaitRead(b, 0, 1);

        return b[0];
    }

    // Read exactly cb bytes into pb
    private void DoRead(IntPtr pb, int cb)
    {
        int iLeft = cb;
        int iRead = 0;

        // Stream uses byte[], so start there.  We're going to block
        // until we can satisfy the entire request, so make it full size.
        byte[] ba = new byte[cb];

        while (iLeft > 0)
        {
            // If there is instream metadata, and if
            // we are going to hit the next metadata while
            // satisfying the current DoRead.
            if (m_MetaInt > 0 && m_SinceLast + iLeft > m_MetaInt)
            {
                // How many bytes before the next metadata?
                int iUse = m_MetaInt - m_SinceLast;

                // Read everything up to the next metadata
                WaitRead(ba, iRead, iUse);

                // How long is the metadata (anywhere from
                // 0 - 4080)?
                int metalength = WaitByte() * 16;

                if (metalength > 0)
                {
                    byte[] metadata = new byte[metalength];
                    WaitRead(metadata, 0, metalength);

                    // Asynchronously handle the meta data while we 
                    // keep reading.
                    m_caller.BeginInvoke(metadata, 
                        new AsyncCallback(CallbackMethod), 
                        null);
                }
                m_SinceLast = 0;
                iLeft -= iUse;
                iRead += iUse;
            }
            else
            {
                // Handle the entire rest of the request
                WaitRead(ba, iRead, iLeft);
                m_SinceLast += iLeft;
                //iLeft -= iLeft;
                //iRead += iLeft;
                break;
            }
        }

        // Copy the byte array to the intptr.
        Marshal.Copy(ba, 0, pb, ba.Length);
    }

    private static bool AllowUnsafeHeaderParsing(bool bNewVal, out bool bOldVal)
    {
        // Get the assembly that contains the internal class
        Type t = typeof(System.Net.Configuration.SettingsSection);
        Assembly aNetAssembly = Assembly.GetAssembly(t);
        if (aNetAssembly != null)
        {
            // Use the assembly in order to get the internal type for the 
            // internal class
            Type aSettingsType = aNetAssembly.GetType(
                "System.Net.Configuration.SettingsSectionInternal");

            if (aSettingsType != null)
            {
                // Use the internal static property to get an instance of the
                // internal settings class. If the static instance isn't 
                // created already, the property will create it for us.
                object anInstance = aSettingsType.InvokeMember("Section",
                    BindingFlags.Static | 
                    BindingFlags.GetProperty | 
                    BindingFlags.NonPublic, 
                    null, 
                    null, 
                    new object[] { });

                if (anInstance != null)
                {
                    // Locate the private bool field that tells the framework
                    // if unsafe header parsing should be allowed or not
                    FieldInfo aUseUnsafeHeaderParsing = 
                        aSettingsType.GetField(
                            "useUnsafeHeaderParsing", 
                            BindingFlags.NonPublic | BindingFlags.Instance);

                    if (aUseUnsafeHeaderParsing != null)
                    {
                        bOldVal = (bool)aUseUnsafeHeaderParsing.GetValue(
                            anInstance);
                        aUseUnsafeHeaderParsing.SetValue(anInstance, bNewVal);

                        return true;
                    }
                }
            }
        }

        bOldVal = false;
        return false;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion

    #region IMFAsyncCallback

    public HResult GetParameters(
        out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
    {
        pdwFlags = 0;
        pdwQueue = 0;

        return HResult.E_NOTIMPL;
    }

    public HResult Invoke(IMFAsyncResult pAsyncResult)
    {
        // IMFByteStream::BeginRead end up here.

        object pState;
        object pUnk;
        IMFAsyncResult pCallerResult;
        MFError hrthrowonerror;

        ReadParams rp;

        // Get the asynchronous result object for the application callback.
        hrthrowonerror = pAsyncResult.GetState(out pState);

        pCallerResult = (IMFAsyncResult)pState;

        // Get the object that holds the state information for the 
        // asynchronous method.
        hrthrowonerror = pCallerResult.GetObject(out pUnk);

        rp = (ReadParams)pUnk;

        HResult hr;

        // Do the work.
        try
        {
            // DoRead might block
            DoRead(rp.pb, rp.cb);
            hr = HResult.S_OK;
        }
        catch
        {
            Dispose();
            hr = HResult.E_ABORT;
        }

        pCallerResult.SetStatus(hr);
        hr = MFExtern.MFInvokeCallback(pCallerResult);

        return hr;
    }

    #endregion

    #region IMFByteStream

    public HResult BeginRead(
        IntPtr pb, int cb, IMFAsyncCallback pCallback, object pUnkState)
    {
        // For reasons beyond my understanding, sometime we are asked to
        // return bytes at negative offsets.  In these cases, we change
        // the number of bytes requested to 0, which results in EndRead
        // returning 0 bytes.
        if (m_Position < 0)
            cb = 0;

        // This handles the case where SetCurrentPosition was called to
        // rewind the stream.
        if (m_Position == 0)
        {
            m_PositionReal = 0;
            m_SinceLast = 0;
        }

        // Since BeginRead is async, caller could call GetCurrentPosition
        // while the read is running.  Spec says they get the new value.
        m_Position += cb;

        IMFAsyncResult pResult;
        ReadParams rp = new ReadParams(pb, cb);

        HResult hr = MFExtern.MFCreateAsyncResult(
            rp, pCallback, pUnkState, out pResult);

        // If 2 BeginReads are called consecutively, might the Workqueue
        // run them concurrently?  I don't know, but that would be bad.
        if (MFError.Succeeded(hr))
            hr = MFExtern.MFPutWorkItem(
                (int)MFASYNC_WORKQUEUE_TYPE.StandardWorkqueue,
                this, pResult);

        return hr;
    }

    public HResult BeginWrite(IntPtr pb,
        int cb, IMFAsyncCallback pCallback, object pUnkState)
    {
        throw new NotImplementedException();
    }

    public HResult Close()
    {
        Dispose();
        return HResult.S_OK;
    }

    public HResult EndRead(IMFAsyncResult pResult, out int pcbRead)
    {
        // In theory, pcbRead might be less than what was requested. However
        // that seems to drive our caller nuts, so we only do that when
        // m_Position < 0.
        HResult hr;
        object o;

        hr = pResult.GetObject(out o);

        if (MFError.Succeeded(hr))
        {
            ReadParams rp = (ReadParams)o;
            pcbRead = rp.cb;
        }
        else
            pcbRead = 0;

        return hr;
    }

    public HResult EndWrite(IMFAsyncResult pResult, out int pcbWritten)
    {
        throw new NotImplementedException();
    }

    public HResult Flush()
    {
        throw new NotImplementedException();
    }

    public HResult GetCapabilities(
        out MFByteStreamCapabilities pdwCapabilities)
    {
        pdwCapabilities = MFByteStreamCapabilities.IsReadable |
            MFByteStreamCapabilities.IsRemote;

        // We only say we support seeking to fool the SourceResolver.  Once
        // we're past that point, no reason to lie.
        if (m_FirstBlock != null)
            pdwCapabilities |= MFByteStreamCapabilities.IsSeekable;

        return HResult.S_OK;
    }

    public HResult GetCurrentPosition(out long pqwPosition)
    {
        pqwPosition = m_Position;

        return HResult.S_OK;
    }

    public HResult GetLength(out long pqwLength)
    {
        pqwLength = -1;

        return HResult.S_OK;
    }

    public HResult IsEndOfStream(out bool pfEndOfStream)
    {
        pfEndOfStream = m_bShutdown;

        return HResult.S_OK;
    }

    public HResult Read(IntPtr pb, int cb, out int pcbRead)
    {
        pcbRead = cb;
        try
        {
            // Read exactly cb bytes into pb.  Block if necessary.
            DoRead(pb, cb);
            m_Position += cb;
        }
        catch
        {
            Dispose();
            throw;
        }

        return HResult.S_OK;
    }

    public HResult Seek(MFByteStreamSeekOrigin SeekOrigin,
        long llSeekOffset,
        MFByteStreamSeekingFlags dwSeekFlags,
        out long pqwCurrentPosition)
    {
        throw new NotImplementedException();
    }

    public HResult SetCurrentPosition(long qwPosition)
    {
        m_Position = qwPosition;

        return HResult.S_OK;
    }

    public HResult SetLength(long qwLength)
    {
        throw new NotImplementedException();
    }

    public HResult Write(IntPtr pb, int cb, out int pcbWritten)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMFGetService

    public HResult GetService(Guid guidService,
        Guid riid, out IntPtr ppvObject)
    {
        // If ppvObject were just an object, this routine would not return
        // the riid the caller requested.  That's why there's a custom
        // implementation of this.

        if (guidService == MFServices.MF_METADATA_PROVIDER_SERVICE)
        {
            // performs an AddRef
            IntPtr ip = Marshal.GetIUnknownForObject(m_MetaData);

            // performs an AddRef
            HResult hr = (HResult)Marshal.QueryInterface(ip,
                ref riid, out ppvObject);

            Debug.WriteLineIf(hr != HResult.S_OK,
                string.Format(
                    "IMFGetService::QueryInterface not supported: {0}: {1}",
                    MFDump.LookupName(typeof(MFServices), guidService),
                    MFDump.LookupInterface(riid)));

            // Must release one of those AddRefs
            Marshal.Release(ip);

            return hr;
        }

        Debug.WriteLine("IMFGetService::GetService not supported: {0}: {1}",
            MFDump.LookupName(typeof(MFServices), guidService),
            MFDump.LookupInterface(riid));

        ppvObject = IntPtr.Zero;
        return HResult.MF_E_UNSUPPORTED_SERVICE;
    }

    #endregion

    #region IMFMediaEventGenerator

    public HResult GetEvent(MFEventFlag dwFlags, out IMFMediaEvent ppEvent)
    {
        HResult hr;

        hr = m_events.GetEvent(dwFlags, out ppEvent);

        return hr;
    }

    public HResult BeginGetEvent(IMFAsyncCallback pCallback, object o)
    {
        HResult hr;

        hr = m_events.BeginGetEvent(pCallback, o);

        return hr;
    }

    public HResult EndGetEvent(IMFAsyncResult pResult,
        out IMFMediaEvent ppEvent)
    {
        HResult hr;

        hr = m_events.EndGetEvent(pResult, out ppEvent);

        return hr;
    }

    public HResult QueueEvent(MediaEventType met,
        Guid guidExtendedType, HResult hrStatus, ConstPropVariant pvValue)
    {
        IMFMediaEvent pEvent;
        HResult hr;

        hr = MFExtern.MFCreateMediaEvent(met,
            Guid.Empty, HResult.S_OK, null, out pEvent);

        if (MFError.Succeeded(hr))
            hr = m_events.QueueEvent(pEvent);

        return hr;
    }

    #endregion
}
