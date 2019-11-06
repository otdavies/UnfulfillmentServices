using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor;
using RealtimeCSG;
using Object = UnityEngine.Object;

namespace InternalRealtimeCSG
{
	internal sealed class PointSelection
	{
		public PointSelection(int brushIndex, int pointIndex) { BrushIndex = brushIndex; PointIndex = pointIndex; }
		public readonly int BrushIndex;
		public readonly int PointIndex;
	}

	internal static class SceneQueryUtility
	{
		#region GetAllComponentsInScene
		public static List<T> GetAllComponentsInScene<T>(Scene scene)
			where T : Component
		{
			var items = new List<T>();
			var rootItems = GetRootGameObjectsInScene(scene);
			for (int i = 0; i < rootItems.Length; i++)
			{
				var root = rootItems[i];
				if (!root)
					continue;
				items.AddRange(root.GetComponentsInChildren<T>(true));
			}
			return items;
		}

		public static GameObject[] GetRootGameObjectsInScene(Scene scene)
		{
			if (scene.isLoaded)
				return scene.GetRootGameObjects();
			
			var rootLookup = new HashSet<Transform>();
			var transforms = Object.FindObjectsOfType<Transform>();
			for (int i = 0; i < transforms.Length;i++)
				rootLookup.Add(transforms[i].root);

			var rootArray = rootLookup.ToArray();
			var gameObjectArray = new GameObject[rootArray.Length];
			for (int i = 0; i < rootArray.Length; i++)
				gameObjectArray[i] = rootArray[i].gameObject;

			return gameObjectArray;
		}
		#endregion

		#region GetFirstGameObjectInSceneWithName
		public static GameObject GetFirstGameObjectInSceneWithName(Scene scene, string name)
		{
			foreach (var root in scene.GetRootGameObjects())
			{
				if (!root)
					continue;
				if (root.name == name)
					return root;
				foreach (var transform in root.GetComponentsInChildren<Transform>(true))
				{
					if (transform.name == name)
						return transform.gameObject;
				}
			}
			return null;
		}
		#endregion

		#region GetUniqueHiddenGameObjectInSceneWithName
		internal static GameObject GetUniqueHiddenGameObjectInSceneWithName(Scene scene, string name)
		{
			var rootGameObjects = scene.GetRootGameObjects();
			GameObject foundRoot = null;
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				var root = rootGameObjects[i];
				if (!root)
					continue;

				if (root.hideFlags != HideFlags.None &&
					root.name == name)
				{
					if (foundRoot)
					{
						Object.DestroyImmediate(root);
						continue;
					}
					foundRoot = root;
				}

				var rootChildren = root.GetComponentsInChildren<Transform>(true);
				for (int j = 0; j < rootChildren.Length; j++)
				{
					var child = rootChildren[j];
					if (child == root)
						continue;
					if (!child)
						continue;

					if (child.hideFlags == HideFlags.None ||
						child.name != name)
						continue;
					
					if (foundRoot)
					{
						Object.DestroyImmediate(child.gameObject);
						continue;
					}
					foundRoot = child.gameObject;
				}
			}
			return null;
		}
		#endregion


		#region GetGroupObjectIfObjectIsPartOfGroup
		public static GameObject GetGroupGameObjectIfObjectIsPartOfGroup(GameObject gameObject)
		{
			if (gameObject == null)
				return null;

			var node = gameObject.GetComponentInChildren<CSGNode>();
			if (!node)
				return gameObject;

			var operation = GetGroupOperationForNode(node);
			return operation == null ? gameObject : operation.gameObject;
		}
		#endregion

		#region GetGroupOperationForNode (private)
		private static CSGOperation GetGroupOperationForNode(CSGNode node)
		{
			if (!node)
				return null;

			var parent = node.transform.parent;
			while (parent)
			{
				var model = parent.GetComponent<CSGModel>();
				if (model)
					return null;

				var parentOp = parent.GetComponent<CSGOperation>();
				if (parentOp &&
					//!parentOp.PassThrough && 
					parentOp.HandleAsOne)
					return parentOp;

				parent = parent.transform.parent;
			}
			return null;
		}
		#endregion

