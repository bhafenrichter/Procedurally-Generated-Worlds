using UnityEngine;

public class WorldEngine : MonoBehaviour
{
  public Renderer textureRenderer;
  public MeshFilter meshFilter;
  public AnimationCurve heightCurve;
  public float heightMultipler;
  Mesh mesh;

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
    mesh = meshFilter.mesh;

    generateWorld();

    // subscribe to the generate world event
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD, generateWorld);
  }

  void Update()
  {

  }

  public void generateWorld() {

    Debug.Log("Generating World");
    mesh = meshFilter.sharedMesh;
    mesh.Clear();

    // Noise Map Provider for all types of Noise
    var NoiseMapService = new NoiseMapService(seed, mapSize, mapSize, scale, persistance, lacunarity, octaves);

    // intialize Mesh Service
    var MeshService = new MeshService();

    // generate the noise map
    Debug.Log("Generating Noise Map");
    NoiseMapService.getNoiseMap("Perlin");
    float[,] noiseMap = NoiseMapService.getNoiseMap("Perlin");

    
    // render the mesh
    Debug.Log("Generating Mesh");
    
    // destroy the previous mesh collider 
    DestroyImmediate(this.GetComponent<MeshCollider>());

    // generate the new mesh
    meshFilter.sharedMesh = MeshService.GenerateMesh(mesh, mapSize, noiseMap, heightMultipler, heightCurve);

    // update the mesh collider
    gameObject.AddComponent<MeshCollider>();

    // render mesh texture
    Texture2D meshTexture = NoiseMapService.getNoiseTexture(terrainConfigs, noiseMap);
    textureRenderer.sharedMaterial.mainTexture = meshTexture;
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