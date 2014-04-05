using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainUtils : MonoBehaviour 
{
	/// Applies 16 bit raw height data to a terrain.
	public static void ApplyHeightmap(Terrain terrain, int heightmapResolution, string heightmapPath)
	{
		byte[] bytes = GetFileBytes(heightmapResolution*heightmapResolution, heightmapPath);
		float[,] heights = GetHeightsFromFileBytes(heightmapResolution, bytes);
		terrain.terrainData.SetHeights(0, 0, heights);
	}


	/// Gets binary data from a file.
	public static byte[] GetFileBytes(int size, string path)
	{
		byte[] bytes = new byte[size*2];
		
		// Initialize byte reader
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
		
		// Read bytes
		binaryReader.Read(bytes, 0, size*2);
		
		// Close
		binaryReader.Close();
		fileStream.Close();
		
		// Return
		return bytes;
	}


	/// Gets height 16 bit raw height data from the passed binary data.
	public static float[,] GetHeightsFromFileBytes(int heightmapResolution, byte[] bytes)
	{
		int i = 0;
		float[,] heights = new float[heightmapResolution,heightmapResolution];
		
		for(int x = 0; x < heightmapResolution; ++x)
			for(int y = 0; y < heightmapResolution; ++y)
				//heights[heightmapResolution-1-x,y] = (bytes[i++] * 256.0f + bytes[i++]) / 65535.0f;
				heights[heightmapResolution-1-x, y] = (bytes[i++] + bytes[i++]*256) / 65535.0f;
		
		return heights;
	}


	public static float[,,] ApplySplatmapDataFromTexture(Terrain terrain, Texture2D texture)
	{
		int width = texture.width;
		Color[] pixels = texture.GetPixels();
		float[,,] output = terrain.terrainData.GetAlphamaps(0, 0, width, width);

		// Loop through each channel (RGBA) on each pixel
		for (int z = 0; z < 4; z++)
			for (int y = 0; y < width; y++)
				for (int x = 0; x < width; x++)
					output[x,y,z] = pixels[((width-1)-x)*width + y][z];

		// Set splat maps
		terrain.terrainData.SetAlphamaps(0, 0, output);

		return output;
	}
	
	
	/// Shifts all terrain cells DOWN, wrapping the bottom-most row to the TOP
	public static void WrapTerrainGridDown(int numCellsWide, TerrainGridCell[,] grid)
	{
		TerrainGridCell[] firstRow = new TerrainGridCell[numCellsWide];
		
		// Save first row
		for(int x = 0; x < numCellsWide; x++)
			firstRow[x] = grid[x, 0];
		
		// Select and shift all but last row
		for(int x = 0; x < numCellsWide; x++)
			for(int y = 0; y < numCellsWide-1; y++)
				grid[x,y] = grid[x, y+1];
		
		// Replace last row with original first row
		for(int x = 0; x < numCellsWide; x++)
			grid[x, numCellsWide-1] = firstRow[x];
	}


	/// Shifts all terrain cells UP, wrapping the top-most row to the BOTTOM
	public static void WrapTerrainGridUp(int numCellsWide, TerrainGridCell[,] grid)
	{
		TerrainGridCell[] lastRow = new TerrainGridCell[numCellsWide];
		
		// Save last row
		for(int x = 0; x < numCellsWide; x++)
			lastRow[x] = grid[x, numCellsWide-1];
		
		// Select and shift all but first row
		for(int x = 0; x < numCellsWide; x++)
			for(int y = numCellsWide-1; y >= 1; y--)
				grid[x,y] = grid[x, y-1];
		
		// Replace first row with original last row
		for(int x = 0; x < numCellsWide; x++)
			grid[x, 0] = lastRow[x];
	}

	
	/// Shifts all terrain cells LEFT, wrapping the left-most row to the far RIGHT
	public static void WrapTerrainGridLeft(int numCellsWide, TerrainGridCell[,] grid)
	{
		TerrainGridCell[] firstColumn = new TerrainGridCell[numCellsWide];
		
		// Save first column
		for(int y = 0; y < numCellsWide; y++)
			firstColumn[y] = grid[0, y];
		
		// Select and shift all but last column
		for(int x = 0; x < numCellsWide-1; x++)
			for(int y = 0; y < numCellsWide; y++)
				grid[x,y] = grid[x+1, y];
		
		// Replace last column with original first column
		for(int y = 0; y < numCellsWide; y++)
			grid[numCellsWide-1, y] = firstColumn[y];
	}


	/// Shifts all terrain cells RIGHT, wrapping the top-most row to the far LEFT
	public static void WrapTerrainGridRight(int numCellsWide, TerrainGridCell[,] grid)
	{
		TerrainGridCell[] lastColumn = new TerrainGridCell[numCellsWide];
		
		// Save last column
		for(int y = 0; y < numCellsWide; y++)
			lastColumn[y] = grid[numCellsWide-1, y];
		
		// Select and shift all but first column
		for(int y = 0; y < numCellsWide; y++)
			for(int x = numCellsWide-1; x >= 1; x--)
				grid[x,y] = grid[x-1, y];
		
		// Replace first column with original last column
		for(int y = 0; y < numCellsWide; y++)
			grid[0, y] = lastColumn[y];
	}


	/// Debug utility function that will output the structured contents of the terrain grid to the output window.
	public static void TraceTerrainGrid(int numCellsWide, TerrainGridCell[,] grid)
	{
		string finalOutput = "";
		
		for(int y = numCellsWide-1; y >= 0; y--)
		{
			string output = "";
			
			for(int x = 0; x < numCellsWide; x++)
				output += "{"+grid[x,y].terrain.name+" " +grid[x,y].gridX+","+grid[x,y].gridY+"} ";
			
			finalOutput += output+"\n";
		}
		Debug.Log(finalOutput);
	}
}
