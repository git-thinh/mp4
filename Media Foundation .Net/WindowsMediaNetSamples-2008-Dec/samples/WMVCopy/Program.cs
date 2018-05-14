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

namespace WMVCopy
{
    class Program : COMBase
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: wmvcopy  -i <INPUT_FILE> -o <OUTPUT_FILE> -d <TIME> [-s]");
            Console.WriteLine("  -i <INPUT_FILE> = Input Windows Media file name");
            Console.WriteLine("  -o <OUTPUT_FILE> = Output Windows Media file name");
            Console.WriteLine("  -d <TIME> = specify the max duration in seconds of the output file");
            Console.WriteLine("  -s = Move scripts in script stream to header");
            Console.WriteLine("");
            Console.WriteLine("  Note: this program will not copy the image stream in the source file.");
        }

        static void Main(string[] args)
        {
            int hr = 0;
            string pwszInFile = null;
            string pwszOutFile = null;
            long qwMaxDuration = 0;  // Maximum duration of output file 
            bool fMoveScriptStream = false;
            bool fValidArgument = false;
            WMVCopy pWMVCopy = null;

            do
            {
                //
                // Parse the command line
                //
                int i;
                for (i = 0; i < args.Length; i++)
                {
                    fValidArgument = false;

                    if (args[i].ToLower() == "-i")
                    {
                        i++;
                        if (i >= args.Length || null != pwszInFile)
                        {
                            break;
                        }

                        pwszInFile = args[i];
                    }
                    else if (args[i].ToLower() == "-o")
                    {
                        i++;
                        if (i >= args.Length || null != pwszOutFile)
                        {
                            break;
                        }

                        pwszOutFile = args[i];
                    }
                    else if (args[i].ToLower() == "-d")
                    {
                        i++;
                        if (i >= args.Length)
                        {
                            break;
                        }

                        int nMaxDuration = int.Parse(args[i]);
                        if (nMaxDuration <= 0)
                        {
                            break;
                        }

                        qwMaxDuration = nMaxDuration * 10000000;
                    }
                    else if (args[i].ToLower() == "-s")
                    {
                        Console.WriteLine("Move scripts in script stream to header.");
                        fMoveScriptStream = true;
                    }
                    else
                    {
                        break;
                    }

                    fValidArgument = true;
                }

                if (!fValidArgument || i < args.Length)
                {
                    hr = E_InvalidArgument;
                    Console.WriteLine("Invalid argument.");
                    Usage();
                    break;
                }

                if (null == pwszInFile || null == pwszOutFile || pwszInFile == pwszOutFile)
                {
                    hr = E_InvalidArgument;
                    Console.WriteLine("An input file and an output file must be specified.");
                    Usage();
                    break;
                }

                try
                {
                    pWMVCopy = new WMVCopy();

                    //
                    // Copy this file
                    //
                    pWMVCopy.Copy(pwszInFile, pwszOutFile, qwMaxDuration, fMoveScriptStream);
                    pWMVCopy.Dispose();
                }
                catch (Exception e)
                {
                    hr = Marshal.GetHRForException(e);
                }
            }
            while (false);

            //
            // Release all resources
            //
            //SAFE_ARRAYDELETE(pwszInFile);
            //SAFE_ARRAYDELETE(pwszOutFile);
            //SAFE_RELEASE(pWMVCopy);

            Environment.Exit(hr);
        }
    }
}
