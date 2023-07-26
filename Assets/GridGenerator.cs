using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chars.Pathfinding
{
    public enum Tiles
    {
        FREE = 0,
        OBSTACLE = 1,
        DOOR = 2
    }

    public class GridGenerator : MonoBehaviour
    {
        public int Width;
        public int Height;
        public GameObject _cellprefab;
        public Transform _pivot;
        private Grid _grid;
        private GameObject[,] _cells;
        private float _offset = 1.25f;

        [SerializeField][Multiline] public string _map;

        int _halfHeight;
        int _halfWidth;
        int _widthMinusOne;
        int _heightMinusOne;
        int _halfWidthMinusOne;
        int _halfHeightMinusOne;

        private readonly Dictionary<int, Color> _colors = new Dictionary<int, Color>()
        {
            { (int)Tiles.FREE, Color.white },
            { (int)Tiles.OBSTACLE, Color.red },
            { (int)Tiles.DOOR, Color.blue },
        };

        private void Start()
        {
            _grid = new Grid(Width, Height);
            _cells = new GameObject[Width, Height];

            _halfHeight = Height / 2;
            _halfWidth = Width / 2;

            _heightMinusOne = Height - 1;
            _widthMinusOne = Width - 1;

            _halfHeightMinusOne = _heightMinusOne >> 1;
            _halfWidthMinusOne = _widthMinusOne >> 1;

            DrawGrid(ref _grid, _pivot.transform.position);

            var doors = new List<Vector2Int>
            {
                new Vector2Int(_halfWidth, _heightMinusOne), // Top door
                new Vector2Int(_halfWidth, 0), // Bottom door
                new Vector2Int(_widthMinusOne, _halfHeight), // Right door
                new Vector2Int(0, _halfHeight ) // Left door
            };

            foreach (Vector2Int doorPosition in doors)
            {
                var currentDoor = _cells[doorPosition.x, doorPosition.y];
                _grid.Nodes[doorPosition.x, doorPosition.y].type = (int)Tiles.DOOR;
            }

            GenerateValidRoom(ref _grid, doors);
            PaintGrid(_grid);
        }

        private void PaintGrid(Grid grid)
        {
            foreach (Node node in grid.Nodes)
            {
                SetCellColor(_cells[node.position.x, node.position.y], _colors[node.type]);
            }
        }

        private void GenerateValidRoom(ref Grid grid, List<Vector2Int> doorsPositions)
        {
            bool allDoorsHasPath;
            var pathfinding = new AStar(grid, null, null);

            int iterations = 0;
            do
            {
                iterations++;
                if (iterations > 500)
                {
                    break;
                }

                allDoorsHasPath = true;
                ClearGrid(ref grid);
                GenerateObstacles(ref grid);

                if (doorsPositions.Count == 1)
                {
                    doorsPositions.Add(new Vector2Int(_halfWidth, _halfHeight));
                }

                int totalSize = doorsPositions.Count - 1;

                for (int i = 0, j = i + 1; i < totalSize; i++)
                {
                    var start = _grid.Nodes[doorsPositions[i].x, doorsPositions[i].y];
                    var end = _grid.Nodes[doorsPositions[j].x, doorsPositions[j].y];

                    pathfinding.SetStartNode(start);
                    pathfinding.SetEndNode(end);

                    var currentPath = pathfinding.FindPath();

                    if (currentPath.Count == 0)
                    {
                        allDoorsHasPath = false;
                        break;
                    }
                }
            }
            while (!allDoorsHasPath);
        }

        private void GenerateObstacles(ref Grid grid)
        {
            int obstacleCount = Random.Range(0, 13);
            int randomIndexRegion = Random.Range(0, 2);
            bool mirror = Random.Range(0f, 1f) > 0.5f;

            Vector2Int[] regions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, _halfHeightMinusOne)
            };

            GenerateObstaclesAtRegion(ref grid, _halfHeightMinusOne, _halfWidthMinusOne, regions[randomIndexRegion], ref obstacleCount);
            InvertRegions(ref grid, _halfHeightMinusOne, _halfWidthMinusOne, randomIndexRegion);

            if (mirror)
            {
                MirrorGrid(ref grid);
            }
        }

        private void InvertRegions(ref Grid grid, int halfHeight, int halfWidth, int randomIndexRegion)
        {
            bool allowSimetry = Random.Range(0f, 1f) > 0.5f;

            for (int y = 0; y <= halfHeight; y++)
            {
                for (int x = 0; x <= halfWidth; x++)
                {
                    if (randomIndexRegion == 0)
                    {
                        //invierte lado 1 en lado 4;
                        if (IsFreeTile(x + halfWidth, y + halfHeight, _grid) && !IsDoorTile(halfWidth - x, halfHeight - y, grid))
                        {
                            grid.Nodes[x + halfWidth, y + halfHeight].type = grid.Nodes[halfWidth - x, halfHeight - y].type;
                        }

                        if (!allowSimetry)
                            continue;

                        //invierte lado 4 en lado 3;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(x, halfHeight - y, grid))
                        {
                            grid.Nodes[x, y + halfHeight].type = grid.Nodes[x, halfHeight - y].type;
                        }

                        //invierte lado 3 en lado 2;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, y, grid))
                        {
                            grid.Nodes[x + halfWidth, y].type = grid.Nodes[halfWidth - x, y].type;
                        }
                    }
                    else
                    {
                        //invierte lado 3 en lado 2;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, halfWidth - y, grid))
                        {
                            grid.Nodes[x + halfWidth, y].type = grid.Nodes[halfWidth - x, halfWidth - y].type;
                        }

                        if (!allowSimetry)
                            continue;

                        //invierte lado 2 en lado 4;
                        if (IsFreeTile(x + halfWidth, y + halfHeight, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            grid.Nodes[x + halfWidth, y + halfHeight].type = grid.Nodes[halfWidth - x, halfHeight + y].type;
                        }

                        //invierte lado 4 en lado 1;
                        if (IsFreeTile(halfWidth - x, halfHeight - y, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            grid.Nodes[halfWidth - x, halfHeight - y].type = grid.Nodes[halfWidth - x, halfHeight + y].type;
                        }
                    }
                }
            }
        }

        private void MirrorGrid(ref Grid grid)
        {
            Node[,] mirrorGrid = new Node[grid.Width, grid.Height];

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    mirrorGrid[x, y] = grid.Nodes[_widthMinusOne - x, y];
                }
            }

            grid.Nodes = mirrorGrid;
        }

        private void GenerateObstaclesAtRegion(ref Grid grid, int halfHeight, int halfWidth, Vector2Int region, ref int obstacleCount)
        {
            for (int y = 0; y <= halfHeight; y++)
            {
                for (int x = 0; x <= halfWidth; x++)
                {
                    if (Random.Range(0f, 1f) > 0.5f && obstacleCount > 0)
                    {
                        var currentCellPosition = new Vector2Int(x + region.x, y + region.y);

                        if (IsFreeTile(currentCellPosition.x, currentCellPosition.y, grid))
                        {
                            grid.Nodes[currentCellPosition.x, currentCellPosition.y].type = (int)Tiles.OBSTACLE;
                            obstacleCount--;
                        }
                    }
                }
            }
        }

        private bool IsFreeTile(int x, int y, Grid grid)
        {
            return grid.Nodes[x, y].type == (int)Tiles.FREE;
        }

        private bool IsDoorTile(int x, int y, Grid grid)
        {
            return grid.Nodes[x, y].type == (int)Tiles.DOOR;
        }

        private void ClearGrid(ref Grid grid)
        {
            foreach (var node in grid.Nodes)
            {
                if (node.type != (int)Tiles.FREE && node.type != (int)Tiles.DOOR)
                {
                    node.type = (int)Tiles.FREE;
                }
            }
        }

        private void SetCellColor(GameObject cell, Color color)
        {
            cell.GetComponent<Renderer>().material.color = color;
        }

        private void DrawGrid(ref Grid grid, Vector3 initPosition)
        {
            foreach (var node in grid.Nodes)
            {
                var go = Instantiate(_cellprefab, (Vector2)initPosition + (Vector2)node.position * _offset, Quaternion.identity);
                go.name = $"{node.position.x},{node.position.y}";
                _cells[node.position.x, node.position.y] = go;
            }
        }

        private IEnumerator DrawPath(List<Node> path)
        {
            int index = 0;
            foreach (Node node in path)
            {
                if (index != path.Count - 1)
                {
                    yield return new WaitForSeconds(0.05f);
                    var x = node.position.x;
                    var y = node.position.y;
                    SetCellColor(_cells[x, y], Color.green);
                    index++;
                }
            }
        }
    }
}