using UnityEngine;
using System.Collections.Generic;
public class MeshService : MonoBehaviour {
  public float heightMultipler;
  public AnimationCurve heightCurve;
  public int vertexPrecision;
  public List<Vector3> debugMesh = new List<Vector3>();
  internal Mesh GenerateMesh(Mesh mesh, int mapSize, float[,] noiseMap)
  {
    Vector3[] generatedVerticles;
    List<Vector3> vertices = new List<Vector3>();
    debugMesh.Clear();

    // cleare the previous mesh
    mesh.Clear();

    // mesh simplfication needs to be a factor of the map size
    int meshSimplificationIncrement = vertexPrecision == 0 ? 1 : vertexPrecision;
    int verticesPerLine = (mapSize - 1) / meshSimplificationIncrement + 1;

    for (var y = 0; y < mapSize + 1; y+= meshSimplificationIncrement)
    {
      for (var x = 0; x < mapSize + 1; x+= meshSimplificationIncrement)
      {
        float initialHeight = noiseMap[x,y];
        initialHeight = heightCurve.Evaluate(initialHeight) * heightMultipler;
        // vertices.Add(new Vector3(x, 0, y));
        vertices.Add(new Vector3(x, initialHeight, y));
      }
    }

    generatedVerticles = vertices.ToArray();
    mesh.vertices = generatedVerticles;

    // represent the indexes of the verticles in which to reference
    int[] triangles = new int[mapSize * mapSize * 6];


    int vert = 0;
    int tris = 0;

    for (int i = 0; i < mapSize; i+= meshSimplificationIncrement)
    {
      for (int j = 0; j < mapSize; j+= meshSimplificationIncrement)
      {
        triangles[tris + 0] = vert;
        triangles[tris + 1] = vert + verticesPerLine + 1;
        triangles[tris + 2] = vert + 1;
        triangles[tris + 3] = vert + 1;
        triangles[tris + 4] = vert + verticesPerLine + 1;
        triangles[tris + 5] = vert + verticesPerLine + 2;

        vert++;
        tris += 6;
      }
      vert++;
    }

    // apply the trinagles to the mesh
    mesh.triangles = triangles; 

    // uvs are the coordinates on which the texture is applied, these need to be accurate
    // in order for the texture to be rendered correclty
    Vector2[] uvs = new Vector2[vertices.Count];

    // fuck these verticles, switching the x and the z screwed the entire thing over
    // I spent a day debugging trying to figure out why the generation wasn't running 
    // properly
    for (int i = 0; i < uvs.Length; i++)
    {
        uvs[i] = new Vector2((float) vertices[i].z / (mapSize+1), (float)  vertices[i].x / (mapSize+1));
    }

    mesh.uv = uvs;

    // recalculate mesh based on new verticles
    mesh.RecalculateNormals();
    
    return mesh;
  }

  public MeshFilter generateNewMesh() {
    var Mesh = new MeshFilter();
    return Mesh;
  }


  public Mesh generateMeshFromPoints(List<Vector3> points) {
    // Create the Vector3 vertices 
    Mesh msh = new Mesh();
    List<int> triangles = new List<int>();
    Vector2[] uvs = new Vector2[points.Count];
    int pointsIndex = 0;

    msh.vertices = points.ToArray();
    
    while(pointsIndex <= points.Count - 4) {
      triangles.Add(pointsIndex);
      triangles.Add(pointsIndex + 2);
      triangles.Add(pointsIndex + 1);
      triangles.Add(pointsIndex + 2);
      triangles.Add(pointsIndex + 3);
      triangles.Add(pointsIndex + 1);
      pointsIndex += 1;
    }

    for (int i = 0; i < uvs.Length; i++)
    {
      uvs[i] = new Vector2((float) points[i].z, (float)  points[i].x);
    }


    msh.vertices = points.ToArray();
    msh.triangles = triangles.ToArray();

    msh.RecalculateNormals();
    msh.RecalculateBounds();
    return msh;
  }

      private void OnDrawGizmos() {
        if (debugMesh != null) {
            Gizmos.color = Color.red;
            for (var i = 0; i < debugMesh.Count; i++) {
                Gizmos.DrawSphere(debugMesh[i], 0.5f);
            }
        }
    }
}