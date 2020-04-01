using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class RiverEngine
{
    public float LAKE_THRESHOLD = 0.1f;
    public float MOUNTAIN_THRESHOLD = 0.9f;
    public float MAX_RIVER_HEIGHT = 0.8f;
    public float PATH_PRECISION = 0.5f;
    public int RIVER_WIDTH = 10;
    public float RIVER_DEPTH = 0.05f;
    public float MAX_RIVER_DISTANCE = 250f;
    public float[,] generateRivers(float[,] noiseMap) {
        List<Vector2> mountainPoints = new List<Vector2>();
        List<Vector2> lakePoints = new List<Vector2>();

        Dictionary<Vector2, List<Vector2>> lakeClusters = new Dictionary<Vector2, List<Vector2>>();
        Dictionary<Vector2, List<Vector2>> mountainClusters = new Dictionary<Vector2, List<Vector2>>();

        // identify lakes via k means clustering for the lowest points
        lakePoints = getPointsAtThreshold(noiseMap, LAKE_THRESHOLD, "<");
        lakeClusters = KMeansClustering.cluster(50, null, lakePoints, noiseMap.GetLength(0), 0);

        // identify mountains via k means clustering for the highest points
        mountainPoints = getPointsAtThreshold(noiseMap, MOUNTAIN_THRESHOLD, ">");
        mountainClusters = KMeansClustering.cluster(50, null, mountainPoints, noiseMap.GetLength(0), 0);

        // find the shortest distance between each node
        // add a connection and generate the river based
        // on that connection
        List<Vector2[]> connections = new List<Vector2[]>();
        for (var i = 0; i < mountainClusters.Count; i++) {
            Vector2 currentMountain = mountainClusters.Keys.ElementAt(i);
            Vector2 candidate = Vector2.zero;
            float shortestDistance = float.MaxValue;

            for (var j = 0; j < lakeClusters.Count; j++) {
                Vector2 currentLake = lakeClusters.Keys.ElementAt(j);
                float distance = Vector2.Distance(currentMountain, currentLake);
                if (distance < shortestDistance) {
                    shortestDistance = distance;
                    candidate = currentLake;
                }
            }

            connections.Add(new Vector2[]{ currentMountain, candidate});
        }

        // get the path of the river
        Dictionary<string, bool?> previouslyVisited = new Dictionary<string, bool?>(); 
        foreach(var connection in connections) {
            // index as to how far we've traveled along the river
            float riverPathDistance = 0f;
            var distance = Vector2.Distance(connection[0], connection[1]);
            
            if (distance > MAX_RIVER_DISTANCE) {
                continue;
            }
            
            // while we are still traversing along the route
            while(riverPathDistance < distance) {

                // get the current position based on how far we've traveled
                var midpoint = KMeansClustering.LerpByDistance(connection[0], connection[1], riverPathDistance);
                var x = (int) midpoint.x;
                var y = (int) midpoint.y;

                // do it over the course of the width of the river
                var previousHeight = noiseMap[x, y];

                // above a certain height, we don't want rivers generating
                if (previousHeight > MAX_RIVER_HEIGHT) {
                    riverPathDistance += PATH_PRECISION;
                    continue;
                }

                // carve the depth into the river
                var newHeight = noiseMap[x, y] - RIVER_DEPTH;
                noiseMap[x, y] = newHeight;

                // make sure we don't hit this point again to avoid 0 or 1 absolutes
                if (previouslyVisited.ContainsKey((x) + "-" + (y)) == false) {
                    previouslyVisited.Add((x) + "-" + (y), true);
                }
                

                // traverse to the sides of the point and set the depth accordingly
                // slowly progressing higher and higher to make it bowl
                for (var i = 0; i < RIVER_WIDTH; i++) {
                    for (var j = 0; j < RIVER_WIDTH; j++) {

                        // only visit each point one time, with precision, its possible to visit points twice
                        if(previouslyVisited.ContainsKey((x + i) + "-" + (y + j))) {
                            continue;
                        } else {
                            previouslyVisited.Add((x + i) + "-" + (y + j), true);
                        }

                        // get the previous height and lerp it based on how far from the center we are
                        previousHeight = noiseMap[x + i, y + j];
                        var sideNewHeight = Mathf.Lerp(previousHeight - RIVER_DEPTH, previousHeight, i / RIVER_WIDTH);
                        noiseMap[x + i, y + j] = sideNewHeight;
                    }
                }

                // update the index so we keep moving along the path
                riverPathDistance += PATH_PRECISION;
            }
            // get ready for next connection
            previouslyVisited.Clear();
        }
        return noiseMap;
    }

    List<Vector2> getPointsAtThreshold (float[,] noiseMap, float threshold, string comparision) {
        List<Vector2> points = new List<Vector2>();
        for (var i = 0; i < noiseMap.GetLength(0); i++) {
            for (var j = 0; j < noiseMap.GetLength(1); j++) {
                if (comparision == ">" && noiseMap[i, j] >= threshold) {
                    points.Add(new Vector2(i, j));
                }

                 if (comparision == "<" && noiseMap[i, j] <= threshold) {
                    points.Add(new Vector2(i, j));
                }
            }
        }

        return points;
    }
}
