using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class MapManager : MonoBehaviour
{
    public List<GameObject> floorPrefabs; // �ذ�Ԥ�����б�
    public List<GameObject> blockPrefabs; // ����Ԥ�����б�
    public int gridSize = 16; // ��ͼ��С
    public GameManager gameManager;
    
    //�����Թ�����
    public Dropdown mazeDropDown;
    public InputField mazeNameTextField;

    private Dictionary<(int, int), GameObject> blockDictionary = new Dictionary<(int, int), GameObject>(); //�Թ������ֵ�
    private string mapsDirectory;   //�Թ��ļ�Ŀ¼
    private List<MazeData> mazeList = new List<MazeData>(); //�Թ��б�
    private string defaultMazePath; //Ĭ���Թ��ļ���
    private int currentMaze;
    private Camera mainCamera; // �����������

    void Start()
    {
        mainCamera = Camera.main; // ��ʼ��ʱ�����������
        tempGrid = new bool[gridSize * gridSize];   //��ʼ����ʱ����

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

    [System.Serializable]
    public class MazeData
    {
        public string name;
        public int size;
        public string fileName;
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
            mazeData.fileName = Path.GetFileName(file);
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

    MazeData CreateNewMaze()
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

    void SaveMazeData(MazeData mazeData)
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

    void DeleteMazeData(MazeData mazeData)
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

    //�����Թ�
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

        block.name = blockName(x, z);

        block.SetActive(false); // Ĭ��Ϊ�Ǽ���״̬
        blockDictionary[(x,z)] = block;
        block.transform.parent = this.transform;
    }

    string blockName(int x, int z)
    {
        return $"block({x},{z})";
    }

    //�����Թ�
    void LoadMaze()
    {
        MazeData currentMazeData = mazeList[currentMaze];
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

        mazeNameTextField.text = mazeList[currentMaze].name;

        gameManager.currentState = GameState.Manage;
    }

    public void OnMazeNameTextFieldChanged()
    {
        // ����ݹ����
        if (mazeNameTextField.text == mazeList[currentMaze].name)
        {
            return;
        }
        if (mazeNameTextField.text == "")
        {
            mazeNameTextField.text = mazeList[currentMaze].name;
            return;
        }

        mazeList[currentMaze].name = mazeNameTextField.text;
        SaveMazeData(mazeList[currentMaze]);
        RefreshMazeManagePanel();
    }

    public void OnMazeDropDownChanged()
    {
        // ����ݹ����
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
        MazeData newMaze = CreateNewMaze();
        mazeList.Add(newMaze);
        currentMaze = mazeList.Count - 1;
        LoadMaze ();
        RefreshMazeManagePanel();
    }

    public void DeleteCurrentMaze()
    {
        DeleteMazeData(mazeList[currentMaze]);
        LoadAllMazes();
        LoadMaze();
        RefreshMazeManagePanel();
    }


    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.y); // y��߶�
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private bool[] tempGrid;

    void changeBlock()
    {
        if (Input.GetMouseButtonDown(0)) // ���������
        {
            // ��ȡ��������Թ�����
            Vector3 worldPos = GetMouseWorldPosition();
            int x = Mathf.FloorToInt(worldPos.x) + 8;
            int z = Mathf.FloorToInt(worldPos.z) + 8;

            // �߽���
            if (x < 0 || z < 0 || x > 15 || z > 15 || (x == 7 && z == 8) || (x == 15 && z == 0))
            {
                return;
            }

            // �л� tempGrid ��״̬
            int index = x * gridSize + z;
            tempGrid[index] = !tempGrid[index];

            // ���� UI Ԥ��
            GameObject selectedBlock = blockDictionary[(x, z)];
            selectedBlock.SetActive(tempGrid[index]);
        }
    }

    // ȷ���޸ģ��� tempGrid д�� mazeList[currentMaze]
    public void SaveChanges()
    {
        Array.Copy(tempGrid, mazeList[currentMaze].grid, tempGrid.Length);
        SaveMazeData(mazeList[currentMaze]);
    }
}