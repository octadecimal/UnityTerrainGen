using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGenerator : MonoBehaviour 
{
	public int heightmapResolution = 513;
	public string terrain = "Desert";

	void Start () 
	{
		ApplyHeightmap(Terrain.activeTerrain, "Assets/Textures/Terrains/"+terrain+"/height.raw");
	}


	void ApplyHeightmap(Terrain terrain, string heightmapPath)
	{
		float[,] heights = GetHeightsFromFileBytes(GetFileBytes(heightmapPath));

		terrain.terrainData.SetHeights(0, 0, heights);
	}


	byte[] GetFileBytes(string path)
	{
		byte[] bytes = new byte[heightmapResolution*heightmapResolution*2];

		// Initialize byte reader
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

		// Read bytes
		binaryReader.Read(bytes, 0, heightmapResolution*heightmapResolution*2);

		// Close
		binaryReader.Close();
		fileStream.Close();

		// Return
		return bytes;
	}


	float[,] GetHeightsFromFileBytes(byte[] bytes)
	{
		int i = 0;
		float[,] heights = new float[heightmapResolution,heightmapResolution];

		for(int x = 0; x < heightmapResolution; ++x)
		{
			for(int y = 0; y < heightmapResolution; ++y)
			{
				//heights[heightmapResolution-1-x,y] = (bytes[i++] * 256.0f + bytes[i++]) / 65535.0f;
				heights[heightmapResolution-1-x, y] = (bytes[i++] + bytes[i++]*256) / 65535.0f;
			}
		}

		return heights;
	}
}
