using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;  // ���� LineRenderer �������������·��
    public MapManager mapManager;      // ���� MapManager��������ȡ�Թ�����

    private Vector3[] points;  // �洢����·���ĵ�
    private List<List<(int, int)>> allPaths;  // �洢����·�����б�
    private int allPathIndex;  // ��ǰ��ʾ·��������

    // ��ʼ��ʱ���� LineRenderer �Ļ�������
    void Start()
    {
        lineRenderer.positionCount = 0;  // ��ʼ��ʱ·������Ϊ0
        lineRenderer.startWidth = 0.2f;  // ����������ʼ���
        lineRenderer.endWidth = 0.2f;    // ���������������
    }

    // �������·��
    public void DrawShortestPath()
    {
        // ��ȡ��ǰ�Թ������·����������
        DrawPolyline(mapManager.GetCurrentMazeData().shortestPath);
    }

    // ��������·���еĵ�һ��
    public string DrawAllPath()
    {
        allPathIndex = 0;  // ����·������Ϊ0
        allPaths = mapManager.GetCurrentMazeData().GetAllPaths(
                maxWidth: 100,
                maxDepth: 200,
                timeout: TimeSpan.FromSeconds(2)
            );  // ��ȡ����·��
        if (allPaths.Count == 0)
        {
            return "ϡ��ͼ���޷���ȡ";
        }
        DrawPolyline(allPaths[0]);  // ���Ƶ�һ��·��
        return $"{allPathIndex + 1}/{allPaths.Count}";  // ���ص�ǰ·����������·������
    }

    // ������һ��·��
    public string NextAllPath()
    {
        if (allPaths.Count == 0)
        {
            return "ϡ��ͼ���޷���ȡ";
        }
        // ����·��������ѭ����ʾ·��
        allPathIndex = (allPathIndex + 1) % allPaths.Count;
        DrawPolyline(allPaths[allPathIndex]);  // ���Ƶ�ǰ������Ӧ��·��
        return $"{allPathIndex + 1}/{allPaths.Count}";  // ���ص�ǰ·����������·������
    }

    // ����·���������ǰ��ʾ��·��
    public void ResetPath()
    {
        lineRenderer.positionCount = 0;  // ��·����������Ϊ0�����·����ʾ
    }

    // ���ݴ����·�����ݻ�������·��
    void DrawPolyline(List<(int, int)> path)
    {
        // ��·���ĵ���ת��Ϊ Vector3 ���飬�� LineRenderer ʹ��
        points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            // ���Թ�����ת��Ϊ��������
            points[i] = mapManager.GridToWorldPosition(path[i].Item1, path[i].Item2, mapManager.GetCurrentMazeData().size);
        }
        // ���� LineRenderer ��·��������
        lineRenderer.positionCount = points.Length;
        // ��·���㸳ֵ�� LineRenderer �Ի���·��
        lineRenderer.SetPositions(points);
    }
}
