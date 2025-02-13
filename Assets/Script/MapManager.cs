using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using static MapManager;
using System.Drawing;

public class MapManager : MonoBehaviour
{
    public List<GameObject> floorPrefabs; // 地板预制体列表
    public List<GameObject> blockPrefabs; // 方块预制体列表
    public int gridSize; // 地图大小
    public GameManager gameManager;

    //管理迷宫界面
    public Dropdown mazeDropDown;
    public InputField mazeNameTextField;

    private Dictionary<(int, int), GameObject> blockDictionary = new Dictionary<(int, int), GameObject>(); //迷宫方块字典
    private List<MazeData> mazeList = new List<MazeData>(); //迷宫列表
    private int currentMaze;
    private Camera mainCamera; // 缓存主摄像机

    private FileManager fileManager;

    void Start()
    {
        mainCamera = Camera.main; // 初始化时缓存主摄像机
        tempGrid = new long[gridSize];   //初始化临时数组

        // 获取存储路径
        fileManager = new FileManager(Path.Combine(Application.persistentDataPath, "maps"), gridSize);

        mainCamera.orthographicSize = (gridSize+1.0f)/2; // 设置正交投影大小

        // 读取或创建默认迷宫
        currentMaze = fileManager.LoadAllMazes(mazeList);

        // 生成地图
        GenerateFloor();
        GenerateBorder();
        GenerateBlock();

        //生成迷宫
        LoadMaze();
    }

    public void ResetMap()
    {
        LoadMaze();
    }

    void Update()
    {
        switch (gameManager.currentState)
        {
            case GameState.Edit:
                changeBlock();
                break;
        }
    }

    // 生成地板
    void GenerateFloor()
    {
        for (int x = -gridSize + 1; x < gridSize; x += 2)
        {
            for (int z = -gridSize + 1; z < gridSize; z += 2)
            {
                PlaceFloor(x, z);
            }
        }
    }

    // 放置地板
    void PlaceFloor(int x, int z)
    {
        if (floorPrefabs.Count == 0) return;

        GameObject prefab = floorPrefabs[UnityEngine.Random.Range(0, floorPrefabs.Count)];
        GameObject tile = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        Vector3 originalSize = tile.GetComponent<Renderer>().bounds.size;
        float scaleX = 2 * 1f / originalSize.x;
        float scaleZ = 2 * 1f / originalSize.z;

        tile.transform.localScale = new Vector3(scaleX, tile.transform.localScale.y, scaleZ);
        tile.transform.parent = this.transform;
    }

    // 生成边界方块
    void GenerateBorder()
    {
        if (blockPrefabs.Count == 0) return;

        for (int x = -gridSize / 2; x <= gridSize / 2 + 1; x++)
        {
            PlaceBorderBlock(x - 0.5f, -gridSize / 2 - 0.5f);
            PlaceBorderBlock(x - 0.5f, gridSize / 2 + 0.5f);
        }

        for (int z = -gridSize / 2; z < gridSize / 2; z++)
        {
            PlaceBorderBlock(-gridSize / 2 - 0.5f, z + 0.5f);
            PlaceBorderBlock(gridSize / 2 + 0.5f, z + 0.5f);
        }
    }

