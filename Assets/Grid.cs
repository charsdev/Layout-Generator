using System.Collections.Generic;
using UnityEngine;
using MathUtils = Chars.Utils.MathUtils;

namespace Chars.Pathfinding
{
    [System.Serializable]
    public class Grid
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Node[,] Nodes { get; set; }

        private List<Node> _neighbors = new();

        public readonly float NodeOffset;
        public readonly int HalfHeight;
        public readonly int HalfWidth;
        public readonly int WidthMinusOne;
        public readonly int HeightMinusOne;
        //public readonly int HalfWidthMinusOne;
        //public readonly int HalfHeightMinusOne;

        public LayerMask obstacleLayer;

        public Grid(int width, int height, Vector3 center, float nodeOffset)
        {
            Width = width;
            Height = height;
            Nodes = new Node[width, height];
            NodeOffset = nodeOffset;
            HalfHeight = Height >> 1;
            HalfWidth = Width >> 1;
            HeightMinusOne = Height - 1;
            WidthMinusOne = Width - 1;
            //HalfHeightMinusOne = HeightMinusOne >> 1;
            //HalfWidthMinusOne = WidthMinusOne >> 1;

            Vector2 bottonLeft = center - Vector3.right * HalfWidth - Vector3.up * HalfHeight;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var point = bottonLeft + new Vector2(x, y) * nodeOffset;

                    Nodes[x, y] = new Node
                    {
                        Type = (int)Tiles.FREE,
                        GridPosition = new Vector2Int(x, y),
                        Cost = Random.Range(0, 10),
                        WorldPosition = point
                    };
                                   
                }
            }
        }


        public List<Node> GetAdjacentsNodes(Node node, ref Vector2Int[] directions)
        {
            _neighbors.Clear();

            foreach (var direction in directions)
            {
                Vector2Int neighborPos = node.GridPosition + direction;

                if (MathUtils.InsideGridLimits(neighborPos.x, neighborPos.y, Width, Height))
                {
                    var neighborNode = Nodes[neighborPos.x, neighborPos.y];
                    _neighbors.Add(neighborNode);
                }
            }

            return _neighbors;
        }
    }

}


