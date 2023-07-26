using Chars.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
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

        private readonly Dictionary<int, Color> _colors = new Dictionary<int, Color>()
        {
            { 0, Color.white },
            { 1, Color.blue  },
            { 2, Color.red   },
        };

        private void Start()
        {
            _grid = new Grid(Width, Height);
            _cells = new GameObject[Width, Height];

            DrawGrid(ref _grid, _pivot.transform.position);

            var doors = new List<Vector2Int>
            {
                new Vector2Int(Width / 2, Height - 1), // Top door
                new Vector2Int(Width / 2, 0), // Bottom door
                new Vector2Int(Width - 1, Height / 2), // Right door
                new Vector2Int(0, Height / 2) // Left door
            };


            foreach (Vector2Int doorPosition in doors)
            {
                var currentDoor = _cells[doorPosition.x, doorPosition.y];
                _grid.Nodes[doorPosition.x, doorPosition.y].type = (int)Tiles.DOOR;
                SetCellColor(currentDoor, Color.blue);
            }

            GenerateValidRoom(ref _grid, doors);
            PaintGrid(_grid);
        }

        private void PaintGrid(Grid grid)
        {
            foreach (Node node in grid.Nodes)
            {
                Color color = node.type switch
                {
                    (int)Tiles.OBSTACLE => Color.red,
                    (int)Tiles.DOOR => Color.blue,
                    (int)Tiles.FREE => Color.white,
                    _ => throw new NotImplementedException()
                };

                SetCellColor(_cells[node.position.x, node.position.y], color);
            }
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Return))
            //{
            //    var path = new List<Vector2Int>();
            //    //GenerateValidRoom(ref _grid, path);
            //    //PaintGrid(_grid);
            //}
        }

        private void GenerateValidRoom(ref Grid grid, List<Vector2Int> doorsPositions)
        {
            bool allDoorsHasPath;
            var pathfinding = new BreadthFirst();

            do
            {
                allDoorsHasPath = true;
                ClearGrid(ref grid);
                GenerateObstacles(ref grid);

                if (doorsPositions.Count == 1)
                {
                    doorsPositions.Add(new Vector2Int(Width / 2, Height / 2));
                }



                for (int i = 0; i < doorsPositions.Count - 1; i++)
                {
                    var currentPath = pathfinding.FindPath(
                        _grid.Nodes[doorsPositions[0].x, doorsPositions[0].y],
                        _grid.Nodes[doorsPositions[1].x, doorsPositions[1].y],
                        ref _grid
                    );

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
            int height = grid.Height - 1;
            int width = grid.Width - 1;
            int halfHeight = height >> 1;
            int halfWidth = width >> 1;

            int obstacleCount = Random.Range(0, 13);
            int randomIndexRegion = Random.Range(0, 2);
            bool mirror = Random.value > 0.5f;

            Vector2Int[] regions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, halfHeight)
            };

            GenerateObstaclesAtRegion(ref grid, halfHeight, halfWidth, regions[randomIndexRegion], ref obstacleCount);
            InvertRegions(ref grid, halfHeight, halfWidth, randomIndexRegion);

            if (mirror)
            {
                MirrorGrid(ref grid);
            }
        }

        private void InvertRegions(ref Grid grid, int halfHeight, int halfWidth, int randomIndexRegion)
        {
            bool allowSimetry = Random.value > 0.5f;

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
                    mirrorGrid[x, y] = grid.Nodes[_grid.Width - 1 - x, y];
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
                    if (Random.value > 0.5f && obstacleCount > 0)
                    {
                        var currentCellPosition = new Vector2Int(x + region.x, y + region.y);

                        if (grid.Nodes[currentCellPosition.x, currentCellPosition.y].type == (int)Tiles.FREE)
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


//Node start = _grid.Nodes[0];
//SetCellColor(cells[0, 0], Color.blue);

//Node end = _grid.Nodes[^1];
//SetCellColor(cells[Width - 1, Height - 1], Color.red);

//var doors = new List<Vector2Int>();
//doors.Add(new Vector2Int(Width / 2, Height - 1)); // Top door
//doors.Add(new Vector2Int(Width / 2, 0)); // Bottom door
//doors.Add(new Vector2Int(Width - 1, Height / 2)); // Right door
//doors.Add(new Vector2Int(0, Height / 2)); // Left door


//List<Node> path = new List<Node>();

//for (int i = 0; i < doors.Count; i++)
//{
//    var currentDoor = cells[doors[i].x, doors[i].y];
//    SetCellColor(currentDoor, Color.blue);
//}a

//StartCoroutine(GenerateMultiplePath(doors.ToArray()));

//start = _grid.Nodes[GetIndex(doors[2])];
//end = _grid.Nodes[GetIndex(doors[3])];

//dijkstra = new Dijkstra(_grid, start, end);
//path = dijkstra.FindPath();

//StartCoroutine(DrawPath(path));

//start = _grid.Nodes[GetIndex(doors[3])];
//end = _grid.Nodes[GetIndex(doors[0])];

//dijkstra = new Dijkstra(_grid, start, end);
//path = dijkstra.FindPath();

//StartCoroutine(DrawPath(path));

//List<Node> path = new List<Node>();

//DeepFirst deepFirst = new DeepFirst(_grid, start, end);
//path = deepFirst.FindPath();

//BreadthFirst breadthFirst = new BreadthFirst(_grid, start, end);
//path = breadthFirst.FindPath();

//Dijkstra dijkstra = new Dijkstra(_grid, start, end);
//path = dijkstra.FindPath();