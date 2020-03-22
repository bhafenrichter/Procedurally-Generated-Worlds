using UnityEngine;
using System.Collections.Generic;

public class MeshService {
  internal Mesh GenerateMesh(Mesh mesh, int mapSize, float[,] noiseMap, float heightMultipler, AnimationCurve heightCurve)
  {
    Vector3[] generatedVerticles;
    List<Vector3> vertices = new List<Vector3>();

    // cleare the previous mesh
    mesh.Clear();

    for (var y = 0; y < mapSize + 1; y++)
    {
      for (var x = 0; x < mapSize + 1; x++)
      {
        // vertices.Add(new Vector3(x, 0, y));
        float initialHeight = noiseMap[x,y];
        initialHeight = heightCurve.Evaluate(initialHeight) * heightMultipler;
        vertices.Add(new Vector3(x, initialHeight, y));
      }
    }

    generatedVerticles = vertices.ToArray();
    mesh.vertices = generatedVerticles;

    // represent the indexes of the verticles in which to reference
    int[] triangles = new int[mapSize * mapSize * 6];


    int vert = 0;
    int tris = 0;

    for (int i = 0; i < mapSize; i++)
    {
      for (int j = 0; j < mapSize; j++)
      {
        triangles[tris + 0] = vert;
        triangles[tris + 1] = vert + mapSize + 1;
        triangles[tris + 2] = vert + 1;
        triangles[tris + 3] = vert + 1;
        triangles[tris + 4] = vert + mapSize + 1;
        triangles[tris + 5] = vert + mapSize + 2;

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
}