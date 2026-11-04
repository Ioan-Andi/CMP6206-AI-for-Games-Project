
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public LayerMask ObstacleMask;

    public Vector2 gridWorldSize = new Vector2(20, 20);
    public float nodeRadius = 0.5f;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    void Awake()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        nodeDiameter = nodeRadius * 2;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        grid = new Node[gridSizeX, gridSizeY];

        Vector3 bottomLeft = transform.position
            - Vector3.right * gridWorldSize.x / 2
            - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft
                    + Vector3.right * (x * nodeDiameter + nodeRadius)
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics.CheckSphere(
                    worldPoint,
                    nodeRadius * 0.9f,
                    ObstacleMask
                );

                grid[x, y] = new Node(
                    walkable,
                    worldPoint,
                    x,
                    y
                );
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX =
            (worldPosition.x - (transform.position.x - gridWorldSize.x / 2))
            / gridWorldSize.x;

        float percentY =
            (worldPosition.z - (transform.position.z - gridWorldSize.y / 2))
            / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.Clamp(
            Mathf.RoundToInt((gridSizeX - 1) * percentX),
            0, gridSizeX - 1
        );

        int y = Mathf.Clamp(
            Mathf.RoundToInt((gridSizeY - 1) * percentY),
            0, gridSizeY - 1
        );

        return grid[x, y];
    }

    public Node GetNodeAt(int x, int y)
    {
        if (x < 0 || x >= gridSizeX ||
            y < 0 || y >= gridSizeY)
            return null;

        return grid[x, y];
    }

    public IEnumerable<Node> GetAllNodes()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                yield return grid[x, y];
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX < 0 || checkX >= gridSizeX ||
                    checkY < 0 || checkY >= gridSizeY)
                    continue;

                Node neighbour = grid[checkX, checkY];

                if (!neighbour.walkable)
                    continue;

                // Prevent diagonal corner cutting.
                if (x != 0 && y != 0)
                {
                    if (!grid[node.gridX + x, node.gridY].walkable ||
                        !grid[node.gridX, node.gridY + y].walkable)
                        continue;
                }

                neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(gridWorldSize.x, 0.1f, gridWorldSize.y)
        );

        if (grid == null)
            return;

        foreach (Node node in grid)
        {
            Gizmos.color = node.walkable ? Color.white : Color.red;

            Gizmos.DrawCube(
                node.worldPosition,
                Vector3.one * (nodeRadius * 1.8f)
            );
        }
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector3 worldPosition, int x, int y)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        gridX = x;
        gridY = y;
    }
}