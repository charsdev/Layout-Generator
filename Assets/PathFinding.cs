using System.Collections.Generic;
using System.Linq;

namespace Chars.Pathfinding
{
    public abstract class PathFinding
    {
        protected HashSet<Node> Open = new HashSet<Node>();
        protected HashSet<Node> Close = new HashSet<Node>();
        protected Node CurrentNode;
        protected Grid Grid;
        protected Node StartNode;
        protected Node EndNode;

        public void SetStartNode(Node node) => StartNode = node;
        public void SetEndNode(Node node) => EndNode = node;

        public PathFinding(Grid grid, Node start, Node end) {
            Grid = grid;
            StartNode = start;
            EndNode = end;
        }

        public abstract List<Node> FindPath();

        protected List<Node> RetracePath()
        {
            LinkedList<Node> path = new LinkedList<Node>();

            while (CurrentNode.parent != null)
            {
                path.AddFirst(CurrentNode);
                CurrentNode = CurrentNode.parent;
            }

            return path.ToList();
        }
    } 
}
