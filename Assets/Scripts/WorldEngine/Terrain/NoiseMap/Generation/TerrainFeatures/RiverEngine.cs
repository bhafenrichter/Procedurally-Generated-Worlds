using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class RiverEngine
{
    public float LAKE_THRESHOLD = 0.1f;
    public float MOUNTAIN_THRESHOLD = 0.9f;
    public float MAX_RIVER_HEIGHT = 0.8f;
    public float MIN_RIVER_HEIGHT = 0.2f;
    public float PATH_PRECISION = 0.5f;
    public int MIN_RIVER_WIDTH = 5;
    public int MAX_RIVER_WIDTH = 15;
    public float MIN_RIVER_DEPTH = 0.05f;
    public float MAX_RIVER_DEPTH = 0.1f;
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

            var riverWidth = UnityEngine.Random.Range(MIN_RIVER_WIDTH, MAX_RIVER_WIDTH);
            var riverDepth = UnityEngine.Random.Range(MIN_RIVER_DEPTH, MAX_RIVER_DEPTH);

            var distance = Vector2.Distance(connection[0], connection[1]);
            
            if (distance > MAX_RIVER_DISTANCE) {
                continue;
            }

            // generate the route
            var path = generateRiverPath(noiseMap, connection[0], connection[1]);
            
            // while we are still traversing along the route
            for (var k = 0; k < path.Count; k++) {

                // get the current position based on how far we've traveled
                var midpoint = path[k];
                var x = (int) midpoint.x;
                var y = (int) midpoint.y;

                // do it over the course of the width of the river
                var previousHeight = noiseMap[x, y];

                // above a certain height, we don't want rivers generating
                if (previousHeight > MAX_RIVER_HEIGHT) {
                    continue;
                }

                if (previousHeight + riverDepth < MIN_RIVER_HEIGHT) {
                    break;
                }

                // carve the depth into the river
                var newHeight = noiseMap[x, y] - riverDepth;
                noiseMap[x, y] = newHeight;

                // make sure we don't hit this point again to avoid 0 or 1 absolutes
                if (previouslyVisited.ContainsKey((x) + "-" + (y)) == false) {
                    previouslyVisited.Add((x) + "-" + (y), true);
                }
                

                // traverse to the sides of the point and set the depth accordingly
                // slowly progressing higher and higher to make it bowl
                for (var i = 0; i < riverWidth; i++) {
                    for (var j = 0; j < riverWidth; j++) {

                        // only visit each point one time, with precision, its possible to visit points twice
                        if(previouslyVisited.ContainsKey((x + i) + "-" + (y + j))) {
                            continue;
                        } else {
                            previouslyVisited.Add((x + i) + "-" + (y + j), true);
                        }

                        // get the previous height and lerp it based on how far from the center we are
                        previousHeight = noiseMap[x + i, y + j];
                        var sideNewHeight = Mathf.Lerp(previousHeight - riverDepth, previousHeight, i / riverWidth);
                        noiseMap[x + i, y + j] = sideNewHeight;
                    }
                }
            }
            // get ready for next connection
            previouslyVisited.Clear();
        }
        return noiseMap;
    }

    public List<Vector2> generateRiverPath(float[,] noiseMap, Vector2 origin, Vector2 destination) {
        List<Vector2> path = new List<Vector2>();
        var currentPoint = origin;
        var initialDistance = Vector2.Distance(origin, destination);

        while(initialDistance > 10) {
            if (path.Count > 1000) {
                break;
            }
            var up = new Vector2(currentPoint.x, currentPoint.y + 1);
            var down = new Vector2(currentPoint.x, currentPoint.y - 1);
            var left = new Vector2(currentPoint.x - 1, currentPoint.y);
            var right = new Vector2(currentPoint.x + 1, currentPoint.y);

            var upDistance = Vector2.Distance(up, destination);
            var downDistance = Vector2.Distance(down, destination);
            var leftDistance = Vector2.Distance(left, destination);
            var rightDistance = Vector2.Distance(right, destination);

            var upElevation = noiseMap[(int) up.x, (int) up.y];
            var downElevation = noiseMap[(int) down.x, (int) down.y];
            var leftElevation = noiseMap[(int) left.x, (int) left.y];
            var rightElevation = noiseMap[(int) right.x, (int) right.y];

            var estimatedProgress = 0;

            var upScore = evaluateRiverPoint(initialDistance, upDistance, upElevation, estimatedProgress);
            var downScore = evaluateRiverPoint(initialDistance, downDistance, downElevation, estimatedProgress);
            var leftScore = evaluateRiverPoint(initialDistance, leftDistance, leftElevation, estimatedProgress);
            var rightScore = evaluateRiverPoint(initialDistance, rightDistance, rightElevation, estimatedProgress);

            var bestScore = Mathf.Max(upScore, downScore, leftScore, rightScore);
            
            if (upScore == bestScore) { path.Add(up); }
            if (downScore == bestScore) { path.Add(down); }
            if (leftScore == bestScore) { path.Add(left); }
            if (rightScore == bestScore) { path.Add(right); }
            
            currentPoint = path.Last();
            initialDistance = Vector2.Distance(currentPoint, destination);
        }

        return path;
    }

    public float evaluateRiverPoint (float initialDistance, float distance, float height, float progress) {
        float distanceHeuristic = 0.5f;
        float heightHeuristic = 0.5f;

        // rule out going away from water source
        if (initialDistance < distance) {
            return 0;
        }

        return 1 - height;
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
