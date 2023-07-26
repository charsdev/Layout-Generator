using UnityEngine;

namespace Chars.Pathfinding
{
    public class Node
    {
        public int index;
        public int type; // free cell
        public Vector2Int position;
        public float heuristic; //h
        public float cost; //g
        public float priority; //f 
        public bool walkable = true;
        public Node parent;
    }
}


