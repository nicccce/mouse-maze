using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private string mapsDirectory;   // 迷宫文件目录
    private int gridSize;           // 迷宫的大小（例如 16x16）

    // 构造函数，初始化迷宫文件目录和迷宫大小
    public FileManager(string mapsDirectory, int gridSize)
    {
        this.mapsDirectory = mapsDirectory;
        this.gridSize = gridSize;
    }

    // 加载所有迷宫文件，并将其反序列化为 MazeData 对象存入传入的列表
    // 返回默认迷宫的下标
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

        // 如果没有默认迷宫文件，则创建一个
        if (mazeFiles.Length == 0 || !File.Exists(Path.Combine(mapsDirectory, "default.maze")))
        {
            mazeList.Add(CreateDefaultMaze());
        }

        int currentMaze = 0; // 默认迷宫的下标

        // 读取所有迷宫文件
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file); // 读取文件内容
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json); // 反序列化为 MazeData 对象
            mazeData.fileName = Path.GetFileName(file); // 设置文件名
            mazeList.Add(mazeData); // 添加到列表

            // 如果当前文件是默认迷宫，记录其下标
            if (mazeData.fileName == "default.maze")
            {
                currentMaze = i;
            }
        }

        return currentMaze; // 返回默认迷宫的下标
    }

    // 创建一个新的迷宫文件，并返回 MazeData 对象
    public MazeData CreateNewMaze()
    {
        // 基础迷宫名称
        string baseName = "Mouse_Maze";
        string newMazeName = baseName;
        string newMazePath;

        int counter = 1; // 用于生成唯一名称的计数器

        // 确保 Maps 目录存在
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // 检查是否已有相同名称的文件，生成唯一名称
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
        newMaze.Initialize(gridSize); // 初始化迷宫
        newMaze.name = newMazeName; // 设置迷宫名称

        // 序列化并写入文件
        string json = JsonUtility.ToJson(newMaze, true);
        File.WriteAllText(newMazePath, json);

        return newMaze; // 返回新创建的 MazeData 对象
    }

    // 创建默认迷宫文件，并返回 MazeData 对象
    public MazeData CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // 初始化迷宫
        defaultMaze.name = "default"; // 设置迷宫名称
        defaultMaze.fileName = "default.maze"; // 设置文件名

        // 序列化并写入 default.maze 文件
        string json = JsonUtility.ToJson(defaultMaze, true);
        File.WriteAllText(Path.Combine(mapsDirectory, "default.maze"), json);

        return defaultMaze; // 返回默认的 MazeData 对象
    }

    // 保存 MazeData 对象到文件
    public void SaveMazeData(MazeData mazeData)
    {
        // 检查 MazeData 和文件名是否有效
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

    // 删除 MazeData 对象对应的文件
    public void DeleteMazeData(MazeData mazeData)
    {
        // 检查 MazeData 和文件名是否有效
        if (mazeData == null || string.IsNullOrEmpty(mazeData.fileName))
        {
            return;
        }

        // 如果文件是默认迷宫，则重置迷宫数据，而不是删除文件
        if (mazeData.fileName == "default.maze")
        {
            mazeData.name = "default";
            mazeData.Initialize(gridSize); // 重置迷宫数据
            SaveMazeData(mazeData); // 保存重置后的迷宫数据
            return;
        }

        // 生成完整路径
        string filePath = Path.Combine(mapsDirectory, mazeData.fileName);

        // 检查文件是否存在，如果存在则删除
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}