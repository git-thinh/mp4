/************************************************************************
AsfNet - Sends the output of DirectShow capture graphs to a network port 
to be read by Windows Media Player

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

In addition to the Windows Media library, you'll also need DirectShowLib (located
at http://directshownet.sourceforge.net)

After starting this application, you should be able to connect to the output of 
your capture device by connecting to a TCP/IP port.  From Windows Media Player, go to 
File/Open URL and enter your machine name/ip (for example: http://192.168.0.2:8080).

The most common question asked about this sample has to do with reducing latency.  There
is a several second delay between when something happens at the camera and when that is
seen at the player.  While it is possible to reduce (slightly) that delay by playing 
with various settings, there is no way to completely eliminate it.

Second, is how to change the format of the output (ie other than "No audio, 56 Kbps").  
Check out the section titled "Profiles & LoadProfileByID vs LoadProfileByData & GenProfile" 
in docs\readme.rtf.