using UnityEngine;

class PerlinNoiseMap : _INoiseMap
{
    public int seed {get; set;}
    public int width {get; set;}
    public int height {get; set;}
    public float scale {get; set;}
    public float lacunarity {get; set;}
    public float persistance {get; set;}
    public int octaves {get; set;}
    public PerlinNoiseMap (int seed, int width, int height, float scale, float lacunarity, float persistance, int octaves) {
      this.seed = seed;
      this.width = width;
      this.height = height;
      this.scale = scale;
      this.lacunarity = lacunarity;
      this.persistance = persistance;
      this.octaves = octaves;
    }
    public float[,] generateNoise (int chunkX, int chunkY) {
      // create an empty noise map with the mapDepth and mapWidth coordinates
      float[,] noiseMap = new float[width + 1, height + 1];

      float maxHeight = float.MinValue;
      float minHeight = float.MaxValue;

      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          float frequency = 1;
          float amplitude = 1;
          float noiseHeight = 0;

          for (var i = 0; i < octaves; i++) {
            // calculate sample indices based on the coordinates and the scale
            float sampleX = (float) (x + (chunkX * height)) / scale * frequency;
            float sampleZ = (float) (y + (chunkY * height)) / scale * frequency;

            // add seed offset and the chunk coordinates
            sampleX += seed;
            sampleZ += seed;

            // generate noise value using PerlinNoise
            float noise = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            noiseHeight += noise * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
          }

          minHeight = noiseHeight < minHeight ? noiseHeight : minHeight;
          maxHeight = noiseHeight > maxHeight ? noiseHeight : maxHeight;

          noiseMap[x, y] = noiseHeight;
        }
      }

      for (var x = 0; x < width; x++) {
        for (var y = 0; y < height; y++) {
          noiseMap[(int) x, (int) y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x,y]);
        }
      }

      return noiseMap;
    }
}