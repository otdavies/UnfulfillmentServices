using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	internal sealed class CSGBrushEditorGUI
	{
		static int SceneViewBrushEditorOverlayHash = "SceneViewBrushEditorOverlay".GetHashCode();
		static GUIContent	ContentEditModesLabel				= new GUIContent("Edit Modes");
		static GUIContent	HandleAsOneLabel					= new GUIContent("Handle as one object", "Select this operation when a child is selected");
		static GUIContent	PrefabLabelContent					= new GUIContent("Drag & drop behaviour");
		static GUIContent	RaySnappingLabelContent				= new GUIContent("Ray snapping behaviour");

		static GUIContent	PrefabInstantiateBehaviourContent	= new GUIContent("Instantiation");
		static GUIContent	DestinationAlignmentContent			= new GUIContent("Placement");
		static GUIContent	SourceAlignmentContent				= new GUIContent("Front");
//		static GUIContent	ContentLayerContent					= new GUIContent("Contents");
		static GUIContent	SurfacesContent						= new GUIContent("Surfaces");
		
		
		static ToolTip		RaySnappingBehaviourTooltip			= new ToolTip("Ray snapping behaviour", "Here you can set how your object will align to the surface underneath it when dragging it while ray-snapping (holding Shift)");
		
		static ToolTip		PrefabInstantiateBehaviourTooltip	= new ToolTip("Instantiation", "Here you can set if your prefab is copied into the scene without any link to the original prefab, or if it'll be an instance of your prefab (default unity behaviour).");
		static ToolTip		DestinationAlignmentTooltip			= new ToolTip("Placement", "Here you can set how your object will be rotated if it's dragged over a brush surface.\n\n"+
																							   "Align to Surface:\nRotate it so that it's aligned with the surface you drag over.\n\n"+
																							   "Always face up:\nAlign it with the surface but always make it face 'up'.\n\n" +
																							   "None:\ndo not rotate it along the surface.");
		static ToolTip		SourceAlignmentTooltip				= new ToolTip("Front", "Define here what direction the original object was created and what is considered it's 'front', this helps align it properly during placement.");
//		static ToolTip		ContentLayerTooltip					= new ToolTip("Content (experimental)", "What type of brush this is and how it interacts with other brushes.\n\n"+
//																			  "This is an experimental feature and will be improved over time.\n\n" +
//																			  "Currently Water brushes need to be defined before Glass brushes, and both need to be defined before Solid brushes for this to work.");

//		static GUIContent[]	ContentLayerTexts =
//		{
//			new GUIContent("Solid"),
//			new GUIContent("Glass"),
//			new GUIContent("Water")
//		};

		static GUIContent	VersionLabel						= new GUIContent(
#if DEMO
			"EVALUATION " +
#endif
			string.Format("v {0}{1}{2}", 
				ToolConstants.PluginVersion, 
				CSGBindings.HasBeenCompiledInDebugMode() ? " (C++ DEBUG)" : string.Empty,
#if DEMO && DEBUG
				" (C# DEBUG)"
#else
				string.Empty
#endif
			));
		static GUIContent DisabledLabelContent					= new GUIContent("Realtime CSG is disabled, press control-F3 to enable");
		static GUIContent EnableRealtimeCSGContent              = new GUIContent("Enable Realtime-CSG");

		static GUI.WindowFunction windowFunction = new GUI.WindowFunction(CSGBrushEditorGUI.HandleSceneGUI);

		public static float OnEditModeSelectionGUI()
		{			
			GUIStyleUtility.InitStyles();

			EditorGUI.BeginChangeCheck();
			Rect editModeBounds;
			var newEditMode = (ToolEditMode)GUIStyleUtility.ToolbarWrapped((int)CSGBrushEditorManager.EditMode, ref editModeRects, out editModeBounds, GUIStyleUtility.brushEditModeContent, GUIStyleUtility.brushEditModeTooltips);
			if (EditorGUI.EndChangeCheck())
			{
				CSGBrushEditorManager.EditMode = newEditMode;
				SceneView.RepaintAll();
			}
			GUILayout.Space(editModeBounds.height);
				

			return editModeBounds.height;
		}

		static GUIStyle sceneViewOverlayTransparentBackground = "SceneViewOverlayTransparentBackground";
		
		public static void HandleWindowGUI(Rect windowRect)
		{
			GUILayout.Window(SceneViewBrushEditorOverlayHash,
						windowRect,
						windowFunction,
						string.Empty, sceneViewOverlayTransparentBackground,
						GUIStyleUtility.ContentEmpty);
		}

		static void HandleSceneGUI(int id)
        {
            var sceneView = SceneView.lastActiveSceneView;
            TooltipUtility.InitToolTip(sceneView);
			var originalSkin = GUI.skin;
			{
				OnEditModeSelectionSceneGUI();

				var viewRect = new Rect(4, 0, sceneView.position.width, sceneView.position.height - (GUIStyleUtility.BottomToolBarHeight + 4));
                GUILayout.BeginArea(viewRect);

				if (CSGBrushEditorManager.ActiveTool != null)
                {
                    CSGBrushEditorManager.ActiveTool.OnSceneGUI(viewRect);
                }
                GUILayout.EndArea();

                if (RealtimeCSG.CSGSettings.EnableRealtimeCSG)
                    BottomBarGUI.ShowGUI(sceneView, haveOffset: false);
            }
			GUI.skin = originalSkin;
			Handles.BeginGUI();
			TooltipUtility.DrawToolTip(getLastRect: false);
			Handles.EndGUI();
		}
		
		//static Vector2 scrollPos;

		public static void HandleWindowGUI(EditorWindow window)
		{
			TooltipUtility.InitToolTip(window);
			var originalSkin = GUI.skin;
			{
				var height = OnEditModeSelectionGUI(); 
				//var applyOffset = !TooltipUtility.FoundToolTip();

				EditorGUILayout.Space();

				ShowRealtimeCSGDisabledMessage();

				//scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				{
					EditorGUI.BeginDisabledGroup(!RealtimeCSG.CSGSettings.EnableRealtimeCSG);
					{
						if (CSGBrushEditorManager.ActiveTool != null)
							CSGBrushEditorManager.ActiveTool.OnInspectorGUI(window, height);
					}
					EditorGUI.EndDisabledGroup();
				}
				//EditorGUILayout.EndScrollView();
				//if (applyOffset) 
				//	TooltipUtility.HandleAreaOffset(scrollPos);

				GUILayout.Label(VersionLabel, EditorStyles.miniLabel);
			}
			GUI.skin = originalSkin;
			TooltipUtility.DrawToolTip();
		}


		static Rect[] editModeRects;

		
		static void OnEditModeSelectionSceneGUI()
		{
            GUIStyleUtility.InitStyles();
			if (GUIStyleUtility.brushEditModeNames == null ||
                GUIStyleUtility.brushEditModeNames.Length == 0)
				return;

            var oldSkin = GUI.skin;
            GUIStyleUtility.SetDefaultGUISkin();
			GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
			{
				GUIStyle windowStyle = GUI.skin.window;

				var bounds = new Rect(10, 30, 500, 40);

				GUILayout.BeginArea(bounds, ContentEditModesLabel, windowStyle);
				{
					//GUILayout.Space(bounds.height);
					Rect editModeBounds;
			
					GUIStyleUtility.InitStyles();
					EditorGUI.BeginChangeCheck();
					var newEditMode = (ToolEditMode)GUIStyleUtility.ToolbarWrapped((int)CSGBrushEditorManager.EditMode, ref editModeRects, out editModeBounds, GUIStyleUtility.brushEditModeContent, GUIStyleUtility.brushEditModeTooltips, yOffset:20, areaWidth: bounds.width);
					//var newEditMode = (ToolEditMode)GUILayout.Toolbar((int)CSGBrushEditorManager.EditMode, GUIStyleUtility.brushEditModeContent, GUIStyleUtility.brushEditModeTooltips);
					if (EditorGUI.EndChangeCheck())
					{
						CSGBrushEditorManager.EditMode = newEditMode;
						SceneView.RepaintAll();
					}
				
					var buttonArea = bounds;
					buttonArea.x = bounds.width - 17;
					buttonArea.y = 2;
					buttonArea.height = 13;
					buttonArea.width = 13;
					if (GUI.Button(buttonArea, GUIContent.none, "WinBtnClose"))
						CSGBrushEditorWindow.GetWindow();
					TooltipUtility.SetToolTip(GUIStyleUtility.PopOutTooltip, buttonArea); 

					var versionWidth = GUIStyleUtility.versionLabelStyle.CalcSize(VersionLabel);
					var versionArea = bounds;
					versionArea.x = bounds.width - (17 + versionWidth.x);
					versionArea.y = 1;
					versionArea.height = 15;
					versionArea.width = versionWidth.x;
					GUI.Label(versionArea, VersionLabel, GUIStyleUtility.versionLabelStyle);
				}
				GUILayout.EndArea();
                     
				int controlID = GUIUtility.GetControlID(SceneViewBrushEditorOverlayHash, FocusType.Keyboard, bounds);
				switch (Event.current.GetTypeForControl(controlID))
				{
					case EventType.MouseDown:	{ if (bounds.Contains(Event.current.mousePosition)) { GUIUtility.hotControl = controlID; GUIUtility.keyboardControl = controlID; EditorGUIUtility.editingTextField = false; Event.current.Use(); } break; }
					case EventType.MouseMove:	{ if (bounds.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
					case EventType.MouseUp:		{ if (GUIUtility.hotControl == controlID) { GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0; Event.current.Use(); } break; }
					case EventType.MouseDrag:	{ if (GUIUtility.hotControl == controlID) { Event.current.Use(); } break; }
					case EventType.ScrollWheel: { if (bounds.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
				}
			}
			GUILayout.EndHorizontal();
			GUI.skin = oldSkin;
		}

		
		
		static Camera MainCamera
		{
			get
			{
				var mainCamera = Camera.main;
				if (mainCamera != null)
					return mainCamera;

				Camera[] allCameras = Camera.allCameras;
				if (allCameras != null && allCameras.Length == 1)
					return allCameras[0];

				return null;
			}
		}
		
		static RenderingPath SceneViewRenderingPath
		{
			get
			{
				var mainCamera = MainCamera;
				if (mainCamera != null)
					return mainCamera.renderingPath;
				return RenderingPath.UsePlayerSettings;
			}
		}
		/*
		static bool IsUsingDeferredRenderingPath
		{
			get
			{
				RenderingPath renderingPath = SceneViewRenderingPath;
				return (renderingPath == RenderingPath.DeferredShading) ||
					   (renderingPath == RenderingPath.UsePlayerSettings &&
					   PlayerSettings.renderingPath == RenderingPath.DeferredShading);
			}
		}
		*/

		static public void ShowRealtimeCSGDisabledMessage()
		{
			if (!RealtimeCSG.CSGSettings.EnableRealtimeCSG)
			{
				GUILayout.BeginVertical(GUIStyleUtility.redTextArea);
				{
					GUILayout.Label(DisabledLabelContent, GUIStyleUtility.redTextLabel);
					if (GUILayout.Button(EnableRealtimeCSGContent))
						RealtimeCSG.CSGSettings.SetRealtimeCSGEnabled(true);
				}
				GUILayout.EndVertical();
			}
		}

		static bool OpenSurfaces = false;

		static public void OnInspectorGUI(Editor editor, UnityEngine.Object[] targets)
		{
			TooltipUtility.InitToolTip(editor);
			try
			{ 
				var models = new CSGModel[targets.Length];

				for (int i = targets.Length - 1; i >= 0; i--)
				{
					models[i] = targets[i] as CSGModel;
					if (!models[i])
					{
						ArrayUtility.RemoveAt(ref models, i);
					}
				}
			
				GUIStyleUtility.InitStyles();
				ShowRealtimeCSGDisabledMessage();

				if (models.Length > 0 && models.Length == targets.Length)
				{
					ModelInspectorGUI.OnInspectorGUI(targets);
					return;
				}

				var filteredSelection	= CSGBrushEditorManager.FilteredSelection;
				var targetNodes			= filteredSelection.NodeTargets;
				var targetModels		= filteredSelection.ModelTargets;
				var targetBrushes		= filteredSelection.BrushTargets;
				var targetOperations	= filteredSelection.OperationTargets;
				if (targetNodes == null)
				{
					return;
				}

			

				bool? isPrefab = false;
				PrefabInstantiateBehaviour? prefabBehaviour				= PrefabInstantiateBehaviour.Reference;
				PrefabSourceAlignment?		prefabSourceAlignment		= PrefabSourceAlignment.AlignedTop;
				PrefabDestinationAlignment?	prefabDestinationAlignment	= PrefabDestinationAlignment.AlignToSurface;
				
				if (targetNodes.Length > 0)
				{
					var gameObject = targetNodes[0].gameObject;
					isPrefab = SelectionUtility.IsPrefab(gameObject);
					prefabBehaviour = targetNodes[0].PrefabBehaviour;
					prefabSourceAlignment = targetNodes[0].PrefabSourceAlignment;
					prefabDestinationAlignment = targetNodes[0].PrefabDestinationAlignment;
					for (int i = 1; i < targetNodes.Length; i++)
					{
						gameObject = targetNodes[i].gameObject;
						var currentIsPrefab = PrefabUtility.GetPrefabParent(gameObject) == null && PrefabUtility.GetPrefabObject(gameObject) != null && gameObject.transform.parent == null;
						var currentPrefabBehaviour = targetNodes[i].PrefabBehaviour;
						var currentPrefabSourceAlignment = targetNodes[i].PrefabSourceAlignment;
						var currentPrefabDestinationAlignment = targetNodes[i].PrefabDestinationAlignment;
						if (isPrefab.HasValue && isPrefab.Value != currentIsPrefab)
							isPrefab = null;
						if (prefabBehaviour.HasValue && prefabBehaviour.Value != currentPrefabBehaviour)
							prefabBehaviour = null;
						if (prefabSourceAlignment.HasValue && prefabSourceAlignment.Value != currentPrefabSourceAlignment)
							prefabSourceAlignment = null;
						if (prefabDestinationAlignment.HasValue && prefabDestinationAlignment.Value != currentPrefabDestinationAlignment)
							prefabDestinationAlignment = null;
					}
				}
				
				GUILayout.BeginVertical(GUI.skin.box);
				{
					if (isPrefab.HasValue && isPrefab.Value)
					{
						EditorGUILayout.LabelField(PrefabLabelContent);
					} else
					{
						EditorGUILayout.LabelField(RaySnappingLabelContent);
						TooltipUtility.SetToolTip(RaySnappingBehaviourTooltip);
					}
			
					EditorGUI.indentLevel++;
					{
						if (isPrefab.HasValue && isPrefab.Value)
						{
							EditorGUI.showMixedValue = !prefabBehaviour.HasValue;
							var prefabBehavour = prefabBehaviour.HasValue ? prefabBehaviour.Value : PrefabInstantiateBehaviour.Reference;
							EditorGUI.BeginChangeCheck();
							{
								prefabBehavour = (PrefabInstantiateBehaviour)EditorGUILayout.EnumPopup(PrefabInstantiateBehaviourContent, prefabBehavour);
								TooltipUtility.SetToolTip(PrefabInstantiateBehaviourTooltip);
							}
							if (EditorGUI.EndChangeCheck())
							{
								for (int i = 0; i < targetNodes.Length; i++)
								{
									targetNodes[i].PrefabBehaviour = prefabBehavour;
								}
							}
							EditorGUI.showMixedValue = false;
						}


						EditorGUI.showMixedValue = !prefabDestinationAlignment.HasValue;
						var destinationAlignment = prefabDestinationAlignment.HasValue ? prefabDestinationAlignment.Value : PrefabDestinationAlignment.AlignToSurface;
						EditorGUI.BeginChangeCheck();
						{
							destinationAlignment = (PrefabDestinationAlignment)EditorGUILayout.EnumPopup(DestinationAlignmentContent, destinationAlignment);
							TooltipUtility.SetToolTip(DestinationAlignmentTooltip);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < targetNodes.Length; i++)
							{
								targetNodes[i].PrefabDestinationAlignment = destinationAlignment;
							}
						}
						EditorGUI.showMixedValue = false;


						EditorGUI.showMixedValue = !prefabSourceAlignment.HasValue;
						var sourceAlignment = prefabSourceAlignment.HasValue ? prefabSourceAlignment.Value : PrefabSourceAlignment.AlignedFront;
						EditorGUI.BeginChangeCheck();
						{
							sourceAlignment = (PrefabSourceAlignment)EditorGUILayout.EnumPopup(SourceAlignmentContent, sourceAlignment);
							TooltipUtility.SetToolTip(SourceAlignmentTooltip);
						}
						if (EditorGUI.EndChangeCheck())
						{
							for (int i = 0; i < targetNodes.Length; i++)
							{
								targetNodes[i].PrefabSourceAlignment = sourceAlignment;
							}
						}
						EditorGUI.showMixedValue = false;
					}
					EditorGUI.indentLevel--;
				}
				GUILayout.EndVertical();
				GUILayout.Space(10);
				

				if (targetModels.Length == 0)
				{
					int					invalidOperationType	= 999;
					bool?				handleAsOne		= null;
					bool				selMixedValues	= false;
					CSGOperationType	operationType	= (CSGOperationType)invalidOperationType;
					bool				opMixedValues	= false;
					uint?				targetContentLayer	= null;
					if (targetBrushes.Length > 0)
					{
						operationType		= targetBrushes[0].OperationType;
						targetContentLayer	= targetBrushes[0].ContentLayer;
					}
					for (int b = 1; b < targetBrushes.Length; b++)
					{
						var brush = targetBrushes[b];
						if (operationType != brush.OperationType)
						{
							opMixedValues = true;
						}
						if (targetContentLayer.HasValue && targetContentLayer.Value != brush.ContentLayer)
						{
							targetContentLayer = null;
						}
					}
					foreach(var operation in targetOperations)
					{
						if (operationType == (CSGOperationType)invalidOperationType)
						{
							operationType = operation.OperationType;
						} else
						if (operationType != operation.OperationType)
						{
							opMixedValues = true;
						}
					
						if (!handleAsOne.HasValue)
						{
							handleAsOne = operation.HandleAsOne;
						} else
						if (handleAsOne.Value != operation.HandleAsOne)
						{
							selMixedValues	= true; 
						}
					}
					GUILayout.BeginVertical(GUI.skin.box);
					{
						bool passThroughValue	= false;
						if (targetBrushes.Length == 0 && targetOperations.Length > 0) // only operations
						{
							bool? passThrough = targetOperations[0].PassThrough;
							for (int i = 1; i < targetOperations.Length; i++)
							{
								if (passThrough.HasValue && passThrough.Value != targetOperations[i].PassThrough)
								{
									passThrough = null;
									break;
								}
							}
							
							opMixedValues = !passThrough.HasValue || passThrough.Value;

							var ptMixedValues		= !passThrough.HasValue;
							passThroughValue		= passThrough.HasValue ? passThrough.Value : false;
							if (GUIStyleUtility.PassThroughButton(passThroughValue, ptMixedValues))
							{
								Undo.RecordObjects(targetNodes, "Changed CSG operation of nodes");
								foreach (var operation in targetOperations)
								{
									operation.PassThrough = true;
								}
								InternalCSGModelManager.Refresh();
								EditorApplication.RepaintHierarchyWindow();
							}

							if (passThroughValue)							
								operationType = (CSGOperationType)255;
						}
						EditorGUI.BeginChangeCheck();
						{
							operationType = GUIStyleUtility.ChooseOperation(operationType, opMixedValues);
						}
						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObjects(targetNodes, "Changed CSG operation of nodes");
							foreach (var brush in targetBrushes)
							{
								brush.OperationType = operationType;
							}
							foreach (var operation in targetOperations)
							{
								operation.PassThrough = false;
								operation.OperationType = operationType;
							}
							InternalCSGModelManager.Refresh();
							EditorApplication.RepaintHierarchyWindow();
						}
					}
					GUILayout.EndVertical();

					if (targetOperations.Length == 0 && targetModels.Length == 0)
					{
						GUILayout.Space(10);
						/*
						GUILayout.BeginVertical(GUI.skin.box);
						{
							GUILayout.Label("Experimental");
							EditorGUI.indentLevel++;
							{ 
								EditorGUI.showMixedValue = !targetContentLayer.HasValue;
								var contentLayer = targetContentLayer.HasValue ? targetContentLayer.Value : 0;
								EditorGUI.BeginChangeCheck();
								{
									contentLayer = (uint)EditorGUILayout.Popup(ContentLayerContent, (int)contentLayer, ContentLayerTexts);
									TooltipUtility.SetToolTip(ContentLayerTooltip);
								}
								if (EditorGUI.EndChangeCheck())
								{
									for (int i = 0; i < targetBrushes.Length; i++)
									{
										targetBrushes[i].ContentLayer = contentLayer;
									}
								}
							}
							EditorGUI.indentLevel--;
						}
						GUILayout.EndVertical();

						GUILayout.Space(10);
						//*/
						if (targetBrushes.Length == 1)
						{ 
							GUILayout.BeginVertical(GUI.skin.box);
							{
								EditorGUI.indentLevel++;
								OpenSurfaces = EditorGUILayout.Foldout(OpenSurfaces, SurfacesContent);
								EditorGUI.indentLevel--;
								if (OpenSurfaces)
								{ 
									var targetShape		= targetBrushes[0].Shape;
									var texGens			= targetShape.TexGens;
									var texGenFlagArray = targetShape.TexGenFlags;
									for (int t = 0; t < texGens.Length; t++)
									{
										GUILayout.Space(2);

										var texGenFlags			= texGenFlagArray[t];
										var material			= targetShape.TexGens[t].RenderMaterial;
										EditorGUI.BeginChangeCheck();
										{
											GUILayout.BeginHorizontal();
											{
												GUILayout.Space(4);
												material = GUIStyleUtility.MaterialImage(material);
												GUILayout.Space(2);
												GUILayout.BeginVertical();
												{
													EditorGUI.BeginDisabledGroup(texGenFlags != TexGenFlags.None);
													{
														material = EditorGUILayout.ObjectField(material, typeof(Material), true) as Material;
													}
													EditorGUI.EndDisabledGroup();

													texGenFlags = CommonGUI.OnSurfaceFlagButtons(texGenFlags);
												}
												GUILayout.EndVertical();
												GUILayout.Space(4);
											}
											GUILayout.EndHorizontal();
										}
										if (EditorGUI.EndChangeCheck())
										{
											var selectedBrushSurfaces = new []
											{
												new SelectedBrushSurface(targetBrushes[0], t)
											};
											using (new UndoGroup(selectedBrushSurfaces, "discarding surface"))
											{
												texGenFlagArray[t] = texGenFlags;
												targetShape.TexGens[t].RenderMaterial = material;
											}
										}
										GUILayout.Space(4);
									}
								}
							}
							GUILayout.EndVertical();
						}
					}

					if (handleAsOne.HasValue)
					{ 
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = selMixedValues;
							handleAsOne = EditorGUILayout.Toggle(HandleAsOneLabel, handleAsOne.Value);
						}
						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObjects(targetNodes, "Changed CSG operation 'Handle as one object'");
							foreach (var operation in targetOperations)
							{
								operation.HandleAsOne = handleAsOne.Value;
							}
							EditorApplication.RepaintHierarchyWindow();
						}
					}
				}

	#if false
				if (targetNodes.Length == 1)
				{
					var node = targetNodes[0];
					var brush = node as CSGBrush;
					if (brush != null)
					{
						var brush_cache = CSGSceneManager.GetBrushCache(brush);
						if (brush_cache == null ||
							brush_cache.childData == null ||
							brush_cache.childData.modelTransform == null)
						{
							EditorGUILayout.LabelField("brush-cache: null");
						} else
						{
							EditorGUILayout.LabelField("node-id: " + brush.nodeID + " brush-id: " + brush.brushID);
						}
					}
					var operation = node as CSGOperation;
					if (operation != null)
					{
						var operation_cache = CSGSceneManager.GetOperationCache(operation);
						if (operation_cache == null ||
							operation_cache.childData == null ||
							operation_cache.childData.modelTransform == null)
						{
							EditorGUILayout.LabelField("operation-cache: null");
						} else
						{
							EditorGUILayout.LabelField("operation-id: " + operation.nodeID + " operation-id: " + operation.operationID);
						}
					}
					var model = node as CSGModel;
					if (model != null)
					{
						var model_cache = CSGSceneManager.GetModelCache(model);
						if (model_cache == null ||
							model_cache.meshContainer == null)
						{
							EditorGUILayout.LabelField("model-cache: null");
						}  else
						{
							EditorGUILayout.LabelField("model-id: " + model.nodeID + " model-id: " + model.modelID);
						}
					}
				}
	#endif
			}
			finally
			{
				TooltipUtility.DrawToolTip(getLastRect: true, goUp: true);
			}
		}
	}
}
