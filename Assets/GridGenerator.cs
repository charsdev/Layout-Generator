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
        private Grid _grid;
        [SerializeField] private float _offset = 1.25f;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField][Multiline] public string _map;

        private readonly Dictionary<int, Color> _colors = new Dictionary<int, Color>()
        {
            { (int)Tiles.FREE, Color.white },
            { (int)Tiles.OBSTACLE, Color.red },
            { (int)Tiles.DOOR, Color.blue },
        };

        private void Start()
        {
            _grid = new Grid(Width, Height, transform.position, _offset);

            var doors = new List<Vector2Int>
            {
                new Vector2Int(_grid.HalfWidth, _grid.HeightMinusOne), // Top door
                new Vector2Int(_grid.HalfWidth, 0), // Bottom door
                new Vector2Int(_grid.WidthMinusOne, _grid.HalfHeight), // Right door
                new Vector2Int(0, _grid.HalfHeight ) // Left door
            };

            foreach (Vector2Int doorPosition in doors)
            {
                _grid.Nodes[doorPosition.x, doorPosition.y].Type = (int)Tiles.DOOR;
            }

            GenerateValidRoom(ref _grid, doors);
        }

        private void Update()
        {
            foreach (var node in _grid.Nodes)
            {
                if (node.Type == (int)Tiles.DOOR)
                {
                    continue;
                }

                bool collisionCondition = Physics2D.OverlapBox(node.WorldPosition, Vector2.one, 0, _layerMask);

                if (node.Walkable && node.Type == (int)Tiles.FREE && collisionCondition)
                {
                    node.Walkable = false;
                    node.Type = (int)Tiles.OBSTACLE;
                }
                else if (!node.Walkable && node.Type == (int)Tiles.OBSTACLE && !collisionCondition)
                {
                    node.Walkable = true;
                    node.Type = (int)Tiles.FREE;
                }
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
                    doorsPositions.Add(new Vector2Int(grid.HalfWidth, grid.HalfHeight));
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
                new Vector2Int(0, grid.HalfHeight)
            };

            GenerateObstaclesAtRegion(ref grid, grid.HalfHeight, grid.HalfWidth, regions[randomIndexRegion], ref obstacleCount);
            InvertRegions(ref grid, grid.HalfHeight, grid.HalfWidth, randomIndexRegion);

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
                            grid.Nodes[x + halfWidth, y + halfHeight].Type = grid.Nodes[halfWidth - x, halfHeight - y].Type;
                        }

                        if (!allowSimetry)
                            continue;

                        //invierte lado 4 en lado 3;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(x, halfHeight - y, grid))
                        {
                            grid.Nodes[x, y + halfHeight].Type = grid.Nodes[x, halfHeight - y].Type;
                        }

                        //invierte lado 3 en lado 2;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, y, grid))
                        {
                            grid.Nodes[x + halfWidth, y].Type = grid.Nodes[halfWidth - x, y].Type;
                        }
                    }
                    else
                    {
                        //invierte lado 3 en lado 2;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, halfWidth - y, grid))
                        {
                            grid.Nodes[x + halfWidth, y].Type = grid.Nodes[halfWidth - x, halfWidth - y].Type;
                        }

                        if (!allowSimetry)
                            continue;

                        //invierte lado 2 en lado 4;
                        if (IsFreeTile(x + halfWidth, y + halfHeight, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            grid.Nodes[x + halfWidth, y + halfHeight].Type = grid.Nodes[halfWidth - x, halfHeight + y].Type;
                        }

                        //invierte lado 4 en lado 1;
                        if (IsFreeTile(halfWidth - x, halfHeight - y, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            grid.Nodes[halfWidth - x, halfHeight - y].Type = grid.Nodes[halfWidth - x, halfHeight + y].Type;
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
                    mirrorGrid[x, y] = grid.Nodes[grid.WidthMinusOne - x, y];
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
                            grid.Nodes[currentCellPosition.x, currentCellPosition.y].Type = (int)Tiles.OBSTACLE;
                            obstacleCount--;
                        }
                    }
                }
            }
        }

        private bool IsFreeTile(int x, int y, Grid grid)
        {
            return grid.Nodes[x, y].Type == (int)Tiles.FREE;
        }

        private bool IsDoorTile(int x, int y, Grid grid)
        {
            return grid.Nodes[x, y].Type == (int)Tiles.DOOR;
        }

        private void ClearGrid(ref Grid grid)
        {
            foreach (var node in grid.Nodes)
            {
                if (node.Type != (int)Tiles.FREE && node.Type != (int)Tiles.DOOR)
                {
                    node.Type = (int)Tiles.FREE;
                }
            }
        }

        private void DrawGrid(ref Grid grid)
        {
            var floorHalfWidth = Width >> 1;
            var floorHalfHeight = Height >> 1;

            Vector2 bottonLeft = transform.position - Vector3.right * floorHalfWidth - Vector3.up * floorHalfHeight;
            var WorldPosition = bottonLeft + new Vector2(floorHalfWidth, floorHalfHeight) * _offset;

            Gizmos.DrawWireCube(WorldPosition, new Vector3(Width * _offset, Height * _offset));

            if (!Application.isPlaying) 
                return;

            if (grid != null)
            {
                foreach (var node in grid.Nodes)
                {
                    Gizmos.color = _colors[node.Type];
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_offset - 0.1f));
                }
            }
        }

        private void OnDrawGizmos()
        {
            DrawGrid(ref _grid);
        }

        private IEnumerator DrawPath(List<Node> path)
        {
            int index = 0;
            foreach (Node node in path)
            {
                if (index != path.Count - 1)
                {
                    yield return new WaitForSeconds(0.05f);
                    var x = node.GridPosition.x;
                    var y = node.GridPosition.y;
                    //SetCellColor(_cells[x, y], Color.green);
                    index++;
                }
            }
        }
    }
}