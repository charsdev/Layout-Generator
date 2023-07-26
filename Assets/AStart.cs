using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class AStart : PathFinding
    {
        public AStart(Grid grid, Node start, Node end) : base(grid, start, end)
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

                Open.Remove(CurrentNode);
                Close.Add(CurrentNode);

                if (CurrentNode == EndNode)
                {
                    return RetracePath();
                }

                var adjs = Grid.GetAdjacentsNodes(CurrentNode);

                foreach (var adj in adjs)
                {
                    if (Close.Contains(adj))
                    {
                        continue;
                    }

                    var tentativeCost = adj.cost + GetManhattanDistance(CurrentNode, adj);

                    if (!Open.Contains(adj) || tentativeCost < CurrentNode.cost)
                    {
                        adj.cost = tentativeCost;
                        adj.heuristic = GetManhattanDistance(adj, EndNode);
                        adj.priority = adj.cost + adj.heuristic;
                        adj.parent = CurrentNode;
                    }

                    Open.Add(adj);
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
