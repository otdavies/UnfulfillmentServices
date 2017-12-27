using System;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;
using System.Linq;

namespace RealtimeCSG
{
	[Serializable]
	internal enum ShapeMode
	{
		FreeDraw,
		Box,
		Sphere,
		Cylinder,
//		SpiralStairs,
		LinearStairs,

		Last = LinearStairs
	}

	internal sealed class GenerateBrushTool : ScriptableObject, IBrushTool
	{
		public bool UsesUnitySelection	{ get { return false; } }
		public bool IgnoreUnityRect		{ get { return true; } }

		public static event Action ShapeCommitted = null;
		public static event Action ShapeCancelled = null;
		
		public ShapeMode BuilderMode
		{
			get
			{
				return builderMode;
			}
			set
			{
				if (builderMode == value)
					return;
				builderMode = value;
				RealtimeCSG.CSGSettings.ShapeBuildMode = builderMode;
				RealtimeCSG.CSGSettings.Save();
				ResetTool();
			}
		}
		
		IBrushGenerator InternalCurrentGenerator
		{
			get
			{ 
				switch (builderMode)
				{
					default:
					case ShapeMode.FreeDraw:
					{
						return freedrawGenerator;
					}
					case ShapeMode.Cylinder:
					{
						return cylinderGenerator;
					}
					case ShapeMode.Box:
					{
						return boxGenerator;
                    }
                    case ShapeMode.Sphere:
                    {
                        return sphereGenerator;
                    }
					case ShapeMode.LinearStairs:
					{
						return linearStairsGenerator;
					}/*
					case ShapeMode.SpiralStairs:
					{
						return spiralStairsGenerator;
					}*/
                }
			}
		}

		public IBrushGenerator CurrentGenerator
		{
			get
			{
				var generator = InternalCurrentGenerator;
				var obj = generator as ScriptableObject;
				if (obj != null && obj)
					return generator;				
				ResetTool();
				return generator;
			}
		}
		
		[SerializeField] CSGBrush[]			brushes				= new CSGBrush[0];	

		[SerializeField] ShapeMode			builderMode			= ShapeMode.FreeDraw;

		[SerializeField] FreeDrawGenerator		freedrawGenerator;
		[SerializeField] CylinderGenerator		cylinderGenerator;
		[SerializeField] LinearStairsGenerator	linearStairsGenerator;
		[SerializeField] SpiralStairsGenerator	spiralStairsGenerator;
        [SerializeField] SphereGenerator		sphereGenerator;
        [SerializeField] BoxGenerator			boxGenerator;

		[NonSerialized] bool				isEnabled		= false;
		[NonSerialized] bool				hideTool		= false;

		public void SetTargets(FilteredSelection filteredSelection)
		{
			hideTool = filteredSelection.NodeTargets.Length > 0;
			brushes = filteredSelection.GetAllContainedBrushes().ToArray();
			lastLineMeshGeneration--;
			if (isEnabled)
				Tools.hidden = hideTool;
		}

		void OnEnable()
		{
			RealtimeCSG.CSGSettings.Reload();
			builderMode = RealtimeCSG.CSGSettings.ShapeBuildMode;
		}

		public void OnEnableTool()
		{
			isEnabled		= true;
			Tools.hidden	= hideTool;
			ResetTool();
		}
		
		public void OnDisableTool()
		{
			isEnabled = false;
			Tools.hidden = false;
			ResetTool();
		}

