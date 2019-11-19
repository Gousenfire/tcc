using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveStatiscsGenerator : MonoBehaviour
{
    public int generateAmount = 1000;
    private PCGAlgorithm algorithm;
    private float percent;

    private void Start()
    {
        algorithm = GetComponent<PCGAlgorithm>();
        StartCoroutine(GenerateCaves());
    }

    private IEnumerator GenerateCaves()
    {
        string[] lines = new string[generateAmount + 1];
        lines[0] = generateAmount + "";
        for (int i = 0; i < generateAmount; i++)
        {
            algorithm.ClearCave();
            // SPEED PARAMTER
            float startTime = Time.realtimeSinceStartup;
            CaveCell[][] cave = algorithm.GenerateCave();
            int caveWidth = cave.Length;
            int caveHeight = cave[0].Length;
            float seconds = Time.realtimeSinceStartup - startTime;
            string line = seconds + "|";
            percent = (i + (1f / 3f)) / (float)generateAmount;
            yield return new WaitForEndOfFrame();

            // DIVERSITY PARAMTER
            int groundFloors = 0;
            int wallFloors = 0;
            int corridorFloors = 0;
            int cornerFloors = 0;
            int deadEndFloor = 0;
            for (int centerX = 0; centerX < caveWidth; centerX++)
            {
                for (int centerY = 0; centerY < caveHeight; centerY++)
                {
                    if (cave[centerX][centerY].type == CaveCellType.Ground)
                    {
                        int wallsCount = 0;
                        // Top Neighbour
                        bool topFloor = false;
                        if (IsOnGrid(cave, centerX, centerY + 1))
                        {
                            if (cave[centerX][centerY + 1].type == CaveCellType.Wall)
                            {
                                wallsCount++; topFloor = true;
                            }
                        }
                        else
                        {
                            wallsCount++; topFloor = true;
                        }

                        // Right Neighbour
                        bool rightFloor = false;
                        if (IsOnGrid(cave, centerX + 1, centerY))
                        {
                            if (cave[centerX + 1][centerY].type == CaveCellType.Wall)
                            {
                                wallsCount++; rightFloor = true;
                            }
                        }
                        else
                        {
                            wallsCount++; rightFloor = true;
                        }

                        // Bottom Neighbour 
                        bool bottomFloor = false;
                        if (IsOnGrid(cave, centerX, centerY - 1))
                        {
                            if (cave[centerX][centerY - 1].type == CaveCellType.Wall)
                            {
                                wallsCount++; bottomFloor = true;
                            }
                        }
                        else
                        {
                            wallsCount++; bottomFloor = true;
                        }

                        // Left Neighbour 
                        bool leftFloor = false;
                        if (IsOnGrid(cave, centerX - 1, centerY))
                        {
                            if (cave[centerX - 1][centerY].type == CaveCellType.Wall)
                            {
                                wallsCount++; leftFloor = true;
                            }
                        }
                        else
                        {
                            wallsCount++; leftFloor = true;
                        }

                        if (wallsCount == 0)
                        {
                            groundFloors++;
                        }
                        else if (wallsCount == 1)
                        {
                            wallFloors++;
                        }
                        else if (wallsCount == 2)
                        {
                            if (topFloor && bottomFloor && !rightFloor && !leftFloor ||
                                !topFloor && !bottomFloor && rightFloor && leftFloor)
                            {
                                corridorFloors++;
                            }
                            else
                            {
                                cornerFloors++;
                            }
                        }
                        else if (wallsCount == 3)
                        {
                            deadEndFloor++;
                        }
                    }
                }
            }
            line += groundFloors + "|" + wallFloors + "|" + corridorFloors + "|" + cornerFloors + "|" + deadEndFloor + "|";
            percent = (i + (2f / 3f)) / (float)generateAmount;
            yield return new WaitForEndOfFrame();

            // COMPLEXITY PARAMTER
            List<CaveCell> groundCells = new List<CaveCell>();
            for (int x = 0; x < caveWidth; x++)
            {
                for (int y = 0; y < caveHeight; y++)
                {
                    if (cave[x][y].type == CaveCellType.Ground)
                    {
                        groundCells.Add(cave[x][y]);
                    }
                }
            }

            float sum = 0;
            for (int j = 0; j < 100; j++)
            {
                CaveCell randomStart = groundCells[UnityEngine.Random.Range(0, groundCells.Count)];
                CaveCell randomEnd = groundCells[UnityEngine.Random.Range(0, groundCells.Count)];
                if (randomStart == randomEnd)
                {
                    j--;
                    continue;
                }
                float pathValue = AStar.CalculatePath(cave, caveWidth, caveHeight, randomStart.x, randomStart.y, randomEnd.x, randomEnd.y) / 10f;
                if (pathValue != -1)
                {
                    sum += pathValue;
                } else
                {
                    j--;
                    continue;
                }
            }
            float media = sum / 100f;
            line += media + "";

            lines[i + 1] = line;
            percent = (float)(i+1) / (float)generateAmount;
            yield return new WaitForEndOfFrame();
        }
        System.IO.File.WriteAllLines(@"C:\Users\GousenPride\Desktop\" + algorithm.GetType().ToString() + ".txt", lines);
    }

    protected bool IsOnGrid(CaveCell[][] cave, int x, int y)
    {
        return x >= 0 && x < cave.Length && y >= 0 && y < cave[x].Length;
    }

    public float GetPercent() { return percent; }
    public string GetAlgorithmName() { return algorithm.GetType().ToString(); }
}
