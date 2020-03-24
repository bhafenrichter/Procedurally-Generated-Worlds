using UnityEngine;
using System.Collections.Generic;
public class TreeService : MonoBehaviour {
  
  [Range(0, 5)]
  public float minTreeHeightThreshold;

  [Range(0, 5)]
  public float maxTreeHeightThreshold;
  public string generationType;
  public GameObject[] treePrefabs;
  void Start () {
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD_COMPLETE, populateTrees);
  }

  internal void populateTrees(dynamic mesh) {

    Debug.Log("Generating Trees");
    // dynamic casting
    Vector3[] meshVertices = mesh.vertices;

    // clear previous trees
    ClearTrees();

    // generate points for trees
    Vector3[] treePoints = generateTreePoints(meshVertices);
    Transform parent = gameObject.transform.Find("Trees").transform;
    for (var i = 0; i < treePoints.Length; i++) {
      int seed = UnityEngine.Random.Range(0, treePrefabs.Length);
      GameObject tree = GameObject.Instantiate(treePrefabs[seed], treePoints[i], treePrefabs[seed].transform.rotation);
      tree.transform.parent = parent;
    }
  }

  internal Vector3[] generateTreePoints(Vector3[] vertices) {
    List<Vector3> trees = new List<Vector3>();

    for (var i = 0; i < vertices.Length; i++) {
      if (vertices[i].y > minTreeHeightThreshold && vertices[i].y < maxTreeHeightThreshold) {
        trees.Add(vertices[i]);
      }

      i+= 10;
    }

    return trees.ToArray();
  }

    public void ClearTrees() {
  }
}