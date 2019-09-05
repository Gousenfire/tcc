using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PCGAlgorithm : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] protected int caveWidth;
    [SerializeField] protected int caveHeight;

    private void Start()
    {
        float time = Time.realtimeSinceStartup;
        GenerateCave();
        Debug.Log("Cave created in: " + (Time.realtimeSinceStartup - time) + " seconds");
    }

    public abstract CaveCell[][] GenerateCave();
    public abstract void ClearCave();

    #region HelperMethods

    protected bool IsOnGrid(int x, int y)
    {
        return x >= 0 && x < caveWidth && y >= 0 && y < caveHeight;
    }


    protected bool IsBorderCell(int x, int y)
    {
        return (x == 0 || x == caveWidth - 1) || (y == 0 || y == caveHeight - 1);
    }

    protected delegate void CaveCellsIterator(int x, int y);

    protected void IterateThroughCave(CaveCellsIterator iterator)
    {
        for (int x = 0; x < caveWidth; x++)
        {
            for (int y = 0; y < caveHeight; y++)
            {
                iterator(x, y);
            }
        }
    }

    protected void IterateThroughNeighbours(CaveCellsIterator iterator, int centerX, int centerY)
    {
        IterateThroughNeighbours(iterator, (int x, int y) => { }, centerX, centerY);
    }

    protected void IterateThroughNeighbours(CaveCellsIterator iterator, CaveCellsIterator notOnGridIterator, int centerX, int centerY)
    {
        // Top Neighbour 
        if (IsOnGrid(centerX, centerY + 1)) iterator(centerX, centerY + 1);
        else notOnGridIterator(centerX, centerY + 1);

        // Right Neighbour 
        if (IsOnGrid(centerX + 1, centerY)) iterator(centerX + 1, centerY);
        else notOnGridIterator(centerX + 1, centerY);

        // Bottom Neighbour 
        if (IsOnGrid(centerX, centerY - 1)) iterator(centerX, centerY - 1);
        else notOnGridIterator(centerX, centerY - 1);

        // Left Neighbour 
        if (IsOnGrid(centerX - 1, centerY)) iterator(centerX - 1, centerY);
        else notOnGridIterator(centerX - 1, centerY);
    }

    #endregion

}

public enum CaveCellType
{
    Ground,
    Wall,
    Door,
    Trap
}

[System.Serializable]
public class CaveCell
{
    public int x;
    public int y;
    public CaveCellType type;

    public CaveCell(int x, int y) : this(x, y, CaveCellType.Ground) { }

    public CaveCell(int x, int y, CaveCellType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}
