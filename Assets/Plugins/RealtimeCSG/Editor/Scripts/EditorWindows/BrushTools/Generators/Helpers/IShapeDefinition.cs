using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	internal interface IShapeSettings
	{
		void CalculatePlane(ref CSGPlane plane);
		Vector3 GetCenter(CSGPlane plane);
		RealtimeCSG.AABB CalculateBounds(Quaternion rotation, Vector3 gridTangent, Vector3 gridBinormal);
	}
}
