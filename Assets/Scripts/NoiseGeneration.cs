using UnityEngine;
using MyUtils.Structs;
using Unity.Jobs;
using Unity.Collections;
using System;
using MyUtils.Classes;
using Unity.Mathematics;
using Unity.Burst;

public static class NoiseGeneration {
    public static uint _seed = 768754;
    public static int _chunkSize;



    public static void GenerateRandomSeed() {
        _seed = (uint)UnityEngine.Random.Range(uint.MinValue, uint.MaxValue);
    }



    public static void GenerateNoiseMap(NoiseSettingData data, Vector2Int offset, Action<float[,], Vector2Int> callback) {
        var chunkSize = _chunkSize;
        float[,] finalResult = new float[chunkSize, chunkSize];
        NativeList<JobHandle> allJobs = new(Allocator.Temp);
        NativeArray<float> noiseResults = new(chunkSize * chunkSize, Allocator.TempJob);

        //generate [0,x,y] - height map
        int index = 0;
        foreach (var w in data._settings._weightedNoiseSettings) {
            GenerateNoiseMapLayerJob noiseGenJob = new() {
                result = noiseResults,
                seed = _seed,
                noiseSetting = w._noiseSetting,
                normalizedWeight = GetNormalizedWeight(data._settings._weightedNoiseSettings, w._weight),
                offset = new(offset.x, offset.y),
                mapSize = chunkSize
            };
            // Debug.Log(GetNormalizedWeight(data._settings._weightedNoiseSettings, w._weight));
            if (index != 0) allJobs.Add(noiseGenJob.Schedule(allJobs[index - 1]));
            else allJobs.Add(noiseGenJob.Schedule());
            index++;
        }

        JobHandle.CompleteAll(allJobs);

        // Debug.Log(weightedNoiseSettings.Length);
        for (int x = 0; x < finalResult.GetLength(1); x++) {
            for (int y = 0; y < finalResult.GetLength(2); y++) {
                var arrayPos = x + chunkSize * y;
                finalResult[x, y] = noiseResults[arrayPos];
            }
        }

        noiseResults.Dispose();
        allJobs.Dispose();
        // temperatureResult.Dispose();
        // humidityResult.Dispose();
        callback?.Invoke(finalResult, offset);
    }

    private static float GetNormalizedWeight(WeightedNoiseSetting[] wNS, float v) {
        float allWeight = 0f;
        foreach (WeightedNoiseSetting w in wNS) allWeight += w._weight;
        return Mathf.InverseLerp(0, allWeight, v);
    }


