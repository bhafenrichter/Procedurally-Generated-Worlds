interface _INoiseMap
{

    int width { get; set; }
    int height { get; set; }
    float scale { get; set; }
    float persistance { get; set; }
    float lacunarity { get; set; }
    int octaves { get; set; }
    float[,] generateNoise();
}
