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

using WindowsMediaLib;

namespace ReadFromStream
{
    class Program
    {
        static void Main(string[] args)
        {
            string ptszInput = null;

            //
            // Process the command line options
            //
            CommandLineParser(args, out ptszInput);

            if (ptszInput != null)
            {
                //
                // Use CReader to read from the input file
                //
                CReader pReader = new CReader();

                pReader.Open(ptszInput);

                // Start processing samples
                pReader.Start();

                //
                // Wait until all data has been sent to output.
                //
                while (!pReader.EOF())
                {
                    System.Threading.Thread.Sleep(1);
                }

                Console.WriteLine("Playback finished.");

                pReader.Close();
            }

        }

        static private void Usage()
        {
            Console.WriteLine("ReadFromStream -i <InFile>");
            Console.WriteLine("\tInFile:\tThe input Windows Media Format file (WMA/WMV/ASF) name\n");
        }

        static private void CommandLineParser(string[] args, out string ptszInput)
        {
            ptszInput = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-i")
                {
                    i++;

                    if (i < args.Length)
                    {
                        ptszInput = args[i];
                    }
                }
                else
                {
                    Usage();
                    return;
                }
            }

            if (null == ptszInput)
            {
                Usage();
                return;
            }
        }
    }
}
