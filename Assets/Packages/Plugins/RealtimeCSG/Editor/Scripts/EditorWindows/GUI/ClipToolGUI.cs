using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	internal static class ClipToolGUI
	{
		static int SceneViewMeshOverlayHash = "SceneViewClipOverlay".GetHashCode();

		private static GUIContent			ContentClipLabel;
		
		static readonly ClipMode[] clipModeValues = new ClipMode[]
			{
				ClipMode.RemovePositive,
				ClipMode.RemoveNegative,
				ClipMode.Split
//				,ClipEditBrushTool.ClipMode.Mirror			
			};

		private static readonly GUIContent	ContentDefaultMaterial	= new GUIContent("Default");
		private static readonly GUIContent	ContentCommit			= new GUIContent("Commit");
		private static readonly GUIContent	ContentCancel			= new GUIContent("Cancel");
		private static readonly ToolTip		CommitTooltip			= new ToolTip("Commit your changes", "Split the selected brush(es) with the current clipping plane. This makes your changes final.", Keys.PerformActionKey);
		private static readonly ToolTip		CancelTooltip			= new ToolTip("Cancel your changes", "Do not clip your selected brushes and return them to their original state.", Keys.CancelActionKey);

		private static readonly GUILayoutOption		largeLabelWidth		= GUILayout.Width(80);
		private static readonly GUILayoutOption[]	MaterialSceneWidth	= new GUILayoutOption[] { GUILayout.Width(140) };

		static void InitLocalStyles()
		{
			if (ContentClipLabel != null)
				return;

			ContentClipLabel	= new GUIContent(GUIStyleUtility.brushEditModeNames[(int)ToolEditMode.Clip]);
		}
		
		static bool doCommit = false; // unity bug workaround
		static bool doCancel = false; // unity bug workaround

		static Rect lastGuiRect;
		public static Rect GetLastSceneGUIRect(ClipBrushTool tool)
		{
			return lastGuiRect;
		}

		public static void OnSceneGUI(Rect windowRect, ClipBrushTool tool)
		{
			doCommit = false; // unity bug workaround
			doCancel = false; // unity bug workaround

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
						GUILayout.BeginVertical(ContentClipLabel, windowStyle, GUIStyleUtility.ContentEmpty);
						{
							OnGUIContents(true, tool);
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

						int controlID = GUIUtility.GetControlID(SceneViewMeshOverlayHash, FocusType.Passive, currentArea);
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

			if (tool != null)
			{ 
				if (doCommit) tool.Commit();	// unity bug workaround
				if (doCancel) tool.Cancel();	// unity bug workaround
			}
		}

		static void OnGUIContents(bool isSceneGUI, ClipBrushTool tool)
		{
			CommonGUI.StartToolGUI();

			if (tool.ClipBrushCount == 0)
			{
				GUILayout.Label(string.Format("no brushes selected", tool.ClipBrushCount), GUIStyleUtility.redTextArea);
			} else
			{ 
				if (tool.ClipBrushCount == 1)
					GUILayout.Label(string.Format("{0} brush selected", tool.ClipBrushCount));
				else
					GUILayout.Label(string.Format("{0} brushes selected", tool.ClipBrushCount));
			}
			EditorGUILayout.Space();
			EditorGUI.BeginDisabledGroup(tool == null);
			{ 
				GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
				{
					var newClipMode = (tool != null) ? tool.clipMode : ((ClipMode)999);
					var skin = GUIStyleUtility.Skin;
					for (int i = 0; i < clipModeValues.Length; i++)
					{
						var selected = newClipMode == clipModeValues[i];
						GUIContent content;
						GUIStyle style;
						if (selected)	{ style = GUIStyleUtility.selectedIconLabelStyle;   content = skin.clipNamesOn[i]; }
						else			{ style = GUIStyleUtility.unselectedIconLabelStyle; content = skin.clipNames[i];   }
						if (GUILayout.Toggle(selected, content, style))
						{
							newClipMode = clipModeValues[i];
						}
						TooltipUtility.SetToolTip(GUIStyleUtility.clipTooltips[i]);
					}
					if (tool != null && tool.clipMode != newClipMode)
					{
						tool.SetClipMode(newClipMode);
					}
				}
				GUILayout.EndVertical();
				if (!isSceneGUI)
					GUILayout.Space(10);

				bool disabled = (tool == null || tool.editMode != ClipBrushTool.EditMode.EditPoints);

				var defaultMaterial = CSGSettings.DefaultMaterial;
				GUILayout.BeginVertical(isSceneGUI ? MaterialSceneWidth : GUIStyleUtility.ContentEmpty);
				{
					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					{
						if (isSceneGUI)
						{
							EditorGUI.BeginChangeCheck();
							{
								defaultMaterial = GUIStyleUtility.MaterialImage(defaultMaterial);
							}
							if (EditorGUI.EndChangeCheck() && defaultMaterial)
							{
								CSGSettings.DefaultMaterial = defaultMaterial;
								CSGSettings.Save();
							}
							GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
						}
						{
							EditorGUI.BeginDisabledGroup(disabled);
							{
								if (GUILayout.Button(ContentCancel)) { doCancel = true; }
								TooltipUtility.SetToolTip(CancelTooltip);
								if (GUILayout.Button(ContentCommit)) { doCommit = true; }
								TooltipUtility.SetToolTip(CommitTooltip);
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
							CSGSettings.Save();
						}
					}
				}
				if (!isSceneGUI)
				{
					EditorGUILayout.Space();
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
								CSGSettings.Save();
							}
						}
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							GUILayout.Space(5);
							defaultMaterial = GUIStyleUtility.MaterialImage(defaultMaterial, small: false);
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
				GUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();
		}

		public static void OnInspectorGUI(EditorWindow window, float height)
		{
			lastGuiRect = Rect.MinMaxRect(-1, -1, -1, -1);
			var tool = CSGBrushEditorManager.ActiveTool as ClipBrushTool;

			doCommit = false; // unity bug workaround
			doCancel = false; // unity bug workaround
			
			GUIStyleUtility.InitStyles();
			InitLocalStyles();
			OnGUIContents(false, tool);

			if (tool != null)
			{ 
				if (doCommit) tool.Commit();	// unity bug workaround
				if (doCancel) tool.Cancel();	// unity bug workaround
			}
		}
	}
}
