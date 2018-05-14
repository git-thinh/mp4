/************************************************************************
ShoutCast - An upgraded PlaybackFX sample that shows song titles.

While the underlying library is covered by LGPL or BSD, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is the PlaybackFX sample reworked to show song titles if the 
specified URL provides them.  To follow the changes, look at m_pMetadata in 
player.cs for the metadata handling.

In Windows 7, the normal MF SourceResolver could connect to ShoutCast stations 
and report song titles.  With only a minor change from PlaybackFX, this sample 
is able to show them.  However in W8, the resolver insists on being able to seek 
the stream (presumably) in case it needs to try using more than one 
ByteStreamHandler.  But ShoutCast streams can't be "seeked."

What's more, in both W7 and W8, MF's SchemeHandler for networks only "sort of" 
handles ShoutCast streams.  If the server sends back "ICY 200 OK", then it 
handles the metadata (mostly) correctly (the mapping of icy attributes to WM 
attributes could use some work).  But if the server sends back "200 OK" (which 
isn't uncommon), it does not report song titles, and can sometimes try to 
"play" the text.

So to support song titles in W8, this sample implements a scheme handler.  In
fact it implements one that works with both kinds of ShoutCast servers.  The 
downside is that it only works in W8.  See MFRegisterLocalSchemeHandler in
player.cs for how this gets hooked in.

In theory this Scheme handler could be made to work with W7 (I've done it), but
it gets ugly.  You must support IMFByteStreamBuffering and you need to rework 
the IMFByteStream interface so that BeginRead takes an IntPtr instead of an 
object for pUnkState (when will MS learn the difference between IUnknown and
LPVOID?!!?).  This has cascade implications as other things must change to 
support this.

NB: This code is specific to ShoutCast, and may or may not play well with other
http media sources.

NB2: The code requires (what is currently) a beta build of the MF library.
A pre-built one is included in the zip, and the source is available at 
http://mfnet.cvs.sourceforge.net/viewvc/mfnet/.

NB3: The IcyScheme project produces a COM object that must be registered (VS
does this for you as part of the build).  But that also means you have to 
explicitly build it for x86 or x64, since registering for one does not affect 
the other.