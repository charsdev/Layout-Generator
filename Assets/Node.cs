using UnityEngine;

namespace Chars.Pathfinding
{
    public class Node
    {
        public byte Type; // free cell
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public float Heuristic; //h
        public float Cost; //g
        public float Priority; //f 
        public bool Walkable = true;
        public Node Parent;
    }
}


