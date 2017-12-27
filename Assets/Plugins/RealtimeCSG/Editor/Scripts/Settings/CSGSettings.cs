﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;
using UnityEngine.Serialization;

namespace RealtimeCSG
{
	[Serializable]
	public enum ToolEditMode
	{
		Object,
		Generate,
		Mesh,
		Clip,
		Surfaces,
	}

	[Flags]
	[Serializable]
	public enum HelperSurfaceFlags
	{		
		ShowVisibleSurfaces			= 1,
		[FormerlySerializedAs("ShowDiscardedSurfaces")] ShowHiddenSurfaces	= 2,	// manually hidden surfaces
		[FormerlySerializedAs("ShowInvisibleSurfaces")] ShowCulledSurfaces	= 4,	// surfaces removed by CSG process
		ShowColliderSurfaces		= 8,
		ShowTriggerSurfaces			= 16,
		ShowCastShadowsSurfaces		= 32,
		ShowReceiveShadowsSurfaces	= 64
	};
	
	[Serializable]
	public enum ClipMode
	{
		RemovePositive,
		RemoveNegative,
		Split
	};
	 
	public static class CSGSettings
	{
		public static bool ShowVisibleSurfaces			{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowVisibleSurfaces) != 0; } }
		public static bool ShowHiddenSurfaces			{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowHiddenSurfaces) != 0; } }
		public static bool ShowCulledSurfaces			{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowCulledSurfaces) != 0; } }
		public static bool ShowColliderSurfaces			{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowColliderSurfaces) != 0; } }
		public static bool ShowTriggerSurfaces			{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowTriggerSurfaces) != 0; } }
		public static bool ShowCastShadowsSurfaces		{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowCastShadowsSurfaces) != 0; } }
		public static bool ShowReceiveShadowsSurfaces	{ get { return (VisibleHelperSurfaces & HelperSurfaceFlags.ShowReceiveShadowsSurfaces) != 0; } }
				

		internal static HashSet<string>					wireframeSceneviews = new HashSet<string>();
		internal static Dictionary<SceneView, bool>		sceneViewShown		= new Dictionary<SceneView, bool>();


		internal static bool Assume2DView(SceneView sceneview)
		{
			return sceneview != null && sceneview.camera != null && sceneview.camera.orthographic && IsWireframeShown(sceneview);
		}

		internal static bool IsWireframeShown(SceneView sceneView)
		{
			if (!sceneView)
				return false;

			bool isShown;
			if (sceneViewShown.TryGetValue(sceneView, out isShown))
				return isShown;
			
			var name = sceneView.name;
			if (name != null) name = name.Trim();
			if (string.IsNullOrEmpty(name))
				sceneView.name = GetUniqueSceneviewName(GetKnownSceneviewNames());
			isShown = wireframeSceneviews.Contains(sceneView.name);
			sceneViewShown[sceneView] = isShown;
			return isShown;
		}

		internal static void UpdateWireframeModes()
		{
			var sceneViews = SortedSceneViews();
			foreach (SceneView sceneView in sceneViews)
			{
				if (IsWireframeShown(sceneView) && RealtimeCSG.CSGSettings.EnableRealtimeCSG)
				{
					sceneView.SetSceneViewShaderReplace(ColorSettings.GetWireframeShader(), null);
				} else
				{
					sceneView.SetSceneViewShaderReplace(null, null);
				}
			}
		}

		internal static void SetWireframeShown(SceneView sceneView, bool show)
		{
			if (!sceneView)
				return;

			var name = sceneView.name;
			if (name != null) name = name.Trim();
			if (string.IsNullOrEmpty(name))
			{
				sceneView.name = GetUniqueSceneviewName(GetKnownSceneviewNames());
			}
			
			if (show)
			{
				if (!wireframeSceneviews.Contains(sceneView.name))
				{
					sceneView.SetSceneViewShaderReplace(ColorSettings.GetWireframeShader(), null);
					wireframeSceneviews.Add(sceneView.name);
				}
			} else
			{
				if (wireframeSceneviews.Contains(sceneView.name))
				{
					sceneView.SetSceneViewShaderReplace(null, null);
					wireframeSceneviews.Remove(sceneView.name);
				}
			}
			
			sceneViewShown[sceneView] = show;
		}

		static List<SceneView> SortedSceneViews()
		{
			var list = from item in SceneView.sceneViews.Cast<SceneView>() orderby item.position.y,item.position.x select item;
			return list.ToList();
		}

		public const HelperSurfaceFlags DefaultHelperSurfaceFlags =	 HelperSurfaceFlags.ShowVisibleSurfaces;

		static public ToolEditMode			EditMode				= ToolEditMode.Generate;

		static public bool					LockAxisX				= false;
		static public bool					LockAxisY				= false;
		static public bool					LockAxisZ				= false;
		static public bool					SnapToGrid				= true;
		static public bool					UniformGrid				= true;
		static public bool					GridVisible				= true;
		static public HelperSurfaceFlags    VisibleHelperSurfaces	= DefaultHelperSurfaceFlags;
		static public Vector3				SnapVector				= MathConstants.oneVector3;
		static public float					SnapRotation			= 15.0f;
		static public float					SnapScale				= 0.1f;
