using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGrid : MonoBehaviour 
{
	public string tileSet = "Desert4";		// Name of tileset to use
	public int minimumVisibleSize = 1536;	// Minimum visible terrain area size (derived from n*cellSize)
	public int cellSize = 512;				// Terrain cell size
	public int heightmapResolution = 513;	// Heightmap resolution
	
	public Texture2D color0;
	public Texture2D color1;
	public Texture2D color2;
	public Texture2D color3;

	private TerrainGridCell[,] grid;		// Terrain grid

	private int visibleSize;				// The grid's visible size
	private int numCellsWide;				// Number of cells along one direction in grid
	private int center;						// The index of the center cell
	private int x = 1;						// The index of the current x position in grid space.
	private int y = 1;						// The index of the current y position in grid space.

	private GameObject player;
	
	
	/// Use this for initialization
	void Start() 
	{
		DeriveVisibleSize();
		InitializeGrid();
		ApplyTextures();

		transform.position = new Vector3(-visibleSize/2, 0, -visibleSize/2);

		player = GameObject.FindGameObjectWithTag("Player");

		player.transform.position = new Vector3(0, 256, 0);
	}
	
	
	/// Derives the size and number of cells to satisfy the passed `minimumVisibleSize` and `cellSize`
	void DeriveVisibleSize()
	{
		// Derive number of cells to fit along per dimensions
		numCellsWide = (int)(Mathf.Ceil((float)minimumVisibleSize/(float)cellSize));

		// Enforce odd number of cells wide
		if (numCellsWide % 2 == 0)
			numCellsWide += 1;

		// Derive visible size
		visibleSize = numCellsWide * cellSize;

		// Derive center index
		center = numCellsWide / 2;

		// Debug
		Debug.Log("Visible size set: " + visibleSize + "x" + visibleSize + " -> " + numCellsWide + "x" + numCellsWide);
	}


	/// Initializes the terrain grid
	void InitializeGrid()
	{
		grid = new TerrainGridCell[numCellsWide, numCellsWide];

		for(int r = 0; r < numCellsWide; r++)
		{
			for(int c = 0; c < numCellsWide; c++)
			{
				grid[c,r] = new TerrainGridCell(this, c, r, cellSize);
			}
		}
	}


	// Applies textures to all grid contents.
	void ApplyTextures()
	{
		for(int x = 0; x < grid.GetLength(0); x++)
			for(int y = 0; y < grid.GetLength(1); y++)
				grid[x,y].ApplyTextures(color0, color1, color2, color3);
	}


	/// Update is called once per frame
	void Update() 
	{
		if(player.transform.position.z > cellSize)
			MoveUp();
		
		if(player.transform.position.z < -cellSize)
			MoveDown();
		
		if(player.transform.position.x > cellSize)
			MoveRight();
		
		if(player.transform.position.x < -cellSize)
			MoveLeft();
	}


	/// Moves the grid up
	void MoveUp()
	{
		if(y >= 24 - numCellsWide + center - 1)
			return;

		y = Mathf.Min(y+1, 99);

		for(int r = 0; r < numCellsWide; r++)
		{
			for(int c = 0; c < numCellsWide; c++)
			{
				TerrainGridCell currentCell = grid[r,c];

				// Normal cells, only offset
				if(c > 0)
				{
					currentCell.terrain.transform.Translate(0, 0, -cellSize);
				}
				// Bottom cells, swap to top
				else
				{
					currentCell.terrain.transform.Translate(0, 0, cellSize*(numCellsWide-1));
					currentCell.gridY += numCellsWide;

					currentCell.UpdateHeightmap();
					currentCell.UpdateSplatmap();
				}
			}
		}

		TerrainUtils.WrapTerrainGridDown(numCellsWide, grid);

		player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z-cellSize);
	}
	
	/// Moves the grid down
	void MoveDown()
	{
		if(y <= center-1)
			return;

		y = Mathf.Max(y-1, 0);

		for(int r = 0; r < numCellsWide; r++)
		{
			for(int c = 0; c < numCellsWide; c++)
			{
				TerrainGridCell currentCell = grid[r,c];
				
				// Normal cells, only offset
				if(c < numCellsWide-1)
				{
					currentCell.terrain.transform.Translate(0, 0, cellSize);
				}
				// Bottom cells, swap to top
				else
				{
					currentCell.terrain.transform.Translate(0, 0, -cellSize*(numCellsWide-1));
					currentCell.gridY -= numCellsWide;
					
					currentCell.UpdateHeightmap();
					currentCell.UpdateSplatmap();
				}
			}
		}
		
		TerrainUtils.WrapTerrainGridUp(numCellsWide, grid);
		
		player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z+cellSize);
	}
	
	/// Moves the grid right
	void MoveRight()
	{
		x = Mathf.Min(x+1, 99);

		for(int r = 0; r < numCellsWide; r++)
		{
			for(int c = 0; c < numCellsWide; c++)
			{
				TerrainGridCell currentCell = grid[r,c];
				
				// Normal cells, only offset
				if(r > 0)
				{
					currentCell.terrain.transform.Translate(-cellSize, 0, 0);
				}
				// Bottom cells, swap to top
				else
				{
					currentCell.terrain.transform.Translate(cellSize*(numCellsWide-1), 0, 0);
					currentCell.gridX += numCellsWide;
					
					currentCell.UpdateHeightmap();
					currentCell.UpdateSplatmap();
				}
			}
		}
		
		TerrainUtils.WrapTerrainGridLeft(numCellsWide, grid);

		player.transform.position = new Vector3(player.transform.position.x-cellSize, player.transform.position.y, player.transform.position.z);
	}
	
	/// Moves the grid left
	void MoveLeft()
	{
		x = Mathf.Max(x-1, 0);

		for(int r = 0; r < numCellsWide; r++)
		{
			for(int c = 0; c < numCellsWide; c++)
			{
				TerrainGridCell currentCell = grid[r,c];
				
				// Normal cells, only offset
				if(r < numCellsWide-1)
				{
					currentCell.terrain.transform.Translate(cellSize, 0, 0);
				}
				// Bottom cells, swap to top
				else
				{
					currentCell.terrain.transform.Translate(-cellSize*(numCellsWide-1), 0, 0);
					currentCell.gridX -= numCellsWide;
					
					currentCell.UpdateHeightmap();
					currentCell.UpdateSplatmap();
				}
			}
		}
		
		TerrainUtils.WrapTerrainGridRight(numCellsWide, grid);
		
		player.transform.position = new Vector3(player.transform.position.x+cellSize, player.transform.position.y, player.transform.position.z);
	}
}
