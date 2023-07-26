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
                    if (node.priority <= CurrentNode.priority)
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
                        && Grid.Nodes[adj.position.x, adj.position.y].type != (byte)Tiles.OBSTACLE)
                    {
                        var tentativeCost = adj.cost + GetManhattanDistance(CurrentNode, adj);

                        //float tentativeCost = CurrentNode.cost + adj.cost;

                        if (adj.cost < CurrentNode.cost || !Open.Contains(adj))
                        {
                            adj.parent = CurrentNode;
                            adj.cost = tentativeCost;
                            adj.heuristic = GetManhattanDistance(adj, EndNode);
                            adj.priority = adj.cost + adj.heuristic;

                            Open.Add(adj);
                        }
                    }
                }
            }

            return new List<Node>();
        }

        private float GetManhattanDistance(Node source, Node target)
        {
            return Mathf.Abs(source.position.x - target.position.x) + Mathf.Abs(source.position.y - target.position.y);
        }
    }

}
