using System.Globalization;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RealtimeCSG
{
	internal sealed class BottomBarGUI
	{
		static int BottomBarEditorOverlayHash = "BottomBarEditorOverlay".GetHashCode();

		static ToolTip showGridTooltip		= new ToolTip("Show the grid", "Click this to toggle between showing the grid or hiding it.\nWhen hidden you can still snap against it.", Keys.ToggleShowGridKey);
		static ToolTip snapToGridTooltip	= new ToolTip("Toggle snapping", "Click this if you want to turn snapping your movements on or off.", Keys.ToggleSnappingKey);
		static ToolTip showWireframeTooltip	= new ToolTip("Toggle wireframe", "Click this to switch between showing the\nwireframe of your scene or the regular view.");
		static ToolTip rebuildTooltip		= new ToolTip("Rebuild your CSG meshes", "Click this to rebuild your CSG meshes\nin case something didn't go quite right.");
		static ToolTip helperSurfacesTooltip = new ToolTip("Helper surfaces", "Select what kind of helper surfaces you want to display in this sceneview.");

		static GUIContent xLabel			= new GUIContent("X");
		static GUIContent yLabel			= new GUIContent("Y");
		static GUIContent zLabel			= new GUIContent("Z");
		
		static ToolTip xTooltipOff			= new ToolTip("Lock X axis", "Click to disable movement on the X axis");
		static ToolTip yTooltipOff			= new ToolTip("Lock Y axis", "Click to disable movement on the Y axis");
		static ToolTip zTooltipOff			= new ToolTip("Lock Z axis", "Click to disable movement on the Z axis");
		static ToolTip xTooltipOn			= new ToolTip("Unlock X axis", "Click to enable movement on the X axis");
		static ToolTip yTooltipOn			= new ToolTip("Unlock Y axis", "Click to enable movement on the Y axis");
		static ToolTip zTooltipOn			= new ToolTip("Unlock Z axis", "Click to enable movement on the Z axis");
		
		static GUIContent positionLargeLabel	= new GUIContent("position");
		static GUIContent positionSmallLabel	= new GUIContent("pos");
		static GUIContent positionPlusLabel		= new GUIContent("+");
		static GUIContent positionMinusLabel	= new GUIContent("-");
		
		static ToolTip positionTooltip			= new ToolTip("Grid size", "Here you can set the size of the grid. Click this\nto switch between setting the grid size for X Y Z\nseparately, or for all of them uniformly.");
		static ToolTip positionPlusTooltip		= new ToolTip("Double grid size", "Multiply the grid size by 2", Keys.DoubleGridSizeKey);
		static ToolTip positionMinnusTooltip	= new ToolTip("Half grid size", "Divide the grid size by 2", Keys.HalfGridSizeKey);
		
		static GUIContent scaleLargeLabel		= new GUIContent("scale");
		static GUIContent scaleSmallLabel		= new GUIContent("scl");
		static GUIContent scalePlusLabel		= new GUIContent("+");
		static GUIContent scaleMinusLabel		= new GUIContent("-");
		static GUIContent scaleUnitLabel		= new GUIContent("%");
		
		static ToolTip scaleTooltip				= new ToolTip("Scale snapping", "Here you can set scale snapping.");
		static ToolTip scalePlusTooltip			= new ToolTip("Increase scale snapping", "Multiply the scale snapping by 10");
		static ToolTip scaleMinnusTooltip		= new ToolTip("Decrease scale snapping", "Divide the scale snapping by 10");

		static GUIContent angleLargeLabel		= new GUIContent("angle");
		static GUIContent angleSmallLabel		= new GUIContent("ang");
		static GUIContent anglePlusLabel		= new GUIContent("+");
		static GUIContent angleMinusLabel		= new GUIContent("-");
		static GUIContent angleUnitLabel		= new GUIContent("°");
		
		static ToolTip angleTooltip				= new ToolTip("Angle snapping", "Here you can set rotational snapping.");
		static ToolTip anglePlusTooltip			= new ToolTip("Double angle snapping", "Multiply the rotational snapping by 2");
		static ToolTip angleMinnusTooltip		= new ToolTip("Half angle snapping", "Divide the rotational snapping by 2");



//		static GUILayoutOption EnumMaxWidth		= GUILayout.MaxWidth(165);
//		static GUILayoutOption EnumMinWidth		= GUILayout.MinWidth(20);
//		static GUILayoutOption MinSnapWidth		= GUILayout.MinWidth(30);
//		static GUILayoutOption MaxSnapWidth		= GUILayout.MaxWidth(70);

		static GUIStyle			miniTextStyle;
		static GUIStyle			textInputStyle;

		static bool localStyles = false;

		static int BottomBarGUIHash = "BottomBarGUI".GetHashCode();

		public static void ShowGUI(SceneView sceneView, bool haveOffset = true)
		{
			if (!localStyles)
			{
				miniTextStyle = new GUIStyle(EditorStyles.miniLabel);
				miniTextStyle.contentOffset = new Vector2(0, -1);
				textInputStyle = new GUIStyle(EditorStyles.miniTextField);
				textInputStyle.padding.top--;
				textInputStyle.margin.top+=2;
				localStyles = true;
			}
			GUIStyleUtility.InitStyles();
			if (sceneView != null)
			{
				float height	= sceneView.position.height;//Screen.height;
				float width		= sceneView.position.width;//Screen.width;
				Rect bottomBarRect;
				if (haveOffset)
				{
#if UNITY_5_5_OR_NEWER
					bottomBarRect = new Rect(0, height - (GUIStyleUtility.BottomToolBarHeight + 18), 
											  width, GUIStyleUtility.BottomToolBarHeight);
#else
					bottomBarRect = new Rect(0, height - (GUIStyleUtility.BottomToolBarHeight + SceneView.kToolbarHeight + 1),
												width, GUIStyleUtility.BottomToolBarHeight);
#endif
				} else
					bottomBarRect = new Rect(0, height - (GUIStyleUtility.BottomToolBarHeight + 1), width, GUIStyleUtility.BottomToolBarHeight);

				try
				{ 
					Handles.BeginGUI();
					
					bool prevGUIChanged = GUI.changed;
					if (Event.current.type == EventType.Repaint)
						GUIStyleUtility.BottomToolBarStyle.Draw(bottomBarRect, false, false, false, false);
					OnBottomBarGUI(sceneView, bottomBarRect);
					GUI.changed = prevGUIChanged || GUI.changed;
					
					int controlID = GUIUtility.GetControlID(BottomBarGUIHash, FocusType.Keyboard, bottomBarRect);
					var type = Event.current.GetTypeForControl(controlID);
					//Debug.Log(controlID + " " + GUIUtility.hotControl + " " + type + " " + bottomBarRect.Contains(Event.current.mousePosition));
					switch (type)
					{
						case EventType.MouseDown: { if (bottomBarRect.Contains(Event.current.mousePosition)) { GUIUtility.hotControl = controlID; GUIUtility.keyboardControl = controlID; Event.current.Use(); } break; }
						case EventType.MouseMove: { if (bottomBarRect.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
						case EventType.MouseUp:   { if (GUIUtility.hotControl == controlID) { GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0; Event.current.Use(); } break; }
						case EventType.MouseDrag: { if (GUIUtility.hotControl == controlID) { Event.current.Use(); } break; }
						case EventType.ScrollWheel: { if (bottomBarRect.Contains(Event.current.mousePosition)) { Event.current.Use(); } break; }
					}

					//TooltipUtility.HandleAreaOffset(new Vector2(-bottomBarRect.xMin, -bottomBarRect.yMin));
				}
				finally
				{
					Handles.EndGUI();
				}
			}
		}

		

		
		static Rect currentRect = new Rect();
		static void OnBottomBarGUI(SceneView sceneView, Rect barSize)
		{
			//if (Event.current.type == EventType.Layout)
			//	return;

			var snapToGrid		= RealtimeCSG.CSGSettings.SnapToGrid;
			var uniformGrid		= RealtimeCSG.CSGSettings.UniformGrid;
			var moveSnapVector  = RealtimeCSG.CSGSettings.SnapVector;
			var rotationSnap	= RealtimeCSG.CSGSettings.SnapRotation;
			var scaleSnap		= RealtimeCSG.CSGSettings.SnapScale;
			var showGrid		= RealtimeCSG.CSGSettings.GridVisible;
			var lockAxisX		= RealtimeCSG.CSGSettings.LockAxisX;
			var lockAxisY		= RealtimeCSG.CSGSettings.LockAxisY;
			var lockAxisZ		= RealtimeCSG.CSGSettings.LockAxisZ;
			var distanceUnit	= RealtimeCSG.CSGSettings.DistanceUnit;
			var helperSurfaces  = RealtimeCSG.CSGSettings.VisibleHelperSurfaces;
			var showWireframe	= RealtimeCSG.CSGSettings.IsWireframeShown(sceneView);
			var skin			= GUIStyleUtility.Skin;
			var updateSurfaces	= false;
			bool wireframeModified = false;

			var viewWidth = sceneView.position.width;

			float layoutHeight = barSize.height;
			float layoutX = 6.0f;

			bool modified = false;
			GUI.changed = false;
			{
				currentRect.width	= 27;
				currentRect.y		= 0;
				currentRect.height	= layoutHeight - currentRect.y;
				currentRect.y		+= barSize.y;
				currentRect.x		= layoutX;
				layoutX += currentRect.width;

				#region "Grid" button
				if (showGrid)
				{
					showGrid = GUI.Toggle(currentRect, showGrid, skin.gridIconOn, EditorStyles.toolbarButton);
				} else
				{
					showGrid = GUI.Toggle(currentRect, showGrid, skin.gridIcon,   EditorStyles.toolbarButton);
				}
				//(x:6.00, y:0.00, width:27.00, height:18.00)
				TooltipUtility.SetToolTip(showGridTooltip, currentRect);
				#endregion

				if (viewWidth >= 800)
					layoutX += 6; //(x:33.00, y:0.00, width:6.00, height:6.00)
					
				var prevBackgroundColor = GUI.backgroundColor;
				var lockedBackgroundColor = skin.lockedBackgroundColor;
				if (lockAxisX)
					GUI.backgroundColor = lockedBackgroundColor;

				#region "X" lock button
				currentRect.width	= 17;
				currentRect.y		= 0;
				currentRect.height	= layoutHeight - currentRect.y;
				currentRect.y		+= barSize.y;
				currentRect.x		= layoutX;
				layoutX += currentRect.width;

				lockAxisX = !GUI.Toggle(currentRect, !lockAxisX, xLabel, skin.xToolbarButton);
				//(x:39.00, y:0.00, width:17.00, height:18.00)
				if (lockAxisX)
					TooltipUtility.SetToolTip(xTooltipOn, currentRect);
				else
					TooltipUtility.SetToolTip(xTooltipOff, currentRect);
				GUI.backgroundColor = prevBackgroundColor;
				#endregion
												
				#region "Y" lock button
				currentRect.x		= layoutX;
				layoutX += currentRect.width;

				if (lockAxisY)
					GUI.backgroundColor = lockedBackgroundColor;
				lockAxisY = !GUI.Toggle(currentRect, !lockAxisY, yLabel, skin.yToolbarButton);
				//(x:56.00, y:0.00, width:17.00, height:18.00)
				if (lockAxisY)
					TooltipUtility.SetToolTip(yTooltipOn, currentRect);
				else
					TooltipUtility.SetToolTip(yTooltipOff, currentRect);
				GUI.backgroundColor = prevBackgroundColor;
				#endregion
						
				#region "Z" lock button
				currentRect.x		= layoutX;
				layoutX += currentRect.width;

				if (lockAxisZ)
					GUI.backgroundColor = lockedBackgroundColor;
				lockAxisZ = !GUI.Toggle(currentRect, !lockAxisZ, zLabel, skin.zToolbarButton);
				//(x:56.00, y:0.00, width:17.00, height:18.00)
				if (lockAxisZ)
					TooltipUtility.SetToolTip(zTooltipOn, currentRect);
				else
					TooltipUtility.SetToolTip(zTooltipOff, currentRect);
				GUI.backgroundColor = prevBackgroundColor;
				#endregion
			}
			modified = GUI.changed || modified;

			if (viewWidth >= 800)
				layoutX += 6; // (x:91.00, y:0.00, width:6.00, height:6.00)
				
			#region "SnapToGrid" button
			GUI.changed = false;
			{
				currentRect.width	= 27;
				currentRect.y		= 0;
				currentRect.height	= layoutHeight - currentRect.y;
				currentRect.y		+= barSize.y;
				currentRect.x		= layoutX;
				layoutX += currentRect.width;

				snapToGrid = GUI.Toggle(currentRect, snapToGrid, GUIStyleUtility.Skin.snappingIconOn, EditorStyles.toolbarButton);
				//(x:97.00, y:0.00, width:27.00, height:18.00)
				TooltipUtility.SetToolTip(snapToGridTooltip, currentRect);

				layoutX += 4;
			}
			modified = GUI.changed || modified;
			#endregion
				
			if (viewWidth >= 460)
			{
				if (snapToGrid)
				{
					#region "Position" label
					if (viewWidth >= 500)
					{ 
						if (viewWidth >= 865)
						{
							currentRect.width	= 44;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							uniformGrid = GUI.Toggle(currentRect, uniformGrid, positionLargeLabel, miniTextStyle);
							//(x:128.00, y:2.00, width:44.00, height:16.00)

							TooltipUtility.SetToolTip(positionTooltip, currentRect);
						} else
						{
							currentRect.width	= 22;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							uniformGrid = GUI.Toggle(currentRect, uniformGrid, positionSmallLabel, miniTextStyle);
							//(x:127.00, y:2.00, width:22.00, height:16.00)

							TooltipUtility.SetToolTip(positionTooltip, currentRect);
						}
					}
					#endregion
							
					layoutX += 2;

					#region "Position" field
					if (uniformGrid || viewWidth < 515)
					{
						EditorGUI.showMixedValue = !(moveSnapVector.x == moveSnapVector.y && moveSnapVector.x == moveSnapVector.z);
						GUI.changed = false;
						{
							currentRect.width	= 70;
							currentRect.y		= 3;
							currentRect.height	= layoutHeight - (currentRect.y - 1);
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;
							
							moveSnapVector.x = Units.DistanceUnitToUnity(distanceUnit, EditorGUI.DoubleField(currentRect, Units.UnityToDistanceUnit(distanceUnit, moveSnapVector.x), textInputStyle));//, MinSnapWidth, MaxSnapWidth));
							//(x:176.00, y:3.00, width:70.00, height:16.00)
						}
						if (GUI.changed)
						{
							modified = true;
							moveSnapVector.y = moveSnapVector.x;
							moveSnapVector.z = moveSnapVector.x;
						}
						EditorGUI.showMixedValue = false;
					} else
					{
						GUI.changed = false;
						{
							currentRect.width	= 70;
							currentRect.y		= 3;
							currentRect.height	= layoutHeight - (currentRect.y - 1);
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;
							layoutX ++;

							moveSnapVector.x = Units.DistanceUnitToUnity(distanceUnit, EditorGUI.DoubleField(currentRect, Units.UnityToDistanceUnit(distanceUnit, moveSnapVector.x), textInputStyle));//, MinSnapWidth, MaxSnapWidth));
							//(x:175.00, y:3.00, width:70.00, height:16.00)
								

							currentRect.x		= layoutX;
							layoutX += currentRect.width;
							layoutX ++;

							moveSnapVector.y = Units.DistanceUnitToUnity(distanceUnit, EditorGUI.DoubleField(currentRect, Units.UnityToDistanceUnit(distanceUnit, moveSnapVector.y), textInputStyle));//, MinSnapWidth, MaxSnapWidth));
							//(x:247.00, y:3.00, width:70.00, height:16.00)
								

							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							moveSnapVector.z = Units.DistanceUnitToUnity(distanceUnit, EditorGUI.DoubleField(currentRect, Units.UnityToDistanceUnit(distanceUnit, moveSnapVector.z), textInputStyle));//, MinSnapWidth, MaxSnapWidth));
							//(x:319.00, y:3.00, width:70.00, height:16.00)
						}
						modified = GUI.changed || modified;
					}
					#endregion

					layoutX++;

					#region "Position" Unit
					DistanceUnit nextUnit = Units.CycleToNextUnit(distanceUnit);
					GUIContent   unitText = Units.GetUnitGUIContent(distanceUnit);
						
					currentRect.width	= 22;
					currentRect.y		= 2;
					currentRect.height	= layoutHeight - currentRect.y;
					currentRect.y		+= barSize.y;
					currentRect.x		= layoutX;
					layoutX += currentRect.width;

					if (GUI.Button(currentRect, unitText, miniTextStyle))//(x:393.00, y:2.00, width:13.00, height:16.00)
					{
						distanceUnit = nextUnit;
						modified = true;
					}
					#endregion

					layoutX += 2;

					#region "Position" +/-
					if (viewWidth >= 700)
					{
						currentRect.width	= 19;
						currentRect.y		= 2;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, positionPlusLabel,  EditorStyles.miniButtonLeft))  { GridUtility.DoubleGridSize(); moveSnapVector = RealtimeCSG.CSGSettings.SnapVector; }
						//(x:410.00, y:2.00, width:19.00, height:15.00)
						TooltipUtility.SetToolTip(positionPlusTooltip, currentRect);

						currentRect.width	= 17;
						currentRect.y		= 2;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, positionMinusLabel, EditorStyles.miniButtonRight)) { GridUtility.HalfGridSize(); moveSnapVector = RealtimeCSG.CSGSettings.SnapVector; }
						//(x:429.00, y:2.00, width:17.00, height:15.00)
						TooltipUtility.SetToolTip(positionMinnusTooltip, currentRect);
					}
					#endregion

					layoutX += 2;

					#region "Angle" label
					if (viewWidth >= 750)
					{
						if (viewWidth >= 865)
						{
							currentRect.width	= 31;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							GUI.Label(currentRect, angleLargeLabel, miniTextStyle);
							//(x:450.00, y:2.00, width:31.00, height:16.00)
						} else
						{
							currentRect.width	= 22;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							GUI.Label(currentRect, angleSmallLabel, miniTextStyle);
							//(x:355.00, y:2.00, width:22.00, height:16.00)
						}
						TooltipUtility.SetToolTip(angleTooltip, currentRect);
					}
					#endregion
						
					layoutX += 2;

					#region "Angle" field
					GUI.changed = false;
					{
						currentRect.width	= 70;
						currentRect.y		= 3;
						currentRect.height	= layoutHeight - (currentRect.y - 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						rotationSnap = EditorGUI.FloatField(currentRect, rotationSnap, textInputStyle);//, MinSnapWidth, MaxSnapWidth);
						//(x:486.00, y:3.00, width:70.00, height:16.00)
						if (viewWidth <= 750)
							TooltipUtility.SetToolTip(angleTooltip, currentRect);
					}
					modified = GUI.changed || modified;
					#endregion

					layoutX++;

					#region "Angle" Unit
					if (viewWidth >= 370)
					{
						currentRect.width	= 14;
						currentRect.y		= 1;
						currentRect.height	= layoutHeight - currentRect.y;
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;
							
						GUI.Label(currentRect, angleUnitLabel, miniTextStyle);
					}
					#endregion
						
					layoutX += 2;

					#region "Angle" +/-
					if (viewWidth >= 700)
					{
						currentRect.width	= 19;
						currentRect.y		= 1;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, anglePlusLabel, EditorStyles.miniButtonLeft)) { rotationSnap *= 2.0f; modified = true; }
						//(x:573.00, y:2.00, width:19.00, height:15.00)
						TooltipUtility.SetToolTip(anglePlusTooltip, currentRect);
							

						currentRect.width	= 17;
						currentRect.y		= 1;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, angleMinusLabel, EditorStyles.miniButtonRight)) { rotationSnap /= 2.0f; modified = true; }
						//(x:592.00, y:2.00, width:17.00, height:15.00)
						TooltipUtility.SetToolTip(angleMinnusTooltip, currentRect);
					}
					#endregion

					layoutX += 2;

					#region "Scale" label
					if (viewWidth >= 750)
					{
						if (viewWidth >= 865)
						{
							currentRect.width	= 31;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							GUI.Label(currentRect, scaleLargeLabel, miniTextStyle);
							//(x:613.00, y:2.00, width:31.00, height:16.00)
						} else
						{
							currentRect.width	= 19;
							currentRect.y		= 1;
							currentRect.height	= layoutHeight - currentRect.y;
							currentRect.y		+= barSize.y;
							currentRect.x		= layoutX;
							layoutX += currentRect.width;

							GUI.Label(currentRect, scaleSmallLabel, miniTextStyle);
							//(x:495.00, y:2.00, width:19.00, height:16.00)
						}
						TooltipUtility.SetToolTip(scaleTooltip, currentRect);
					}
					#endregion
						
					layoutX += 2;

					#region "Scale" field
					GUI.changed = false;
					{
						currentRect.width	= 70;
						currentRect.y		= 3;
						currentRect.height	= layoutHeight - (currentRect.y - 1); 
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						scaleSnap = EditorGUI.FloatField(currentRect, scaleSnap, textInputStyle);//, MinSnapWidth, MaxSnapWidth);
						//(x:648.00, y:3.00, width:70.00, height:16.00)
						if (viewWidth <= 750)
							TooltipUtility.SetToolTip(scaleTooltip, currentRect);
					}
					modified = GUI.changed || modified;
					#endregion

					layoutX ++;
						
					#region "Scale" Unit
					if (viewWidth >= 370)
					{
						currentRect.width	= 15;
						currentRect.y		= 1;
						currentRect.height	= layoutHeight - currentRect.y; 
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						GUI.Label(currentRect, scaleUnitLabel, miniTextStyle);
						//(x:722.00, y:2.00, width:15.00, height:16.00)
					}
					#endregion
						
					layoutX += 2;

					#region "Scale" +/-
					if (viewWidth >= 700)
					{
						currentRect.width	= 19;
						currentRect.y		= 2;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, scalePlusLabel, EditorStyles.miniButtonLeft)) { scaleSnap *= 10.0f; modified = true; }
						//(x:741.00, y:2.00, width:19.00, height:15.00)
						TooltipUtility.SetToolTip(scalePlusTooltip, currentRect);
							

						currentRect.width	= 17;
						currentRect.y		= 2;
						currentRect.height	= layoutHeight - (currentRect.y + 1);
						currentRect.y		+= barSize.y;
						currentRect.x		= layoutX;
						layoutX += currentRect.width;

						if (GUI.Button(currentRect, scaleMinusLabel, EditorStyles.miniButtonRight)) { scaleSnap /= 10.0f; modified = true; }
						//(x:760.00, y:2.00, width:17.00, height:15.00)
						TooltipUtility.SetToolTip(scaleMinnusTooltip, currentRect);
					}
					#endregion
				}
			}


			var prevLayoutX = layoutX;
				
			layoutX = viewWidth;

				
			#region "Rebuild"
			currentRect.width	= 27;
			currentRect.y		= 0;
			currentRect.height	= layoutHeight - currentRect.y; 
			currentRect.y		+= barSize.y;
			layoutX -= currentRect.width;
			currentRect.x		= layoutX;

			if (GUI.Button(currentRect, GUIStyleUtility.Skin.rebuildIcon, EditorStyles.toolbarButton))
			{
				Debug.Log("Starting complete rebuild");

				var text = new System.Text.StringBuilder();

				InternalCSGModelManager.skipRefresh = true;
				RealtimeCSG.CSGSettings.Reload();
				SceneViewEventHandler.UpdateDefines();

				InternalCSGModelManager.registerTime = 0.0;
				InternalCSGModelManager.validateTime = 0.0;
				InternalCSGModelManager.updateHierarchyTime = 0.0;

				var startTime = EditorApplication.timeSinceStartup;
				InternalCSGModelManager.Rebuild();
				InternalCSGModelManager.OnHierarchyModified();
				var hierarchy_update_endTime = EditorApplication.timeSinceStartup;				
				text.AppendFormat(CultureInfo.InvariantCulture, "Full hierarchy rebuild in {0:F} ms. ", (hierarchy_update_endTime - startTime) * 1000);


				CSGBindings.RebuildAll();
				var csg_endTime = EditorApplication.timeSinceStartup;
				text.AppendFormat(CultureInfo.InvariantCulture, "Full CSG rebuild done in {0:F} ms. ", (csg_endTime - hierarchy_update_endTime) * 1000);

				InternalCSGModelManager.UpdateMeshes(text);

				updateSurfaces = true;
				SceneViewEventHandler.ResetUpdateRoutine();
				RealtimeCSG.CSGSettings.Save();
				InternalCSGModelManager.skipRefresh = false;

				Debug.Log(text.ToString());
			}
			//(x:1442.00, y:0.00, width:27.00, height:18.00)
			TooltipUtility.SetToolTip(rebuildTooltip, currentRect);
			#endregion

			if (viewWidth >= 800)
				layoutX -= 6; //(x:1436.00, y:0.00, width:6.00, height:6.00)

			#region "Helper Surface Flags" Mask
			if (viewWidth >= 250)
			{
				GUI.changed = false;
				{
					prevLayoutX += 8;  // extra space
					prevLayoutX += 26; // width of "Show wireframe" button

					currentRect.width	= Mathf.Max(20, Mathf.Min(165, (viewWidth - prevLayoutX - currentRect.width)));

					currentRect.y		= 0;
					currentRect.height	= layoutHeight - currentRect.y; 
					currentRect.y		+= barSize.y;
					layoutX -= currentRect.width;
					currentRect.x		= layoutX;

					SurfaceVisibilityPopup.Button(currentRect);
					
					//(x:1267.00, y:2.00, width:165.00, height:16.00)
					TooltipUtility.SetToolTip(helperSurfacesTooltip, currentRect);
				}
				if (GUI.changed)
				{
					updateSurfaces = true;
					modified = true;
				}
			}
			#endregion

			#region "Show wireframe" button
			GUI.changed = false;
			currentRect.width	= 26;
			currentRect.y		= 0;
			currentRect.height	= layoutHeight - currentRect.y; 
			currentRect.y		+= barSize.y;
			layoutX -= currentRect.width;
			currentRect.x		= layoutX;

			if (showWireframe)
			{
				showWireframe = GUI.Toggle(currentRect, showWireframe, GUIStyleUtility.Skin.wireframe, EditorStyles.toolbarButton);
				//(x:1237.00, y:0.00, width:26.00, height:18.00)
			} else
			{
				showWireframe = GUI.Toggle(currentRect, showWireframe, GUIStyleUtility.Skin.wireframeOn, EditorStyles.toolbarButton);
				//(x:1237.00, y:0.00, width:26.00, height:18.00)
			}
			TooltipUtility.SetToolTip(showWireframeTooltip, currentRect);
			if (GUI.changed)
			{
				wireframeModified = true;
				modified = true;
			}
			#endregion





			#region Capture mouse clicks in empty space
			var mousePoint  = Event.current.mousePosition;
			int controlID = GUIUtility.GetControlID(BottomBarEditorOverlayHash, FocusType.Passive, barSize);
			switch (Event.current.GetTypeForControl(controlID))
			{
				case EventType.MouseDown:	{ if (barSize.Contains(mousePoint)) { GUIUtility.hotControl = controlID; GUIUtility.keyboardControl = controlID; Event.current.Use(); } break; }
				case EventType.MouseMove:	{ if (barSize.Contains(mousePoint)) { Event.current.Use(); } break; }
				case EventType.MouseUp:		{ if (GUIUtility.hotControl == controlID) { GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0; Event.current.Use(); } break; }
				case EventType.MouseDrag:	{ if (GUIUtility.hotControl == controlID) { Event.current.Use(); } break; }
				case EventType.ScrollWheel: { if (barSize.Contains(mousePoint)) { Event.current.Use(); } break; }
			}
			#endregion



			#region Store modified values
			rotationSnap = Mathf.Max(1.0f, Mathf.Abs((360 + (rotationSnap % 360))) % 360);
			moveSnapVector.x = Mathf.Max(1.0f / 1024.0f, moveSnapVector.x);
			moveSnapVector.y = Mathf.Max(1.0f / 1024.0f, moveSnapVector.y);
			moveSnapVector.z = Mathf.Max(1.0f / 1024.0f, moveSnapVector.z);
			
			scaleSnap = Mathf.Max(MathConstants.MinimumScale, scaleSnap);
						
			RealtimeCSG.CSGSettings.SnapToGrid				= snapToGrid;
			RealtimeCSG.CSGSettings.SnapVector				= moveSnapVector;
			RealtimeCSG.CSGSettings.SnapRotation			= rotationSnap;
			RealtimeCSG.CSGSettings.SnapScale				= scaleSnap;
			RealtimeCSG.CSGSettings.UniformGrid				= uniformGrid;
//			RealtimeCSG.Settings.SnapVertex					= vertexSnap;
			RealtimeCSG.CSGSettings.GridVisible				= showGrid;
			RealtimeCSG.CSGSettings.LockAxisX				= lockAxisX;
			RealtimeCSG.CSGSettings.LockAxisY				= lockAxisY;
			RealtimeCSG.CSGSettings.LockAxisZ				= lockAxisZ;
			RealtimeCSG.CSGSettings.DistanceUnit			= distanceUnit;
			RealtimeCSG.CSGSettings.VisibleHelperSurfaces	= helperSurfaces;

			if (wireframeModified)
			{
				RealtimeCSG.CSGSettings.SetWireframeShown(sceneView, showWireframe);
			}

			if (updateSurfaces)
			{
				MeshInstanceManager.UpdateHelperSurfaceVisibility();
			}
			
			if (modified)
			{
				GUI.changed = true;
				RealtimeCSG.CSGSettings.UpdateSnapSettings();
				RealtimeCSG.CSGSettings.Save();
				SceneView.RepaintAll();
			}
			#endregion
		}
		
	}
}