		#region GetTopMostGroupForNode
		public static CSGNode GetTopMostGroupForNode(CSGNode node)
		{
			if (!node)
				return null;

			var topSelected = node;
			var parent = node.transform.parent;
			while (parent)
			{
				var model = parent.GetComponent<CSGModel>();
				if (model)
					break;

				var parentOp = parent.GetComponent<CSGOperation>();
				if (parentOp &&
					parentOp.HandleAsOne &&
					!parentOp.PassThrough)
					topSelected = parentOp;

				parent = parent.transform.parent;
			}
			return topSelected;
		}
		#endregion


		#region DeselectAllChildBrushes (private)
		private static void DeselectAllChildBrushes(Transform transform, HashSet<GameObject> objectsInFrustum)
		{
			var visibleLayers = Tools.visibleLayers;
			 
			for (int i = 0, childCount = transform.childCount; i < childCount; i++)
			{
				var childTransform = transform.GetChild(i);
				var childNode = childTransform.GetComponent<CSGNode>();
				if (!childNode || (childNode is CSGModel) || ((1 << childNode.gameObject.layer) & visibleLayers) == 0)
					continue;

				var childGameObject = childTransform.gameObject;
				objectsInFrustum.Remove(childGameObject);
				DeselectAllChildBrushes(childTransform.transform, objectsInFrustum);
			}
		}
		#endregion

		#region AreAllBrushesSelected (private)
		private static bool AreAllBrushesSelected(Transform transform, HashSet<GameObject> objectsInFrustum)
		{			
			var visibleLayers = Tools.visibleLayers;
			

			var allChildrenSelected = true;
			var i = 0;
			var childCount = transform.childCount;
			for (; i < childCount; i++)
			{
				var childTransform = transform.GetChild(i);
				var childNode = childTransform.GetComponent<CSGNode>();
				if (!childNode || (childNode is CSGModel) || ((1 << childNode.gameObject.layer) & visibleLayers) == 0)
				{
					continue;
				}

				var childGameObject = childTransform.gameObject;
				if (!childTransform.gameObject.activeInHierarchy)
				{
					objectsInFrustum.Remove(childGameObject);
					continue;
				}

				if (objectsInFrustum.Contains(childGameObject))
				{
					objectsInFrustum.Remove(childGameObject);
					continue;
				}

				var childOperation = childNode as CSGOperation;
				if (childOperation == null ||
					!childOperation.PassThrough)
				{
					objectsInFrustum.Remove(childGameObject);
					allChildrenSelected = false;
					break;
				}

				var result = AreAllBrushesSelected(childTransform, objectsInFrustum);
				objectsInFrustum.Remove(childGameObject);

				if (result)
					continue;

				objectsInFrustum.Remove(childGameObject);
				allChildrenSelected = false;
				break;
			}
			if (allChildrenSelected)
				return true;

			for (; i < childCount; i++)
			{
				var childTransform = transform.GetChild(i);
				var childNode = childTransform.GetComponent<CSGNode>();
				if (!childNode || (childNode is CSGModel))
					continue;

				var childGameObject = childTransform.gameObject;
				objectsInFrustum.Remove(childGameObject);
				DeselectAllChildBrushes(childTransform.transform, objectsInFrustum);
			}
			return false;
		}
		#endregion


		#region GetItemsInFrustum
		public static bool GetItemsInFrustum(CSGPlane[] planes,
											 HashSet<GameObject> objectsInFrustum)
		{
			if (objectsInFrustum == null)
				return false;

			objectsInFrustum.Clear();
			var found = false;
			foreach (var model in InternalCSGModelManager.Models)
			{
				if (!ModelTraits.WillModelRender(model))
					continue;
				found = InternalCSGModelManager.External.GetItemsInFrustum(model, planes, objectsInFrustum) || found;
			}

			var visibleLayers = Tools.visibleLayers;

			var items = objectsInFrustum.ToArray();
			for (var i = items.Length - 1; i >= 0; i--)
			{
				var child = items[i];
				var node = child.GetComponent<CSGNode>();
				if (!node || ((1 << node.gameObject.layer) & visibleLayers) == 0)
					continue;

				if (!objectsInFrustum.Contains(child))
					continue;

				while (true)
				{
					var parent = GetGroupOperationForNode(node);
					if (!parent ||
						!AreAllBrushesSelected(parent.transform, objectsInFrustum))
						break;

					objectsInFrustum.Add(parent.gameObject);
					node = parent;
				}
			}
			return found;
		}
		#endregion

