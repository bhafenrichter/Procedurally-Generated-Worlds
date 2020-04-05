
using UnityEngine;
using System.Collections.Generic;
public class Utils
{

  public static string getChunkName(int chunkX, int chunkY)
  {
    return chunkX + "-" + chunkY;
  }
  public static int getIndexFrom2DArray(int arrayLength, int x, int y)
  {
    return (int)Mathf.Sqrt(arrayLength) * x + y;
  }

  public static List<Vector2> getPointsAtThreshold(float[,] noiseMap, float threshold, string comparision)
  {
    List<Vector2> points = new List<Vector2>();
    for (var i = 0; i < noiseMap.GetLength(0); i++)
    {
      for (var j = 0; j < noiseMap.GetLength(1); j++)
      {
        if (comparision == ">" && noiseMap[i, j] >= threshold)
        {
          points.Add(new Vector2(i, j));
        }

        if (comparision == "<" && noiseMap[i, j] <= threshold)
        {
          points.Add(new Vector2(i, j));
        }
      }
    }

    return points;
  }
}