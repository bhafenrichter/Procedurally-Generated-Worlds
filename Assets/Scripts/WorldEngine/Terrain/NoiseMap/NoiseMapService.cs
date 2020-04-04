// provides the perlin noise service based on the parameters provided
using UnityEngine;

public class NoiseMapService {
  PerlinNoiseMap PerlinNoise;
  SimplexNoiseMap SimplexNoise;
  RiverEngine RiverEngine;
  
  NoiseMapRenderer NoiseRenderer;
  public NoiseMapService(int seed, int width, int height, float scale, float persistance, float lacunarity, int octaves) {

    // initialize all of the different noise providers
    PerlinNoise = new PerlinNoiseMap(seed, width, height, scale, lacunarity, persistance, octaves);
    SimplexNoise = new SimplexNoiseMap(seed, width, height, scale, lacunarity, persistance, octaves);

    // initialize renderer
    NoiseRenderer = new NoiseMapRenderer();
  }

  public float[,] getNoiseMap(string noiseType, int chunkX, int chunkY) {
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

    return noiseMap;
  }

  public Texture2D getNoiseTexture(TerrainType[] terrainConfig, AnimationCurve heightCurve, float[,] noiseMap) {
    return NoiseRenderer.BuildTexture(terrainConfig, heightCurve, noiseMap);
  }

  public Color[] getNoiseColorMap(TerrainType[] terrainConfig, AnimationCurve heightCurve, float[,] noiseMap) {
    return NoiseRenderer.BuildPixelData(terrainConfig, heightCurve, noiseMap);
  }
}