//		static public float					SnapVertex				= 0.001f;
		static public DistanceUnit			DistanceUnit			= DistanceUnit.Meters;
		static public PixelUnit				PixelUnit				= PixelUnit.Relative;
		static public float                 DefaultShapeHeight      = 1.0f;
		static public uint					CurveSides				= 10;
		static public ClipMode              ClipMode                = ClipMode.RemovePositive;
		static public Material				DefaultMaterial			= MaterialUtility.WallMaterial;

		static public int					CircleSides				= 18;
		static public float					CircleOffset			= 0;
		static public bool					CircleSmoothShading		= true;
		static public bool					CircleSingleSurfaceEnds	= true;
		static public bool					CircleDistanceToSide	= true;
		static public bool					CircleRecenter			= true;

		static public int					SphereSplits			= 3;
		static public float					SphereOffset			= 0;
		static public bool					SphereSmoothShading		= true;
		static public bool					SphereDistanceToSide	= true;
		static public bool					HemiSphereMode			= true;

		static public float					LinearStairsStepLength		= 0.30f;
		static public float					LinearStairsStepHeight		= 0.20f;
		static public float					LinearStairsStepWidth		= 1.0f;
		static public float					LinearStairsLength			= 16.0f;
		static public float					LinearStairsHeight			= 16.0f;
		static public float					LinearStairsLengthOffset	= 0.0f;
		static public float					LinearStairsHeightOffset	= 0.0f;
		static public StairsBottom          LinearStairsBottom          = StairsBottom.Filled;
		static public int					LinearStairsTotalSteps		= 4;

		static public bool					HiddenSurfacesNotSelectable		= true;
		static public bool					HiddenSurfacesOrthoSelectable	= true;

		static public Vector3				DefaultMoveOffset		= Vector3.zero;
		static public Vector3				DefaultRotateOffset		= Vector3.zero;

		static System.Type						UnitySnapType				= null;
		static System.Reflection.PropertyInfo   UnitySnapTypeMoveProperty	= null;
		static System.Reflection.PropertyInfo   UnitySnapTypeRotateProperty = null;
		
		static public bool					EnableRealtimeCSG				= true;

		internal static ShapeMode			ShapeBuildMode	= ShapeMode.Box;


		static public void SetRealtimeCSGEnabled(bool isEnabled)
		{
			RealtimeCSG.CSGSettings.EnableRealtimeCSG = isEnabled;
			if (!RealtimeCSG.CSGSettings.EnableRealtimeCSG)
			{
				Tools.hidden = false;
				SceneViewEventHandler.ShowGrid = CSGSettings.GridVisible;
			} else
				CSGSettings.GridVisible = SceneViewEventHandler.ShowGrid;
			RealtimeCSG.CSGSettings.UpdateWireframeModes();
			RealtimeCSG.CSGSettings.Save();
		}
		

		#region UpdateSnapSettings
		internal static void UpdateSnapSettings()
		{
			// get our snapping values from the EditorPrefs ...

			var moveSnapVector  = SnapVector;
			var rotationSnap	= SnapRotation;

			List<System.Type> types = null;

			// ... but unfortunately Unity also caches it internally 
			// and doesn't expose it to us, so we're forced to use reflection ...
			if (UnitySnapType == null)
			{
				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				types = new List<System.Type>();
				foreach (var assembly in assemblies)
				{
					try
					{
						types.AddRange(assembly.GetTypes());
					}
					catch { }
				}
			}

			if (UnitySnapType == null)
			{ 
				UnitySnapType = types
					.FirstOrDefault(t => t.FullName == "UnityEditor.SnapSettings");
				if (UnitySnapType != null)
				{
					UnitySnapTypeMoveProperty   = UnitySnapType.GetProperty("move", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
					UnitySnapTypeRotateProperty = UnitySnapType.GetProperty("rotate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				} else
				{
					UnitySnapTypeMoveProperty   = null;
					UnitySnapTypeRotateProperty = null;
				}
			}
			if (UnitySnapType != null)
			{
				if (UnitySnapTypeMoveProperty != null)
					UnitySnapTypeMoveProperty.SetValue(UnitySnapType, moveSnapVector, null);

				if (UnitySnapTypeRotateProperty != null)
					UnitySnapTypeRotateProperty.SetValue(UnitySnapType, rotationSnap, null);
			}
		}
		#endregion

		static HashSet<string> GetKnownSceneviewNames()
		{
			var knownNames = new HashSet<string>();
			for (int i = 0; i < SceneView.sceneViews.Count; i++)
			{
				var sceneView = SceneView.sceneViews[i] as SceneView;
				if (!sceneView || string.IsNullOrEmpty(sceneView.name))
					continue;
				knownNames.Add(sceneView.name);
			}
			return knownNames;
		}

		static string GetUniqueSceneviewName(HashSet<string> knownNames)
		{
			int count = 1;
			string name;
			do
			{
				name = "View" + count;
				count++;
			} while (knownNames.Contains(name));
			return name;
		}

		static void EnsureValidSceneviewNames(List<SceneView> sceneViews)
		{
			for (int i = 0; i < sceneViews.Count; i++)
			{
				var sceneView = sceneViews[i];
				var name = sceneView.name;
				if (name == null || name.Length == 0)
					continue;
				name = name.Trim();
				if (name.Length == 0)
					continue;
				name = name.Replace(',', '_').Replace(':', '_');
			}
			var knownNames = GetKnownSceneviewNames();
			var foundNames = new HashSet<string>();
			for (int i = 0; i < sceneViews.Count; i++)
			{
				var sceneView = sceneViews[i];
				if (string.IsNullOrEmpty(sceneView.name) || foundNames.Contains(sceneView.name))
				{
					sceneView.name = GetUniqueSceneviewName(knownNames);
					knownNames.Add(sceneView.name);
					foundNames.Add(sceneView.name);
				}
			}
		}

		static void LoadWireframeSettings(List<SceneView> sceneViews, string arrayString)
		{
			wireframeSceneviews.Clear();
			var items = arrayString.Split(':');
			for (int i = 0; i < items.Length; i++)
			{
				var name = items[i];
				if (string.IsNullOrEmpty(name.Trim()))
					continue;
				wireframeSceneviews.Add(name.Trim());
			}
		}

		static void LegacyLoadWireframeSettings(List<SceneView> sceneViews, string arrayString)
		{
			var items		= arrayString.Split(',');
			var ids			= new int[items.Length];
			var enabled		= new bool[items.Length];
			for (int j=items.Length - 1;j>=0;j--)
			{
				var item = items[j];
				var sub_item = item.Split(':');

				var id			= 0;
				var is_enabled	= false;
				if (sub_item.Length != 2 ||
					!Int32.TryParse(sub_item[0], out id) ||
					!Boolean.TryParse(sub_item[1], out is_enabled))
				{
					ids[j] = 0;
					enabled[j] = false;
					continue;
				}
				ids[j]		= id;
				enabled[j]	= is_enabled;
			}

			wireframeSceneviews.Clear();
			var wireframeInstanceIDs = new int[items.Length];
			if (sceneViews.Count != items.Length)
			{
				wireframeInstanceIDs = new int[0];
				//Debug.Log("Number of scene-views have changed");
			} else
			{
				bool found_all = true;
				for (int j = 0; j < sceneViews.Count; j++)
				{
					if (!ArrayUtility.Contains(ids, sceneViews[j].GetInstanceID()))
					{
						found_all = false;
						break;
					}
				}

				if (found_all)
				{ 
					for (int j = ids.Length - 1; j >= 0; j--)
					{
						if (!enabled[j])
						{
							ArrayUtility.RemoveAt(ref wireframeInstanceIDs, j);
							continue;
						}
						wireframeInstanceIDs[j] = ids[j];
					}
				} else
				{
					//Debug.Log("Instance-ids of scene-views have changed, assuming order has not changed ...");
					for (int j = sceneViews.Count - 1; j >= 0; j--)
					{
						if (!enabled[j])
						{
							ArrayUtility.RemoveAt(ref wireframeInstanceIDs, j);
							continue;
						}
						wireframeInstanceIDs[j] = sceneViews[j].GetInstanceID();
					}
				}
			}

			for (int i = 0; i < sceneViews.Count; i++)
			{
				if (ArrayUtility.Contains(wireframeInstanceIDs, sceneViews[i].GetInstanceID()))
				{
					wireframeSceneviews.Add(sceneViews[i].name);
				}
			}
		}


		#region Reload
		public static void Reload()
		{
			RealtimeCSG.CSGSettings.LockAxisX			= EditorPrefs.GetBool("LockAxisX", false);
			RealtimeCSG.CSGSettings.LockAxisY			= EditorPrefs.GetBool("LockAxisY", false);
			RealtimeCSG.CSGSettings.LockAxisZ			= EditorPrefs.GetBool("LockAxisZ", false);

			RealtimeCSG.CSGSettings.UniformGrid			= EditorPrefs.GetBool("UniformGrid", true);
			RealtimeCSG.CSGSettings.EditMode			= (ToolEditMode)EditorPrefs.GetInt("EditMode", (int)ToolEditMode.Generate);
			
			var moveSnapVector  = new Vector3(EditorPrefs.GetFloat("MoveSnapX", 1.0f), 
											  EditorPrefs.GetFloat("MoveSnapY", 1.0f), 
											  EditorPrefs.GetFloat("MoveSnapZ", 1.0f));
			RealtimeCSG.CSGSettings.SnapVector			= moveSnapVector;

			var moveOffsetVector = new Vector3(EditorPrefs.GetFloat("DefaultMoveOffsetX", 0.0f),
											   EditorPrefs.GetFloat("DefaultMoveOffsetY", 0.0f),
											   EditorPrefs.GetFloat("DefaultMoveOffsetZ", 0.0f));
			RealtimeCSG.CSGSettings.DefaultMoveOffset = moveOffsetVector;

			var rotateOffsetVector = new Vector3(EditorPrefs.GetFloat("DefaultRotateOffsetX", 0.0f),
											     EditorPrefs.GetFloat("DefaultRotateOffsetY", 0.0f),
											     EditorPrefs.GetFloat("DefaultRotateOffsetZ", 0.0f));
			RealtimeCSG.CSGSettings.DefaultRotateOffset = rotateOffsetVector;

			RealtimeCSG.CSGSettings.ShapeBuildMode		= (ShapeMode)EditorPrefs.GetInt("ShapeBuildMode", (int)ShapeMode.Box);

			RealtimeCSG.CSGSettings.GridVisible			= EditorPrefs.GetBool("ShowGrid", true);			
			RealtimeCSG.CSGSettings.SnapToGrid			= EditorPrefs.GetBool("ForceSnapToGrid",    true);
			
			var helperSurfaces	= (HelperSurfaceFlags)EditorPrefs.GetInt("HelperSurfaces", (int)DefaultHelperSurfaceFlags);
			RealtimeCSG.CSGSettings.VisibleHelperSurfaces = helperSurfaces;
			
			RealtimeCSG.CSGSettings.ClipMode			= (ClipMode)EditorPrefs.GetInt("ClipMode", 0);
			RealtimeCSG.CSGSettings.EnableRealtimeCSG	= EditorPrefs.GetBool("EnableRealtimeCSG", true);

			var defaultMaterialGUID = EditorPrefs.GetString("DefaultMaterial", null);
			if (defaultMaterialGUID != null)
			{
				var defaultMaterialAssetPath = AssetDatabase.GUIDToAssetPath(defaultMaterialGUID);
				var materialFromID = AssetDatabase.LoadAssetAtPath<Material>(defaultMaterialAssetPath);
				if (materialFromID)
					DefaultMaterial = materialFromID;
				else
					DefaultMaterial = MaterialUtility.WallMaterial;
			} else 
				DefaultMaterial = MaterialUtility.WallMaterial;


			RealtimeCSG.CSGSettings.SnapScale			= EditorPrefs.GetFloat("ScaleSnap", 0.1f);
			RealtimeCSG.CSGSettings.SnapRotation		= EditorPrefs.GetFloat("RotationSnap", 15.0f);
			RealtimeCSG.CSGSettings.DefaultShapeHeight	= EditorPrefs.GetFloat("DefaultShapeHeight", 1.0f);
			RealtimeCSG.CSGSettings.CurveSides			= (uint)EditorPrefs.GetInt("CurveSides", 10);
			 

			CircleSides					= Mathf.Max(3, EditorPrefs.GetInt("CircleSides", 18));
			CircleOffset				= EditorPrefs.GetFloat("CircleOffset",				0);
			CircleSmoothShading			= EditorPrefs.GetBool("CircleSmoothShading",		true);
			CircleSingleSurfaceEnds		= EditorPrefs.GetBool("CircleSingleSurfaceEnds",	true);
			CircleDistanceToSide		= EditorPrefs.GetBool("CircleDistanceToSide",		true);
			CircleRecenter				= EditorPrefs.GetBool("CircleRecenter",				true);
			

			SphereSplits				= Mathf.Max(1, EditorPrefs.GetInt("SphereSplits",	1));
			SphereOffset				= EditorPrefs.GetFloat("SphereOffset",				0);
			SphereSmoothShading			= EditorPrefs.GetBool("SphereSmoothShading",		true);
			SphereDistanceToSide		= EditorPrefs.GetBool("SphereDistanceToSide",		true);
			HemiSphereMode				= EditorPrefs.GetBool("HemiSphereMode",			false); 
			
			LinearStairsStepLength		= EditorPrefs.GetFloat("LinearStairsStepLength",	0.30f);
			LinearStairsStepHeight		= EditorPrefs.GetFloat("LinearStairsStepHeight",	0.20f);
			LinearStairsStepWidth		= EditorPrefs.GetFloat("LinearStairsStepWidth",		1.0f);
			LinearStairsTotalSteps		= EditorPrefs.GetInt("LinearStairsTotalSteps",		4);

			LinearStairsLength			= EditorPrefs.GetFloat("LinearStairsLength",		16.0f);
			LinearStairsHeight			= EditorPrefs.GetFloat("LinearStairsHeight",		16.0f);
			LinearStairsLengthOffset	= EditorPrefs.GetFloat("LinearStairsLengthOffset",	0.0f);
			LinearStairsHeightOffset	= EditorPrefs.GetFloat("LinearStairsHeightOffset",	0.0f);

			 var distanceUnit			= (DistanceUnit)EditorPrefs.GetInt("DistanceUnit", (int)DistanceUnit.Meters);
			RealtimeCSG.CSGSettings.DistanceUnit	= distanceUnit;
			
			var sceneViews = SortedSceneViews();
			EnsureValidSceneviewNames(sceneViews);
			var arrayString = EditorPrefs.GetString("Wireframe", string.Empty);
			if (arrayString.Contains(','))
			{
				LegacyLoadWireframeSettings(sceneViews, arrayString);
			} else
			{
				LoadWireframeSettings(sceneViews, arrayString);
			}

			UpdateSnapSettings();
		}
		#endregion

		#region Save
		public static void Save()
		{
			EditorPrefs.SetBool ("LockAxisX",			RealtimeCSG.CSGSettings.LockAxisX);
			EditorPrefs.SetBool ("LockAxisY",			RealtimeCSG.CSGSettings.LockAxisY);
			EditorPrefs.SetBool ("LockAxisZ",			RealtimeCSG.CSGSettings.LockAxisZ);

			EditorPrefs.SetBool ("ShowGrid",			RealtimeCSG.CSGSettings.GridVisible);
			EditorPrefs.SetBool ("UniformGrid",			RealtimeCSG.CSGSettings.UniformGrid);

			EditorPrefs.SetInt  ("EditMode",			(int)RealtimeCSG.CSGSettings.EditMode);

			EditorPrefs.SetFloat("MoveSnapX",			RealtimeCSG.CSGSettings.SnapVector.x);
			EditorPrefs.SetFloat("MoveSnapY",			RealtimeCSG.CSGSettings.SnapVector.y);
			EditorPrefs.SetFloat("MoveSnapZ",			RealtimeCSG.CSGSettings.SnapVector.z);

			EditorPrefs.SetFloat("DefaultMoveOffsetX", RealtimeCSG.CSGSettings.DefaultMoveOffset.x);
			EditorPrefs.SetFloat("DefaultMoveOffsetY", RealtimeCSG.CSGSettings.DefaultMoveOffset.y);
			EditorPrefs.SetFloat("DefaultMoveOffsetZ", RealtimeCSG.CSGSettings.DefaultMoveOffset.z);

			EditorPrefs.SetFloat("DefaultRotateOffsetX", RealtimeCSG.CSGSettings.DefaultRotateOffset.x);
			EditorPrefs.SetFloat("DefaultRotateOffsetY", RealtimeCSG.CSGSettings.DefaultRotateOffset.y);
			EditorPrefs.SetFloat("DefaultRotateOffsetZ", RealtimeCSG.CSGSettings.DefaultRotateOffset.z);

			EditorPrefs.SetFloat("ScaleSnap",			RealtimeCSG.CSGSettings.SnapScale);
			EditorPrefs.SetFloat("RotationSnap",		RealtimeCSG.CSGSettings.SnapRotation);
			EditorPrefs.SetFloat("DefaultShapeHeight",	RealtimeCSG.CSGSettings.DefaultShapeHeight);
			EditorPrefs.SetInt  ("CurveSides",			(int)RealtimeCSG.CSGSettings.CurveSides);
			EditorPrefs.SetInt  ("ClipMode",			(int)RealtimeCSG.CSGSettings.ClipMode);
			EditorPrefs.SetBool	("EnableRealtimeCSG",	RealtimeCSG.CSGSettings.EnableRealtimeCSG);

			EditorPrefs.SetBool	("ForceSnapToGrid",		RealtimeCSG.CSGSettings.SnapToGrid);			
			EditorPrefs.SetInt  ("HelperSurfaces",		(int)RealtimeCSG.CSGSettings.VisibleHelperSurfaces);
			EditorPrefs.SetInt  ("DistanceUnit",		(int)RealtimeCSG.CSGSettings.DistanceUnit);
			
			EditorPrefs.SetInt  ("ShapeBuildMode",		(int)RealtimeCSG.CSGSettings.ShapeBuildMode);
			
			EditorPrefs.SetInt	("CircleSides",				Mathf.Max(3, CircleSides));
			EditorPrefs.SetFloat("CircleOffset",			CircleOffset);
			EditorPrefs.SetBool("CircleSmoothShading",		CircleSmoothShading);
			EditorPrefs.SetBool("CircleSingleSurfaceEnds",	CircleSingleSurfaceEnds);
			EditorPrefs.SetBool("CircleDistanceToSide",		CircleDistanceToSide);
			EditorPrefs.SetBool("CircleRecenter",			CircleRecenter);
			
			EditorPrefs.SetInt("SphereSplits",				Mathf.Max(1, SphereSplits));
			EditorPrefs.SetFloat("SphereOffset",			SphereOffset);
			EditorPrefs.SetBool("SphereSmoothShading",		SphereSmoothShading);
			EditorPrefs.SetBool("SphereDistanceToSide",		SphereDistanceToSide);
			EditorPrefs.SetBool("HemiSphereMode",			HemiSphereMode);

			EditorPrefs.SetFloat("LinearStairsStepLength",	LinearStairsStepLength);
			EditorPrefs.SetFloat("LinearStairsStepHeight",	LinearStairsStepHeight);
			EditorPrefs.SetFloat("LinearStairsStepWidth",	LinearStairsStepWidth);
			EditorPrefs.SetInt("LinearStairsTotalSteps",	LinearStairsTotalSteps);
			EditorPrefs.SetFloat("LinearStairsLength",		LinearStairsLength);
			EditorPrefs.SetFloat("LinearStairsHeight",		LinearStairsHeight);
			EditorPrefs.SetFloat("LinearStairsLengthOffset", LinearStairsLengthOffset);
			EditorPrefs.SetFloat("LinearStairsHeightOffset", LinearStairsHeightOffset);

			if (!DefaultMaterial)
				DefaultMaterial = MaterialUtility.WallMaterial;
			var defaultMaterialAssetPath = AssetDatabase.GetAssetPath(DefaultMaterial);
			var defaultMaterialGUID	     = string.IsNullOrEmpty(defaultMaterialAssetPath) ? null : AssetDatabase.AssetPathToGUID(defaultMaterialAssetPath);
			EditorPrefs.SetString("DefaultMaterial", defaultMaterialGUID);


			var builder = new System.Text.StringBuilder();
			var sceneViews = SortedSceneViews(); 
			EnsureValidSceneviewNames(sceneViews);
			foreach(SceneView sceneView in sceneViews)
			{
				if (IsWireframeShown(sceneView))
				{ 
					if (builder.Length != 0) builder.Append(':');
					builder.Append(sceneView.name);
				}
			}
			EditorPrefs.SetString("Wireframe", builder.ToString());
		}
		#endregion

	}
}