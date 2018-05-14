/************************************************************************
AsfCreate - Creates an Asf or WMV file from a collection of bitmaps

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

There are some useful comments at the top of CwmvFile.cs.

Description:

Not really much to say.  Create an instance of the class, then pass it bitmaps:

    // Profile id for "Windows Media Video 8 for Dial-up Modem (No audio, 56 Kbps)"
    Guid g = new Guid(0x6E2A6955, 0x81DF, 0x4943, 0xBA, 0x50, 0x68, 0xA9, 0x86, 0xA7, 0x08, 0xF6);

    CwmvFile f = new CwmvFile("foo.asf", g, 2); // 2 FPS
    Bitmap b;

    for (int x=0; x < 100; x++)
    {
        b = new Bitmap(string.Format("C:\\pictures\\{0:00000000}.jpg", x));
        f.AppendNewFrame(b);
        b.Dispose();
    }
    f.Close();

Probably the most common question about this sample is how to change the format
of the output (ie other than "No audio, 56 Kbps").  Check out the section titled
"Profiles & LoadProfileByID vs LoadProfileByData & GenProfile" in docs\readme.rtf.