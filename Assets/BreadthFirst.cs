using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    class BreadthFirst
    {
        public List<Node> FindPath(Node startNode, Node targetNode, ref Grid grid)
        {
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();

                if (currentNode.Equals(targetNode))
                {
                    return GeneratePath(startNode, targetNode);
                }

                var neighborsPositions = GetNeighborsPositions(currentNode, ref grid);

                foreach (var neighborPosition in neighborsPositions)
                {
                    var currentNeighbor = grid.Nodes[neighborPosition.x, neighborPosition.y];

                    if (!visited.Contains(currentNeighbor) 
                        && grid.Nodes[currentNeighbor.position.x, currentNeighbor.position.y].type != (int)Tiles.OBSTACLE)
                    {
                        queue.Enqueue(currentNeighbor);
                        visited.Add(currentNeighbor);
                        currentNeighbor.parent = currentNode;
                    }
                }
            }

            return new List<Node>(); // No se encontró una ruta válida
        }

        public List<Node> GeneratePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (!currentNode.position.Equals(startNode.position))
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }

        public List<Vector2Int> GetNeighborsPositions(Node node, ref Grid grid)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            if (node.position.x > 0)
                neighbors.Add(new Vector2Int(node.position.x - 1, node.position.y));

            if (node.position.x < grid.Width - 1)
                neighbors.Add(new Vector2Int(node.position.x + 1, node.position.y));

            if (node.position.y > 0)
                neighbors.Add(new Vector2Int(node.position.x, node.position.y - 1));

            if (node.position.y < grid.Height - 1)
                neighbors.Add(new Vector2Int(node.position.x, node.position.y + 1));

            return neighbors;
        }
    }
}
