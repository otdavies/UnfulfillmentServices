using InternalRealtimeCSG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealtimeCSG
{
	internal static class GenerateBrushToolGUI
	{
		static int SceneViewMeshOverlayHash = "SceneViewMeshOverlay".GetHashCode();

		static GUIContent			ContentTitleLabel;
		
		private static readonly GUIContent	ContentDefaultMaterial	= new GUIContent("Default");
		private static readonly GUIContent	CreateContent			= new GUIContent("Create");
		private static readonly GUIContent	CancelContent			= new GUIContent("Cancel");
		private static readonly ToolTip		CreateTooltip			= new ToolTip("Create your brush", "Create the brush from the current box shape.", Keys.PerformActionKey);
		private static readonly ToolTip		CancelTooltip			= new ToolTip("Cancel brush creation", "Do not generate the brush.", Keys.CancelActionKey);

		private static readonly GUILayoutOption largeLabelWidth = GUILayout.Width(80);
		private static readonly GUILayoutOption Width100		= GUILayout.Width(100);

		static void InitLocalStyles()
		{
			if (ContentTitleLabel != null)
				return;
			ContentTitleLabel	= new GUIContent(GUIStyleUtility.brushEditModeNames[(int)ToolEditMode.Generate]);
		}

		static Rect lastGuiRect;
		public static Rect GetLastSceneGUIRect(GenerateBrushTool tool)
		{
			return lastGuiRect;
		}

		public static bool OnSceneGUI(Rect windowRect, GenerateBrushTool tool)
		{
			tool.CurrentGenerator.StartGUI();
			GUIStyleUtility.InitStyles();
			InitLocalStyles();
			GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
			{
				GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
				{
					GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
					{
						GUILayout.FlexibleSpace();

						GUIStyleUtility.ResetGUIState();
						GUIStyle windowStyle = GUI.skin.window;		
						
						GUILayout.BeginVertical(ContentTitleLabel, windowStyle, GUILayout.Width(275));
						{
							OnGUIContents(tool, true, 0);
						}
						GUILayout.EndVertical();
						var currentArea = GUILayoutUtility.GetLastRect();
						lastGuiRect = currentArea;

						var buttonArea = currentArea;
						buttonArea.x += buttonArea.width - 17;
						buttonArea.y += 2;
						buttonArea.height = 13;
						buttonArea.width = 13;
						if (GUI.Button(buttonArea, GUIContent.none, "WinBtnClose"))
							CSGBrushEditorWindow.GetWindow();
						TooltipUtility.SetToolTip(GUIStyleUtility.PopOutTooltip, buttonArea);

						int controlID = GUIUtility.GetControlID(SceneViewMeshOverlayHash, FocusType.Keyboard, currentArea);
						switch (Event.current.GetTypeForControl(controlID))
						{
							case EventType.MouseDown:	{ if (currentArea.Contains(Event.current.mousePosition)) { GUIUtility.hotControl = controlID; GUIUtility.keyboardControl = controlID; Event.current.Use(); } break; }
							case EventType.MouseMove:	{ if (currentArea.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
							case EventType.MouseUp:		{ if (GUIUtility.hotControl == controlID) { GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0; Event.current.Use(); } break; }
							case EventType.MouseDrag:	{ if (GUIUtility.hotControl == controlID) { Event.current.Use(); } break; }
							case EventType.ScrollWheel: { if (currentArea.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();			
			tool.CurrentGenerator.FinishGUI();
			return true;
		}

		static void ChooseOperation(GenerateBrushTool tool, bool isSceneGUI)
		{
			var generator = tool.CurrentGenerator;
			bool enabled = generator.HaveBrushes;
			EditorGUI.BeginDisabledGroup(!enabled);
			{
				if (isSceneGUI)
					GUILayout.BeginVertical(GUI.skin.box, Width100);
				else
					GUILayout.BeginVertical(GUIStyle.none);
				{
					bool mixedValues = !enabled;
					CSGOperationType operation = generator.CurrentCSGOperationType;
					EditorGUI.BeginChangeCheck();
					operation = GUIStyleUtility.ChooseOperation(operation, mixedValues);
					if (EditorGUI.EndChangeCheck())
					{
						generator.CurrentCSGOperationType = operation;
					}
				}
				GUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();
		}

		static void CommitOrCancel(bool isSceneGUI, GenerateBrushTool tool)
		{
			var defaultMaterial = CSGSettings.DefaultMaterial;
			GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
			{
				if (isSceneGUI)
				{ 
					GUILayout.Space(5);
					GUILayout.BeginVertical(GUILayout.MinWidth(10));
					{
						EditorGUI.BeginChangeCheck();
						{
							defaultMaterial = GUIStyleUtility.MaterialImage(defaultMaterial);
						}
						if (EditorGUI.EndChangeCheck() && defaultMaterial)
						{
							CSGSettings.DefaultMaterial = defaultMaterial;
							tool.CurrentGenerator.OnDefaultMaterialModified();
							CSGSettings.Save();
						}
					}
					GUILayout.EndVertical();
					GUILayout.Space(4);
					GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
				}
				{
					var generator = tool.CurrentGenerator;
					EditorGUI.BeginDisabledGroup(!generator.CanCommit);
					{
						if (GUILayout.Button(CancelContent)) { generator.DoCancel(); }
						TooltipUtility.SetToolTip(CancelTooltip);
						if (GUILayout.Button(CreateContent)) { generator.DoCommit(); }
						TooltipUtility.SetToolTip(CreateTooltip);
					}
					EditorGUI.EndDisabledGroup();
				}
				if (isSceneGUI)
					GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			if (isSceneGUI)
			{
				GUILayout.Space(2);
				EditorGUI.BeginChangeCheck();
				{
					defaultMaterial = EditorGUILayout.ObjectField(defaultMaterial, typeof(Material), true) as Material;
				}
				if (EditorGUI.EndChangeCheck() && defaultMaterial)
				{
					CSGSettings.DefaultMaterial = defaultMaterial;
					tool.CurrentGenerator.OnDefaultMaterialModified();
					CSGSettings.Save();
				}
			}
		}

		static Vector2 scrollPos;
		static Rect[] shapeModeRects;
		static void OnGUIContents(GenerateBrushTool tool, bool isSceneGUI, float height)
		{
			if (!isSceneGUI)
			{
				Rect shapeModeBounds;
				var csg_skin = GUIStyleUtility.Skin;
				tool.BuilderMode = (ShapeMode)GUIStyleUtility.ToolbarWrapped((int)tool.BuilderMode, ref shapeModeRects, out shapeModeBounds, csg_skin.shapeModeNames, tooltips: GUIStyleUtility.shapeModeTooltips, yOffset: height, areaWidth: EditorGUIUtility.currentViewWidth - 4);
				GUILayout.Space(shapeModeBounds.height);
				
				CommonGUI.StartToolGUI();

				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				{
					ChooseOperation(tool, isSceneGUI);
					tool.CurrentGenerator.OnShowGUI(isSceneGUI);
					CommitOrCancel(isSceneGUI, tool);
					EditorGUILayout.Space();
					var defaultMaterial = CSGSettings.DefaultMaterial;
					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					{
						EditorGUILayout.LabelField(ContentDefaultMaterial, largeLabelWidth);
						GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
						{
							EditorGUI.BeginChangeCheck();
							{
								defaultMaterial = EditorGUILayout.ObjectField(defaultMaterial, typeof(Material), true) as Material;
							}
							if (EditorGUI.EndChangeCheck() && defaultMaterial)
							{
								CSGSettings.DefaultMaterial = defaultMaterial;
								tool.CurrentGenerator.OnDefaultMaterialModified();
								CSGSettings.Save();
							}
						}
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							GUILayout.Space(5);
							EditorGUI.BeginChangeCheck();
							{
								defaultMaterial = GUIStyleUtility.MaterialImage(defaultMaterial, small: false);
							}
							if (EditorGUI.EndChangeCheck() && defaultMaterial)
							{
								CSGSettings.DefaultMaterial = defaultMaterial;
								tool.CurrentGenerator.OnDefaultMaterialModified();
								CSGSettings.Save();
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
					/*
					// Unity won't let us do this
					GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
					OnGUIContentsMaterialInspector(first_material, multiple_materials);
					GUILayout.EndVertical();
					*/
				}
				EditorGUILayout.EndScrollView();
			}
			if (isSceneGUI)
			{
				GUILayout.BeginHorizontal(GUILayout.MinHeight(100));
				{
					GUILayout.BeginVertical(GUILayout.Width(100));
					{
						GUILayout.FlexibleSpace();

						var rect = GUILayoutUtility.GetLastRect();

						var csg_skin = GUIStyleUtility.Skin;
						Rect shapeModeBounds;
						tool.BuilderMode = (ShapeMode)GUIStyleUtility.ToolbarWrapped((int)tool.BuilderMode, ref shapeModeRects, out shapeModeBounds, csg_skin.shapeModeNames, tooltips: GUIStyleUtility.shapeModeTooltips, yOffset: rect.y, areaWidth: 100);
						GUILayout.Space(shapeModeBounds.height);
				
						ChooseOperation(tool, isSceneGUI);
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
					{
						tool.CurrentGenerator.OnShowGUI(isSceneGUI);
						GUILayout.FlexibleSpace();
						CommitOrCancel(isSceneGUI, tool);
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
		}

		public static void OnInspectorGUI(GenerateBrushTool tool, EditorWindow window, float height)
		{
			lastGuiRect = Rect.MinMaxRect(-1, -1, -1, -1);
			tool.CurrentGenerator.StartGUI();
			GUIStyleUtility.InitStyles();
			InitLocalStyles();
			OnGUIContents(tool, false, height);
			tool.CurrentGenerator.FinishGUI();
		}
		
	}
}
