using UnityEngine;
using System.Collections.Generic;

public class CellularAutomata : PCGAlgorithm
{
    [Header("Cellular Automata")]
    [Range(0, 100)]
    [SerializeField] private int fillPercent = 80;
    [SerializeField] private int smoothTime = 5;
    [SerializeField] private int wallThreshholdSize = 50;
    [SerializeField] private int groundThreshholdSize = 30;

    private CaveCell[][] grid;

    public override void GenerateCave()
    {
        InitCave();
        for (int i = 0; i < smoothTime; i++)
        {
            SmoothCave();
        }
        ProcessMap();
    }

    protected override void InitCave()
    {
        base.InitCave();
        grid = new CaveCell[caveWidth][];
        for (int x = 0; x < caveWidth; x++)
        {
            grid[x] = new CaveCell[caveHeight];
            for (int y = 0; y < caveHeight; y++)
            {
                if (Random.Range(0, 100) < fillPercent && !IsBorderCell(x, y))
                    grid[x][y] = new CaveCell(x, y, CaveCellType.Ground);
                else
                    grid[x][y] = new CaveCell(x, y, CaveCellType.Wall);
            }
        }
    }

    private void SmoothCave()
    {
        IterateThroughCave(new CaveCellsIterator((int x, int y) =>
        {
            // Counting neighbours
            int wallCount = 0;
            IterateThroughMooreNeighbours(new CaveCellsIterator((int nX, int nY) =>
            {
                if (grid[nX][nY].type == CaveCellType.Wall)
                {
                    wallCount++;
                }
            }), new CaveCellsIterator((int nX, int nY) =>
            {
                wallCount++;
            }), x, y);

            // Cellular Automata Rules
            if (wallCount > 2) grid[x][y].type = CaveCellType.Wall;
            else if (wallCount < 2) grid[x][y].type = CaveCellType.Ground;
        }));
    }

    private CaveCell[] GetRegionOfTile(int x, int y)
    {
        CaveCellType firstType = grid[x][y].type;
        List<CaveCell> region = new List<CaveCell>();
        int[,] mapFlag = new int[caveWidth, caveHeight];
        Queue<CaveCell> cellsToLook = new Queue<CaveCell>();
        cellsToLook.Enqueue(grid[x][y]);
        while(cellsToLook.Count > 0)
        {
            CaveCell currentCell = cellsToLook.Dequeue();
            region.Add(currentCell);
            IterateThroughNeumannNeighbours(new CaveCellsIterator((int nX, int nY) =>
            {
                if (mapFlag[nX, nY] == 0 && grid[nX][nY].type == firstType)
                {
                    mapFlag[nX, nY] = 1;
                    cellsToLook.Enqueue(grid[nX][nY]);
                }
            }), currentCell.x, currentCell.y);
        }
        return region.ToArray();
    }

    private List<CaveCell[]> GetRegions(CaveCellType type)
    {
        int[,] mapFlag = new int[caveWidth, caveHeight];
        List<CaveCell[]> regions = new List<CaveCell[]>();
        IterateThroughCave(new CaveCellsIterator((x, y) =>
        {
            if (mapFlag[x,y] == 0 && grid[x][y].type == type)
            {
                CaveCell[] currentRegion = GetRegionOfTile(x, y);
                regions.Add(currentRegion);
                foreach (CaveCell cell in currentRegion)
                {
                    mapFlag[cell.x, cell.y] = 1;
                }
            }
        }));
        return regions;
    }

    private void ProcessMap()
    {
        List<CaveCell[]> wallRegions = GetRegions(CaveCellType.Wall);
        foreach (CaveCell[] region in wallRegions)
        {
            if (region.Length < wallThreshholdSize)
            {
                for (int i = 0; i < region.Length; i++)
                {
                    CaveCell currentCell = region[i];
                    grid[currentCell.x][currentCell.y].type = CaveCellType.Ground;
                }
            }
        }

        List<CaveCell[]> groundRegions = GetRegions(CaveCellType.Ground);
        foreach (CaveCell[] region in groundRegions)
        {
            if (region.Length < groundThreshholdSize)
            {
                for (int i = 0; i < region.Length; i++)
                {
                    CaveCell currentCell = region[i];
                    grid[currentCell.x][currentCell.y].type = CaveCellType.Wall;
                }
            }
        }
    }

    public override void ClearCave()
    {
        grid = null;
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            for (int y = 0; y < caveHeight; y++)
            {
                for (int x = 0; x < caveWidth; x++)
                {
                    switch (grid[x][y].type)
                    {
                        case CaveCellType.Ground:
                            Gizmos.color = new Color(1, 1, 1, 0.8f);
                            break;
                        case CaveCellType.Wall:
                            Gizmos.color = new Color(0, 0, 0, 0.8f);
                            break;
                        case CaveCellType.Door:
                            Gizmos.color = new Color(0, 1, 0, 0.8f);
                            break;
                        default:
                            Gizmos.color = new Color(1, 1, 1, 0.8f);
                            break;
                    }
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one * 0.9f);
                }
            }
        }
    }

}
