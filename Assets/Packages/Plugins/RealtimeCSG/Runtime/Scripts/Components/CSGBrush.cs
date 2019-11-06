using System;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	[ExecuteInEditMode]
	[AddComponentMenu("CSG/Brush")]
	[System.Reflection.Obfuscation(Exclude = true)]
    public sealed class CSGBrush : CSGNode
	{
		const float LatestVersion = 2.0f;
		[HideInInspector] public float Version = LatestVersion;
#if UNITY_EDITOR
        [SerializeField] public CSGOperationType	OperationType	= CSGOperationType.Additive;
        [SerializeField] public Shape				Shape;
		[SerializeField] public ControlMesh			ControlMesh;
		[SerializeField] public uint				ContentLayer;

		public bool IsRegistered { get { return nodeID != CSGNode.InvalidNodeID; } }
		
		[HideInInspector][SerializeField] public BrushFlags flags = BrushFlags.None;
		[HideInInspector][NonSerialized] public Int32		nodeID  = CSGNode.InvalidNodeID;
		[HideInInspector][NonSerialized] public Int32		brushID	= CSGNode.InvalidNodeID;
		[HideInInspector][NonSerialized] public Color?		outlineColor;
		
		void OnApplicationQuit()		{ CSGSceneManagerRedirector.Interface.OnApplicationQuit(); }

        // register ourselves with our scene manager
        void Awake()
		{
			// cannot change visibility since this might have an effect on exporter
			this.hideFlags |= HideFlags.DontSaveInBuild;
			this.gameObject.tag = "EditorOnly";
			this.nodeID = CSGNode.InvalidNodeID;
			this.brushID = CSGNode.InvalidNodeID;
			if (CSGSceneManagerRedirector.Interface != null)
				CSGSceneManagerRedirector.Interface.OnCreated(this);
		}

        void OnEnable()					{ if (CSGSceneManagerRedirector.Interface != null) CSGSceneManagerRedirector.Interface.OnEnabled(this); }

        // unregister ourselves from our scene manager
        void OnDisable()				{ if (CSGSceneManagerRedirector.Interface != null) CSGSceneManagerRedirector.Interface.OnDisabled(this); }
        void OnDestroy()				{ if (CSGSceneManagerRedirector.Interface != null) CSGSceneManagerRedirector.Interface.OnDestroyed(this); }

        // detect if this node has been moved within the hierarchy
        void OnTransformParentChanged()	{ if (CSGSceneManagerRedirector.Interface != null) CSGSceneManagerRedirector.Interface.OnTransformParentChanged(this); }

        // called when any value of this brush has been modified from within the inspector
        void OnValidate()				{ if (CSGSceneManagerRedirector.Interface != null) CSGSceneManagerRedirector.Interface.OnValidate(this); }
		
		public void EnsureInitialized()
		{
			if (CSGSceneManagerRedirector.Interface != null)
				CSGSceneManagerRedirector.Interface.EnsureInitialized(this);			
		}

		public void CheckVersion()
		{
			if (Version >= LatestVersion)
				return;

			if (Version < 1.0f)
				Version = 1.0f;

			if (Version == 1.0f)
			{
				#pragma warning disable 618 // Type is now obsolete
				if (Shape.Materials != null && Shape.Materials.Length > 0)
				{
					// update textures
					if (Shape.TexGens != null)
					{
						for (int i = 0; i < Shape.TexGens.Length; i++)
						{ 
							Shape.TexGens[i].RenderMaterial = null;
						}
						
						#pragma warning disable 618 // Type is now obsolete
						for (int i = 0; i < Mathf.Min(Shape.Materials.Length, Shape.TexGens.Length); i++) 
						{
							#pragma warning disable 618 // Type is now obsolete
							Shape.TexGens[i].RenderMaterial = Shape.Materials[i];
						}

						for (int i = 0; i < Shape.TexGenFlags.Length; i++)
						{
							var oldFlags			= (int)Shape.TexGenFlags[i];
							var isWorldSpaceTexture	= (oldFlags & 1) == 1;

							var isNotVisible		= (oldFlags & 2) == 2;
							var isNoCollision		= isNotVisible;
							var isNotCastingShadows	= ((oldFlags & 4) == 0) && !isNotVisible;

							TexGenFlags newFlags = (TexGenFlags)0;
							if (isNotVisible)		 newFlags |= RealtimeCSG.TexGenFlags.NoRender;
							if (isNoCollision)		 newFlags |= RealtimeCSG.TexGenFlags.NoCollision;
							if (isNotCastingShadows) newFlags |= RealtimeCSG.TexGenFlags.NoCastShadows;
							if (isWorldSpaceTexture) newFlags |= RealtimeCSG.TexGenFlags.WorldSpaceTexture;
						} 
					}
				}

			}

			Version = LatestVersion;
		}
#else
        void Awake()
		{
			this.hideFlags = HideFlags.DontSaveInBuild;
			this.gameObject.tag = "EditorOnly"; 
		}
#endif
	}
}
