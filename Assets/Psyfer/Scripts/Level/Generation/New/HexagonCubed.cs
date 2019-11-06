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

	public struct Hexagon
	{
        // Gridding coordinates
		public Vector3Int position;
        public Vector2Int gridPosition;
        public int index;

        // Practical coordinates
        public Vector2 worldPosition;
        public float weight;

        public unsafe Hexagon*[] neighbors;

        public bool LegalPosition()
        {
            return position.x + position.y + position.z == 0;
        }
	}
    
    private Hexagon[] hexagons = new Hexagon[GRID_SIZE * GRID_SIZE];

    private void Initialize()
    {
        float w = Mathf.Sqrt(3);
        float h = 1.5f;
        for (int i = 0; i < hexagons.Length; i++)
        {
            Hexagon hex = hexagons[i];

            // Initialize hexagon
            hex.index = i;
            hex.gridPosition = IndexToGrid(i);
            hex.position = GridToCube(hex.gridPosition);
            hex.worldPosition = GridToWorld(hex.gridPosition, w, h);

            hex.neighbors = GetNeighbours(hex);

            unsafe 
            {
                if(i == 15)
                {
                    foreach(Hexagon* hexigon in hex.neighbors)
                    {
                        hexigon->weight = 1;
                    }
                }
            }

            Assert.IsTrue(hex.LegalPosition());
            hexagons[i] = hex;
        }
    }

    private Vector2 GridToWorld(Vector2Int grid, float width, float height)
    {
        return new Vector2(grid.x * width - (grid.y&1) * (width*0.5f), -grid.y * height);
    }

    private unsafe Hexagon*[] GetNeighbours(Hexagon h)
    {
        fixed(Hexagon* root = &hexagons[0])
        {
            Hexagon*[] neighbours = new Hexagon*[6];

            // Do some magic bound checking shit here

            // Z Axis
            neighbours[0] = root + h.index-1;
            neighbours[3] = root + h.index+1;

            // X Axis
            neighbours[1] = root + h.index - GRID_SIZE - 1;
            neighbours[4] = root + h.index + GRID_SIZE;

            // Y Axis
            neighbours[2] = root + h.index - GRID_SIZE;
            neighbours[5] = root + h.index + GRID_SIZE - 1;

            return neighbours;
        }
    }

    private Vector2Int IndexToGrid(int index)
    {
        return new Vector2Int(index % GRID_SIZE, index / GRID_SIZE);
    }

    private int GridToIndex(Vector2Int grid)
    {
        return grid.x + (grid.y * GRID_SIZE);
    }

    private Vector3Int GridToCube(Vector2Int grid)
    {
        int x = grid.x - (grid.y + (grid.y & 1)) / 2;
        int z = grid.y;
        int y = -x-z;
        return new Vector3Int(x, y, z);
    }
    
    private Vector2Int CubeToGrid(Vector3Int cube)
    {
        int x = cube.x + (cube.z + (cube.z & 1)) / 2;
        int y = cube.z;
        return new Vector2Int(x, y);
    }

    private Vector3Int IndexToCube(int index)
    {
        return GridToCube(IndexToGrid(index));
    }

    private int CubeToIndex(Vector3Int cube)
    {
        return GridToIndex(CubeToGrid(cube));
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
                Color color = Color.Lerp(Color.white, Color.green, h.weight);

                Vector3 first = HexagonCorner(h.worldPosition, 0.95f, 0);
                Vector3 previous = first;
                Vector3 current = first;
                for(int i = 1; i < 6; i++)
                {
                    current = HexagonCorner(h.worldPosition, 0.95f, i);
                    Debug.DrawLine(previous, current, color);
                    #if UNITY_EDITOR
                    if(i==5) Handles.Label(current - Vector3.right * 0.05f + Vector3.up * 0.35f, h.position.y.ToString());
                    if(i==3) Handles.Label(current + Vector3.right * 0.1f - Vector3.up * 0.05f, h.position.x.ToString());
                    if(i==1) Handles.Label(current - Vector3.right * 0.15f - Vector3.up * 0.05f, h.position.z.ToString());
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
