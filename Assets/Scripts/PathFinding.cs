using System.Collections.Generic;
using System.Linq;
using MyUtils.Classes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PathFinding {

    public const int DIAGONAL_COST = 15;
    public const int STRAIGHT_COST = 10;

    public int _gridSize;
    public GridCell[,] _cells;

    NativeArray<PathfindingCellItem> gridOfCellItem;

    public PathFinding(Grid grid) {
        _gridSize = grid._cells.GetLength(0);
        _cells = grid._cells;
    }
    public NativeArray<PathfindingCellItem> GetGridInNativeArray() {
        gridOfCellItem = new(_gridSize * _gridSize, Allocator.Persistent);
        for (int x = 0; x < _gridSize; x++) {
            for (int y = 0; y < _gridSize; y++) {
                int pos = x * _gridSize + y;
                var p = new PathfindingCellItem() {
                    _isWalkable = _cells[x, y]._isWalkable,
                    _index = pos,

                };
                gridOfCellItem[pos] = p;
            }
        }
        return gridOfCellItem;
    }
    public List<int2> FindPath(int2 startPos, int2 endPos) {
        NativeList<int2> res = new(Allocator.TempJob);
        FindPathJob job = new() {
            gridOfCellItem = GetGridInNativeArray(),
            _gridSize = _gridSize,
            startPos = startPos,
            endPos = endPos,
            result = res,
        };
        JobHandle handle = job.Schedule();
        handle.Complete();
        return res.ToList<int2>();
    }

    [BurstCompile]
    public struct FindPathJob : IJob {
        public NativeArray<PathfindingCellItem> gridOfCellItem;
        public int _gridSize;
        public int2 startPos;
        public int2 endPos;
        public NativeList<int2> result;
        public void Execute() {
            result = FindPath(startPos, endPos);
        }

        public NativeList<int2> FindPath(int2 startPos, int2 endPos) {

            for (int x = 0; x < _gridSize; x++) {
                for (int y = 0; y < _gridSize; y++) {
                    int pos = x * _gridSize + y;
                    var p = new PathfindingCellItem() {
                        _isWalkable = gridOfCellItem[pos]._isWalkable,
                        _index = pos,
                        _previousIndex = -1,
                        _gCost = int.MaxValue,
                        _hCost = CalculateDistance(new int2(x, y), new int2(endPos.x, endPos.y)),

                    };
                    p.CalculateFCost();
                    gridOfCellItem[pos] = p;
                }
            }

            PathfindingCellItem startCell = gridOfCellItem[CalculateIndex(startPos.x, startPos.y, _gridSize)];
            startCell._gCost = 0;
            startCell._hCost = CalculateDistance(new int2(startCell._x, startCell._y), new int2(endPos.x, endPos.y));
            startCell.CalculateFCost();
            gridOfCellItem[startCell._index] = startCell;
            NativeArray<int2> neighborsIndexPos = new(new int2[]{
            new(0 , 1),
            new(1 , 1),
            new(1,0),
            new(0,-1),
            new(-1,-1),
            new(-1,0),
            new(-1,1),
            new(1,-1),
        }, Allocator.Temp);
            NativeList<int> _openList = new() { startCell._index };
            NativeList<int> _closedList = new();

            int endIndex = GetNode(endPos.x, endPos.y)._index;

            while (!_openList.IsEmpty) {
                int current = GetLowestFCost(gridOfCellItem, _openList);
                PathfindingCellItem item = gridOfCellItem[current];
                if (current == endIndex) {
                    return CalculatePath(item);
                }
                _openList.RemoveAtSwapBack(current);
                _closedList.Add(current);

                for (int i = 0; i < _openList.Length; i++) {
                    if (_openList[i] == current) {
                        _openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                for (int i = 0; i < neighborsIndexPos.Length; i++) {
                    int2 neighborOffset = neighborsIndexPos[i];
                    int2 neighborPos = new(item._x + neighborOffset.x, item._y + neighborOffset.y);
                    if (!IsInGridBounds(neighborPos)) {
                        continue;
                    }

                    int neighborIndex = CalculateIndex(neighborPos.x, neighborPos.y, _gridSize);

                    if (_closedList.Contains(neighborIndex)) continue;
                    PathfindingCellItem neigh = gridOfCellItem[neighborIndex];
                    if (!neigh._isWalkable) {
                        continue;
                    }
                    int tentativeCost = item._gCost + CalculateDistance(current, neighborPos);
                    if (tentativeCost < neigh._gCost) {
                        neigh._previousIndex = current;
                        neigh._gCost = tentativeCost;
                        // neigh._hCost = CalculateDistance(neigh._index, endPos);   
                        item.CalculateFCost();
                        gridOfCellItem[neigh._index] = neigh;
                    }
                    if (!_openList.Contains(neigh._index)) _openList.Add(neigh._index);

                }

            }

            neighborsIndexPos.Dispose();
            return default;

        }
        private bool IsInGridBounds(int2 pos) {
            return pos.x >= 0 && pos.y >= 0 && pos.x < _gridSize && pos.y < _gridSize;
        }
        private List<PathfindingCellItem> GetNeighborList(PathfindingCellItem currentNode) {
            List<PathfindingCellItem> neighborList = new();
            if (currentNode._x - 1 >= 0) {
                neighborList.Add(GetNode(currentNode._x - 1, currentNode._y));
                if (currentNode._y - 1 >= 0) neighborList.Add(GetNode(currentNode._x - 1, currentNode._y - 1));
                if (currentNode._y + 1 < _gridSize) neighborList.Add(GetNode(currentNode._x - 1, currentNode._y + 1));
            }
            if (currentNode._x + 1 < _gridSize) {
                neighborList.Add(GetNode(currentNode._x + 1, currentNode._y));
                if (currentNode._y - 1 >= 0) neighborList.Add(GetNode(currentNode._x + 1, currentNode._y - 1));
                if (currentNode._y + 1 < _gridSize) neighborList.Add(GetNode(currentNode._x + 1, currentNode._y + 1));
            }
            if (currentNode._y - 1 >= 0) neighborList.Add(GetNode(currentNode._x, currentNode._y - 1));
            if (currentNode._y + 1 < _gridSize) neighborList.Add(GetNode(currentNode._x, currentNode._y + 1));
            return neighborList;
        }

        private PathfindingCellItem GetNode(int x, int y) {
            return gridOfCellItem[x * _gridSize + y];
        }

        private NativeList<int2> CalculatePath(PathfindingCellItem endNode) {
            if (endNode._previousIndex == -1) {
                return new NativeList<int2>(Allocator.Temp);
            } else {
                NativeList<int2> path = new(Allocator.Temp) {
             new int2(endNode._x,endNode._y)
             };
                PathfindingCellItem current = endNode;
                while (current._previousIndex != -1) {
                    PathfindingCellItem previous = gridOfCellItem[current._previousIndex];
                    path.Add(new(previous._x, previous._y));
                    current = previous;
                }
                path.Reverse();
                return path;
            }
        }
        public int CalculateIndex(int x, int y, int length) {
            return x * length + y;
        }
        private int CalculateDistance(int2 a, int2 b) {
            int xDist = Mathf.Abs(a.x - b.x);
            int yDist = Mathf.Abs(a.y - b.y);
            int remain = Mathf.Abs(xDist - yDist);
            return DIAGONAL_COST * Mathf.Min(xDist, yDist) + STRAIGHT_COST * remain;
        }
        private int GetLowestFCost(NativeArray<PathfindingCellItem> pathNodes, NativeList<int> openList) {
            PathfindingCellItem lowestF = pathNodes[openList[0]];
            for (int i = 0; i < pathNodes.Length; i++) {
                if (pathNodes[openList[i]]._fCost < lowestF._fCost) lowestF = pathNodes[openList[i]];
            }
            return lowestF._index;

        }
    }

}