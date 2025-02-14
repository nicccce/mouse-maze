using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;  // 引用 LineRenderer 组件，用来绘制路径
    public MapManager mapManager;      // 引用 MapManager，用来获取迷宫数据

    private Vector3[] points;  // 存储折线路径的点
    private List<List<(int, int)>> allPaths;  // 存储所有路径的列表
    private int allPathIndex;  // 当前显示路径的索引

    // 初始化时设置 LineRenderer 的基本参数
    void Start()
    {
        lineRenderer.positionCount = 0;  // 初始化时路径数量为0
        lineRenderer.startWidth = 0.2f;  // 设置线条起始宽度
        lineRenderer.endWidth = 0.2f;    // 设置线条结束宽度
    }

    // 绘制最短路径
    public void DrawShortestPath()
    {
        // 获取当前迷宫的最短路径，并绘制
        DrawPolyline(mapManager.GetCurrentMazeData().shortestPath);
    }

    // 绘制所有路径中的第一条
    public string DrawAllPath()
    {
        allPathIndex = 0;  // 重置路径索引为0
        allPaths = mapManager.GetCurrentMazeData().GetAllPaths(100, 200);  // 获取所有路径
        DrawPolyline(allPaths[0]);  // 绘制第一条路径
        return $"{allPathIndex + 1}/{allPaths.Count}";  // 返回当前路径索引与总路径数量
    }

    // 绘制下一条路径
    public string NextAllPath()
    {
        // 更新路径索引，循环显示路径
        allPathIndex = (allPathIndex + 1) % allPaths.Count;
        DrawPolyline(allPaths[allPathIndex]);  // 绘制当前索引对应的路径
        return $"{allPathIndex + 1}/{allPaths.Count}";  // 返回当前路径索引与总路径数量
    }

    // 重置路径，清除当前显示的路径
    public void ResetPath()
    {
        lineRenderer.positionCount = 0;  // 将路径数量设置为0，清除路径显示
    }

    // 根据传入的路径数据绘制折线路径
    void DrawPolyline(List<(int, int)> path)
    {
        // 将路径的点数转换为 Vector3 数组，供 LineRenderer 使用
        points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            // 将迷宫坐标转换为世界坐标
            points[i] = mapManager.GridToWorldPosition(path[i].Item1, path[i].Item2, mapManager.GetCurrentMazeData().size);
        }
        // 设置 LineRenderer 的路径点数量
        lineRenderer.positionCount = points.Length;
        // 将路径点赋值给 LineRenderer 以绘制路径
        lineRenderer.SetPositions(points);
    }
}
