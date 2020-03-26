using UnityEngine;
using System.Collections;

public class WorldEngine : MonoBehaviour
{
  // configuration setttings for map generation
  public AnimationCurve heightCurve;
  public float heightMultipler;
  public int seed;
  public int mapSize;
  public float scale = 5f;
  public float lacunarity = 1f;
  public float persistance = 1f;
  public int octaves = 1;
  public int vertexPrecision;
  public string noiseType;
  public TerrainType[] terrainConfigs;
  public GameObject Chunks;

  private NoiseMapService NoiseMapService;
  private MeshService MeshService;
  private Mesh debugMesh;
  
  void Start()
  {
    // initialize services
    MeshService = new MeshService();
    NoiseMapService = new NoiseMapService(seed, mapSize, mapSize, scale, persistance, lacunarity, octaves);

    // subscribe to the generate world event
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD, generateWorld);

    EventBus.Manager.Broadcast(EventBus.Actions.GENERATE_WORLD, "", "");
  }

  public void generateWorld(dynamic parameters, dynamic dummy) {
    // debugging instances only
    MeshService = new MeshService();
    NoiseMapService = new NoiseMapService(seed, mapSize, mapSize, scale, persistance, lacunarity, octaves);
    // debugging instances only

    ClearChunks();

    generateChunk(0, 0);
    generateChunk(1, 0);
    generateChunk(0, 1);
    generateChunk(1, 1);
    
    // GameObject.Find("TreeEngine").GetComponent<TreeService>().populateTrees(0,0);
    // GameObject.Find("TreeEngine").GetComponent<TreeService>().populateTrees(0,1);
    // GameObject.Find("TreeEngine").GetComponent<TreeService>().populateTrees(1,0);
    // GameObject.Find("TreeEngine").GetComponent<TreeService>().populateTrees(1,1);
    
    // notify other modules in the generator that the terrain is complete
    EventBus.Manager.Broadcast(EventBus.Actions.GENERATE_WORLD_COMPLETE, 0, 0);
  }

  public Mesh generateChunk(int chunkX, int chunkY) {
    // debugging instances only
    MeshService = new MeshService();
    NoiseMapService = new NoiseMapService(seed, mapSize, mapSize, scale, persistance, lacunarity, octaves);
    // debugging instances only

    GameObject chunk = loadChunk(chunkX, chunkY);
    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
    MeshRenderer textureRenderer = chunk.GetComponent<MeshRenderer>();

    var mesh = meshFilter.mesh;

    // Noise Map Provider for all types of Noises
    float[,] noiseMap = NoiseMapService.getNoiseMap(noiseType, chunkX, chunkY);

    // generate the new mesh
    meshFilter.sharedMesh = MeshService.GenerateMesh(mesh, mapSize, noiseMap, heightMultipler, heightCurve, vertexPrecision);

    // destroy the previous mesh collider 
    DestroyImmediate(chunk.GetComponent<MeshCollider>());

    // update the mesh collider
    chunk.AddComponent<MeshCollider>();

    // render mesh texture
    Texture2D meshTexture = NoiseMapService.getNoiseTexture(terrainConfigs, noiseMap);
    textureRenderer.material.mainTexture = meshTexture;

    // render the trees

    return mesh;
  }
  public GameObject loadChunk(int chunkX, int chunkY) {

    // check to see if chunk is already generated
    string chunkName = Utils.getChunkName(chunkX, chunkY);

    Transform chunkTransform = Chunks.transform.Find(chunkName);
    GameObject chunk;

    if (chunkTransform == null) {
      chunk = new GameObject();
      chunk.name = chunkName;
      chunk.transform.position = new Vector3(mapSize * chunkX, 0, mapSize * chunkY);
      chunk.transform.parent = Chunks.transform;
    } else {
      chunk = chunkTransform.gameObject;
    }

    // see if chunk has already been rendered
    // if not give it the neccessary things
    var meshFilter = chunk.GetComponent<MeshFilter>();
    var textureRenderer = chunk.GetComponent<MeshRenderer>();
    if (meshFilter == null || textureRenderer == null) {
      meshFilter = chunk.AddComponent(typeof(MeshFilter)) as MeshFilter;
      textureRenderer = chunk.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    }

    return chunk;
  }

  public void ClearChunks() {
    DestroyImmediate(Chunks);

    GameObject freshChunk = new GameObject();
    freshChunk.transform.parent = gameObject.transform;
    freshChunk.name = "Chunks";
    Chunks = freshChunk;
  }

  private void OnDrawGizmos() {
      if (debugMesh != null && debugMesh.vertices != null) {
          Gizmos.color = Color.red;
          for (var i = 0; i < debugMesh.vertices.Length; i++) {
              Gizmos.DrawSphere(debugMesh.vertices[i], 0.1f);
          }
      }
  }
}