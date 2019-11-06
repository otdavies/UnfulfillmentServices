using System;
using UnityEngine;
using System.Runtime.InteropServices;
using RealtimeCSG;

namespace InternalRealtimeCSG
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Surface
    {
        public CSGPlane		Plane;
        public Vector3		Tangent;	
        public Vector3		BiNormal;
		public Int32		TexGenIndex;

		public override string ToString()
		{
			return string.Format("Plane: {0} Tangent: {1} BiNormal: {2} TexGenIndex: {3}", Plane, Tangent, BiNormal, TexGenIndex);
		}
	}
}
