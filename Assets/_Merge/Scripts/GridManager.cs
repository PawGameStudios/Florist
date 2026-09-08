namespace Florist.Merge
{
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const int c_GridSizeX = 9;
    private const int c_GridSizeY = 5;
    [SerializeField] private Transform m_GridParent;
    [SerializeField] private Cell m_CellPrefab;
    private Cell[,] m_Cells = new Cell[c_GridSizeX, c_GridSizeY];

    public void Initialize(GameManager owner)
    {
        if (m_Cells[0, 0] != null) return;
        CreateGrid(owner);
    }

    private void CreateGrid(GameManager owner)
    {
        for (int y = 0; y < c_GridSizeY; y++)
        for (int x = 0; x < c_GridSizeX; x++)
        {
            Cell cell = Instantiate(m_CellPrefab, m_GridParent);
            cell.Initialize(x, y, owner);
            m_Cells[x, y] = cell;
        }
    }

    public IEnumerable<Cell> GetAllCells()
    {
        foreach (var cell in m_Cells)
            yield return cell;
    }

    public void ResetGrid()
    {
        foreach (var cell in m_Cells)
            cell.Clear();
    }

    public List<Cell> GetClosestEmptyCells(int x, int y)
    {
        var visited = new bool[c_GridSizeX, c_GridSizeY];
        var queue = new Queue<(int x, int y, int dist)>();
        var result = new List<Cell>();
        int minDist = int.MaxValue;

        queue.Enqueue((x, y, 0));
        visited[x, y] = true;

        while (queue.Count > 0)
        {
            var (cx, cy, dist) = queue.Dequeue();
            if (dist > minDist) break;

            if (!(cx == x && cy == y) && m_Cells[cx, cy].IsEmpty)
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    result.Clear();
                }
                result.Add(m_Cells[cx, cy]);
                continue;
            }

            int[,] directions = new int[,] {
                {0,1}, {1,0}, {0,-1}, {-1,0},
                {1,1}, {1,-1}, {-1,1}, {-1,-1}
            };
            for (int i = 0; i < 8; i++)
            {
                int nx = cx + directions[i,0];
                int ny = cy + directions[i,1];
                if (nx >= 0 && nx < c_GridSizeX && ny >= 0 && ny < c_GridSizeY && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny, dist + 1));
                }
            }
        }
        return result;
    }
}
}
