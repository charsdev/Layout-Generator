﻿using Chars.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chars.Pathfinding
{
    public class Dijkstra : PathFinding
    {
        public Dijkstra(Grid grid, Node start, Node end) : base(grid, start, end)
        {
        }

        public override List<Node> FindPath()
        {
            StartNode.cost = 0;
            Open.Clear();
            Close.Clear();

            foreach (var item in Grid.Nodes)
            {
                item.cost = UnityEngine.Random.Range(0, 10);
            }

            Open.Add(StartNode);

            while (Open.Count > 0)
            {
                CurrentNode = Open.First();
                CurrentNode = GetBestNode(CurrentNode);

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
                        && Grid.Nodes[adj.position.x, adj.position.y].type != (int)Tiles.OBSTACLE)
                    {
                        float tentativeCost = CurrentNode.cost + adj.cost;

                        if (adj.cost < CurrentNode.cost)
                        {
                            adj.parent = CurrentNode;
                            adj.cost = tentativeCost;
                            Open.Add(adj);
                        }
                    }
                }
            }

            return new List<Node>();
        }

        private Node GetBestNode(Node currentNode)
        {
            return Open.FirstOrDefault(node => node.cost <= currentNode.cost);
        }

        private float GetManhattanDistance(Node source, Node target)
        {
            return Mathf.Abs(source.position.x - target.position.x) + Mathf.Abs(source.position.y - target.position.y);
        }
    }
}

