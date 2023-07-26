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
            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.Last();
                Open.Remove(CurrentNode);

                if (CurrentNode == EndNode)
                {
                    return RetracePath();
                }
                
                Close.Add(CurrentNode);
                var adjs = Grid.GetAdjacentsNodes(CurrentNode);

                foreach (var adj in adjs)
                {
                    if (!Close.Contains(adj) && !Open.Contains(adj))
                    {
                        adj.parent = CurrentNode;
                        Open.Add(adj);
                    } 
                }
            }

            Debug.Log("error " + CurrentNode.position);
            return new List<Node>();
        }
    }
}
