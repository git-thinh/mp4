/************************************************************************
AudioPlayer - Play the audio portion of a windows media file

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This file is based on the audioplayer sample in the WMF sdk.  It has a few
improvements over the original.  A couple hundred lines get removed due to
the fact that this code uses exceptions rather than checking the return
value from each method.  

Also, rather than WM allocating a buffer and sending that to a callback 
which copies the data into buffers for the waveOut device, this code uses 
the same buffer for both, avoiding the copy.

Further, rather then allocating, initing, and copying a new buffer for
each sample, this code uses an array of buffers in a loop.  IMO, this is
a more efficient scheme.

I also didn't like how incestuous the form and the player were.  There
was no good way to break out the player code in the original c++, it was
too intertwined with the form.  I've fixed this.

I've also added a volume control.