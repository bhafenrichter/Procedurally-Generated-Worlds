// provides the perlin noise service based on the parameters provided
using UnityEngine;

public class NoiseMapService {
  PerlinNoiseMap PerlinNoise;
  NoiseMapRenderer NoiseRenderer;
  public NoiseMapService(int seed, int width, int height, float scale, float persistance, float lacunarity, int octaves) {

    // initialize all of the different noise providers
    PerlinNoise = new PerlinNoiseMap(seed, width, height, scale, lacunarity, persistance, octaves);

    // initialize renderer
    NoiseRenderer = new NoiseMapRenderer();
  }

  public float[,] getNoiseMap(string noiseType, int chunkX, int chunkY) {
    switch (noiseType) {
      case "Perlin": {
        return PerlinNoise.generateNoise(chunkX, chunkY);
      }
      default: {
        // default to perlin
        return PerlinNoise.generateNoise(chunkX, chunkY);
      }
    }
  }

  public Texture2D getNoiseTexture(TerrainType[] terrainConfig, float[,] noiseMap) {
    return NoiseRenderer.BuildTexture(terrainConfig, noiseMap);
  }

  public Color[] getNoiseColorMap(TerrainType[] terrainConfig, float[,] noiseMap) {
    return NoiseRenderer.BuildPixelData(terrainConfig, noiseMap);
  }
}