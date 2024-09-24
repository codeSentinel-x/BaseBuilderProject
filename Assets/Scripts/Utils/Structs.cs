using System;
using UnityEngine;

namespace MyUtils.Structs {

    [Serializable]
    public struct NoiseLayerSetting {
        [Tooltip("Higher values create higher resolution terrain")]
        public float _scale;

        [Tooltip("Higher values add more detail.")]
        public int _octaves;

        [Range(0, 1)]
        [Tooltip("Controls the amplitude of each octave.")]
        public float _persistance;

        [Tooltip("Controls the frequency of each octave.")]
        public float _lacunarity;



    }
    [Serializable]
    public struct WeightedNoiseSetting {
        [Tooltip("")]
        public NoiseLayerSetting _noiseSetting;

        [Tooltip("Weight of the noise layer in the overall noise. Higher values make this layer more important.")]
        public float _weight;

    }
    [Serializable]
    public struct MultipleLayerNoiseSetting {
        [Tooltip("Array of weighted noise settings for multiple layers.")]
        public WeightedNoiseSetting[] _weightedNoiseSettings;

        [Tooltip("Size of each chunk (in pixels)")]
        public int _chunkSize;

        [Tooltip("Number of chunks to render.")]
        public int _chunkRenderDistance;

    }
    [Serializable]
    public struct TerrainRule {
        [Tooltip("Gradient for terrain coloring.")]
        public Gradient _gradient;

        [Tooltip("Maximum height for the terrain rule to apply.")]
        public float _maxHeight;

        [Tooltip("Name of the cell (From terrain rule).")]
        public string _cellName;

        [Tooltip("Determines if the terrain is walkable (For pathfinding).")]
        public bool _isWalkable;

    }

    [Serializable]
    public struct ChunkItem<T> {
        [Tooltip("Name of the terrain type.")]
        public string _terrainTypeName;

        [Tooltip("Height of the cell from noise map.")]
        public float _cellHeight;

        [Tooltip("Determines if the terrain is walkable (For pathfinding).")]
        public bool _isWalkable;

        [Tooltip("Fully customizable content of the chunk. Typeof T")]
        public T _content;
        public bool isEmpty;
        public string biomeName;
    }


}
