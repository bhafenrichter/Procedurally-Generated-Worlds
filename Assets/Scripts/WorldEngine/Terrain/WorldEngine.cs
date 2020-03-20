using System.Collections.Generic;
using UnityEngine;

public class WorldEngine : MonoBehaviour
{
  public Renderer textureRenderer;
  public MeshFilter meshFilter;
  Mesh mesh;
  int[] triangles;

  // configuration setttings for map generation
  public int seed;
  public int mapSize;
  public float scale = 5f;
  public float lacunarity = 1f;
  public float persistance = 1f;
  public int octaves = 1;
  public string noiseType = "Perlin";
  public TerrainType[] terrainConfigs;

  void Start()
  {
    // textureRenderer = GetComponent<MeshRenderer>();
    // meshFilter = GetComponent<MeshFilter>();
    mesh = meshFilter.mesh;

    generateWorld();

    // subscribe to the generate world event
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD, generateWorld);
  }

  void Update()
  {

  }

  public void generateWorld() {
    mesh = meshFilter.mesh;
    mesh.Clear();
    // Noise Map Provider for all types of Noise
    var NoiseMapService = new NoiseMapService(seed, mapSize, mapSize, scale, persistance, lacunarity, octaves);

    // generate the noise map
    NoiseMapService.getNoiseMap("Perlin");
    float[,] noiseMap = NoiseMapService.getNoiseMap("Perlin");

    
    // render the mesh
    meshFilter.mesh = GenerateMesh(noiseMap);

    // render mesh texture
    Texture2D meshTexture = NoiseMapService.getNoiseTexture(terrainConfigs, noiseMap);
    textureRenderer.sharedMaterial.mainTexture = meshTexture;
    // textureRenderer.transform.localScale = new Vector3(mapSize, mapSize, mapSize);

    // meshFilter.mesh.colors = NoiseMapService.getNoiseColorMap(terrainConfigs, noiseMap);
  }

  Mesh GenerateMesh(float[,] noiseMap)
  {
    mesh.Clear();
    Vector3[] generatedVerticles;

    List<Vector3> vertices = new List<Vector3>();

    for (var y = 0; y < mapSize + 1; y++)
    {
      for (var x = 0; x < mapSize + 1; x++)
      {
        // vertices.Add(new Vector3(x, 0, y));
        vertices.Add(new Vector3(x, noiseMap[x,y] * scale, y));

        generatedVerticles = vertices.ToArray();
        mesh.vertices = generatedVerticles;
      }
    }

    // represent the indexes of the verticles in which to reference
    triangles = new int[mapSize * mapSize * 6];

    var vert = 0;
    var tris = 0;
    var vertexIndex = 0;
    for (var i = 0; i < mapSize; i++)
    {
      for (var j = 0; j < mapSize; j++)
      {
        triangles[tris + 0] = vert;
        triangles[tris + 1] = vert + mapSize + 1;
        triangles[tris + 2] = vert + 1;
        triangles[tris + 3] = vert + 1;
        triangles[tris + 4] = vert + mapSize + 1;
        triangles[tris + 5] = vert + mapSize + 2;

        vert++;
        tris += 6;
        mesh.triangles = triangles; 

        vertexIndex += 6;
      }
      vert++;
    }


    Vector2[] uvs = new Vector2[vertices.Count];

    // fuck these verticles, switching the x and the z screwed the entire thing over
    // I spent a day debugging trying to figure out why the generation wasn't running 
    // properly
    for (int i = 0; i < uvs.Length; i++)
    {
        uvs[i] = new Vector2((float) vertices[i].z / (mapSize+1), (float)  vertices[i].x / (mapSize+1));
    }

    mesh.uv = uvs;

    mesh.RecalculateNormals();
    
    return mesh;
  }

  // private void OnDrawGizmos() {
  //     if (mesh.vertices != null) {
  //         Gizmos.color = Color.red;
  //         for (var i = 0; i < mesh.vertices.Length; i++) {
  //             Gizmos.DrawSphere(mesh.vertices[i], 0.1f);
  //         }
  //     }
  // }
}
[System.Serializable]
public struct TerrainType {
    public float threshold;
    public string name;
    public Color color;
}