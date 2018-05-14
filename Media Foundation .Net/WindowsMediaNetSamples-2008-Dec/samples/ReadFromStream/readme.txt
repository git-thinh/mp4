/*************************************************************************
ReadFromStream - Walk the samples in a file where the source is an IStream

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

***************************************************************************/

This file is based on the ReadFromStream sample in the WMF sdk.  It is a
simpler version of what you would see in the AudioPlayer sample, except that
it also shows how to process data in an IStream.

All it does is walk each of the samples in a file (which get sent to OnSample).  
It doesn't do anything with the returned samples other than count them, but
you could do more.

It also shows how to disable the clock so samples get sent at full speed, rather
than at playback speed.
