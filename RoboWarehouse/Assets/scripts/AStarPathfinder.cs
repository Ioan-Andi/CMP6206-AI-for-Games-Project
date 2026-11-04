
using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour
{
    public GridManager grid;

    public List<Vector3> FindPath(
        Vector3 startPosition,
        Vector3 targetPosition)
    {
        Node startNode = grid.GetNodeFromWorldPoint(startPosition);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPosition);

        if (!startNode.walkable || !targetNode.walkable)
            return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        foreach (Node node in grid.GetAllNodes())
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            foreach (Node node in openSet)
            {
                if (node.fCost < currentNode.fCost ||
                    (node.fCost == currentNode.fCost &&
                     node.hCost < currentNode.hCost))
                {
                    currentNode = node;
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (closedSet.Contains(neighbour))
                    continue;

                int newCost = currentNode.gCost +
                    GetDistance(currentNode, neighbour);

                if (newCost < neighbour.gCost)
                {
                    neighbour.gCost = newCost;
                    neighbour.hCost =
                        GetDistance(neighbour, targetNode);

                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        Debug.LogWarning("No path found!");

        return null;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);

        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Abs(dx - dy);

        return diagonal * 14 + straight * 10;
    }

    List<Vector3> RetracePath(Node start, Node end)
    {
        List<Vector3> path = new List<Vector3>();

        Node current = end;

        while (current != start)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }

        path.Reverse();

        return path;
    }
}
