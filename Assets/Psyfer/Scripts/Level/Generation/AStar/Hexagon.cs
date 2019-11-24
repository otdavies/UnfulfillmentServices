using UnityEngine;
using UnityEngine.Assertions;

public class Hexagon
	{
        // Gridding coordinates
		public int 			arrayPosition;
        public Vector2Int 	gridPosition;
		public Vector3Int 	cubePosition;

        // Practical coordinates
        public Vector2 worldPosition;

		// weighted elements
        public float weight;

        // Internal
		private int gridSize;

        public Hexagon[] neighbours = new Hexagon[6];

        public Hexagon(int index, float height, float width, int gridSize)
        {
            // Initialize hexagon
            this.arrayPosition = index;
            this.gridPosition = IndexToGrid(index, gridSize);
            this.cubePosition = GridToCube(this.gridPosition);
            this.worldPosition = GridToWorld(this.gridPosition, width, height);
			this.gridSize = gridSize;
			this.weight = Random.Range(0f, 1f);
			this.neighbours = new Hexagon[6];

            Assert.IsTrue(this.LegalPosition());
        }

        public Hexagon FindLowestWeightNeighbour()
        {
            Hexagon best = null;
            float weight = float.MaxValue;

			Assert.IsNotNull(neighbours);

            for(int i = 0; i < neighbours.Length; i++)
            {
				Hexagon n = neighbours[i];
				if(n == null) continue;

                float w = n.weight;
                if(w < weight) 
                {
                    weight = w;
                    best = n;
                }
            }
			if(best != null) this.weight = best.weight;
            return best;
        }

		public bool LegalPosition()
        {
            return cubePosition.x + cubePosition.y + cubePosition.z == 0;
        }
        
        public void DetermineNeighbours(Hexagon[] grid)
        {
            // Z Axis
            neighbours[0] = LegalNeighbour(grid, arrayPosition-1);
            neighbours[3] = LegalNeighbour(grid, arrayPosition+1);

            // X Axis
            neighbours[1] = LegalNeighbour(grid, arrayPosition - gridSize-1);
            neighbours[4] = LegalNeighbour(grid, arrayPosition + gridSize);

            // Y Axis
            neighbours[2] = LegalNeighbour(grid, arrayPosition - gridSize);
            neighbours[5] = LegalNeighbour(grid, arrayPosition + gridSize-1);
        }
		
		private Hexagon LegalNeighbour(Hexagon[] grid, int i)
		{
			if(i < 0 || i > grid.Length - 1) return null;
			return grid[i];
		}


		// STATIC --------------------------------------------------------------------------------

        public static Vector2 GridToWorld(Vector2Int grid, float width, float height)
        {
            return new Vector2(grid.x * width - (grid.y&1) * (width*0.5f), -grid.y * height);
        }

        public static Vector2Int IndexToGrid(int index, int gridSize)
        {
            return new Vector2Int(index % gridSize, index / gridSize);
        }

        public static int GridToIndex(Vector2Int grid , int gridSize)
        {
            return grid.x + (grid.y * gridSize);
        }

        public static Vector3Int GridToCube(Vector2Int grid)
        {
            int x = grid.x - (grid.y + (grid.y & 1)) / 2;
            int z = grid.y;
            int y = -x-z;
            return new Vector3Int(x, y, z);
        }
        
        public static Vector2Int CubeToGrid(Vector3Int cube)
        {
            int x = cube.x + (cube.z + (cube.z & 1)) / 2;
            int y = cube.z;
            return new Vector2Int(x, y);
        }

        public static Vector3Int IndexToCube(int index, int gridSize)
        {
            return GridToCube(IndexToGrid(index, gridSize));
        }

        public static int CubeToIndex(Vector3Int cube, int gridSize)
        {
            return GridToIndex(CubeToGrid(cube), gridSize);
        }
	}