		void ResetTool()
		{
			RealtimeCSG.Grid.ForceGrid = false;
			if (!freedrawGenerator)
			{
				freedrawGenerator = ScriptableObject.CreateInstance<FreeDrawGenerator>();
				freedrawGenerator.snapFunction		= CSGBrushEditorManager.SnapPointToGrid;
				freedrawGenerator.raySnapFunction	= CSGBrushEditorManager.SnapPointToRay;
				freedrawGenerator.shapeCancelled	= OnShapeCancelledEvent;
				freedrawGenerator.shapeCommitted	= OnShapeCommittedEvent;
			}
			if (!cylinderGenerator)
			{
				cylinderGenerator = ScriptableObject.CreateInstance<CylinderGenerator>();
				cylinderGenerator.snapFunction		= CSGBrushEditorManager.SnapPointToGrid;
				cylinderGenerator.raySnapFunction	= CSGBrushEditorManager.SnapPointToRay;
				cylinderGenerator.shapeCancelled	= OnShapeCancelledEvent;
				cylinderGenerator.shapeCommitted	= OnShapeCommittedEvent;
			}
			if (!boxGenerator)
			{
				boxGenerator = ScriptableObject.CreateInstance<BoxGenerator>();
				boxGenerator.snapFunction			= CSGBrushEditorManager.SnapPointToGrid;
				boxGenerator.raySnapFunction		= CSGBrushEditorManager.SnapPointToRay;
				boxGenerator.shapeCancelled			= OnShapeCancelledEvent;
				boxGenerator.shapeCommitted			= OnShapeCommittedEvent;
            }
            if (!sphereGenerator)
            {
                sphereGenerator = ScriptableObject.CreateInstance<SphereGenerator>();
                sphereGenerator.snapFunction = CSGBrushEditorManager.SnapPointToGrid;
                sphereGenerator.raySnapFunction = CSGBrushEditorManager.SnapPointToRay;
                sphereGenerator.shapeCancelled = OnShapeCancelledEvent;
                sphereGenerator.shapeCommitted = OnShapeCommittedEvent;
            }
			if (!linearStairsGenerator)
			{
				linearStairsGenerator = ScriptableObject.CreateInstance<LinearStairsGenerator>();
				linearStairsGenerator.snapFunction = CSGBrushEditorManager.SnapPointToGrid;
				linearStairsGenerator.raySnapFunction = CSGBrushEditorManager.SnapPointToRay;
				linearStairsGenerator.shapeCancelled = OnShapeCancelledEvent;
				linearStairsGenerator.shapeCommitted = OnShapeCommittedEvent;
			}
			if (!spiralStairsGenerator)
			{
				spiralStairsGenerator = ScriptableObject.CreateInstance<SpiralStairsGenerator>();
				spiralStairsGenerator.snapFunction = CSGBrushEditorManager.SnapPointToGrid;
				spiralStairsGenerator.raySnapFunction = CSGBrushEditorManager.SnapPointToRay;
				spiralStairsGenerator.shapeCancelled = OnShapeCancelledEvent;
				spiralStairsGenerator.shapeCommitted = OnShapeCommittedEvent;
			}

            var generator = InternalCurrentGenerator;
			if (generator != null)
			{
				var obj = generator as ScriptableObject;
				if (obj)
					generator.Init();
			}
		}

		public bool HotKeyReleased()
		{
			if (CurrentGenerator == null)
				return false;
			return CurrentGenerator.HotKeyReleased();
		}

		void OnShapeCancelledEvent()
		{
			CurrentGenerator.Init();
			ShapeCancelled.Invoke();
		}

		void OnShapeCommittedEvent()
		{
			ShapeCommitted.Invoke();
		}
		
		public bool UndoRedoPerformed()
		{
			return CurrentGenerator.UndoRedoPerformed();
		}

		public bool DeselectAll()
		{
			CurrentGenerator.PerformDeselectAll();
			return true;
		}

		void SetOperationType(CSGOperationType operationType)
		{
			CurrentGenerator.CurrentCSGOperationType = operationType;
		}

		void OnDestroy()
		{
			zTestLineMeshManager.Destroy();
			noZTestLineMeshManager.Destroy();
		}

		LineMeshManager zTestLineMeshManager = new LineMeshManager();
		LineMeshManager noZTestLineMeshManager = new LineMeshManager();
		int lastLineMeshGeneration = -1;


