using UnityEngine;
using System.Collections.Generic;
using System;

public class CellularAutomata : PCGAlgorithm
{
    [Header("Cellular Automata")]
    [Range(0, 100)]
    [SerializeField] private int fillPercent = 80;
    [SerializeField] private int smoothTime = 5;
    [SerializeField] private int wallThreshholdSize = 50;
    [SerializeField] private int groundThreshholdSize = 30;

    [Header("Create Passages Algorithm")]
    [SerializeField] private int minPassageSize = 1;
    [SerializeField] private int maxPassageSize = 1;

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
                if (UnityEngine.Random.Range(0, 100) < fillPercent && !IsBorderCell(x, y))
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
            if (wallCount > 4) grid[x][y].type = CaveCellType.Wall;
            else if (wallCount < 4) grid[x][y].type = CaveCellType.Ground;
        }));
    }

    private CaveCell[] GetRegionOfTile(int x, int y)
    {
        CaveCellType firstType = grid[x][y].type;
        List<CaveCell> region = new List<CaveCell>();
        int[,] mapFlag = new int[caveWidth, caveHeight];
        Queue<CaveCell> cellsToLook = new Queue<CaveCell>();
        cellsToLook.Enqueue(grid[x][y]);
        while (cellsToLook.Count > 0)
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
            if (mapFlag[x, y] == 0 && grid[x][y].type == type)
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
        List<Room> roomsInCave = new List<Room>();
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
            else
            {
                roomsInCave.Add(new Room(region, grid));
            }
        }

        roomsInCave.Sort();
        roomsInCave[0].SetMainRoom();
        ConnectClosestRooms(roomsInCave.ToArray());
    }

    private void ConnectClosestRooms(Room[] rooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();
        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in rooms)
            {
                if (room.IsAccessibleFromMainRoom())
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA.AddRange(rooms);
            roomListB.AddRange(rooms);
        }

        int bestDistance = 0;
        CaveCell bestCellA = null;
        CaveCell bestCellB = null;
        Room bestRoomA = null;
        Room bestRoomB = null;
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0) continue;
            }

            foreach (Room roomB in roomListB)
            {
                for (int cellIndexA = 0; cellIndexA < roomA.floorCells.Length; cellIndexA++)
                {
                    if (roomA == roomB || roomA.IsConnected(roomB))
                    {
                        continue;
                    }

                    for (int cellIndexB = 0; cellIndexB < roomB.floorCells.Length; cellIndexB++)
                    {
                        CaveCell cellA = roomA.floorCells[cellIndexA];
                        CaveCell cellB = roomB.floorCells[cellIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(cellA.x - cellB.x, 2) + Mathf.Pow(cellA.y - cellB.y, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestCellA = cellA;
                            bestCellB = cellB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestCellA, bestCellB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestCellA, bestCellB);
            ConnectClosestRooms(rooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(rooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, CaveCell bestCellA, CaveCell bestCellB)
    {
        Room.ConnectRooms(roomA, roomB);
        CaveCell[] line = GetLine(bestCellA, bestCellB);
        foreach (CaveCell cell in line)
        {
            CreateCirclePassage(cell.x, cell.y, UnityEngine.Random.Range(minPassageSize, maxPassageSize));
        }
    }

    private CaveCell[] GetLine(CaveCell from, CaveCell to)
    {
        List<CaveCell> line = new List<CaveCell>();
        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Math.Abs(dy);
            shortest = Math.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(grid[x][y]);
            if (inverted) y += step;
            else x += step;
            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted) x += gradientStep;
                else y += gradientStep;
                gradientAccumulation -= longest;
            }
        }

        return line.ToArray();
    }

    private void CreateCirclePassage(int x, int y, int radius)
    {
        for (int cX = -radius; cX < radius; cX++)
        {
            for (int cY = -radius; cY < radius; cY++)
            {
                if (cX * cX + cY * cY <= radius * radius)
                {
                    int realX = cX + x;
                    int realY = cY + y;
                    if (IsOnGrid(realX, realY))
                    {
                        grid[realX][realY].type = CaveCellType.Ground;
                    }
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
                            Gizmos.color = new Color(1, 0, 0, 0.8f);
                            break;
                    }
                    Gizmos.DrawCube(new Vector3(x, 0, y), Vector3.one * 0.9f);
                }
            }
        }
    }

    private class Room : IComparable<Room>
    {
        public CaveCell[] floorCells;
        public CaveCell[] wallCells;
        public List<Room> connectedRooms;
        private int roomSize;
        private bool isAccessibleFromMainRoom = false;
        private bool isMainRoom;

        public Room(CaveCell[] floorCells, CaveCell[][] grid)
        {
            this.floorCells = floorCells;
            List<CaveCell> wallCells = new List<CaveCell>();
            foreach (CaveCell cell in floorCells)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 0 || y != 0)
                        {
                            int currentX = cell.x + x;
                            int currentY = cell.y + y;
                            if (grid[currentX][currentY].type == CaveCellType.Wall && !wallCells.Contains(grid[currentX][currentY]))
                            {
                                wallCells.Add(grid[currentX][currentY]);
                            }
                        }
                    }
                }
            }
            this.wallCells = wallCells.ToArray();
            this.connectedRooms = new List<Room>();
            this.roomSize = floorCells.Length;
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
            if (roomA.isAccessibleFromMainRoom && !roomB.isAccessibleFromMainRoom)
            {
                roomB.isAccessibleFromMainRoom = true;
                foreach (Room room in roomB.connectedRooms)
                {
                    room.isAccessibleFromMainRoom = true;
                }
            }
            else if (roomB.isAccessibleFromMainRoom && !roomA.isAccessibleFromMainRoom)
            {
                roomA.isAccessibleFromMainRoom = true;
                foreach (Room room in roomA.connectedRooms)
                {
                    room.isAccessibleFromMainRoom = true;
                }
            }
        }

        public void SetMainRoom()
        {
            isMainRoom = true;
            isAccessibleFromMainRoom = true;
        }

        public bool IsMainRoom() { return isMainRoom; }
        public bool IsAccessibleFromMainRoom() { return isAccessibleFromMainRoom; }
        public bool IsConnected(Room other) { return connectedRooms.Contains(other); }
        public int CompareTo(Room other) { return other.roomSize.CompareTo(roomSize); }
    }
}
