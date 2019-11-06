using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using InternalRealtimeCSG;

namespace RealtimeCSG
{

	internal static class SurfaceToolGUI
	{
		private static readonly int SceneViewSurfaceOverlayHash = "SceneViewSurfaceOverlay".GetHashCode();

		const int materialSize		= 100;
		const int windowWidth		= materialSize;
		const int justifyButtonSize = 22;
		const int labelWidthConst	= 56;
		const int buttonWidth		= 42;

		[NonSerialized]
		private static MaterialEditor	materialEditor	= null;

		private static GUIContent		ContentSurfacesLabel;

		private static readonly GUIContent	ContentJustifyLabel		= new GUIContent("UV Justify");
		private static readonly GUIContent	ContentJustifyUpLeft	= new GUIContent("↖");
		private static readonly GUIContent	ContentJustifyUp		= new GUIContent("↑");
		private static readonly GUIContent	ContentJustifyUpRight	= new GUIContent("↗");
		private static readonly GUIContent	ContentJustifyLeft		= new GUIContent("←");
		private static readonly GUIContent	ContentJustifyCenter	= new GUIContent("C");
		private static readonly GUIContent	ContentJustifyRight		= new GUIContent("→");
		private static readonly GUIContent	ContentJustifyDownLeft	= new GUIContent("↙");
		private static readonly GUIContent	ContentJustifyDown		= new GUIContent("↓");
		private static readonly GUIContent	ContentJustifyDownRight	= new GUIContent("↘");
		private static readonly ToolTip		ToolTipJustifyUpLeft	= new ToolTip("Top Left Align",		"Align the texture to the top (smallest U) left (smallest V) corner of the surface");
		private static readonly ToolTip		ToolTipJustifyUp		= new ToolTip("Top Align",			"Align the texture to the top (smallest U) side of the surface");
		private static readonly ToolTip		ToolTipJustifyUpRight	= new ToolTip("Top Right Align",	"Align the texture to the top (smallest U) right (largest V) corner of the surface");
		private static readonly ToolTip		ToolTipJustifyLeft		= new ToolTip("Left Align",			"Align the texture to the left (smallest V) side of the surface");
		private static readonly ToolTip		ToolTipJustifyCenter	= new ToolTip("Center Align",		"Align the texture to the center of the surface");
		private static readonly ToolTip		ToolTipJustifyRight		= new ToolTip("Right Align",		"Align the texture to the right (largest V) side of the surface");
		private static readonly ToolTip		ToolTipJustifyDownLeft	= new ToolTip("Bottom Left Align",	"Align the texture to the bottom (largest U) left (smallest U) corner of the surface");
		private static readonly ToolTip		ToolTipJustifyDown		= new ToolTip("Bottom Align",		"Align the texture to the bottom (largest U) side of the surface");
		private static readonly ToolTip		ToolTipJustifyDownRight = new ToolTip("Bottom Right Align", "Align the texture to the bottom (largest U) right (largest U) corner of the surface");

		private static readonly GUIContent	ContentMaterial			= new GUIContent("Material");

		private static readonly GUIContent	ContentLockTexture		= new GUIContent("Lock texture to object");		
		private static readonly ToolTip		ToolTipLockTexture		= new ToolTip("Lock texture to object", "When not set, the texture will stay\n" +
																											"in the same world position when the\n" +
																											"brush is moved in the world. When \n" +
																											"set it'll move with the brush.");
		

		private static readonly GUIContent	ContentReset			= new GUIContent("Reset");
		private static readonly GUIContent	ContentResetXY			= new GUIContent("UV");
		private static readonly GUIContent	ContentResetX			= new GUIContent("U");
		private static readonly GUIContent	ContentResetY			= new GUIContent("Y");
		private static readonly ToolTip		ToolTipResetXY			= new ToolTip("Reset U & V coordinates", "Reset the texture coordinates in the U and V direction");
		private static readonly ToolTip		ToolTipResetX			= new ToolTip("Reset U coordinates", "Reset the texture coordinates in the U direction");
		private static readonly ToolTip		ToolTipResetY			= new ToolTip("Reset V coordinates", "Reset the texture coordinates in the V direction");

		private static readonly GUIContent	ContentFit				= new GUIContent("Fit");
		private static readonly GUIContent	ContentFitXY			= new GUIContent("UV");
		private static readonly GUIContent	ContentFitX				= new GUIContent("U");
		private static readonly GUIContent	ContentFitY				= new GUIContent("V");
		private static readonly ToolTip		ToolTipFitXY			= new ToolTip("Fit U & V coordinates", "Fit the texture coordinates in both the U and V direction");
		private static readonly ToolTip		ToolTipFitX				= new ToolTip("Fit U coordinates", "Fit the texture coordinates in the U direction");
		private static readonly ToolTip		ToolTipFitY				= new ToolTip("Fit V coordinates", "Fit the texture coordinates in the V direction");

