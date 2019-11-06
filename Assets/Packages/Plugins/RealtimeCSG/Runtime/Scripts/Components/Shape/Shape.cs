using System;
using System.Runtime.InteropServices;
using UnityEngine;
using RealtimeCSG;
using System.Globalization;
using UnityEngine.Serialization;

namespace InternalRealtimeCSG
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct CutNode
	{
		public static Int16 Inside	= -1;
		public static Int16 Outside	= -2;

		[SerializeField] public Int16			backNodeIndex;
		[SerializeField] public Int16			frontNodeIndex;
		[SerializeField] public Int32			planeIndex;
		
		
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "(backNodeIndex: {0}, frontNodeIndex: {1}, planeIndex: {2})",  backNodeIndex, frontNodeIndex, planeIndex);
		}
	}

	[Serializable]
    public sealed class Shape
    {
		[SerializeField] public float           Version         = 1.0f;

		[SerializeField] public Surface[]		Surfaces		= new Surface[0];
		[SerializeField] public TexGen[]		TexGens			= new TexGen[0];
		[SerializeField] public TexGenFlags[]	TexGenFlags		= new TexGenFlags[0];

		// Do not use this anymore, it's deprecated
		[FormerlySerializedAs("LegacyMaterials")]
		[Obsolete("Materials are now stored in TexGens")]
		[SerializeField] internal Material[]	Materials		= new Material[0];

#if UNITY_EDITOR
		public Shape() { }
		public Shape(Shape other)
		{
			CopyFrom(other);
		}

		public Shape(int polygonCount)
		{
			Surfaces	= new Surface[polygonCount];
			TexGenFlags = new TexGenFlags[polygonCount];
			TexGens		= new TexGen[polygonCount];
		}

		public void Reset()
		{
			Surfaces		= new Surface[0];
			TexGens			= new TexGen[0];
			TexGenFlags		= new TexGenFlags[0];
		} 

		public void CopyFrom(Shape other)
		{
			if (other == null)
			{
				Reset();
				return;
			}

			if (this.Surfaces != null)
			{
				if (this.Surfaces == null || this.Surfaces.Length != other.Surfaces.Length)
					this.Surfaces = new Surface[other.Surfaces.Length];
				Array.Copy(other.Surfaces, this.Surfaces, other.Surfaces.Length);
			} else
				this.Surfaces = null;
			
			if (this.TexGens != null)
			{
				if (this.TexGens == null || this.TexGens.Length != other.TexGens.Length)
					this.TexGens = new TexGen[other.TexGens.Length];
				Array.Copy(other.TexGens, this.TexGens, other.TexGens.Length);
			} else
				this.TexGens = null;

			if (this.TexGenFlags != null)
			{
				if (this.TexGenFlags == null || this.TexGenFlags.Length != other.TexGenFlags.Length)
					this.TexGenFlags = new TexGenFlags[other.TexGenFlags.Length];
				Array.Copy(other.TexGenFlags, this.TexGenFlags, other.TexGenFlags.Length);
			} else
				this.TexGenFlags = null;
		}
		
		public Shape Clone() { return new Shape(this); }

		public bool CheckMaterials()
		{
			bool dirty = false;
			if (Surfaces == null ||
				Surfaces.Length == 0)
			{
				Debug.LogWarning("Surfaces == null || Surfaces.Length == 0");
				return true;
			}
			
			int maxTexGenIndex = 0;
			for (int i = 0; i < Surfaces.Length; i++)
			{
				maxTexGenIndex = Mathf.Max(maxTexGenIndex, Surfaces[i].TexGenIndex);
			}
			maxTexGenIndex++;

			if (TexGens == null ||
				TexGens.Length < maxTexGenIndex)
			{
				dirty = true;
				var newTexGens = new TexGen[maxTexGenIndex];
				var newTexGenFlags = new TexGenFlags[maxTexGenIndex];
				if (TexGens != null &&
					TexGens.Length > 0)
				{
					for (int i = 0; i < TexGens.Length; i++)
					{
						newTexGens[i] = TexGens[i];
						newTexGenFlags[i] = TexGenFlags[i];
					}
					for (int i = TexGens.Length; i < newTexGens.Length; i++)
					{
						newTexGens[i].Color = Color.white;
					}
				}
				TexGens = newTexGens;
				TexGenFlags = newTexGenFlags;
			}

			for (int i = 0; i < TexGens.Length; i++)
			{
				if (TexGens[i].Color == Color.clear)
					TexGens[i].Color = Color.white;
			}

			return dirty;
		}

#endif
	} 
}
 