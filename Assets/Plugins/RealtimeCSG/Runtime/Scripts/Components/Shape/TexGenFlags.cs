using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeCSG
{
	// mirrored on C++ side
	[Flags]
	public enum TexGenFlags : int //32 bits
	{
		None				= 0,
		WorldSpaceTexture	= 1,
		NoRender			= 2,		// do not render
		NoCastShadows		= 4,
		NoReceiveShadows	= 8,
		NoCollision			= 16		// do not add surface to collider
	};
}