		#region GetPointsInFrustum
		internal static PointSelection[] GetPointsInFrustum(SceneView sceneView,
															CSGPlane[] planes,
														    CSGBrush[] brushes,
															ControlMeshState[] controlMeshStates,
															bool isOrthoCamera)
		{
			bool ignoreHiddenPoints = CSGSettings.HiddenSurfacesNotSelectable && (!isOrthoCamera || !CSGSettings.HiddenSurfacesOrthoSelectable);
			
			var pointSelection = new List<PointSelection>();
			for (var t = 0; t < brushes.Length; t++)
			{
				var targetMeshState = controlMeshStates[t];
				if (targetMeshState == null)
					continue;

				var sceneViewState = targetMeshState.GetSceneViewState(sceneView, false);

				for (var p = 0; p < targetMeshState.WorldPoints.Length; p++)
				{
					if (ignoreHiddenPoints && sceneViewState.WorldPointBackfaced[p])
						continue;
					var point = targetMeshState.WorldPoints[p];
					var found = true;
					for (var i = 0; i < 6; i++)
					{
						if (!(planes[i].Distance(point) > MathConstants.DistanceEpsilon))
							continue;

						found = false;
						break;
					}

					if (found)
					{
						pointSelection.Add(new PointSelection(t, p));
					}
				}
			}
			return pointSelection.ToArray();
		}
		#endregion

		#region DeepSelection (private)

		private static BrushIntersection[] _deepClickIntersections;
		private static Vector2 _prevSceenPos = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		private static SceneView _prevSceneView;
		private static int _deepIndex;
		private static void ResetDeepClick()
		{
			_deepClickIntersections = null;
			_prevSceneView = null;
			_prevSceenPos = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			_deepIndex = 0;
		}

		#endregion

		#region Find..xx..Intersection

