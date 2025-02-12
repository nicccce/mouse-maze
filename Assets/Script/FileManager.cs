using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private string mapsDirectory;   //�Թ��ļ�Ŀ¼
    private int gridSize;

    public FileManager(string mapsDirectory, int gridSize)
    {
        this.mapsDirectory = mapsDirectory;
        this.gridSize = gridSize;
    }

    // ����ȡ�������л����Թ��ļ����봫������飬������Ĭ���Թ����±�
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

        if (mazeFiles.Length == 0 || !File.Exists(Path.Combine(mapsDirectory, "default.maze")))
        {
            // ���û��Ĭ���Թ����򴴽�һ��
            mazeList.Add(CreateDefaultMaze());
        }

        int currentMaze = 0;

        // ��ȡ�����Թ��ļ�
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file);
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json);
            mazeData.fileName = Path.GetFileName(file);
            mazeList.Add(mazeData);

            // ƥ���ļ���
            if (mazeData.fileName == "default.maze")
            {
                currentMaze = i;
            }
        }
        
        return currentMaze;
    }

    public MazeData CreateNewMaze()
    {
        // �����Թ�����
        string baseName = "Mouse_Maze";
        string newMazeName = baseName;
        string newMazePath;

        int counter = 1;

        // ȷ�� Maps Ŀ¼����
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // ����Ƿ�������ͬ���Ƶ��ļ�
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
        newMaze.Initialize(gridSize);
        newMaze.name = newMazeName; // ����Ψһ����

        // ���л���д���ļ�
        string json = JsonUtility.ToJson(newMaze, true);
        File.WriteAllText(newMazePath, json);

        return newMaze;
    }

    public MazeData CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // ��ʽ��ʼ�� grid
        defaultMaze.name = "default";
        defaultMaze.fileName = "default.maze";

        // ���л���д�� default.maze
        string json = JsonUtility.ToJson(defaultMaze, true);
        File.WriteAllText(Path.Combine(mapsDirectory, "default.maze"), json);

        return defaultMaze;
    }

    public void SaveMazeData(MazeData mazeData)
    {
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

    public void DeleteMazeData(MazeData mazeData)
    {
        if (mazeData == null || string.IsNullOrEmpty(mazeData.fileName))
        {
            return;
        }

        if (mazeData.fileName == "default.maze")
        {
            mazeData.name = "default";
            mazeData.Initialize(gridSize);
            SaveMazeData(mazeData);
            return;
        }

        // ��������·��
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // ����ļ��Ƿ����
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return;
        }
    }

}
