using Chars.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class BreadthFirst : PathFinding
    {
        public BreadthFirst(Grid grid, Node start, Node end) : base(grid, start, end)
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

                if (CurrentNode == EndNode)
                {
                    return RetracePath();
                }

                Open.Remove(CurrentNode);
                Close.Add(CurrentNode);

                var neighbors = Grid.GetAdjacentsNodes(CurrentNode, ref MathUtils.FourDirectionsInt);

                foreach (var currentNeighbor in neighbors)
                {
                    if (!Close.Contains(currentNeighbor) 
                        && Grid.Nodes[currentNeighbor.position.x, currentNeighbor.position.y].type != (int)Tiles.OBSTACLE)
                    {
                        currentNeighbor.parent = CurrentNode;
                        Open.Add(currentNeighbor);
                    }
                }
            }

            return new List<Node>();
        }
    }
}