		public void HandleEvents(Rect sceneRect)
		{
			if (CurrentGenerator == null)
				return;

			CurrentGenerator.HandleEvents(sceneRect); 
			switch (Event.current.type)
			{
				case EventType.ValidateCommand:
				{
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedAdditiveKey    .IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedSubtractiveKey .IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedIntersectingKey.IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.CancelActionKey             .IsKeyPressed()) { Event.current.Use(); break; }
					if (Keys.HandleSceneValidate(CSGBrushEditorManager.CurrentTool, false)) { Event.current.Use(); HandleUtility.Repaint(); break; }
					break;
				}

				case EventType.KeyDown:
				{
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedAdditiveKey    .IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedSubtractiveKey .IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedIntersectingKey.IsKeyPressed()) { Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.CancelActionKey             .IsKeyPressed()) { Event.current.Use(); break; }
					if (Keys.HandleSceneKeyDown(CSGBrushEditorManager.CurrentTool, false)) { Event.current.Use(); HandleUtility.Repaint(); break; }
					break;
				}

				case EventType.KeyUp:
				{
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedAdditiveKey    .IsKeyPressed()) { SetOperationType(CSGOperationType.Additive);     Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedSubtractiveKey .IsKeyPressed()) { SetOperationType(CSGOperationType.Subtractive);  Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.MakeSelectedIntersectingKey.IsKeyPressed()) { SetOperationType(CSGOperationType.Intersecting); Event.current.Use(); break; }
					if (!EditorGUIUtility.editingTextField && Keys.CancelActionKey.IsKeyPressed()) { CurrentGenerator.PerformDeselectAll(); Event.current.Use(); break; }
					if (Keys.HandleSceneKeyUp(CSGBrushEditorManager.CurrentTool, false)) { Event.current.Use(); HandleUtility.Repaint(); break; }
					break;
				}

				case EventType.Repaint:
				{
					if (lastLineMeshGeneration != InternalCSGModelManager.MeshGeneration)
					{
						lastLineMeshGeneration = InternalCSGModelManager.MeshGeneration;

						var brush_translations	= new Vector3[brushes.Length];
						var brush_ids			= new Int32[brushes.Length];
						for (int i = brushes.Length - 1; i >= 0; i--)
						{
							var brush = brushes[i];
							var brush_cache = InternalCSGModelManager.GetBrushCache(brush);
							if (brush.brushID == -1 ||	// could be a prefab
								brush_cache == null ||
								brush_cache.compareTransformation == null ||
								brush_cache.childData == null ||
								brush_cache.childData.ModelTransform == null ||
								!brush_cache.childData.ModelTransform)
							{
								ArrayUtility.RemoveAt(ref brush_translations, i);
								ArrayUtility.RemoveAt(ref brush_ids, i);
								continue;
							}
							brush_translations[i] = brush_cache.compareTransformation.modelLocalPosition + brush_cache.childData.ModelTransform.position;
							brush_ids[i] = brush.brushID;
						}
						CSGRenderer.DrawSelectedBrushes(zTestLineMeshManager, noZTestLineMeshManager, brush_ids, brush_translations, 
							ColorSettings.SelectedOutlines, ToolConstants.lineScale);
					}
					
					MaterialUtility.LineAlphaMultiplier = 1.0f;
					MaterialUtility.LineDashMultiplier = 2.0f;
					MaterialUtility.LineThicknessMultiplier = 2.0f;
					noZTestLineMeshManager.Render(MaterialUtility.NoZTestGenericLine);
					MaterialUtility.LineThicknessMultiplier = 1.0f;
					zTestLineMeshManager.Render(MaterialUtility.ZTestGenericLine);

					break;
				}
			}
		}

		public void OnInspectorGUI(EditorWindow window, float height)
		{
			GenerateBrushToolGUI.OnInspectorGUI(this, window, height);
		}
		
		public Rect GetLastSceneGUIRect()
		{
			return GenerateBrushToolGUI.GetLastSceneGUIRect(this);
		}

		public bool OnSceneGUI(Rect windowRect)
		{
			return GenerateBrushToolGUI.OnSceneGUI(windowRect, this);
		}

		public void GenerateFromPolygon(CSGBrush brush, CSGPlane plane, Vector3 direction, Vector3[] meshVertices, int[] indices, uint[] smoothingGroups, bool drag)
		{
			BuilderMode = ShapeMode.FreeDraw;
			freedrawGenerator.GenerateFromPolygon(brush, plane, direction, meshVertices, indices, smoothingGroups, drag);
		}
	}
}
