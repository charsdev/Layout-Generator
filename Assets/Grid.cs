using Chars.Utils;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            Nodes = new Node[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Nodes[x, y] = new Node
                    {
                        type = 0,
                        position = new Vector2Int(x, y),
                        cost = Random.Range(0, 10)
                    };
                }
            }
        }


        public List<Node> GetAdjacentsNodes(Node node, ref Vector2Int[] directions)
        {
            _neighbors.Clear();

            foreach (var direction in directions)
            {
                Vector2Int neighborPos = node.position + direction;

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


