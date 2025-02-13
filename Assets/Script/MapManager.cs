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
    public List<GameObject> floorPrefabs; // �ذ�Ԥ�����б�
    public List<GameObject> blockPrefabs; // ����Ԥ�����б�
    public int gridSize; // ��ͼ��С
    public GameManager gameManager;

    //�����Թ�����
    public Dropdown mazeDropDown;
    public InputField mazeNameTextField;

    private Dictionary<(int, int), GameObject> blockDictionary = new Dictionary<(int, int), GameObject>(); //�Թ������ֵ�
    private List<MazeData> mazeList = new List<MazeData>(); //�Թ��б�
    private int currentMaze;
    private Camera mainCamera; // �����������

    private FileManager fileManager;

    void Start()
    {
        mainCamera = Camera.main; // ��ʼ��ʱ�����������
        tempGrid = new long[gridSize];   //��ʼ����ʱ����

        // ��ȡ�洢·��
        fileManager = new FileManager(Path.Combine(Application.persistentDataPath, "maps"), gridSize);

        mainCamera.orthographicSize = (gridSize+1.0f)/2; // ��������ͶӰ��С

        // ��ȡ�򴴽�Ĭ���Թ�
        currentMaze = fileManager.LoadAllMazes(mazeList);

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

    //��ʼ���Թ�����
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

    // �������±� (x, z) ת��Ϊ��������
    public Vector3 GridToWorldPosition(int x, int z, int size)
    {
        return new Vector3(-size / 2 + x + 0.5f, 0.5f, -size / 2 + z + 0.5f);
    }

    public MazeData GetCurrentMazeData()
    {
        return mazeList[currentMaze];
    }

    // ���÷���
    void PlaceBlock(int x, int z)
    {
        GameObject prefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        GameObject block = Instantiate(prefab, GridToWorldPosition(x, z, gridSize), Quaternion.identity);

        Vector3 originalSize = block.GetComponent<Renderer>().bounds.size;
        block.transform.localScale = new Vector3(1f / originalSize.x, 1f / originalSize.y, 1f / originalSize.z);

        block.name = $"block({x},{z})";

        block.SetActive(false); // Ĭ��Ϊ�Ǽ���״̬
        blockDictionary[(x,z)] = block;
        block.transform.parent = this.transform;
    }

    //�����Թ�
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

        mazeNameTextField.text = GetCurrentMazeData().name;

        gameManager.currentState = GameState.Manage;
    }

    public void OnMazeNameTextFieldChanged()
    {
        // ����ݹ����
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
        mousePos.z = Mathf.Abs(mainCamera.transform.position.y); // y��߶�
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    // ��ʱ�������޸ĵĵ�ͼ
    private long[] tempGrid;

    // ��ȡ��ʱ��ͼ��ֵ
    private bool GetTempGridCell(int x, int z)
    {
        return (tempGrid[z] & (1L << x)) != 0L; // �� index λ�Ƿ�Ϊ 1
    }

    //�޸���ʱ��ͼ��ֵ
    public void SetTempGridCell(int x, int z, bool value)
    {
        if (x < 0 || z < 0 || x >= gridSize || z >= gridSize)
            return;

        if (value)
            tempGrid[z] |= (1L << x); // ���� index λ��Ϊ 1
        else
            tempGrid[z] &= ~(1L << x); // ���� index λ��Ϊ 0
    }

    void changeBlock()
    {
        if (Input.GetMouseButtonDown(0)) // ���������
        {
            // ��ȡ��������Թ�����
            Vector3 worldPos = GetMouseWorldPosition();
            int x = Mathf.FloorToInt(worldPos.x) + gridSize/2;
            int z = Mathf.FloorToInt(worldPos.z) + gridSize/2;

            // �߽���
            if (!GetCurrentMazeData().IsInBounds((x, z)) || GetCurrentMazeData().IsStratOrEnd((x, z)))
            {
                return;
            }

            // �л� tempGrid ��״̬
            SetTempGridCell(x, z, !GetTempGridCell(x, z));

            // ���� UI Ԥ��
            GameObject selectedBlock = blockDictionary[(x, z)];
            selectedBlock.SetActive(GetTempGridCell(x, z));
        }
    }

    // ȷ���޸ģ��� tempGrid д�� GetCurrentMazeData()
    public bool SaveChanges()
    {
        MazeData currentMazeData = GetCurrentMazeData();
        long[] temp = currentMazeData.grid;
        currentMazeData.grid = tempGrid;
        tempGrid = temp;

        List<(int, int)> shortestPath = currentMazeData.FindShortestPath();
        // ���û��ͨ·
        if (shortestPath.Count == 0)
        {
            //����
            tempGrid = currentMazeData.grid;
            currentMazeData.grid = temp;
            return false;
        }

        // �������ͨ·���򱣴�
        Array.Copy(currentMazeData.grid, tempGrid, tempGrid.Length);
        currentMazeData.shortestPath = shortestPath;
        fileManager.SaveMazeData(GetCurrentMazeData());
        return true;
    }

}