using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PCGAlgorithm : MonoBehaviour
{
    [Header("Random")]
    [Tooltip("Leave blank for the script to generate a random seed")]
    [SerializeField] protected string seed;

    [Header("Grid")]
    [SerializeField] protected int caveWidth = 20;
    [SerializeField] protected int caveHeight = 20;

    private void Start()
    {
        float time = Time.realtimeSinceStartup;
        GenerateCave();
        Debug.Log("Cave created in: " + (Time.realtimeSinceStartup - time) + " seconds");
    }

    public abstract void GenerateCave();
    public abstract void ClearCave();
    protected virtual void InitCave()
    {
        if (seed == "")
        {
            seed = UnityEngine.Random.value + "|" + UnityEngine.Random.value;
        }
        UnityEngine.Random.InitState(seed.GetHashCode());
    }

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

    /// <summary>
    /// Itarete through all (top, bottom, left and right) the neighbours of a cell giving its coords
    /// </summary>
    /// <param name="iterator">The function that will be call when the neightbour exist</param>
    /// <param name="centerX">X coordinates of the main cell</param>
    /// <param name="centerY">Y coordinates of the main cell</param>
    protected void IterateThroughNeumannNeighbours(CaveCellsIterator iterator, int centerX, int centerY)
    {
        IterateThroughNeumannNeighbours(iterator, (int x, int y) => { }, centerX, centerY);
    }

    /// <summary>
    /// Itarete through all (top, bottom, left and right) the neighbours of a cell giving its coords
    /// </summary>
    /// <param name="iterator">The function that will be called when the neightbour exist</param>
    /// <param name="notOnGridIterator">The function that will be called when the neightbour doesn't exists</param>
    /// <param name="centerX">X coordinates of the main cell</param>
    /// <param name="centerY">Y coordinates of the main cell</param>
    protected void IterateThroughNeumannNeighbours(CaveCellsIterator iterator, CaveCellsIterator notOnGridIterator, int centerX, int centerY)
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

    protected void IterateThroughMooreNeighbours(CaveCellsIterator iterator, int centerX, int centerY)
    {
        IterateThroughMooreNeighbours(iterator, (int x, int y) => { }, centerX, centerY);
    }

    protected void IterateThroughMooreNeighbours(CaveCellsIterator iterator, CaveCellsIterator notOnGridIterator, int centerX, int centerY)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 || y != 0)
                {
                    if (IsOnGrid(centerX + x, centerY + y)) iterator(centerX + x, centerY + y);
                    else notOnGridIterator(centerX + x, centerY + y);
                }
            }
        }
    }

    #endregion

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

public enum CaveCellType
{
    Ground,
    Wall,
    Door
}
