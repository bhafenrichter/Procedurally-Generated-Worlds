using UnityEngine;

public class WorldEngine : MonoBehaviour
{
  // configuration setttings for map generation
  public GameObject Chunks;

  private NoiseMapService NoiseMapService;
  private MeshService MeshService;
  private Mesh debugMesh;
  
  void Start()
  {
    // initialize services
    NoiseMapService = GetComponent<NoiseMapService>();

    // initialize other services

    // subscribe to the generate world event
    EventBus.Manager.Subscribe(EventBus.Actions.GENERATE_WORLD, generateWorld);

    EventBus.Manager.Broadcast(EventBus.Actions.GENERATE_WORLD, "", "");
  }

  public void generateWorld(dynamic parameters, dynamic dummy) {
    // debugging instances only
    MeshService = GetComponent<MeshService>();
    NoiseMapService = GetComponent<NoiseMapService>();

    // debugging instances only

    ClearChunks();

    generateChunk(0, 0);
    generateChunk(1, 0);

    var LakeService = GetComponent<LakeService>();
  }

  public Mesh generateChunk(int chunkX, int chunkY) {
    // debugging instances only
    MeshService = GetComponent<MeshService>();
    NoiseMapService = GetComponent<NoiseMapService>();
    // debugging instances only

    GameObject chunk = loadChunk(chunkX, chunkY);
    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
    MeshRenderer textureRenderer = chunk.GetComponent<MeshRenderer>();

    var mesh = meshFilter.mesh;

    // Noise Map Provider for all types of Noises
    float[,] noiseMap = NoiseMapService.getNoiseMap(chunkX, chunkY);

    // generate the new mesh
    meshFilter.sharedMesh = MeshService.GenerateMesh(mesh, NoiseMapService.mapSize, noiseMap);

    // destroy the previous mesh collider 
    DestroyImmediate(chunk.GetComponent<MeshCollider>());

    // update the mesh collider
    chunk.AddComponent<MeshCollider>();

    // render mesh texture
    Texture2D meshTexture = NoiseMapService.getNoiseTexture(MeshService.heightCurve, noiseMap);
    textureRenderer.material.mainTexture = meshTexture;

    // notify other modules in the generator that the terrain is complete
    EventBus.Manager.Broadcast(EventBus.Actions.GENERATE_WORLD_COMPLETE, chunkX, chunkY);

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
      chunk.transform.position = new Vector3(NoiseMapService.mapSize * chunkX, 0, NoiseMapService.mapSize * chunkY);
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
}