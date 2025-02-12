using System;
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
    public long[] grid; // 用 int 的二进制位表示 grid 的每一行


    // 通过 getter 返回起始点元组
    private (int, int) start
    {
        get => (size / 2 - 1, size / 2);
    }
    // 通过 getter 返回终点元组
    private (int, int) end
    {
        get => (size - 1, 0);
    }

    // 初始化方法
    public void Initialize(int size)
    {
        this.size = size;
        grid = new long[size]; // 默认全 false（未激活）
    }

    public bool GetCell(int x, int z)
    {
        return (grid[z] & (1L << x)) != 0L; // 第 index 位是否为 1
    }

    public void SetCell(int x, int z, bool value)
    {
        if (!IsInBounds((x, z)))
            return;

        if (value)
            grid[z] |= (1L << x); // 将第 index 位设为 1
        else
            grid[z] &= ~(1L << x); // 将第 index 位设为 0
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

    // 计算最短路径
    // 1. 先检查连通性（BFS）
    // 2. 若连通，使用 A* 搜索
    public List<(int, int)> FindShortestPath()
    {
        return AStar();
    }

    private (int, int)[] directions = { (0, 1), (0, -1), (1, 0), (-1, 0) };

    // 使用 BFS 计算从 start 到 end 的最短路径，并返回路径坐标列表
    private List<(int, int)> BFS()
    {
        // 记录下级要遍历的坐标
        Queue<(int, int)> queue = new Queue<(int, int)>();  

        // 记录路径：neighbor -> 来自哪个格子
        Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();

        // 记录是否访问过
        HashSet<(int, int)> visited = new HashSet<(int, int)>();    

        queue.Enqueue(start);
        visited.Add(start);
        parent[start] = (-1, -1);  // 标记起点的父节点为空

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end)
            {
                // 🔥 反向回溯路径，从 end 回溯到 start
                return ReconstructPath(parent);
            }

            foreach (var dir in directions)
            {
                (int, int) next = (current.Item1 + dir.Item1, current.Item2 + dir.Item2);
                if (IsInBounds(next) && !GetCell(next.Item1, next.Item2) && !visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                    parent[next] = current;  // 记录父节点
                }
            }
        }

        // 如果终点没有被访问过，说明无解
        return new List<(int, int)>();
        
    }

    //自定义的比较器
    public class TupleSecondItemComparer : IComparer<((int, int), int)>
    {
        public int Compare(((int, int), int) x, ((int, int), int) y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }

    private int Abs(int a)
    {
        return a >= 0 ? a : -a;
    }

    // 计算曼哈顿距离（A* 启发式）
    private int ManhattanDistance((int, int) a, (int, int) b)
    {
        return Abs(a.Item1 - b.Item1) + Abs(a.Item2 - b.Item2);
    }

    // A* 搜索最短路径
    private List<(int, int)> AStar()
    {
        // 优先队列 (坐标, 预估代价 fScore)，按照 fScore 从小到大排序
        PriorityQueue<((int, int), int)> openSet = new PriorityQueue<((int, int), int)>(new TupleSecondItemComparer(), size*size/2);
        HashSet<(int, int)> done = new HashSet<(int, int)>(); // 用于检查某点是否出队过

        // 记录路径：neighbor -> 来自哪个格子
        Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();

        // G 值：从起点到当前点的实际代价
        Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();

        // F 值：估计的总代价 = gScore + hScore
        Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();

        // 初始化起点
        openSet.Enqueue((start, 0));
        gScore[start] = 0;
        fScore[start] = ManhattanDistance(start, end);

        while (openSet.Size > 0)
        {
            // 取出 fScore 最小的点
            var (current, _) = openSet.Dequeue();

            // 排除已经找到最短路的点
            if (done.Contains(current))
            {
                continue;
            }

            done.Add(current);

            // 如果到达终点，返回最短路径
            if (current == end)
                return ReconstructPath(parent);

            foreach (var dir in directions)
            {
                (int, int) neighbor = (current.Item1 + dir.Item1, current.Item2 + dir.Item2);

                // 检查是否越界或是墙壁
                if (!IsInBounds(neighbor) || !GetCell(neighbor.Item1, neighbor.Item2))
                    continue;

                int tentativeGScore = gScore[current] + 1; // 代价 +1

                // 若 neighbor 之前未访问过，或者新路径更短
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    parent[neighbor] = current; // 记录路径
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + ManhattanDistance(neighbor, end);

                    // 因为选取的启发式函数满足三角形不等式，所以即使多次入队，第一次出队的一定是最好的情况
                    openSet.Enqueue((neighbor, fScore[neighbor]));
                }
            }
        }

        return new List<(int, int)>(); // 无解，返回空路径
    }

    // 回溯路径
    private List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom)
    {
        List<(int, int)> path = new List<(int, int)>();
        (int, int) current = end;

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();     // 反向顺序变成从 start 到 end
        return path;
    }

}
