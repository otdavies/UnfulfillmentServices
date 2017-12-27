//#define DEBUG_API
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct PolygonInput
	{
		public Int32 surfaceIndex;
		public Int32 firstHalfEdge;
		public Int32 edgeCount;
	}
	
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct IntersectionOutput
	{
		public Single		smallestT;
		public Int32		uniqueID;
		public Int32		brushID;
		public Int32		surfaceIndex;
		public Int32		texGenIndex;
		public Vector2		surfaceIntersection;
		public Vector3		worldIntersection;
		public CSGPlane		plane;
	}

	
	[Serializable]
	internal enum MeshType : int
	{
		RenderReceiveCastShadows	= 0,
		RenderReceiveShadows		= 1,
		RenderCastShadows			= 2,
		RenderOnly					= 3,
		ShadowOnly					= 4,

		Collider					= 5,
		Hidden						= 6,
		Culled						= 7,	// surfaces removed by CSG process
	};

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct MeshDescription
	{
		public MeshType meshType;
		public int		uniqueID;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct NativeTexGen
	{
		public Color	Color;
		public Vector2	Translation;
		public Vector2	Scale;
		public float	RotationAngle;
		public Int32	RenderMaterialID;			// instanceID to material
		public Int32	PhysicsMaterialID;			// instanceID to physicsMaterial
		public UInt32	SmoothingGroup;

		public NativeTexGen(TexGen texGen)
		{
			Color = texGen.Color;
			Translation = texGen.Translation;
			Scale = texGen.Scale;
			RotationAngle = texGen.RotationAngle;
			RenderMaterialID = (texGen.RenderMaterial) ? texGen.RenderMaterial.GetInstanceID() : 0;
			PhysicsMaterialID = (texGen.PhysicsMaterial) ? texGen.PhysicsMaterial.GetInstanceID() : 0;
			SmoothingGroup = texGen.SmoothingGroup;
		}
	}

	internal static class CSGBindings
	{
#if DEMO
		const string NativePluginName = "RealtimeCSG-DEMO-Native";
#else
		const string NativePluginName = "RealtimeCSG[" + ToolConstants.NativeVersion + "]";
#endif

#if DEMO
		[DllImport(NativePluginName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int GetBrushLimit();

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int BrushesAvailable();
#endif

		[DllImport(NativePluginName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool HasBeenCompiledInDebugMode();

		#region Functionality to allow C# methods to be called from C++
		public delegate float   GetFloatAction();
        public delegate Int32   GetInt32Action();
        public delegate void	StringLog(string text, int uniqueObjectID);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        struct UnityMethods
        {
            public StringLog		DebugLog;
            public StringLog		DebugLogError;
            public StringLog		DebugLogWarning;
        }

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern void RegisterMethods([In] ref UnityMethods unityMethods);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void ClearMethods();
		
		public static void RegisterUnityMethods()
		{
			UnityMethods unityMethods;
			
			unityMethods.DebugLog           = delegate (string message, int uniqueObjectID)
			{
				Debug.Log(message, (uniqueObjectID != 0) ? EditorUtility.InstanceIDToObject(uniqueObjectID) : null);
			};
			unityMethods.DebugLogError      = delegate (string message, int uniqueObjectID) 
			{
				Debug.LogError(message, (uniqueObjectID != 0) ? EditorUtility.InstanceIDToObject(uniqueObjectID) : null);
			};
			unityMethods.DebugLogWarning    = delegate (string message, int uniqueObjectID) 
			{
				Debug.LogWarning(message, (uniqueObjectID != 0) ? EditorUtility.InstanceIDToObject(uniqueObjectID) : null);
			};
			
			RegisterMethods(ref unityMethods);
		}

        public static void ClearUnityMethods()
		{
			ClearMethods();
            ResetCSG();
		}
		#endregion

		#region C++ Registration/Update functions

		#region Diagnostics
        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
	    public static extern void	LogDiagnostics();
        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
	    public static extern void	RebuildAll();
		#endregion

		#region Scene event functions

        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void	ResetCSG();

		#endregion

		#region Polygon Convex Decomposition
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool DecomposeStart(Int32			vertexCount,
												  IntPtr		vertices,		
												  out Int32		polygonCount);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool DecomposeGetSizes(Int32		polygonCount,
													 IntPtr		polygonSizes);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool DecomposeGetPolygon(Int32	polygonIndex,
													   Int32	vertexSize,
													   IntPtr	vertices);

		private static List<List<Vector2>> ConvexPartition(Vector2[] points)
		{
			Int32 polygonCount = 0;
			GCHandle	pointsHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
			IntPtr		pointsPtr = pointsHandle.AddrOfPinnedObject();
			var result = DecomposeStart(points.Length, pointsPtr, out polygonCount);
			pointsHandle.Free();
			if (!result)
				return null;

			if (polygonCount == 0)
				return null;

			var polygonSizes = new Int32[polygonCount];
			GCHandle	polygonSizesHandle	= GCHandle.Alloc(polygonSizes, GCHandleType.Pinned);
			IntPtr		polygonSizesPtr		= polygonSizesHandle.AddrOfPinnedObject();
			result = DecomposeGetSizes(polygonCount, polygonSizesPtr);
			polygonSizesHandle.Free();
			if (!result)
				return null;

			var polygons = new List<List<Vector2>>();
			for (int i = 0; i < polygonCount; i++)
			{
				var vertexCount = polygonSizes[i];
				var vertices	= new Vector2[vertexCount];
				GCHandle	verticesHandle	= GCHandle.Alloc(vertices, GCHandleType.Pinned);
				IntPtr		verticesPtr		= verticesHandle.AddrOfPinnedObject();
				result = DecomposeGetPolygon(i, vertexCount, verticesPtr);
				verticesHandle.Free();
				if (!result)
					return null;
				polygons.Add(new List<Vector2>(vertices));
			}

			return polygons;
		}

		#endregion

		#region Models C++ functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GenerateModelID(Int32		uniqueID,
												   [In] string	name,
												   out Int32	generatedModelID,
												   out Int32	generatedNodeID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetModel(Int32				modelID, 
											bool				isEnabled);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetModelChildren(Int32		modelID, 
													Int32		childCount,
													IntPtr		childrenNodeIDs);
		private static bool SetModelChildren(Int32		modelID,
											 Int32		childCount,
											 Int32[]	childrenNodeIDs)
		{
			GCHandle	childrenNodeIDsHandle	= GCHandle.Alloc(childrenNodeIDs, GCHandleType.Pinned);
			IntPtr		childrenNodeIDsPtr		= childrenNodeIDsHandle.AddrOfPinnedObject();
			var result = SetModelChildren(modelID,
										  childCount,
										  childrenNodeIDsPtr);
			childrenNodeIDsHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool UpdateNode		  (Int32	nodeID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetModelEnabled(Int32		modelID, 
												   bool			isEnabled);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveModel	(Int32			modelID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveModels	(Int32			modelCount,
												 IntPtr			modelIDs);
		private static bool RemoveModels(Int32		modelCount,
										 Int32[]	modelIDs)
		{
			GCHandle	modelIDsHandle	= GCHandle.Alloc(modelIDs, GCHandleType.Pinned);
			IntPtr		modelIDsPtr		= modelIDsHandle.AddrOfPinnedObject();
			var result = RemoveModels(modelCount,
									  modelIDsPtr);
			modelIDsHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern Int32 RayCastIntoModelMultiCount(Int32			modelID,
															   [In] ref Vector3	rayStart,
															   [In] ref Vector3	rayEnd,
															   bool				ignoreInvisiblePolygons,
															   float			growDistance,
															   IntPtr			ignoreNodeIndices,
															   Int32			ignoreNodeIndexCount);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RayCastIntoModelMultiGet(int			objectCount,
															IntPtr		distance,
															IntPtr		uniqueID,
															IntPtr		brushID,
															IntPtr		surfaceIndex,
															IntPtr		NativeTexGenIndex,
															IntPtr		surfaceIntersection,
															IntPtr		worldIntersection,
															IntPtr		surfaceInverted,
															IntPtr		plane);

		private static bool RayCastIntoModelMulti(CSGModel					model,
												  Vector3					rayStart,
												  Vector3					rayEnd,
												  bool						ignoreInvisiblePolygons,
												  float						growDistance,
												  out BrushIntersection[]	intersections,
												  CSGBrush[]				ignoreBrushes = null)
        {
			var visibleLayers = Tools.visibleLayers;
			if (!model || ((1 << model.gameObject.layer) & visibleLayers) == 0)
			{
				intersections = null;
				return false;
			}

			IntPtr ignoreNodeIndicesPtr = IntPtr.Zero;
			GCHandle ignoreNodeIndicesHandle = new GCHandle();

			Int32[] ignoreNodeIndices = null;
			if (ignoreBrushes != null)
			{
				ignoreNodeIndices = new Int32[ignoreBrushes.Length];
				for (int i = ignoreBrushes.Length - 1; i >= 0; i--)
				{
					if (!ignoreBrushes[i])
					{
						ArrayUtility.RemoveAt(ref ignoreNodeIndices, i);
						continue;
					}
					if (ignoreBrushes[i].nodeID == CSGNode.InvalidNodeID)
						continue;
					ignoreNodeIndices[i] = ignoreBrushes[i].nodeID;
				}
				
				ignoreNodeIndicesHandle = GCHandle.Alloc(ignoreNodeIndices, GCHandleType.Pinned);
				ignoreNodeIndicesPtr = ignoreNodeIndicesHandle.AddrOfPinnedObject();
			}
			
			Int32 intersectionCount = RayCastIntoModelMultiCount(model.modelID, 
																 ref rayStart,
																 ref rayEnd,
																 ignoreInvisiblePolygons,
															     growDistance,
																 ignoreNodeIndicesPtr,
																 (ignoreBrushes == null) ? 0 : ignoreBrushes.Length);

			if (ignoreNodeIndicesHandle.IsAllocated)
				ignoreNodeIndicesHandle.Free();

			if (intersectionCount == 0)
			{
				intersections = null;
				return false;
			}
			
			Single[]		distance = new Single[intersectionCount];
			Int32[]			uniqueID = new Int32[intersectionCount];
			Int32[]			brushID = new Int32[intersectionCount];
			Int32[]			surfaceIndex = new Int32[intersectionCount];
			Int32[]			texGenIndex = new Int32[intersectionCount];
			Vector2[]		surfaceIntersection = new Vector2[intersectionCount];
			Vector3[]		worldIntersection = new Vector3[intersectionCount];
			byte[]			surfaceInverted = new byte[intersectionCount];
			CSGPlane[]		planes = new CSGPlane[intersectionCount];

			
			GCHandle distanceHandle				= GCHandle.Alloc(distance, GCHandleType.Pinned);
			GCHandle uniqueIDHandle				= GCHandle.Alloc(uniqueID, GCHandleType.Pinned);
			GCHandle brushIDHandle				= GCHandle.Alloc(brushID, GCHandleType.Pinned);
			GCHandle surfaceIndexHandle			= GCHandle.Alloc(surfaceIndex, GCHandleType.Pinned);
			GCHandle texGenIndexHandle			= GCHandle.Alloc(texGenIndex, GCHandleType.Pinned);
			GCHandle surfaceIntersectionHandle	= GCHandle.Alloc(surfaceIntersection, GCHandleType.Pinned);
			GCHandle worldIntersectionHandle	= GCHandle.Alloc(worldIntersection, GCHandleType.Pinned);
			GCHandle surfaceInvertedHandle		= GCHandle.Alloc(surfaceInverted, GCHandleType.Pinned);
			GCHandle planesHandle				= GCHandle.Alloc(planes, GCHandleType.Pinned);

			IntPtr distancePtr				= distanceHandle.AddrOfPinnedObject();
			IntPtr uniqueIDPtr				= uniqueIDHandle.AddrOfPinnedObject();
			IntPtr brushIDPtr				= brushIDHandle.AddrOfPinnedObject();
			IntPtr surfaceIndexPtr			= surfaceIndexHandle.AddrOfPinnedObject();
			IntPtr texGenIndexPtr			= texGenIndexHandle.AddrOfPinnedObject();
			IntPtr surfaceIntersectionPtr	= surfaceIntersectionHandle.AddrOfPinnedObject();
			IntPtr worldIntersectionPtr		= worldIntersectionHandle.AddrOfPinnedObject();
			IntPtr surfaceInvertedPtr		= surfaceInvertedHandle.AddrOfPinnedObject();
			IntPtr planesPtr				= planesHandle.AddrOfPinnedObject();

			var result = RayCastIntoModelMultiGet(intersectionCount,
										  distancePtr,
										  uniqueIDPtr,
										  brushIDPtr,
										  surfaceIndexPtr,
										  texGenIndexPtr,
										  surfaceIntersectionPtr,
										  worldIntersectionPtr,
										  surfaceInvertedPtr,
										  planesPtr);

			distanceHandle.Free();
			uniqueIDHandle.Free();
			brushIDHandle.Free();
			surfaceIndexHandle.Free();
			texGenIndexHandle.Free();
			surfaceIntersectionHandle.Free();
			worldIntersectionHandle.Free();
			surfaceInvertedHandle.Free();
			planesHandle.Free();

			if (!result)
			{
				intersections = null;
				return false;
			}
			
			intersections = new BrushIntersection[intersectionCount];
			
			for (int i = 0, t = 0; i < intersectionCount; i++, t+=3)
			{
				var obj					= EditorUtility.InstanceIDToObject(uniqueID[i]);
				var monoBehaviour = obj as MonoBehaviour;
				if (monoBehaviour == null || !monoBehaviour)
				{
					intersections = null;
					//Debug.Log("EditorUtility.InstanceIDToObject(uniqueID:" + uniqueID[i] + ") does not lead to an object");
					return false;
				}

				var newIntersection = new BrushIntersection();
				newIntersection.distance			= distance[i];
				newIntersection.brushID				= brushID[i];
				newIntersection.gameObject			= monoBehaviour.gameObject;
				newIntersection.surfaceIndex		= surfaceIndex[i];
				newIntersection.texGenIndex			= texGenIndex[i];
				newIntersection.surfaceIntersection	= surfaceIntersection[i];
				newIntersection.worldIntersection	= worldIntersection[i];
				newIntersection.surfaceInverted		= (surfaceInverted[i] == 1);
				newIntersection.plane				= planes[i];
				newIntersection.model				= model;
				intersections[i] = newIntersection;
			}
			return true;
        }

		

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RayCastIntoBrush(Int32			brushID,
													Int32			texGenID,
													[In]ref Vector3	rayStart,
													[In]ref Vector3	rayEnd,
													bool			ignoreInvisiblePolygons,
													float			growDistance,
                                                    out Int32		surfaceIndex,
													out Int32		texGenIndex,
                                                    out Vector2		surfaceIntersection,
                                                    out Vector3		worldIntersection,
													out bool		surfaceInverted,
                                                    out CSGPlane	plane);

		private static bool RayCastIntoBrush(Int32					brushID, 
											 Int32					texGenIndex,
											 Vector3				rayStart,
											 Vector3				rayEnd,
											 bool					ignoreInvisiblePolygons,
											 float					growDistance,
											 out BrushIntersection	intersection)
        {
			if (brushID == -1)
			{
				intersection = null;
				return false;
			}
			
			Int32		surfaceIndex;
			Vector2		surfaceIntersection;
			Vector3		worldIntersection;
			bool		surfaceInverted;
			CSGPlane	plane;
			if (!RayCastIntoBrush(brushID,
								  texGenIndex,
								  ref rayStart,
								  ref rayEnd,
								  ignoreInvisiblePolygons,
								  growDistance,
								  out surfaceIndex,
								  out texGenIndex,
								  out surfaceIntersection,
								  out worldIntersection,
								  out surfaceInverted,
								  out plane))
			{
				intersection = null;
				return false;
			}
			
			var	result = new BrushIntersection();
			result.brushID				= brushID;
			result.gameObject			= null;
			result.surfaceIndex			= surfaceIndex;
			result.texGenIndex			= texGenIndex;
			result.surfaceIntersection	= surfaceIntersection;
			result.worldIntersection	= worldIntersection;
			result.surfaceInverted		= surfaceInverted;
			result.plane				= plane;
			result.model				= null;
			
			intersection = result;
			return true;
        }

		
		

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RayCastIntoBrushSurface(Int32				brushID, 
														   Int32				surfaceIndex,
														   [In]ref Vector3		rayStart,
														   [In]ref Vector3		rayEnd,
														   bool					ignoreInvisiblePolygons,
														   float				growDistance,
														   out Int32			texGenIndex,
														   out Vector2			surfaceIntersection,
														   out Vector3			worldIntersection,
														   out bool				surfaceInverted,
                                                           out CSGPlane			plane);

		private static bool RayCastIntoBrushSurface(Int32					brushID, 
												    Int32					surfaceIndex, 
												    Vector3					rayStart,
												    Vector3					rayEnd,
													bool					ignoreInvisiblePolygons,
													float					growDistance,
												    out BrushIntersection	intersection)
        {
			if (brushID == -1)
			{
				intersection = null;
				return false;
			}

			Vector2		surfaceIntersection;
			Vector3		worldIntersection;	
			Int32		texGenIndex;
			bool		surfaceInverted;
			CSGPlane	plane;
			if (!RayCastIntoBrushSurface(brushID,
								 		 surfaceIndex,
								 		 ref rayStart,
										 ref rayEnd,
										 ignoreInvisiblePolygons,
										 growDistance,
										 out texGenIndex,
										 out surfaceIntersection,
										 out worldIntersection,
										 out surfaceInverted,
										 out plane))
			{
				intersection = null;
				return false;
			}
			
			var	result = new BrushIntersection();
			result.brushID				= brushID;
			result.gameObject			= null;
			result.surfaceIndex			= surfaceIndex;
			result.texGenIndex			= texGenIndex;
			result.surfaceIntersection	= surfaceIntersection;
			result.worldIntersection	= worldIntersection;
			result.surfaceInverted		= surfaceInverted;
			result.plane				= plane;
			result.model				= null;
			
			intersection = result;
			return true;
        }
		
			
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern int FindItemsInFrustum(Int32		modelID,
													 IntPtr		planes, 
													 Int32		planeCount);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RetrieveItemIDsInFrustum(IntPtr	objectIDs,
															Int32	objectIDCount);

		private static bool GetItemsInFrustum(CSGModel				model, 
											  CSGPlane[]			planes, 
											  HashSet<GameObject>	gameObjects)
		{
			var visibleLayers = Tools.visibleLayers;
			if (!model || ((1 << model.gameObject.layer) & visibleLayers) == 0)
			{
				return false;
			}
						
			if (planes == null ||
				planes.Length != 6)
			{
				return false;
			}

			var translated_planes = new CSGPlane[planes.Length];
			for (int i = 0; i < planes.Length; i++)
			{
				translated_planes[i] = planes[i].Translated(-model.transform.position);
			}

			GCHandle	planesHandle	= GCHandle.Alloc(translated_planes, GCHandleType.Pinned);
			IntPtr		planesPtr		= planesHandle.AddrOfPinnedObject();
			var itemCount = FindItemsInFrustum(model.modelID, planesPtr, translated_planes.Length);
			planesHandle.Free();
			if (itemCount == 0)
			{
				return false;
			}

			var ids = new int[itemCount];
			GCHandle	idsHandle	= GCHandle.Alloc(ids, GCHandleType.Pinned);
			IntPtr		idsPtr		= idsHandle.AddrOfPinnedObject();
			var result = RetrieveItemIDsInFrustum(idsPtr, ids.Length);
			idsHandle.Free();
			if (!result)
			{
				return false;
			}

			bool found = false;
			for (int i = ids.Length - 1; i >= 0; i--)
			{
				var obj			= EditorUtility.InstanceIDToObject(ids[i]);
				var brush		= obj as MonoBehaviour;
				var gameObject	= (brush != null) ? brush.gameObject : null;
				if (gameObject == null)
					continue;
				
				gameObjects.Add(gameObject);
				found = true;
			}

			return found;
		}
		#endregion


		#region Operation C++ functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GenerateOperationID(Int32		uniqueID,
													   [In] string	name,
													   out Int32	generatedOperationID,
													   out Int32	generatedNodeID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetOperationHierarchy(Int32		operationID,
														 Int32		modelID,
														 Int32		parentID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetOperation(Int32				operationID, 
											    Int32				modelID, 
												Int32				parentID, 
												CSGOperationType	operation);

		[DllImport(NativePluginName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool SetOperationChildren(Int32	modelID,
														Int32	childCount,
														IntPtr	childrenNodeIDs);

		private static bool SetOperationChildren(Int32		modelID, 
												 Int32		childCount,
												 Int32[]	childrenNodeIDs)
		{
			GCHandle	childrenNodeIDsHandle	= GCHandle.Alloc(childrenNodeIDs, GCHandleType.Pinned);
			IntPtr		childrenNodeIDsPtr		= childrenNodeIDsHandle.AddrOfPinnedObject();

			var result = SetOperationChildren(modelID,
											  childCount,
											  childrenNodeIDsPtr);

			childrenNodeIDsHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetOperationOperationType(Int32				operationID, 
															 CSGOperationType	operation);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveOperation	(Int32			operationID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveOperations	(Int32			operationCount,
													 IntPtr			operationIDs);
		private static bool RemoveOperations(Int32		operationCount,
											 Int32[]	operationIDs)
		{
			GCHandle	operationIDsHandle	= GCHandle.Alloc(operationIDs, GCHandleType.Pinned);
			IntPtr		operationIDsPtr		= operationIDsHandle.AddrOfPinnedObject();

			var result = RemoveOperations(operationCount,
										  operationIDsPtr);

			operationIDsHandle.Free();
			return result;
		}

		#endregion


		#region Brush C++ functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GenerateBrushID(Int32		uniqueID,
												   [In] string	name,
												   out Int32	generatedBrushID,
												   out Int32	generatedNodeID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrush(Int32				brushID, 
										    Int32				modelID, 
											Int32				parentID,
											CSGOperationType	operation,
											UInt32				contentLayer,
											[In] ref Matrix4x4	planeToObjectSpace,								
                                            [In] ref Matrix4x4	objectToPlaneSpace,
											[In] ref Vector3	translation,
											Int32				surfaceCount,
											IntPtr				surfaces,
											Int32				texGenCount,
											IntPtr				nativeTexgens,
											IntPtr				texgenFlags);
		
		private static bool SetBrush(Int32				brushID, 
									 Int32				modelID, 
									 Int32				parentID,
									 CSGOperationType	operation,
									 UInt32				contentLayer,
									 ref Matrix4x4		planeToObjectSpace,								
                                     ref Matrix4x4		objectToPlaneSpace,
									 ref Vector3		translation,
									 Int32				surfaceCount,
									 Surface[]			surfaces,
									 Int32				texGenCount,
									 NativeTexGen[]		nativeTexgens,
									 TexGenFlags[]		texgenFlags)
		{
			GCHandle surfacesHandle = GCHandle.Alloc(surfaces, GCHandleType.Pinned);
			GCHandle nativeTexgensHandle = GCHandle.Alloc(nativeTexgens, GCHandleType.Pinned);
			GCHandle texgenFlagsHandle = GCHandle.Alloc(texgenFlags, GCHandleType.Pinned);

			IntPtr surfacesPtr		= surfacesHandle.AddrOfPinnedObject();
			IntPtr nativeTexgensPtr = nativeTexgensHandle.AddrOfPinnedObject();
			IntPtr texgenFlagsPtr	= texgenFlagsHandle.AddrOfPinnedObject();

			var result = SetBrush(brushID,
								  modelID,
								  parentID,
								  operation,
								  contentLayer,
								  ref planeToObjectSpace,
								  ref objectToPlaneSpace,
								  ref translation,
								  surfaceCount,
								  surfacesPtr,
								  texGenCount,
								  nativeTexgensPtr,
								  texgenFlagsPtr);

			surfacesHandle.Free();
			nativeTexgensHandle.Free();
			texgenFlagsHandle.Free();
			return result;
		}
		
		private static bool SetBrush(Int32				brushID, 
									 Int32				modelID, 
									 Int32				parentID,
									 CSGOperationType	operation,
									 UInt32				contentLayer,
									 Matrix4x4			planeToObjectSpace,
                                     Matrix4x4			objectToPlaneSpace,
									 Vector3			translation,
									 Surface[]			surfaces,
									 TexGen[]			texgens,
									 TexGenFlags[]		texgenFlags)
		{
			var nativeTexgens = new NativeTexGen[texgens.Length];
			for (int i = 0; i < texgens.Length; i++)
				nativeTexgens[i] = new NativeTexGen(texgens[i]);
			return SetBrush(brushID, modelID, parentID, operation, contentLayer, 
							 ref planeToObjectSpace, ref objectToPlaneSpace, ref translation,
							 surfaces.Length, surfaces, texgens.Length, nativeTexgens, texgenFlags);
		}


		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushInfinite(Int32				brushID, 
													Int32				modelID, 
													Int32				parentID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushOperationType(Int32				brushID,
														 UInt32				contentLayer,
														 CSGOperationType	operation);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushMesh(Int32		brushID,
												Int32		vertexCount,
												IntPtr		vertices,
												Int32		halfEdgeCount,
												IntPtr		vertexIndices,
												IntPtr		halfEdgeTwins,
												Int32		polygonCount,
												IntPtr		polygons);
		private static bool SetBrushMesh(Int32				brushID,
										 Int32				vertexCount,
										 Vector3[]			vertices,
										 Int32				halfEdgeCount,
										 Int32[]			vertexIndices,
										 Int32[]			halfEdgeTwins,
										 Int32				polygonCount,
										 PolygonInput[]		polygons)
		{
			GCHandle	verticesHandle		= GCHandle.Alloc(vertices, GCHandleType.Pinned);
			GCHandle	vertexIndicesHandle	= GCHandle.Alloc(vertexIndices, GCHandleType.Pinned);
			GCHandle	halfEdgeTwinsHandle	= GCHandle.Alloc(halfEdgeTwins, GCHandleType.Pinned);
			GCHandle	polygonsHandle		= GCHandle.Alloc(polygons, GCHandleType.Pinned);
			IntPtr		verticesPtr			= verticesHandle.AddrOfPinnedObject();
			IntPtr		vertexIndicesPtr	= vertexIndicesHandle.AddrOfPinnedObject();
			IntPtr		halfEdgeTwinsPtr	= halfEdgeTwinsHandle.AddrOfPinnedObject();
			IntPtr		polygonsPtr			= polygonsHandle.AddrOfPinnedObject();

			var result = SetBrushMesh(brushID,
									  vertexCount,
									  verticesPtr,
									  halfEdgeCount,
									  vertexIndicesPtr,
									  halfEdgeTwinsPtr,
									  polygonCount,
									  polygonsPtr);

			verticesHandle.Free();
			vertexIndicesHandle.Free();
			halfEdgeTwinsHandle.Free();
			polygonsHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushHierarchy(Int32		brushID, 
													 Int32		modelID, 
													 Int32		parentID);

        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        public static extern bool GetBrushBounds   (Int32		brushIndex,
								                    ref AABB	bounds);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushTransformation(Int32				brushID,
														  [In] ref Matrix4x4 planeToObjectspace,								
                                                          [In] ref Matrix4x4 objectToPlaneSpace);

        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushTranslation(Int32			brushID,
													   [In] ref Vector3	translation);

        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushSurfaces(Int32		brushID,
													Int32		surfaceCount,
													IntPtr		surfaces);
		private static bool SetBrushSurfaces(Int32		brushID,
											 Int32		surfaceCount,
											 Surface[]	surfaces)
		{
			GCHandle	surfacesHandle	= GCHandle.Alloc(surfaces, GCHandleType.Pinned);
			IntPtr		surfacesPtr		= surfacesHandle.AddrOfPinnedObject();

			var result = SetBrushSurfaces(brushID,
										  surfaceCount,
										  surfacesPtr);

			surfacesHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushSurface(Int32		brushID,
												   Int32		surfaceIndex,
												   [In] ref Surface	surface);

        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushTexGens (Int32		brushID, 
												    Int32		texGenCount,
													IntPtr		nativeTexgens,
													IntPtr		texgenFlags);
		private static bool SetBrushTexGens (Int32			brushID, 
											 Int32			texGenCount,
											 NativeTexGen[]	nativeTexgens,
											 TexGenFlags[]	texgenFlags)
		{
			GCHandle	nativeTexgensHandle = GCHandle.Alloc(nativeTexgens, GCHandleType.Pinned);
			GCHandle	texgenFlagsHandle	= GCHandle.Alloc(texgenFlags, GCHandleType.Pinned);
			IntPtr		nativeTexgensPtr	= nativeTexgensHandle.AddrOfPinnedObject();
			IntPtr		texgenFlagsPtr		= texgenFlagsHandle.AddrOfPinnedObject();

			var result = SetBrushTexGens(brushID,
										 texGenCount,
										 nativeTexgensPtr,
										 texgenFlagsPtr);

			nativeTexgensHandle.Free();
			texgenFlagsHandle.Free();
			return result;
		}

		private static bool SetBrushTexGens(Int32 brushID,
											TexGen[] texgens,
											TexGenFlags[] texgenFlags)		
		{
			var nativeTexgens = new NativeTexGen[texgens.Length];
			for (int i = 0; i<texgens.Length; i++)
				nativeTexgens[i] = new NativeTexGen(texgens[i]);
			return SetBrushTexGens(brushID, texgens.Length, nativeTexgens, texgenFlags);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushSurfaceTexGens (Int32	brushID, 
														   Int32	surfaceCount,
														   IntPtr	surfaces,
														   Int32	texGenCount,
														   IntPtr	nativeTexgens,
														   IntPtr	texgenFlags);
		private static bool SetBrushSurfaceTexGens (Int32			brushID, 
													Int32			surfaceCount,
													Surface[]		surfaces,
													Int32			texGenCount,
													NativeTexGen[]	nativeTexgens,
													TexGenFlags[]	texgenFlags)
		{
			GCHandle surfacesHandle			= GCHandle.Alloc(surfaces, GCHandleType.Pinned);
			GCHandle nativeTexgensHandle	= GCHandle.Alloc(nativeTexgens, GCHandleType.Pinned);
			GCHandle texgenFlagsHandle		= GCHandle.Alloc(texgenFlags, GCHandleType.Pinned);

			IntPtr surfacesPtr		= surfacesHandle.AddrOfPinnedObject();
			IntPtr nativeTexgensPtr = nativeTexgensHandle.AddrOfPinnedObject();
			IntPtr texgenFlagsPtr	= texgenFlagsHandle.AddrOfPinnedObject();

			var result = SetBrushSurfaceTexGens(brushID,
												surfaceCount,
												surfacesPtr,
												texGenCount,
												nativeTexgensPtr,
												texgenFlagsPtr);

			surfacesHandle.Free();
			nativeTexgensHandle.Free();
			texgenFlagsHandle.Free();
			return result;
		}
		
		private static bool SetBrushSurfaceTexGens(Int32 brushID,
												   Surface[] surfaces,
												   TexGen[] texgens,
												   TexGenFlags[] texgenFlags)
		{
			var nativeTexgens = new NativeTexGen[texgens.Length];
			for (int i = 0; i<texgens.Length; i++)
				nativeTexgens[i] = new NativeTexGen(texgens[i]);
			return SetBrushSurfaceTexGens(brushID, surfaces.Length, surfaces, texgens.Length, nativeTexgens, texgenFlags);
		}
        
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushTexGen (Int32					brushID,
												   Int32					texGenIndex,
												   [In] ref NativeTexGen	texGen);

		private static bool SetBrushTexGen(Int32 brushID, Int32 texGenIndex, ref TexGen texgen)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			return SetBrushTexGen(brushID, texGenIndex, ref nativeTexGen);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool SetBrushTexGenFlags (Int32		brushID,
													    Int32		texGenIndex,
													    TexGenFlags	texGenFlags);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveBrush(Int32				brushID);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool RemoveBrushes(Int32			brushCount,
												 IntPtr			brushIDs);
		private static bool RemoveBrushes(Int32 brushCount,
										  Int32[] brushIDs)
		{
			GCHandle	brushIDsHandle	= GCHandle.Alloc(brushIDs, GCHandleType.Pinned);
			IntPtr		brushIDsPtr		= brushIDsHandle.AddrOfPinnedObject();

			var result = RemoveBrushes(brushCount,
									   brushIDsPtr);

			brushIDsHandle.Free();
			return result;
		}
		#endregion

		#region TexGen manipulation C++ functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool FitSurface				 (Int32					brushIndex,
															  Int32					texGenIndex, 
															  [In] ref NativeTexGen nativeTexGen);
		private static bool FitSurface(Int32 brushID, Int32 texGenIndex, ref TexGen texgen)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			return FitSurface(brushID, texGenIndex, ref nativeTexGen);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool FitSurfaceX				 (Int32					brushIndex,
															  Int32					texGenIndex, 
															  [In] ref NativeTexGen nativeTexGen);
		private static bool FitSurfaceX(Int32 brushID, Int32 texGenIndex, ref TexGen texgen)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			return FitSurfaceX(brushID, texGenIndex, ref nativeTexGen);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool FitSurfaceY				 (Int32					brushIndex,
															  Int32					texGenIndex, 
															  [In] ref NativeTexGen nativeTexGen);
		private static bool FitSurfaceY(Int32 brushID, Int32 texGenIndex, ref TexGen texgen)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			return FitSurfaceX(brushID, texGenIndex, ref nativeTexGen);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetSurfaceMinMaxTexCoords (Int32			brushIndex,
															  Int32			surfaceIndex, 
															  out Vector2	minTextureCoordinate, 		
															  out Vector2	maxTextureCoordinate);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetSurfaceMinMaxWorldCoord(Int32			brushIndex,
															  Int32			surfaceIndex, 
															  out Vector3	minWorldCoordinate, 
															  out Vector3	maxWorldCoordinate);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool ConvertWorldToTextureCoord(Int32				brushIndex,
															  Int32				surfaceIndex,
															  [In]ref Vector3	worldCoordinate, 
															  out Vector2		textureCoordinate);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool ConvertTextureToWorldCoord(Int32			brushIndex,
															  Int32			surfaceIndex, 
															  float			textureCoordinateU, 
															  float			textureCoordinateV, 
															  out Vector3	worldCoordinate);
		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void GetTexGenMatrices			([In] ref NativeTexGen	texGen,
																 [In] TexGenFlags		texGenFlags,
																 [In] ref Surface		surface,
																 out Matrix4x4			textureSpaceToLocalSpace,
																 out Matrix4x4			localSpaceToTextureSpace);
		private static void GetTexGenMatrices(ref TexGen	texgen, 
											  TexGenFlags	texGenFlags, 
											  ref Surface	surface, 
											  out Matrix4x4	textureSpaceToLocalSpace,
											  out Matrix4x4	localSpaceToTextureSpace)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			GetTexGenMatrices(ref nativeTexGen, texGenFlags, ref surface, out textureSpaceToLocalSpace, out localSpaceToTextureSpace);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void GetTextureToLocalSpaceMatrix	([In] ref NativeTexGen	texGen,
																 [In] TexGenFlags		texGenFlags,
																 [In] ref Surface		surface,
																 out Matrix4x4			textureSpaceToLocalSpace);
		private static void GetTextureToLocalSpaceMatrix(ref TexGen		texgen,
														 TexGenFlags	texGenFlags,
														 ref Surface	surface,
														 out Matrix4x4	textureSpaceToLocalSpace)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			GetTextureToLocalSpaceMatrix(ref nativeTexGen, texGenFlags, ref surface, out textureSpaceToLocalSpace);
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void GetLocalToTextureSpaceMatrix	([In] ref NativeTexGen	texGen,
																 TexGenFlags			texGenFlags,
																 [In] ref Surface		surface,
																 out Matrix4x4			localSpaceToTextureSpace);
		private static void GetLocalToTextureSpaceMatrix(ref TexGen		texgen,
														 TexGenFlags	texGenFlags,
														 ref Surface	surface,
														 out Matrix4x4	localSpaceToTextureSpace)
		{
			var nativeTexGen = new NativeTexGen(texgen);
			GetLocalToTextureSpaceMatrix(ref nativeTexGen, texGenFlags, ref surface, out localSpaceToTextureSpace);
		}

		#endregion

		#region C++ Mesh functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool UpdateModelMeshes();

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ForceModelUpdate(Int32			modelID); 

		
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern bool HasModelChanged		(Int32		modelID, 
														 ref Int32	meshDescriptionCount);

		[DllImport(NativePluginName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool GetMeshDescriptions	(Int32 meshDescriptionCount,
													     IntPtr meshDescriptions);
		private static bool GetMeshDescriptions(Int32 meshDescriptionCount,
												MeshDescription[] meshDescriptions)
		{
			GCHandle meshDescriptionsHandle = GCHandle.Alloc(meshDescriptions, GCHandleType.Pinned);
			
			IntPtr meshDescriptionsPtr = meshDescriptionsHandle.AddrOfPinnedObject();
			
			var result = GetMeshDescriptions(meshDescriptionCount, meshDescriptionsPtr);

			meshDescriptionsHandle.Free();
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern bool TryUpdatingModelMeshes(Int32				modelID,
													   [In]ref Matrix4x4	modelMatrix); 
		

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern void ModelMeshFinishedUpdating(Int32	modelID);

		[DllImport(NativePluginName, CallingConvention = CallingConvention.Cdecl)]
		private static extern Int32 GetModelSubMeshCount(Int32 modelID);
        

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern UInt32 CreateModelMeshes(Int32				modelID,
													  [In]ref Matrix4x4		modelMatrix,
													  MeshType				meshType,
													  Int32					meshUniqueID,
													  VertexChannelFlags	meshVertexChannels); 
        
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetSubMeshStatus(UInt32 subMeshCount,
												    IntPtr subMeshVertexHashes,
													IntPtr subMeshTriangleHashes,
													IntPtr subMeshSurfaceHashes,
													IntPtr subMeshIndices,
													IntPtr subMeshVertexCounts);
		private static bool GetSubMeshStatus(UInt32 subMeshCount,
											 UInt64[] subMeshVertexHashes,
											 UInt64[] subMeshTriangleHashes,
											 UInt64[] subMeshSurfaceHashes,
											 Int32[] subMeshIndices,
											 Int32[] subMeshVertexCounts)
		{
			GCHandle subMeshVertexHashesHandle	 = GCHandle.Alloc(subMeshVertexHashes,	 GCHandleType.Pinned);
			GCHandle subMeshTriangleHashesHandle = GCHandle.Alloc(subMeshTriangleHashes, GCHandleType.Pinned);
			GCHandle subMeshSurfaceHashesHandle	 = GCHandle.Alloc(subMeshSurfaceHashes,  GCHandleType.Pinned);
			GCHandle subMeshIndicesHandle		 = GCHandle.Alloc(subMeshIndices,		 GCHandleType.Pinned);
			GCHandle subMeshVertexCountsHandle	 = GCHandle.Alloc(subMeshVertexCounts,	 GCHandleType.Pinned);

			IntPtr subMeshVertexHashesPtr	= subMeshVertexHashesHandle	 .AddrOfPinnedObject();
			IntPtr subMeshTriangleHashesPtr = subMeshTriangleHashesHandle.AddrOfPinnedObject();
			IntPtr subMeshSurfaceHashesPtr	= subMeshSurfaceHashesHandle .AddrOfPinnedObject();
			IntPtr subMeshIndicesPtr		= subMeshIndicesHandle		 .AddrOfPinnedObject();
			IntPtr subMeshVertexCountsPtr	= subMeshVertexCountsHandle	 .AddrOfPinnedObject();

			var result = GetSubMeshStatus(subMeshCount,
									subMeshVertexHashesPtr,
									subMeshTriangleHashesPtr,
									subMeshSurfaceHashesPtr,
									subMeshIndicesPtr,
									subMeshVertexCountsPtr);

			subMeshVertexHashesHandle	.Free();
			subMeshTriangleHashesHandle .Free();
			subMeshSurfaceHashesHandle	.Free();
			subMeshIndicesHandle		.Free();
			subMeshVertexCountsHandle	.Free();

			return result;
		}
		
        [DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern bool FillVertices(Int32		subMeshIndex,
												Int32		vertexCount,
												IntPtr		colors,
												IntPtr		tangents,
												IntPtr		normals,
												IntPtr		positions,
												IntPtr		uvs);

		private static bool FillVertices(Int32		subMeshIndex,
										 Int32		vertexCount,
										 Color[]	colors,
										 Vector4[]	tangents,
										 Vector3[]	normals,
										 Vector3[]	positions,
										 Vector2[]	uvs)
		{
			GCHandle tangentHandle		= new GCHandle();
			GCHandle normalHandle		= new GCHandle();
			GCHandle uvsHandle			= new GCHandle();
			GCHandle colorHandle		= new GCHandle();
			GCHandle positionHandle		= new GCHandle();

			IntPtr tangentPtr	= IntPtr.Zero;
			IntPtr normalPtr	= IntPtr.Zero;
			IntPtr uvsPtr		= IntPtr.Zero;
			IntPtr colorPtr		= IntPtr.Zero;
			IntPtr positionPtr	= IntPtr.Zero;

			if (tangents	!= null) { tangentHandle	= GCHandle.Alloc(tangents,	GCHandleType.Pinned); tangentPtr  = tangentHandle.AddrOfPinnedObject(); }
			if (normals		!= null) { normalHandle		= GCHandle.Alloc(normals,	GCHandleType.Pinned); normalPtr   = normalHandle.AddrOfPinnedObject(); }
			if (uvs			!= null) { uvsHandle		= GCHandle.Alloc(uvs,		GCHandleType.Pinned); uvsPtr	  = uvsHandle.AddrOfPinnedObject(); }
			if (colors		!= null) { colorHandle		= GCHandle.Alloc(colors,	GCHandleType.Pinned); colorPtr	  = colorHandle.AddrOfPinnedObject(); }
			if (positions	!= null) { positionHandle	= GCHandle.Alloc(positions, GCHandleType.Pinned); positionPtr = positionHandle.AddrOfPinnedObject(); }

			var result = FillVertices(subMeshIndex,
									  vertexCount,
									  colorPtr,
									  tangentPtr,
									  normalPtr,
									  positionPtr,
									  uvsPtr);
			
			if (tangents	!= null) { tangentHandle .Free(); }
			if (normals		!= null) { normalHandle	 .Free(); }
			if (uvs			!= null) { uvsHandle	 .Free(); }
			if (colors		!= null) { colorHandle	 .Free(); }
			if (positions	!= null) { positionHandle.Free(); }
			return result;
		}

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
        private static extern bool FillIndices(Int32		subMeshIndex,
											   Int32		indexCount,
											   IntPtr		indices);

		private static bool FillIndices(Int32 subMeshIndex,
										Int32 indexCount,
										Int32[] indices)
		{
			GCHandle	indicesHandle	= GCHandle.Alloc(indices, GCHandleType.Pinned);
			IntPtr		indicesPtr		= indicesHandle.AddrOfPinnedObject();

			var result = FillIndices((Int32)subMeshIndex,
									 (Int32)indices.Length,
									 indicesPtr);

			indicesHandle.Free();
			return result;
		}

		#endregion


		#region Outline C++ functions
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern UInt64 GetBrushOutlineGeneration(Int32 brushID);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetBrushOutlineSizes(Int32		brushID,
													    out Int32	vertexCount,
														out Int32	visibleOuterLineCount,
														out Int32	visibleInnerLineCount,
														out Int32	invisibleOuterLineCount,
														out Int32	invisibleInnerLineCount,
														out Int32	invalidLineCount);

		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetBrushOutlineValues(Int32		brushID,
														 Int32		vertexCount,
														 IntPtr		vertices,
														 Int32		visibleOuterLineCount,
														 IntPtr		visibleOuterLines,
														 Int32		visibleInnerLineCount,
														 IntPtr		visibleInnerLines,
														 Int32		invisibleOuterLineCount,
														 IntPtr		invisibleOuteLines,
														 Int32		invisibleInnerLineCount,
														 IntPtr		invisibleInnerLines,
														 Int32		invalidLineCount,
														 IntPtr		invalidLines);
		
		private static bool GetBrushOutline(Int32			brushID,
											ref Vector3[]	vertices,
											ref Int32[]		visibleOuterLines,
											ref Int32[]		visibleInnerLines,
											ref Int32[]		invisibleOuterLines,
											ref Int32[]		invisibleInnerLines,
											ref Int32[]		invalidLines)
		{
			int vertexCount = 0;
			int visibleOuterLineCount = 0;
			int visibleInnerLineCount = 0;
            int invisibleOuterLineCount = 0;
			int invisibleInnerLineCount = 0;
			int invalidLineCount = 0;
			if (!GetBrushOutlineSizes(brushID,
									  out vertexCount,
									  out visibleOuterLineCount,
									  out visibleInnerLineCount,
									  out invisibleOuterLineCount,
									  out invisibleInnerLineCount,
									  out invalidLineCount))
			{
				return false;
			}
			
			if (vertexCount == 0 ||
				(visibleOuterLineCount == 0 && invisibleOuterLineCount == 0 && 
                 visibleInnerLineCount == 0 && invisibleInnerLineCount == 0 && 
                 invalidLineCount == 0))
			{
				vertexCount = 0;
				visibleOuterLineCount = 0;
				visibleInnerLineCount = 0;
				invisibleOuterLineCount = 0;
				invisibleInnerLineCount = 0;
                invalidLineCount = 0;
                return false;
			}

            if (vertices == null || vertices.Length != vertexCount)
			{
				vertices = new Vector3[vertexCount];
			}

            if (visibleOuterLineCount > 0 &&
                (visibleOuterLines == null || visibleOuterLines.Length != visibleOuterLineCount))
			{
				visibleOuterLines = new Int32[visibleOuterLineCount];
            }

            if (visibleInnerLineCount > 0 &&
                (visibleInnerLines == null || visibleInnerLines.Length != visibleInnerLineCount))
			{
				visibleInnerLines = new Int32[visibleInnerLineCount];
            }

			if (invisibleOuterLineCount > 0 &&
                (invisibleOuterLines == null || invisibleOuterLines.Length != invisibleOuterLineCount))
			{
				invisibleOuterLines = new Int32[invisibleOuterLineCount];
            }

			if (invisibleInnerLineCount > 0 &&
                (invisibleInnerLines == null || invisibleInnerLines.Length != invisibleInnerLineCount))
			{
				invisibleInnerLines = new Int32[invisibleInnerLineCount];
            }

            if (invalidLineCount > 0 &&
                (invalidLines == null || invalidLines.Length != invalidLineCount))
            {
                invalidLines = new Int32[invalidLineCount];
			}

			GCHandle verticesHandle				= GCHandle.Alloc(vertices, GCHandleType.Pinned);
			GCHandle visibleOuterLinesHandle	= GCHandle.Alloc(visibleOuterLines, GCHandleType.Pinned);
			GCHandle visibleInnerLinesHandle	= GCHandle.Alloc(visibleInnerLines, GCHandleType.Pinned);
			GCHandle invisibleOuterLinesHandle	= GCHandle.Alloc(invisibleOuterLines, GCHandleType.Pinned);
			GCHandle invisibleInnerLinesHandle	= GCHandle.Alloc(invisibleInnerLines, GCHandleType.Pinned);
			GCHandle invalidLinesHandle			= GCHandle.Alloc(invalidLines, GCHandleType.Pinned);

			IntPtr verticesPtr				= verticesHandle.AddrOfPinnedObject();
			IntPtr visibleOuterLinesPtr		= visibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr visibleInnerLinesPtr		= visibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr invisibleOuterLinesPtr	= invisibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr invisibleInnerLinesPtr	= invisibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr invalidLinesPtr			= invalidLinesHandle.AddrOfPinnedObject();
			
			if (!GetBrushOutlineValues(brushID,
									   vertexCount,
									   verticesPtr,
									   visibleOuterLineCount,
									   visibleOuterLinesPtr,
									   visibleInnerLineCount,
									   visibleInnerLinesPtr,
									   invisibleOuterLineCount,
									   invisibleOuterLinesPtr,
									   invisibleInnerLineCount,
									   invisibleInnerLinesPtr,
                                       invalidLineCount,
									   invalidLinesPtr))
			{
				return false;
			}

			verticesHandle.Free();
			visibleOuterLinesHandle.Free();
			visibleInnerLinesHandle.Free();
			invisibleOuterLinesHandle.Free();
			invisibleInnerLinesHandle.Free();
			invalidLinesHandle.Free();

			return true;
		}

        
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetSurfaceOutlineSizes(Int32		    brushID,
														  Int32		    surfaceID,
													      out Int32	    vertexCount,
														  out Int32	    visibleOuterLineCount,
														  out Int32	    visibleInnerLineCount,
														  out Int32	    visibleTriangleCount,
														  out Int32	    invisibleOuterLineCount,
														  out Int32	    invisibleInnerLineCount,
														  out Int32	    invalidLineCount);
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetSurfaceOutlineValues(Int32		brushID,
														   Int32        surfaceID,
                                                           Int32		vertexCount,
														   IntPtr		vertices,
														   Int32		visibleOuterLineCount,
														   IntPtr		visibleOuterLines,
														   Int32		visibleInnerLineCount,
														   IntPtr		visibleInnerLines,
														   Int32		visibleTriangleCount,
														   IntPtr		visibleTriangleLines,
														   Int32		invisibleOuterLineCount,
														   IntPtr		invisibleOuterLines,
														   Int32		invisibleInnerLineCount,
														   IntPtr		invisibleInnerLines,
														   Int32		invalidLineCount,
														   IntPtr		invalidLines);
		
		private static bool GetSurfaceOutline(Int32			brushID,
											  Int32			surfaceID,
											  ref Vector3[]	vertices,
											  ref Int32[]	visibleOuterLines,
											  ref Int32[]	visibleInnerLines,
											  ref Int32[]	visibleTriangles,
											  ref Int32[]	invisibleOuterLines,
											  ref Int32[]	invisibleInnerLines,
											  ref Int32[]	invalidLines)
		{
			int vertexCount = 0;
			int visibleOuterLineCount = 0;
			int visibleInnerLineCount = 0;
			int visibleTriangleCount = 0;
			int invisibleOuterLineCount = 0;
			int invisibleInnerLineCount = 0;
			int invalidLineCount = 0;
			if (!GetSurfaceOutlineSizes(brushID,
									    surfaceID,
                                        out vertexCount,
										out visibleOuterLineCount,
										out visibleInnerLineCount,
										out visibleTriangleCount,
										out invisibleOuterLineCount,
										out invisibleInnerLineCount,
										out invalidLineCount))
			{
				return false;
			}
			
			if (vertexCount == 0 ||
				(visibleOuterLineCount == 0 && invisibleOuterLineCount == 0 && 
                 visibleInnerLineCount == 0 && invisibleInnerLineCount == 0 &&
				 visibleTriangleCount == 0 &&
				invalidLineCount == 0))
			{
				vertexCount = 0;
				visibleOuterLineCount = 0;
				visibleInnerLineCount = 0;
				visibleTriangleCount = 0;
				invisibleOuterLineCount = 0;
				invisibleInnerLineCount = 0;
				invalidLineCount = 0;
				return false;
			}

            if (vertices == null || vertices.Length != vertexCount)
			{
				vertices = new Vector3[vertexCount];
			}

            if (visibleOuterLineCount > 0 &&
                (visibleOuterLines == null || visibleOuterLines.Length != visibleOuterLineCount))
			{
				visibleOuterLines = new Int32[visibleOuterLineCount];
            }

            if (visibleInnerLineCount > 0 &&
                (visibleInnerLines == null || visibleInnerLines.Length != visibleInnerLineCount))
			{
				visibleInnerLines = new Int32[visibleInnerLineCount];
            }

            if (visibleTriangleCount > 0 &&
                (visibleTriangles == null || visibleTriangles.Length != visibleTriangleCount))
			{
				visibleTriangles = new Int32[visibleTriangleCount];
            }

			if (invisibleOuterLineCount > 0 &&
                (invisibleOuterLines == null || invisibleOuterLines.Length != invisibleOuterLineCount))
			{
				invisibleOuterLines = new Int32[invisibleOuterLineCount];
            }

			if (invisibleInnerLineCount > 0 &&
                (invisibleInnerLines == null || invisibleInnerLines.Length != invisibleInnerLineCount))
			{
				invisibleInnerLines = new Int32[invisibleInnerLineCount];
            }

			if (invalidLineCount > 0 &&
                (invalidLines == null || invalidLines.Length != invalidLineCount))
			{
                invalidLines = new Int32[invalidLineCount];
			}

			GCHandle verticesHandle				= GCHandle.Alloc(vertices, GCHandleType.Pinned);
			GCHandle visibleOuterLinesHandle	= GCHandle.Alloc(visibleOuterLines, GCHandleType.Pinned);
			GCHandle visibleInnerLinesHandle	= GCHandle.Alloc(visibleInnerLines, GCHandleType.Pinned);
			GCHandle visibleTrianglesHandle		= GCHandle.Alloc(visibleTriangles, GCHandleType.Pinned);
			GCHandle invisibleOuterLinesHandle	= GCHandle.Alloc(invisibleOuterLines, GCHandleType.Pinned);
			GCHandle invisibleInnerLinesHandle	= GCHandle.Alloc(invisibleInnerLines, GCHandleType.Pinned);
			GCHandle invalidLinesHandle			= GCHandle.Alloc(invalidLines, GCHandleType.Pinned);

			IntPtr verticesPtr				= verticesHandle.AddrOfPinnedObject();
			IntPtr visibleOuterLinesPtr		= visibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr visibleInnerLinesPtr		= visibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr visibleTrianglesPtr		= visibleTrianglesHandle.AddrOfPinnedObject();
			IntPtr invisibleOuterLinesPtr	= invisibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr invisibleInnerLinesPtr	= invisibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr invalidLinesPtr			= invalidLinesHandle.AddrOfPinnedObject();

			if (!GetSurfaceOutlineValues(brushID,
									     surfaceID,
                                         vertexCount,
									     verticesPtr,
									     visibleOuterLineCount,
										 visibleOuterLinesPtr,
									     visibleInnerLineCount,
										 visibleInnerLinesPtr,
										 visibleTriangleCount,
										 visibleTrianglesPtr,
										 invisibleOuterLineCount,
										 invisibleOuterLinesPtr,
									     invisibleInnerLineCount,
										 invisibleInnerLinesPtr,
                                         invalidLineCount,
										 invalidLinesPtr))
			{
				return false;
			}

			verticesHandle.Free();
			visibleOuterLinesHandle.Free();
			visibleInnerLinesHandle.Free();
			visibleTrianglesHandle.Free();
			invisibleOuterLinesHandle.Free();
			invisibleInnerLinesHandle.Free();
			invalidLinesHandle.Free();
			return true;
		}


        

        
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetTexGenOutlineSizes(Int32		    brushID,
													     Int32		    texGenID,
													     out Int32	    vertexCount,
														 out Int32	    visibleOuterLineCount,
														 out Int32	    visibleInnerLineCount,
														 out Int32	    visibleTriangleCount,
														 out Int32	    invisibleOuterLineCount,
														 out Int32	    invisibleInnerLineCount,
														 out Int32	    invalidLineCount);
		[DllImport(NativePluginName, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool GetTexGenOutlineValues(Int32		brushID,
                                                          Int32		texGenID,
                                                          Int32     vertexCount,
														  IntPtr	vertices,
														  Int32		visibleOuterLineCount,
														  IntPtr	visibleOuterLines,
														  Int32		visibleInnerLineCount,
														  IntPtr	visibleInnerLines,
														  Int32		visibleTriangleCount,
														  IntPtr	visibleTriangles,
														  Int32		invisibleOuterLineCount,
														  IntPtr	invisibleOuterLines,
														  Int32		invisibleInnerLineCount,
														  IntPtr	invisibleInnerLines,
														  Int32		invalidLineCount,
														  IntPtr	invalidLines);
		
		private static bool GetTexGenOutline(Int32			brushID,
											 Int32			texGenID,
											 ref Vector3[]	vertices,
											 ref Int32[]	visibleOuterLines,
											 ref Int32[]	visibleInnerLines,
											 ref Int32[]	visibleTriangles,
											 ref Int32[]	invisibleOuterLines,
											 ref Int32[]	invisibleInnerLines,
											 ref Int32[]	invalidLines)
		{
			int vertexCount = 0;
			int visibleOuterLineCount = 0;
			int visibleInnerLineCount = 0;
			int visibleTriangleCount = 0;
			int invisibleOuterLineCount = 0;
			int invisibleInnerLineCount = 0;
			int invalidLineCount = 0;
			if (!GetTexGenOutlineSizes(brushID,
                                       texGenID,
									   out vertexCount,
									   out visibleOuterLineCount,
									   out visibleInnerLineCount,
									   out visibleTriangleCount,
									   out invisibleOuterLineCount,
									   out invisibleInnerLineCount,
									   out invalidLineCount))
			{
				return false;
			}
			
			if (vertexCount == 0 ||
				(visibleOuterLineCount == 0 && invisibleOuterLineCount == 0 && 
                 visibleInnerLineCount == 0 && invisibleInnerLineCount == 0 &&
				 visibleTriangleCount == 0 &&
				invalidLineCount == 0))
			{
				vertexCount = 0;
				visibleOuterLineCount = 0;
				visibleInnerLineCount = 0;
				visibleTriangleCount = 0;
				invisibleOuterLineCount = 0;
				invisibleInnerLineCount = 0;
                invalidLineCount = 0;
                return false;
			}

            if (vertices == null || vertices.Length != vertexCount)
			{
				vertices = new Vector3[vertexCount];
			}
            

            if (visibleOuterLineCount > 0 &&
                (visibleOuterLines == null || visibleOuterLines.Length != visibleOuterLineCount))
			{
				visibleOuterLines = new Int32[visibleOuterLineCount];
            }

            if (visibleInnerLineCount > 0 &&
                (visibleInnerLines == null || visibleInnerLines.Length != visibleInnerLineCount))
			{
				visibleInnerLines = new Int32[visibleInnerLineCount];
			}

			if (visibleTriangleCount > 0 &&
				(visibleTriangles == null || visibleTriangles.Length != visibleTriangleCount))
			{
				visibleTriangles = new Int32[visibleTriangleCount];
			}

			if (invisibleOuterLineCount > 0 &&
                (invisibleOuterLines == null || invisibleOuterLines.Length != invisibleOuterLineCount))
			{
				invisibleOuterLines = new Int32[invisibleOuterLineCount];
            }

			if (invisibleInnerLineCount > 0 &&
                (invisibleInnerLines == null || invisibleInnerLines.Length != invisibleInnerLineCount))
			{
				invisibleInnerLines = new Int32[invisibleInnerLineCount];
            }

			if (invalidLineCount > 0 &&
                (invalidLines == null || invalidLines.Length != invalidLineCount))
			{
                invalidLines = new Int32[invalidLineCount];
			}

			GCHandle verticesHandle				= GCHandle.Alloc(vertices, GCHandleType.Pinned);
			GCHandle visibleOuterLinesHandle	= GCHandle.Alloc(visibleOuterLines, GCHandleType.Pinned);
			GCHandle visibleInnerLinesHandle	= GCHandle.Alloc(visibleInnerLines, GCHandleType.Pinned);
			GCHandle visibleTrianglesHandle		= GCHandle.Alloc(visibleTriangles, GCHandleType.Pinned);
			GCHandle invisibleOuterLinesHandle	= GCHandle.Alloc(invisibleOuterLines, GCHandleType.Pinned);
			GCHandle invisibleInnerLinesHandle	= GCHandle.Alloc(invisibleInnerLines, GCHandleType.Pinned);
			GCHandle invalidLinesHandle			= GCHandle.Alloc(invalidLines, GCHandleType.Pinned);

			IntPtr verticesPtr				= verticesHandle.AddrOfPinnedObject();
			IntPtr visibleOuterLinesPtr		= visibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr visibleInnerLinesPtr		= visibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr visibleTrianglesPtr		= visibleTrianglesHandle.AddrOfPinnedObject();
			IntPtr invisibleOuterLinesPtr	= invisibleOuterLinesHandle.AddrOfPinnedObject();
			IntPtr invisibleInnerLinesPtr	= invisibleInnerLinesHandle.AddrOfPinnedObject();
			IntPtr invalidLinesPtr			= invalidLinesHandle.AddrOfPinnedObject();

			if (!GetTexGenOutlineValues(brushID,
                                        texGenID,
                                        vertexCount,
									    verticesPtr,
									    visibleOuterLineCount,
										visibleOuterLinesPtr,
									    visibleInnerLineCount,
										visibleInnerLinesPtr,
										visibleTriangleCount,
										visibleTrianglesPtr,
										invisibleOuterLineCount,
										invisibleOuterLinesPtr,
									    invisibleInnerLineCount,
										invisibleInnerLinesPtr,
                                        invalidLineCount,
										invalidLinesPtr))
			{
				return false;
			}

			verticesHandle.Free();
			visibleOuterLinesHandle.Free();
			visibleInnerLinesHandle.Free();
			visibleTrianglesHandle.Free();
			invisibleOuterLinesHandle.Free();
			invisibleInnerLinesHandle.Free();
			invalidLinesHandle.Free();
			return true;
		}
#endregion


        public static void RegisterExternalMethods()
		{
			var methods = new ExternalMethods();
#if DEBUG_API
			methods.ResetCSG    				= delegate () { Debug.Log("ResetCSG"); ResetCSG(); };

			methods.ConvexPartition				= delegate (Vector2[] points) { Debug.Log("ConvexPartition"); return ConvexPartition(points); };

			methods.GenerateModelID				= delegate (Int32 uniqueID, string name, out int generatedModelID, out int generatedNodeID) { Debug.Log("GenerateModelID"); return GenerateModelID(uniqueID, name, out generatedModelID, out generatedNodeID); };
			methods.SetModel					= delegate (Int32 modelID, UInt32 materialCount, bool enabled) { Debug.Log("SetModel"); return SetModel(modelID, materialCount, enabled); };
			methods.SetModelChildren			= delegate (Int32 modelID, Int32 childCount, Int32[] children) { Debug.Log("SetModelChildren"); return SetModelChildren(modelID, childCount, children); };
			methods.UpdateNode					= delegate (Int32 nodeID) { Debug.Log("UpdateNode"); return UpdateNode(nodeID); };
			methods.SetModelEnabled				= delegate (Int32 modelID, bool enabled) { Debug.Log("SetModelEnabled"); return SetModelEnabled(modelID, enabled); };
            methods.RemoveModel					= delegate (Int32 modelID) { Debug.Log("RemoveModel"); return RemoveModel(modelID); };
            methods.RemoveModels				= delegate (Int32[] modelIDs) { Debug.Log("RemoveModels"); return RemoveModels(modelIDs.Length, modelIDs); };

			methods.RayCastIntoModelMulti		= RayCastIntoModelMulti;
            methods.RayCastIntoBrush			= RayCastIntoBrush;
            methods.RayCastIntoBrushSurface		= RayCastIntoBrushSurface;
			methods.GetItemsInFrustum			= GetItemsInFrustum;
            

            methods.GenerateOperationID			= GenerateOperationID;
			methods.SetOperation				= SetOperation;
			methods.SetOperationChildren		= SetOperationChildren;
			methods.SetOperationHierarchy		= SetOperationHierarchy;
			methods.SetOperationOperationType	= SetOperationOperationType;
			methods.RemoveOperation				= RemoveOperation;
			methods.RemoveOperations			= delegate(Int32[] operationIDs) { return RemoveOperations(operationIDs.Length, operationIDs); };
			
			methods.GenerateBrushID				= GenerateBrushID;
			methods.SetBrush					= delegate (Int32 brushID,
													Int32 modelID,
													Int32 parentID,
													CSGOperationType operation,
													UInt32 contentLayer,
													ref Matrix4x4 planeToObjectSpace,
													ref Matrix4x4 objectToPlaneSpace,
													ref Vector3 translation,
													Int32 surfaceCount, Surface[] surfaces,
													Int32 texGenCount, NativeTexGen[] nativeTexgens, TexGenFlags[] texgenFlags) 
													{ Debug.Log("SetBrush"); return SetBrush(brushID,
																					modelID,
																					parentID,
																					operation,
																					contentLayer,
																					ref planeToObjectSpace,
																					ref objectToPlaneSpace,
																					ref translation,
																					surfaceCount, surfaces,
																					texGenCount, nativeTexgens, texgenFlags); };
			methods.SetBrushInfinite			= SetBrushInfinite;
			methods.SetBrushOperationType		= SetBrushOperationType;
			methods.SetBrushTransformation		= SetBrushTransformation;
			methods.SetBrushTranslation			= SetBrushTranslation;
			methods.SetBrushSurfaces			= SetBrushSurfaces;
			methods.SetBrushSurface				= SetBrushSurface;
			methods.SetBrushSurfaceTexGens		= SetBrushSurfaceTexGens;
			methods.SetBrushTexGens				= SetBrushTexGens;
			methods.SetBrushTexGen				= SetBrushTexGen;
			methods.SetBrushTexGenFlags			= SetBrushTexGenFlags;
			methods.SetBrushMesh				= SetBrushMesh;
			methods.SetBrushHierarchy			= SetBrushHierarchy;
			methods.RemoveBrush					= RemoveBrush;
			methods.RemoveBrushes				= delegate (Int32[] brushIDs) { return RemoveBrushes(brushIDs.Length, brushIDs); };
			
			methods.FitSurface					= FitSurface;
			methods.FitSurfaceX					= FitSurfaceX;
			methods.FitSurfaceY					= FitSurfaceY;
			methods.GetSurfaceMinMaxTexCoords	= GetSurfaceMinMaxTexCoords;
			methods.GetSurfaceMinMaxWorldCoord	= GetSurfaceMinMaxWorldCoord;
			methods.ConvertWorldToTextureCoord	= ConvertWorldToTextureCoord;
			methods.ConvertTextureToWorldCoord	= ConvertTextureToWorldCoord;
			methods.GetTexGenMatrices			= GetTexGenMatrices;
			methods.GetTextureToLocalSpaceMatrix = GetTextureToLocalSpaceMatrix;
			methods.GetLocalToTextureSpaceMatrix = GetLocalToTextureSpaceMatrix;

			methods.GetBrushOutlineGeneration	= GetBrushOutlineGeneration;
			methods.GetBrushOutline				= GetBrushOutline;
			methods.GetSurfaceOutline			= GetSurfaceOutline;
			methods.GetTexGenOutline			= GetTexGenOutline;
			
			methods.UpdateModelMeshes			= UpdateModelMeshes;
			methods.ForceModelUpdate			= ForceModelUpdate;
			methods.HasModelChanged				= HasModelChanged;
			methods.GetMeshDescriptions			= GetMeshDescriptions;
			methods.TryUpdatingModelMeshes		= TryUpdatingModelMeshes;
			methods.ModelMeshFinishedUpdating	= ModelMeshFinishedUpdating;
			methods.GetModelSubMeshCount		= GetModelSubMeshCount;
			methods.GetMeshStatus				= GetMeshStatus;
			methods.FillVertices				= FillVertices;
			methods.FillIndices					= FillIndices;
			
			methods.CreateModelMeshes			= CreateModelMeshes;
			methods.GetSubMeshStatus			= GetSubMeshStatus;
			methods.FillVertices				= FillVertices;
			methods.FillIndices					= FillIndices;
#else
			methods.ResetCSG    				= ResetCSG;

			methods.ConvexPartition				= ConvexPartition;

			methods.GenerateModelID				= GenerateModelID;
			methods.SetModel					= SetModel;
			methods.SetModelChildren			= SetModelChildren;
			methods.UpdateNode					= UpdateNode;
			methods.SetModelEnabled				= SetModelEnabled;
            methods.RemoveModel					= RemoveModel;
            methods.RemoveModels				= delegate (Int32[] modelIDs) { return RemoveModels(modelIDs.Length, modelIDs); };

			methods.RayCastIntoModelMulti		= RayCastIntoModelMulti;
            methods.RayCastIntoBrush			= RayCastIntoBrush;
            methods.RayCastIntoBrushSurface		= RayCastIntoBrushSurface;
			methods.GetItemsInFrustum			= GetItemsInFrustum;
            

            methods.GenerateOperationID			= GenerateOperationID;
			methods.SetOperation				= SetOperation;
			methods.SetOperationChildren		= SetOperationChildren;
			methods.SetOperationHierarchy		= SetOperationHierarchy;
			methods.SetOperationOperationType	= SetOperationOperationType;
			methods.RemoveOperation				= RemoveOperation;
			methods.RemoveOperations			= delegate(Int32[] operationIDs) { return RemoveOperations(operationIDs.Length, operationIDs); };
			
			methods.GenerateBrushID				= GenerateBrushID;
			methods.SetBrush					= SetBrush;
			methods.SetBrushInfinite			= SetBrushInfinite;
			methods.SetBrushOperationType		= SetBrushOperationType;
			methods.SetBrushTransformation		= SetBrushTransformation;
			methods.SetBrushTranslation			= SetBrushTranslation;
			methods.SetBrushSurfaces			= SetBrushSurfaces;
			methods.SetBrushSurface				= SetBrushSurface;
			methods.SetBrushSurfaceTexGens		= SetBrushSurfaceTexGens;
			methods.SetBrushTexGens				= SetBrushTexGens;
			methods.SetBrushTexGen				= SetBrushTexGen;
			methods.SetBrushTexGenFlags			= SetBrushTexGenFlags;
			methods.SetBrushMesh				= SetBrushMesh;
			methods.SetBrushHierarchy			= SetBrushHierarchy;
			methods.RemoveBrush					= RemoveBrush;
			methods.RemoveBrushes				= delegate (Int32[] brushIDs) { return RemoveBrushes(brushIDs.Length, brushIDs); };
			
			methods.FitSurface					= FitSurface;
			methods.FitSurfaceX					= FitSurfaceX;
			methods.FitSurfaceY					= FitSurfaceY;
			methods.GetSurfaceMinMaxTexCoords	= GetSurfaceMinMaxTexCoords;
			methods.GetSurfaceMinMaxWorldCoord	= GetSurfaceMinMaxWorldCoord;
			methods.ConvertWorldToTextureCoord	= ConvertWorldToTextureCoord;
			methods.ConvertTextureToWorldCoord	= ConvertTextureToWorldCoord;
			methods.GetTexGenMatrices			= GetTexGenMatrices;
			methods.GetTextureToLocalSpaceMatrix = GetTextureToLocalSpaceMatrix;
			methods.GetLocalToTextureSpaceMatrix = GetLocalToTextureSpaceMatrix;

			methods.GetBrushOutlineGeneration	= GetBrushOutlineGeneration;
			methods.GetBrushOutline				= GetBrushOutline;
			methods.GetSurfaceOutline			= GetSurfaceOutline;
			methods.GetTexGenOutline			= GetTexGenOutline;
			
			methods.UpdateModelMeshes			= UpdateModelMeshes;
			methods.ForceModelUpdate			= ForceModelUpdate;
			methods.HasModelChanged				= HasModelChanged;
			methods.GetMeshDescriptions			= GetMeshDescriptions;
			methods.TryUpdatingModelMeshes		= TryUpdatingModelMeshes;

			methods.ModelMeshFinishedUpdating	= ModelMeshFinishedUpdating;

			methods.CreateModelMeshes			= CreateModelMeshes;
			methods.GetSubMeshStatus			= GetSubMeshStatus;
			methods.FillVertices				= FillVertices;
			methods.FillIndices					= FillIndices;
#endif
			InternalCSGModelManager.External = methods;
		}

        public static void ClearExternalMethods()
		{
			InternalCSGModelManager.External = null;
		}
#endregion
    }
}
#endif