using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
using InternalRealtimeCSG;
using System.Reflection;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace RealtimeCSG
{
	internal static class InternalCSGModelManager
	{
		class CSGSceneState
		{
			public GameObject	DefaultModelObject	= null;
			public CSGModel		DefaultModel		= null;
		}

		internal const string	DefaultModelName	= "[default-CSGModel]";
		private static readonly Dictionary<Scene, CSGSceneState> SceneStates = new Dictionary<Scene, CSGSceneState>();

		#region DefaultCSGModel
		internal static CSGModel GetDefaultCSGModelForObject(Transform transform)
		{
			Scene currentScene;
			if (!transform || !transform.gameObject)
			{
				currentScene = SceneManager.GetActiveScene();
			} else
				currentScene = transform.gameObject.scene;
			
			return InternalCSGModelManager.GetDefaultCSGModel(currentScene);
		}
		internal static CSGModel GetDefaultCSGModelForObject(MonoBehaviour component)
		{
			Scene currentScene;
			if (!component || !component.gameObject)
			{
				currentScene = SceneManager.GetActiveScene();
			} else
				currentScene = component.gameObject.scene;
			
			return InternalCSGModelManager.GetDefaultCSGModel(currentScene);
		}

		internal static CSGModel GetDefaultCSGModel(Scene scene)
		{
			// make sure we're properly initialized (just in case)
			if (!SceneStates.ContainsKey(scene))
				SceneStates[scene] = new CSGSceneState();
			if (!SceneStates[scene].DefaultModel)
				InitializeDefaultCSGModel(scene, SceneStates[scene], createIfNoExists: true);

			return SceneStates[scene].DefaultModel;
		}
		#endregion

		internal static CSGModel[]				Models							= new CSGModel[0];
		private static CSGModelCache[]			_modelCaches						= new CSGModelCache[0];

		private static readonly HashSet<CSGModel>		ModelLookup					= new HashSet<CSGModel>();
		private static readonly HashSet<CSGModel>		RegisterModels				= new HashSet<CSGModel>();
		private static readonly HashSet<CSGModel>		EnableModels				= new HashSet<CSGModel>();
		private static readonly HashSet<CSGModel>		DisableModels				= new HashSet<CSGModel>();
		private static readonly HashSet<Int32>			RemovedModels				= new HashSet<Int32>();


		internal static CSGOperation[]			Operations			= new CSGOperation[0];
		private static CSGOperationCache[]		_operationCaches	= new CSGOperationCache[0];

		private static readonly HashSet<CSGOperation>	RegisterOperations			= new HashSet<CSGOperation>();
		private static readonly HashSet<CSGOperation>	ValidateOperations			= new HashSet<CSGOperation>();
		private static readonly HashSet<CSGOperation>	UnregisterOperations		= new HashSet<CSGOperation>();
		private static readonly HashSet<CSGOperation>	OperationTransformChanged	= new HashSet<CSGOperation>();
		private static readonly HashSet<Int32>			RemovedOperations			= new HashSet<Int32>();


		internal static CSGBrush[]				Brushes				= new CSGBrush[0];
		private static CSGBrushCache[]			_brushCaches		= new CSGBrushCache[0];

		private static readonly HashSet<CSGBrush>		RegisterBrushes				= new HashSet<CSGBrush>();
		private static readonly HashSet<CSGBrush>		ValidateBrushes				= new HashSet<CSGBrush>();
		private static readonly HashSet<CSGBrush>		UnregisterBrushes			= new HashSet<CSGBrush>();
		private static readonly HashSet<CSGBrush>		BrushTransformChanged		= new HashSet<CSGBrush>();
		private static readonly HashSet<Int32>			RemovedBrushes				= new HashSet<Int32>();


		internal static void Shutdown()
		{
			SceneStates.Clear();
			ModelLookup.Clear();
			Models = new CSGModel[0];
			_modelCaches = new CSGModelCache[0];

			RegisterModels.Clear();
			EnableModels.Clear();
			DisableModels.Clear();
			RemovedModels.Clear();

			Operations = new CSGOperation[0];
			_operationCaches = new CSGOperationCache[0];

			RegisterOperations.Clear();
			ValidateOperations.Clear();
			UnregisterOperations.Clear();
			OperationTransformChanged.Clear();
			RemovedOperations.Clear();
			
			Brushes			= new CSGBrush[0];
			_brushCaches	= new CSGBrushCache[0];

			RegisterBrushes.Clear();
			ValidateBrushes.Clear();
			UnregisterBrushes.Clear();
			BrushTransformChanged.Clear();
			RemovedBrushes.Clear();

			_isInitialized = false;
		}


		private static bool	_isHierarchyModified	= true;
		private static bool	_isInitialized			= false;

		public static bool	IsQuitting				= false;

		public static ExternalMethods External;
		


		#region Clear
		static void Clear()
		{
			BrushOutlineManager.ClearOutlines();

			RemovedBrushes	    .Clear();
			RemovedOperations   .Clear();
			RemovedModels	    .Clear();

			RegisterModels      .Clear();
			EnableModels        .Clear();
			DisableModels       .Clear();

			RegisterOperations  .Clear();
			ValidateOperations  .Clear();
			UnregisterOperations.Clear();
		
			RegisterBrushes     .Clear();
			ValidateBrushes     .Clear();
			UnregisterBrushes   .Clear();

			ModelLookup			.Clear();
			Models				= new CSGModel[0];
			Operations			= new CSGOperation[0];
			Brushes 			= new CSGBrush[0];
			
			_modelCaches		 = new CSGModelCache[0];
			_operationCaches	 = new CSGOperationCache[0];
			_brushCaches		 = new CSGBrushCache[0];
			_isHierarchyModified = true;
		}
		#endregion

		public static bool EnsureBrushCache(CSGBrush brush, bool reset = true)
		{
			if (System.Object.Equals(brush, null) || !brush || brush.brushID < 0)
				return false;
			if (_brushCaches.Length <= brush.brushID)
			{
				Array.Resize(ref _brushCaches, brush.brushID + 1);
			}
			if (_brushCaches[brush.brushID] == null)
			{
				_brushCaches[brush.brushID] = new CSGBrushCache();
				_brushCaches[brush.brushID].controlMeshGeneration = (brush.ControlMesh != null) ? brush.ControlMesh.Generation - 1 : -1;
			} else
			if (reset)
				_brushCaches[brush.brushID].Reset();
			_brushCaches[brush.brushID].brush = brush;
			return true;
		}

		public static CSGBrushCache GetBrushCache(CSGBrush brush)
		{
			if (System.Object.Equals(brush, null) || !brush || brush.brushID < 0)
				return null;
			if (brush.brushID >= _brushCaches.Length)
			{
				EnsureBrushCache(brush);
			}
			return _brushCaches[brush.brushID];
		}

		public static bool EnsureOperationCache(CSGOperation operation)
		{
			if (System.Object.Equals(operation, null) || !operation || operation.operationID < 0)
				return false;
			if (_operationCaches.Length <= operation.operationID)
			{
				Array.Resize(ref _operationCaches, operation.operationID + 1);
			}
			if (_operationCaches[operation.operationID] == null)
				_operationCaches[operation.operationID] = new CSGOperationCache();
			return true;
		}

		public static CSGOperationCache GetOperationCache(CSGOperation operation)
		{
			if (System.Object.Equals(operation, null) || !operation || operation.operationID < 0)
				return null;
			if (operation.operationID >= _operationCaches.Length)
				EnsureOperationCache(operation);
			return _operationCaches[operation.operationID];
		}

		public static bool EnsureModelCache(CSGModel model)
		{
			if (System.Object.Equals(model, null) || !model || model.modelID < 0)
				return false;
			if (_modelCaches.Length <= model.modelID)
			{
				Array.Resize(ref _modelCaches, model.modelID + 1);
			}
			if (_modelCaches[model.modelID] == null)
			{
				_modelCaches[model.modelID] = new CSGModelCache();
				_modelCaches[model.modelID].VertexChannelFlags = model.VertexChannels;
				MeshInstanceManager.EnsureInitialized(model);
			}
			return true;
		}

		public static CSGModelCache GetModelCache(CSGModel model)
		{
			if (System.Object.Equals(model, null) || !model || model.modelID < 0)
				return null;
			if (model.modelID >= _modelCaches.Length)
				EnsureModelCache(model);
			return _modelCaches[model.modelID];
		}

		public static ParentNodeData GetParentData(ChildNodeData childNode)
		{
			var parent = childNode.Parent;
			if (System.Object.Equals(parent, null) || !parent || !parent.transform)
			{
				var model = childNode.Model;
				if (System.Object.Equals(model, null) || !model || model.modelID < 0|| !model.transform ||
					_modelCaches == null || _modelCaches.Length < model.modelID)
					return null;
				var id = model.modelID;
				if (_modelCaches[id] == null)
				{
					model.nodeID = CSGNode.InvalidNodeID;
					InternalCSGModelManager.RegisterModel(model);
					MeshInstanceManager.EnsureInitialized(model);
					if (_modelCaches[id] == null)
						return null;
				}
				return _modelCaches[id].ParentData;
			}
			return parent.operationID < 0 ? null : _operationCaches[parent.operationID].ParentData;
		}

		#region InitOnNewScene
		public static void InitOnNewScene()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}
			
			if (External == null) CSGBindings.RegisterExternalMethods();
			if (External == null) return;
			
			if (RegisterAllComponents())
				External.ResetCSG();
		}
		#endregion

		#region RegisterAllComponents
		static bool RegisterAllComponents()
		{
			//Debug.Log("RegisterAllComponents");
			if (External == null)
			{
				//Debug.Log("External == null");
				return false;
			}

			Clear();

			if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return false;
			}

			var allNodes = new List<CSGNode>();
			for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
			{
				var scene = SceneManager.GetSceneAt(sceneIndex);
				if (!scene.isLoaded)
					continue;
				var sceneNodes = SceneQueryUtility.GetAllComponentsInScene<CSGNode>(scene);
				allNodes.AddRange(sceneNodes);
			}
			for (var i = allNodes.Count - 1; i >= 0; i--) Reset(allNodes[i]);
			for (var i = 0; i < allNodes.Count; i++) AddNodeRegistration(allNodes[i]);
			return true;
		}
		#endregion

		#region Rebuild
		public static void Rebuild()
		{
			Clear();
			if (External == null ||
				External.ResetCSG == null)
				CSGBindings.RegisterExternalMethods();

			External.ResetCSG();

			ClearMeshInstances();

			RegisterAllComponents();

			for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
			{
				var scene = SceneManager.GetSceneAt(sceneIndex);
				if (!scene.isLoaded)
					continue;
				var sceneModels = SceneQueryUtility.GetAllComponentsInScene<CSGModel>(scene);
				for (int i = 0; i < sceneModels.Count; i++)
				{
					Transform selfTransform = sceneModels[i].transform;
					Transform[] transforms = selfTransform.GetComponentsInChildren<Transform>();
					foreach (var transform in transforms)
					{
						if (!transform || transform.parent != selfTransform)
							continue;

						if (transform.name != MeshInstanceManager.MeshContainerName)
							continue;
						
						UnityEngine.Object.DestroyImmediate(transform.gameObject);
					}
				}
			}
		}
		#endregion

		public static void ClearMeshInstances()
		{
			MeshInstanceManager.Reset();
			
			for (var i = 0; i < Models.Length; i++)
			{
				var model		= Models[i];
				var modelCache	= InternalCSGModelManager.GetModelCache(model);
				if (modelCache != null)
					modelCache.GeneratedMeshes = null;
			}
		}

		#region Reset
		static void Reset(CSGNode node)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			var brush = node as CSGBrush;
			if (brush) { Reset(brush); return; }
			var operation = node as CSGOperation;
			if (operation && !operation.PassThrough) { Reset(operation); return; }
			var model = node as CSGModel;
			if (model) { Reset(model); return; }
		}
		#endregion

		#region AddNodeRegistration
		static void AddNodeRegistration(CSGNode node)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			var brush = node as CSGBrush;
			if (brush) { RegisterBrushes.Add(brush); return; }
			var operation = node as CSGOperation;
			if (operation && !operation.PassThrough) { RegisterOperations.Add(operation); return; }
			var model = node as CSGModel;
			if (model)
			{
				RegisterModels.Add(model);
				return;
			}
		}
		#endregion
		

		public static CSGBrush FindBrushByID(int brushID)
		{
			for (var i = 0; i < Brushes.Length; i++)
			{
				if (Brushes[i].brushID == brushID)
					return Brushes[i];
			}
			return null;
		}

		// for a given transform, try to to find the first transform parent that is a csg-node
		#region FindParentTransform
		public static Transform FindParentTransform(Transform childTransform)
		{
			var iterator	= childTransform.parent;
			if (!iterator)
				return null;

			var currentNode = iterator.GetComponent<CSGNode>();
			if (currentNode)
			{
				var brush = currentNode as CSGBrush;
				if (brush)
				{
					var brushCache = InternalCSGModelManager.GetBrushCache(brush);
					return ((!brushCache.childData.Parent) ? ((!brushCache.childData.Model) ? null : brushCache.childData.Model.transform) : brushCache.childData.Parent.transform);
				}
			}
			while (iterator)
			{
				currentNode = iterator.GetComponent<CSGNode>();
				if (currentNode)
					return iterator; 

				iterator = iterator.parent;
			}
			return null;
		}
		#endregion

		// for a given transform, try to to find the model transform
		#region FindParentTransform
		public static Transform FindModelTransform(Transform childTransform)
		{
			var currentNode = childTransform.GetComponent<CSGNode>();
			if (currentNode)
			{
				var model = currentNode as CSGModel;
				if (model)
					return null;
				var brush = currentNode as CSGBrush;
				if (brush)
				{
					var brushCache = InternalCSGModelManager.GetBrushCache(brush);
					if (brushCache == null)
						return null;
					return (brushCache.childData.Model == null) ? null : brushCache.childData.Model.transform;
				}
				var operation = currentNode as CSGOperation;
				if (operation && !operation.PassThrough)
				{
					var operationCache = InternalCSGModelManager.GetOperationCache(operation);
					if (operationCache == null)
						return null;
					return (operationCache.ChildData.Model == null) ? null : operationCache.ChildData.Model.transform;
				}
			}
			
			var iterator = childTransform.parent;
			while (iterator)
			{
				var currentModel = iterator.GetComponent<CSGModel>();
				if (currentModel)
					return currentModel.transform; 

				iterator = iterator.parent;
			}
			return null;
		}
		#endregion

		#region FindParentNode
		static void FindParentNode(Transform opTransform, out CSGNode parentNode, out Transform parentTransform)
		{
			var iterator = opTransform.parent;
			while (iterator)
			{
				var currentNode = iterator.GetComponent<CSGNode>();
				if (currentNode)
				{
					var operationNode = currentNode as CSGOperation;
					if (!operationNode ||
						!operationNode.PassThrough)
					{
						parentNode = currentNode;
						parentTransform = iterator;
						return;
					}
				}

				iterator = iterator.parent;
			}
			parentTransform = null;
			parentNode = null;
		}
		#endregion

		#region FindParentOperation
		static void FindParentOperation(Transform opTransform, out CSGOperation parentOp)
		{
			if (opTransform)
			{
				var iterator = opTransform.parent;
				while (iterator)
				{
					var currentParent = iterator.GetComponent<CSGOperation>();
					if (currentParent &&
						!currentParent.PassThrough)
					{
						parentOp = currentParent;
						return;
					}

					iterator = iterator.parent;
				}
			}
			parentOp = null;
		}
		#endregion

		#region FindParentModel
		static void FindParentModel(Transform opTransform, out CSGModel parentModel)
		{
			if (!opTransform)
			{
				parentModel = null;
				return;
			}

			var iterator = opTransform.parent;
			while (iterator)
			{
				var currentModel = iterator.GetComponent<CSGModel>();
				if (currentModel)
				{
					parentModel = currentModel;
					return;
				}

				iterator = iterator.parent;
			}
			parentModel = GetDefaultCSGModelForObject(opTransform);
		}
		#endregion

		#region FindParentOperationAndModel
		static void FindParentOperationAndModel(Transform opTransform, out CSGOperation parentOp, out CSGModel parentModel)
		{
			parentOp = null;
			parentModel = null;
			if (!opTransform)
				return;
			var iterator = opTransform.parent;
			while (iterator)
			{
				var currentNode = iterator.GetComponent<CSGNode>();
				if (currentNode)
				{
					parentModel = currentNode as CSGModel;
					if (parentModel)
					{
						parentOp = null;
						return;
					}
					parentOp = currentNode as CSGOperation;
					if (parentOp && !parentOp.PassThrough)
						break;
				}

				iterator = iterator.parent;
			}

			while (iterator)
			{
				var currentModel = iterator.GetComponent<CSGModel>();
				if (currentModel)
				{
					parentModel = currentModel;
					return;
				}

				iterator = iterator.parent;
			}
			parentModel = GetDefaultCSGModelForObject(opTransform);
		}
		#endregion
		
		#region CreateCSGModel
		internal static CSGModel CreateCSGModel(GameObject gameObject)
		{
			var model = gameObject.AddComponent<CSGModel>();
			StaticEditorFlags defaultFlags = //StaticEditorFlags.LightmapStatic |
												StaticEditorFlags.BatchingStatic |
												StaticEditorFlags.NavigationStatic |
												StaticEditorFlags.OccludeeStatic |
												StaticEditorFlags.OffMeshLinkGeneration |
												StaticEditorFlags.ReflectionProbeStatic;
			GameObjectUtility.SetStaticEditorFlags(gameObject, defaultFlags);
			return model;
		}
		#endregion

		#region InitializeDefaultCSGModel
		static void InitializeDefaultCSGModel(bool createIfNoExists = true)
		{
			var loadedScenes = new HashSet<Scene>();
			for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
			{
				var currentScene = SceneManager.GetSceneAt(sceneIndex);
				var sceneState = SceneStates[currentScene];
				InitializeDefaultCSGModel(currentScene, sceneState, createIfNoExists);
			}
			var usedScenes = SceneStates.Keys.ToArray();
			foreach(var usedScene in usedScenes)
			{
				if (!loadedScenes.Contains(usedScene))
					SceneStates.Remove(usedScene);
			}
		}

		static void InitializeDefaultCSGModel(Scene currentScene, CSGSceneState sceneState, bool createIfNoExists = true)
		{
			// make sure we have a default CSG model
			sceneState.DefaultModelObject = SceneQueryUtility.GetUniqueHiddenGameObjectInSceneWithName(currentScene, DefaultModelName);
			if (createIfNoExists)
			{ 
				if (!sceneState.DefaultModelObject)
				{
					sceneState.DefaultModelObject	= new GameObject(DefaultModelName);
					sceneState.DefaultModel			= CreateCSGModel(sceneState.DefaultModelObject);
				} else 
				{
					sceneState.DefaultModel			= sceneState.DefaultModelObject.GetComponent<CSGModel>();
					if (!sceneState.DefaultModel)
						sceneState.DefaultModel		= CreateCSGModel(sceneState.DefaultModelObject);
				}

				// make sure it's transformation has sensible values
				sceneState.DefaultModel.transform.parent		= null;
				sceneState.DefaultModel.transform.localPosition	= MathConstants.zeroVector3;
				sceneState.DefaultModel.transform.localScale	= MathConstants.oneVector3;
				sceneState.DefaultModel.transform.localRotation	= Quaternion.identity;

				var defaultFlags = //StaticEditorFlags.LightmapStatic |
								   StaticEditorFlags.BatchingStatic |
								   StaticEditorFlags.NavigationStatic |
								   StaticEditorFlags.OccludeeStatic |
								   StaticEditorFlags.OffMeshLinkGeneration |
								   StaticEditorFlags.ReflectionProbeStatic;
				GameObjectUtility.SetStaticEditorFlags(sceneState.DefaultModel.gameObject, defaultFlags);

				// do not give default CSG model any extra abilities other than rendering
				//  since an invisible object doesn't work well with navmesh/colliders etc.
				// force the users to use a CSG-model, but gracefully let it still be 
				//  visible for objects outside of it.
				sceneState.DefaultModelObject.hideFlags = MeshInstanceManager.ComponentHideFlags;
			 
				RegisterModel(sceneState.DefaultModel);
				MeshInstanceManager.EnsureInitialized(sceneState.DefaultModel);
			}
		}
		#endregion
	   
		 
		#region InitializeChildDefaults
		static void InitializeChildDefaults(ChildNodeData childData, Transform childTransform)
		{
			Transform parentTransform;
			CSGNode parentNode;
			FindParentNode(childTransform, out parentNode, out parentTransform);
			if (!parentNode)
			{
				parentTransform	= childTransform.root;
				childData.Model		= GetDefaultCSGModelForObject(childTransform);
				childData.Parent	= null;
			} else
			{
				// maybe our parent is a model?
				var model = parentNode as CSGModel;
				if (model)
				{
					childData.Model = model;
				} else
				{
					// is our parent an operation?
					var operation = parentNode as CSGOperation;
					if (operation &&
						!operation.PassThrough)
					{
						childData.Parent = operation;
					}

					// see if our parent has already found a model
					if (childData.Parent)
					{
						var operationCache = InternalCSGModelManager.GetOperationCache(childData.Parent);
						if (operationCache != null &&
							operationCache.ChildData != null)
							childData.Model = operationCache.ChildData.Model;
					}
				}

				// haven't found a model?
				if (!childData.Model)
				{
					// if not, try higher up in the hierarchy ..
					FindParentModel(parentTransform, out childData.Model);
				}
			}
		}
		#endregion

		#region InitializeOperationDefaults
		static void InitializeOperationDefaults(CSGOperation op)
		{
			EnsureInitialized(op);

			var operation_cache = InternalCSGModelManager.GetOperationCache(op);
			InitializeChildDefaults(operation_cache.ChildData, op.transform);
			operation_cache.PrevOperation = op.OperationType;
		}
		#endregion

		#region InitializeBrushDefaults
		static void InitializeBrushDefaults(CSGBrush brush, Transform brushTransform)
		{
			var brushCache = InternalCSGModelManager.GetBrushCache(brush);
			InitializeChildDefaults(brushCache.childData, brushTransform);

			var currentModel	 = brushCache.childData.Model;
			var modelTransform	 = (!currentModel) ? null : currentModel.transform;
			brushCache.compareTransformation.EnsureInitialized(brushTransform, modelTransform);
			
			brushCache.compareTransformation.prevScale			= brushTransform.lossyScale;
			brushCache.compareTransformation.prevRotation		= brushTransform.rotation;
			brushCache.compareTransformation.objectToPlaneSpace = Matrix4x4.TRS(-brushTransform.position, Quaternion.identity, MathConstants.oneVector3) * brushTransform.localToWorldMatrix;
			brushCache.compareTransformation.planeToObjectSpace = brushCache.compareTransformation.objectToPlaneSpace.inverse;

			brushCache.prevOperation = brush.OperationType;
			brushCache.prevContentLayer = brush.ContentLayer;
			EnsureInitialized(brush);

			brushCache.controlMeshGeneration = brush.ControlMesh.Generation;
			brushCache.compareShape.EnsureInitialized(brush.Shape);
		}
		#endregion


		#region Component events
		
		#region Brush events
		public static void OnCreated(CSGBrush component)
		{
			RegisterBrushes  .Add   (component);
			ValidateBrushes  .Remove(component);
			UnregisterBrushes.Remove(component);
			_isHierarchyModified = true;
		}
		public static void OnEnabled(CSGBrush component)
		{
			RegisterBrushes  .Add   (component);
			ValidateBrushes  .Remove(component);
			UnregisterBrushes.Remove(component);
			_isHierarchyModified = true;
		}
		public static void OnValidate(CSGBrush component)
		{
			if (!UnregisterBrushes.Contains(component))
				ValidateBrushes.Add(component);
		}
		public static void OnTransformParentChanged(CSGBrush component)
		{
			InternalCSGModelManager.OnBrushTransformChanged(component);
		}
		public static void OnDisabled(CSGBrush component)
		{
			RegisterBrushes  .Remove(component);
			ValidateBrushes  .Remove(component);
			UnregisterBrushes.Add   (component);
			_isHierarchyModified = true;
		} 
		public static void OnDestroyed(CSGBrush component)
		{
			RegisterBrushes  .Remove(component);
			ValidateBrushes  .Remove(component);
			UnregisterBrushes.Remove(component);
			InternalCSGModelManager.UnregisterBrush(component);
			_isHierarchyModified = true;
		}
		public static void EnsureInitialized(CSGBrush component)
		{
			component.CheckVersion();
			if (!component)
			{
				if (component.IsRegistered)
					OnDestroyed(component);
			}

			bool dirty = false;
			var brushCache = InternalCSGModelManager.GetBrushCache(component);

			var statement1 = !System.Object.ReferenceEquals(brushCache, null) && 
							brushCache.hierarchyItem.TransformID == 0;
			var statement3 = component.nodeID != CSGNode.InvalidNodeID;
			var statement4 = statement1 && statement3;

			if (statement4)
			{
				dirty = true;
				brushCache.hierarchyItem.Transform		= component.transform;
				brushCache.hierarchyItem.TransformID	= component.transform.GetInstanceID();
				brushCache.hierarchyItem.NodeID			= component.nodeID;
			}

			// make sure that the surface array is not empty, 
			//  otherwise nothing can get rendered
			if (component.Shape == null ||
				component.Shape.Surfaces == null ||
				component.Shape.Surfaces.Length == 0 ||
				component.Shape.TexGens == null ||
				component.Shape.TexGens.Length == 0)
			{
				dirty = true;
				BrushFactory.CreateCubeControlMesh(out component.ControlMesh, out component.Shape, Vector3.one);
			}
			if (component.ControlMesh == null)
			{
				component.ControlMesh = ControlMeshUtility.EnsureValidControlMesh(component);
			}

			dirty = ShapeUtility.EnsureInitialized(component.Shape) || dirty;

			if (dirty)
				UnityEditor.EditorUtility.SetDirty(component);
		}
		public static void Reset(CSGBrush component)
		{
			if (component.IsRegistered)
			{
				var brush_cache = InternalCSGModelManager.GetBrushCache(component);
				if (brush_cache != null)
				{
					if (brush_cache.childData != null)
						brush_cache.childData    .Reset();
					if (brush_cache.hierarchyItem != null)
						brush_cache.hierarchyItem.Reset();
				}
			}

			component.nodeID	= CSGNode.InvalidNodeID;
			component.brushID	= CSGNode.InvalidNodeID;
		}
		#endregion

		#region Operation events
		public static void OnCreated(CSGOperation component)
		{
			RegisterOperations  .Add   (component);
			ValidateOperations  .Remove(component);
			UnregisterOperations.Remove(component);
			//CSGModelManager.RegisterOperation(component);
			_isHierarchyModified = true;
		}
		public static void OnEnabled(CSGOperation component)
		{
			RegisterOperations  .Add   (component);
			ValidateOperations  .Remove(component);
			UnregisterOperations.Remove(component);
			//CSGModelManager.RegisterOperation(component);
			_isHierarchyModified = true;
		}
		public static void OnValidate(CSGOperation component)
		{
			if (!UnregisterOperations.Contains(component))
				ValidateOperations  .Add(component);
			//CSGModelManager.ValidateOperation(component);
		}
		public static void OnTransformParentChanged(CSGOperation component)
		{
			InternalCSGModelManager.OnOperationTransformChanged(component);
		}
		public static void OnDisabled(CSGOperation component)
		{
			RegisterOperations  .Remove(component);
			ValidateOperations  .Remove(component);
			UnregisterOperations.Add   (component);
			//CSGModelManager.UnregisterOperation(component);
			_isHierarchyModified = true;
		}
		public static void OnDestroyed(CSGOperation component)
		{
			RegisterOperations  .Remove(component);
			ValidateOperations  .Remove(component);
			UnregisterOperations.Remove(component);
			InternalCSGModelManager.UnregisterOperation(component);
			_isHierarchyModified = true;
		}
		public static void EnsureInitialized(CSGOperation component)
		{
			if (!component)
			{
				if (component.IsRegistered)
					OnDestroyed(component);
				return;
			}

			var operationCache = InternalCSGModelManager.GetOperationCache(component);
			if (operationCache != null &&
				operationCache.ParentData.TransformID == 0 &&
				component.nodeID != -1)
			{
				operationCache.ParentData.Transform	= component.transform;
				operationCache.ParentData.TransformID	= component.transform.GetInstanceID();
				operationCache.ParentData.NodeID		= component.nodeID;
			}
		}

		public static void Reset(CSGOperation component)
		{
			if (component.IsRegistered)
			{
				var operationCache = InternalCSGModelManager.GetOperationCache(component);
				if (operationCache != null)
				{
					if (operationCache.ParentData != null)
						operationCache.ParentData   .Reset();
					if (operationCache.ChildData != null)
						operationCache.ChildData    .Reset();
				}
			}

			component.nodeID		= CSGNode.InvalidNodeID;
			component.operationID	= CSGNode.InvalidNodeID;
		}
		#endregion

		#region Model events
		public static void OnCreated(CSGModel component)
		{
			RegisterModels  .Add   (component);
			EnableModels    .Remove(component);
			DisableModels   .Remove(component);
			_isHierarchyModified = true;
		}
		public static void OnEnabled(CSGModel component)
		{
			EnableModels    .Add   (component);
			DisableModels   .Remove(component);
			_isHierarchyModified = true;
		}
		public static void OnValidate(CSGModel component)
		{
			RegisterModels  .Add   (component);
			EnableModels    .Remove(component);
			DisableModels   .Remove(component);
			//CSGModelManager.RegisterModel(component);
		}
		public static void OnUpdate(CSGModel component)
		{
			var modelCache = InternalCSGModelManager.GetModelCache(component);
			if (modelCache != null)
			{
				InternalCSGModelManager.RegisterModel(component);
				MeshInstanceManager.EnsureInitialized(component);
				modelCache = InternalCSGModelManager.GetModelCache(component);
				if (modelCache == null)
					return;
			}
			
			RegisterModels  .Add   (component);
			EnableModels    .Remove(component);
			DisableModels   .Remove(component);
			//CSGModelManager.RegisterModel(component);
			//MeshInstanceManager.UpdateTransform(model_cache.meshContainer);
		}

		public static void OnTransformChildrenChanged(CSGModel component)
		{
			MeshInstanceManager.ValidateModel(component, checkChildren: true);
		}

		public static void OnDisabled(CSGModel component)
		{
			EnableModels    .Remove(component);
			DisableModels   .Add   (component);
			//CSGModelManager.DisableModel(component);
			_isHierarchyModified = true;
		}

		public static void OnDestroyed(CSGModel component)
		{
			if (!component)
				return;

			RegisterModels  .Remove(component);
			EnableModels    .Remove(component);
			DisableModels   .Remove(component);

			GeneratedMeshes meshContainer = null;
			var model_cache = InternalCSGModelManager.GetModelCache(component);
			if (model_cache != null &&
				model_cache.GeneratedMeshes != null)
			{
				meshContainer = model_cache.GeneratedMeshes;
				model_cache.GeneratedMeshes = null;
			}
			InternalCSGModelManager.UnregisterModel(component);
			if (meshContainer)
				MeshInstanceManager.Destroy(meshContainer);
			_isHierarchyModified = true;
		}

		public static void EnsureInitialized(CSGModel component)
		{
			if (!component)
			{
				if (component.IsRegistered)
					OnDestroyed(component);
				return;
			}

			var modelCache = InternalCSGModelManager.GetModelCache(component);
			if (modelCache != null &&
				modelCache.ParentData.TransformID == 0 &&
				component.nodeID != -1)
			{
				modelCache.ParentData.Transform		= component.transform;
				modelCache.ParentData.TransformID	= component.transform.GetInstanceID();
				modelCache.ParentData.NodeID		= component.nodeID;
			}
		}
		public static void Reset(CSGModel component)
		{
			if (component.IsRegistered)
			{
				var model_cache = InternalCSGModelManager.GetModelCache(component);
				if (model_cache != null &&
					model_cache.ParentData != null)
					model_cache.ParentData.Reset();
			}

			component.nodeID	= CSGNode.InvalidNodeID;
			component.modelID	= CSGNode.InvalidNodeID;
		}
		#endregion

		#endregion


		#region RegisterChild
		static void RegisterChild(ChildNodeData childData)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			
			// make sure our model has actually been initialized
			if (childData.Model &&
				!childData.Model.IsRegistered)
			{
				RegisterModel(childData.Model);
				MeshInstanceManager.EnsureInitialized(childData.Model);
			}

			// make sure our parent (if any) has actually been initialized
			if (childData.Parent &&
				!childData.Parent.IsRegistered)
			{
				RegisterOperations  .Remove(childData.Parent);
				UnregisterOperations.Remove(childData.Parent);
				RegisterOperation(childData.Parent);
			}
		}
		#endregion

		#region RegisterBrush
		static void RegisterBrush(CSGBrush brush)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode ||
				External == null ||
				!brush ||
				!brush.isActiveAndEnabled ||
				brush.IsRegistered)
			{
				return;
			}

			if (!External.GenerateBrushID(brush.GetInstanceID(), brush.name, out brush.brushID, out brush.nodeID))
			{
#if DEMO
				if (CSGBindings.BrushesAvailable() > 0)
#endif
					Debug.LogError("Failed to generate ID for brush", brush);
				return;
			}
			
			EnsureBrushCache(brush);
			
			InitializeBrushDefaults(brush, brush.transform);

			var brushCache = InternalCSGModelManager.GetBrushCache(brush);
			RegisterChild(brushCache.childData);


			var brushID		= brush.brushID;
			var modelID		= brushCache.childData.modelID;
			var parentID	= brushCache.childData.parentID;
				
			if (brushCache.childData.Model == null)
			{
				if (brush.brushID != CSGNode.InvalidNodeID)
				{
					External.RemoveBrush(brush.brushID);
					brush.brushID = CSGNode.InvalidNodeID;
				}
				Debug.LogError("Brush has no registered model?");
				return;
			}

			var parentData = GetParentData(brushCache.childData);
			if (parentData == null)
				return;
			
			EnsureInitialized(brush);
			
			
			brushCache.childData.OwnerParentData = parentData;
			brushCache.childData.ModelTransform  = (!brushCache.childData.Model) ? null : brushCache.childData.Model.transform;

			parentData.AddNode(brushCache.hierarchyItem);

			if (brush.Shape == null || brush.Shape.Surfaces == null || brush.Shape.Surfaces.Length < 4)
			{
				Debug.LogError("The brush (" + brush.name + ") is infinitely thin and is invalid", brush);
				brush.ControlMesh.IsValid = false;
			} else
			{
				var planeToObjectSpace	= brushCache.compareTransformation.planeToObjectSpace;
				var objectToPlaneSpace	= brushCache.compareTransformation.objectToPlaneSpace;
				var translation			= brushCache.compareTransformation.modelLocalPosition;

				if ((brush.flags & BrushFlags.InfiniteBrush) != BrushFlags.InfiniteBrush)
				{
					if (brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length)
					{
						Debug.LogWarning("brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length");
					} else
					{
						External.SetBrush(brushID,
										  modelID,
										  parentID,
										  brush.OperationType,
										  brush.ContentLayer,
										  planeToObjectSpace,
										  objectToPlaneSpace,
										  translation,
										  brush.Shape.Surfaces,
										  brush.Shape.TexGens,
										  brush.Shape.TexGenFlags);
					}
				} else
				{
					External.SetBrushInfinite(brushID,
											  modelID,
											  parentID);
				}


				SetBrushMesh(brush);
			}

			ArrayUtility.Add(ref Brushes, brush);
			_isHierarchyModified = true;
		}
		#endregion

		#region SetBrushMesh
		// set the polygons using the control-mesh if we have one
		static void SetBrushMesh(CSGBrush brush)
		{
			if (!brush)
			{
				//Debug.Log("geometryBrush == null");
				return;
			}

			var shape		= brush.Shape;
			var controlMesh	= brush.ControlMesh;
			if (controlMesh == null ||
				shape == null)
				return;

			if (!ControlMeshUtility.Validate(controlMesh, shape))
			{
				//Debug.Log("!ControlMeshUtility.Validate(controlMesh, shape)");
				//Debug.Log("!controlMesh.Validate()");
				return;
			}

			//AABB bounds = new AABB();
			var dstPolygons		= new PolygonInput[controlMesh.Polygons.Length];

			// !*!*!*!*!*!*!
			// WARNING: assumption that polygon-index == surface-index!!
			// !*!*!*!*!*!*!

			var srcEdges	= controlMesh.Edges;
			var srcPolygons	= controlMesh.Polygons;

			if (srcPolygons.Length > shape.Surfaces.Length)
			{
				Debug.LogWarning("srcPolygons.Length ("+ srcPolygons.Length + ") > shape.Surfaces.Length (" + shape.Surfaces.Length + ")");
				return;
			}

			int counter = 0;
			for (var i = 0; i < srcPolygons.Length; i++)
			{
				var edgeIndices = srcPolygons[i].EdgeIndices;
				dstPolygons[i].surfaceIndex		= i;
				dstPolygons[i].firstHalfEdge	= counter;
				dstPolygons[i].edgeCount		= edgeIndices.Length;
				counter += edgeIndices.Length;
			}

			var vertexIndices = new Int32[counter];
			var halfEdgeTwins = new Int32[counter];

			counter = 0;
			for (var i = 0; i < srcPolygons.Length; i++)
			{
				var edgeIndices = srcPolygons[i].EdgeIndices;
				for (var v = 0; v < edgeIndices.Length; v++)
				{
					var edge = srcEdges[edgeIndices[v]];
					vertexIndices[counter + v] = edge.VertexIndex;

					var twinIndex			= edge.TwinIndex;
					var twinPolygonIndex	= srcEdges[twinIndex].PolygonIndex;
					var twinEdges			= srcPolygons[twinPolygonIndex].EdgeIndices;
					var found				= false;
					for (var t = 0; t < twinEdges.Length; t++)
					{
						if (twinEdges[t] == twinIndex)
						{
							// FIXME: looks like it fails sometimes?? or problem somewhere else??
							twinIndex = t + dstPolygons[twinPolygonIndex].firstHalfEdge;
							found = true;
							break;
						}
					}

					if (!found)
					{
						//Debug.Log("!found");
						return;
					}

					halfEdgeTwins[counter + v] = twinIndex;
				}
				counter += edgeIndices.Length;
			}

			if (External == null || External.SetBrushMesh == null)
				return;

			External.SetBrushMesh(brush.brushID,
								  controlMesh.Vertices.Length,
								  controlMesh.Vertices,
								  vertexIndices.Length,
								  vertexIndices,
								  halfEdgeTwins,
								  dstPolygons.Length,
								  dstPolygons);
		}
		#endregion

		#region RegisterOperation
		static void RegisterOperation(CSGOperation op)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode ||
				External == null ||
				!op ||
				!op.isActiveAndEnabled ||
				op.PassThrough ||
				op.IsRegistered)
			{
				return;
			}

			if (!External.GenerateOperationID(op.GetInstanceID(), op.name, out op.operationID, out op.nodeID))
			{
				Debug.LogError("Failed to generate ID for operation");
				return;
			}

			EnsureOperationCache(op);

			InitializeOperationDefaults(op);
			UpdateChildrenParent(op, op.transform);

			var operationCache = InternalCSGModelManager.GetOperationCache(op);
			RegisterChild(operationCache.ChildData);

			if (!operationCache.ChildData.Model)
			{
				External.RemoveOperation(op.operationID);
				op.operationID	= CSGNode.InvalidNodeID;
				op.nodeID		= CSGNode.InvalidNodeID;
				Debug.LogError("Operation has no registered model?");
				return;
			}

			var parentData = GetParentData(operationCache.ChildData);
			if (parentData == null)
			{
				Debug.LogError("!GetParentData");
				return;
			}
			
			EnsureInitialized(op);

			operationCache.ChildData.OwnerParentData = parentData;
			operationCache.ChildData.ModelTransform  = (!operationCache.ChildData.Model) ? null : operationCache.ChildData.Model.transform;

			parentData.AddNode(operationCache.ParentData);

			var opID		= op.operationID;
			var modelID		= operationCache.ChildData.modelID;
			var parentID	= operationCache.ChildData.parentID;
				
			//Debug.Log("  SetOperation[" + op.name + "] node_id: " + op.NodeID + " op_id: " + op_id + ", model_id: " + model_id + ", parent_id: " + parent_id);

			External.SetOperation(opID,
								  modelID,
								  parentID, 
								  op.OperationType);
			ArrayUtility.Add(ref Operations, op);
			_isHierarchyModified = true;
		}
		#endregion

		#region RegisterModel
		private static void RegisterModel(CSGModel model)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode ||
				External == null ||
				!model ||
				!model.isActiveAndEnabled ||
				model.IsRegistered)
			{
				return;
			}

			if (model.IsRegistered)
			{
				if (ModelLookup.Contains(model))
					return;

				Debug.LogWarning("Model named " + model.name + " marked as registered, but wasn't actually registered", model);
			}

			if (!External.GenerateModelID(model.GetInstanceID(), model.name, out model.modelID, out model.nodeID))
			{
				Debug.LogError("Failed to generate ID for model named " + model.name);
				return;
			}
			
			EnsureModelCache(model);
			
			EnsureInitialized(model);
			
			var modelCache = InternalCSGModelManager.GetModelCache(model);
			modelCache.IsEnabled = model && model.isActiveAndEnabled;
			//Debug.Log("  SetModel[" + model.name + "] node_id: " + model.NodeID +", model_id: " + model.ModelID);

			External.SetModel(model.modelID, modelCache.IsEnabled);

			ArrayUtility.Add(ref Models, model);
			ModelLookup.Add(model);
			UpdateChildrenModel(model, model.transform);
			_isHierarchyModified = true;
		}
		#endregion

		#region UnregisterBrush  
		static void UnregisterBrush(CSGBrush brush)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// If we're quitting, we don't care about unregistering (might actually do harm)
			if (InternalCSGModelManager.IsQuitting)
				return;
			
			if (External == null)
			{
				return;
			}
			if (!brush || !brush.IsRegistered)
			{
				return;
			}

			var brushID		= brush.brushID;
			var brushCache	= InternalCSGModelManager.GetBrushCache(brush);

			if (brushCache != null && 
				brushCache.childData != null && 
				brushCache.childData.Model &&
				brushCache.childData.Model.IsRegistered)
			{
				var parent_data = GetParentData(brushCache.childData);
				if (parent_data != null)
					parent_data.RemoveNode(brushCache.hierarchyItem);
			}

			ArrayUtility.Remove(ref Brushes, brush);
			RemovedBrushes.Add(brushID);

			// did we remove component, or entire game-object?
			if (brush &&
				brush.gameObject)
			{
				var transform = brush.transform;
				if (brush.gameObject.activeInHierarchy)
				{
					CSGOperation parent;
					FindParentOperation(transform, out parent);
					UpdateChildrenParent(parent, transform);
				}
			}
			
			brush.nodeID  = CSGNode.InvalidNodeID;
			brush.brushID = CSGNode.InvalidNodeID;

			if (brushCache != null)
				brushCache.Reset();
		}
		#endregion

		#region UnregisterOperation
		static void UnregisterOperation(CSGOperation op)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// If we're quitting, we don't care about unregistering (might actually do harm)
			if (InternalCSGModelManager.IsQuitting)
				return;

			if (External == null)
			{
				return;
			}
			if (!op.IsRegistered)
			{
				return;
			}

			var opID = op.operationID;

			var operationCache = InternalCSGModelManager.GetOperationCache(op);
			if (operationCache != null && 
				operationCache.ChildData.Model &&
				operationCache.ChildData.Model.IsRegistered)
			{
				var parentData = GetParentData(operationCache.ChildData);
				if (parentData != null && operationCache.ParentData != null)
					parentData.RemoveNode(operationCache.ParentData);
			}

			ArrayUtility.Remove(ref Operations, op);
			RemovedOperations.Add(opID);

			// did we remove component, or entire game-object?
			if (op &&
				op.gameObject)
			{
				var transform = op.transform;
				if (op.gameObject.activeInHierarchy)
				{
					CSGOperation parent;
					FindParentOperation(transform, out parent);
					UpdateChildrenParent(parent, transform);
				}
			}

			op.operationID	= CSGNode.InvalidNodeID;
			op.nodeID		= CSGNode.InvalidNodeID;

			if (operationCache != null)
				operationCache.Reset();
		}
		#endregion

		#region UnregisterModel
		static void UnregisterModel(CSGModel model)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// If we're quitting, we don't care about unregistering (might actually do harm)
			if (InternalCSGModelManager.IsQuitting)
				return;

			if (!model.IsRegistered)
			{
				if (!ModelLookup.Contains(model))
					return;
				Debug.LogWarning("model marked as unregistered, but was registered");
			}

			// did we remove component, or entire game-object?
			if (model &&
				model.gameObject)
			{
				var transform = model.transform;
				if (model.gameObject.activeInHierarchy)
				{
					// NOTE: returns default model when it can't find parent model
					CSGModel parentModel;
					FindParentModel(transform, out parentModel);

					// make sure it's children are set to another model
					UpdateChildrenModel(parentModel, transform);
				}
			}

			//Debug.Log("Unregister model");
			RemovedModels.Add(model.modelID);

			//External.RemoveModel(model.ID); // delayed
			ArrayUtility.Remove(ref Models, model);
			ModelLookup.Remove(model);

			model.modelID	= CSGNode.InvalidNodeID;
			model.nodeID	= CSGNode.InvalidNodeID;
			
			MeshGeneration++;
		}
		#endregion


		#region EnableModel
		private static void EnableModel(CSGModel model)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (External == null)
			{
				return;
			}
			if (!model.IsRegistered)
			{
				return;
			}

			var enabled = model && model.isActiveAndEnabled;
			var modelCache = InternalCSGModelManager.GetModelCache(model);
			if (enabled == modelCache.IsEnabled)
			{
				return;
			}
			modelCache.IsEnabled = enabled;

			External.SetModelEnabled(model.modelID, enabled);
			MeshInstanceManager.OnEnable(modelCache.GeneratedMeshes);
		}
		#endregion

		#region DisableModel
		private static void DisableModel(CSGModel model)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (External == null)
			{
				return;
			}
			if (!model.IsRegistered)
			{
				return;
			}
			
			External.SetModelEnabled(model.modelID, false);
			var modelCache = InternalCSGModelManager.GetModelCache(model);
			if (modelCache == null)
			{
				Reset(model);
				InternalCSGModelManager.RegisterModel(model);
				MeshInstanceManager.EnsureInitialized(model);
				modelCache = InternalCSGModelManager.GetModelCache(model);
				if (modelCache == null)
					return;
			}
			modelCache.IsEnabled = false;
			MeshInstanceManager.OnDisable(modelCache.GeneratedMeshes);
		}
		#endregion


		#region UpdateChildrenParent
		static void UpdateChildrenParent(CSGOperation parent, Transform container)
		{
			if (External == null)
			{
				return;
			}
			for (int i = 0; i < container.childCount; i++)
			{
				var child = container.GetChild(i);
				var node = child.GetComponent<CSGNode>();
				if (node)
				{
					var op = node as CSGOperation;
					if (op && 
						!op.PassThrough)
					{
						// make sure the node has already been initialized, otherwise
						// assume it'll still get initialized at some point, in which
						// case we shouldn't update it's hierarchy here
						if (!op.IsRegistered)
							continue;

						var operationCache = InternalCSGModelManager.GetOperationCache(op);
						if (operationCache != null &&
							operationCache.ChildData != null &&
							operationCache.ChildData.Parent != parent &&
							operationCache.ChildData.Model)		// assume we're still initializing
						{
							SetCSGOperationHierarchy(op, parent, operationCache.ChildData.Model);
						}
						continue;
					}

					var brush = node as CSGBrush;
					if (brush)
					{
						// make sure the node has already been initialized, otherwise
						// assume it'll still get initialized at some point, in which
						// case we shouldn't update it's hierarchy here
						if (!brush.IsRegistered)
							continue;

						var brushCache = InternalCSGModelManager.GetBrushCache(brush);
						if (brushCache != null &&
							brushCache.childData != null && 
							brushCache.childData.Parent != parent &&
							brushCache.childData.Model)	// assume we're still initializing
						{
							SetCSGBrushHierarchy(brush, parent, brushCache.childData.Model);
						}
						continue;
					}
				}
				UpdateChildrenParent(parent, child);
			}
		}
		#endregion

		#region UpdateChildrenModel
		static void UpdateChildrenModel(CSGModel model, Transform container)
		{
			if (External == null)
			{
				return;
			}
			if (model == null)
				return;

			for (int i = 0; i < container.childCount; i++)
			{
				var child = container.GetChild(i);
				var node = child.GetComponent<CSGNode>();
				if (node)
				{
					var op = node as CSGOperation;
					if (op && 
						!op.PassThrough)
					{
						// make sure the node has already been initialized, otherwise
						// assume it'll still get initialized at some point, in which
						// case we shouldn't update it's hierarchy here
						if (!op.IsRegistered)
							continue;

						var operation_cache = InternalCSGModelManager.GetOperationCache(op);
						if (operation_cache.ChildData.Model != model)
						{
							if (model) // assume we're still initializing
							{
								SetCSGOperationHierarchy(op, operation_cache.ChildData.Parent, model);
							}
						} else
						{
							// assume that if this operation already has the 
							// correct model, then it's children will have the same model
							break;
						}
					}
					

					var brush = node as CSGBrush;
					if (brush)
					{
						// make sure the node has already been initialized, otherwise
						// assume it'll still get initialized at some point, in which
						// case we shouldn't update it's hierarchy here
						if (!brush.IsRegistered)
							continue;

						var brushCache = InternalCSGModelManager.GetBrushCache(brush);
						if (brushCache.childData.Model != model)
						{
							if (model) // assume we're still initializing
							{
								SetCSGBrushHierarchy(brush, brushCache.childData.Parent, model);
								InternalCSGModelManager.CheckSurfaceModifications(brush);
							}
						} else
						{
							// assume that if this brush already has the 
							// correct model, then it's children will have the same model
							break;
						}
					}
				}
				UpdateChildrenModel(model, child);
			}
		}
		#endregion

		#region OnOperationTransformChanged
		public static void OnOperationTransformChanged(CSGOperation op)
		{
			// unfortunately this event is sent before it's destroyed, so we need to defer it.
			OperationTransformChanged.Add(op);
		}
		#endregion

		#region OnBrushTransformChanged
		public static void OnBrushTransformChanged(CSGBrush brush)
		{
			// unfortunately this event is sent before it's destroyed, so we need to defer it.
			BrushTransformChanged.Add(brush);
		}
		#endregion
		
		#region SetNodeParent
		static void SetNodeParent(ChildNodeData childData, HierarchyItem hierarchyItem, CSGOperation parentOp, CSGModel parentModel)
		{
			var oldParentData = childData.OwnerParentData;

			childData.Parent = parentOp;
			childData.Model  = parentModel;			
			var newParentData = GetParentData(childData); 
			if (oldParentData != newParentData)
			{
				if (oldParentData != null) oldParentData.RemoveNode(hierarchyItem);
				if (newParentData != null) newParentData.AddNode(hierarchyItem);
				childData.OwnerParentData = newParentData;
				childData.ModelTransform = (!childData.Model) ? null : childData.Model.transform;
			}
		}
		#endregion
		
		#region SetCSGOperationHierarchy
		static void SetCSGOperationHierarchy(CSGOperation op, CSGOperation parentOp, CSGModel parentModel)
		{
			var operationCache = InternalCSGModelManager.GetOperationCache(op);
			SetNodeParent(operationCache.ChildData, operationCache.ParentData, parentOp, parentModel);
			
			if (!operationCache.ChildData.Model)
				return;
			
			External.SetOperationHierarchy(op.operationID,
										   operationCache.ChildData.modelID,
										   operationCache.ChildData.parentID);
		}
		#endregion

		#region CheckOperationHierarchy
		static void CheckOperationHierarchy(CSGOperation op)
		{
			if (External == null)
			{
				return;
			}

			if (!op || !op.gameObject.activeInHierarchy)
			{
				return;
			}

			// make sure the node has already been initialized, 
			// otherwise ignore it
			if (!op.IsRegistered)
			{
				return;
			}

			// NOTE: returns default model when it can't find parent model
			CSGModel parentModel;
			CSGOperation parentOp;
			FindParentOperationAndModel(op.transform, out parentOp, out parentModel);

			var operationCache = InternalCSGModelManager.GetOperationCache(op);
			if (operationCache.ChildData.Parent == parentOp &&
				operationCache.ChildData.Model == parentModel)
			{
				return;
			}

			SetCSGOperationHierarchy(op, parentOp, parentModel);
		}
		#endregion

		#region SetCSGBrushHierarchy
		static void SetCSGBrushHierarchy(CSGBrush brush, CSGOperation parentOp, CSGModel parentModel)
		{
			var brushCache = InternalCSGModelManager.GetBrushCache(brush);
			SetNodeParent(brushCache.childData, brushCache.hierarchyItem, parentOp, parentModel);

			if (!brushCache.childData.Model)
				return;
			
			External.SetBrushHierarchy(brush.brushID,
									   brushCache.childData.modelID,
									   brushCache.childData.parentID);
		}
		#endregion

		#region CheckSiblingPosition
		static void CheckSiblingPosition(CSGBrush brush)
		{
			if (!brush || !brush.gameObject.activeInHierarchy)
				return;

			// NOTE: returns default model when it can't find parent model
			CSGModel parentModel;
			CSGOperation parentOp;
			FindParentOperationAndModel(brush.transform, out parentOp, out parentModel);
			if (!parentOp)
				return;

			var brushCache = InternalCSGModelManager.GetBrushCache(brush);
			if (brushCache.childData.Parent != parentOp || 
				brushCache.childData.Model != parentModel)
				return;

			var operationCache = InternalCSGModelManager.GetOperationCache(parentOp);
			var parentData = operationCache.ParentData;
			ParentNodeData.UpdateNodePosition(brushCache.hierarchyItem, parentData);
		}
		#endregion

		#region CheckBrushHierarchy
		static void CheckBrushHierarchy(CSGBrush brush)
		{
			if (External == null)
			{
				return;
			}

			if (!brush || !brush.gameObject.activeInHierarchy)
			{
				if (!brush && brush.IsRegistered)
					OnDestroyed(brush);
				return;
			}

			// make sure the node has already been initialized, 
			// otherwise ignore it
			if (!brush.IsRegistered)
				return; 

			if (RemovedBrushes.Contains(brush.brushID))
				return;

			// NOTE: returns default model when it can't find parent model
			CSGModel parentModel;
			CSGOperation parentOp;
			FindParentOperationAndModel(brush.transform, out parentOp, out parentModel);

			var brushCache = InternalCSGModelManager.GetBrushCache(brush);
			if (brushCache.childData.Parent == parentOp &&
				brushCache.childData.Model  == parentModel)
			{
				if (parentOp)
				{ 
					var operationCache	= InternalCSGModelManager.GetOperationCache(parentOp);
					var parentData		= operationCache.ParentData;
					ParentNodeData.UpdateNodePosition(brushCache.hierarchyItem, parentData);
					return;
				}
			}
			
			SetCSGBrushHierarchy(brush, parentOp, parentModel);
		}
		#endregion
		

		#region ValidateOperation
		public static void ValidateOperation(CSGOperation op)
		{
			if (External == null)
			{
				return;
			}

			// make sure that this is not a prefab that's not actually in the scene ...
			if (!op || !op.gameObject.activeInHierarchy)
			{
				return;
			}

			EnsureInitialized(op);

			if (!op.IsRegistered)
			{
				RegisterOperations  .Remove(op);
				UnregisterOperations.Remove(op);
				RegisterOperation(op);

				// make sure it's registered, otherwise ignore it
				if (!op.IsRegistered)
					return;
			}

			var operationCache = InternalCSGModelManager.GetOperationCache(op);
			if (op.OperationType != operationCache.PrevOperation)
			{
				operationCache.PrevOperation = op.OperationType;
				
				External.SetOperationOperationType(op.operationID,
												   operationCache.PrevOperation);
			}
		}
		#endregion

		#region CheckSurfaces
		public static void CheckSurfaceModifications(CSGBrush[] brushes, bool surfacesModified = false)
		{
			for (int t = 0; t < brushes.Length; t++)
			{
				var brush = brushes[t];
				InternalCSGModelManager.CheckSurfaceModifications(brush, true);
			}
		}

		public static void CheckSurfaceModifications(CSGBrush brush, bool surfacesModified = false)
		{
			if (External == null || !brush)
				return;
			
			if (brush.Shape == null || brush.Shape.Surfaces == null || brush.Shape.Surfaces.Length < 4)
			{
				//Debug.LogError("Shape is infinitely thin and is invalid");
				if (brush.ControlMesh != null)
					brush.ControlMesh.IsValid = false;
				return;
			}
			
			var brush_cache		= InternalCSGModelManager.GetBrushCache(brush);
			if (brush_cache == null)
			{
				EnsureInitialized(brush);
				return; 
			}

			var shape           = brush.Shape;
			var compareShape    = brush_cache.compareShape;
			var brushID         = brush.brushID;
			if (compareShape.prevSurfaces == null || compareShape.prevTexGens == null)
			{
				Debug.LogWarning("storedShape.prevSubShapes == null || storedShape.prevTexGens == null");
				return;
			}
			
			bool updateTexGens = compareShape.prevTexGens   == null || compareShape.prevTexGens  .Length != shape.TexGens  .Length || 
								 surfacesModified;
			if (!updateTexGens)
			{
				for (int i = 0; i < shape.TexGens.Length; i++)
				{
					if (shape.TexGens[i].Color			== compareShape.prevTexGens[i].Color &&
						shape.TexGens[i].RotationAngle	== compareShape.prevTexGens[i].RotationAngle &&
						shape.TexGens[i].Scale			== compareShape.prevTexGens[i].Scale &&
						shape.TexGens[i].Translation	== compareShape.prevTexGens[i].Translation &&
						shape.TexGens[i].SmoothingGroup == compareShape.prevTexGens[i].SmoothingGroup &&
						shape.TexGenFlags[i]			== compareShape.prevTexGenFlags[i])
						continue;
					
					updateTexGens = true;
					break;			
				}
				if (!updateTexGens)
				{
					for (int i = 0; i < shape.TexGens.Length; i++)
					{
						if (shape.TexGens[i].RenderMaterial == compareShape.prevTexGens[i].RenderMaterial)
							continue;
						
						updateTexGens = true;
						break;
					}
				}
			}
			
			if (compareShape.prevSurfaces.Length != shape.Surfaces.Length)
			{
				surfacesModified = true;
			} else
			if (!surfacesModified)
			{
				if (shape == null || shape.Surfaces == null || shape.Surfaces.Length < 4)
				{
					Debug.LogError("Shape is infinitely thin and is invalid");
					brush.ControlMesh.IsValid = false;
				} else
				{
					for (int surfaceIndex = 0; surfaceIndex < shape.Surfaces.Length; surfaceIndex++)
					{
						if (shape.Surfaces[surfaceIndex].Plane.normal != compareShape.prevSurfaces[surfaceIndex].Plane.normal ||
							shape.Surfaces[surfaceIndex].Plane.d != compareShape.prevSurfaces[surfaceIndex].Plane.d ||
							shape.Surfaces[surfaceIndex].Tangent != compareShape.prevSurfaces[surfaceIndex].Tangent ||
							shape.Surfaces[surfaceIndex].TexGenIndex != compareShape.prevSurfaces[surfaceIndex].TexGenIndex)
						{
							surfacesModified = true;
							break;
						}
					}
				}
			}
			

			if (updateTexGens)
			{
				if (External.SetBrushTexGens != null)
				{
					if (shape == null || shape.Surfaces == null || shape.Surfaces.Length < 4)
					{
						Debug.LogError("Shape is infinitely thin and is invalid");
						brush.ControlMesh.IsValid = false;
					} else
					{
						if (brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length)
						{
							Debug.LogWarning("brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length");
						} else
						{
							External.SetBrushSurfaceTexGens(brushID,
															shape.Surfaces,
															shape.TexGens,
															shape.TexGenFlags);
						}

						if (compareShape.prevTexGens == null ||
							compareShape.prevTexGens.Length != shape.TexGens.Length)
						{
							compareShape.prevTexGens	 = new TexGen[shape.TexGens.Length];
							compareShape.prevTexGenFlags = new TexGenFlags[shape.TexGens.Length];
						}
					
						Array.Copy(shape.TexGens,	  compareShape.prevTexGens,		shape.TexGens.Length);
						Array.Copy(shape.TexGenFlags, compareShape.prevTexGenFlags, shape.TexGenFlags.Length);
					
						if (shape.Surfaces.Length > 0)
						{
							if (compareShape.prevSurfaces == null ||
								compareShape.prevSurfaces.Length != shape.Surfaces.Length)
							{
								compareShape.prevSurfaces = new Surface[shape.Surfaces.Length];
							}
							Array.Copy(shape.Surfaces, compareShape.prevSurfaces, shape.Surfaces.Length);
						}
				
						surfacesModified = true;
					}
				}				
			}
			
			if (surfacesModified)
			{

				if (External.SetBrushSurfaces != null)
				{
					if (shape == null || shape.Surfaces == null || shape.Surfaces.Length < 4)
					{
						Debug.LogError("Shape is infinitely thin and is invalid");
						brush.ControlMesh.IsValid = false;
					} else
					{
						External.SetBrushSurfaces(brushID,
													shape.Surfaces.Length,
													shape.Surfaces);
						surfacesModified = true;

						if (compareShape.prevSurfaces == null ||
							compareShape.prevSurfaces.Length != shape.Surfaces.Length)
						{
							compareShape.prevSurfaces = new Surface[shape.Surfaces.Length];
						}

						if (shape.Surfaces.Length > 0)
						{
							Array.Copy(shape.Surfaces, compareShape.prevSurfaces, shape.Surfaces.Length);
						}
					}
				}
			}
			
			if (surfacesModified)
			{
				SetBrushMesh(brush);
			}
		}
		#endregion

		#region ValidateBrush
		public static void ValidateBrush(CSGBrush brush, bool surfacesModified = false)
		{
			if (External == null)
			{
				return;
			}

			// make sure that this is not a prefab that's not actually in the scene ...
			if (!brush || !brush.gameObject.activeInHierarchy)
			{
				return;
			}

			if (!brush.IsRegistered)
			{
				RegisterBrushes  .Remove(brush);
				UnregisterBrushes.Remove(brush);
				RegisterBrush(brush);
				 
				// make sure it's registered, otherwise ignore it
				if (!brush.IsRegistered)
					return;
			}
			
			EnsureInitialized(brush);

			var wasRegistered = brush.IsRegistered;
			var brushCache = InternalCSGModelManager.GetBrushCache(brush);			
			if (brush.OperationType != brushCache.prevOperation ||
				brush.ContentLayer != brushCache.prevContentLayer)
			{
				brushCache.prevOperation = brush.OperationType;
				brushCache.prevContentLayer = brush.ContentLayer;

				External.SetBrushOperationType(brush.brushID,
											   brush.ContentLayer,
											   brush.OperationType);
			}
			
			// check if surfaces changed
			CheckSurfaceModifications(brush, surfacesModified);
			
			if (wasRegistered && 
				brushCache.controlMeshGeneration != brush.ControlMesh.Generation)
			{
				brushCache.controlMeshGeneration = brush.ControlMesh.Generation;
				ControlMeshUtility.RebuildShape(brush);
			}
		}
		#endregion
	

		#region CheckTransformChanged

		internal const int BrushCheckChunk = 100;
		internal static int BrushCheckPos = 0;

		public static void CheckTransformChanged()
		{
			if (External == null)
			{
				return;
			}

			for (int i = 0; i < Operations.Length; i++)
			{
				var operation = Operations[i];
				if (!Operations[i])
					continue;

				var operationCache = _operationCaches[operation.operationID]; // CSGModelManager.GetOperationCache(operation);
				if ((int)operationCache.PrevOperation != (int)operation.OperationType)
				{
					operationCache.PrevOperation = operation.OperationType;
					External.SetOperationOperationType(operation.operationID,
													   operationCache.PrevOperation);
				}
			}

			for (int i = 0; i < Models.Length; i++)
			{
				if (!Models[i])
					continue;

				if (!Models[i].cachedTransform)
					Models[i].cachedTransform = Models[i].transform;

				Models[i].cachedPosition = Models[i].cachedTransform.position;
			}
			
			for (int brushID = 0; brushID < _brushCaches.Length; brushID++)
			{
				//if (brushID <  BrushCheckPos ||
				//	brushID >= BrushCheckPos + BrushCheckChunk)
				//	continue;
				var brushCache			= _brushCaches[brushID];
				if (brushCache == null)
					continue;

				var brush				= _brushCaches[brushID].brush;
				if (System.Object.ReferenceEquals(brush, null))
					continue;

				if (!brush || brush.brushID != brushID)
				{
					if (brush.IsRegistered)
						OnDestroyed(brush);
					_brushCaches[brushID].Reset();
					continue;
				}

				var brushNodeID			= brush.nodeID;
				// make sure it's registered, otherwise ignore it
				if (brushNodeID == CSGNode.InvalidNodeID)
					continue;

				//20.5ms
				var brushTransform		= brushCache.hierarchyItem.Transform;

				var currentPosition		= brushTransform.position;				
				var currentScale		= brushTransform.lossyScale;
				var currentRotation		= brushTransform.rotation;

				var modelLocalPosition	= currentPosition;
				var modelPosition		= brushCache.childData.Model.cachedPosition;
				modelLocalPosition.x -= modelPosition.x;
				modelLocalPosition.y -= modelPosition.y;
				modelLocalPosition.z -= modelPosition.z;
				
				//16.8ms
				if (brushCache.compareTransformation.modelLocalPosition.x != modelLocalPosition.x ||
					brushCache.compareTransformation.modelLocalPosition.y != modelLocalPosition.y ||
					brushCache.compareTransformation.modelLocalPosition.z != modelLocalPosition.z)
				{
					brushCache.compareTransformation.prevPosition		= currentPosition;
					brushCache.compareTransformation.modelLocalPosition = modelLocalPosition;
					var translation = brushCache.compareTransformation.modelLocalPosition;

					External.SetBrushTranslation(brushID, ref translation);
				}
				
				//23.0ms
				if (brushCache.compareTransformation.prevScale.x	!= currentScale.x ||
					brushCache.compareTransformation.prevScale.y	!= currentScale.y ||
					brushCache.compareTransformation.prevScale.z	!= currentScale.z ||
					brushCache.compareTransformation.prevRotation.x	!= currentRotation.x ||
					brushCache.compareTransformation.prevRotation.y	!= currentRotation.y ||
					brushCache.compareTransformation.prevRotation.z	!= currentRotation.z ||
					brushCache.compareTransformation.prevRotation.w	!= currentRotation.w)
				{
					brushCache.compareTransformation.prevScale		= currentScale;
					brushCache.compareTransformation.prevRotation	= currentRotation;
					
					brushCache.compareTransformation.objectToPlaneSpace = Matrix4x4.TRS(-currentPosition, MathConstants.identityQuaternion, MathConstants.oneVector3) * brushTransform.localToWorldMatrix;
					brushCache.compareTransformation.planeToObjectSpace = brushCache.compareTransformation.objectToPlaneSpace.inverse;

					External.SetBrushTransformation(brushID,
													ref brushCache.compareTransformation.planeToObjectSpace,
													ref brushCache.compareTransformation.objectToPlaneSpace);
					if (brush.ControlMesh != null)
						brush.ControlMesh.Generation = brushCache.controlMeshGeneration + 1;
				}
				
				if (brush.OperationType != brushCache.prevOperation ||
					brush.ContentLayer != brushCache.prevContentLayer)
				{
					brushCache.prevOperation = brush.OperationType;
					brushCache.prevContentLayer = brush.ContentLayer;

					External.SetBrushOperationType(brushID,
												   brush.ContentLayer,
												   brush.OperationType);
				}

				if (brush.ControlMesh == null)
				{
					brush.ControlMesh = ControlMeshUtility.EnsureValidControlMesh(brush);
					if (brush.ControlMesh == null)
						continue;
					
					brushCache.controlMeshGeneration = brush.ControlMesh.Generation;
					ControlMeshUtility.RebuildShape(brush);
				} else
				if (brushCache.controlMeshGeneration != brush.ControlMesh.Generation)
				{
					brushCache.controlMeshGeneration = brush.ControlMesh.Generation;
					ControlMeshUtility.RebuildShape(brush);
				}
			}

			BrushCheckPos += BrushCheckChunk;
			if (BrushCheckPos > _brushCaches.Length)
				BrushCheckPos = 0;

			for (var i = 0; i < Models.Length; i++)
				MeshInstanceManager.ValidateModel(Models[i]);

			MeshInstanceManager.UpdateTransforms();
		}
		#endregion

		#region UndoRedoPerformed
		public static void UndoRedoPerformed()
		{
			BrushOutlineManager.ClearOutlines();

			//Rebuild();
			Refresh(forceHierarchyUpdate: true);
		}
		#endregion

		public static void UpdateHierarchy()
		{
			_isHierarchyModified = true;
		}

		public static bool skipRefresh = false;
		
		#region Refresh
		public static void Refresh(bool forceHierarchyUpdate = false)
		{
			if (!forceHierarchyUpdate && skipRefresh)
				return;
			 
			lock (LockObj)
			{ 
				if (!_isInitialized)
				{
					RegisterAllComponents();
					forceHierarchyUpdate = true;
					_isInitialized = true;

					// FIXME: when loading level / after compiling script the first 
					//			generated mesh is wrong somehow and invalidates lighting
					InternalCSGModelManager.OnHierarchyModified();
					UpdateRemoteMeshes(); 
				}
				
				//Profiler.BeginSample("UpdateModels");
				forceHierarchyUpdate = InternalCSGModelManager.UpdateModelSettings() || forceHierarchyUpdate;
				//Profiler.EndSample();
				
				// unfortunately this is the only reliable way I could find
				// to determine when a transform is modified, either in 
				// the inspector or the scene.
				//Profiler.BeginSample("CheckTransformChanged");
				InternalCSGModelManager.CheckTransformChanged();
				//Profiler.EndSample();
				
				if (_isHierarchyModified || forceHierarchyUpdate)
				{
					//Profiler.BeginSample("OnHierarchyModified");
					InternalCSGModelManager.OnHierarchyModified();
					//Profiler.EndSample();
					_isHierarchyModified = false;
				}
				
				//Profiler.BeginSample("UpdateMeshes");
				InternalCSGModelManager.UpdateMeshes();
				//Profiler.EndSample();
				
				//Profiler.BeginSample("UpdateHelperSurfaceVisibility");
				MeshInstanceManager.UpdateHelperSurfaceVisibility();
				//Profiler.EndSample();
			}
		}
		#endregion

		#region RefreshMeshes
		public static void RefreshMeshes()
		{
			if (skipRefresh)
				return;

			lock (LockObj)
			{
				//Profiler.BeginSample("UpdateModels");
				InternalCSGModelManager.UpdateModelSettings();
				//Profiler.EndSample();

				//Profiler.BeginSample("UpdateMeshes");
				InternalCSGModelManager.UpdateMeshes();
				//Profiler.EndSample();

				//Profiler.BeginSample("UpdateHelperSurfaceVisibility");
				MeshInstanceManager.UpdateHelperSurfaceVisibility();
				//Profiler.EndSample();
			}
		}
		#endregion

		#region UpdateModelSettings
		public static bool UpdateModelSettings()
		{
			var forceHierarchyUpdate = false;
			for (var i = 0; i < Models.Length; i++)
			{ 
				var model = Models[i];
				if (!model || !model.isActiveAndEnabled)
					continue;
				var modelModified	= false;
				var invertedWorld	= model.InvertedWorld;
				if (invertedWorld)
				{
					if (model.infiniteBrush == null)
					{
						var gameObject = new GameObject("*hidden infinite brush*");
						gameObject.hideFlags = MeshInstanceManager.ComponentHideFlags;
						gameObject.transform.SetParent(model.transform, false);

						model.infiniteBrush = gameObject.AddComponent<CSGBrush>();
						model.infiniteBrush.flags = BrushFlags.InfiniteBrush;

						modelModified	= true;
					}
					if (model.infiniteBrush.transform.GetSiblingIndex() != 0)
					{
						model.infiniteBrush.transform.SetSiblingIndex(0);
						modelModified	= true;
					}
				} else
				{
					if (model.infiniteBrush)
					{
						if (model.infiniteBrush.gameObject)
							UnityEngine.Object.DestroyImmediate(model.infiniteBrush.gameObject);
						model.infiniteBrush = null;
						modelModified	= true;
					}
				}
				if (modelModified)
				{
					var childBrushes = model.GetComponentsInChildren<CSGBrush>();
					for (int j = 0; j < childBrushes.Length; j++)
					{
						if (!childBrushes[j] ||
							childBrushes[j].ControlMesh == null)
							continue;
						childBrushes[j].ControlMesh.Generation++;
					}
					forceHierarchyUpdate = true;
				}
			}
			return forceHierarchyUpdate;
		}
		#endregion

		#region UpdateChildList
		struct TreePosition
		{
			public TreePosition(HierarchyItem _item) { item = _item; index = 0; }
			public HierarchyItem item;
			public int index;
		}

		static Int32[] UpdateChildList(HierarchyItem top)
		{
			var ids = new List<int>();
			{
				var parents = new List<TreePosition>();
				parents.Add(new TreePosition(top));
				while (parents.Count > 0)
				{
					var parent		= parents[parents.Count - 1];
					parents.RemoveAt(parents.Count - 1);
					var children	= parent.item.ChildNodes;
					for (var i = parent.index; i < children.Length; i++)
					{
						var node = children[i];
						var nodeID = node.NodeID;
						if (nodeID < 0)
						{
							var next_index = i + 1;
							if (next_index < children.Length)
							{
								parent.index = next_index;
								parents.Add(parent);
							}
							parents.Add(new TreePosition(node));
							break;
						}
						ids.Add(nodeID);
						if (node.PrevSiblingIndex != node.SiblingIndex)
						{
							External.UpdateNode(nodeID);
							node.PrevSiblingIndex = node.SiblingIndex;
						}
					}
				}
			}
			return ids.ToArray();
		}
		#endregion

		private static readonly object LockObj = new object();

		private static readonly CSGBrush[]		EmptyBrushArray		= new CSGBrush[0];
		private static readonly CSGOperation[]	EmptyOperationArray	= new CSGOperation[0];
		private static readonly CSGModel[]		EmptyModelArray		= new CSGModel[0];

		internal static double registerTime = 0.0;
		internal static double validateTime = 0.0;
		internal static double updateHierarchyTime = 0.0;

		public static void OnHierarchyModified()
		{
			if (External == null)
			{
				return;
			}

			HierarchyItem.CurrentLoopCount = (HierarchyItem.CurrentLoopCount + 1);


			for (var i = 0; i < Brushes.Length; i++)
			{
				var brush = Brushes[i];
				if (!brush && brush.IsRegistered)
					OnDestroyed(brush);
			}
			for (var i = 0; i < Operations.Length; i++)
			{
				var operation = Operations[i];
				if (!operation && operation.IsRegistered)
					OnDestroyed(operation);
			}
			for (var i = 0; i < Models.Length; i++)
			{
				var model = Models[i];
				if (!model && model.IsRegistered)
					OnDestroyed(model);
			}

			// unregister old components 
			if (UnregisterBrushes.Count > 0 || UnregisterOperations.Count > 0 || DisableModels.Count > 0)
			{
				var unregisterBrushesList       = (UnregisterBrushes   .Count == 0) ? EmptyBrushArray     : UnregisterBrushes   .ToArray();
				var unregisterOperationsList    = (UnregisterOperations.Count == 0) ? EmptyOperationArray : UnregisterOperations.ToArray();
				var disableModelsList           = (DisableModels       .Count == 0) ? EmptyModelArray     : DisableModels       .ToArray();
				UnregisterBrushes.Clear();
				UnregisterOperations.Clear();
				DisableModels.Clear();

				if (unregisterBrushesList.Length > 0 ||
					unregisterOperationsList.Length > 0 ||
					disableModelsList.Length > 0)
					_isHierarchyModified = true;

				for (var i = 0; i < unregisterBrushesList.Length; i++)
				{
					var component = unregisterBrushesList[i];
					if (component) InternalCSGModelManager.UnregisterBrush(component);
				}
				for (var i = 0; i < unregisterOperationsList.Length; i++)
				{
					var component = unregisterOperationsList[i];
					if (component) InternalCSGModelManager.UnregisterOperation(component);
				}
				for (var i = 0; i < disableModelsList.Length; i++)
				{
					var component = disableModelsList[i];
					if (component) InternalCSGModelManager.DisableModel(component);
				}
			}


			var startTime = EditorApplication.timeSinceStartup;
			// register new components
			if (RegisterModels.Count > 0 || EnableModels.Count > 0 || RegisterOperations.Count > 0 || RegisterBrushes.Count > 0)
			{
				var registerModelsList      = (RegisterModels    .Count == 0) ? EmptyModelArray     : RegisterModels.ToArray();
				var enableModelsList        = (EnableModels      .Count == 0) ? EmptyModelArray     : EnableModels.ToArray();
				var registerOperationsList  = (RegisterOperations.Count == 0) ? EmptyOperationArray : RegisterOperations.ToArray();
				var registerBrushesList     = (RegisterBrushes   .Count == 0) ? EmptyBrushArray     : RegisterBrushes.ToArray();
				RegisterModels.Clear();
				EnableModels.Clear();
				RegisterOperations.Clear();
				RegisterBrushes.Clear();

				if (registerBrushesList.Length > 0 ||
					registerModelsList.Length > 0 ||
					enableModelsList.Length > 0 ||
					registerOperationsList.Length > 0)
					_isHierarchyModified = true;

				var prefabInstances = new HashSet<GameObject>();
				{ 
					var foundPrefabs	= new HashSet<GameObject>();
					for (var i = registerModelsList.Length - 1; i >= 0; i--)
					{
						var component = registerModelsList[i];
						var prefabType = PrefabUtility.GetPrefabType(component);
						if (prefabType == PrefabType.None || prefabType == PrefabType.Prefab)
							continue;
						var parent = PrefabUtility.FindPrefabRoot(component.gameObject);
						if (!parent) continue;
						if (prefabInstances.Contains(parent))
						{
							ArrayUtility.RemoveAt(ref registerModelsList, i);
							continue;
						}
						if (foundPrefabs.Contains(parent)) continue;
						var parentNode = parent.GetComponent<CSGNode>();
						foundPrefabs.Add(parent);
						if (!parentNode) continue;
						if (parentNode.PrefabBehaviour != PrefabInstantiateBehaviour.Copy) continue;
						prefabInstances.Add(parent);
						ArrayUtility.RemoveAt(ref registerModelsList, i);
					}
					for (var i = registerOperationsList.Length - 1; i >= 0; i--)
					{
						var component = registerOperationsList[i];
						var prefabType = PrefabUtility.GetPrefabType(component);
						if (prefabType == PrefabType.None || prefabType == PrefabType.Prefab)
							continue;
						var parent = PrefabUtility.FindPrefabRoot(component.gameObject);
						if (!parent) continue;
						if (prefabInstances.Contains(parent))
						{
							ArrayUtility.RemoveAt(ref registerOperationsList, i);
							continue;
						}
						if (foundPrefabs.Contains(parent)) continue;
						var parentNode = parent.GetComponent<CSGNode>();
						foundPrefabs.Add(parent);
						if (!parentNode) continue;
						if (parentNode.PrefabBehaviour != PrefabInstantiateBehaviour.Copy) continue;
						prefabInstances.Add(parent);
						ArrayUtility.RemoveAt(ref registerOperationsList, i);
					}
					for (var i = registerBrushesList.Length - 1; i >= 0; i--)
					{
						var component = registerBrushesList[i];
						var prefabType = PrefabUtility.GetPrefabType(component);
						if (prefabType == PrefabType.None || prefabType == PrefabType.Prefab)
							continue;
						var parent = PrefabUtility.FindPrefabRoot(component.gameObject);
						if (!parent) continue;
						if (prefabInstances.Contains(parent))
						{
							ArrayUtility.RemoveAt(ref registerBrushesList, i);
							continue;
						}
						if (foundPrefabs.Contains(parent)) continue;
						var parentNode = parent.GetComponent<CSGNode>();
						foundPrefabs.Add(parent);
						if (!parentNode) continue;
						if (parentNode.PrefabBehaviour != PrefabInstantiateBehaviour.Copy) continue;
						prefabInstances.Add(parent);
						ArrayUtility.RemoveAt(ref registerBrushesList, i);
					}
				}

				// we've found new prefabs that should have been copied instead of prefab-instances (this can happen when you drag it into the hierarchy)
				if (prefabInstances.Count > 0)
				{
					foreach (var instance in prefabInstances)
						Undo.ClearUndo(instance);
					Undo.IncrementCurrentGroup();
					int currentGroup = Undo.GetCurrentGroup(); 
					var selectionChanged = false;
					var selection = Selection.objects;
					foreach (var instance in prefabInstances)
					{
						var instanceTransform	= instance.transform;
						var instanceParent		= instanceTransform.parent;
						var instanceIndex		= instanceTransform.GetSiblingIndex();

						GameObject newInstance;
						if (instanceParent) newInstance = (GameObject)UnityEngine.Object.Instantiate(instance, instanceParent);
						else				newInstance = GameObject.Instantiate(instance);

						if (ArrayUtility.Contains(selection, instance))
						{
							ArrayUtility.Remove(ref selection, instance);
							ArrayUtility.Add(ref selection, newInstance);
							selectionChanged = true;
						}

						Undo.RegisterCreatedObjectUndo(newInstance, "Copied prefab");
						Undo.RecordObject(newInstance.transform, "Copied prefab");
						newInstance.name = instance.name;
						UnityEngine.Object.DestroyImmediate(instance);
						newInstance.transform.SetSiblingIndex(instanceIndex);
					}
					if (selectionChanged)
					{
						Selection.objects = selection;
					}
					Undo.CollapseUndoOperations(currentGroup);
				}



				for (var i = 0; i < registerModelsList.Length; i++)
				{
					var component = registerModelsList[i];
					InternalCSGModelManager.RegisterModel(component);
					if (component.isActiveAndEnabled)
						SelectionUtility.LastUsedModel = component;
				}
				for (var i = 0; i < enableModelsList.Length; i++)
				{
					var component = enableModelsList[i];
					InternalCSGModelManager.EnableModel(component);
				}
				for (var i = 0; i < registerOperationsList.Length; i++)
				{
					var component = registerOperationsList[i];
					InternalCSGModelManager.RegisterOperation(component);
				}
				// TODO: register all brushes in list at the same time, optimize for lots of brushes at same time
				for (var i = 0; i < registerBrushesList.Length; i++)
				{
					var component = registerBrushesList[i];
					InternalCSGModelManager.RegisterBrush(component);
				}
				for (var i = 0; i < registerModelsList.Length; i++)
				{
					var component = registerModelsList[i];
					MeshInstanceManager.EnsureInitialized(component);
				}
			}
			registerTime = EditorApplication.timeSinceStartup - startTime;

			startTime = EditorApplication.timeSinceStartup;
			// validate components
			if (ValidateBrushes.Count > 0 || ValidateOperations.Count > 0)
			{
				var validateBrushesList     = (ValidateBrushes   .Count == 0) ? EmptyBrushArray     : ValidateBrushes.ToArray();
				var validateOperationsList  = (ValidateOperations.Count == 0) ? EmptyOperationArray : ValidateOperations.ToArray();
				ValidateBrushes.Clear();
				ValidateOperations.Clear();
				
				for (var i = 0; i < validateBrushesList.Length; i++)
				{
					var component = validateBrushesList[i];
					InternalCSGModelManager.ValidateBrush(component);
				}
				for (var i = 0; i < validateOperationsList.Length; i++)
				{
					var component = validateOperationsList[i];
					InternalCSGModelManager.ValidateOperation(component);
				}
			}

			if (OperationTransformChanged.Count > 0)
			{ 
				foreach (var item in OperationTransformChanged)
				{
					if (item) CheckOperationHierarchy(item);
				}
				OperationTransformChanged.Clear();
			}


			if (BrushTransformChanged.Count > 0)
			{
				foreach (var item in BrushTransformChanged)
				{
					if (!item)
						continue;

					CheckBrushHierarchy(item);
					ValidateBrush(item); // to detect material changes when moving between models
				}
				BrushTransformChanged.Clear();
			}
			validateTime = EditorApplication.timeSinceStartup - startTime;


			// remove all nodes that have been scheduled for removal
			if (RemovedBrushes   .Count > 0) { External.RemoveBrushes   (RemovedBrushes   .ToArray()); RemovedBrushes   .Clear(); }
			if (RemovedOperations.Count > 0) { External.RemoveOperations(RemovedOperations.ToArray()); RemovedOperations.Clear(); }
			if (RemovedModels    .Count > 0) { External.RemoveModels    (RemovedModels    .ToArray()); RemovedModels    .Clear(); }
			

			for (var i = Brushes.Length - 1; i >= 0; i--)
			{
				var item = Brushes[i];
				if (item && item.brushID >= 0)
					continue;

				UnregisterBrush(item);
			}
			
			for (var i = Operations.Length - 1; i >= 0; i--)
			{
				var item = Operations[i];
				if (!item || item.operationID < 0)
				{
					UnregisterOperation(item);
					continue;
				}

				var cache = GetOperationCache(item);
				if (!cache.ParentData.Transform)
					cache.ParentData.Init(item, item.nodeID);
			}

			for (var i = Models.Length - 1; i >= 0; i--)
			{
				var item = Models[i];
				if (!item || item.modelID < 0)
				{
					UnregisterModel(item);
					continue;
				}

				var cache = GetModelCache(item);
				if (!cache.ParentData.Transform)
					cache.ParentData.Init(item, item.nodeID);
			}

			startTime = EditorApplication.timeSinceStartup;
			if (External.SetBrushHierarchy != null)
			{ 
				for (var i = Brushes.Length - 1; i >= 0; i--)
				{
					var item = Brushes[i];
					if (!item || item.brushID < 0)
						continue;

					var itemCache	= _brushCaches[item.brushID];
					var parentData	= itemCache.childData.OwnerParentData;
					if (parentData == null)
						continue;

					if (!ParentNodeData.UpdateNodePosition(itemCache.hierarchyItem, parentData))
						continue;
					
					External.SetBrushHierarchy(item.brushID,
											   itemCache.childData.modelID,
											   itemCache.childData.parentID);
				}
			}

			if (External.SetOperationHierarchy != null)
			{
				for (var i = Operations.Length - 1; i >= 0; i--)
				{
					var item = Operations[i];
					if (!item || item.operationID < 0)
						continue;

					var itemCache = _operationCaches[item.operationID];
					var parentData = itemCache.ChildData.OwnerParentData;// GetParentData(item_cache.childData);
					if (parentData == null)
						continue;

					if (!ParentNodeData.UpdateNodePosition(itemCache.ParentData, parentData))
						continue;
					
					External.SetOperationHierarchy(item.operationID,
													itemCache.ChildData.modelID,
													itemCache.ChildData.parentID);
				}
			}

			if (External.SetOperationChildren != null)
			{
				for (var i = 0; i < Operations.Length; i++)
				{
					var item = Operations[i];
					if (!item || item.operationID < 0)
						continue;

					var itemCache = _operationCaches[item.operationID];
					if (!itemCache.ParentData.ChildrenModified)
						continue;

					var childList = UpdateChildList(itemCache.ParentData);
					External.SetOperationChildren(item.operationID, childList.Length, childList);
					itemCache.ParentData.ChildrenModified = false;
				}
			}

			if (External.SetModelChildren != null)
			{
				for (var i = 0; i < Models.Length; i++)
				{
					var item = Models[i];
					if (!item || item.modelID < 0)
						continue;

					var itemCache = _modelCaches[item.modelID];
					if (!itemCache.ParentData.ChildrenModified)
						continue;

					var childList = UpdateChildList(itemCache.ParentData);
					External.SetModelChildren(item.modelID, childList.Length, childList);
					itemCache.ParentData.ChildrenModified = false;
				}
			}

			updateHierarchyTime = EditorApplication.timeSinceStartup - startTime;
		}
		

		#region UpdateRemoteMeshes
		public static void UpdateRemoteMeshes()
		{
			if (External != null &&
				External.UpdateModelMeshes != null)
				External.UpdateModelMeshes();
		}
		#endregion

		#region UpdateMeshes

		static MethodInfo[] modelModifiedEvents;

		[InitializeOnLoadMethod]
		static void FindModelModifiedAttributeMethods()
		{
			var foundMethods = new List<MethodInfo>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var allTypes = assembly.GetTypes();
				for (var i = 0; i < allTypes.Length; i++)
				{
					var allMethods = allTypes[i].GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
					for (var j = 0; j < allMethods.Length; j++)
					{
						if (Attribute.IsDefined(allMethods[j], typeof(CSGModelModifiedEventAttribute)))
						{
							var parameters = allMethods[j].GetParameters();
							if (parameters.Length != 2 ||
								parameters[0].ParameterType != typeof(CSGModel) ||
								parameters[1].ParameterType != typeof(GameObject[]))
							{
								Debug.LogWarning("Found static method with OnCSGModelModifiedAttribute, but incorrect signature. It should be:\nstatic void my_method_name(CSGModel model, GameObject[] modifiedMeshes);\nWhere my_method_name can be any name");
							}
							foundMethods.Add(allMethods[j]);
						}
					}
				}
			}
			modelModifiedEvents = foundMethods.ToArray();
		}

		static bool ignoreEvents = false;
		static void OnPostModelModified(CSGModel model, GameObject[] modifiedMeshes)
		{
			if (ignoreEvents) 
				return;
			ignoreEvents = true;
			try
			{
				for (var i = 0; i < modelModifiedEvents.Length; i++)
				{
					modelModifiedEvents[i].Invoke(null, new object[] { model, modifiedMeshes });
				}
			}
			finally
			{
				ignoreEvents = false;
			}
		}

		static bool forcedUpdateRequired = false;

		public static void DoForcedMeshUpdate()
		{
			forcedUpdateRequired = true;
		}

		internal static int MeshGeneration = 0;

		internal static int prevSubMeshCount = 0;
		internal static UInt64[] vertexHashValues	= null;
		internal static UInt64[] triangleHashValues = null;
		internal static UInt64[] surfaceHashValues	= null;
		internal static Int32[] vertexCounts		= null;
		internal static Int32[] indexCounts			= null;

		internal static double createModelMeshesTime = 0.0;
		
		private static void GetModelMesh(GeneratedMeshes meshContainer, int modelID, ref Matrix4x4 modelMatrix, VertexChannelFlags vertexChannelFlags, MeshType meshType, int meshUniqueID, Material material, PhysicMaterial defaultPhysicsMaterial, bool vertexLayoutModified, HashSet<GeneratedMeshInstance> foundInstances)
		{
			var startCreateModelMeshesTime = EditorApplication.timeSinceStartup;
			UInt32 subMeshCount = External.CreateModelMeshes(modelID, ref modelMatrix, meshType, meshUniqueID, vertexChannelFlags);

			if (subMeshCount == 0)
				return;

			if (subMeshCount > prevSubMeshCount)
			{ 
				vertexHashValues		= new UInt64[subMeshCount];
				triangleHashValues		= new UInt64[subMeshCount];
				surfaceHashValues		= new UInt64[subMeshCount];
				vertexCounts			= new Int32[subMeshCount];
				indexCounts				= new Int32[subMeshCount];
			}
			
			if (!External.GetSubMeshStatus(subMeshCount,
											vertexHashValues,
											triangleHashValues,
											surfaceHashValues,
											indexCounts,
											vertexCounts))
			{
				Debug.LogWarning("GetSubMeshStatus failed");
				return;
			}
			createModelMeshesTime += EditorApplication.timeSinceStartup - startCreateModelMeshesTime;

			for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
			{ 
				var vertexCount	= vertexCounts[subMeshIndex];
				var indexCount	= indexCounts[subMeshIndex];

				if ((vertexCount <= 0 || vertexCount > 65535) || 
					(indexCount  <= 0))
				{
					if (vertexCount > 0 && indexCount > 0)
					{
						Debug.LogError("Mesh has too many vertices (vertexCount > 65535)");
					}
					continue;
				}

				var meshInstance = MeshInstanceManager.GetMesh(meshContainer, subMeshIndex, meshType, material, defaultPhysicsMaterial);
				if (!meshInstance)
					continue;

				var vertexHashValue		= vertexHashValues[subMeshIndex];
				var triangleHashValue	= triangleHashValues[subMeshIndex];
				var surfaceHashValue	= surfaceHashValues[subMeshIndex];
						
				if (!vertexLayoutModified && 
					meshInstance.VertexHashValue	== vertexHashValue &&
					meshInstance.TriangleHashValue	== triangleHashValue &&
					meshInstance.SurfaceHashValue	== surfaceHashValue && 
					meshInstance.SharedMesh)
				{
					foundInstances.Add(meshInstance);
					continue;
				}

				startCreateModelMeshesTime = EditorApplication.timeSinceStartup;
				// create our arrays on the C# side with the correct size
				var tangents	= ((vertexChannelFlags & VertexChannelFlags.Tangent) != 0) ? new Vector4[vertexCount] : null;
				var normals		= ((vertexChannelFlags & VertexChannelFlags.Normal ) != 0) ? new Vector3[vertexCount] : null;
				var uvs			= ((vertexChannelFlags & VertexChannelFlags.UV0    ) != 0) ? new Vector2[vertexCount] : null;
				var colors		= ((vertexChannelFlags & VertexChannelFlags.Color  ) != 0) ? new Color[vertexCount] : null;
				var positions	= new Vector3[vertexCount];
				
				var success = External.FillVertices((Int32)subMeshIndex,
													(Int32)vertexCount,
													colors,
													tangents,
													normals,
													positions,
													uvs);

				// .. and then fill our arrays from the C++ side of things
				if (!success)
				{
					Debug.LogWarning("FillVertices failed");
					continue;
				}

				// retrieve the indices for the sub-mesh for the given materialIndex
				var indices = new int[indexCount];
				//Profiler.BeginSample("FillIndices");
				success = External.FillIndices((Int32)subMeshIndex,
											   (Int32)indices.Length,
											   indices);
				//Profiler.EndSample();
				if (!success)
				{
					Debug.LogWarning("FillIndices failed");
					continue;
				}
				createModelMeshesTime += EditorApplication.timeSinceStartup - startCreateModelMeshesTime;

				MeshInstanceManager.ClearMesh(meshInstance);

				if (positions.Length == 0 ||
					float.IsNaN(positions[0].x) || float.IsInfinity(positions[0].x) ||
					float.IsNaN(positions[0].y) || float.IsInfinity(positions[0].y) ||
					float.IsNaN(positions[0].z) || float.IsInfinity(positions[0].z))
				{
					if (meshInstance &&
						meshInstance.gameObject)
						UnityEngine.Object.DestroyImmediate(meshInstance.gameObject);
					continue;
				}

				var sharedMesh = meshInstance.SharedMesh;

				// finally, we start filling our (sub)meshes using the C# arrays
				sharedMesh.Clear(keepVertexLayout:!vertexLayoutModified);
				sharedMesh.MarkDynamic();
				sharedMesh.vertices = positions;
				if ((vertexChannelFlags & VertexChannelFlags.Tangent) != 0) sharedMesh.tangents	= tangents; 
				if ((vertexChannelFlags & VertexChannelFlags.Normal ) != 0) sharedMesh.normals	= normals;	
				if ((vertexChannelFlags & VertexChannelFlags.UV0    ) != 0) sharedMesh.uv		= uvs;		
				if ((vertexChannelFlags & VertexChannelFlags.Color  ) != 0) sharedMesh.colors	= colors;
			
				// fill the sub-mesh indices
				if (sharedMesh.subMeshCount != 1)
					sharedMesh.subMeshCount = 1;

				// fill the sub-mesh with the given indices
				sharedMesh.triangles = indices;

				sharedMesh.RecalculateBounds();

				meshInstance.ResetLighting		= vertexLayoutModified || (meshInstance.VertexHashValue != vertexHashValue) || (meshInstance.TriangleHashValue != triangleHashValue);
				meshInstance.SharedMesh			= sharedMesh;
				meshInstance.VertexHashValue	= vertexHashValue;
				meshInstance.TriangleHashValue	= triangleHashValue;
				meshInstance.SurfaceHashValue	= surfaceHashValue;

				if (meshInstance.ResetLighting)
				{
					EditorUtility.SetDirty(meshInstance);
					EditorSceneManager.MarkSceneDirty(meshInstance.gameObject.scene);
				}
				foundInstances.Add(meshInstance);
			}
		}

		static bool inUpdateMeshes = false;
		public static bool UpdateMeshes(System.Text.StringBuilder text = null, bool forceUpdate = false)
		{
			if (EditorApplication.isPlaying
				|| EditorApplication.isPlayingOrWillChangePlaymode)
				return false;
			
			if (inUpdateMeshes)
				return false;
			
			var getModelMeshTime = 0.0;
			createModelMeshesTime = 0;

			inUpdateMeshes = true;
			try
			{
				if (External == null)
					return false;

				if (forcedUpdateRequired)
				{
					forceUpdate = true;
					forcedUpdateRequired = false;
				}

				var modelCount = Models.Length;
				if (modelCount == 0)
					return false;

				for (var i = 0; i < modelCount; i++)
				{
					var model = Models[i];
					var modelCache = InternalCSGModelManager.GetModelCache(model);
					if (modelCache == null ||
						!modelCache.IsEnabled)
					{
						continue;
					}

					var renderSurfaceType = ModelTraits.GetModelSurfaceType(model);
					modelCache.RenderSurfaceType = renderSurfaceType;

					if (!forceUpdate &&
						!modelCache.ForceUpdate &&
						model.VertexChannels == modelCache.VertexChannelFlags)
						continue;

					External.ForceModelUpdate(model.modelID);
					modelCache.ForceUpdate = false;
				}

				//Profiler.BeginSample("UpdateModelMeshes");
				var needToUpdateMesh = External.UpdateModelMeshes();
				//Profiler.EndSample();
				// update the model meshes
				if (!needToUpdateMesh)
					// nothing to do
					return false;

				var instances = new List<GameObject>();
				MeshGeneration++;

				var culledMaterial		= MaterialUtility.CulledMaterial;
				var hiddenMaterial		= MaterialUtility.HiddenMaterial;
				var colliderMaterial	= MaterialUtility.ColliderMaterial;
				var triggerMaterial		= MaterialUtility.TriggerMaterial;

				for (var i = 0; i < modelCount; i++)
				{
					var model = Models[i];
					if (!model)
					{
						continue;
					}

					var vertexChannelFlags = model.VertexChannels;

					EnsureInitialized(model);
					MeshInstanceManager.EnsureInitialized(model);

					var modelCache = InternalCSGModelManager.GetModelCache(model);
					if (modelCache == null ||
						!modelCache.IsEnabled)
					{
						continue;
					}

					var vertexLayoutModified = modelCache.VertexChannelFlags != model.VertexChannels;

					modelCache.VertexChannelFlags = model.VertexChannels;

					var modelTransform = model.transform;
					var modelMatrix = Matrix4x4.TRS(MathConstants.zeroVector3,
													modelTransform.rotation,
													modelTransform.lossyScale
													).inverse;

					var modelID = model.modelID;

					int meshDescriptionCount = 0;
					if (!External.HasModelChanged(modelID, ref meshDescriptionCount))// || meshDescriptionCount <= 0)
						continue;

					var meshDescriptions = new MeshDescription[meshDescriptionCount];
					if (!External.GetMeshDescriptions(meshDescriptionCount, meshDescriptions))
						continue;

					var meshContainer = modelCache.GeneratedMeshes;
					if (!meshContainer)
						return false;

					MeshInstanceManager.EnsureInitialized(meshContainer);

					var renderSurfaceType = ModelTraits.GetModelSurfaceType(model);


					var startGetModelMeshTime = EditorApplication.timeSinceStartup;
					var foundInstances = new HashSet<GeneratedMeshInstance>();
					if (model.IsRenderable)
					{
						for (int p = 0; p < meshDescriptions.Length; p++)
						{
							if (meshDescriptions[p].meshType == MeshType.RenderReceiveCastShadows ||
								meshDescriptions[p].meshType == MeshType.RenderReceiveShadows ||
								meshDescriptions[p].meshType == MeshType.RenderCastShadows ||
								meshDescriptions[p].meshType == MeshType.RenderOnly ||
								meshDescriptions[p].meshType == MeshType.ShadowOnly)
							{
								var material = EditorUtility.InstanceIDToObject(meshDescriptions[p].uniqueID) as Material;
								GetModelMesh(meshContainer, modelID, ref modelMatrix, vertexChannelFlags, meshDescriptions[p].meshType, meshDescriptions[p].uniqueID, material, null, vertexLayoutModified, foundInstances);
							}
						}
					}

					if (model.IsTrigger)
					{
						GetModelMesh(meshContainer, modelID, ref modelMatrix, VertexChannelFlags.Normal | VertexChannelFlags.UV0, MeshType.Collider, 0, triggerMaterial, model.DefaultPhysicsMaterial, vertexLayoutModified, foundInstances);
					} else
					if (model.HaveCollider)
					{
						GetModelMesh(meshContainer, modelID, ref modelMatrix, VertexChannelFlags.Normal | VertexChannelFlags.UV0, MeshType.Collider, 0, colliderMaterial, model.DefaultPhysicsMaterial, vertexLayoutModified, foundInstances);
					}

					if (renderSurfaceType == RenderSurfaceType.Normal)
					{
						GetModelMesh(meshContainer, modelID, ref modelMatrix, VertexChannelFlags.UV0, MeshType.Culled, 0, culledMaterial, null, vertexLayoutModified, foundInstances);
					}

					for (int p = 0; p < meshDescriptions.Length; p++)
					{
						if (meshDescriptions[p].meshType != MeshType.Hidden)
							continue;
						GetModelMesh(meshContainer, modelID, ref modelMatrix, VertexChannelFlags.UV0, MeshType.Hidden, 0, hiddenMaterial, null, vertexLayoutModified, foundInstances);
					}

					getModelMeshTime += (EditorApplication.timeSinceStartup - startGetModelMeshTime);


					instances.Clear();
					foreach(var meshInstance in foundInstances)
					{
						if (!meshInstance || !meshInstance.gameObject)
							continue;
						if (meshInstance.RenderSurfaceType == RenderSurfaceType.Normal)
							instances.Add(meshInstance.gameObject);
					}

					MeshInstanceManager.UpdateContainerComponents(meshContainer, foundInstances);

					External.ModelMeshFinishedUpdating(modelID);
					
					OnPostModelModified(model, instances.ToArray());
				}
				MeshInstanceManager.UpdateHelperSurfaceVisibility(force: true);

				
				if (text != null)
				{
					var meshColliderTime = MeshInstanceManager.updateMeshColliderMeshTime;
					text.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
									  "All mesh generation done in {0:F} ms. All Unity mesh updates done in {1:F} ms. MeshCollider setting done in {2:F} ms. ",
									  createModelMeshesTime * 1000, (getModelMeshTime - createModelMeshesTime) * 1000, meshColliderTime * 1000);
				}

				return true;
			}
			finally
			{
				inUpdateMeshes = false;
			}
		}
		#endregion

	}
}