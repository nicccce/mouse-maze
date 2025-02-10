using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public List<GameObject> floorPrefabs; // �ذ�Ԥ�����б�
    public List<GameObject> blockPrefabs; // ����Ԥ�����б�
    public int gridSize = 16; // ��ͼ��С
    public GameManager gameManager;
    public Dropdown mazeDropDown;

    private Dictionary<string, GameObject> blockDictionary = new Dictionary<string, GameObject>(); //�Թ������ֵ�
    private string mapsDirectory;   //�Թ��ļ�Ŀ¼
    private List<MazeData> mazeList = new List<MazeData>(); //�Թ��б�
    private string defaultMazePath; //Ĭ���Թ��ļ���
    private int currentMaze;

    void Start()
    {
        // ��ȡ�洢·��
        mapsDirectory = Path.Combine(Application.persistentDataPath, "maps");
        defaultMazePath = Path.Combine(mapsDirectory, "default.maze");

        // ��ȡ�򴴽�Ĭ���Թ�
        LoadAllMazes();

        // ���ɵ�ͼ
        GenerateFloor();
        GenerateBorder();
        GenerateBlock();

        //�����Թ�
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
        public bool[] grid; // ���Դ洢 16x16 �Թ�����

        // ��ʼ������
        public void Initialize(int size)
        {
            this.size = size;
            grid = new bool[size * size]; // Ĭ��ȫ false��δ���
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
        // ȷ�� Maps Ŀ¼����
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        // ��ȡ���� .maze �ļ�
        string[] mazeFiles = Directory.GetFiles(mapsDirectory, "*.maze");

        if (mazeFiles.Length == 0 || !File.Exists(defaultMazePath))
        {
            // ���û��Ĭ���Թ����򴴽�һ��
            CreateDefaultMaze();
        }

        // ����б���ֹ�ظ�����
        mazeList.Clear();

        // ��ȡ�����Թ��ļ�
        for (int i = 0; i < mazeFiles.Length; i++)
        {
            string file = mazeFiles[i];
            string json = File.ReadAllText(file);
            MazeData mazeData = JsonUtility.FromJson<MazeData>(json);
            mazeList.Add(mazeData);

            // ƥ���ļ���
            if (Path.GetFileName(file) == Path.GetFileName(defaultMazePath))
            {
                currentMaze = i;
            }
        }
    }

    void CreateDefaultMaze()
    {
        MazeData defaultMaze = new MazeData();
        defaultMaze.Initialize(gridSize); // ��ʽ��ʼ�� grid
        defaultMaze.name = "default";

        // ���л���д�� default.maze
        string json = JsonUtility.ToJson(defaultMaze, true);
        File.WriteAllText(defaultMazePath, json);
    }

    // ���ɵذ�
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

    // ���õذ�
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

    // ���ɱ߽緽��
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
                PlaceBlock(x, z); // ��ȡ�Թ����ݾ����Ƿ񼤻�
            }
        }
    }

    // ���ñ߽緽��
    void PlaceBorderBlock(float x, float z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, new Vector3(x, 0.5f, z), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);
        block.transform.parent = this.transform;
    }

    // ���÷���
    void PlaceBlock(int x, int z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, new Vector3(-gridSize / 2 + x + 0.5f, 0.5f, -gridSize / 2 + z + 0.5f), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);

        string name = blockName(x, z);
        block.name = name;

        block.SetActive(false); // �����Թ����ݾ����Ƿ񼤻�
        blockDictionary[name] = block;
        block.transform.parent = this.transform;
    }

    string blockName(int x, int z)
    {
        return $"block({x},{z})";
    }

    //�����Թ�
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
        // ��յ�ǰѡ��
        mazeDropDown.ClearOptions();

        // ����µ�ѡ��
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
        mousePos.z = Mathf.Abs(Camera.main.transform.position.y); // y��߶�

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        return new Vector3(worldPos.x, 0, worldPos.z); // ֻ���� XZ ƽ������
    }

    void changeBlock()
    {
        if (Input.GetMouseButtonDown(0)) // ���������
        {
            //��ȡ��������Թ�����
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