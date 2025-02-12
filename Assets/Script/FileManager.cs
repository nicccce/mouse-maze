using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private string mapsDirectory;   //迷宫文件目录
    private int gridSize;

    public FileManager(string mapsDirectory, int gridSize)
    {
        this.mapsDirectory = mapsDirectory;
        this.gridSize = gridSize;
    }

    // 将读取并反序列化的迷宫文件存入传入的数组，并返回默认迷宫的下标
    public int LoadAllMazes(List<MazeData> mazeList)
    {
        // 确保 Maps 目录存在
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // 获取所有 .maze 文件
        string[] mazeFiles = Directory.GetFiles(mapsDirectory, "*.maze");

        // 清空列表，防止重复加载
        mazeList.Clear();

        if (mazeFiles.Length == 0 || !File.Exists(Path.Combine(mapsDirectory, "default.maze")))
        {
            // 如果没有默认迷宫，则创建一个
            mazeList.Add(CreateDefaultMaze());
        }

        int currentMaze = 0;

        // 读取所有迷宫文件
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file);
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json);
            mazeData.fileName = Path.GetFileName(file);
            mazeList.Add(mazeData);

            // 匹配文件名
            if (mazeData.fileName == "default.maze")
            {
                currentMaze = i;
            }
        }
        
        return currentMaze;
    }

    public MazeData CreateNewMaze()
    {
        // 基础迷宫名称
        string baseName = "Mouse_Maze";
        string newMazeName = baseName;
        string newMazePath;

        int counter = 1;

        // 确保 Maps 目录存在
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // 检查是否已有相同名称的文件
        do
        {
            newMazePath = Path.Combine(mapsDirectory, newMazeName + ".maze");

            if (File.Exists(newMazePath))
            {
                newMazeName = $"{baseName}_{counter++}"; // 递增编号
            }
            else
            {
                break;
            }
        } while (true);

        // 创建新的迷宫数据
        MazeData newMaze = new MazeData();
        newMaze.Initialize(gridSize);
        newMaze.name = newMazeName; // 赋予唯一名称

        // 序列化并写入文件
        string json = JsonUtility.ToJson(newMaze, true);
        File.WriteAllText(newMazePath, json);

        return newMaze;
    }

    public MazeData CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // 显式初始化 grid
        defaultMaze.name = "default";
        defaultMaze.fileName = "default.maze";

        // 序列化并写入 default.maze
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

        // 生成完整路径
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // 序列化为 JSON
        string json = JsonUtility.ToJson(mazeData, true);

        // 写入文件
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

        // 生成完整路径
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // 检查文件是否存在
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return;
        }
    }

}
