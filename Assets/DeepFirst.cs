using Chars.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class DeepFirst : PathFinding
    {
        public DeepFirst(Grid grid, Node start, Node end) : base(grid, start, end)
        {
        }

        public override List<Node> FindPath()
        {
            Open.Clear();
            Close.Clear();

            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.Last();

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
                        adj.Parent = CurrentNode;
                        Open.Add(adj);
                    } 
                }
            }

            return new List<Node>();
        }
    }
}
