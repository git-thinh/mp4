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
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Runtime.InteropServices;

using WindowsMediaLib;

namespace ReadFromStream
{
    class CROStream : COMBase, IStream
    {
        BinaryReader m_hFile;

        //------------------------------------------------------------------------------
        // Name: CROStream::CROStream()
        // Desc: Constructor.
        //------------------------------------------------------------------------------
        public CROStream()
        {
            m_hFile = null;
        }

        //------------------------------------------------------------------------------
        // Name: ~CROStream()
        // Desc: Destructor.
        //------------------------------------------------------------------------------
        ~CROStream()
        {
            Close();
        }

        //------------------------------------------------------------------------------
        // Name: CROStream::Open()
        // Desc: Opens the file.
        //------------------------------------------------------------------------------
        public void Open(string ptszURL)
        {
            Close();

            //
            // Open the file
            //
            m_hFile = new BinaryReader(new FileStream(ptszURL, FileMode.Open, FileAccess.Read));
        }

        public void Close()
        {
            if (m_hFile != null)
            {
                m_hFile.Close();
                m_hFile = null;
            }
        }

        #region IStream Members

        public void Clone(out IStream ppstm)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Commit(int grfCommitFlags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            if (null == m_hFile)
            {
                throw new COMException("File not open", E_Unexpected);
            }
            int i = m_hFile.Read(pv, 0, cb);

            if (pcbRead != IntPtr.Zero)
            {
                Marshal.WriteInt32(pcbRead, i);
            }
        }

        public void Revert()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            if (null == m_hFile)
            {
                throw new COMException("File not open", E_Unexpected);
            }

            long l = m_hFile.BaseStream.Seek(dlibMove, (SeekOrigin)dwOrigin);
        }

        public void SetSize(long libNewSize)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            if (1 != grfStatFlag)
            {
                throw new COMException("Bad arg to Stat", E_InvalidArgument);
            }

            if (null == m_hFile)
            {
                throw new COMException("File not open", E_Unexpected);
            }

            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG();
            pstatstg.cbSize = m_hFile.BaseStream.Length;
            pstatstg.type = 2;

        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
