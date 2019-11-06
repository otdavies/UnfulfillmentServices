using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
    internal sealed class OperationsUtility
	{
		public static bool CanModifyOperationsOnSelected()
		{
			foreach (var gameObject in Selection.gameObjects)
			{
				var brush	= gameObject.GetComponentInChildren<CSGBrush>();
				if (brush != null)
					return true;

				var operation	= gameObject.GetComponentInChildren<CSGOperation>();
				if (operation != null)
					return true;
			}
			return false;
		}
		

		public static void SetPassThroughOnSelected()
		{
			var modified = false;
			foreach(var gameObject in Selection.gameObjects)
			{
				var operation	= gameObject.GetComponent<CSGOperation>();
				if (operation == null || operation.PassThrough)
					continue;

				modified = true;
				Undo.RecordObject(operation, "Modifying csg operation of operation component");
				operation.PassThrough = true;
			}

			if (!modified)
				return;

			InternalCSGModelManager.Refresh();
			EditorApplication.RepaintHierarchyWindow();
		}

		public static void ModifyOperationsOnSelected(CSGOperationType operationType)
		{
			var modified = false;
			foreach(var gameObject in Selection.gameObjects)
			{
				var brush		= gameObject.GetComponent<CSGBrush>();
				if (brush != null &&
					brush.OperationType != operationType)
				{
					modified = true;
					Undo.RecordObject(brush, "Modifying csg operation of brush component");
					brush.OperationType = operationType;
				}

				var operation	= gameObject.GetComponent<CSGOperation>();
				if (operation == null || operation.OperationType == operationType)
					continue;

				modified = true;
				Undo.RecordObject(operation, "Modifying csg operation of operation component");
				operation.PassThrough = false;
				operation.OperationType = operationType;
			}
			if (modified)
			{
				InternalCSGModelManager.Refresh();
				EditorApplication.RepaintHierarchyWindow();
			}
		}
		
		
		public static GameObject CreateGameObject(Transform parent, string name, bool worldPositionStays)
		{
			GameObject gameObject;
			if (name == null) gameObject = new GameObject();
			else gameObject = new GameObject(name);
			if (parent != null && parent)
			{
				gameObject.transform.SetParent(parent, worldPositionStays);
				if (name != null)
					gameObject.name = GameObjectUtility.GetUniqueNameForSibling(parent, name);
			}
			return gameObject;
		}


		[UnityEditor.MenuItem("GameObject/Realtime-CSG/Model", false, 30)]
		public static CSGModel CreateModelInstanceInScene()
		{
			var gameObject = new GameObject("Model");
			gameObject.name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(null, "Model");
			var model = InternalCSGModelManager.CreateCSGModel(gameObject);

			UnityEditor.Selection.activeGameObject = gameObject;
			SelectionUtility.LastUsedModel = model;
			Undo.RegisterCreatedObjectUndo(gameObject, "Created model");
			InternalCSGModelManager.Refresh();
			return model;
		}

		public static CSGModel CreateModelInstanceInScene(bool selectModel)
		{
			var gameObject = new GameObject("Model");
			gameObject.name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(null, "Model");
			var model = InternalCSGModelManager.CreateCSGModel(gameObject);

			if (selectModel)
			{
				UnityEditor.Selection.activeGameObject = gameObject;
			}
			SelectionUtility.LastUsedModel = model;
			Undo.RegisterCreatedObjectUndo(gameObject, "Created model");
			InternalCSGModelManager.Refresh();
			return model;
		}

		public static GameObject CreateOperation(Transform parent, string name, bool worldPositionStays)
		{
			var gameObject = CreateGameObject(parent, name, worldPositionStays);
			//var operation = 
			gameObject.AddComponent<CSGOperation>();
			Undo.RegisterCreatedObjectUndo(gameObject, "Created operation");
			InternalCSGModelManager.Refresh();
			return gameObject;
		}

		[UnityEditor.MenuItem("GameObject/Realtime-CSG/Operation", false, 31)]
		public static CSGOperation CreateOperationInstanceInScene()
		{			
			var lastUsedModelTransform = !SelectionUtility.LastUsedModel ? null : SelectionUtility.LastUsedModel.transform;
			if (lastUsedModelTransform == null)
				lastUsedModelTransform = CreateModelInstanceInScene().transform;

			var name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(lastUsedModelTransform, "Operation"); ;
			var gameObject = new GameObject(name);
			gameObject.transform.SetParent(lastUsedModelTransform, true);
			var operation = gameObject.AddComponent<CSGOperation>();

			UnityEditor.Selection.activeGameObject = gameObject;
			Undo.RegisterCreatedObjectUndo(gameObject, "Created operation");
			InternalCSGModelManager.Refresh();
			InternalCSGModelManager.UpdateMeshes();
			return operation;
		}

		public static GameObject CreateBrush(ControlMesh controlMesh, Shape shape, Transform parent, string name, bool worldPositionStays)
		{
#if DEMO
			if (CSGBindings.BrushesAvailable() <= 0)
			{
				return null;
			}
#endif
			var gameObject = CreateGameObject(parent, name, worldPositionStays);
			var brush = gameObject.AddComponent<CSGBrush>();
			brush.ControlMesh = controlMesh;
			brush.Shape = shape;
			gameObject.SetActive(shape != null && controlMesh != null);
			Undo.RegisterCreatedObjectUndo(gameObject, "Created brush");
			InternalCSGModelManager.Refresh();
			return gameObject;
		}

		[UnityEditor.MenuItem("GameObject/Realtime-CSG/Brush", false, 31)]
		public static CSGBrush CreateBrushInstanceInScene()
		{
#if DEMO
			if (CSGBindings.BrushesAvailable() <= 0)
			{
				return null;
			}
#endif
			var lastUsedModelTransform = !SelectionUtility.LastUsedModel ? null : SelectionUtility.LastUsedModel.transform;
			if (lastUsedModelTransform == null)
				lastUsedModelTransform = CreateModelInstanceInScene().transform;

			var name = UnityEditor.GameObjectUtility.GetUniqueNameForSibling(lastUsedModelTransform, "Brush");
			var gameObject = new GameObject(name);
			var brush = gameObject.AddComponent<CSGBrush>();

			gameObject.transform.SetParent(lastUsedModelTransform, true);
			gameObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f); // this aligns it's vertices to the grid
			BrushFactory.CreateCubeControlMesh(out brush.ControlMesh, out brush.Shape, Vector3.one);

			UnityEditor.Selection.activeGameObject = gameObject;
			Undo.RegisterCreatedObjectUndo(gameObject, "Created brush");
			InternalCSGModelManager.Refresh();
			InternalCSGModelManager.UpdateMeshes();
			return brush;
		}

		[UnityEditor.MenuItem("GameObject/Group selection %G", false, 32)]
		public static void GroupSelectionInOperation()
		{
			if (Selection.activeObject == null)
				return;

			var childTransforms = new List<Transform>(Selection.transforms);
			Selection.activeObject = null;
			if (childTransforms.Count == 0)
				return;

			for (int i = childTransforms.Count - 1; i >= 0; i--)
			{
				var iterator = childTransforms[i].parent;
				bool found = false;
				while (iterator != null)
				{
					if (childTransforms.Contains(iterator))
					{
						found = true;
						break;
					}
					iterator = iterator.parent;
				}
				if (found)
				{
					childTransforms.RemoveAt(i);
				}
			}
			var sortedChildTransform = new SortedList<int, Transform>();
			for (int i = 0; i < childTransforms.Count; i++)
			{
				sortedChildTransform.Add(childTransforms[i].GetSiblingIndex(), childTransforms[i]);
			}

			childTransforms.Clear();
			childTransforms.AddRange(sortedChildTransform.Values);
			
			var parentTransforms	= new List<Transform>(childTransforms.Count);
			var groupTransforms		= new List<Transform>(childTransforms.Count);
			var parentIndices		= new List<int>(childTransforms.Count);
			for (int i = 0; i < childTransforms.Count; i ++)
			{
				var index = parentTransforms.IndexOf(childTransforms[i].parent);
				if (index == -1)
				{
					var parent			= childTransforms[i].parent;
					parentTransforms    .Add(parent);
					index = parentTransforms.Count - 1;
				}
				parentIndices.Add(index);
			}

			Undo.IncrementCurrentGroup();
			var undo_group_index = Undo.GetCurrentGroup();
			
			foreach (var parentTransform in parentTransforms)
			{
				var name		= UnityEditor.GameObjectUtility.GetUniqueNameForSibling(parentTransform, "Operation");
				var group		= CreateGameObject(parentTransform, name, false);
				var operation	= group.AddComponent<CSGOperation>();
				operation.PassThrough = true;
				groupTransforms.Add(group.transform);
				Undo.RegisterCreatedObjectUndo(group, "Created operation");
			}


			Transform prevGroup = null;
			//for (int i = childTransforms.Count - 1; i >= 0; i--)
			for (int i = 0; i < childTransforms.Count; i++)
			{
				var group	= groupTransforms[parentIndices[i]];
				var index	= childTransforms[i].GetSiblingIndex();
				Undo.SetTransformParent(childTransforms[i], group, "Moved gameObject under operation");
				Undo.RecordObject(childTransforms[i], "Set sibling index of operation");
				if (prevGroup != group)
					group.SetSiblingIndex(index);
				prevGroup = group;
			}

			Undo.CollapseUndoOperations(undo_group_index);
		}
	}
}
