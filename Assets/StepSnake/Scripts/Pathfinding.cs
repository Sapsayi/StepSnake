using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    public class Node
    {
        public bool walkable;
        public Vector2Int pos;
        public int gCost; // Стоимость пути от начальной точки до этой клетки
        public int hCost; // Эвристическая оценка (расстояние до цели)
        public Node parent;

        public int fCost => gCost + hCost; // сумарная оценка

        public Node(bool walkable, Vector2Int pos)
        {
            this.walkable = walkable;
            this.pos = pos;
        }
    }

    private static Node[,] grid;

    public static Vector2Int GetDirection(Vector2Int startPos, Vector2Int targetPos)
    {
        grid = GetGrid(startPos, targetPos);
        var startNode = grid[startPos.x, startPos.y];
        var targetNode = grid[targetPos.x, targetPos.y];

        List<Node> openSet = new();
        HashSet<Node> closedSet = new();

        startNode.gCost = 0;
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
            {
                return RetracePath(startNode, targetNode).pos - startNode.pos;
            }

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

        return Vector2Int.zero;
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
        Vector2Int[] direction = new Vector2Int[] { new(1, 0), new(0, 1), new(0, -1), new(-1, 0) };

        for (int i = 0; i < direction.Length; i++)
        {
            if (GridManager.Instance.CheckBorders(node.pos + direction[i]))
            {
                neighbours.Add(grid[node.pos.x + direction[i].x, node.pos.y + direction[i].y]);
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

        foreach (var node in path)
        {
            Debug.Log(node.pos);
        }
        
        return path[0];
    }
}
