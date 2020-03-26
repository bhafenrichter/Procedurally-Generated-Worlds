using UnityEngine;
using System.Collections.Generic;
public class TreeService : MonoBehaviour {
  
  [Range(0, 5)]
  public float minTreeHeightThreshold;

  [Range(0, 5)]
  public float maxTreeHeightThreshold;
  public float treeRadius;
  public string generationType;
  public GameObject[] treePrefabs;
  void Start () {
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD_COMPLETE, populateTrees);
  }

  internal void populateTrees(dynamic offsetX, dynamic offsetY) {
    Debug.Log("Generating Trees");

    int chunkX = (int) offsetX;
    int chunkY = (int) offsetY;

    int precision = gameObject.transform.parent.GetComponent<WorldEngine>().vertexPrecision;

    // dynamic casting
    GameObject chunk = GameObject.Find(Utils.getChunkName(offsetX, offsetY));
    MeshFilter chunkMesh = chunk.GetComponent<MeshFilter>();
    Vector3[] meshVertices = chunkMesh.mesh.vertices;
    int mapSize = ((int) Mathf.Sqrt(meshVertices.Length) - 1);

    // create trees game object for chunk
    Transform treesTransform = chunk.transform.Find("Trees");
    GameObject trees = treesTransform != null ? treesTransform.gameObject : null;
    if (trees == null) {
      trees = new GameObject();
      trees.name = "Trees";
      trees.transform.parent = chunk.transform;
    }    

    ClearTrees(trees);

    // generate points for trees
    Vector3[] treePoints = generateTreePoints(precision, meshVertices);

    for (var i = 0; i < treePoints.Length; i++) {
      int seed = UnityEngine.Random.Range(0, treePrefabs.Length);
      Vector3 position = new Vector3(treePoints[i].x, treePoints[i].y, treePoints[i].z);
      GameObject tree = GameObject.Instantiate(treePrefabs[seed], position, treePrefabs[seed].transform.rotation);
      tree.transform.parent = trees.transform;
    }
    trees.transform.position = new Vector3(chunkX * (mapSize * 2), 0, chunkY * (mapSize * 2));
  }

  internal Vector3[] generateTreePoints(int precision, Vector3[] vertices) {
    // use poisson disk sampling to generate clustered points
    // multiply by vertex percision
    int mapSize = ((int) Mathf.Sqrt(vertices.Length) - 1);
    // depending on the precision of the vertices, we'll need to adjust the mapsize to be 1:1 with the precision
    List<Vector2> poissonPoints = PoissonDiscSampling.GeneratePoints(treeRadius, new Vector2(mapSize / precision, mapSize / precision), 30);
    List<Vector3> trees = new List<Vector3>();

    for (var i = 0; i < poissonPoints.Count; i++) {
      Vector2 current = poissonPoints[i];
      int verticesIndex = (int) ((mapSize + 1) * (int) current.x) + (int) current.y;
      float height = vertices[verticesIndex].y;
      
      if (height >= minTreeHeightThreshold && height <= maxTreeHeightThreshold) {
        trees.Add(new Vector3(current.y * 2 * precision, height, current.x * 2 * precision));
      }
      
    }

    return trees.ToArray();
  }

  public void ClearTrees(GameObject trees) {
    foreach(Transform child in trees.transform)
    {
        DestroyImmediate(child.gameObject);
    }
  }
}