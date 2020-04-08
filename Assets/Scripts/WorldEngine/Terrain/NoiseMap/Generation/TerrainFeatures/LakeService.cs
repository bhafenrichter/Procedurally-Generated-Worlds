using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LakeService : MonoBehaviour {
  private WorldEngine WorldEngine;
  private NoiseMapService NoiseMapService;

  private MeshService MeshService;

  private List<Vector3> debugPoints;
  public Material lakeMaterial;  
  public int MESH_PADDING = 1;
  void Start()
  {
      WorldEngine = GetComponent<WorldEngine>();
      NoiseMapService = GetComponent<NoiseMapService>();
      MeshService = GetComponent<MeshService>();

      EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD_COMPLETE, generateLakes);
  }

  public void generateLakes(dynamic chunkX, dynamic chunkY) {
    // run kmeans to find the lake points
    Dictionary<Vector2, List<Vector2>> lakeClusters = new Dictionary<Vector2, List<Vector2>>();
    float[,] chunkNoiseMap = NoiseMapService.getCachedNoiseMap(chunkX, chunkY);
    int mapSize = chunkNoiseMap.GetLength(0);
    GameObject chunk = GameObject.Find(Utils.getChunkName(chunkX, chunkY));
    float heightMultipler = MeshService.heightMultipler;
    float seaLevel = NoiseMapService.getTerrainLevel("Water");
    float beachLevel = NoiseMapService.getTerrainLevel("Beach");
    List<Vector2> mountainPoints = Utils.getPointsAtThreshold(chunkNoiseMap, beachLevel * 2, "<");
    List<MeshFilter> meshes = new List<MeshFilter>();

    lakeClusters = KMeansClustering.cluster(50, null, mountainPoints, mapSize, 0);

    // instantiate lakes game object
    var lakeMesh = new GameObject();
    lakeMesh.name = "Lakes";
    lakeMesh.transform.parent = chunk.transform; 
    // placeholder for final mesh
    lakeMesh.AddComponent<MeshFilter>();
    var MeshRenderer = lakeMesh.AddComponent<MeshRenderer>();
    MeshRenderer.material = lakeMaterial;

    // send 4 runners in 4 directions to get the dimensions of the lake
    foreach(var cluster in lakeClusters) {
      
      Vector2 bottomleftCorner = new Vector2();
      Vector2 topRightCorner = new Vector2();
      List<Vector2> clusterPoints = cluster.Value;

      // find the lowest x
      bottomleftCorner.x = clusterPoints.OrderBy(x => x.x).FirstOrDefault().x;

      // find the highest x
      topRightCorner.x = clusterPoints.OrderByDescending(x => x.x).FirstOrDefault().x;

      // find the lowest y
      bottomleftCorner.y = clusterPoints.OrderBy(x => x.y).FirstOrDefault().y;

      // find the highest y
      topRightCorner.y = clusterPoints.OrderByDescending(x => x.y).FirstOrDefault().y;
      
      // generate the mesh based on the 4 bounds of the square
      List<Vector3> points = new List<Vector3>();

      // shader calculations require more than 4 vertices, lets break it up into increments of 1
      points.Add(new Vector3(bottomleftCorner.x - MESH_PADDING, 0, bottomleftCorner.y - MESH_PADDING));
      points.Add(new Vector3(topRightCorner.x + MESH_PADDING, 0, bottomleftCorner.y - MESH_PADDING));
      points.Add(new Vector3(bottomleftCorner.x - MESH_PADDING, 0, topRightCorner.y + MESH_PADDING));
      points.Add(new Vector3(topRightCorner.x + MESH_PADDING, 0, topRightCorner.y + MESH_PADDING));
      
      var lake = new GameObject();
      var MeshFilter = lake.AddComponent<MeshFilter>();
      var testRenderer = lake.AddComponent<MeshRenderer>();
      testRenderer.material = lakeMaterial;

      var Mesh = MeshService.generateMeshFromPoints(points);
      MeshFilter.mesh = Mesh;
      lake.transform.parent = lakeMesh.transform;

      lake.transform.position = new Vector3(
        -lake.transform.position.x + (mapSize * chunkX), 
        -lake.transform.position.y + (seaLevel * heightMultipler), 
        -lake.transform.position.z + (mapSize * chunkY));
    }

    // combine all lakes into one mesh
    MeshService.combineMeshes(lakeMesh);
  }

  private void OnDrawGizmos() {
      if (debugPoints != null) {
          Gizmos.color = Color.red;
          for (var i = 0; i < debugPoints.Count; i++) {
              Gizmos.DrawSphere(new Vector3(debugPoints[i].x, 50, debugPoints[i].y), 1f);
          }
      }
  }

}