    //初始化迷宫方块
    void GenerateBlock()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                PlaceBlock(x, z); // 读取迷宫数据决定是否激活
            }
        }
    }

    // 放置边界方块
    void PlaceBorderBlock(float x, float z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, new Vector3(x, 0.5f, z), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);
        block.transform.parent = this.transform;
    }

    // 将矩阵下标 (x, z) 转换为世界坐标
    public Vector3 GridToWorldPosition(int x, int z, int size)
    {
        return new Vector3(-size / 2 + x + 0.5f, 0.5f, -size / 2 + z + 0.5f);
    }

    public MazeData GetCurrentMazeData()
    {
        return mazeList[currentMaze];
    }

    // 放置方块
    void PlaceBlock(int x, int z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, GridToWorldPosition(x, z, gridSize), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);

        block.name = $"block({x},{z})";

        block.SetActive(false); // 默认为非激活状态
        blockDictionary[(x,z)] = block;
        block.transform.parent = this.transform;
    }

    //放置迷宫
    void LoadMaze()
    {
        MazeData currentMazeData = GetCurrentMazeData();
        Array.Copy(currentMazeData.grid, tempGrid, tempGrid.Length);
        for (int x = 0; x < currentMazeData.size; x++)
        {
            for (int z = 0; z < currentMazeData.size; z++)
            {
                if (currentMazeData.GetCell(x, z))
                {
                    blockDictionary[(x, z)].SetActive(true);
                }
                else
                {
                    blockDictionary[(x, z)].SetActive(false);
                }
            }
        }
    }

    public void RefreshMazeManagePanel()
    {
        // 清空当前选项
        mazeDropDown.ClearOptions();

        // 添加新的选项
        List<string> options = new List<string> { };
       foreach (MazeData d in mazeList)
        {
            options.Add(d.name);
        }
        mazeDropDown.AddOptions(options);

        mazeDropDown.value = currentMaze;

        mazeNameTextField.text = GetCurrentMazeData().name;

        gameManager.currentState = GameState.Manage;
    }

    public void OnMazeNameTextFieldChanged()
    {
        // 避免递归调用
        if (mazeNameTextField.text == GetCurrentMazeData().name)
        {
            return;
        }
        if (mazeNameTextField.text == "")
        {
            mazeNameTextField.text = GetCurrentMazeData().name;
            return;
        }

        GetCurrentMazeData().name = mazeNameTextField.text;
        fileManager.SaveMazeData(GetCurrentMazeData());
        RefreshMazeManagePanel();
    }

    public void OnMazeDropDownChanged()
    {
        // 避免递归调用
        if (mazeDropDown.value == currentMaze)
        {
            return;
        }
        currentMaze = mazeDropDown.value;
        LoadMaze();
        RefreshMazeManagePanel();
    }

    public void OnNewButtionClicked()
    {
        MazeData newMaze = fileManager.CreateNewMaze();
        mazeList.Add(newMaze);
        currentMaze = mazeList.Count - 1;
        LoadMaze ();
        RefreshMazeManagePanel();
    }

    public void DeleteCurrentMaze()
    {
        fileManager.DeleteMazeData(GetCurrentMazeData());
        currentMaze = fileManager.LoadAllMazes(mazeList);
        LoadMaze();
        RefreshMazeManagePanel();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.y); // y轴高度
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    // 临时的用于修改的地图
    private long[] tempGrid;

    // 获取临时地图的值
    private bool GetTempGridCell(int x, int z)
    {
        return (tempGrid[z] & (1L << x)) != 0L; // 第 index 位是否为 1
    }

    //修改临时地图的值
    public void SetTempGridCell(int x, int z, bool value)
    {
        if (x < 0 || z < 0 || x >= gridSize || z >= gridSize)
            return;

        if (value)
            tempGrid[z] |= (1L << x); // 将第 index 位设为 1
        else
            tempGrid[z] &= ~(1L << x); // 将第 index 位设为 0
    }

    void changeBlock()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键点击
        {
            // 获取鼠标点击的迷宫方块
            Vector3 worldPos = GetMouseWorldPosition();
            int x = Mathf.FloorToInt(worldPos.x) + gridSize/2;
            int z = Mathf.FloorToInt(worldPos.z) + gridSize/2;

            // 边界检查
            if (!GetCurrentMazeData().IsInBounds((x, z)) || GetCurrentMazeData().IsStratOrEnd((x, z)))
            {
                return;
            }

            // 切换 tempGrid 的状态
            SetTempGridCell(x, z, !GetTempGridCell(x, z));

            // 更新 UI 预览
            GameObject selectedBlock = blockDictionary[(x, z)];
            selectedBlock.SetActive(GetTempGridCell(x, z));
        }
    }

    // 确认修改，将 tempGrid 写入 GetCurrentMazeData()
    public bool SaveChanges()
    {
        MazeData currentMazeData = GetCurrentMazeData();
        long[] temp = currentMazeData.grid;
        currentMazeData.grid = tempGrid;
        tempGrid = temp;

        List<(int, int)> shortestPath = currentMazeData.FindShortestPath();
        // 如果没有通路
        if (shortestPath.Count == 0)
        {
            //回溯
            tempGrid = currentMazeData.grid;
            currentMazeData.grid = temp;
            return false;
        }

        // 如果存在通路，则保存
        Array.Copy(currentMazeData.grid, tempGrid, tempGrid.Length);
        currentMazeData.shortestPath = shortestPath;
        fileManager.SaveMazeData(GetCurrentMazeData());
        return true;
    }

}