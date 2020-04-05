// provides the perlin noise service based on the parameters provided
using UnityEngine;
using System.Collections.Generic;

public class NoiseMapService : MonoBehaviour {
  public int seed;
  public int mapSize;
  public float scale = 5f;
  public float lacunarity = 1f;
  public float persistance = 1f;
  public int octaves = 1;
  public string noiseType;
  PerlinNoiseMap PerlinNoise;
  SimplexNoiseMap SimplexNoise;
  
  NoiseMapRenderer NoiseRenderer;
  Dictionary<string, float[,]> cachedNoiseMaps;
  private void Start() {
    // initialize renderer
    NoiseRenderer = new NoiseMapRenderer();
    cachedNoiseMaps = new Dictionary<string, float[,]>();
  }

  public float[,] getNoiseMap(int chunkX, int chunkY) {
    // initialize all of the different noise providers
    PerlinNoise = new PerlinNoiseMap(seed, mapSize, mapSize, scale, lacunarity, persistance, octaves);
    SimplexNoise = new SimplexNoiseMap(seed, mapSize, mapSize, scale, lacunarity, persistance, octaves);
      
    float[,] noiseMap = new float[0, 0];
    switch (noiseType) {
      case "Perlin": {
        noiseMap = PerlinNoise.generateNoise(chunkX, chunkY);
        break;
      }
      case "Simplex": {
        noiseMap = SimplexNoise.generateNoise(chunkX, chunkY);
        break;
      }
      default: {
        // default to perlin
        noiseMap = PerlinNoise.generateNoise(chunkX, chunkY);
        break;
      }
    }

    // cache noise maps for later calls
    string chunkIndex = Utils.getChunkName(chunkX, chunkY);
    if (cachedNoiseMaps.ContainsKey(chunkIndex)) {
      cachedNoiseMaps.Remove(chunkIndex);
    }
    cachedNoiseMaps.Add(chunkIndex, noiseMap);

    return noiseMap;
  }

  public Texture2D getNoiseTexture(TerrainType[] terrainConfig, AnimationCurve heightCurve, float[,] noiseMap) {
    return NoiseRenderer.BuildTexture(terrainConfig, heightCurve, noiseMap);
  }

  public Color[] getNoiseColorMap(TerrainType[] terrainConfig, AnimationCurve heightCurve, float[,] noiseMap) {
    return NoiseRenderer.BuildPixelData(terrainConfig, heightCurve, noiseMap);
  }
  public float[,] getCachedNoiseMap(int chunkX, int chunkY) {
    var index = Utils.getChunkName(chunkX, chunkY);

    if (cachedNoiseMaps.ContainsKey(index)) {
      return cachedNoiseMaps[index];
    } else {
      return new float[0,0];
    }
  }
}