using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    public class Node
    {
        public bool walkable;
        public Vector2Int pos;
        public int gCost; // Cost from the start to this node
        public int hCost; // Heuristic cost (distance to target)
        public Node parent;

        public int fCost => gCost + hCost; // Total cost

        public Node(bool walkable, Vector2Int pos)
        {
            this.walkable = walkable;
            this.pos = pos;
            gCost = int.MaxValue;
        }
    }

    private static Node[,] grid;

    public static Vector2Int GetDirection(Vector2Int startPos, Vector2Int targetPos)
    {
        grid = GetGrid(startPos, targetPos);
        var startNode = grid[startPos.x, startPos.y];
        var targetNode = grid[targetPos.x, targetPos.y];
        
        startNode.gCost = 0;

        if (AStarSearch(startNode, targetNode))
        {
            Node firstStep = RetracePath(startNode, targetNode);
            return firstStep.pos - startNode.pos;
        }

        return Vector2Int.zero;
    }
    
    public static int GetDistance(Vector2Int startPos, Vector2Int targetPos)
    {
        grid = GetGrid(startPos, targetPos);
        var startNode = grid[startPos.x, startPos.y];
        var targetNode = grid[targetPos.x, targetPos.y];
        
        startNode.gCost = 0;

        if (AStarSearch(startNode, targetNode))
            return GetPathDistance(startNode, targetNode);

        return int.MaxValue;
    }
    
    private static bool AStarSearch(Node startNode, Node targetNode)
    {
        List<Node> openSet = new();
        HashSet<Node> closedSet = new();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node curNode = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < curNode.fCost ||
                    (openSet[i].fCost == curNode.fCost && openSet[i].hCost < curNode.hCost))
                {
                    curNode = openSet[i];
                }
            }

            openSet.Remove(curNode);
            closedSet.Add(curNode);

            if (curNode == targetNode)
                return true;

            foreach (var neighbour in GetNeighbours(curNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = curNode.gCost + 1;
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = Mathf.Abs(neighbour.pos.x - targetNode.pos.x) +
                                      Mathf.Abs(neighbour.pos.y - targetNode.pos.y);
                    neighbour.parent = curNode;
                    
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return false;
    }

    private static Node[,] GetGrid(Vector2Int startPos, Vector2Int targetPos)
    {
        grid = new Node[GridManager.Instance.GridSize.x, GridManager.Instance.GridSize.y];

        List<Vector2Int> notWalkablePositions = new List<Vector2Int>();
        
        notWalkablePositions.AddRange(Player.Instance.GetSegments());
        foreach (var enemy in EnemyController.Instance.Enemies)
        {
            notWalkablePositions.AddRange(enemy.GetSegments());
        }
        
        for (int x = 0; x < GridManager.Instance.GridSize.x; x++)
        {
            for (int y = 0; y < GridManager.Instance.GridSize.y; y++)
            {
                var pos = new Vector2Int(x, y);
                bool walkable = notWalkablePositions.All(p => p != pos) || (pos == targetPos || pos == startPos);

                grid[x, y] = new Node(walkable, pos);
            }
        }

        return grid;
    }

    private static List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new();
        Vector2Int[] directions = { new(1, 0), new(0, 1), new(0, -1), new(-1, 0) };
        
        // Shuffle the directions array using Fisher-Yates algorithm.
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (directions[i], directions[randomIndex]) = (directions[randomIndex], directions[i]);
        }

        for (int i = 0; i < directions.Length; i++)
        {
            if (GridManager.Instance.CheckBorders(node.pos + directions[i]))
            {
                neighbours.Add(grid[node.pos.x + directions[i].x, node.pos.y + directions[i].y]);
            }
        }

        return neighbours;
    }

    private static Node RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new();
        Node curNode = endNode;
        while (curNode != startNode)
        {
            path.Add(curNode);
            curNode = curNode.parent;
        }

        path.Reverse();
        
        return path[0];
    }
    
    private static int GetPathDistance(Node startNode, Node endNode)
    {
        Node curNode = endNode;
        int distance = 0;
        while (curNode != startNode)
        {
            curNode = curNode.parent;
            distance++;
        }
        
        return distance;
    }
}
