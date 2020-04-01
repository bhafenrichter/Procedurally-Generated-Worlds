using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class KMeansClustering {
  
    public static float MIN_NONDUPLICATE_DISTANCE = 20f;
  public static Dictionary<Vector2, List<Vector2>> cluster(int k, Dictionary<Vector2, List<Vector2>> clusterPoints, List<Vector2> points, int mapSize, int attempts) {
    bool hasShifted = false;
    
    // first iteration
    if (clusterPoints == null) {
      clusterPoints = new Dictionary<Vector2, List<Vector2>>();

      // generate random points to start the clusters
      for (var i = 0; i < k; i++) {
        float randX = UnityEngine.Random.Range(0, mapSize);
        float randY = UnityEngine.Random.Range(0, mapSize);
        clusterPoints.Add(new Vector2(randX, randY), new List<Vector2>());
      }
    }

    // iterate through points and assign them to the correct (closest) cluster
    int changeCount = 0;
    for (var i = 0; i < points.Count; i++) {
      Vector2 closestVector = Vector2.zero;
      float shortestDistance = float.MaxValue;
      Vector2 currentPoint = points[i];

      foreach(var item in clusterPoints)
      {
        Vector2 currentClusterPoint = item.Key;
        float distance = Vector2.Distance(currentPoint, currentClusterPoint);

        if (distance < shortestDistance) {
          hasShifted = true;
          closestVector = currentClusterPoint;
          shortestDistance = distance;
          changeCount++;
        }
      }


      try {
        clusterPoints[closestVector].Add(currentPoint);
      } catch(Exception e) {
        // Debug.Log("Bad Vector: " + closestVector);
      }
      
    }

    // iterate through the clusters and average out based on current subset
    Dictionary<Vector2, List<Vector2>> newClusters = new Dictionary<Vector2, List<Vector2>>();
    foreach(var item in clusterPoints) {
      Vector2 sum = Vector2.zero;
      List<Vector2> clusterValues = item.Value;
      for (var i = 0; i < clusterValues.Count; i++) {
        sum += clusterValues[i];
      }

      if (clusterValues.Count != 0) {
        sum /= clusterValues.Count;
        newClusters.Add(sum, new List<Vector2>());
      }
    }

    attempts++;

    if (hasShifted && attempts < 100) {
      return cluster(k, newClusters, points, mapSize, attempts);
    } else {
      newClusters = removeDuplicateClusters(newClusters);
      return newClusters;
    }
  }
  public static Dictionary<Vector2, List<Vector2>> removeDuplicateClusters(Dictionary<Vector2, List<Vector2>> unSanitizedClusters) {
    Dictionary<Vector2, List<Vector2>> sanitizedClusters = new Dictionary<Vector2, List<Vector2>>();

    foreach(var clusterToAdd in unSanitizedClusters) {
      bool isDuplicate = false;
      foreach(var clusterToTest in sanitizedClusters) {
        if (Vector2.Distance(clusterToTest.Key, clusterToAdd.Key) < MIN_NONDUPLICATE_DISTANCE) {
          isDuplicate = true;
        }
      }

      if (isDuplicate == false) {
        sanitizedClusters.Add(clusterToAdd.Key, clusterToAdd.Value);
      }
    }

    return sanitizedClusters;
  }

  public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
  {
      Vector3 P = x * Vector3.Normalize(B - A) + A;
      return P;
  }

}