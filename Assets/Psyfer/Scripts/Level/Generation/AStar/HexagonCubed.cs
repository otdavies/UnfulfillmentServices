using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Assertions;

public class HexagonCubed : MonoBehaviour 
{
    private const int GRID_SIZE = 11;
    
    private Hexagon[] hexagons = new Hexagon[GRID_SIZE * GRID_SIZE];

    private void Initialize()
    {
        float w = Mathf.Sqrt(3);
        float h = 1.5f;

        // Setup all hexigons
        for (int i = 0; i < hexagons.Length; i++)
        {
            hexagons[i] = new Hexagon(i, h, w, GRID_SIZE);
        }

        // Find neighbours
        for (int i = 0; i < hexagons.Length; i++)
        {
            hexagons[i].DetermineNeighbours(hexagons);
            hexagons[i].FindLowestWeightNeighbour();
        }
    }

	private void Awake () 
	{
        Initialize();
	}
	
	private void Update () 
	{
		
	}

    // Draw hexagon cube
    private void OnDrawGizmos() 
    {
        if(hexagons != null && hexagons.Length > 0)
        {
            foreach (Hexagon h in hexagons)
            {
                if(h == null) return;

                Color color = Color.Lerp(Color.white, Color.green, h.weight);

                Vector3 first = HexagonCorner(h.worldPosition, 0.95f, 0);
                Vector3 previous = first;
                Vector3 current = first;
                for(int i = 1; i < 6; i++)
                {
                    current = HexagonCorner(h.worldPosition, 0.95f, i);
                    Debug.DrawLine(previous, current, color);
                    #if UNITY_EDITOR
                    if(i==5) Handles.Label(current - Vector3.right * 0.05f + Vector3.up * 0.35f, h.cubePosition.y.ToString());
                    if(i==3) Handles.Label(current + Vector3.right * 0.1f - Vector3.up * 0.05f, h.cubePosition.x.ToString());
                    if(i==1) Handles.Label(current - Vector3.right * 0.15f - Vector3.up * 0.05f, h.cubePosition.z.ToString());
                    #endif
                    previous = current;
                }
                Debug.DrawLine(current, first, color);
            }
        }
    }

    private Vector3 HexagonCorner(Vector2 center, float size, int index)
    {
        int angle_deg = 60 * index - 30;
        float angle_rad = angle_deg * Mathf.Deg2Rad;
        return new Vector3(center.x + size * Mathf.Cos(angle_rad),center.y + size * Mathf.Sin(angle_rad));
    }
}
