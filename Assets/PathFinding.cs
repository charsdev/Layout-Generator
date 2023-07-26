using System.Collections.Generic;

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
            List<Node> path = new List<Node>();
            Node currentNode = EndNode;

            while (currentNode != StartNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }
    } 
}
