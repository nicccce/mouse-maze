using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private string mapsDirectory;   // �Թ��ļ�Ŀ¼
    private int gridSize;           // �Թ��Ĵ�С������ 16x16��

    // ���캯������ʼ���Թ��ļ�Ŀ¼���Թ���С
    public FileManager(string mapsDirectory, int gridSize)
    {
        this.mapsDirectory = mapsDirectory;
        this.gridSize = gridSize;
    }

    // ���������Թ��ļ��������䷴���л�Ϊ MazeData ������봫����б�
    // ����Ĭ���Թ����±�
    public int LoadAllMazes(List<MazeData> mazeList)
    {
        // ȷ�� Maps Ŀ¼����
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // ��ȡ���� .maze �ļ�
        string[] mazeFiles = Directory.GetFiles(mapsDirectory, "*.maze");

        // ����б���ֹ�ظ�����
        mazeList.Clear();

        // ���û��Ĭ���Թ��ļ����򴴽�һ��
        if (mazeFiles.Length == 0 || !File.Exists(Path.Combine(mapsDirectory, "default.maze")))
        {
            mazeList.Add(CreateDefaultMaze());
        }

        int currentMaze = 0; // Ĭ���Թ����±�

        // ��ȡ�����Թ��ļ�
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file); // ��ȡ�ļ�����
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json); // �����л�Ϊ MazeData ����
            mazeData.fileName = Path.GetFileName(file); // �����ļ���
            mazeList.Add(mazeData); // ��ӵ��б�

            // �����ǰ�ļ���Ĭ���Թ�����¼���±�
            if (mazeData.fileName == "default.maze")
            {
                currentMaze = i;
            }
        }

        return currentMaze; // ����Ĭ���Թ����±�
    }

    // ����һ���µ��Թ��ļ��������� MazeData ����
    public MazeData CreateNewMaze()
    {
        // �����Թ�����
        string baseName = "Mouse_Maze";
        string newMazeName = baseName;
        string newMazePath;

        int counter = 1; // ��������Ψһ���Ƶļ�����

        // ȷ�� Maps Ŀ¼����
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // ����Ƿ�������ͬ���Ƶ��ļ�������Ψһ����
        do
        {
            newMazePath = Path.Combine(mapsDirectory, newMazeName + ".maze");

            if (File.Exists(newMazePath))
            {
                newMazeName = $"{baseName}_{counter++}"; // �������
            }
            else
            {
                break;
            }
        } while (true);

        // �����µ��Թ�����
        MazeData newMaze = new MazeData();
        newMaze.Initialize(gridSize); // ��ʼ���Թ�
        newMaze.name = newMazeName; // �����Թ�����

        // ���л���д���ļ�
        string json = JsonUtility.ToJson(newMaze, true);
        File.WriteAllText(newMazePath, json);

        return newMaze; // �����´����� MazeData ����
    }

    // ����Ĭ���Թ��ļ��������� MazeData ����
    public MazeData CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // ��ʼ���Թ�
        defaultMaze.name = "default"; // �����Թ�����
        defaultMaze.fileName = "default.maze"; // �����ļ���

        // ���л���д�� default.maze �ļ�
        string json = JsonUtility.ToJson(defaultMaze, true);
        File.WriteAllText(Path.Combine(mapsDirectory, "default.maze"), json);

        return defaultMaze; // ����Ĭ�ϵ� MazeData ����
    }

    // ���� MazeData �����ļ�
    public void SaveMazeData(MazeData mazeData)
    {
        // ��� MazeData ���ļ����Ƿ���Ч
        if (mazeData == null || string.IsNullOrEmpty(mazeData.fileName))
        {
            return;
        }

        // ��������·��
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // ���л�Ϊ JSON
        string json = JsonUtility.ToJson(mazeData, true);

        // д���ļ�
        File.WriteAllText(filePath, json);
    }

    // ɾ�� MazeData �����Ӧ���ļ�
    public void DeleteMazeData(MazeData mazeData)
    {
        // ��� MazeData ���ļ����Ƿ���Ч
        if (mazeData == null || string.IsNullOrEmpty(mazeData.fileName))
        {
            return;
        }

        // ����ļ���Ĭ���Թ����������Թ����ݣ�������ɾ���ļ�
        if (mazeData.fileName == "default.maze")
        {
            mazeData.name = "default";
            mazeData.Initialize(gridSize); // �����Թ�����
            SaveMazeData(mazeData); // �������ú���Թ�����
            return;
        }

        // ��������·��
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // ����ļ��Ƿ���ڣ����������ɾ��
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}