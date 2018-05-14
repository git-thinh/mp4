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
using System.Runtime.InteropServices;

using WindowsMediaLib;

namespace DrmHeader
{
    class Program
    {
        //
        // DRM properties to be queried
        //
        static private void Usage()
        {
            Console.WriteLine("Usage:\n\tDRMHeader.exe <FileName> [ PropertyName ]\n");
            Console.WriteLine("Queries the requested Property in the file.");
            Console.WriteLine("If no property name is specified, all existing properties will be displayed.\n");
        }

        //------------------------------------------------------------------------------
        // Name: PrintProperties()
        // Desc: Displays all DRM properties 
        //------------------------------------------------------------------------------

        static void PrintProperties(string pwszFileName, string pwszPropertyName)
        {
            string[] g_DRMProperties = {
	            Constants.g_wszWMDRM_IsDRM,
	            Constants.g_wszWMDRM_IsDRMCached,
	            Constants.g_wszWMDRM_BaseLicenseAcqURL,
	            Constants.g_wszWMDRM_Rights,
	            Constants.g_wszWMDRM_LicenseID,

	            Constants.g_wszWMDRM_ActionAllowed_Playback            ,  //= L\"ActionAllowed.Play\"
	            Constants.g_wszWMDRM_ActionAllowed_CopyToCD            ,  //= L\"ActionAllowed.Print.redbook\"
	            Constants.g_wszWMDRM_ActionAllowed_CopyToSDMIDevice    ,  //= L\"ActionAllowed.Transfer.SDMI\"
	            Constants.g_wszWMDRM_ActionAllowed_CopyToNonSDMIDevice ,  //= L\"ActionAllowed.Transfer.NONSDMI\"
	            Constants.g_wszWMDRM_ActionAllowed_Backup              ,  //= L\"ActionAllowed.Backup\"

	            Constants.g_wszWMDRM_DRMHeader                         ,  //= L\"DRMHeader.\"
	            Constants.g_wszWMDRM_DRMHeader_KeyID                   ,  //= L\"DRMHeader.KID\"
	            Constants.g_wszWMDRM_DRMHeader_LicenseAcqURL           ,  //= L\"DRMHeader.LAINFO\"
	            Constants.g_wszWMDRM_DRMHeader_ContentID               ,  //= L\"DRMHeader.CID\"
	            Constants.g_wszWMDRM_DRMHeader_IndividualizedVersion   ,  //= L\"DRMHeader.SECURITYVERSION\"
	            Constants.g_wszWMDRM_DRMHeader_ContentDistributor      ,  //= L\"DRMHeader.ContentDistributor\"
	            Constants.g_wszWMDRM_DRMHeader_SubscriptionContentID   ,  //= L\"DRMHeader.SubscriptionContentID\"
                Constants.g_wszWMDRM_LicenseState_Playback, 
                Constants.g_wszWMDRM_LicenseState_CopyToCD ,
                Constants.g_wszWMDRM_LicenseState_CopyToSDMIDevice ,
                Constants.g_wszWMDRM_LicenseState_CopyToNonSDMIDevice,
                Constants.g_wszWMDRM_LicenseState_Copy,
                Constants.g_wszWMDRM_LicenseState_PlaylistBurn,
                Constants.g_wszWMDRM_LicenseState_CreateThumbnailImage,
                Constants.g_wszWMDRM_LicenseState_Backup,
                Constants.g_wszWMDRM_LicenseState_CollaborativePlay
            };

            CDrmHeaderQuery drmHQ = new CDrmHeaderQuery();

            try
            {
                drmHQ.Open(pwszFileName);
            }
            catch (Exception e)
            {
                int hr = Marshal.GetHRForException(e);
                Console.WriteLine(string.Format("Failed to open file, HR = 0x{0}", hr));
                return;
            }

            try
            {
                if (null != pwszPropertyName)
                {
                    try
                    {
                        string s = drmHQ.GetPropertyAsString(pwszPropertyName);

                        Console.WriteLine(string.Format("{0} :\t{1}", pwszPropertyName, s));
                    }
                    catch (Exception e)
                    {
                        int hr = Marshal.GetHRForException(e);
                        Console.WriteLine(string.Format("Failed to query for the given property. HR = 0x{0}", hr));
                    }
                }
                else
                {
                    foreach (string sProp in g_DRMProperties)
                    {
                        try
                        {
                            string s = drmHQ.GetPropertyAsString(sProp);

                            Console.WriteLine(string.Format("{0} :\t{1}", sProp, s));
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e)
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

                Console.WriteLine(string.Format("0x{0:x}: {1}", hr, s));
            }

        }

        //------------------------------------------------------------------------------
        // Name: _tmain()
        // Desc: The entry point for this console application 
        //------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Usage();
                Environment.Exit(-1);
            }

            string pwszPropertyName = null;

            if (args.Length > 1)
            {
                pwszPropertyName = args[1];
            }

            PrintProperties(args[0], pwszPropertyName);

            Environment.Exit(0);
        }
    }
}
