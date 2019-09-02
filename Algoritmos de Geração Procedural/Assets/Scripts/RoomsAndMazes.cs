using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsAndMazes : PCGAlgorithm
{
    [SerializeField] private bool initWithSurroundingWalls = false;
    [Header("Rooms Configuration")]
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int maxRoomSize = 9;
    [SerializeField] private int maxRoomsAmmout = 10;
    [SerializeField] private int createRoomsAttempts = 200;

    private RoomsAndMazesCell[][] grid;
    private bool gridInitialized = false;

    public override CaveCell[][] GenerateCave()
    {
        bool dungeonValid = false;

        while (!dungeonValid)
        {
            InitializeCave();

            CreateRooms();

            CreateMaze();

            ConnectDungeon();

            CleanDungeon();

            RemoveDeadEnds();

            dungeonValid = ValidateDungeon();
        }

        return ConvertRoomsAndMazesDungeon2PCGDungeon();
    }

    #region createCaveRegion

    private void InitializeCave()
    {
        grid = new RoomsAndMazesCell[caveWidth][];

        for (int x = 0; x < caveWidth; x++)
        {
            grid[x] = new RoomsAndMazesCell[caveHeight];
            for (int y = 0; y < caveHeight; y++)
            {
                grid[x][y] = new RoomsAndMazesCell(x, y);
                if (initWithSurroundingWalls && IsBorderCell(x, y))
                {
                    grid[x][y].type = RoomsAndMazesCellType.Wall;
                }
            }
        }

        gridInitialized = true;
    }

    private void CreateRooms()
    {
        int currentRoomsAmount = 0;
        int currentAttempt = 0;
        while (currentAttempt < createRoomsAttempts && currentRoomsAmount < maxRoomsAmmout)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);
            int startX = Random.Range(0, caveWidth - roomWidth);
            int startY = Random.Range(0, caveHeight - roomHeight);
            bool canDungeonBeMade = true;

            for (int x = startX + 1; x < startX + roomWidth - 1; x++)
            {
                for (int y = startY + 1; y < startY + roomHeight - 1; y++)
                {
                    if (grid[x][y].type != RoomsAndMazesCellType.Null)
                    {
                        canDungeonBeMade = false;
                        x = caveWidth;
                        y = caveHeight;
                    }
                }
            }

            if (canDungeonBeMade)
            {
                for (int x = startX; x < startX + roomWidth; x++)
                {
                    for (int y = startY; y < startY + roomHeight; y++)
                    {
                        if (x == startX || x == startX + roomWidth - 1 || y == startY || y == startY + roomHeight - 1)
                        {
                            grid[x][y].type = RoomsAndMazesCellType.Wall;
                        }
                        else
                        {
                            grid[x][y].type = RoomsAndMazesCellType.SemiGround;
                        }
                    }
                }

                currentRoomsAmount++;
            }

            currentAttempt++;
        }
    }

    private void CreateMaze()
    {
        // check all the dungeon and get all null cells and add it to the list 'nullCells'
        List<RoomsAndMazesCell> nullCells = new List<RoomsAndMazesCell>();
        IterateThroughCave(new CaveCellsIterator((int x, int y) =>
        {
            if (grid[x][y].type == RoomsAndMazesCellType.Null)
            {
                nullCells.Add(grid[x][y]);
            }
        }));

        // while the is an null cell inside 'nullCells'
        while (nullCells.Count > 0)
        {
            // create a list representing all cells that have a possibility to enter the maze
            List<RoomsAndMazesCell> mazePossibleCells = new List<RoomsAndMazesCell>();

            // add a random null cell in the maze
            RoomsAndMazesCell currentCell = nullCells[Random.Range(0, nullCells.Count)];
            mazePossibleCells.Add(currentCell);
            nullCells.Remove(currentCell);

            while (mazePossibleCells.Count > 0)
            {
                currentCell = mazePossibleCells[Random.Range(0, mazePossibleCells.Count)];
                mazePossibleCells.Remove(currentCell);
                grid[currentCell.x][currentCell.y].type = RoomsAndMazesCellType.SemiGround;

                IterateThroughNeighbours(new CaveCellsIterator((int x, int y) =>
                {
                    if (grid[x][y].type == RoomsAndMazesCellType.Null)
                    {
                        grid[x][y].type = RoomsAndMazesCellType.SemiWall;
                        mazePossibleCells.Add(grid[x][y]);
                        nullCells.Remove(grid[x][y]);
                    }
                    else if (grid[x][y].type == RoomsAndMazesCellType.SemiWall)
                    {
                        grid[x][y].type = RoomsAndMazesCellType.Wall;
                        mazePossibleCells.Remove(grid[x][y]);
                    }
                }), currentCell.x, currentCell.y);
            }
        }
    }

    private bool IsConnection(int x, int y)
    {
        string neighboursCode = "";
        IterateThroughNeighbours(new CaveCellsIterator((int nX, int nY) =>
        {
            neighboursCode += "" + (int)grid[nX][nY].type;
        }), new CaveCellsIterator((int nX, int nY) =>
        {
            neighboursCode += '0';
        }), x, y);

        return neighboursCode == "1424" ||
               neighboursCode == "2414" ||
               neighboursCode == "4241" ||
               neighboursCode == "4142";
    }

    private void ConnectDungeon()
    {
        RoomsAndMazesCell currentCell = null;
        for (int x = 0; x < caveWidth; x++)
        {
            for (int y = 0; y < caveHeight; y++)
            {
                if (grid[x][y].type == RoomsAndMazesCellType.SemiGround)
                {
                    currentCell = grid[x][y];
                    x = caveWidth;
                    y = caveHeight;
                }
            }
        }

        RoomsAndMazesCell[] boudries = PaintDungeon(currentCell.x, currentCell.y, RoomsAndMazesCellType.Ground);
        List<RoomsAndMazesCell> connections = new List<RoomsAndMazesCell>();

        for (int i = 0; i < boudries.Length; i++)
        {
            if (IsConnection(boudries[i].x, boudries[i].y))
            {
                boudries[i].type = RoomsAndMazesCellType.Connection;
                connections.Add(boudries[i]);
            }
        }

        while (connections.Count > 0)
        {
            RoomsAndMazesCell currentConnection = connections[Random.Range(0, connections.Count)];
            connections.Remove(currentConnection);

            if (IsConnection(currentConnection.x, currentConnection.y))
            {
                RoomsAndMazesCell cellToPaint = null;
                IterateThroughNeighbours(new CaveCellsIterator((int x, int y) =>
                {
                    if (grid[x][y].type == RoomsAndMazesCellType.SemiGround)
                    {
                        cellToPaint = grid[x][y];
                    }
                }), currentConnection.x, currentConnection.y);

                if (cellToPaint != null)
                {
                    currentConnection.type = RoomsAndMazesCellType.Door;
                    boudries = PaintDungeon(cellToPaint.x, cellToPaint.y, RoomsAndMazesCellType.Ground);
                    for (int i = 0; i < boudries.Length; i++)
                    {
                        if (IsConnection(boudries[i].x, boudries[i].y))
                        {
                            boudries[i].type = RoomsAndMazesCellType.Connection;
                            connections.Add(boudries[i]);
                        }
                        else if (grid[boudries[i].x][boudries[i].y].type == RoomsAndMazesCellType.Connection)
                        {
                            boudries[i].type = RoomsAndMazesCellType.Wall;
                            connections.Remove(grid[boudries[i].x][boudries[i].y]);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Change the 'color' of the grid according to the type of the first cell (bucket)
    /// </summary>
    /// <param name="x">Start cell X position</param>
    /// <param name="y">Start cell Y position</param>
    /// <param name="color">the 'color' to paint the cave with with</param>
    /// <returns>Returns the boundries cells</returns>
    private RoomsAndMazesCell[] PaintDungeon(int x, int y, RoomsAndMazesCellType color)
    {
        RoomsAndMazesCellType firstType = grid[x][y].type;
        List<RoomsAndMazesCell> cellsToPaint = new List<RoomsAndMazesCell>();
        cellsToPaint.Add(grid[x][y]);
        List<RoomsAndMazesCell> boundries = new List<RoomsAndMazesCell>();

        while (cellsToPaint.Count > 0)
        {
            RoomsAndMazesCell currentCell = cellsToPaint[0];
            cellsToPaint.Remove(currentCell);

            if (currentCell.type == firstType)
            {
                currentCell.type = color;
                IterateThroughNeighbours(new CaveCellsIterator((int nX, int nY) =>
                {
                    if (grid[nX][nY].type == firstType)
                    {
                        cellsToPaint.Add(grid[nX][nY]);
                    }
                    else if (grid[nX][nY].type != color)
                    {
                        boundries.Add(grid[nX][nY]);
                    }
                }), currentCell.x, currentCell.y);
            }
        }

        return boundries.ToArray();
    }

    private void CleanDungeon()
    {
        IterateThroughCave(new CaveCellsIterator((int x, int y) =>
        {
            RoomsAndMazesCell cell = grid[x][y];
            if (cell.type != RoomsAndMazesCellType.Ground &&
                cell.type != RoomsAndMazesCellType.Wall &&
                cell.type != RoomsAndMazesCellType.Door)
            {
                cell.type = RoomsAndMazesCellType.Wall;
            }
        }));
    }

    private void RemoveDeadEnds()
    {
        List<RoomsAndMazesCell> deadEnds = new List<RoomsAndMazesCell>();
        IterateThroughCave(new CaveCellsIterator((int x, int y) =>
        {
            RoomsAndMazesCell cell = grid[x][y];
            if (cell.type == RoomsAndMazesCellType.Ground)
            {
                if (IsDeadEnd(cell.x, cell.y))
                {
                    deadEnds.Add(cell);
                }
            }
        }));

        while (deadEnds.Count > 0)
        {
            RoomsAndMazesCell cell = deadEnds[Random.Range(0, deadEnds.Count)];
            cell.type = RoomsAndMazesCellType.Wall;
            deadEnds.Remove(cell);

            IterateThroughNeighbours(new CaveCellsIterator((int nX, int nY) =>
            {
                if ((grid[nX][nY].type == RoomsAndMazesCellType.Ground || grid[nX][nY].type == RoomsAndMazesCellType.Door)
                    && IsDeadEnd(nX, nY))
                {
                    deadEnds.Add(grid[nX][nY]);
                }
            }), cell.x, cell.y);
        }
    }

    private bool ValidateDungeon() { return true; }

    private CaveCell[][] ConvertRoomsAndMazesDungeon2PCGDungeon()
    {
        CaveCell[][] caveGrid = new CaveCell[caveWidth][];

        for (int x = 0; x < caveWidth; x++)
        {
            caveGrid[x] = new CaveCell[caveHeight];
            for (int y = 0; y < caveHeight; y++)
            {
                caveGrid[x][y] = grid[x][y].convertCellToPCGCell();
            }
        }

        return caveGrid;
    }

    #endregion

    #region HelperMethods

    private bool IsDeadEnd(int x, int y)
    {
        int neigbourCount = 0;
        IterateThroughNeighbours(new CaveCellsIterator((int nX, int nY) =>
        {
            if (grid[nX][nY].type != RoomsAndMazesCellType.Ground && grid[nX][nY].type != RoomsAndMazesCellType.Door)
            {
                neigbourCount++;
            }
        }), new CaveCellsIterator((int nX, int nY) =>
        {
            neigbourCount++;
        }), x, y);
        return neigbourCount == 3;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (gridInitialized)
        {
            for (int y = 0; y < caveHeight; y++)
            {
                for (int x = 0; x < caveWidth; x++)
                {
                    switch (grid[x][y].type)
                    {
                        case RoomsAndMazesCellType.Null:
                            Gizmos.color = new Color(1, 0, 0, 0.8f);
                            break;
                        case RoomsAndMazesCellType.SemiGround:
                            Gizmos.color = new Color(0.63f, 0.12f, 0.67f, 0.8f);
                            break;
                        case RoomsAndMazesCellType.Ground:
                            Gizmos.color = new Color(1, 1, 1, 0.8f);
                            break;
                        case RoomsAndMazesCellType.SemiWall:
                            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                            break;
                        case RoomsAndMazesCellType.Wall:
                            Gizmos.color = new Color(0, 0, 0, 0.8f);
                            break;
                        case RoomsAndMazesCellType.Connection:
                            Gizmos.color = new Color(0, 0, 1, 0.8f);
                            break;
                        case RoomsAndMazesCellType.Door:
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

[System.Serializable]
public class RoomsAndMazesCell
{
    public int x;
    public int y;
    public RoomsAndMazesCellType type;

    public RoomsAndMazesCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.type = RoomsAndMazesCellType.Null;
    }

    public CaveCell convertCellToPCGCell()
    {
        switch (type)
        {
            case RoomsAndMazesCellType.SemiGround:
                return new CaveCell(x, y, CaveCellType.Ground);
            case RoomsAndMazesCellType.Door:
                return new CaveCell(x, y, CaveCellType.Door);
            default:
                return new CaveCell(x, y, CaveCellType.Wall);
        }
    }
}

public enum RoomsAndMazesCellType
{
    Null,
    SemiGround,
    Ground,
    SemiWall,
    Wall,
    Connection,
    Door
}