		private static readonly GUIContent	ContentFlip				= new GUIContent("Reverse");
		private static readonly GUIContent	ContentFlipXY			= new GUIContent("UV");
		private static readonly GUIContent	ContentFlipX			= new GUIContent("U");
		private static readonly GUIContent	ContentFlipY			= new GUIContent("V");
		private static readonly ToolTip		ToolTipFlipXY			= new ToolTip("Flip U & V coordinates", "Reverse the texture coordinates in both the U and V direction");
		private static readonly ToolTip		ToolTipFlipX			= new ToolTip("Flip U coordinates", "Reverse the texture coordinates in the U direction");
		private static readonly ToolTip		ToolTipFlipY			= new ToolTip("Flip V coordinates", "Reverse the texture coordinates in the V direction");

		private static readonly GUIContent	ContentUVScale			= new GUIContent("UV Scale");
		private static readonly GUIContent	ContentScale			= new GUIContent("Scale");
		private static readonly ToolTip		ToolTipScaleUV			= new ToolTip("UV Scale", "Sets the U and V scale of the texture on the surface of the brush.");
		private static readonly GUIContent	ContentDoubleScale		= new GUIContent("x2");
		private static readonly GUIContent	ContentHalfScale		= new GUIContent("/2");
		private static readonly ToolTip		ToolTipDoubleScale		= new ToolTip("Double the scale", "Make the texturing bigger by doubling the scale. This will decrease the tiling (if any).");
		private static readonly ToolTip		ToolTipHalfScale		= new ToolTip("Half the scale", "Make the texturing smaller by halving the scale. This will increase the tiling (if any).");

		private static readonly GUIContent	ContentUSymbol			= new GUIContent("U");
		private static readonly GUIContent	ContentVSymbol			= new GUIContent("V");
		private static readonly GUIContent	ContentOffset			= new GUIContent("UV Offset");
		private static readonly ToolTip		ToolTipOffsetUV			= new ToolTip("UV Offset", "Sets the U and V position of the texture on the surface of the brush.");
		private static readonly GUIContent	ContentRotate			= new GUIContent("Rotation");
		private static readonly ToolTip		ToolTipRotation			= new ToolTip("Rotation", "Sets the rotation the texture on the surface of the brush.");
		private static readonly GUIContent	ContentAngleSymbol		= new GUIContent("°");
		private static readonly GUIContent	ContentRotate90Negative	= new GUIContent("-90°");
		private static readonly GUIContent	ContentRotate90Positive	= new GUIContent("+90°");
		private static readonly ToolTip		ToolTipRotate90Negative = new ToolTip("Rotate texture -90°", "Subtracts 90 from the rotation of the texture");
		private static readonly ToolTip		ToolTipRotate90Positive = new ToolTip("Rotate texture +90°", "Adds 90 to the rotation of the texture");
				
		private static readonly GUIContent	ContentUnSmoothSurfaces		= new GUIContent("Un-smooth");
		private static readonly GUIContent	ContentSmoothSurfaces		= new GUIContent("Smooth");
		private static readonly ToolTip		ToolTipUnSmoothSurfaces		= new ToolTip("Un-smooth", "Remove all selected smoothed surfaces from smoothing groups");
		private static readonly ToolTip		ToolTipSmoothSurfaces		= new ToolTip("Smooth", "Put all selected surfaces in same smoothing group");


		private static readonly GUILayoutOption		minFloatFieldWidth  = GUILayout.MinWidth(20);
		private static readonly GUILayoutOption		labelWidth          = GUILayout.Width(labelWidthConst);
		private static readonly GUILayoutOption		angleButtonWidth    = GUILayout.Width(buttonWidth);
		public static readonly GUILayoutOption		smallLabelWidth		= GUILayout.Width(45);
		public static readonly GUILayoutOption		largeLabelWidth		= GUILayout.Width(80);
		private static readonly GUILayoutOption		unitWidth			= GUILayout.Width(14);

		private static readonly GUILayoutOption		justifyButtonWidth	= GUILayout.Width(justifyButtonSize);
		private static readonly GUILayoutOption		justifyButtonHeight	= GUILayout.Height(justifyButtonSize);
		private static readonly GUILayoutOption[]	justifyButtonLayout = new GUILayoutOption[] { justifyButtonWidth, justifyButtonHeight };

