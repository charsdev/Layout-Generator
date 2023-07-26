using System.Collections.Generic;
using System.Linq;

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
            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.First();
                Open.Remove(CurrentNode);

                if (CurrentNode == EndNode)
                {
                    return RetracePath();
                }

                if (!Close.Contains(CurrentNode))
                {
                    Close.Add(CurrentNode);
                    var adjs = Grid.GetAdjacentsNodes(CurrentNode);

                    foreach (var adj in adjs)
                    {
                        if (!Close.Contains(adj) && !Open.Contains(adj))
                        {
                            Open.Add(adj);
                            adj.parent = CurrentNode;
                        }
                    }
                }
            }

            return new List<Node>();
        }

    }
}
