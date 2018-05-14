/****************************************************************************
  While the underlying library is covered by LGPL or BSD, this sample is
  released as public domain.  It is distributed in the hope that it will
  be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
****************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using MediaFoundation;
using MediaFoundation.Misc;

// Generic implementation of a MetaData class.  Does not support
// languages.
internal class MetaDataContainer : IMFMetadataProvider, IMFMetadata
{
    #region Members

    // Values of current meta data are stored here.
    private readonly Dictionary<string, string> m_MetaValues;

    #endregion

    internal MetaDataContainer()
    {
        m_MetaValues = new Dictionary<string, string>(10);
    }

    #region IMFMetadataProvider

    public HResult GetMFMetadata(
        IMFPresentationDescriptor pPresentationDescriptor,
        int dwStreamIdentifier,
        int dwFlags,
        out IMFMetadata ppMFMetadata)
    {
        ppMFMetadata = this;

        return HResult.S_OK;
    }

    #endregion

    #region IMFMetadata

    public HResult DeleteProperty(string pwszName)
    {
        bool b;

        lock (m_MetaValues)
        {
            b = m_MetaValues.Remove(pwszName);
        }
        if (b)
            return HResult.S_OK;
        else
            return HResult.MF_E_PROPERTY_NOT_FOUND;
    }

    public HResult GetAllLanguages(PropVariant ppvLanguages)
    {
        string[] sa = new string[0];
        PropVariant pv = new PropVariant(sa);

        pv.Copy(ppvLanguages);

        return HResult.S_OK;
    }

    public HResult GetAllPropertyNames(PropVariant ppvNames)
    {
        PropVariant pv;

        if (m_MetaValues.Count > 0)
        {
            string[] sa;
            lock (this)
            {
                sa = m_MetaValues.Keys.ToArray();
            }
            pv = new PropVariant(sa);
        }
        else
        {
            // By spec, send back VT_EMPTY for 0 names.
            pv = new PropVariant();
        }
        pv.Copy(ppvNames);

        return HResult.S_OK;
    }

    public HResult GetLanguage(out string ppwszRFC1766)
    {
        ppwszRFC1766 = null;

        return HResult.E_NOTIMPL;
    }

    public HResult GetProperty(string pwszName, PropVariant ppvValue)
    {
        string s;
        bool b;

        lock (this)
        {
            b = m_MetaValues.TryGetValue(pwszName, out s);
        }

        if (b)
        {
            PropVariant pv = new PropVariant(s);
            pv.Copy(ppvValue);

            return HResult.S_OK;
        }

        return HResult.MF_E_PROPERTY_NOT_FOUND;
    }

    public HResult SetLanguage(string pwszRFC1766)
    {
        return HResult.E_NOTIMPL;
    }

    public HResult SetProperty(string pwszName, ConstPropVariant ppvValue)
    {
        lock (m_MetaValues)
        {
            m_MetaValues[pwszName] = ppvValue.GetString();
        }

        return HResult.S_OK;
    }

    #endregion
}
