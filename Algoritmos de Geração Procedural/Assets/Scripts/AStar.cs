using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public static int CalculatePath(CaveCell[][] cave, int caveWidth, int caveHeight, int startX, int startY, int desiredX, int desiredY)
    {
        Node[][] grid = new Node[caveWidth][];
        for (int x = 0; x < caveWidth; x++)
        {
            grid[x] = new Node[caveHeight];
            for (int y = 0; y < caveHeight; y++)
            {
                grid[x][y] = new Node(x, y);
            }
        }
        List<Node> openNodes = new List<Node>();
        List<Node> closeNodes = new List<Node>();

        Node startNode = grid[startX][startY];
        Node desireNode = grid[desiredX][desiredY];

        openNodes.Add(startNode);

        while (openNodes.Count > 0)
        {
            Node currentNode = GetLowerFCostNode(openNodes);
            openNodes.Remove(currentNode);
            closeNodes.Add(currentNode);

            if (currentNode == desireNode)
            {
                return currentNode.fCost;
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        int currentX = currentNode.x + x;
                        int currentY = currentNode.y + y;
                        if (currentX >= 0 && currentX < caveWidth && currentY >= 0 && currentY < caveHeight)
                        {
                            Node neighbourNode = grid[currentX][currentY];
                            if (cave[currentX][currentY].type == CaveCellType.Wall || closeNodes.Contains(neighbourNode))
                            {
                                continue;
                            }

                            int newPathGCost = CalculateCost(startNode, neighbourNode);
                            int newPathHCost = CalculateCost(desireNode, neighbourNode); ;
                            int newPathCost = newPathGCost + newPathHCost;
                            bool openNodeContains = openNodes.Contains(neighbourNode);
                            if (neighbourNode.fCost < newPathCost || !openNodeContains)
                            {
                                neighbourNode.gCost = newPathGCost;
                                neighbourNode.hCost = newPathHCost;
                                neighbourNode.parentNode = currentNode;
                                if (!openNodeContains)
                                {
                                    openNodes.Add(neighbourNode);
                                }
                            }
                        }
                    }
                }
            }
        }

        return -1;
    }

    private static int CalculateCost(Node nodeA, Node nodeB)
    {
        int x = Mathf.Abs(nodeA.x - nodeB.x);
        int y = Mathf.Abs(nodeA.y - nodeB.y);
        int h = (int)(Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)) * 10);
        return h;
    }

    private static Node GetLowerFCostNode(List<Node> nodes)
    {
        Node currentNode = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (currentNode.fCost > nodes[i].fCost)
            {
                currentNode = nodes[i];
            }
            else if (currentNode.fCost == nodes[i].fCost && currentNode.hCost > nodes[i].hCost)
            {
                currentNode = nodes[i];
            }
        }
        return currentNode;
    }

    private class Node
    {
        public int x;
        public int y;
        public int gCost; // Distance from starting node
        public int hCost; // Distance from end node
        public Node parentNode;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.gCost = 0;
            this.hCost = 0;
            this.parentNode = null;
        }

        public int fCost { get { return gCost + hCost; } }
    }

}
