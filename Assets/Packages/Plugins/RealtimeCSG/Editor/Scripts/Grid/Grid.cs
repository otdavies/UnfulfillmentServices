using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	internal enum GridAxis
	{
		AxisXZ = 0,
		AxisYZ = 1,
		AxisXY = 2
	}

	internal enum GridMode
	{
		Regular,
		Ortho,
		WorkPlane
	}
	internal static class Grid
	{
		static Material gridMaterial_ = null;
		static int gridSizeID = -1;
		static int gridSpacingID = -1;
		static int gridColorID = -1;
		static int gridCenterColorID = -1;
		static Material GridMaterial
		{
			get
			{
				if (!gridMaterial_)
				{
					var shader = Shader.Find("Hidden/CSG/internal/Grid");
					if (shader == null)
						return null;
					
					gridSizeID			= Shader.PropertyToID("_GridSize");		// _GridSize("Grid Size", Float) = 1
					gridSpacingID		= Shader.PropertyToID("_GridSpacing");	// _GridSpacing("Grid Spacing", Float) = (1.0, 1.0, 1.0)
					gridColorID			= Shader.PropertyToID("_GridColor");	// _GridColor("Grid Color", Color) = (1.0, 1.0, 1.0, 0.5)
					gridCenterColorID	= Shader.PropertyToID("_CenterColor");	// _CenterColor("Center Color", Color) = (1.0, 1.0, 1.0, 0.75)

					gridMaterial_ = new Material(shader); 
					gridMaterial_.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
					gridMaterial_.SetInt("_ZWrite", 0);
					gridMaterial_.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
					gridMaterial_.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
				}
				return gridMaterial_;
			}
		}

		static Mesh gridMesh_;
		static Mesh GridMesh
		{
			get
			{
				if (!gridMesh_)
				{
					gridMesh_ = new Mesh()
					{
						name = "Plane",
						vertices = new[]
						{
							new Vector3(-1, -1, 0),
							new Vector3( 1, -1, 0),
							new Vector3( 1,  1, 0),
							new Vector3(-1,  1, 0)
						},
						triangles = new[]
						{
							0, 1, 2,
							0, 2, 3
						},
						hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset
					};
				}
				return gridMesh_;
			}
		}
		
		static void DrawGrid(Camera camera, Vector3 cameraPosition, Vector3 gridCenter, Quaternion gridRotation, GridAxis axis, float alpha, GridMode gridMode)
		{
			if (alpha <= 0.03f)
				return;
			
			// find the nearest point on the plane
			var	normal	= gridRotation * MathConstants.upVector3;
			var d		= Vector3.Dot(normal, gridCenter);
			var t		= (Vector3.Dot(normal, cameraPosition) - d) / Vector3.Dot(normal, normal);
			
			var	snap_x				= RealtimeCSG.CSGSettings.SnapVector.x;
			var	snap_y				= RealtimeCSG.CSGSettings.SnapVector.y;
			var	snap_z				= RealtimeCSG.CSGSettings.SnapVector.z;
			
			// calculate a point on the camera at the same distance as the camera is to the grid, in world-space
			var	forward				= camera.cameraToWorldMatrix.MultiplyVector(MathConstants.forwardVector3);
			var	projectedCenter		= cameraPosition - (t * forward);

			// calculate the snap sizes relatively to that point in the center (this makes them relative to camera position, but not rotation)
			var sideways			= camera.cameraToWorldMatrix.MultiplyVector(MathConstants.leftVector3);
			var screenPointCenter	= camera.WorldToScreenPoint(projectedCenter);
			var screenPointAxisX	= camera.WorldToScreenPoint(projectedCenter + (sideways * snap_x));
			var screenPointAxisY	= camera.WorldToScreenPoint(projectedCenter + (sideways * snap_y));
			var screenPointAxisZ	= camera.WorldToScreenPoint(projectedCenter + (sideways * snap_z));
			var pixelSizeX			= (screenPointAxisX - screenPointCenter).magnitude; // size in pixels
			var pixelSizeY			= (screenPointAxisY - screenPointCenter).magnitude; // size in pixels
			var pixelSizeZ			= (screenPointAxisZ - screenPointCenter).magnitude; // size in pixels

			float screenPixelSize;
			switch (axis)
			{
				default:
				case GridAxis.AxisXZ: { screenPixelSize = Mathf.Min(pixelSizeX, pixelSizeZ); break; } //X/Z
				case GridAxis.AxisYZ: { screenPixelSize = Mathf.Min(pixelSizeY, pixelSizeZ); break; } //Y/Z
				case GridAxis.AxisXY: { screenPixelSize = Mathf.Min(pixelSizeX, pixelSizeY); break; } //X/Y
			}
			
			const float minPixelSize = 64.0f;
			
			float	gridLevelPow	= (minPixelSize / screenPixelSize);

			var gridMaterial = GridMaterial;
			var gridMesh = GridMesh;

			if (!gridMaterial || !gridMesh)
				return; 
			
			gridMaterial.SetFloat(gridSizeID			, Mathf.Max(0, gridLevelPow));
			gridMaterial.SetVector(gridSpacingID		, RealtimeCSG.CSGSettings.SnapVector);

			var gridColor = ColorSettings.gridColorW;
			gridColor.a *= alpha;
			gridMaterial.SetColor(gridCenterColorID, gridColor);
			
			gridColor = ColorSettings.gridColor1W;			
			gridColor.a *= alpha;
			gridMaterial.SetColor(gridColorID, gridColor);

			SceneView.lastActiveSceneView.camera.depthTextureMode = DepthTextureMode.Depth;
			
			gridMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			gridMaterial.SetInt("_ZWrite", 0);
			gridMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
			
			gridMaterial.SetPass(0); 
			var gridMatrix = Matrix4x4.TRS(gridCenter, gridRotation, Vector3.one);
			Graphics.DrawMeshNow(gridMesh, gridMatrix, 0);
		}

		public static bool          ForceGrid           = false;
		static Vector3				forcedGridCenter	= MathConstants.zeroVector3;
		public static Vector3		ForcedGridCenter
		{
			get
			{
				return forcedGridCenter;
			}
			set
			{
				if (forcedGridCenter == value)
					return;
				forcedGridCenter = value;
			}
		}
				 
		static Quaternion			forcedGridRotation	= MathConstants.identityQuaternion;
		public static Quaternion	ForcedGridRotation
		{
			get
			{
				return forcedGridRotation;
			}
			set
			{
				if (forcedGridRotation == value)
					return;
				forcedGridRotation = value;
			}
		}

		public static CSGPlane		CurrentGridPlane		{ get { UpdateGridOrientation(); return gridOrientation.gridPlane; } }
		public static CSGPlane		CurrentWorkGridPlane	{ get { UpdateGridOrientation(); return gridOrientation.gridWorkPlane; } }
		public static Vector3		CurrentGridSnapVector	{ get { UpdateGridOrientation(); return gridOrientation.gridSnapVector; } }
		
		internal sealed class GridOrientation
		{
			public Camera       gridCamera;
			public Vector3      gridCameraPosition;
			public Vector3      gridCameraSnapped;

			public Vector3		gridCenter				= MathConstants.zeroVector3;
			public Quaternion	gridRotation			= MathConstants.identityQuaternion;
			public CSGPlane		gridPlane;
			
			public Quaternion	gridOrthoXRotation		= MathConstants.identityQuaternion;
			public Quaternion	gridOrthoYRotation		= MathConstants.identityQuaternion;
			public Quaternion	gridOrthoZRotation		= MathConstants.identityQuaternion;

			public Vector3		gridWorkCenter			= MathConstants.zeroVector3;
			public Quaternion	gridWorkRotation		= MathConstants.identityQuaternion;
			public Quaternion	gridWorkInvRotation		= MathConstants.identityQuaternion;
			public CSGPlane		gridWorkPlane;

			public Vector3	    gridSnapVector;
			public Vector3	    gridSnapScale;
			
			public bool			gridOrtho				= false;
			public bool			gridOrthoXVisible		= false;
			public bool			gridOrthoYVisible		= false;
			public bool			gridOrthoZVisible		= false;
			public float		gridOrthoXAlpha			= 0.0f;
			public float		gridOrthoYAlpha			= 0.0f;
			public float		gridOrthoZAlpha			= 0.0f;
		}

		internal static GridOrientation gridOrientation = null;

		static void OnRender()
		{
			UpdateGridOrientation();
						
			if (gridOrientation.gridOrtho)
			{
				if (gridOrientation.gridOrthoXVisible)
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition,
							 gridOrientation.gridCenter, gridOrientation.gridOrthoXRotation,
							 GridAxis.AxisYZ, gridOrientation.gridOrthoXAlpha, GridMode.Ortho);
				
				if (gridOrientation.gridOrthoYVisible)
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition, 
							 gridOrientation.gridCenter, gridOrientation.gridOrthoYRotation, 
							 GridAxis.AxisXZ, gridOrientation.gridOrthoYAlpha, GridMode.Ortho);

				if (gridOrientation.gridOrthoZVisible)
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition, 
							 gridOrientation.gridCenter, gridOrientation.gridOrthoZRotation, 
							 GridAxis.AxisXY, gridOrientation.gridOrthoZAlpha, GridMode.Ortho);
			} else
			{//*
				Vector3 forward			= gridOrientation.gridRotation      * MathConstants.forwardVector3;
				Vector3 work_forward	= gridOrientation.gridWorkRotation * MathConstants.forwardVector3;
				if (ForceGrid &&
					!((forward - work_forward).sqrMagnitude < 0.001f ||
					  (forward + work_forward).sqrMagnitude < 0.001f
					  // && gridOrientation.grid_work_center   == gridOrientation.grid_center
						))
				{
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition,
							 gridOrientation.gridWorkCenter, gridOrientation.gridWorkRotation,
							 GridAxis.AxisXZ, 0.75f, GridMode.WorkPlane);
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition,
							 gridOrientation.gridCenter, gridOrientation.gridRotation,
							 GridAxis.AxisXZ, 0.125f, GridMode.Regular);
				} else//*/
					DrawGrid(gridOrientation.gridCamera, gridOrientation.gridCameraPosition,
							 gridOrientation.gridCenter, gridOrientation.gridRotation,
							 GridAxis.AxisXZ, 1.0f, GridMode.Regular);
			}	
		}

		public static void RenderGrid()
		{
			if (Event.current.type != EventType.Repaint)
				return;
			
			OnRender();
		}

		public static void UpdateGridOrientation()
		{
			var sceneView		= SceneView.currentDrawingSceneView;
			var camera			= sceneView != null ? sceneView.camera : Camera.current;
			if (camera == null)
				return;

			var camera_position = camera.cameraToWorldMatrix.MultiplyPoint(MathConstants.zeroVector3);
			var camera_forward	= camera.cameraToWorldMatrix.MultiplyVector(MathConstants.forwardVector3);
			
			gridOrientation = new GridOrientation();
			gridOrientation.gridCamera				= camera;
			gridOrientation.gridCameraPosition	= camera_position;

			if (Tools.pivotRotation == PivotRotation.Local)
			{
				var activeTransform = Selection.activeTransform;
				if (activeTransform != null)
				{
					var parentCenter	= MathConstants.zeroVector3;
					var parent			= activeTransform.parent;
					if (parent != null)
					{
						parentCenter	= parent.position;
					}

					gridOrientation.gridRotation	= Tools.handleRotation;					
					gridOrientation.gridCenter		= parentCenter;
				}
			}
			
			gridOrientation.gridOrthoXVisible	= false;
			gridOrientation.gridOrthoYVisible	= false;
			gridOrientation.gridOrthoZVisible	= false;
			gridOrientation.gridOrtho				= false;
			gridOrientation.gridOrthoXAlpha		= 0.0f;
			gridOrientation.gridOrthoYAlpha		= 0.0f;
			gridOrientation.gridOrthoZAlpha		= 0.0f;
			
			gridOrientation.gridWorkCenter		= gridOrientation.gridCenter;
			gridOrientation.gridWorkRotation	= gridOrientation.gridRotation;

			if (camera.orthographic)
			{
				gridOrientation.gridOrtho			= true;
				
				Vector3 dots = new Vector3(
						Mathf.Clamp01(Mathf.Abs(Vector3.Dot(camera_forward, gridOrientation.gridRotation * MathConstants.rightVector3  )) - 0.6f),
						Mathf.Clamp01(Mathf.Abs(Vector3.Dot(camera_forward, gridOrientation.gridRotation * MathConstants.upVector3     )) - 0.3f),
						Mathf.Clamp01(Mathf.Abs(Vector3.Dot(camera_forward, gridOrientation.gridRotation * MathConstants.forwardVector3)) - 0.6f)
					).normalized;

				dots.x *= dots.x;
				dots.y *= dots.y;
				dots.z *= dots.z;
								
				if (dots.x > 0.5f)
				{
					Quaternion rotation = Quaternion.AngleAxis(90.0f, MathConstants.forwardVector3);
					gridOrientation.gridOrthoXRotation	= gridOrientation.gridRotation * rotation;
					gridOrientation.gridOrthoXVisible	= true;
					gridOrientation.gridOrthoXAlpha		= dots.x;
				}
				
				if (dots.y > 0.5f)
				{
					gridOrientation.gridOrthoYRotation	= gridOrientation.gridRotation;
					gridOrientation.gridOrthoYVisible	= true;
					gridOrientation.gridOrthoYAlpha		= dots.y;
				}

				if (dots.z > 0.5f)
				{
					Quaternion rotation = Quaternion.AngleAxis(90.0f, MathConstants.leftVector3);
					gridOrientation.gridOrthoZRotation	= gridOrientation.gridRotation * rotation;
					gridOrientation.gridOrthoZVisible	= true;
					gridOrientation.gridOrthoZAlpha		= dots.z;
				}
				
				if (dots.y > dots.z)
				{
					if (dots.y > dots.x)	gridOrientation.gridWorkRotation = gridOrientation.gridOrthoYRotation;
					else					gridOrientation.gridWorkRotation = gridOrientation.gridOrthoXRotation;
				} else
				{
					if (dots.z > dots.x)	gridOrientation.gridWorkRotation = gridOrientation.gridOrthoZRotation;
					else					gridOrientation.gridWorkRotation = gridOrientation.gridOrthoXRotation;
				}
				gridOrientation.gridPlane = new CSGPlane(gridOrientation.gridWorkRotation, gridOrientation.gridWorkCenter);
			} else
			{
				gridOrientation.gridPlane = new CSGPlane(gridOrientation.gridWorkRotation, gridOrientation.gridWorkCenter); 
				if (ForceGrid)
				{
					gridOrientation.gridWorkCenter		= ForcedGridCenter;
					gridOrientation.gridWorkRotation	= ForcedGridRotation;
				}
			}

			gridOrientation.gridWorkInvRotation = Quaternion.Inverse(gridOrientation.gridWorkRotation);
			
			// find point on the plane that is nearest to camera
			var	normal		= gridOrientation.gridWorkRotation * MathConstants.upVector3;
			var d			= Vector3.Dot(normal, gridOrientation.gridCenter);
			var position	= (new CSGPlane(normal, d)).Project(gridOrientation.gridCameraPosition);
			gridOrientation.gridCameraSnapped = position;

			gridOrientation.gridWorkPlane = new CSGPlane(normal, position);



			var euler	= gridOrientation.gridWorkInvRotation.eulerAngles;
			euler.x = Mathf.Round(euler.x / 90) * 90;
			euler.y = Mathf.Round(euler.y / 90) * 90;
			euler.z = Mathf.Round(euler.z / 90) * 90;
			
			gridOrientation.gridSnapVector = Quaternion.Euler(euler) * RealtimeCSG.CSGSettings.SnapVector;
			var snap_scale  = Quaternion.Euler(euler) * 
				new Vector3(RealtimeCSG.CSGSettings.LockAxisX ? 0 : 1,
							RealtimeCSG.CSGSettings.LockAxisY ? 0 : 1,
							RealtimeCSG.CSGSettings.LockAxisZ ? 0 : 1);

			snap_scale.x = Mathf.Abs(snap_scale.x);
			snap_scale.y = Mathf.Abs(snap_scale.y);
			snap_scale.z = Mathf.Abs(snap_scale.z);

			gridOrientation.gridSnapScale = snap_scale;
		}
		
		public static Matrix4x4	ToGridSpaceMatrix	()	{ return Matrix4x4.TRS(MathConstants.zeroVector3, gridOrientation.gridWorkInvRotation, MathConstants.oneVector3); }
		public static Matrix4x4	FromGridSpaceMatrix	()	{ return Matrix4x4.TRS(MathConstants.zeroVector3, gridOrientation.gridWorkRotation,     MathConstants.oneVector3); }
		
		public static Quaternion ToGridSpaceQuaternion		()	{ return gridOrientation.gridWorkInvRotation; }
		public static Quaternion FromGridSpaceQuaternion	()	{ return gridOrientation.gridWorkRotation; }
		
		public static Vector3	PointToGridSpace	(Vector3 pos)	{ return gridOrientation.gridWorkInvRotation * (pos - gridOrientation.gridWorkCenter); }
		public static Vector3	VectorToGridSpace	(Vector3 pos)	{ return gridOrientation.gridWorkInvRotation * pos; }
		public static CSGPlane	PlaneToGridSpace	(CSGPlane p)	{ return new CSGPlane(VectorToGridSpace(p.normal), PointToGridSpace(p.pointOnPlane)); }
		
		public static Vector3	PointFromGridSpace	(Vector3 pos)	{ return (gridOrientation.gridWorkRotation * pos) + gridOrientation.gridWorkCenter; }		
		public static Vector3	VectorFromGridSpace	(Vector3 pos)	{ return (gridOrientation.gridWorkRotation * pos); }
		public static CSGPlane	PlaneFromGridSpace	(CSGPlane p)	{ return new CSGPlane(VectorFromGridSpace(p.normal), PointFromGridSpace(p.pointOnPlane)); }

		static Vector3 SnapRoundPosition(Vector3 currentPosition, Vector3 snapVector)
		{
			currentPosition.x = Mathf.RoundToInt(currentPosition.x / snapVector.x) * snapVector.x;
			currentPosition.y = Mathf.RoundToInt(currentPosition.y / snapVector.y) * snapVector.y;
			currentPosition.z = Mathf.RoundToInt(currentPosition.z / snapVector.z) * snapVector.z;
			return currentPosition;
		}

		static Vector3 SnapFloorPosition(Vector3 currentPosition, Vector3 snapVector)
		{
			currentPosition.x = Mathf.FloorToInt(currentPosition.x / snapVector.x) * snapVector.x;
			currentPosition.y = Mathf.FloorToInt(currentPosition.y / snapVector.y) * snapVector.y;
			currentPosition.z = Mathf.FloorToInt(currentPosition.z / snapVector.z) * snapVector.z;
			return currentPosition;
		}

		static Vector3 SnapCeilPosition(Vector3 currentPosition, Vector3 snapVector)
		{
			currentPosition.x = Mathf.FloorToInt(currentPosition.x / snapVector.x + 1) * snapVector.x;
			currentPosition.y = Mathf.FloorToInt(currentPosition.y / snapVector.y + 1) * snapVector.y;
			currentPosition.z = Mathf.FloorToInt(currentPosition.z / snapVector.z + 1) * snapVector.z;
			return currentPosition;
		}

		public static List<Vector3> FindAllGridEdgesThatTouchPoint(Vector3 point)
		{
			var lines = new List<Vector3>();
			
			UpdateGridOrientation();
			if (gridOrientation == null)
				return lines;

			var snapVector			= gridOrientation.gridSnapVector;

			var gridPoint			= PointToGridSpace(point);
			var snappedGridPoint	= SnapRoundPosition(gridPoint, snapVector);

			var gridPlane			= new CSGPlane(Grid.CurrentWorkGridPlane.normal, point);

			if (Math.Abs(gridPoint.x - snappedGridPoint.x) < MathConstants.EqualityEpsilon)
			{
				var lineSize = 10000;
				var pointA = new Vector3(-lineSize, gridPoint.y, gridPoint.z);
				var pointB = new Vector3( lineSize, gridPoint.y, gridPoint.z);
				lines.Add(gridPlane.Project(PointFromGridSpace(pointA)));
				lines.Add(gridPlane.Project(PointFromGridSpace(pointB)));
			}

			if (Math.Abs(gridPoint.y - snappedGridPoint.y) < MathConstants.EqualityEpsilon)
			{
				var lineSize = 10000;
				var pointA = new Vector3(gridPoint.x, -lineSize, gridPoint.z);
				var pointB = new Vector3(gridPoint.x,  lineSize, gridPoint.z);
				lines.Add(gridPlane.Project(PointFromGridSpace(pointA)));
				lines.Add(gridPlane.Project(PointFromGridSpace(pointB)));
			}

			if (Math.Abs(gridPoint.z - snappedGridPoint.z) < MathConstants.EqualityEpsilon)
			{
				var lineSize = 10000;
				var pointA = new Vector3(gridPoint.x, gridPoint.y, -lineSize);
				var pointB = new Vector3(gridPoint.x, gridPoint.y,  lineSize);
				lines.Add(gridPlane.Project(PointFromGridSpace(pointA)));
				lines.Add(gridPlane.Project(PointFromGridSpace(pointB)));
			}
			return lines;
		}
		
		public static Vector3 ForceSnapToGrid(Vector3 worldPoint)
		{
			return worldPoint + SnapDeltaToGrid(MathConstants.zeroVector3, new Vector3[] { worldPoint });
		}

		public static Vector3 ForceSnapDeltaToGrid(Vector3 worldDeltaMovement, Vector3 worldPoint)
		{
			return SnapDeltaToGrid(worldDeltaMovement, new Vector3[] { worldPoint });
		}

		public static Vector3 HandleLockedAxi(Vector3 worldDeltaMovement)
		{
			if (gridOrientation == null)
				return worldDeltaMovement;
			var snapScale				= gridOrientation.gridSnapScale;
			var gridLocalDeltaMovement	= VectorToGridSpace(worldDeltaMovement);
			gridLocalDeltaMovement.x *= snapScale.x;
			gridLocalDeltaMovement.y *= snapScale.y;
			gridLocalDeltaMovement.z *= snapScale.z;
			return VectorFromGridSpace(gridLocalDeltaMovement);
		}

		public static Vector3 SnapDeltaToGrid(Vector3 worldDeltaMovement, Vector3[] worldPoints, bool snapToGridPlane = true, bool snapToSelf = false)
		{
			UpdateGridOrientation();
			if (gridOrientation == null || worldPoints == null || worldPoints.Length == 0)
				return worldDeltaMovement;
			
			var worldPlane	= gridOrientation.gridWorkPlane;
			var scaleVector = gridOrientation.gridSnapScale;
			var snapVector	= gridOrientation.gridSnapVector;
			
			var gridLocalDeltaMovement	= VectorToGridSpace(worldDeltaMovement);
			var gridLocalPlane			= PlaneToGridSpace(worldPlane);

			if (snapToGridPlane)
			{
				scaleVector.x *= (Mathf.Abs(gridLocalPlane.a) >= 1 - MathConstants.EqualityEpsilon) ? 0 : 1;
				scaleVector.y *= (Mathf.Abs(gridLocalPlane.b) >= 1 - MathConstants.EqualityEpsilon) ? 0 : 1;
				scaleVector.z *= (Mathf.Abs(gridLocalPlane.c) >= 1 - MathConstants.EqualityEpsilon) ? 0 : 1;
			}
			var snappedDeltaMovement	= gridLocalDeltaMovement;

			if (Mathf.Abs(scaleVector.x) < MathConstants.EqualityEpsilon || Mathf.Abs(worldDeltaMovement.x) < MathConstants.EqualityEpsilon) snappedDeltaMovement.x = 0;
			if (Mathf.Abs(scaleVector.y) < MathConstants.EqualityEpsilon || Mathf.Abs(worldDeltaMovement.y) < MathConstants.EqualityEpsilon) snappedDeltaMovement.y = 0;
			if (Mathf.Abs(scaleVector.z) < MathConstants.EqualityEpsilon || Mathf.Abs(worldDeltaMovement.z) < MathConstants.EqualityEpsilon) snappedDeltaMovement.z = 0;
			
			Vector3[] gridLocalPoints;
			if (worldPoints.Length > 1)
			{ 
				var bounds = new AABB();
				bounds.Reset();
				for (int i = 0; i < worldPoints.Length; i++)
				{
					Vector3 localPoint = PointToGridSpace(worldPoints[i]);
					if (snapToGridPlane)
						localPoint = GeometryUtility.ProjectPointOnPlane(gridLocalPlane, localPoint);
					if (float.IsNaN(localPoint.x) || float.IsNaN(localPoint.y) || float.IsNaN(localPoint.z) ||
						float.IsInfinity(localPoint.x) || float.IsInfinity(localPoint.y) || float.IsInfinity(localPoint.z))
						continue;
					bounds.Extend(localPoint);
				}
				gridLocalPoints = bounds.GetCorners();
			} else
			{
				var localGridSpacePoint = PointToGridSpace(worldPoints[0]);
				Vector3 projectedPoint = localGridSpacePoint;
				if (snapToGridPlane)
					projectedPoint		= GeometryUtility.ProjectPointOnPlane(gridLocalPlane, localGridSpacePoint);

				if (float.IsNaN(projectedPoint.x) || float.IsNaN(projectedPoint.y) || float.IsNaN(projectedPoint.z) ||
					float.IsInfinity(projectedPoint.x) || float.IsInfinity(projectedPoint.y) || float.IsInfinity(projectedPoint.z))
					gridLocalPoints = new Vector3[0] {  };
				else
					gridLocalPoints = new Vector3[] { projectedPoint };
			}
			
			for (int i = 0; i < gridLocalPoints.Length; i++)
			{
				var oldPoint = gridLocalPoints[i];
				var newPoint = gridLocalPoints[i] + gridLocalDeltaMovement;
				if (snapToGridPlane)
					newPoint = GeometryUtility.ProjectPointOnPlane(gridLocalPlane, newPoint);				
				newPoint = GridUtility.CleanPosition(newPoint);
				
				var snappedNewPoint = SnapRoundPosition(newPoint, snapVector);
				
				if (snapToGridPlane)
					snappedNewPoint = GeometryUtility.ProjectPointOnPlane(gridLocalPlane, snappedNewPoint);
				snappedNewPoint = GridUtility.CleanPosition(snappedNewPoint);
						
				var foundDeltaMovement = (snappedNewPoint - oldPoint);
				
				foundDeltaMovement.x *= scaleVector.x;
				foundDeltaMovement.y *= scaleVector.y;
				foundDeltaMovement.z *= scaleVector.z;

				if (i == 0 || Math.Abs(foundDeltaMovement.x) < Mathf.Abs(snappedDeltaMovement.x)) snappedDeltaMovement.x = foundDeltaMovement.x;
				if (i == 0 || Math.Abs(foundDeltaMovement.y) < Mathf.Abs(snappedDeltaMovement.y)) snappedDeltaMovement.y = foundDeltaMovement.y;
				if (i == 0 || Math.Abs(foundDeltaMovement.z) < Mathf.Abs(snappedDeltaMovement.z)) snappedDeltaMovement.z = foundDeltaMovement.z;
			}

			if (snapToSelf)
			{ 
				var snapDelta = (snappedDeltaMovement - gridLocalDeltaMovement);
				if (Mathf.Abs(snapDelta.x) > Mathf.Abs(gridLocalDeltaMovement.x)) snappedDeltaMovement.x = 0;
				if (Mathf.Abs(snapDelta.y) > Mathf.Abs(gridLocalDeltaMovement.y)) snappedDeltaMovement.y = 0;
				if (Mathf.Abs(snapDelta.z) > Mathf.Abs(gridLocalDeltaMovement.z)) snappedDeltaMovement.z = 0;
			}
			
			worldDeltaMovement = VectorFromGridSpace(snappedDeltaMovement);
			//Debug.Log(worldDeltaMovement + " " + snappedDeltaMovement);
			return worldDeltaMovement;
		}

		public static Vector3 ForceSnapToRay(Ray worldRay, Vector3 worldPoint)
		{
			return worldPoint + SnapDeltaToRay(worldRay, MathConstants.zeroVector3, new Vector3[] { worldPoint });
		}
		
		public static Vector3 ForceSnapDeltaToRay(Ray worldRay, Vector3 worldDeltaMovement, Vector3 worldPoint)
		{
			return SnapDeltaToRay(worldRay, worldDeltaMovement, new Vector3[] { worldPoint });
		}
		
		public static Vector3 SnapDeltaToRay(Ray worldRay, Vector3 worldDeltaMovement, Vector3[] worldPoints, bool snapToSelf = false)
		{
			UpdateGridOrientation();
			if (gridOrientation == null || worldPoints == null || worldPoints.Length == 0)
				return worldDeltaMovement;
			
			var snapVector	= gridOrientation.gridSnapVector;
			var scaleVector = gridOrientation.gridSnapScale;
			
			var localDeltaMovement		= VectorToGridSpace(worldDeltaMovement);
			var localLineDir			= VectorToGridSpace(worldRay.direction);
			var localLineOrg			= PointToGridSpace(worldRay.origin);
			
			scaleVector.x *= ((Mathf.Abs(localLineDir.y) >= 1 - MathConstants.EqualityEpsilon) || (Mathf.Abs(localLineDir.z) >= 1 - MathConstants.EqualityEpsilon)) ? 0 : 1;
			scaleVector.y *= ((Mathf.Abs(localLineDir.x) >= 1 - MathConstants.EqualityEpsilon) || (Mathf.Abs(localLineDir.z) >= 1 - MathConstants.EqualityEpsilon)) ? 0 : 1;
			scaleVector.z *= ((Mathf.Abs(localLineDir.x) >= 1 - MathConstants.EqualityEpsilon) || (Mathf.Abs(localLineDir.y) >= 1 - MathConstants.EqualityEpsilon)) ? 0 : 1;

			var snappedDeltaMovement	= localDeltaMovement;
			if (Mathf.Abs(scaleVector.x) < MathConstants.EqualityEpsilon) snappedDeltaMovement.x = 0;
			if (Mathf.Abs(scaleVector.y) < MathConstants.EqualityEpsilon) snappedDeltaMovement.y = 0;
			if (Mathf.Abs(scaleVector.z) < MathConstants.EqualityEpsilon) snappedDeltaMovement.z = 0;

			Vector3[] localPoints;
			if (worldPoints.Length > 1)
			{ 
				var bounds = new AABB();
				bounds.Reset();
				for (int i = 0; i < worldPoints.Length; i++)
				{
					var localPoint = GeometryUtility.ProjectPointOnInfiniteLine(PointToGridSpace(worldPoints[i]), localLineOrg, localLineDir);
					bounds.Extend(localPoint);
				}
				localPoints = bounds.GetCorners();
			} else
			{
				localPoints = new Vector3[] { GeometryUtility.ProjectPointOnInfiniteLine(PointToGridSpace(worldPoints[0]), localLineOrg, localLineDir) };
			}
			
			for (int i = 0; i < localPoints.Length; i++)
			{
				var oldPoint = localPoints[i];
				var newPoint = GeometryUtility.ProjectPointOnInfiniteLine(oldPoint + localDeltaMovement, localLineOrg, localLineDir);

				var snappedNewPoint = SnapRoundPosition(newPoint, snapVector);

				snappedNewPoint = GridUtility.CleanPosition(GeometryUtility.ProjectPointOnInfiniteLine(snappedNewPoint, localLineOrg, localLineDir));
						
				var foundDeltaMovement = (snappedNewPoint - oldPoint);
				
				foundDeltaMovement.x *= scaleVector.x;
				foundDeltaMovement.y *= scaleVector.y;
				foundDeltaMovement.z *= scaleVector.z;

				if (i == 0 || Math.Abs(foundDeltaMovement.x) < Mathf.Abs(snappedDeltaMovement.x)) snappedDeltaMovement.x = foundDeltaMovement.x; 
				if (i == 0 || Math.Abs(foundDeltaMovement.y) < Mathf.Abs(snappedDeltaMovement.y)) snappedDeltaMovement.y = foundDeltaMovement.y; 
				if (i == 0 || Math.Abs(foundDeltaMovement.z) < Mathf.Abs(snappedDeltaMovement.z)) snappedDeltaMovement.z = foundDeltaMovement.z; 
			}

			if (snapToSelf)
			{ 
				var snapDelta = (snappedDeltaMovement - localDeltaMovement);
				if (Mathf.Abs(snapDelta.x) > Mathf.Abs(localDeltaMovement.x)) snappedDeltaMovement.x = 0;
				if (Mathf.Abs(snapDelta.y) > Mathf.Abs(localDeltaMovement.y)) snappedDeltaMovement.y = 0;
				if (Mathf.Abs(snapDelta.z) > Mathf.Abs(localDeltaMovement.z)) snappedDeltaMovement.z = 0;
			}
			
			worldDeltaMovement = VectorFromGridSpace(snappedDeltaMovement);
			return worldDeltaMovement;
		}

		public static bool SnapToLine(Vector3 worldPoint, Vector3 worldVertex1, Vector3 worldVertex2, CSGPlane? snapPlane, ref Vector3 worldSnappedPoint)
		{
			var localGridPoint	= PointToGridSpace(worldPoint);
			var localVertex1	= PointToGridSpace(worldVertex1);
			var localVertex2	= PointToGridSpace(worldVertex2);
			
			var snapVector = gridOrientation.gridSnapVector;
			
			float minx = Mathf.Min(localVertex1.x, localVertex2.x);
			float maxx = Mathf.Max(localVertex1.x, localVertex2.x);
			
			float miny = Mathf.Min(localVertex1.y, localVertex2.y);
			float maxy = Mathf.Max(localVertex1.y, localVertex2.y);
			
			float minz = Mathf.Min(localVertex1.z, localVertex2.z);
			float maxz = Mathf.Max(localVertex1.z, localVertex2.z);

			var localLengthX = (maxx - minx);
			var localLengthY = (maxy - miny);
			var localLengthZ = (maxz - minz);
			if (localLengthX < MathConstants.AlignmentTestEpsilon &&
				localLengthY < MathConstants.AlignmentTestEpsilon &&
				localLengthZ < MathConstants.AlignmentTestEpsilon)
			{
				worldSnappedPoint = worldPoint;
				return false;
			}
			
			var found_points = new Vector3[6];
			var point_count = 0;

			if (localLengthX > MathConstants.AlignmentTestEpsilon)
			{
				float x1 = Mathf.FloorToInt(localGridPoint.x / snapVector.x) * snapVector.x;
				if (x1 > minx && x1 < maxx)
				{
					var xpos = x1;
					var t = (xpos - minx) / localLengthX;
					if (t > 0 && t < 1.0f)
					{ 
						var ypos = localVertex1.y + (t * (localVertex2.y - localVertex1.y));
						var zpos = localVertex1.z + (t * (localVertex2.z - localVertex1.z));
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
				
				float x2 = Mathf.CeilToInt (localGridPoint.x / snapVector.x) * snapVector.x;
				if (x2 > minx && x2 < maxx)
				{
					var xpos = x2;
					var t = (xpos - minx) / localLengthX;
					if (t > 0 && t < 1.0f)
					{ 
						var ypos = localVertex1.y + (t * (localVertex2.y - localVertex1.y));
						var zpos = localVertex1.z + (t * (localVertex2.z - localVertex1.z));
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
			}

			if (localLengthY > MathConstants.AlignmentTestEpsilon)
			{
				float y1 = Mathf.FloorToInt(localGridPoint.y / snapVector.y) * snapVector.y;
				if (y1 > miny && y1 < maxy)
				{
					var ypos = y1;
					var t = (ypos - miny) / localLengthY;
					if (t > 0 && t < 1.0f)
					{ 
						var zpos = localVertex1.z + (t * localLengthZ);
						var xpos = localVertex1.x + (t * localLengthX);
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
				
				float y2 = Mathf.CeilToInt (localGridPoint.y / snapVector.y) * snapVector.y;
				if (y2 > miny && y2 < maxy)
				{
					var ypos = y2;
					var t = (ypos - miny) / localLengthY;
					if (t > 0 && t < 1.0f)
					{ 
						var zpos = localVertex1.z + (t * localLengthZ);
						var xpos = localVertex1.x + (t * localLengthX);
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
			}

			if (localLengthZ > MathConstants.AlignmentTestEpsilon)
			{
				float z1 = Mathf.FloorToInt(localGridPoint.z / snapVector.z) * snapVector.z;
				if (z1 > minz && z1 < maxz)
				{
					var zpos = z1;
					var t = (zpos - minz) / localLengthZ;
					if (t > 0 && t < 1.0f)
					{ 
						var xpos = localVertex1.x + (t * (localVertex2.x - localVertex1.x));
						var ypos = localVertex1.y + (t * (localVertex2.y - localVertex1.y));
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
						
				float z2 = Mathf.CeilToInt (localGridPoint.z / snapVector.z) * snapVector.z;
				if (z2 > minz && z2 < maxz)
				{
					var zpos = z2;
					var t = (zpos - minz) / localLengthZ;
					if (t > 0 && t < 1.0f)
					{ 
						var xpos = localVertex1.x + (t * (localVertex2.x - localVertex1.x));
						var ypos = localVertex1.y + (t * (localVertex2.y - localVertex1.y));
						var localIntersection = new Vector3(xpos, ypos, zpos);
						var worldIntersection = PointFromGridSpace(localIntersection);
						if (snapPlane.HasValue)
							worldIntersection = snapPlane.Value.Project(worldIntersection);
						float dist = HandleUtility.DistancePointLine(worldIntersection, worldVertex1, worldVertex2);
						if (dist < MathConstants.DistanceEpsilon)
						{ 
							found_points[point_count] = worldIntersection; point_count++;
						}
					}
				}
			}

			if (point_count == 0)
				return false;

			if (point_count == 1)
			{
				worldSnappedPoint = found_points[0];
				return true;
			}

			float	found_dist	= (found_points[0] - worldPoint).sqrMagnitude;
			int		found_index = 0;
			for (int i = 1; i < point_count; i++)
			{
				float dist = (found_points[i] - worldPoint).sqrMagnitude;
				if (found_dist > dist)
				{
					found_dist = dist;
					found_index = i;
				}
			}
			
			worldSnappedPoint = found_points[found_index];
			//Debug.Log(worldSnappedPoint + " " + worldVertex1 + " " + worldVertex2);
			return true;
		}
		

		//public static bool YMoveModeActive { get; set; }


		public static bool SetupWorkPlane(Vector3 worldCenterPoint, ref CSGPlane workPlane)
		{
			var camera = Camera.current; 
			if (camera == null || !camera)
				return false;

			if (camera.orthographic)
			{				
				Grid.ForceGrid = false;
				workPlane = Grid.CurrentWorkGridPlane;
				return true;
			}
			
			var normal = Grid.CurrentGridPlane.normal;
			/*
			if (YMoveModeActive)
			{
				var forward = camera.transform.forward;
				Vector3 tangent, binormal;
				GeometryUtility.CalculateTangents(normal, out tangent, out binormal);
				if (Mathf.Abs(Vector3.Dot(forward, tangent)) > Mathf.Abs(Vector3.Dot(forward, binormal)))
					normal = tangent;
				else
					normal = binormal;
			}*/

			workPlane = new CSGPlane(GridUtility.CleanNormal(normal), worldCenterPoint);
			return Grid.SetForcedGrid(workPlane);
		}

		public static bool SetupWorkPlane(Vector3 worldCenterPoint, Vector3 worldDirection, ref CSGPlane workPlane)
		{
			var camera = Camera.current; 
			if (camera == null || !camera)
				return false;

			if (camera.orthographic)
			{				
				Grid.ForceGrid = false;
				workPlane = Grid.CurrentWorkGridPlane;
				return true;
			}
			
			var normal = worldDirection;
			/*
			if (YMoveModeActive)
			{
				var forward = camera.transform.forward;
				Vector3 tangent, binormal;
				GeometryUtility.CalculateTangents(normal, out tangent, out binormal);
				if (Mathf.Abs(Vector3.Dot(forward, tangent)) > Mathf.Abs(Vector3.Dot(forward, binormal)))
					normal = tangent;
				else
					normal = binormal;
			}*/

			workPlane = new CSGPlane(GridUtility.CleanNormal(normal), worldCenterPoint);
			return Grid.SetForcedGrid(workPlane);
		}

		public static bool SetupRayWorkPlane(Vector3 worldOrigin, Vector3 worldDirection, ref CSGPlane outWorkPlane)
		{
			var camera = Camera.current; 
			if (camera == null || !camera)
				return false;			
							
			Vector3 tangent, normal;
			var cameraBackwards			= -camera.transform.forward;
			var closestAxisForward		= GeometryUtility.SnapToClosestAxis(cameraBackwards);
			var closestAxisDirection	= GeometryUtility.SnapToClosestAxis(worldDirection);
			if (Vector3.Dot(closestAxisForward, closestAxisDirection) != 0)
			{
				float dot1 = Mathf.Abs(Vector3.Dot(cameraBackwards, MathConstants.rightVector3));
				float dot2 = Mathf.Abs(Vector3.Dot(cameraBackwards, MathConstants.upVector3));
				float dot3 = Mathf.Abs(Vector3.Dot(cameraBackwards, MathConstants.forwardVector3));
				if (dot1 < dot2)
				{
					if (dot1 < dot3)	tangent = MathConstants.rightVector3;
					else				tangent = MathConstants.forwardVector3;
				} else
				{
					if (dot2 < dot3)	tangent = MathConstants.upVector3;
					else				tangent = MathConstants.forwardVector3;
				}
			} else
				tangent = Vector3.Cross(worldDirection, closestAxisForward);
			
			if (!camera.orthographic)
			{ 
				normal = Vector3.Cross(worldDirection, tangent);
			} else
				normal = cameraBackwards;

			outWorkPlane = new CSGPlane(GridUtility.CleanNormal(normal), worldOrigin);
			
			return Grid.SetForcedGrid(outWorkPlane);
		}

		public static Vector3 CubeProject(CSGPlane plane, Vector3 pos)
		{
			UpdateGridOrientation();
			var closest_axis	= GeometryUtility.SnapToClosestAxis(plane.normal);
			var intersection	= plane.Intersection(pos, pos + closest_axis);

			if (float.IsNaN(intersection.x) || float.IsInfinity(intersection.x) ||
				float.IsNaN(intersection.y) || float.IsInfinity(intersection.y) ||
				float.IsNaN(intersection.z) || float.IsInfinity(intersection.z))
			{
				// should never happen, but if all else fails just do a projection ..
				intersection = plane.Project(pos);
			}
			return intersection;
		}

		public static bool SetForcedGrid(CSGPlane plane)
		{
			if (float.IsNaN(plane.a) || float.IsInfinity(plane.a) ||
				float.IsNaN(plane.b) || float.IsInfinity(plane.b) ||
				float.IsNaN(plane.c) || float.IsInfinity(plane.c) ||
				float.IsNaN(plane.d) || float.IsInfinity(plane.d) ||
				plane.normal.sqrMagnitude < MathConstants.NormalEpsilon)
			{
				Debug.LogWarning("Invalid plane passed to SetForcedGrid");
				return false;
			}

			// cube-project the center of the grid so that it lies on the plane
			
			UpdateGridOrientation();
			var center = CubeProject(plane, gridOrientation.gridCenter);

			var normal = Quaternion.Inverse(gridOrientation.gridRotation) * plane.normal;
			if (normal.sqrMagnitude < MathConstants.NormalEpsilon)
				return false;
			var tangent		= Vector3.Cross(normal, Vector3.Cross(normal, GeometryUtility.CalculateTangent(normal)).normalized).normalized;
			Quaternion q	= gridOrientation.gridRotation * Quaternion.LookRotation(tangent, normal);
			
			Grid.ForceGrid			= true;
			Grid.ForcedGridCenter	= center;
			Grid.ForcedGridRotation = q;
			return true;
		}
	}
}
 