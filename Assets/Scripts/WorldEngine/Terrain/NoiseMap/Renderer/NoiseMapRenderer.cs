using UnityEngine;

public class NoiseMapRenderer : MonoBehaviour
{
  internal Texture2D BuildTexture(TerrainType[] terrainConfigs, AnimationCurve heightCurve, float[,] heightMap)
  {
    int tileDepth = heightMap.GetLength(0);
    int tileWidth = heightMap.GetLength(1);

    Color[] colorMap = BuildPixelData(terrainConfigs, heightCurve, heightMap);

    // create a new texture and set it  s pixel colors
    Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
    tileTexture.filterMode = FilterMode.Point;
    tileTexture.wrapMode = TextureWrapMode.Clamp;
    tileTexture.SetPixels(colorMap);
    tileTexture.Apply();
    return tileTexture;
  }

  internal Color[] BuildPixelData(TerrainType[] terrainConfigs, AnimationCurve heightCurve, float[,] heightMap) {
    
    int tileDepth = heightMap.GetLength(0);
    int tileWidth = heightMap.GetLength(1);

    Color[] colorMap = new Color[tileDepth * tileWidth];
    for (int zIndex = 0; zIndex < tileDepth; zIndex++)
    {
      for (int xIndex = 0; xIndex < tileWidth; xIndex++)
      {
        // transform the 2D map index is an Array index
        int colorIndex = zIndex * tileWidth + xIndex;
        float height = heightCurve.Evaluate(heightMap[zIndex, xIndex]);
        // assign as color a shade of grey proportional to the height value
        for (var i = 0; i < terrainConfigs.Length; i++) {
          if (terrainConfigs[i].threshold >= height) {
            colorMap[colorIndex] = terrainConfigs[i].color;
            break;
          }
        }

        // colorMap[colorIndex] = Color.Lerp(Color.white, Color.black, heightCurve.Evaluate(heightMap[zIndex, xIndex]));
      }
    }
    
    return colorMap;
  }
}

[System.Serializable]
public struct TerrainType {
    public float threshold;
    public string name;
    public Color color;
}