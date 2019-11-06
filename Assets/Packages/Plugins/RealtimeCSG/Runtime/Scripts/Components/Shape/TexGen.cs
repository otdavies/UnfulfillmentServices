using System;
using UnityEngine;
using System.Runtime.InteropServices;
using RealtimeCSG;

namespace InternalRealtimeCSG
{
    [Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct TexGen
    {
		public Color			Color;
        public Vector2			Translation;
        public Vector2			Scale;
        public float			RotationAngle;
        public Material			RenderMaterial;
        public PhysicMaterial	PhysicsMaterial;
        public UInt32			SmoothingGroup;

		public TexGen(Material renderMaterial = null, PhysicMaterial physicsMaterial = null)
		{
			Color				= Color.white;
			Translation			= MathConstants.zeroVector3;
			Scale				= MathConstants.oneVector3;
			RotationAngle		= 0;
			RenderMaterial		= renderMaterial;
			PhysicsMaterial		= physicsMaterial;
			SmoothingGroup		= 0;
		}
    }

}
