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

namespace WMProp
{
    class Program
    {
        private static void Usage()
        {
            Console.WriteLine("WMProp <infile>");
            Console.WriteLine("infile = Input Windows Media file for which properties are sought");
        }

        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                Usage();
                Environment.Exit(-1);
            }

            CWMProp wmProp = new CWMProp();

            wmProp.Report(args[0]);
        }
    }
}