		private static readonly GUILayoutOption		materialWidth		= GUILayout.Width(materialSize);
		private static readonly GUILayoutOption		materialHeight		= GUILayout.Height(materialSize);
		private static readonly GUILayoutOption[]	materialDoubleWidth	= new GUILayoutOption[] { GUILayout.Width(windowWidth) };

		static void InitLocalStyles()
		{
			if (ContentSurfacesLabel != null)
				return;
			ContentSurfacesLabel = new GUIContent(GUIStyleUtility.brushEditModeNames[(int)ToolEditMode.Surfaces]);
		}

		static Material GetDragMaterial()
		{
			if (DragAndDrop.objectReferences != null &&
				DragAndDrop.objectReferences.Length > 0)
			{
				var dragMaterials = new List<Material>();
				foreach (var obj in DragAndDrop.objectReferences)
				{
					var dragMaterial = obj as Material;
					if (dragMaterial == null)
						continue;
					dragMaterials.Add(dragMaterial);
				}
				if (dragMaterials.Count == 1)
					return dragMaterials[0];
			}
			return null;
		}

		static void OnGUIContentsJustify(bool isSceneGUI, SelectedBrushSurface[] selectedBrushSurfaces)
		{
			GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
			{
				if (!isSceneGUI)
				{
					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					EditorGUILayout.LabelField(ContentJustifyLabel, largeLabelWidth);
				} else
					GUILayout.Label(ContentJustifyLabel);
				GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
				{ 
					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					{
						if (GUILayout.Button(ContentJustifyUpLeft,	justifyButtonLayout)) SurfaceUtility.JustifyLayout(selectedBrushSurfaces, -1,-1);
						TooltipUtility.SetToolTip(ToolTipJustifyUpLeft);
						if (GUILayout.Button(ContentJustifyUp,		justifyButtonLayout)) SurfaceUtility.JustifyLayoutY(selectedBrushSurfaces, -1);
						TooltipUtility.SetToolTip(ToolTipJustifyUp);
						if (GUILayout.Button(ContentJustifyUpRight, justifyButtonLayout)) SurfaceUtility.JustifyLayout(selectedBrushSurfaces,  1,-1);
						TooltipUtility.SetToolTip(ToolTipJustifyUpRight);
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					{
						if (GUILayout.Button(ContentJustifyLeft,   justifyButtonLayout)) SurfaceUtility.JustifyLayoutX(selectedBrushSurfaces, -1);
						TooltipUtility.SetToolTip(ToolTipJustifyLeft);
						if (GUILayout.Button(ContentJustifyCenter, justifyButtonLayout)) SurfaceUtility.JustifyLayout(selectedBrushSurfaces,  0, 0);
						TooltipUtility.SetToolTip(ToolTipJustifyCenter);
						if (GUILayout.Button(ContentJustifyRight,  justifyButtonLayout)) SurfaceUtility.JustifyLayoutX(selectedBrushSurfaces,  1);
						TooltipUtility.SetToolTip(ToolTipJustifyRight);
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
					{
						if (GUILayout.Button(ContentJustifyDownLeft,  justifyButtonLayout)) SurfaceUtility.JustifyLayout(selectedBrushSurfaces, -1, 1);
						TooltipUtility.SetToolTip(ToolTipJustifyDownLeft);
						if (GUILayout.Button(ContentJustifyDown,	  justifyButtonLayout)) SurfaceUtility.JustifyLayoutY(selectedBrushSurfaces, 1);
						TooltipUtility.SetToolTip(ToolTipJustifyDown);
						if (GUILayout.Button(ContentJustifyDownRight, justifyButtonLayout)) SurfaceUtility.JustifyLayout(selectedBrushSurfaces,  1, 1);
						TooltipUtility.SetToolTip(ToolTipJustifyDownRight);
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				
				if (!isSceneGUI)
				{
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();
		}

		static void OnGUIContentsMaterialImage(bool isSceneGUI, Material material, bool mixedValues, SelectedBrushSurface[] selectedBrushSurfaces)
		{
			GUILayout.BeginHorizontal(materialWidth, materialHeight);
			{
				//if (materialEditor == null || prevMaterial != material)
				{
					var editor = materialEditor as Editor;
					Editor.CreateCachedEditor(material, typeof(MaterialEditor), ref editor);
					materialEditor = editor as MaterialEditor;
					//prevMaterial = material;
				}

				if (materialEditor != null)
				{
					var rect = GUILayoutUtility.GetRect(materialSize, materialSize);
					EditorGUI.showMixedValue = mixedValues;
					materialEditor.OnPreviewGUI(rect, GUIStyle.none);
					EditorGUI.showMixedValue = false;
				} else
				{
					GUILayout.Box(new GUIContent(), GUIStyleUtility.emptyMaterialStyle, materialWidth, materialHeight);
				}
			}
			GUILayout.EndHorizontal();
			var currentArea = GUILayoutUtility.GetLastRect();
			var currentPoint = Event.current.mousePosition;
			if (currentArea.Contains(currentPoint))
			{
				if (Event.current.type == EventType.DragUpdated &&
					GetDragMaterial())
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
					Event.current.Use();
				}
				if (Event.current.type == EventType.DragPerform)
				{
					var new_material = GetDragMaterial();
					if (new_material)
					{
						SurfaceUtility.SetMaterials(selectedBrushSurfaces, new_material);
						CSGSettings.DefaultMaterial = new_material;
						CSGSettings.Save();
						GUI.changed = true;
						Event.current.Use();
					}
				}
			}
		}

		/*
		static void OnGUIContentsMaterialInspector(Material material, bool mixedValues)
		{
			//if (materialEditor == null || prevMaterial != material)
			{
				var editor = materialEditor as Editor;
				Editor.CreateCachedEditor(material, typeof(MaterialEditor), ref editor); 
				materialEditor = editor as MaterialEditor;
			}

			if (materialEditor != null)
			{
				EditorGUI.showMixedValue = mixedValues;
				try
				{
					materialEditor.DrawHeader();
					if (materialEditor.PropertiesGUI())
					{
						materialEditor.PropertiesChanged();
					}
				}
				catch
				{}
				EditorGUI.showMixedValue = false;
			}
		}
		*/

		private static void OnGUIContents(bool isSceneGUI, SurfaceEditBrushTool tool)
		{
			CommonGUI.StartToolGUI();
			
			var selectedBrushSurfaces = (tool == null) ? new SelectedBrushSurface[0] : tool.GetSelectedSurfaces();
			var enabled = selectedBrushSurfaces.Length > 0;
			EditorGUI.BeginDisabledGroup(!enabled);
			{
				Material material = null;
				var currentTexGen = new TexGen();

				var haveTexgen				= false;
				var multipleColors			= !enabled;
				var multipleTranslationX	= !enabled;
				var multipleTranslationY	= !enabled;
				var multipleScaleX			= !enabled;
				var multipleScaleY			= !enabled;
				var multipleRotationAngle	= !enabled;
				var multipleMaterials		= !enabled;
				bool? textureLocked			= null;

				bool foundHelperMaterial	= false;
				RenderSurfaceType? firstRenderSurfaceType = null;
				Material firstMaterial		= null;
				if (selectedBrushSurfaces.Length > 0)
				{
					for (var i = 0; i < selectedBrushSurfaces.Length; i++)
					{
						var brush			= selectedBrushSurfaces[i].brush;
						if (!brush)
							continue;
						var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
						if (surfaceIndex >= brush.Shape.Surfaces.Length)
						{
							Debug.LogWarning("surface_index >= brush.Shape.Surfaces.Length");
							continue; 
						}
						var texGenIndex	= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;
						if (texGenIndex >= brush.Shape.TexGens.Length)
						{
							Debug.LogWarning("texGen_index >= brush.Shape.TexGens.Length");
							continue;
						}
						var brushCache = InternalCSGModelManager.GetBrushCache(brush);
						var model = (brushCache != null) ? brushCache.childData.Model : null;
						Material foundMaterial;
						var texGenFlags = brush.Shape.TexGenFlags[texGenIndex];
						if (model && (!model.IsRenderable || model.ShadowsOnly))
						{
							foundHelperMaterial = true;
							if (!firstRenderSurfaceType.HasValue)
								firstRenderSurfaceType = ModelTraits.GetModelSurfaceType(model);
							foundMaterial = null;
						} else
						if ((texGenFlags & TexGenFlags.NoRender) == TexGenFlags.NoRender)
						{
							foundHelperMaterial = true;
							if (!firstRenderSurfaceType.HasValue)
							{
								if ((texGenFlags & TexGenFlags.NoCastShadows) != TexGenFlags.NoCastShadows)
								{
									firstRenderSurfaceType = RenderSurfaceType.ShadowOnly;
								} else
								if ((texGenFlags & TexGenFlags.NoCollision) != TexGenFlags.NoCollision)
								{
									firstRenderSurfaceType = RenderSurfaceType.Collider;
								} else
								{
									firstRenderSurfaceType = RenderSurfaceType.Hidden;
								}
							}
							foundMaterial = null;
						} else
						{
							var surfaceMaterial = brush.Shape.TexGens[texGenIndex].RenderMaterial;
							if (!foundHelperMaterial)
							{
								var surfaceType		= MaterialUtility.GetMaterialSurfaceType(surfaceMaterial);
								if (!firstRenderSurfaceType.HasValue)
									firstRenderSurfaceType = surfaceType;
								foundHelperMaterial = surfaceType != RenderSurfaceType.Normal;
							}
							foundMaterial	= surfaceMaterial;
						}
						if ((texGenFlags & TexGenFlags.WorldSpaceTexture) == TexGenFlags.WorldSpaceTexture)
						{
							if (i == 0) textureLocked = false;
							else if (textureLocked.HasValue && textureLocked.Value)
								textureLocked = null;
						} else
						{
							if (i == 0) textureLocked = true;
							else if (textureLocked.HasValue && !textureLocked.Value)
								textureLocked = null;
						}
						if (foundMaterial != material)
						{
							if (!material)
							{
								firstMaterial = foundMaterial;
								material = foundMaterial;
							} else
								multipleMaterials = true;
						}
						if (!haveTexgen)
						{
							currentTexGen = brush.Shape.TexGens[texGenIndex];
							haveTexgen = true;
						} else
						{
							if (!multipleColors)
							{ 
								var color			= brush.Shape.TexGens[texGenIndex].Color;
								multipleColors		= currentTexGen.Color.a != color.a ||
													  currentTexGen.Color.b != color.b ||
													  currentTexGen.Color.g != color.g ||
													  currentTexGen.Color.r != color.r;
							}
							if (!multipleScaleX || !multipleScaleY)
							{ 
								var scale			= brush.Shape.TexGens[texGenIndex].Scale;
								multipleScaleX	= multipleScaleX || currentTexGen.Scale.x != scale.x;
								multipleScaleY	= multipleScaleY || currentTexGen.Scale.y != scale.y;
							}

							if (!multipleTranslationX || !multipleTranslationY)
							{ 
								var translation			= brush.Shape.TexGens[texGenIndex].Translation;
								multipleTranslationX	= multipleTranslationX || currentTexGen.Translation.x != translation.x;
								multipleTranslationY	= multipleTranslationY || currentTexGen.Translation.y != translation.y;
							}

							if (!multipleRotationAngle)
							{
								var rotationAngle		= brush.Shape.TexGens[texGenIndex].RotationAngle;
								multipleRotationAngle	= currentTexGen.RotationAngle != rotationAngle;
							}
						}
					}
					if (foundHelperMaterial && !firstMaterial)
					{
						if (firstRenderSurfaceType.HasValue)
							firstMaterial = MaterialUtility.GetSurfaceMaterial(firstRenderSurfaceType.Value);
						else
							firstMaterial = MaterialUtility.HiddenMaterial;
					}
				}
				
				GUILayout.BeginVertical(isSceneGUI ? materialDoubleWidth : GUIStyleUtility.ContentEmpty);
				{
					GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
					{
						/*
						Color new_color;
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = multipleColors;
							// why doesn't the colorfield return a modified color?
							try
							{
								new_color = EditorGUILayout.ColorField(GUIContent.none, currentTexGen.Color);
							}
							catch
							{
								new_color = currentTexGen.Color;
							}
						}
						if (EditorGUI.EndChangeCheck() || currentTexGen.Color != new_color)
						{
							SurfaceUtility.SetColors(selectedBrushSurfaces, new_color);
						}
						*/
						if (isSceneGUI)
						{
							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{
								EditorGUI.BeginDisabledGroup(material == null);
								{
									GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
									{
										GUILayout.Space(1);
										/*
										Color new_color;
										EditorGUI.BeginChangeCheck();
										{
											EditorGUI.showMixedValue = multipleColors;
											// why doesn't the colorfield return a modified color?
											try
											{
												new_color = EditorGUILayout.ColorField(GUIContent.none, currentTexGen.Color);
											}
											catch 
											{
												new_color = currentTexGen.Color;
											}
										}
										if (EditorGUI.EndChangeCheck() || currentTexGen.Color != new_color)
										{
											SurfaceUtility.SetColors(selectedBrushSurfaces, new_color);
										}
										
										GUILayout.Space(1);
										*/
										Material newMaterial;
										EditorGUI.BeginChangeCheck();
										{
											EditorGUI.showMixedValue = multipleMaterials;
											newMaterial = EditorGUILayout.ObjectField(material, typeof(Material), true) as Material;
											EditorGUI.showMixedValue = false;
										}
										if (EditorGUI.EndChangeCheck())
										{
											if (newMaterial)
											{
												SurfaceUtility.SetMaterials(selectedBrushSurfaces, newMaterial);
												CSGSettings.DefaultMaterial = newMaterial;
												CSGSettings.Save(); 
											}
										}
									}
									GUILayout.EndVertical();
									GUILayout.Space(1);
								}
								EditorGUI.EndDisabledGroup();
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(4);
							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{
								GUILayout.Space(2);
								OnGUIContentsMaterialImage(isSceneGUI, firstMaterial, multipleMaterials, selectedBrushSurfaces);
								GUILayout.BeginHorizontal(materialWidth);
								{
									GUILayout.FlexibleSpace();
									GUILayout.BeginVertical(materialHeight);
									{
										OnGUIContentsJustify(isSceneGUI, selectedBrushSurfaces);
										GUILayout.FlexibleSpace();
									}
									GUILayout.EndVertical();
								}
								GUILayout.EndHorizontal();
								GUILayout.FlexibleSpace();
							}
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.EndVertical();

					if (!isSceneGUI)
						EditorGUILayout.Space();
					
					if (currentTexGen.Scale.x == 0.0f)
						currentTexGen.Scale.x = 1.0f;
					if (currentTexGen.Scale.y == 0.0f)
						currentTexGen.Scale.y = 1.0f;

					const float scale_round = 10000.0f;
					currentTexGen.Scale.x = Mathf.RoundToInt(currentTexGen.Scale.x * scale_round) / scale_round;
					currentTexGen.Scale.y = Mathf.RoundToInt(currentTexGen.Scale.y * scale_round) / scale_round;
					currentTexGen.Translation.x = Mathf.RoundToInt(currentTexGen.Translation.x * scale_round) / scale_round;
					currentTexGen.Translation.y = Mathf.RoundToInt(currentTexGen.Translation.y * scale_round) / scale_round;
					currentTexGen.RotationAngle = Mathf.RoundToInt(currentTexGen.RotationAngle * scale_round) / scale_round;
					
					var leftStyle	= isSceneGUI ? EditorStyles.miniButtonLeft  : GUI.skin.button;
					var middleStyle	= isSceneGUI ? EditorStyles.miniButtonMid   : GUI.skin.button;
					var rightStyle	= isSceneGUI ? EditorStyles.miniButtonRight : GUI.skin.button;

					GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
					{						
						EditorGUI.BeginChangeCheck();
						{
							EditorGUI.showMixedValue = !textureLocked.HasValue;
							textureLocked = EditorGUILayout.ToggleLeft(ContentLockTexture, textureLocked.HasValue ? textureLocked.Value : false);
							TooltipUtility.SetToolTip(ToolTipLockTexture);
						}
						if (EditorGUI.EndChangeCheck())
						{
							SurfaceUtility.SetTextureLock(selectedBrushSurfaces, textureLocked.Value);
						}							
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
					{ 				
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (isSceneGUI)
								EditorGUILayout.LabelField(ContentUVScale, EditorStyles.miniLabel, labelWidth);
							else
								EditorGUILayout.LabelField(ContentUVScale, largeLabelWidth);
							TooltipUtility.SetToolTip(ToolTipScaleUV); 

							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{ 
								if (!isSceneGUI)
									EditorGUILayout.LabelField(ContentUSymbol, unitWidth);
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleScaleX;
									currentTexGen.Scale.x = EditorGUILayout.FloatField(currentTexGen.Scale.x, minFloatFieldWidth);
								}
								if (EditorGUI.EndChangeCheck()) { SurfaceUtility.SetScaleX(selectedBrushSurfaces, currentTexGen.Scale.x); }	
								if (!isSceneGUI)
									EditorGUILayout.LabelField(ContentVSymbol, unitWidth);
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleScaleY;
									currentTexGen.Scale.y = EditorGUILayout.FloatField(currentTexGen.Scale.y, minFloatFieldWidth);
								}
								if (EditorGUI.EndChangeCheck()) { SurfaceUtility.SetScaleY(selectedBrushSurfaces, currentTexGen.Scale.y); }
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (isSceneGUI)
								EditorGUILayout.LabelField(ContentOffset, EditorStyles.miniLabel, labelWidth);
							else
								EditorGUILayout.LabelField(ContentOffset, largeLabelWidth);
							TooltipUtility.SetToolTip(ToolTipOffsetUV);

							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{ 
								if (!isSceneGUI)
									EditorGUILayout.LabelField(ContentUSymbol, unitWidth);
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleTranslationX;
									currentTexGen.Translation.x = EditorGUILayout.FloatField(currentTexGen.Translation.x, minFloatFieldWidth);
								}
								if (EditorGUI.EndChangeCheck()) { SurfaceUtility.SetTranslationX(selectedBrushSurfaces, currentTexGen.Translation.x); }
								
								if (!isSceneGUI)
									EditorGUILayout.LabelField(ContentVSymbol, unitWidth);
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleTranslationY;
									currentTexGen.Translation.y = EditorGUILayout.FloatField(currentTexGen.Translation.y, minFloatFieldWidth);
								}
								if (EditorGUI.EndChangeCheck()) { SurfaceUtility.SetTranslationY(selectedBrushSurfaces, currentTexGen.Translation.y); }
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (isSceneGUI)
								EditorGUILayout.LabelField(ContentRotate, EditorStyles.miniLabel, labelWidth);
							else
								EditorGUILayout.LabelField(ContentRotate, largeLabelWidth);
							TooltipUtility.SetToolTip(ToolTipRotation);

							if (!isSceneGUI)
							{
								GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
							}
							
							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{ 
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleRotationAngle;
									currentTexGen.RotationAngle = EditorGUILayout.FloatField(currentTexGen.RotationAngle, minFloatFieldWidth);
									if (!isSceneGUI)
										EditorGUILayout.LabelField(ContentAngleSymbol, unitWidth);
								}
								if (EditorGUI.EndChangeCheck()) { SurfaceUtility.SetRotationAngle(selectedBrushSurfaces, currentTexGen.RotationAngle); }
							}
							GUILayout.EndHorizontal();
							
							var buttonWidth = isSceneGUI ? new GUILayoutOption[] { angleButtonWidth } : new GUILayoutOption[0];
							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{
								if (GUILayout.Button(ContentRotate90Negative, leftStyle,  buttonWidth)) { SurfaceUtility.AddRotationAngle(selectedBrushSurfaces, -90.0f); }
								TooltipUtility.SetToolTip(ToolTipRotate90Negative);
								if (GUILayout.Button(ContentRotate90Positive, rightStyle, buttonWidth)) { SurfaceUtility.AddRotationAngle(selectedBrushSurfaces, +90.0f); }
								TooltipUtility.SetToolTip(ToolTipRotate90Positive);
							}
							GUILayout.EndHorizontal();
							if (!isSceneGUI)
							{
								GUILayout.EndVertical();
							}
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					if (!isSceneGUI)
						EditorGUILayout.Space();
					
					GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
					{ 				
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (!isSceneGUI)
							{
								GUILayout.Label(ContentFit, largeLabelWidth);
							} else
								GUILayout.Label(ContentFit, EditorStyles.miniLabel, smallLabelWidth);
							if (GUILayout.Button(ContentFitX , leftStyle  )) { SurfaceUtility.FitSurfaceX(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFitX);
							if (GUILayout.Button(ContentFitXY, middleStyle)) { SurfaceUtility.FitSurface(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFitXY);
							if (GUILayout.Button(ContentFitY , rightStyle )) { SurfaceUtility.FitSurfaceY(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFitY);
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (!isSceneGUI)
							{
								GUILayout.Label(ContentReset, largeLabelWidth);
							} else
								GUILayout.Label(ContentReset, EditorStyles.miniLabel, smallLabelWidth);
							if (GUILayout.Button(ContentResetX , leftStyle  )) { SurfaceUtility.ResetSurfaceX(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipResetX);
							if (GUILayout.Button(ContentResetXY, middleStyle)) { SurfaceUtility.ResetSurface(selectedBrushSurfaces);  }
							TooltipUtility.SetToolTip(ToolTipResetXY);
							if (GUILayout.Button(ContentResetY , rightStyle )) { SurfaceUtility.ResetSurfaceY(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipResetY);
							
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (!isSceneGUI)
							{
								GUILayout.Label(ContentFlip, largeLabelWidth);
							} else
								GUILayout.Label(ContentFlip, EditorStyles.miniLabel, smallLabelWidth);
							if (GUILayout.Button(ContentFlipX , leftStyle  ))	{ SurfaceUtility.FlipX(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFlipX);
							if (GUILayout.Button(ContentFlipXY, middleStyle))	{ SurfaceUtility.FlipXY(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFlipXY);
							if (GUILayout.Button(ContentFlipY , rightStyle ))	{ SurfaceUtility.FlipY(selectedBrushSurfaces); }
							TooltipUtility.SetToolTip(ToolTipFlipY);
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							if (!isSceneGUI)
							{
								GUILayout.Label(ContentScale, largeLabelWidth);
							} else
								GUILayout.Label(ContentScale, EditorStyles.miniLabel, smallLabelWidth);
							if (GUILayout.Button(ContentDoubleScale , leftStyle  ))	{ SurfaceUtility.MultiplyScale(selectedBrushSurfaces, 2.0f); }
							TooltipUtility.SetToolTip(ToolTipDoubleScale);
							if (GUILayout.Button(ContentHalfScale   , rightStyle ))	{ SurfaceUtility.MultiplyScale(selectedBrushSurfaces, 0.5f); }
							TooltipUtility.SetToolTip(ToolTipHalfScale);
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					if (!isSceneGUI)
						EditorGUILayout.Space();
					
					if (!isSceneGUI)
					{
						OnGUIContentsJustify(isSceneGUI, selectedBrushSurfaces);
					}

					if (!isSceneGUI)
					{
						EditorGUILayout.Space();
					}

					GUILayout.BeginVertical(isSceneGUI ? GUI.skin.box : GUIStyle.none);
					{
						CommonGUI.OnSurfaceFlagButtons(selectedBrushSurfaces, isSceneGUI);
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							EditorGUI.BeginDisabledGroup(!SurfaceUtility.CanSmooth(selectedBrushSurfaces));
							{
								if (GUILayout.Button(ContentSmoothSurfaces, leftStyle)) { SurfaceUtility.Smooth(selectedBrushSurfaces); }
								TooltipUtility.SetToolTip(ToolTipSmoothSurfaces);
							}
							EditorGUI.EndDisabledGroup();
							EditorGUI.BeginDisabledGroup(!SurfaceUtility.CanUnSmooth(selectedBrushSurfaces));
							{
								if (GUILayout.Button(ContentUnSmoothSurfaces, rightStyle)) { SurfaceUtility.UnSmooth(selectedBrushSurfaces); }
								TooltipUtility.SetToolTip(ToolTipUnSmoothSurfaces);
							}
							EditorGUI.EndDisabledGroup();
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					if (!isSceneGUI)
					{
						EditorGUILayout.Space();
						Material new_material;
						GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
						{
							EditorGUILayout.LabelField(ContentMaterial, largeLabelWidth);
							GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
							{
								EditorGUI.BeginChangeCheck();
								{
									EditorGUI.showMixedValue = multipleMaterials;
									new_material = EditorGUILayout.ObjectField(material, typeof(Material), true) as Material;
									EditorGUI.showMixedValue = false;
								}
								if (EditorGUI.EndChangeCheck())
								{
									if (!new_material)
										new_material = MaterialUtility.MissingMaterial;
									SurfaceUtility.SetMaterials(selectedBrushSurfaces, new_material);
								}
							}
							GUILayout.Space(2);
							GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
							{
								GUILayout.Space(5);
								OnGUIContentsMaterialImage(isSceneGUI, firstMaterial, multipleMaterials, selectedBrushSurfaces);
							}
							GUILayout.EndHorizontal();
							GUILayout.EndVertical();
						}
						GUILayout.EndHorizontal();
						// Unity won't let us do this
						//GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
						//OnGUIContentsMaterialInspector(first_material, multiple_materials);
						//GUILayout.EndVertical();
					}
				}
				GUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.showMixedValue = false;
		}
		
		static Rect lastGuiRect;
		public static Rect GetLastSceneGUIRect(SurfaceEditBrushTool tool)
		{
			return lastGuiRect;
		}

		static Vector2 scrollPosition = Vector2.zero;
		
		public static void OnSceneGUI(Rect windowRect, SurfaceEditBrushTool tool)
		{
			GUIStyleUtility.InitStyles();
			InitLocalStyles();

			var maxHeight = windowRect.height - 80;
			
			GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
			{
				GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
				{
					GUILayout.FlexibleSpace();

					GUILayout.BeginVertical(GUILayout.MaxHeight(maxHeight));
					{
						GUILayout.FlexibleSpace();

						GUIStyleUtility.ResetGUIState();
						
						GUIStyle windowStyle = GUI.skin.window;
						GUILayout.BeginVertical(ContentSurfacesLabel, windowStyle, GUIStyleUtility.ContentEmpty);
						{
							GUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

							OnGUIContents(true, tool);

							GUILayout.EndScrollView();
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
						int controlID = GUIUtility.GetControlID(SceneViewSurfaceOverlayHash, FocusType.Keyboard, currentArea);
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
		}

		public static void OnInspectorGUI(EditorWindow window, float height)
		{
			lastGuiRect = Rect.MinMaxRect(-1, -1, -1, -1);
			var tool = CSGBrushEditorManager.ActiveTool as SurfaceEditBrushTool;

			GUIStyleUtility.InitStyles();
			InitLocalStyles();
			OnGUIContents(false, tool);
		}
	}
}
