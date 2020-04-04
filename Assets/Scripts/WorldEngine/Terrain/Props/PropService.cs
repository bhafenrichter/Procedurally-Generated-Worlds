using UnityEngine;
using System.Collections.Generic;
public class PropService : MonoBehaviour {
  
  [Range(0, 1)]
  public float minPropHeightThreshold;

  [Range(0, 1)]
  public float maxPropHeightThreshold;
  public string propName;
  public float propRadius;
  public GameObject[] propPrefabs;
  void Start () {
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD_COMPLETE, populateProps);
  }

  internal void populateProps(dynamic offsetX, dynamic offsetY) {
    int chunkX = (int) offsetX;
    int chunkY = (int) offsetY;

    int precision = gameObject.transform.parent.GetComponent<MeshService>().vertexPrecision;
    float heightMultipler = gameObject.transform.parent.GetComponent<MeshService>().heightMultipler;

    // dynamic casting
    GameObject chunk = GameObject.Find(Utils.getChunkName(offsetX, offsetY));
    MeshFilter chunkMesh = chunk.GetComponent<MeshFilter>();
    Vector3[] meshVertices = chunkMesh.mesh.vertices;
    int mapSize = ((int) Mathf.Sqrt(meshVertices.Length) - 1);

    // create trees game object for chunk
    Transform treesTransform = chunk.transform.Find(propName);
    GameObject trees = treesTransform != null ? treesTransform.gameObject : null;
    if (trees == null) {
      trees = new GameObject();
      trees.name = propName;
      trees.transform.parent = chunk.transform;
    }    

    ClearTrees(trees);

    // generate points for trees
    Vector3[] treePoints = generateTreePoints(chunkX, chunkY, precision, heightMultipler, meshVertices);

    for (var i = 0; i < treePoints.Length; i++) {
      int seed = UnityEngine.Random.Range(0, propPrefabs.Length);
      Vector3 position = new Vector3(treePoints[i].x, treePoints[i].y, treePoints[i].z);
      GameObject tree = GameObject.Instantiate(propPrefabs[seed], position, propPrefabs[seed].transform.rotation);
      tree.transform.parent = trees.transform;
    }
    trees.transform.position = new Vector3(chunkX * (mapSize * precision), 0, chunkY * (mapSize * precision));
  }

  internal Vector3[] generateTreePoints(int chunkX, int chunkY, int precision, float heightMultiplier, Vector3[] vertices) {
    // use poisson disk sampling to generate clustered points
    // multiply by vertex percision
    int mapSize = ((int) Mathf.Sqrt(vertices.Length) - 1);
    // depending on the precision of the vertices, we'll need to adjust the mapsize to be 1:1 with the precision
    List<Vector2> poissonPoints = PoissonDiscSampling.GeneratePoints(propRadius, new Vector2(mapSize, mapSize), 30);
    List<Vector3> trees = new List<Vector3>();

    for (var i = 0; i < poissonPoints.Count; i++) {
      Vector2 current = poissonPoints[i];
      int verticesIndex = (int) ((mapSize + 1) * (int) current.x) + (int) current.y;
      float height = vertices[verticesIndex].y;
      
      // the heights aren't the heights from the heightmap, but the mesh.  These 
      // need to be scaled back down to the noisemap values to get the correct
      // data
      float normalizedHeight = Mathf.Lerp(0f, 1f, height / heightMultiplier);
      
      if (normalizedHeight >= minPropHeightThreshold && normalizedHeight <= maxPropHeightThreshold) {
        // position must scale with the precision
        trees.Add(new Vector3(current.y * precision, height, current.x * precision));
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