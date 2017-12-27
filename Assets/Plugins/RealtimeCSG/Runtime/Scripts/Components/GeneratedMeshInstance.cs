using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InternalRealtimeCSG
{
#if UNITY_EDITOR
	public struct MeshInstanceKey
	{
		public static MeshInstanceKey GenerateKey(RenderSurfaceType renderSurfaceType, int subMeshIndex, int meshType, Material renderMaterial, PhysicMaterial physicsMaterial)
		{
			var realRenderMaterial = renderMaterial;
			if (renderSurfaceType != RenderSurfaceType.Normal)
				realRenderMaterial = null;

			return new MeshInstanceKey(renderSurfaceType, subMeshIndex, meshType, (!realRenderMaterial ) ? 1 : realRenderMaterial.GetInstanceID(), 
																				  (!physicsMaterial) ? 1 : physicsMaterial.GetInstanceID());
		}

		private MeshInstanceKey(RenderSurfaceType renderSurfaceType, int subMeshIndex, int meshType, int renderMaterialInstanceID, int physicsMaterialInstanceID)
		{
			RenderSurfaceType		  = renderSurfaceType;
			SubMeshIndex			  = subMeshIndex;
			MeshType				  = meshType;
			RenderMaterialInstanceID  = renderMaterialInstanceID;
			PhysicsMaterialInstanceID = physicsMaterialInstanceID;
		}

		public readonly RenderSurfaceType	RenderSurfaceType;
		public readonly int                 SubMeshIndex;
		public readonly int                 MeshType;
		public readonly int					RenderMaterialInstanceID;
		public readonly int					PhysicsMaterialInstanceID;

		public override int GetHashCode()
		{
			var hash1 = RenderSurfaceType		 .GetHashCode();
			var hash2 = RenderMaterialInstanceID .GetHashCode();
			var hash3 = PhysicsMaterialInstanceID.GetHashCode();
			var hash4 = SubMeshIndex.GetHashCode();
			var hash5 = MeshType.GetHashCode();
			var hash = hash1;
			hash *= 389 + hash2;
			hash *= 397 + hash3;
			hash *= 401 + hash4;
			hash *= 403 + hash5;

			return hash + (hash1 ^ hash2 ^ hash3 ^ hash4 ^ hash5) + (hash1 + hash2 + hash3 + hash4 + hash5) + (hash1 * hash2 * hash3 * hash4 * hash5);
		}
	}

	[Serializable]
	public enum RenderSurfaceType
	{
		Normal,
		[FormerlySerializedAs("Discarded")] Hidden,	// manually hidden by user
		[FormerlySerializedAs("Invisible")] Culled, // removed by CSG process
		ShadowOnly,									// surface that casts shadows
		Collider,
		Trigger,
		CastShadows,								// surface that casts shadows
		ReceiveShadows								// surface that receive shadows
	}
#endif

	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	[System.Reflection.Obfuscation(Exclude = true)]
	public sealed class GeneratedMeshInstance : MonoBehaviour
	{
		[HideInInspector] public float Version = 1.00f;
#if UNITY_EDITOR
		public int                  SubMeshIndex = 0;
		public Mesh					SharedMesh;
		public int					MeshType;
		public Material				RenderMaterial;
		public PhysicMaterial		PhysicsMaterial;
		public RenderSurfaceType	RenderSurfaceType = (RenderSurfaceType)999;
		
		public UInt64				VertexHashValue		= 0;
		public UInt64				TriangleHashValue	= 0;
		public UInt64				SurfaceHashValue	= 0;

		[HideInInspector] public bool	HasUV2				= false;
		[HideInInspector] public float	ResetUVTime			= float.PositiveInfinity;
		[HideInInspector] public bool	HasCollider			= false;
		[HideInInspector] public float	ResetColliderTime	= float.PositiveInfinity;
		[NonSerialized] [HideInInspector] public bool ResetLighting = false;
		[NonSerialized] [HideInInspector] public bool IsDirty	= true;
		[NonSerialized] [HideInInspector] public MeshCollider	CachedMeshCollider;
		[NonSerialized] [HideInInspector] public MeshFilter		CachedMeshFilter;
		[NonSerialized] [HideInInspector] public MeshRenderer	CachedMeshRenderer;
		[NonSerialized] [HideInInspector] public System.Object	CachedMeshRendererSO;

		public MeshInstanceKey GenerateKey()
		{
			return MeshInstanceKey.GenerateKey(RenderSurfaceType, SubMeshIndex, MeshType, RenderMaterial, PhysicsMaterial);
		}
		internal void Awake()
		{
			// cannot change visibility since this might have an effect on exporter
			this.gameObject.hideFlags |= HideFlags.DontSaveInBuild | HideFlags.NotEditable;
			this.hideFlags |= HideFlags.DontSaveInBuild | HideFlags.NotEditable;
		}
		//internal void OnDestroy() { Debug.Log("OnDestroy", this); }
#else
        void Awake()
		{
			this.hideFlags = HideFlags.DontSaveInBuild;
		}
#endif
	}
}