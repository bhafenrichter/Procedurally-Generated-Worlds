using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LakeService : MonoBehaviour {
  private WorldEngine WorldEngine;
  private NoiseMapService NoiseMapService;

  private MeshService MeshService;

  private List<Vector3> debugPoints;
  public Shader lakeShader;
  public Material lakeMaterial;
  public Texture lakeTexture;
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
    lakeClusters = KMeansClustering.cluster(50, null, mountainPoints, mapSize, 0);

    // send 4 runners in 4 directions to get the dimensions of the lake
    foreach(var cluster in lakeClusters) {
      
      Vector2 bottomleftCorner = new Vector2();
      Vector2 topRightCorner = new Vector2();
      List<Vector2> clusterPoints = cluster.Value;

      // foreach(var clusterPoint in clusterPoints) {
      //   debugPoints.Add(clusterPoint);
      // }
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
      points.Add(new Vector3(bottomleftCorner.x - 1, 0, bottomleftCorner.y - 1));
      points.Add(new Vector3(topRightCorner.x + 1, 0, bottomleftCorner.y - 1));
      points.Add(new Vector3(bottomleftCorner.x - 1, 0, topRightCorner.y + 1));
      points.Add(new Vector3(topRightCorner.x + 1, 0, topRightCorner.y + 1));
      
      var lake = new GameObject();
      var MeshFilter = lake.AddComponent<MeshFilter>();
      var MeshRenderer = lake.AddComponent<MeshRenderer>();
      MeshRenderer.material = lakeMaterial;
      MeshRenderer.material.shader = lakeShader;
      MeshRenderer.material.mainTexture = lakeTexture;

      var Mesh = MeshService.generateMeshFromPoints(points);
      MeshFilter.mesh = Mesh;
      lake.transform.parent = chunk.transform;

      // CONSTANT: sea level * height multiplier
      lake.transform.position = new Vector3(-lake.transform.position.x + (mapSize * chunkX), -lake.transform.position.y + (seaLevel * heightMultipler), -lake.transform.position.z + (mapSize * chunkY));
    }


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