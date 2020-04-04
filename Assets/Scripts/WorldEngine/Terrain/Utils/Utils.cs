
using UnityEngine;
public class Utils {
  
  public static string getChunkName(int chunkX, int chunkY) {
    return chunkX + "-" + chunkY;
  }
  public static int getIndexFrom2DArray(int  arrayLength, int x, int y) {
    return (int) Mathf.Sqrt(arrayLength) * x + y;
  }
}