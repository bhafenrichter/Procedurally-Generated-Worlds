using UnityEngine;
using System;
public class NoiseMapRenderer : MonoBehaviour
{
  public int COLOR_PADDING = 1;
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
    for (int zIndex = 0; zIndex < tileDepth; zIndex++) {
      for (int xIndex = 0; xIndex < tileWidth; xIndex++) {
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

    // average out the colors compared to its neighbors
    for (int zIndex = 0; zIndex < tileDepth; zIndex++) {
      for (int xIndex = 0; xIndex < tileWidth; xIndex++) {
        float redAvg = 0;
        float greenAvg = 0;
        float blueAvg = 0;

        for (var i = 0; i < COLOR_PADDING; i++) {
          try {
            addColor(colorMap[(zIndex) * tileWidth + (xIndex)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex + COLOR_PADDING) * tileWidth + (xIndex)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex - COLOR_PADDING) * tileWidth + (xIndex)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex) * tileWidth + (xIndex + COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex) * tileWidth + (xIndex - COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex - COLOR_PADDING) * tileWidth + (xIndex - COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex + COLOR_PADDING) * tileWidth + (xIndex + COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex - COLOR_PADDING) * tileWidth + (xIndex + COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);
            addColor(colorMap[(zIndex + COLOR_PADDING) * tileWidth + (xIndex - COLOR_PADDING)], ref redAvg, ref greenAvg, ref blueAvg);

            colorMap[(zIndex) * tileWidth + (xIndex)] = new Color(redAvg / 9, greenAvg / 9, blueAvg / 9, 1);
          } catch (Exception e) {}
        }
      }
    }
    
    return colorMap;
  }

  private void addColor(Color color, ref float red, ref float green, ref float blue) {
    red += color.r;
    green += color.g;
    blue += color.b;
  }
}

[System.Serializable]
public struct TerrainType {
    public float threshold;
    public string name;
    public Color color;
}