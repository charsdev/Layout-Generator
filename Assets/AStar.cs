using Chars.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class AStar : PathFinding
    {
        public AStar(Grid grid, Node start, Node end) : base(grid, start, end)
        {
        }

        public override List<Node> FindPath()
        {
            Open.Clear();
            Close.Clear();

            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.First();

                foreach (var node in Open)
                {
                    if (node.Priority <= CurrentNode.Priority)
                    {
                        CurrentNode = node;
                    }
                }

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
                        && Grid.Nodes[adj.GridPosition.x, adj.GridPosition.y].Type != (byte)Tiles.OBSTACLE)
                    {
                        var tentativeCost = adj.Cost + GetManhattanDistance(CurrentNode, adj);

                        //float tentativeCost = CurrentNode.cost + adj.cost;

                        if (adj.Cost < CurrentNode.Cost || !Open.Contains(adj))
                        {
                            adj.Parent = CurrentNode;
                            adj.Cost = tentativeCost;
                            adj.Heuristic = GetManhattanDistance(adj, EndNode);
                            adj.Priority = adj.Cost + adj.Heuristic;

                            Open.Add(adj);
                        }
                    }
                }
            }

            return new List<Node>();
        }

        private float GetManhattanDistance(Node source, Node target)
        {
            return Mathf.Abs(source.GridPosition.x - target.GridPosition.x) + Mathf.Abs(source.GridPosition.y - target.GridPosition.y);
        }
    }

}
