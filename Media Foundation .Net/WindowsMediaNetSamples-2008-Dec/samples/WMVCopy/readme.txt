/************************************************************************
WMVCopy - Code to copy a file

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This code is nearly a direct c# translation of the wmvcopy c++ sample in
the WMF sdk.  While it is much shorter (1015 lines vs 1406), the difference
comes mostly from the fact that the c++ code needs to do error checking on
each method it calls, while in c#, they all throw exceptions.