    public static void GenerateEnvironmentNoiseMap(NoiseSettingData data, Vector2Int offset, Action<float[,], Vector2Int> callback) {
        var chunkSize = _chunkSize;
        float[,] finalResult = new float[chunkSize, chunkSize];
        NativeArray<float> noiseResults = new(chunkSize * chunkSize, Allocator.TempJob);

        GenerateNoiseMapJob noiseGenJob = new() {
            result = noiseResults,
            seed = _seed + 3,
            nS = data._environmentNoise,
            offset = new(offset.x, offset.y),
            mapSize = chunkSize
        };
        JobHandle h = noiseGenJob.Schedule();
        h.Complete();

        // Debug.Log(weightedNoiseSettings.Length);
        for (int x = 0; x < finalResult.GetLength(0); x++) {
            for (int y = 0; y < finalResult.GetLength(1); y++) {
                var arrayPos = x + chunkSize * y;
                finalResult[x, y] = noiseResults[arrayPos];
            }
        }

        noiseResults.Dispose();
        callback?.Invoke(finalResult, offset);
    }
    [BurstCompile]
    public struct GenerateNoiseMapParallel : IJobParallelFor {
        public NativeArray<int2> posArray;
        public float2 offset;
        public NativeArray<float> result;
        public uint seed;
        public float normalizedWeight;
        public int mapSize;
        public NoiseLayerSetting noiseSetting;
        public void Execute(int index) {
            NativeArray<float2> octaveOffset = new(mapSize * mapSize, Allocator.Temp);
            Unity.Mathematics.Random rng = new(seed);
            float maxPossibleH = 0f;
            float amplitude = 1f;

            for (int i = 0; i < noiseSetting._octaves; i++) {
                float x = rng.NextInt(-100000, 100000) + offset.x;
                float y = rng.NextInt(-100000, 100000) + offset.y;

                maxPossibleH += amplitude;
                amplitude *= noiseSetting._persistance;
                octaveOffset[i] = new float2(x, y);
            }
            // float maxNoiseH = float.MinValue;
            // float minNoiseH = float.MaxValue;
            amplitude = 1f;
            float frequency = 1f;
            float noiseH = 0f;

            for (int i = 0; i < noiseSetting._octaves; i++) {

                float2 sample = new((posArray[index].x - (mapSize / 2) + octaveOffset[i].x) / noiseSetting._scale * frequency,
                                    (posArray[index].y - (mapSize / 2) + octaveOffset[i].y) / noiseSetting._scale * frequency);

                float perlinValue = Mathf.PerlinNoise(sample.x, sample.y) * 2 - 1;
                noiseH += perlinValue * amplitude;

                amplitude *= noiseSetting._persistance;
                frequency *= noiseSetting._lacunarity;
            }

            // if (noiseH > maxNoiseH) maxNoiseH = noiseH;
            // else if (noiseH < minNoiseH) minNoiseH = noiseH;

            float normalizedHeight = (noiseH + 1) / (maxPossibleH / 0.9f);
            result[index] += Mathf.Clamp(normalizedHeight, 0, int.MaxValue) * normalizedWeight;
            octaveOffset.Dispose();
        }

    }
    [BurstCompile]
    public struct GenerateNoiseMapLayerJob : IJob {
        public int mapSize;
        public NativeArray<float> result;
        public NoiseLayerSetting noiseSetting;
        public float normalizedWeight;
        public uint seed;
        public float2 offset;
        public void Execute() {
            Unity.Mathematics.Random rng = new(seed);
            NativeArray<float2> octaveOffset = new(noiseSetting._octaves, Allocator.Temp);
            float maxPossibleH = 0f;
            float amplitude = 1f;
            for (int i = 0; i < noiseSetting._octaves; i++) {
                float x = rng.NextInt(-100000, 100000) + offset.x;
                float y = rng.NextInt(-100000, 100000) + offset.y;

                maxPossibleH += amplitude;
                amplitude *= noiseSetting._persistance;
                octaveOffset[i] = new float2(x, y);
            }

            float maxNoiseH = float.MinValue;
            float minNoiseH = float.MaxValue;   

            for (int y = 0; y < mapSize; y++) {
                for (int x = 0; x < mapSize; x++) {
                    amplitude = 1f;
                    float frequency = 1f;
                    float noiseH = 0f;

                    for (int i = 0; i < noiseSetting._octaves; i++) {

                        float2 sample = new((x - (mapSize / 2) + octaveOffset[i].x) / noiseSetting._scale * frequency,
                                            (y - (mapSize / 2) + octaveOffset[i].y) / noiseSetting._scale * frequency);

                        float perlinValue = Mathf.PerlinNoise(sample.x, sample.y) * 2 - 1;
                        noiseH += perlinValue * amplitude;

                        amplitude *= noiseSetting._persistance;
                        frequency *= noiseSetting._lacunarity;
                    }

                    if (noiseH > maxNoiseH) maxNoiseH = noiseH;
                    else if (noiseH < minNoiseH) minNoiseH = noiseH;

                    float normalizedHeight = (noiseH + 1) / (maxPossibleH / 0.9f);
                    result[x + y * mapSize] += Mathf.Clamp(normalizedHeight, 0, int.MaxValue) * normalizedWeight;

                }
            }
            octaveOffset.Dispose();

        }
    }
    public struct GenerateNoiseMapJob : IJob {
        public int mapSize;
        public NativeArray<float> result;
        public NoiseLayerSetting nS;
        public uint seed;
        public float2 offset;
        public void Execute() {
            Unity.Mathematics.Random rng = new(seed);
            NativeArray<float2> octaveOffset = new(nS._octaves, Allocator.Temp);
            float maxPossibleH = 0f;
            float amplitude = 1f;
            for (int i = 0; i < nS._octaves; i++) {
                float x = rng.NextInt(-100000, 100000) + offset.x;
                float y = rng.NextInt(-100000, 100000) + offset.y;

                maxPossibleH += amplitude;
                amplitude *= nS._persistance;
                octaveOffset[i] = new float2(x, y);
            }

            float maxNoiseH = float.MinValue;
            float minNoiseH = float.MaxValue;

            for (int y = 0; y < mapSize; y++) {
                for (int x = 0; x < mapSize; x++) {
                    amplitude = 1f;
                    float frequency = 1f;
                    float noiseH = 0f;

                    for (int i = 0; i < nS._octaves; i++) {

                        float2 sample = new((x - (mapSize / 2) + octaveOffset[i].x) / nS._scale * frequency,
                                            (y - (mapSize / 2) + octaveOffset[i].y) / nS._scale * frequency);

                        float perlinValue = Mathf.PerlinNoise(sample.x, sample.y) * 2 - 1;
                        noiseH += perlinValue * amplitude;

                        amplitude *= nS._persistance;
                        frequency *= nS._lacunarity;
                    }

                    if (noiseH > maxNoiseH) maxNoiseH = noiseH;
                    else if (noiseH < minNoiseH) minNoiseH = noiseH;
                    result[x + y * mapSize] = noiseH;

                    float normalizedHeight = (result[x + y * mapSize] + 1) / (maxPossibleH / 0.9f);
                    result[x + y * mapSize] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
            octaveOffset.Dispose();
        }
    }


}