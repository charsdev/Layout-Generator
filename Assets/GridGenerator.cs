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

            var doors = new List<Vector2Int>();
            doors.Add(new Vector2Int(Width / 2, Height - 1)); // Top door
            doors.Add(new Vector2Int(Width / 2, 0)); // Bottom door
            doors.Add(new Vector2Int(Width - 1, Height / 2)); // Right door
            doors.Add(new Vector2Int(0, Height / 2)); // Left door

            var path = new List<Vector2Int>();

            foreach (Vector2Int doorPosition in doors)
            {
                var currentDoor = _cells[doorPosition.x, doorPosition.y];
                int nodePos = GetIndex(doorPosition, Width);
                _grid.Nodes[nodePos].type = (int)Tiles.DOOR;
                SetCellColor(currentDoor, Color.blue);
            }

            GenerateValidRoom(ref _grid, path);
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
            if (Input.GetKeyDown(KeyCode.Return))
            {
                var path = new List<Vector2Int>();
                GenerateValidRoom(ref _grid, path);
                PaintGrid(_grid);
            }
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Return)) 
        //    {
        //        Node start = _grid.Nodes[0];
        //        SetCellColor(_cells[0, 0], Color.blue);
        //        Node end = _grid.Nodes[^1];
        //        SetCellColor(_cells[Width - 1, Height - 1], Color.red);

        //        var astar = new AStart(_grid, start, end);
        //        var path = astar.FindPath();

        //        StartCoroutine(DrawPath(path));
        //    }

        //    if (Input.anyKeyDown)
        //    {
        //        int type = -1;

        //        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        //        {
        //            type = 1;
        //        }
        //        else if (Input.GetMouseButtonDown((int)MouseButton.Right))
        //        {
        //            type = 2;
        //        }
        //        else if (Input.GetMouseButtonDown((int)MouseButton.Middle))
        //        {
        //            type = 0;
        //        }

        //        if (type != -1)
        //        {
        //            Vector3 worldMousePos = GetMouseWorldPosition() - _pivot.position;
        //            int x = Mathf.RoundToInt(worldMousePos.x / _offset);
        //            int y = Mathf.RoundToInt(worldMousePos.y / _offset);

        //            if (MathUtils.InsideGridLimits(x, y, _grid.Width, _grid.Height))
        //            {
        //                var nodePosition = new Vector2Int(x, y);
        //                int nodeIndex = GetIndex(nodePosition, _grid.Width);
        //                print($"{nodeIndex} : {nodePosition}");
        //                _grid.Nodes[nodeIndex].type = type;

        //                if (type == 2)
        //                {
        //                    _grid.Nodes[nodeIndex].cost = 100;
        //                    _grid.Nodes[nodeIndex].walkable = false;
        //                }

        //                SetCellColor(_cells[nodePosition.x, nodePosition.y], _colors[type]);

        //                StringBuilder sb = new StringBuilder();
        //                for (int i = Height - 1; i >= 0; i--)
        //                {
        //                    for (int j = 0; j < Width; j++)
        //                    {
        //                        var index = GetIndex(new Vector2Int(j, i), _grid.Width);
        //                        sb.Append(_grid.Nodes[index].type);
        //                        sb.Append(' ');
        //                    }
        //                    sb.AppendLine();
        //                }
        //                Debug.Log(sb.ToString());
        //                _map = sb.ToString();
        //            }
        //        }
        //    }
        //}

        private void GenerateValidRoom(ref Grid grid, List<Vector2Int> path)
        {
            bool allDoorsHasPath;

            do
            {
                allDoorsHasPath = true;
                ClearGrid(ref grid);
                GenerateObstacles(ref grid);

                //for (int i = 0; i < points.Count - 1; i++)
                //{
                //    var currentPath = pathfinding.FindPath(points[i], points[i + 1], ref _grid);

                //    if (currentPath.Count == 0)
                //    {
                //        allDoorsHasPath = false;
                //        break;
                //    }

                //    path.AddRange(currentPath);
                //}

                //if (allDoorsHasPath && path.Count > 0)
                //{
                //    path.RemoveAt(path.Count - 1);
                //    PrintPath(_grid, path);
                //}

                //Console.WriteLine("found it!");
                //PrintGrid(_grid);
            }
            while (!allDoorsHasPath);
        }

        private void GenerateObstacles(ref Grid grid)
        {
            int height = grid.Height - 1;
            int width = grid.Width - 1;
            int halfHeight = height >> 1;
            int halfWidth = width >> 1;

            int obstacleCount = 7; //Random.Range(0, 13);
            int randomIndexRegion = 0; //Random.Range(0, 2);
            bool mirror = true; //Random.value > 0.5f;

            Vector2Int[] regions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, halfHeight)
            };

            GenerateObstaclesAtRegion(ref grid, halfHeight, halfWidth, regions[randomIndexRegion], ref obstacleCount);
            InvertSides(ref grid, halfHeight, halfWidth, randomIndexRegion);
            grid.Nodes = MirrorGrid(grid.Nodes);

            //if (mirror)
            //{
            //}
        }

        private void InvertSides(ref Grid grid, int halfHeight, int halfWidth, int randomIndexRegion)
        {
            bool allowSimetry = true;//Random.value > 0.5f;


            for (int y = 0; y <= halfHeight; y++)
            {
                for (int x = 0; x <= halfWidth; x++)
                {
                    if (randomIndexRegion == 0)
                    {
                        //invierte lado 1 en lado 4;
                        if (IsFreeTile(x + halfWidth, y + halfHeight, _grid) && !IsDoorTile(halfWidth - x, halfHeight - y, grid))
                        {
                            int indexA = GetIndex(new Vector2Int(x + halfWidth, y + halfHeight), grid.Width);
                            int IndexB = GetIndex(new Vector2Int(halfWidth - x, halfHeight - y), grid.Width);
                            grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                        }
                    }
                    
                    if (!allowSimetry)
                        continue;

                    //invierte lado 4 en lado 3;
                    if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(x, halfHeight - y, grid))
                    {
                        int indexA = GetIndex(new Vector2Int(x, y + halfHeight), grid.Width);
                        int IndexB = GetIndex(new Vector2Int(x, halfHeight - y), grid.Width);
                        grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                    }

                    //invierte lado 3 en lado 2;
                    if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, y, grid))
                    {
                        int indexA = GetIndex(new Vector2Int(x + halfWidth, y), grid.Width);
                        int IndexB = GetIndex(new Vector2Int(halfWidth - x, y), grid.Width);
                        grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                    }
                    else
                    {
                        //invierte lado 3 en lado 2;
                        if (IsFreeTile(x + halfWidth, y, grid) && !IsDoorTile(halfWidth - x, halfWidth - y, grid))
                        {
                            int indexA = GetIndex(new Vector2Int(x + halfWidth, y), grid.Width);
                            int IndexB = GetIndex(new Vector2Int(halfWidth - x, halfWidth - y), grid.Width);
                            grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                        }

                        if (!allowSimetry)
                            continue;

                        //invierte lado 2 en lado 4;
                        if (IsFreeTile(x + halfWidth, y + halfHeight, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            int indexA = GetIndex(new Vector2Int(x + halfWidth, y + halfHeight), grid.Width);
                            int IndexB = GetIndex(new Vector2Int(halfWidth - x, halfHeight + y), grid.Width);
                            grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                        }

                        //invierte lado 4 en lado 1;
                        if (IsFreeTile(halfWidth - x, halfHeight - y, grid) && !IsDoorTile(halfWidth - x, halfHeight + y, grid))
                        {
                            int indexA = GetIndex(new Vector2Int(halfWidth - x, halfHeight - y), grid.Width);
                            int IndexB = GetIndex(new Vector2Int(halfWidth - x, halfHeight + y), grid.Width);
                            grid.Nodes[indexA].type = grid.Nodes[IndexB].type;
                        }
                    }

               
                }
            }
        }

        private Node[] MirrorGrid(Node[] nodes)
        {
            //Node[] mirrorGrid = new Node[Width * Height];

            //for (int y = 0; y < Height; y++)
            //{
            //    for (int x = 0; x < Width; x++)
            //    {
            //        int originalIndex = y * Width + x;
            //        int mirroredIndex = y * Width + (Width - x - 1);
            //        mirrorGrid[originalIndex] = grid.Nodes[mirroredIndex];
            //    }
            //}

            //grid.Nodes = mirrorGrid;

            int midColumn = Width / 2;

            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < midColumn; col++)
                {
                    int leftIndex = row * Height + col;
                    int rightIndex = (row + 1) * Height - col - 1;

                    // Intercambiar los valores para reflejar la matriz
                    var temp = nodes[leftIndex];
                    nodes[leftIndex] = nodes[rightIndex];
                    nodes[rightIndex] = temp;
                }
            }

            return nodes;
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
                        var pos = GetIndex(currentCellPosition, Width);

                        if (grid.Nodes[pos].type == (int)Tiles.FREE && grid.Nodes[pos].type != (int)Tiles.DOOR)
                        {
                            grid.Nodes[pos].type = (int)Tiles.OBSTACLE;
                            obstacleCount--;
                        }
                    }
                }
            }
        }

        private bool IsFreeTile(int x, int y, Grid grid)
        {
            int pos = GetIndex(new Vector2Int(x, y), grid.Width);
            return grid.Nodes[pos].type == (int)Tiles.FREE;
        }

        private bool IsDoorTile(int x, int y, Grid grid)
        {
            int pos = GetIndex(new Vector2Int(x, y), grid.Width);
            return grid.Nodes[pos].type == (int)Tiles.DOOR;
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

        private int GetIndex(Vector2Int nodePosition, int width)
        {
            return nodePosition.x + nodePosition.y * width;
        }

        private static Vector3 GetMouseWorldPosition()
        {
            var mousePos = Input.mousePosition;

            var worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            return worldMousePos;
        }

        private int GetIndex(Vector2Int position)
        {
            return position.x + position.y * Width;
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