using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;

[System.Serializable]
public class MazeData
{
    public string name;
    public int size;
    public string fileName;
    public bool[] grid; // 线性存储 gridSize x gridSize 迷宫矩阵
    public (int, int) start;
    public (int, int) end;

    // 初始化方法
    public void Initialize(int size)
    {
        this.size = size;
        grid = new bool[size * size]; // 默认全 false（未激活）
        start = (size / 2 - 1, size / 2);
        end = (size - 1, 0);
    }

    public bool GetCell(int x, int z)
    {
        return grid[x * size + z];
    }

    public void SetCell(int x, int z, bool value)
    {
        grid[x * size + z] = value;
    }

    // 检查坐标是否在迷宫范围内
    public bool IsInBounds((int, int) pos)
    {
        return pos.Item1 >= 0 && pos.Item2 >= 0 && pos.Item1 < size && pos.Item2 < size;
    }

    // 检查坐标是否是起点或者终点
    public bool IsStratOrEnd((int, int) pos)
    {
        return pos == start || pos == end;
    }



}