		#region FindClickWorldIntersection
		public static bool FindClickWorldIntersection(Vector2 screenPos, out GameObject foundObject)
		{
			var sceneView = SceneView.currentDrawingSceneView;// ? SceneView.currentDrawingSceneView : SceneView.lastActiveSceneView;
			var camera = sceneView ? sceneView.camera : Camera.current;

			foundObject = null;
			if (!camera)
				return false;

			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (camera.farClipPlane - camera.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			CSGModel intersectionModel = null;
			if (_prevSceenPos == screenPos && _prevSceneView == sceneView && _deepClickIntersections != null)
			{
				var prevIntersection = (_deepIndex > 0 && _deepIndex < _deepClickIntersections.Length) ? _deepClickIntersections[_deepIndex] : null;
				if (_deepClickIntersections.Length > 1)
				{
					var visibleLayers = Tools.visibleLayers;
					for (var i = _deepClickIntersections.Length - 1; i >= 0; i--)
					{
						if (((1 << _deepClickIntersections[i].gameObject.layer) & visibleLayers) == 0)
							continue;

						if (_deepClickIntersections[i].brush)
							continue;
						ArrayUtility.RemoveAt(ref _deepClickIntersections, i);
						if (i <= _deepIndex)
							_deepIndex--;
					}
				}

				if (_deepClickIntersections.Length <= 1)
				{
					ResetDeepClick();
				}
				else
				{
					_deepIndex = (_deepIndex + 1) % _deepClickIntersections.Length;
					var currentIntersection = (_deepIndex > 0 && _deepIndex < _deepClickIntersections.Length) ? _deepClickIntersections[_deepIndex] : null;
					if (currentIntersection != prevIntersection &&
						currentIntersection != null)
					{
						foundObject = currentIntersection.gameObject;
						_prevSceenPos = screenPos;
						_prevSceneView = sceneView;
						intersectionModel = currentIntersection.model;
					}
					else
					{
						ResetDeepClick();
					}
				}
			}

			if (_prevSceenPos != screenPos)
			{
				var wireframeShown = CSGSettings.IsWireframeShown(sceneView);
				if (FindMultiWorldIntersection(rayStart, rayEnd, out _deepClickIntersections, growDistance: 0, ignoreInvisibleSurfaces: !wireframeShown, ignoreUnrenderables: !wireframeShown))
				{					
					var visibleLayers = Tools.visibleLayers;
					for (int i = 0; i < _deepClickIntersections.Length; i++)
					{
						if (((1 << _deepClickIntersections[i].gameObject.layer) & visibleLayers) == 0)
							continue;

						_deepIndex = 0;
						var intersection = _deepClickIntersections[i];
						foundObject = intersection.gameObject;
						_prevSceenPos = screenPos;
						_prevSceneView = sceneView;
						intersectionModel = intersection.model;
						break;
					}
				}
				else
					ResetDeepClick();
			}

			GameObject[] modelMeshes = null;
			if (intersectionModel != null)
			{
				modelMeshes = CSGModelManager.GetModelMeshes(intersectionModel);
			}

			if (modelMeshes != null)
			{
				for (var i = 0; i < modelMeshes.Length; i++)
					modelMeshes[i].hideFlags = HideFlags.None;
			}

			var gameObject = HandleUtility.PickGameObject(screenPos, false);

			if (modelMeshes != null)
			{
				for (var i = 0; i < modelMeshes.Length; i++)
				{
					var modelMesh = modelMeshes[i];
					if (!modelMesh)
						continue;

					if (gameObject == modelMesh)
						gameObject = null;

					modelMesh.hideFlags = MeshInstanceManager.ComponentHideFlags;
				}
			}

			if (!gameObject ||
				gameObject.GetComponent<CSGModel>() ||
				gameObject.GetComponent<CSGBrush>() ||
				gameObject.GetComponent<CSGOperation>() ||
				gameObject.GetComponent<GeneratedMeshInstance>() ||
				gameObject.GetComponent<GeneratedMeshes>())
				return (foundObject != null);

			foundObject = gameObject;
			return true;
		}
		#endregion

		#region FindMeshIntersection
		public static BrushIntersection FindMeshIntersection(Vector2 screenPos, CSGBrush[] ignoreBrushes = null, HashSet<Transform> ignoreTransforms = null)
		{
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var hit = HandleUtility.RaySnap(worldRay);
			while (hit != null)
			{
				var rh = (RaycastHit)hit;
				if (ignoreTransforms != null && ignoreTransforms.Contains(rh.transform))
				{
					worldRay.origin = rh.point + (worldRay.direction * 0.00001f);
					hit = HandleUtility.RaySnap(worldRay);
					continue;
				}

				// Check if it's a mesh ...
					if (rh.transform.GetComponent<MeshRenderer>() &&
						// .. but not one we generated
						!rh.transform.GetComponent<CSGNode>() &&
						!rh.transform.GetComponent<GeneratedMeshInstance>())
				{
					return new BrushIntersection
					{
						brushID = -1,
						surfaceIndex = -1,
						texGenIndex = -1,
						worldIntersection = rh.point,
						plane = new CSGPlane(-rh.normal, rh.point)
					};
				}
				break;
			}

			BrushIntersection intersection;
			if (FindWorldIntersection(worldRay, out intersection, ignoreBrushes: ignoreBrushes))
			{
				if (intersection.surfaceInverted)
					intersection.plane.Negate();
				return intersection;
			}

			var gridPlane = RealtimeCSG.Grid.CurrentGridPlane;
			var intersectionPoint = gridPlane.Intersection(worldRay);
			if (float.IsNaN(intersectionPoint.x) ||
				float.IsNaN(intersectionPoint.y) ||
				float.IsNaN(intersectionPoint.z) ||
				float.IsInfinity(intersectionPoint.x) ||
				float.IsInfinity(intersectionPoint.y) ||
				float.IsInfinity(intersectionPoint.z))
			{
				intersectionPoint = worldRay.GetPoint(10);
				return new BrushIntersection
				{
					brushID = -1,
					surfaceIndex = -1,
					texGenIndex = -1,
					worldIntersection = MathConstants.zeroVector3,
					plane = new CSGPlane(gridPlane.normal, intersectionPoint)
				};
			}

			return new BrushIntersection
			{
				brushID = -1,
				surfaceIndex = -1,
				texGenIndex = -1,
				worldIntersection = intersectionPoint,
				plane = gridPlane
			};
		}
		#endregion

		#region FindUnityWorldIntersection
		public static bool FindUnityWorldIntersection(Vector2 screenPos, out GameObject foundObject)
		{
			var sceneView = SceneView.currentDrawingSceneView;// ? SceneView.currentDrawingSceneView : SceneView.lastActiveSceneView;
			var camera = sceneView ? sceneView.camera : Camera.current;

			foundObject = null;
			if (!camera)
				return false;

			var wireframeShown = CSGSettings.IsWireframeShown(sceneView);
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (camera.farClipPlane - camera.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			CSGModel intersectionModel = null;

			BrushIntersection[] intersections;
			if (FindMultiWorldIntersection(rayStart, rayEnd, out intersections, growDistance: 0, ignoreInvisibleSurfaces: !wireframeShown))
			{
				var visibleLayers = Tools.visibleLayers;
				for (int i = 0; i < intersections.Length; i++)
				{
					if (((1 << intersections[i].gameObject.layer) & visibleLayers) == 0)
						continue;
					intersectionModel = intersections[i].model;
					break;
				}
			}

			GameObject[] modelMeshes = null;
			if (intersectionModel != null)
			{
				modelMeshes = CSGModelManager.GetModelMeshes(intersectionModel);
				if (modelMeshes != null)
				{
					for (var i = 0; i < modelMeshes.Length; i++)
						modelMeshes[i].hideFlags = HideFlags.None;
				}
			}

			var gameObject = HandleUtility.PickGameObject(screenPos, false);

			if (modelMeshes != null)
			{
				for (var i = 0; i < modelMeshes.Length; i++)
				{
					var modelMesh = modelMeshes[i];
					if (!modelMesh)
						continue;

					if (gameObject == modelMesh)
						gameObject = null;

					modelMesh.hideFlags = MeshInstanceManager.ComponentHideFlags;
				}
			}

			if (!gameObject ||
				gameObject.GetComponent<Canvas>() ||
				gameObject.GetComponent<CSGModel>() ||
				gameObject.GetComponent<CSGBrush>() ||
				gameObject.GetComponent<CSGOperation>() ||
				gameObject.GetComponent<GeneratedMeshInstance>() ||
				gameObject.GetComponent<GeneratedMeshes>())
				return false;

			foundObject = gameObject;
			return true;
		}
		#endregion

		#region FindWorldIntersection
		public static bool FindWorldIntersection(Vector2 screenPos, out BrushIntersection intersection, float growDistance = 0.0f, bool ignoreInvisible = true, bool ignoreUnrenderables = true, CSGBrush[] ignoreBrushes = null)
		{
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			return FindWorldIntersection(worldRay, out intersection, growDistance, ignoreInvisible, ignoreUnrenderables, ignoreBrushes);
		}

		public static bool FindWorldIntersection(Ray worldRay, out BrushIntersection intersection, float growDistance = 0.0f, bool ignoreInvisible = true, bool ignoreUnrenderables = true, CSGBrush[] ignoreBrushes = null)
		{
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (Camera.current.farClipPlane - Camera.current.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			return FindWorldIntersection(rayStart, rayEnd, out intersection, growDistance, ignoreInvisible, ignoreUnrenderables, ignoreBrushes);
		}

		public static bool FindWorldIntersection(Vector3 rayStart, Vector3 rayEnd, out BrushIntersection intersection, float growDistance = 0.0f, bool ignoreInvisible = true, bool ignoreUnrenderables = true, CSGBrush[] ignoreBrushes = null)
		{
			BrushIntersection[] intersections;
			if (!FindMultiWorldIntersection(rayStart, rayEnd, out intersections, growDistance, ignoreInvisible, ignoreUnrenderables, ignoreBrushes) ||
				intersections == null ||
				intersections.Length == 0)
			{
				intersection = null;
				return false;
			}
			
			var visibleLayers = Tools.visibleLayers;
			for (int i = 0; i < intersections.Length; i++)
			{
				if (((1 << intersections[i].gameObject.layer) & visibleLayers) == 0)
					continue;

				intersection = intersections[i];
				return true;
			}
			
			intersection = null;
			return false;
		}
		#endregion

		#region FindMultiWorldIntersection
		/*
		public static bool FindMultiWorldIntersection(Vector2 screenPos, out BrushIntersection[] intersections, float growDistance = 0.0f, bool ignoreInvisible = true, bool ignoreUnrenderables = true)
		{
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (Camera.current.farClipPlane - Camera.current.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			return FindMultiWorldIntersection(rayStart, rayEnd, out intersections, growDistance, ignoreInvisible, ignoreUnrenderables);
		}
		*/
		public static bool FindMultiWorldIntersection(Vector3 rayStart, Vector3 rayEnd, out BrushIntersection[] intersections, float growDistance = 0.0f, bool ignoreInvisibleSurfaces = true, bool ignoreUnrenderables = true, CSGBrush[] ignoreBrushes = null)
		{
			intersections = null;
			if (InternalCSGModelManager.External == null ||
				InternalCSGModelManager.External.RayCastIntoModelMulti == null)
				return false;

			var foundIntersections = new Dictionary<CSGNode, BrushIntersection>();
			
			var visibleLayers = Tools.visibleLayers;
			ignoreInvisibleSurfaces = ignoreInvisibleSurfaces && !CSGSettings.ShowCulledSurfaces;
			for (var g = 0; g < InternalCSGModelManager.Models.Length; g++)
			{
				var model = InternalCSGModelManager.Models[g];
				if (!model ||
					!model.isActiveAndEnabled)
				{
					continue;
				}
				if (((1 << model.gameObject.layer) & visibleLayers) == 0)
					continue;
					

				if (ignoreUnrenderables && !ModelTraits.WillModelRender(model) &&
					!Selection.Contains(model.gameObject.GetInstanceID()))
				{
					continue;
				}

				BrushIntersection[] modelIntersections;
				var modelTranslation = model.transform.position;
				if (!InternalCSGModelManager.External.RayCastIntoModelMulti(model,
																			rayStart - modelTranslation,
																			rayEnd - modelTranslation,
																			ignoreInvisibleSurfaces,
																			growDistance,
																			out modelIntersections,
																			ignoreBrushes: ignoreBrushes))
					continue;
				
				for (var i = 0; i < modelIntersections.Length; i++)
				{
					var intersection = modelIntersections[i];
			
					if (((1 << intersection.gameObject.layer) & visibleLayers) == 0)
						continue;
					

					CSGBrush brush = null;
					for (var b = 0; b < InternalCSGModelManager.Brushes.Length; b++)
					{
						if (InternalCSGModelManager.Brushes[b].brushID != intersection.brushID)
							continue;

						brush = InternalCSGModelManager.Brushes[b];
						break;
					}

					if (BrushTraits.IsSurfaceSelectable(brush, intersection.surfaceIndex))
						continue;

					intersection.brush = brush;
					intersection.model = model;
					intersection.worldIntersection += modelTranslation;
					intersection.plane.Translate(modelTranslation);
					
					var currentNode = GetTopMostGroupForNode(intersection.brush);

					BrushIntersection other;
					if (foundIntersections.TryGetValue(currentNode, out other) &&
						other.distance <= intersection.distance)
						continue;

					foundIntersections[currentNode] = modelIntersections[i];
				}
			}

			if (foundIntersections.Count == 0)
				return false;

			var sortedIntersections = foundIntersections.Values.ToArray();
			Array.Sort(sortedIntersections, (x, y) => (x.distance < y.distance) ? -1 : 0);
			
			intersections = sortedIntersections;
			return true;
		}
		#endregion

		#region FindBrushIntersection
		public static bool FindBrushIntersection(CSGBrush brush, Vector3 modelTranslation, Vector2 screenPos, out BrushIntersection intersection)
		{
			var guiPointToWorldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = guiPointToWorldRay.origin;
			var rayVector = (guiPointToWorldRay.direction * (Camera.current.farClipPlane - Camera.current.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			return FindBrushIntersection(brush, modelTranslation, rayStart, rayEnd, out intersection);
		}

		public static bool FindBrushIntersection(CSGBrush brush, Vector3 modelTranslation, Vector3 rayStart, Vector3 rayEnd, out BrushIntersection intersection, bool forceUseInvisible = false, float growDistance = 0.0f)
		{
			intersection = null;
			if (!brush || InternalCSGModelManager.External.RayCastIntoBrush == null)
				return false;

			var ignoreInvisible = !forceUseInvisible && !CSGSettings.ShowCulledSurfaces;
			if (!InternalCSGModelManager.External.RayCastIntoBrush(brush.brushID, -1,
																   rayStart - modelTranslation,
																   rayEnd - modelTranslation,
																   ignoreInvisible,
																   growDistance,
																   out intersection))
				return false;

			if (BrushTraits.IsSurfaceSelectable(brush, intersection.surfaceIndex))
				return false;

			intersection.worldIntersection += modelTranslation;
			intersection.plane.Translate(modelTranslation);
			return true;
		}

		public static bool FindBrushIntersection(CSGBrush brush, Vector3 modelTranslation, int texGenIndex, Vector2 screenPos, out BrushIntersection intersection)
		{
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (Camera.current.farClipPlane - Camera.current.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			return FindBrushIntersection(brush, modelTranslation, texGenIndex, rayStart, rayEnd, out intersection);
		}

		public static bool FindBrushIntersection(CSGBrush brush, Vector3 modelTranslation, int texGenIndex, Vector3 rayStart, Vector3 rayEnd, out BrushIntersection intersection, float growDistance = 0.0f)
		{
			intersection = null;

			if (!brush || InternalCSGModelManager.External.RayCastIntoBrush == null)
				return false;

			var ignoreInvisible = !CSGSettings.ShowCulledSurfaces;
			if (!InternalCSGModelManager.External.RayCastIntoBrush(brush.brushID,
																   texGenIndex,
																   rayStart - modelTranslation,
																   rayEnd - modelTranslation,
																   ignoreInvisible,
																   growDistance,
																   out intersection))
				return false;

			if (BrushTraits.IsSurfaceSelectable(brush, intersection.surfaceIndex))
				return false;

			intersection.worldIntersection += modelTranslation;
			intersection.plane.Translate(modelTranslation);
			return true;
		}
		#endregion

		#region FindSurfaceIntersection
		public static bool FindSurfaceIntersection(CSGBrush brush, Vector3 modelTranslation, Int32 surfaceIndex, Vector2 screenPos, out BrushIntersection intersection)
		{
			var worldRay = HandleUtility.GUIPointToWorldRay(screenPos);
			var rayStart = worldRay.origin;
			var rayVector = (worldRay.direction * (Camera.current.farClipPlane - Camera.current.nearClipPlane));
			var rayEnd = rayStart + rayVector;

			return FindSurfaceIntersection(brush, modelTranslation, surfaceIndex, rayStart, rayEnd, out intersection);
		}

		public static bool FindSurfaceIntersection(CSGBrush brush, Vector3 modelTranslation, Int32 surfaceIndex, Vector3 rayStart, Vector3 rayEnd, out BrushIntersection intersection, float growDistance = 0.0f)
		{
			intersection = null;
			if (!brush ||
				InternalCSGModelManager.External.RayCastIntoBrush == null)
				return false;

			var ignoreInvisible = !CSGSettings.ShowCulledSurfaces;
			if (!InternalCSGModelManager.External.RayCastIntoBrushSurface(brush.brushID,
																		  surfaceIndex,
																		  rayStart - modelTranslation,
																		  rayEnd - modelTranslation,
																		  ignoreInvisible,
																		  growDistance,
																		  out intersection))
			{
				return false;
			}

			if (BrushTraits.IsSurfaceSelectable(brush, intersection.surfaceIndex))
			{
				return false;
			}

			intersection.worldIntersection += modelTranslation;
			intersection.plane.Translate(modelTranslation);
			return true;
		}
		#endregion

		#endregion
	
	}
}