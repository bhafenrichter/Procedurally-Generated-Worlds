using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
public class RiverEngine : MonoBehaviour
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
    private List<List<Vector3>> riverPaths;
    public Texture riverTexture;
    public Shader riverShader;
    public Material riverMaterial;
    public enum DIRECTIONS {
        EMPTY=-1,
        DOWN= 0,
        UP= 1,
        LEFT= 2,
        RIGHT= 3
    };
    private void Start() {
        // EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD_COMPLETE, generateRiverMeshes);
    }

    public float[,] generateRivers(float[,] noiseMap) {
        // save these for later when you're generating the mesh
        riverPaths = new List<List<Vector3>>();

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
            var directions = new List<DIRECTIONS>();
            var path = generateRiverPath(noiseMap, connection[0], connection[1], ref directions);
            // outlines the river so we can generate the mesh later
            var riverOutline = new List<Vector3>();
            var prevDirection = DIRECTIONS.EMPTY;
            // while we are still traversing along the route
            for (var k = 0; k < path.Count; k++) {

                // get the current position based on how far we've traveled
                var midpoint = path[k];
                var direction = directions[k];

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

                // make sure we don't hit this point again to avoid 0 or 1 absolutes
                if (previouslyVisited.ContainsKey((x) + "-" + (y)) == false) {
                    previouslyVisited.Add((x) + "-" + (y), true);
                }

                // carve the depth into the river
                var newHeight = Mathf.Lerp( noiseMap[x, y] - riverDepth, previousHeight, 1);
                noiseMap[x, y] = newHeight;

                // add the beginning and end points to the mesh data
                var startingPoint = configureRiverStartingPoints(x, y, riverWidth, direction, true);
                var outlinePoint = getRiverOutlinePoint(startingPoint.x, startingPoint.y, direction);
                riverOutline.Add(new Vector3(outlinePoint.x, newHeight, outlinePoint.y));
                
                startingPoint = configureRiverStartingPoints(x, y, riverWidth, direction, false);
                outlinePoint = getRiverOutlinePoint(startingPoint.x, startingPoint.y, direction);
                riverOutline.Add(new Vector3(outlinePoint.x, newHeight, outlinePoint.y));

                var sideNewHeight = 0f;
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
                        sideNewHeight = Mathf.Lerp(previousHeight - riverDepth, previousHeight, i / riverWidth);
                        noiseMap[x + i, y + j] = sideNewHeight;
                    }
                }
                prevDirection = direction;
            }
            // get ready for next connection
            previouslyVisited.Clear();

            // add the mesh data
            riverPaths.Add(riverOutline);
        }
        return noiseMap;
    }

    public Vector2 configureRiverStartingPoints (int x, int y, int riverWidth, DIRECTIONS direction, bool isOrigin) {
        switch (direction) {
            case DIRECTIONS.UP:
                return isOrigin ? new Vector2(x + riverWidth, y - 1) : new Vector2(x, y + riverWidth + 1);
            case DIRECTIONS.DOWN:
                return isOrigin ? new Vector2(x, y) : new Vector2(x + riverWidth, y + riverWidth);
            case DIRECTIONS.LEFT:
                return isOrigin ? new Vector2(x, y) : new Vector2(x + riverWidth, y + riverWidth);
            case DIRECTIONS.RIGHT:
                return isOrigin ? new Vector2(x + riverWidth, y) : new Vector2(x, y + riverWidth);
            default:
                return Vector2.zero;
        }
    }
    public Vector2 getRiverOutlinePoint (float x, float y, DIRECTIONS direction) {
        Vector2 point = new Vector2();
        switch (direction) {
            case DIRECTIONS.UP:
                point = new Vector2(x, y + 1);
                break;
            case DIRECTIONS.DOWN:
                point = new Vector2(x, y - 1);
                break;
            case DIRECTIONS.LEFT:
                point = new Vector2(x - 1, y);
                break;
            case DIRECTIONS.RIGHT:
                point = new Vector2(x + 1, y);
                break;
            default: 
                return Vector2.zero;
        }
        return point;
    }

    public List<Vector2> generateRiverPath(float[,] noiseMap, Vector2 origin, Vector2 destination, ref List<DIRECTIONS> directions) {
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

            var upElevation = up.x > 0 ? noiseMap[(int) up.x, (int) up.y] : 1f;
            var downElevation = down.x > 0 ? noiseMap[(int) down.x, (int) down.y] : 1f;
            var leftElevation = left.x > 0 ? noiseMap[(int) left.x, (int) left.y] : 1f;
            var rightElevation = right.x > 0 ? noiseMap[(int) right.x, (int) right.y] : 1f;

            var estimatedProgress = 0;

            var upScore = evaluateRiverPoint(initialDistance, upDistance, upElevation, estimatedProgress);
            var downScore = evaluateRiverPoint(initialDistance, downDistance, downElevation, estimatedProgress);
            var leftScore = evaluateRiverPoint(initialDistance, leftDistance, leftElevation, estimatedProgress);
            var rightScore = evaluateRiverPoint(initialDistance, rightDistance, rightElevation, estimatedProgress);

            var bestScore = Mathf.Max(upScore, downScore, leftScore, rightScore);
            
            if (upScore == bestScore) { path.Add(up); directions.Add(DIRECTIONS.UP); }
            if (downScore == bestScore) { path.Add(down); directions.Add(DIRECTIONS.DOWN); }
            if (leftScore == bestScore) { path.Add(left); directions.Add(DIRECTIONS.LEFT); }
            if (rightScore == bestScore) { path.Add(right); directions.Add(DIRECTIONS.RIGHT); }
            
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

    public void generateRiverMeshes (dynamic chunkX, dynamic chunkY) {
        GameObject chunk = GameObject.Find(Utils.getChunkName(chunkX, chunkY));
        MeshService meshService = GetComponent<MeshService>();

        foreach(var river in riverPaths) {
            GameObject test = new GameObject();
            MeshFilter meshFilter = test.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer renderer = test.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            test.name = "River";
            StartCoroutine(meshService.generateMeshFromPoints(chunk.GetComponent<MeshFilter>().mesh, river));
            // var mesh = meshService.generateMeshFromPoints(chunk.GetComponent<MeshFilter>().mesh, river);
            // meshFilter.mesh = mesh;

            // renderer.material = riverMaterial;
            // renderer.material.mainTexture = riverTexture;
            // renderer.material.shader = riverShader;

            // meshFilter.transform.parent = test.transform;
            // renderer.transform.parent = test.transform;
            // test.transform.parent = chunk.transform.parent;

            // Instantiate(test, Vector3.zero, gameObject.transform.rotation);
        }
    }
}
