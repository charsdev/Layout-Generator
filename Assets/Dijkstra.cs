using Chars.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class Dijkstra : PathFinding
    {
        public Dijkstra(Grid grid, Node start, Node end) : base(grid, start, end)
        {
        }

        public override List<Node> FindPath()
        {
            StartNode.Cost = 0;
            Open.Clear();
            Close.Clear();

            foreach (var item in Grid.Nodes)
            {
                item.Cost = UnityEngine.Random.Range(0, 10);
            }

            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.First();
                CurrentNode = GetBestNode(CurrentNode);

                if (CurrentNode == EndNode)
                {
                    return RetracePath();
                }

                Open.Remove(CurrentNode);
                Close.Add(CurrentNode);

                var adjs = Grid.GetAdjacentsNodes(CurrentNode, ref MathUtils.FourDirectionsInt);

                foreach (var adj in adjs)
                {
                    if (!Close.Contains(adj)
                        && Grid.Nodes[adj.GridPosition.x, adj.GridPosition.y].Type != (int)Tiles.OBSTACLE)
                    {
                        float tentativeCost = CurrentNode.Cost + adj.Cost;

                        if (adj.Cost < CurrentNode.Cost)
                        {
                            adj.Parent = CurrentNode;
                            adj.Cost = tentativeCost;
                            Open.Add(adj);
                        }
                    }
                }
            }

            return new List<Node>();
        }

        private Node GetBestNode(Node currentNode)
        {
            return Open.FirstOrDefault(node => node.Cost <= currentNode.Cost);
        }

        private float GetManhattanDistance(Node source, Node target)
        {
            return Mathf.Abs(source.GridPosition.x - target.GridPosition.x) + Mathf.Abs(source.GridPosition.y - target.GridPosition.y);
        }
    }
}

