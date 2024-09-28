using System;
using MyUtils.Structs;
using UnityEngine;

namespace MyUtils.Classes {

    [Serializable]
    public class NoiseSettingData {
        [Tooltip("Settings for multiple layers of noise.")]
        public MultipleLayerNoiseSetting _settings;

        [Tooltip("Noise setting for environment noise.")]
        public NoiseLayerSetting _environmentNoise;

        [Tooltip("Seed value for all noises.")]
        public uint _seed;
    }


    // [Serializable]
    // public class PathFindingCellItem {
    //     public int _x;
    //     public int _y;
    //     public Vector2Int _worldPos;
    //     public ChunkItem<GameObject> _cell;
    //     public PathFindingCellItem _previous;
    //     public int _gCost;
    //     public int _hCost;
    //     public int _fCost;
    //     public void CalculateFCost() {
    //         _fCost = _hCost + _gCost;
    //     }

    public struct PathfindingCellItem {
        public bool _isWalkable;
        public int _index;
        public int _x;
        public int _y;
        public int _previousIndex;
        public int _gCost;
        public int _hCost;
        public int _fCost;
        public void CalculateIndex(int length) {
            _index = _x * length + _y;
        }
        public void CalculateFCost() {
            _fCost = _hCost + _gCost;
        }
    }
}

