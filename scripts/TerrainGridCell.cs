using UnityEngine;
using System.Collections;

public class TerrainGridCell : Object 
{
	public Terrain terrain;
	public int gridX;
	public int gridY;

	int cellSize;
	TerrainGrid terrainGrid;
	GameObject terrainGameObject;


	public TerrainGridCell(TerrainGrid terrainGrid, int initialX, int initialY, int cellSize)
	{
		this.terrainGrid = terrainGrid;
		this.gridX = initialX;
		this.gridY = initialY;
		this.cellSize = cellSize;

		CreateTerrain();
	}

	void CreateTerrain()
	{
		TerrainData terrainData = new TerrainData();
		terrainData.heightmapResolution = terrainGrid.heightmapResolution;
		terrainData.size = new Vector3(cellSize, cellSize/4, cellSize);
		terrainData.name = "Terrain_"+gridX+"_"+gridY;

		terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
		terrainGameObject.name = "Terrain_"+gridX+"_"+gridY;
		terrainGameObject.transform.parent = terrainGrid.transform;
		terrainGameObject.transform.position = new Vector3(gridX * cellSize, 0, gridY * cellSize);

		terrain = terrainGameObject.GetComponentsInChildren<Terrain>()[0];
		terrain.heightmapPixelError = 1;

		UpdateHeightmap();
	}

	public void ApplyTextures(Texture2D texture0, Texture2D texture1, Texture2D texture2, Texture2D texture3)
	{
		SplatPrototype[] splatPrototypes = new SplatPrototype[4];
		
		splatPrototypes[0] = new SplatPrototype();
		splatPrototypes[0].texture = texture0;
		splatPrototypes[0].tileSize = new Vector2(24,24);
		
		splatPrototypes[1] = new SplatPrototype();
		splatPrototypes[1].texture = texture1;
		splatPrototypes[1].tileSize = new Vector2(24,24);
		
		splatPrototypes[2] = new SplatPrototype();
		splatPrototypes[2].texture = texture2;
		splatPrototypes[2].tileSize = new Vector2(24,24);
		
		splatPrototypes[3] = new SplatPrototype();
		splatPrototypes[3].texture = texture3;
		splatPrototypes[3].tileSize = new Vector2(24,24);

		terrain.terrainData.splatPrototypes = splatPrototypes;

		UpdateSplatmap();
	}

	public void UpdateHeightmap()
	{
		string filename = "height_x";
//		if(gridX < 10) filename += "0";
		filename += gridX.ToString() + "_y";
//		if(gridY < 10) filename += "0";
		filename += gridY.ToString() + ".raw";

		//Debug.Log("UPDATING "+terrain.name+" xy="+gridX+","+gridY+" filename="+filename);

		TerrainUtils.ApplyHeightmap(terrain, terrainGrid.heightmapResolution, "Assets/Resources/Terrains/"+terrainGrid.tileSet+"/"+filename);
	}

	public void UpdateSplatmap()
	{
		string filename = "splat_x";
//		if(gridX < 10) filename += "0";
		filename += gridX.ToString() + "_y";
//		if(gridY < 10) filename += "0";
		filename += gridY.ToString();
		
		string path = "Terrains/"+terrainGrid.tileSet+"/"+filename;
//		string path = "Textures/testsplat";

//		Debug.Log(gridX+","+gridY+" Loading: "+path);

		TerrainUtils.ApplySplatmapDataFromTexture(terrain, Resources.Load(path) as Texture2D);
	}
}
