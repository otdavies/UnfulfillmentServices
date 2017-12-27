using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	internal static class ModelInspectorGUI
	{
		private static bool _showDetails = false;

		private class MeshData
		{
			public int	VertexCount;
			public int	TriangleCount;
			public Mesh Mesh;
		}

		private static readonly GUIContent CastShadows							= new GUIContent("Cast Shadows", "Only opaque materials cast shadows");
		private static readonly GUIContent ReceiveShadowsContent				= new GUIContent("Receive Shadows", "Only opaque materials receive shadows (is always on in deferred mode)");
		private static readonly GUIContent GenerateColliderContent				= new GUIContent("Generate Collider");
		private static readonly GUIContent ModelIsTriggerContent				= new GUIContent("Model Is Trigger");
		private static readonly GUIContent ColliderSetToConvexContent			= new GUIContent("Convex Collider", "Set generated collider to convex");
		private static readonly GUIContent ColliderAutoRigidBodyContent			= new GUIContent("Auto RigidBody", "When enabled the model automatically updates the Rigidbody settings, creates it when needed, destroys it when not needed.");
		private static readonly GUIContent DefaultPhysicsMaterialContent		= new GUIContent("Default Physics Material");
		private static readonly GUIContent InvertedWorldContent					= new GUIContent("Inverted world", "World is solid by default when checked, otherwise default is empty");
		private static readonly GUIContent DoNotRenderContent					= new GUIContent("Do Not Render");
		
		private static readonly GUIContent AutoRebuildUVsContent				= new GUIContent("Auto Rebuild UVs", "Automatically regenerate lightmap UVs when the model has been modified. This might introduce hitches when modifying geometry.");
		private static readonly GUIContent PreserveUVsContent                   = new GUIContent("Preserve UVs", "Preserve the incoming lightmap UVs when generating realtime GI UVs. The incoming UVs are packed but charts are not scaled or merged. This is necessary for correct edge stitching of axis aligned chart edges.");
		private static readonly GUIContent ShowGeneratedMeshesContent			= new GUIContent("Show Meshes", "Select to show the generated Meshes in the hierarchy");

		private static readonly GUIContent VertexChannelColorContent			= new GUIContent("Color channel");
		private static readonly GUIContent VertexChannelTangentContent			= new GUIContent("Tangent channel");
		private static readonly GUIContent VertexChannelNormalContent			= new GUIContent("Normal channel");
		private static readonly GUIContent VertexChannelUV1Content				= new GUIContent("UV1 channel");


		#region Workarounds 
		private static Type				_probesType;
		private static System.Object	_probesInstance;
		private static MethodInfo		_probesInitializeMethod;
		private static MethodInfo		_probesOnGUIMethod;
		private static MethodInfo       _sceneViewIsUsingDeferredRenderingPath;
		private static bool				_haveReflected = false;

		private static void InitReflection() // le *sigh*
		{
			if (_haveReflected)
				return;

			_haveReflected = true;
			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			var types = new List<System.Type>();
			foreach(var assembly in assemblies)
			{
				try
				{
					types.AddRange(assembly.GetTypes());
				}
				catch { }
			}

			_probesType 		= types.FirstOrDefault(t => t.FullName == "UnityEditor.RendererEditorBase+Probes");
			_probesInstance	= Activator.CreateInstance(_probesType);

			var methods = _probesType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
			for (int i = 0; i < methods.Length; i++)
			{
				//internal void Initialize(SerializedObject serializedObject)
				if (methods[i].Name == "Initialize")
				{
					if (methods[i].GetParameters().Length == 1)
						_probesInitializeMethod = methods[i];
				} else
					//internal void OnGUI(UnityEngine.Object[] selection, Renderer renderer, bool useMiniStyle)
				if (methods[i].Name == "OnGUI")
				{
					if (methods[i].GetParameters().Length == 3)
						_probesOnGUIMethod = methods[i];
				}
			}
			
			methods = typeof(SceneView).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "IsUsingDeferredRenderingPath")
				{
					_sceneViewIsUsingDeferredRenderingPath = methods[i];
				}
			}
		}



		internal static Camera GetMainCamera()
		{
			// main camera, if we have any
			var mainCamera = Camera.main;
			if (mainCamera != null)
				return mainCamera;

			// if we have one camera, return it
			Camera[] allCameras = Camera.allCameras;
			if (allCameras != null && allCameras.Length == 1)
				return allCameras[0];

			// otherwise no "main" camera
			return null;
		}

		internal static RenderingPath GetSceneViewRenderingPath()
		{
			var mainCamera = GetMainCamera ();
			if (mainCamera != null)
				return mainCamera.renderingPath;
			return RenderingPath.UsePlayerSettings;
		}

		internal static bool IsUsingDeferredRenderingPath()
		{
			if (_sceneViewIsUsingDeferredRenderingPath != null)
			{
				var ret = _sceneViewIsUsingDeferredRenderingPath.Invoke(null, null);
				return (bool) ret;
			} else
				return false;
		}
		#endregion

		private static bool					_probesInitialized = false;
		private static UnityEngine.Object[] _probesTargets = null;
		private static SerializedObject		_probesSerializedObject;
		
		static void UpdateTargets(CSGModel[] models)
		{
			_probesInitialized = false;
			_probesTargets = null;
			_probesSerializedObject = null;
			if (models.Length == 0)
				return;
			
			_probesTargets = MeshInstanceManager.FindRenderers(models);
			if (_probesTargets.Length == 0 || _probesTargets == null)
				return;

			_probesSerializedObject = new SerializedObject(_probesTargets);

			InitReflection();
			if (_probesInstance != null &&
				_probesInitializeMethod != null && 
				_probesOnGUIMethod != null)
			{
				if (_probesTargets.Length > 0)
				{
					_probesInitializeMethod.Invoke(_probesInstance, new System.Object[] { _probesSerializedObject });
					_probesInitialized = true;
				}
			}
		}

		static GUIStyle popupStyle;

		static bool localStyles = false;

		public static void OnInspectorGUI(UnityEngine.Object[] targets)
		{
			InitReflection();
			if (!localStyles)
			{
				popupStyle = new GUIStyle(EditorStyles.popup);
				//popupStyle.padding.top += 2;
				popupStyle.margin.top += 2;
				localStyles = true;
			}


			bool updateMeshes = false;

			var models = new CSGModel[targets.Length];

			for (int i = targets.Length - 1; i >= 0; i--)
			{
				models[i] = targets[i] as CSGModel;
				if (!models[i])
				{
					ArrayUtility.RemoveAt(ref models, i);
				}
			}

			if (models.Length == 0)
				return;

			
			var settings		= models[0].Settings;
			var vertexChannels	= models[0].VertexChannels;
			ExportType? exportType  = models[0].exportType;
			bool? VertexChannelColor		= (vertexChannels & VertexChannelFlags.Color) == VertexChannelFlags.Color;
			bool? VertexChannelTangent		= (vertexChannels & VertexChannelFlags.Tangent) == VertexChannelFlags.Tangent;
			bool? VertexChannelNormal		= (vertexChannels & VertexChannelFlags.Normal) == VertexChannelFlags.Normal;
			bool? VertexChannelUV0			= (vertexChannels & VertexChannelFlags.UV0) == VertexChannelFlags.UV0;
			bool? InvertedWorld				= (settings & ModelSettingsFlags.InvertedWorld) == ModelSettingsFlags.InvertedWorld;
			bool? NoCollider				= (settings & ModelSettingsFlags.NoCollider) == ModelSettingsFlags.NoCollider;
			bool? IsTrigger					= (settings & ModelSettingsFlags.IsTrigger) == ModelSettingsFlags.IsTrigger;
			bool? SetToConvex				= (settings & ModelSettingsFlags.SetColliderConvex) == ModelSettingsFlags.SetColliderConvex;
			bool? AutoGenerateRigidBody		= (settings & ModelSettingsFlags.AutoUpdateRigidBody) == ModelSettingsFlags.AutoUpdateRigidBody;
			bool? DoNotRender				= (settings & ModelSettingsFlags.DoNotRender) == ModelSettingsFlags.DoNotRender;
			bool? ReceiveShadows			= !((settings & ModelSettingsFlags.DoNotReceiveShadows) == ModelSettingsFlags.DoNotReceiveShadows);
			bool? AutoRebuildUVs            = (settings & ModelSettingsFlags.AutoRebuildUVs) == ModelSettingsFlags.AutoRebuildUVs;
			bool? PreserveUVs               = (settings & ModelSettingsFlags.PreserveUVs) == ModelSettingsFlags.PreserveUVs;
			bool? ShowGeneratedMeshes		= models[0].ShowGeneratedMeshes;
			ShadowCastingMode? ShadowCastingMode = (ShadowCastingMode)(settings & ModelSettingsFlags.ShadowCastingModeFlags);
			var	defaultPhysicsMaterial		= models[0].DefaultPhysicsMaterial;
			var	defaultPhysicsMaterialMixed = false;
			
			for (int i = 1; i< models.Length; i++)
			{
				settings		= models[i].Settings;
				vertexChannels	= models[i].VertexChannels;
				ExportType currExportType		= models[i].exportType;
				bool currVertexChannelColor		= (vertexChannels & VertexChannelFlags.Color) == VertexChannelFlags.Color;
				bool currVertexChannelTangent	= (vertexChannels & VertexChannelFlags.Tangent) == VertexChannelFlags.Tangent;
				bool currVertexChannelNormal	= (vertexChannels & VertexChannelFlags.Normal) == VertexChannelFlags.Normal;
				bool currVertexChannelUV0		= (vertexChannels & VertexChannelFlags.UV0) == VertexChannelFlags.UV0;
				bool currInvertedWorld			= (settings & ModelSettingsFlags.InvertedWorld) == ModelSettingsFlags.InvertedWorld;
				bool currNoCollider				= (settings & ModelSettingsFlags.NoCollider) == ModelSettingsFlags.NoCollider;
				bool currIsTrigger				= (settings & ModelSettingsFlags.IsTrigger) == ModelSettingsFlags.IsTrigger;
				bool currSetToConvex			= (settings & ModelSettingsFlags.SetColliderConvex) == ModelSettingsFlags.SetColliderConvex;
				bool currAutoGenerateRigidBody	= (settings & ModelSettingsFlags.AutoUpdateRigidBody) == ModelSettingsFlags.AutoUpdateRigidBody;
				bool currDoNotRender			= (settings & ModelSettingsFlags.DoNotRender) == ModelSettingsFlags.DoNotRender;
				bool currReceiveShadows			= !((settings & ModelSettingsFlags.DoNotReceiveShadows) == ModelSettingsFlags.DoNotReceiveShadows);
				bool currAutoRebuildUVs         = (settings & ModelSettingsFlags.AutoRebuildUVs) == ModelSettingsFlags.AutoRebuildUVs;
				bool currPreserveUVs            = (settings & ModelSettingsFlags.PreserveUVs) == ModelSettingsFlags.PreserveUVs;
				bool currShowGeneratedMeshes	= models[i].ShowGeneratedMeshes;
				var	 currdefaultPhysicsMaterial	= models[i].DefaultPhysicsMaterial;
				ShadowCastingMode currShadowCastingMode = (ShadowCastingMode)(settings & ModelSettingsFlags.ShadowCastingModeFlags);

				if (VertexChannelColor		.HasValue && VertexChannelColor		.Value != currVertexChannelColor	) VertexChannelColor = null;
				if (VertexChannelTangent	.HasValue && VertexChannelTangent	.Value != currVertexChannelTangent	) VertexChannelTangent = null;
				if (VertexChannelNormal		.HasValue && VertexChannelNormal	.Value != currVertexChannelNormal	) VertexChannelNormal = null;
				if (VertexChannelUV0	    .HasValue && VertexChannelUV0		.Value != currVertexChannelUV0		) VertexChannelUV0 = null;
				
				if (exportType				.HasValue && exportType				.Value != currExportType			) exportType = null;
				
				if (InvertedWorld			.HasValue && InvertedWorld			.Value != currInvertedWorld			) InvertedWorld = null;
				if (NoCollider				.HasValue && NoCollider				.Value != currNoCollider			) NoCollider = null;
				if (IsTrigger				.HasValue && IsTrigger				.Value != currIsTrigger				) IsTrigger = null;
				if (SetToConvex				.HasValue && SetToConvex		    .Value != currSetToConvex			) SetToConvex = null;
				if (AutoGenerateRigidBody	.HasValue && AutoGenerateRigidBody	.Value != currAutoGenerateRigidBody ) AutoGenerateRigidBody = null;
				if (DoNotRender				.HasValue && DoNotRender		    .Value != currDoNotRender			) DoNotRender = null;
				if (ReceiveShadows			.HasValue && ReceiveShadows			.Value != currReceiveShadows		) ReceiveShadows = null;
				if (ShadowCastingMode		.HasValue && ShadowCastingMode		.Value != currShadowCastingMode		) ShadowCastingMode = null;
				if (AutoRebuildUVs     		.HasValue && AutoRebuildUVs     	.Value != currAutoRebuildUVs		) AutoRebuildUVs = null;
				if (PreserveUVs     		.HasValue && PreserveUVs     		.Value != currPreserveUVs	    	) PreserveUVs = null;
				if (ShowGeneratedMeshes		.HasValue && ShowGeneratedMeshes	.Value != currShowGeneratedMeshes	) ShowGeneratedMeshes = null;

				if (defaultPhysicsMaterial != currdefaultPhysicsMaterial) defaultPhysicsMaterialMixed = true;
			}

			GUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Behaviour");
				EditorGUI.indentLevel++;
				{
					bool inverted_world = InvertedWorld.HasValue ? InvertedWorld.Value : false;
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = !InvertedWorld.HasValue;
						inverted_world = EditorGUILayout.Toggle(InvertedWorldContent, inverted_world);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							if (inverted_world)	models[i].Settings |=  ModelSettingsFlags.InvertedWorld;
							else				models[i].Settings &= ~ModelSettingsFlags.InvertedWorld;
						}
						GUI.changed = true;
						InvertedWorld = inverted_world;
					}
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
			if (models != null && models.Length == 1)
			{
				GUILayout.Space(10);
				GUILayout.BeginVertical(GUI.skin.box);
				{
					EditorGUILayout.LabelField("Export");
					GUILayout.BeginHorizontal();
					{
						EditorGUI.BeginDisabledGroup(!exportType.HasValue);
						{
							if (GUILayout.Button("Export to ...") && exportType.HasValue)
							{
#if !DEMO
								MeshInstanceManager.Export(models[0], exportType.Value);
#else
								Debug.LogWarning("Export is disabled in demo version");
#endif
							}
						}
						EditorGUI.EndDisabledGroup();
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = !exportType.HasValue;
							exportType = (ExportType)EditorGUILayout.EnumPopup(exportType ?? ExportType.FBX, popupStyle);
							EditorGUI.showMixedValue = false;
						}
						if (EditorGUI.EndChangeCheck() && exportType.HasValue)
						{
							for (int i = 0; i < models.Length; i++)
							{
								models[i].exportType = exportType.Value;
							}
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.Space(10);
			GUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Physics");
				EditorGUI.indentLevel++;
				{ 
					bool collider_value = NoCollider.HasValue ? NoCollider.Value : false;
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = !NoCollider.HasValue;
						collider_value = !EditorGUILayout.Toggle(GenerateColliderContent, !collider_value);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							if (collider_value)	models[i].Settings |=  ModelSettingsFlags.NoCollider;
							else				models[i].Settings &= ~ModelSettingsFlags.NoCollider;
						}
						GUI.changed = true;
						NoCollider = collider_value;
						updateMeshes = true;
					}
				}
				var have_no_collider = NoCollider.HasValue && NoCollider.Value;
				EditorGUI.BeginDisabledGroup(have_no_collider);
				{
					bool trigger_value_mixed = have_no_collider ? true : !IsTrigger.HasValue;
					bool trigger_value = IsTrigger.HasValue ? IsTrigger.Value : false;
					{
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = trigger_value_mixed;
							trigger_value = EditorGUILayout.Toggle(ModelIsTriggerContent, trigger_value);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								if (trigger_value) models[i].Settings |= ModelSettingsFlags.IsTrigger;
								else models[i].Settings &= ~ModelSettingsFlags.IsTrigger;
							}
							GUI.changed = true;
							IsTrigger = trigger_value;
							updateMeshes = true;
						}
					}
					bool set_convex_value_mixed = have_no_collider ? true : !SetToConvex.HasValue;
					bool set_convex_value = have_no_collider ? false : (SetToConvex.HasValue ? SetToConvex.Value : false);
					{ 
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = set_convex_value_mixed;
							var prevColor = GUI.color;
							if (!set_convex_value && trigger_value)
							{
								var color = new Color(1, 0.25f, 0.25f);
								GUI.color = color;
							}
							set_convex_value = EditorGUILayout.Toggle(ColliderSetToConvexContent, set_convex_value);
							GUI.color = prevColor;
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								if (set_convex_value) models[i].Settings |=  ModelSettingsFlags.SetColliderConvex;
								else				  models[i].Settings &= ~ModelSettingsFlags.SetColliderConvex;
							}
							GUI.changed = true;
							SetToConvex = set_convex_value;
							updateMeshes = true;
						}
					}
					{
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = defaultPhysicsMaterialMixed;
							GUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel(DefaultPhysicsMaterialContent);
							defaultPhysicsMaterial = EditorGUILayout.ObjectField(defaultPhysicsMaterial, typeof(PhysicMaterial), true) as PhysicMaterial;
							GUILayout.EndHorizontal();
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								models[i].DefaultPhysicsMaterial = defaultPhysicsMaterial;
							}
							GUI.changed = true;
							//MeshInstanceManager.Clear();
							updateMeshes = true;
						}
					}
					if (!have_no_collider && !set_convex_value && trigger_value)
					{
						var prevColor = GUI.color;
						var color = new Color(1, 0.25f, 0.25f);
						GUI.color = color;
						GUILayout.Label("Warning:\r\nFor performance reasons colliders need to\r\nbe convex!");
					
						GUI.color = prevColor;
					}
				}
				EditorGUI.EndDisabledGroup();
				{
					bool autoRigidbody = (AutoGenerateRigidBody.HasValue ? AutoGenerateRigidBody.Value : false);
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = !AutoGenerateRigidBody.HasValue;
						autoRigidbody = !EditorGUILayout.Toggle(ColliderAutoRigidBodyContent, !autoRigidbody);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							if (autoRigidbody) models[i].Settings |= ModelSettingsFlags.AutoUpdateRigidBody;
							else models[i].Settings &= ~ModelSettingsFlags.AutoUpdateRigidBody;
						}
						GUI.changed = true;
						AutoGenerateRigidBody = autoRigidbody;
					}
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.BeginVertical(GUI.skin.box);
			{
				ShadowCastingMode shadowcastingValue = ShadowCastingMode.HasValue ? ShadowCastingMode.Value : UnityEngine.Rendering.ShadowCastingMode.On;
				var castOnlyShadow = (shadowcastingValue == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
				EditorGUILayout.LabelField("Rendering");
				EditorGUI.indentLevel++;
				EditorGUI.BeginDisabledGroup(castOnlyShadow);
				{ 
					bool donotrender_value = castOnlyShadow ? true : (DoNotRender.HasValue ? DoNotRender.Value : false);
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = castOnlyShadow ? true : !DoNotRender.HasValue;
						donotrender_value = EditorGUILayout.Toggle(DoNotRenderContent, donotrender_value);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							if (donotrender_value) models[i].Settings |=  ModelSettingsFlags.DoNotRender;
							else				   models[i].Settings &= ~ModelSettingsFlags.DoNotRender;
						}
						GUI.changed = true;
						DoNotRender = donotrender_value;
						updateMeshes = true;
					}
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.Space(10);
				EditorGUI.BeginDisabledGroup(DoNotRender.HasValue && DoNotRender.Value);
				{
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = !ShadowCastingMode.HasValue;						
						shadowcastingValue = (ShadowCastingMode)EditorGUILayout.EnumPopup(CastShadows, shadowcastingValue);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							settings = models[i].Settings;
							settings &= ~ModelSettingsFlags.ShadowCastingModeFlags;
							settings |= (ModelSettingsFlags)(((int)shadowcastingValue) & (int)ModelSettingsFlags.ShadowCastingModeFlags);
							models[i].Settings = settings;
						}
						GUI.changed = true;
						ShadowCastingMode = shadowcastingValue;
						updateMeshes = true;
					}

					var isUsingDeferredRenderingPath = false;//IsUsingDeferredRenderingPath();
					EditorGUI.BeginDisabledGroup(castOnlyShadow || isUsingDeferredRenderingPath);
					{
						var receiveshadowsValue = !castOnlyShadow && (isUsingDeferredRenderingPath || (ReceiveShadows ?? false));
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = (castOnlyShadow || !ReceiveShadows.HasValue) && !isUsingDeferredRenderingPath;
							receiveshadowsValue = EditorGUILayout.Toggle(ModelInspectorGUI.ReceiveShadowsContent, receiveshadowsValue || isUsingDeferredRenderingPath);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								if (receiveshadowsValue) models[i].Settings &= ~ModelSettingsFlags.DoNotReceiveShadows;
								else                     models[i].Settings |=  ModelSettingsFlags.DoNotReceiveShadows;
							}
							GUI.changed = true;
							ReceiveShadows = receiveshadowsValue;
						}
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(castOnlyShadow);
				EditorGUI.showMixedValue = false;
				UpdateTargets(models);
				if (_probesInstance != null &&
					_probesOnGUIMethod != null && 
					_probesTargets != null &&
					_probesSerializedObject != null &&
					_probesInitialized)
				{
					GUILayout.Space(10);
					try
					{
#if UNITY_5_6_OR_NEWER
						_probesSerializedObject.UpdateIfRequiredOrScript();
#else
						_probesSerializedObject.UpdateIfDirtyOrScript();
#endif
						_probesOnGUIMethod.Invoke(_probesInstance, new System.Object[] { _probesTargets, (Renderer)_probesTargets[0], false });
						_probesSerializedObject.ApplyModifiedProperties();
					}
					catch { }
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Lighting");
				EditorGUI.indentLevel++;
				{
					EditorGUI.indentLevel++;
					CommonGUI.GenerateLightmapUVButton(models);
					EditorGUI.indentLevel--;

					EditorGUILayout.LabelField("UV Settings");
					EditorGUI.indentLevel++;
					{
						var autoRebuildUvs = AutoRebuildUVs ?? false;
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = !AutoRebuildUVs.HasValue;
							autoRebuildUvs = EditorGUILayout.Toggle(AutoRebuildUVsContent, autoRebuildUvs);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								if (autoRebuildUvs)
									models[i].Settings |= ModelSettingsFlags.AutoRebuildUVs;
								else
									models[i].Settings &= ~ModelSettingsFlags.AutoRebuildUVs;
							}
							GUI.changed = true;
							AutoRebuildUVs = autoRebuildUvs;
							updateMeshes = true;
						}
					}
					{
						var preserveUVs = PreserveUVs ?? false;
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = !PreserveUVs.HasValue;
							preserveUVs = EditorGUILayout.Toggle(PreserveUVsContent, preserveUVs);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								if (preserveUVs)
									models[i].Settings |= ModelSettingsFlags.PreserveUVs;
								else
									models[i].Settings &= ~ModelSettingsFlags.PreserveUVs;
							}
							GUI.changed = true;
							PreserveUVs = preserveUVs;
							updateMeshes = true;
						}
					}
					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.LabelField("Mesh (advanced)");
				EditorGUI.indentLevel++;
				{
					var showGeneratedMeshes = ShowGeneratedMeshes ?? false;
					EditorGUI.BeginChangeCheck();
					{
						EditorGUI.showMixedValue = !ShowGeneratedMeshes.HasValue;
						showGeneratedMeshes = EditorGUILayout.Toggle(ShowGeneratedMeshesContent, showGeneratedMeshes);
					}
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < models.Length; i++)
						{
							models[i].ShowGeneratedMeshes = showGeneratedMeshes;
							MeshInstanceManager.UpdateGeneratedMeshesVisibility(models[i]);
						}
					}
					GUILayout.Space(10);

					EditorGUILayout.LabelField("Used Vertex Channels");
					EditorGUI.indentLevel++;
					{
						var vertex_channel_color	= VertexChannelColor ?? false;
						var vertex_channel_tangent	= VertexChannelTangent ?? false;
						var vertex_channel_normal	= VertexChannelNormal ?? false;
						var vertex_channel_UV0		= VertexChannelUV0 ?? false;
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = !VertexChannelColor.HasValue;
							vertex_channel_color = EditorGUILayout.Toggle(VertexChannelColorContent, vertex_channel_color);
						
							EditorGUI.showMixedValue = !VertexChannelTangent.HasValue;
							vertex_channel_tangent = EditorGUILayout.Toggle(VertexChannelTangentContent, vertex_channel_tangent);
						
							EditorGUI.showMixedValue = !VertexChannelNormal.HasValue;
							vertex_channel_normal = EditorGUILayout.Toggle(VertexChannelNormalContent, vertex_channel_normal);
						
							EditorGUI.showMixedValue = !VertexChannelUV0.HasValue;
							vertex_channel_UV0 = EditorGUILayout.Toggle(VertexChannelUV1Content, vertex_channel_UV0);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < models.Length; i++)
							{
								var vertexChannel = models[i].VertexChannels;
								vertexChannel &= ~(VertexChannelFlags.Color |
															  VertexChannelFlags.Tangent |
															  VertexChannelFlags.Normal |
															  VertexChannelFlags.UV0);

								if (vertex_channel_color)	vertexChannel |= VertexChannelFlags.Color;
								if (vertex_channel_tangent)	vertexChannel |= VertexChannelFlags.Tangent;
								if (vertex_channel_normal)	vertexChannel |= VertexChannelFlags.Normal;
								if (vertex_channel_UV0)		vertexChannel |= VertexChannelFlags.UV0;
								models[i].VertexChannels = vertexChannel;
							}
							GUI.changed = true;
						}
					}
					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
			if (models != null && models.Length == 1)
			{
				GUILayout.Space(10);

				GUILayout.BeginVertical(GUI.skin.box);
				_showDetails = EditorGUILayout.BeginToggleGroup("Statistics", _showDetails);
				if (_showDetails)
				{
					var model_cache = InternalCSGModelManager.GetModelCache(models[0]);
					if (model_cache == null ||
						model_cache.GeneratedMeshes == null || 
						!model_cache.GeneratedMeshes)
					{
						GUILayout.Label("Could not find model cache for this model.");
					} else
					{
						var meshContainer = model_cache.GeneratedMeshes;


						var totalTriangles = 0;
						var totalVertices = 0;
						var totalMeshes = 0;

						var materialMeshes = new Dictionary<Material, List<MeshData>>();
						foreach (var instance in meshContainer.meshInstances.Values)
						{
							var mesh				= instance.SharedMesh;
							if (!MeshInstanceManager.HasVisibleMeshRenderer(instance))
								continue;

							List<MeshData> meshes;
							if (!materialMeshes.TryGetValue(instance.RenderMaterial, out meshes))
							{
								meshes = new List<MeshData>();
								materialMeshes[instance.RenderMaterial] = meshes;
							}

							var meshData = new MeshData();
							meshData.Mesh = mesh;
							meshData.VertexCount = mesh.vertexCount;
							meshData.TriangleCount = mesh.triangles.Length / 3;
							meshes.Add(meshData);
							
							totalVertices += meshData.VertexCount;
							totalTriangles = meshData.TriangleCount;
							totalMeshes++;
						}
						EditorGUI.indentLevel++;
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("total:");
						EditorGUILayout.LabelField("vertices: " + totalVertices + "  triangles: " + totalTriangles + "  materials: " + materialMeshes.Count + "  meshes: " + totalMeshes);
						GUILayout.Space(10);
						EditorGUILayout.LabelField("meshes:");
						foreach(var item in materialMeshes)
						{
							var material = item.Key;
							var meshes = item.Value;
															
							GUILayout.BeginHorizontal();
							{
								EditorGUI.BeginDisabledGroup(true);
								{
									EditorGUILayout.ObjectField(material, typeof(Material), true);
								}								
								GUILayout.BeginVertical();
								{
									if (meshes.Count == 1)
									{
										EditorGUILayout.ObjectField(meshes[0].Mesh, typeof(Mesh), true);
										EditorGUILayout.LabelField("vertices " + meshes[0].VertexCount + "  triangles " + meshes[0].TriangleCount);
									} else
									{ 
										for (int i = 0; i < meshes.Count; i++)
										{
											EditorGUILayout.ObjectField(meshes[i].Mesh, typeof(Mesh), true);
											EditorGUILayout.LabelField("vertices " + meshes[i].VertexCount + "  triangles " + meshes[i].TriangleCount);
										}
									}
								}
								GUILayout.EndVertical();
								EditorGUI.EndDisabledGroup();
							}
							GUILayout.EndHorizontal();
							EditorGUILayout.Space();
						}
						EditorGUI.indentLevel--;
					}
				}
				EditorGUILayout.EndToggleGroup();
				GUILayout.EndVertical();
			}
			EditorGUI.showMixedValue = false;
			if (updateMeshes)
			{
				InternalCSGModelManager.DoForcedMeshUpdate();
				SceneViewEventHandler.ResetUpdateRoutine();
			}
		}
	}
}
 