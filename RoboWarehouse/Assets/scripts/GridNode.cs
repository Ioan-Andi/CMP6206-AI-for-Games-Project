
using UnityEngine;

public class GridNode
{
    public Vector3 worldPosition;
    public bool walkable;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public int fCost => gCost + hCost;

    public GridNode parent;

    public GridNode(Vector3 position, bool isWalkable, int x, int y)
    {
        worldPosition = position;
        walkable = isWalkable;
        gridX = x;
        gridY = y;
    }
}
