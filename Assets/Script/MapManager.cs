using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public List<GameObject> floorPrefabs; // 地板预制体列表
    public List<GameObject> blockPrefabs; // 方块预制体列表
    public int gridSize = 16; // 地图大小
    public GameManager gameManager;
    public Dropdown mazeDropDown;

    private Dictionary<string, GameObject> blockDictionary = new Dictionary<string, GameObject>(); //迷宫方块字典
    private string mapsDirectory;   //迷宫文件目录
    private List<MazeData> mazeList = new List<MazeData>(); //迷宫列表
    private string defaultMazePath; //默认迷宫文件名
    private int currentMaze;

    void Start()
    {
        // 获取存储路径
        mapsDirectory = Path.Combine(Application.persistentDataPath, "maps");
        defaultMazePath = Path.Combine(mapsDirectory, "default.maze");

        // 读取或创建默认迷宫
        LoadAllMazes();

        // 生成地图
        GenerateFloor();
        GenerateBorder();
        GenerateBlock();

        //生成迷宫
        LoadMaze();

        RefreshMazeDropDown();
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

    [System.Serializable]
    public class MazeData
    {
        public string name;
        public int size;
        public bool[] grid; // 线性存储 16x16 迷宫矩阵

        // 初始化方法
        public void Initialize(int size)
        {
            this.size = size;
            grid = new bool[size * size]; // 默认全 false（未激活）
        }

        public bool GetCell(int x, int z)
        {
            return grid[x * size + z];
        }

        public void SetCell(int x, int z, bool value)
        {
            grid[x * size + z] = value;
        }
    }

    void LoadAllMazes()
    {
        // 确保 Maps 目录存在
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // 获取所有 .maze 文件
        string[] mazeFiles = Directory.GetFiles(mapsDirectory, "*.maze");

        if (mazeFiles.Length == 0 || !File.Exists(defaultMazePath))
        {
            // 如果没有默认迷宫，则创建一个
            CreateDefaultMaze();
        }

        // 清空列表，防止重复加载
        mazeList.Clear();

        // 读取所有迷宫文件
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file);
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json);
            mazeList.Add(mazeData);

            // 匹配文件名
            if (Path.GetFileName(file) == Path.GetFileName(defaultMazePath))
            {
                currentMaze = i;
            }
        }
    }

    void CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // 显式初始化 grid
        defaultMaze.name = "default";

        // 序列化并写入 default.maze
        string json = JsonUtility.ToJson(defaultMaze, true);
        File.WriteAllText(defaultMazePath, json);
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

    // 放置方块
    void PlaceBlock(int x, int z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, new Vector3(-gridSize / 2 + x + 0.5f, 0.5f, -gridSize / 2 + z + 0.5f), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);

        string name = blockName(x, z);
        block.name = name;

        block.SetActive(false); // 根据迷宫数据决定是否激活
        blockDictionary[name] = block;
        block.transform.parent = this.transform;
    }

    string blockName(int x, int z)
    {
        return $"block({x},{z})";
    }

    //放置迷宫
    void LoadMaze()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (mazeList[currentMaze].GetCell(x, z))
                {
                    blockDictionary[blockName(x, z)].SetActive(true);
                }
                else
                {
                    blockDictionary[blockName(x, z)].SetActive(false);
                }
            }
        }
    }

    void RefreshMazeDropDown()
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
    } 
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.y); // y轴高度

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        return new Vector3(worldPos.x, 0, worldPos.z); // 只返回 XZ 平面坐标
    }

    void changeBlock()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键点击
        {
            //获取鼠标点击的迷宫方块
            Vector3 worldPos = GetMouseWorldPosition();
            int x = Mathf.FloorToInt(worldPos.x)+8;
            int z = Mathf.FloorToInt(worldPos.z)+8;
            if(x < 0  || z < 0 || x > 15 || z > 15 || (x == 7 && z == 8) || (x == 15 && z == 0))
            {
                return;
            }
            GameObject selectedBlock = blockDictionary[blockName(x, z)];

            selectedBlock.SetActive(!selectedBlock.activeSelf);
            mazeList[currentMaze].SetCell(x, z, selectedBlock.activeSelf);

        }
    }
}