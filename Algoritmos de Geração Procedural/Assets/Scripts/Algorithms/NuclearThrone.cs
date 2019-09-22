using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuclearThrone : PCGAlgorithm
{
    [SerializeField] int caveSize = 120;

    [Header("Floor Makers")]
    [Range(0, 1)]
    [SerializeField] float createOtherFloorMakerChance = 0.8f;
    [Range(0, 1)]
    [SerializeField] float destroyOneFloorMakerChance = 0.2f;
    [SerializeField] int maxFloorMakers = 5;

    [Header("Individual Floor Maker")]
    [SerializeField] private FloorMaker.Direction startDirection = FloorMaker.Direction.Up;
    [SerializeField] private int spawn1v1Occurance = 1;
    [SerializeField] private int spawn2v2Occurance = 0;
    [SerializeField] private int spawn3v3Occurance = 0;
    [SerializeField] private int forwardOccurance = 3;
    [SerializeField] private int turn90DegreesOccurance = 1;
    [SerializeField] private int turn180DegreesOccurance = 0;

    private CaveCell[][] grid;
    private bool caveIsBeingConstructed = false;
    private List<FloorMaker> floorMakers = new List<FloorMaker>();
    private int currentFloors;
    private int floorMakersCount;

    public override CaveCell[][] GenerateCave()
    {
        if (caveIsBeingConstructed) return null;
        caveIsBeingConstructed = true;
        InitializeCave();
        IterateFloorMakers();
        return grid;
    }

    private void InitializeCave()
    {
        floorMakersCount = currentFloors = 0;
        caveWidth = caveHeight = Mathf.CeilToInt(Mathf.Log(caveSize) / Mathf.Log(2)) * 5;
        grid = new CaveCell[caveWidth][];
        for (int x = 0; x < caveWidth; x++)
        {
            grid[x] = new CaveCell[caveHeight];
            for (int y = 0; y < caveHeight; y++)
            {
                grid[x][y] = new CaveCell(x, y, CaveCellType.Wall);
            }
        }
    }

    private void IterateFloorMakers()
    {
        floorMakers.Add(new FloorMaker(grid.Length / 2, grid[0].Length / 2, startDirection, spawn1v1Occurance, spawn2v2Occurance, spawn3v3Occurance, forwardOccurance, turn90DegreesOccurance, turn180DegreesOccurance));
        currentFloors = 0;
        floorMakersCount = 1;
        while (currentFloors < caveSize)
        {
            int floorsPainted = 0;
            for (int i = 0; i < floorMakers.Count; i++)
            {
                float spawnChance = UnityEngine.Random.Range(0f, 1f);
                float destroyChance = UnityEngine.Random.Range(0f, 1f);
                floorsPainted += floorMakers[i].Itarate(ref grid);
                if (spawnChance < createOtherFloorMakerChance * (1 - (floorMakersCount / (float)maxFloorMakers)))
                {
                    floorMakers.Add(new FloorMaker(floorMakers[i].GetX(), floorMakers[i].GetY(), floorMakers[i].GetDirection(), spawn1v1Occurance, spawn2v2Occurance, spawn3v3Occurance, forwardOccurance, turn90DegreesOccurance, turn180DegreesOccurance));
                    floorMakersCount = floorMakers.Count;
                }
                if (destroyChance < destroyOneFloorMakerChance * ((floorMakersCount - 1) / (float)maxFloorMakers))
                {
                    floorMakers.Remove(floorMakers[i]);
                    floorMakersCount = floorMakers.Count;
                }
            }
            currentFloors += floorsPainted;
        }
        caveIsBeingConstructed = false;
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

public class FloorMaker
{
    private int positionX;
    private int positionY;
    private Direction currentDirection;
    private float spawn1v1Chance;
    private float spawn2v2Chance;
    //private float spawn3v3Chance;
    private float forwardChance;
    private float turn90DegreesChance;
    //private float turn180DegreesChance;

    public FloorMaker(int positionX, int positionY, Direction startDirection, int spawn1v1Occurance, int spawn2v2Occurance, int spawn3v3Occurance, int forwardOccurance, int turn90DegreesOccurance, int turn180DegreesOccurance)
    {
        this.positionX = positionX;
        this.positionY = positionY;
        this.currentDirection = startDirection;

        int spawnMax = spawn1v1Occurance + spawn2v2Occurance + spawn3v3Occurance;
        spawn1v1Chance = (float)spawn1v1Occurance / spawnMax;
        spawn2v2Chance = (float)spawn2v2Occurance / spawnMax;
        //spawn3v3Chance = (float)spawn3v3Occurance / spawnMax;

        int movementMax = forwardOccurance + turn90DegreesOccurance + turn180DegreesOccurance;
        forwardChance = (float)forwardOccurance / movementMax;
        turn90DegreesChance = (float)turn90DegreesOccurance / movementMax;
        //turn180DegreesChance = (float)turn180DegreesOccurance / movementMax;
    }

    public int Itarate(ref CaveCell[][] grid)
    {
        float spawnSizeSort = UnityEngine.Random.Range(0f, 1f);
        int cellsPainted;
        if (spawnSizeSort < spawn1v1Chance)
        {
            // 1v1
            cellsPainted = CreateFloor(ref grid, 1);
        }
        else if (spawnSizeSort > spawn1v1Chance && spawnSizeSort < spawn1v1Chance + spawn2v2Chance)
        {
            // 2v2
            cellsPainted = CreateFloor(ref grid, 2);
        }
        else
        {
            // 3v3
            cellsPainted = CreateFloor(ref grid, 3);
        }

        ManageDirection();
        switch (currentDirection)
        {
            case Direction.Up:
                positionY++;
                break;
            case Direction.Right:
                positionX++;
                break;
            case Direction.Down:
                positionY--;
                break;
            case Direction.Left:
                positionX--;
                break;
            default:
                break;
        }
        positionX = Mathf.Clamp(positionX, 0, grid.Length - 1);
        positionY = Mathf.Clamp(positionY, 0, grid.Length - 1);

        return cellsPainted;
    }

    private int CreateFloor(ref CaveCell[][] grid, int size)
    {
        int cellsPainted = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (positionX + x >= 0 && positionX + x < grid.Length &&
                    positionY + y >= 0 && positionY + y < grid.Length)
                {
                    if (grid[positionX + x][positionY + y].type != CaveCellType.Ground) cellsPainted++;
                    grid[positionX + x][positionY + y].type = CaveCellType.Ground;
                }
            }
        }
        return cellsPainted;
    }

    private void ManageDirection()
    {
        float directionChangeSort = UnityEngine.Random.Range(0f, 1f);
        int enumLenght = Enum.GetValues(currentDirection.GetType()).Length;
        if (directionChangeSort < forwardChance) { /* Continue the same direction */ }
        else if (directionChangeSort > forwardChance && directionChangeSort < forwardChance + turn90DegreesChance)
        {
            float rightOrLeft = UnityEngine.Random.Range(0f, 1f);
            if (rightOrLeft > 0.5)
            {
                // Turn Right
                currentDirection = (Direction)((int)(currentDirection + 1) % enumLenght);
            }
            else
            {
                // Turn Left
                currentDirection = (Direction)((int)(currentDirection + enumLenght - 1) % enumLenght);
            }
        }
        else
        {
            // Turn 180º
            currentDirection = (Direction)((int)(currentDirection + enumLenght - 2) % enumLenght);
        }
    }

    public int GetX() { return positionX; }
    public int GetY() { return positionY; }
    public Direction GetDirection() { return currentDirection; }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }
}