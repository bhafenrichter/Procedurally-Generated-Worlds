using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class RiverEngine
{
    public float LAKE_THRESHOLD = 0.1f;
    public float MOUNTAIN_THRESHOLD = 0.9f;
    public float[,] generateRivers(float[,] noiseMap) {
        List<Vector2> mountainPoints = new List<Vector2>();
        List<Vector2> lakePoints = new List<Vector2>();
        Dictionary<Vector2, List<Vector2>> lakeClusters = new Dictionary<Vector2, List<Vector2>>();
        Dictionary<Vector2, List<Vector2>> mountainClusters = new Dictionary<Vector2, List<Vector2>>();

        // identify lakes
        lakePoints = getPointsAtThreshold(noiseMap, LAKE_THRESHOLD, "<");
        lakeClusters = KMeansClustering.cluster(50, null, lakePoints, noiseMap.GetLength(0), 0);

        // identify mountains
        mountainPoints = getPointsAtThreshold(noiseMap, MOUNTAIN_THRESHOLD, ">");
        mountainClusters = KMeansClustering.cluster(50, null, mountainPoints, noiseMap.GetLength(0), 0);

        // find the shortest distance between each node
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
        float precision = 0.1f;
        float riverWidth = 5f;
        float riverDepth = 0.05f;
        Dictionary<string, bool?> previouslyVisited = new Dictionary<string, bool?>(); 
        foreach(var connection in connections) {
            float riverPathDistance = 0f;
            var distance = Vector2.Distance(connection[0], connection[1]);
            var difference = connection[0] - connection[1];
            var trajectory = difference.x < 0 && difference.y < 0;
            while(riverPathDistance < distance) {
                var midpoint = KMeansClustering.LerpByDistance(connection[0], connection[1], riverPathDistance);
                var x = (int) midpoint.x;
                var y = (int) midpoint.y;

                // only visit each point one time, with precision, its possible to visit points twice
                if(previouslyVisited.ContainsKey(x + "-" + y)) {
                    riverPathDistance += precision;
                    continue;
                } else {
                    previouslyVisited.Add(x + "-" + y, true);
                }

                // do it over the course of the width of the river
                var previousHeight = noiseMap[x, y];
                var newHeight = noiseMap[x, y] - riverDepth;
                noiseMap[x, y] = newHeight;

                if (trajectory == false) {
                    for (var i = 0; i < riverWidth; i++) {
                        for (var j = 0; j < riverWidth; j++) {
                            try {
                                previousHeight = noiseMap[x + i, y + j];
                                var sideNewHeight = Mathf.Lerp(newHeight, previousHeight, i / riverWidth);
                                noiseMap[x + i, y + j] = sideNewHeight;
                                previouslyVisited.Add((x+i) + "-" + (y+j), true);
                            } catch (Exception e) {}
                        }
                    }
                } else {
                    for (var i = (int) riverWidth; i > 0; i--) {
                        for (var j = (int) riverWidth; j > 0; j--) {
                            try {
                                previousHeight = noiseMap[x - i, y - j];
                                var sideNewHeight = Mathf.Lerp(newHeight, previousHeight, i / riverWidth);
                                noiseMap[x - i, y - j] = sideNewHeight;
                                previouslyVisited.Add((x-i) + "-" + (y-j), true);
                            } catch (Exception e) {}
                        }
                    }
                }
                riverPathDistance += precision;
            }
            // get ready for next connection
            previouslyVisited.Clear();
        }

        // modify the vertices at that path to simulate river

        // generate mesh that will act as the river

        // foreach(var item in lakeClusters) {
        //     Vector2 current = item.Key;
        //     noiseMap[(int) current.x, (int) current.y] = 1;
        //     noiseMap[(int) current.x, (int) current.y] = 1;
        //     noiseMap[(int) current.x + 1, (int) current.y] = 1;
        //     noiseMap[(int) current.x, (int) current.y + 1] = 1;
        //     noiseMap[(int) current.x + 1, (int) current.y + 1] = 1;
        // }

        //   foreach(var item in mountainClusters) {
        //     Vector2 current = item.Key;
        //     noiseMap[(int) current.x, (int) current.y] = 0;
        //     noiseMap[(int) current.x + 1, (int) current.y] = 0;
        //     noiseMap[(int) current.x, (int) current.y + 1] = 0;
        //     noiseMap[(int) current.x + 1, (int) current.y + 1] = 0;
        // }

        // for (var i = 0; i < mountainPoints.Count; i++) {
        //     Vector2 current = mountainPoints[i];
        //     noiseMap[(int) current.x, (int) current.y] = 1;
        // }

        // for (var i = 0; i < lakePoints.Count; i++) {
        //     Vector2 current = lakePoints[i];
        //     noiseMap[(int) current.x, (int) current.y] = -1;
        // }
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
