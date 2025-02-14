using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

// ��Ϸ״̬ö��
public enum GameState
{
    Default,    // ϷĬ�Ͻ���
    Play,       // ��Ϸ������
    Pause,      // ��Ϸ��ͣ
    Win,        //��Ϸʤ��
    Manage,     // �����Թ�
    Edit,     // �޸��Թ�
    Path,       // �鿴·��
}

// ��Ϸ������
public class GameManager : MonoBehaviour
{
    public GameState currentState { get; set; } // ��ǰ��Ϸ״̬

    public GameObject Canvas;
    public GameObject PlayPanel;
    public GameObject ManagePanel;
    public GameObject EditPanel;
    public GameObject PathPanel;

    public MapManager mapManager;
    public PlayManager playManager;

    public PathDrawer pathDrawer;
    public GameObject PathRenderer;
    public Text PathCountText;
    public GameObject PathCount;
    public GameObject NextAllPathButton;

    public GameObject NoPathErrorPanel;


    private Text ModText;

    void Start()
    {
        currentState = GameState.Default;
        ModText = GameObject.Find("Status").GetComponent<Text>();
    }

    void Update()
    {
        // ����Ƿ��� F11 ��
        if (Input.GetKeyDown(KeyCode.F11))
        {
            // ����UI��ȫ����ȡ��ȫ��
            Canvas.SetActive(!Canvas.activeSelf);
        }
    }

    public void OnStartButtonClicked()
    {
        if (currentState == GameState.Play || currentState == GameState.Pause || currentState == GameState.Win)
        {
            return;
        }
        ResetPanel();
        PlayPanel.SetActive(true);
        currentState = GameState.Play;
        playManager.Initialize();
        ModText.text = "��Ϸ��";
    }

    public void OnManageButtonClicked()
    {
        if (currentState == GameState.Manage || currentState == GameState.Edit || currentState == GameState.Win)
        {
            return;
        }
        ResetPanel();
        ManagePanel.SetActive(true);
        mapManager.RefreshMazeManagePanel();
        ModText.text = "�����Թ�";
        currentState = GameState.Manage;
    }

    public void ExitClicked()
    {
        if (currentState == GameState.Win)
        {
            return;
        }
        Application.Quit();
    }

    public void OnEditButtonClicked()
    {
        ResetPanel();
        EditPanel.SetActive(true);
        ModText.text = "�༭�Թ�";
        currentState = GameState.Edit;
    }

    public void OnPathButtonClicked()
    {
        if (currentState == GameState.Path || currentState == GameState.Win)
        {
            return;
        }
        ResetPanel();
        PathPanel.SetActive(true);
        currentState= GameState.Path;
        pathDrawer.ResetPath();
        PathRenderer.SetActive(true);
        PathCount.SetActive(false);
        NextAllPathButton.SetActive(false);
        ModText.text = "�鿴·��";
    }

    public void OnShortestPathButtonClicked()
    {
        pathDrawer.DrawShortestPath();
        PathCount.SetActive(false);
        NextAllPathButton.SetActive(false);
        ModText.text = "���·��";
    }

    public void OnAllPathButtonClicked()
    {
        PathCountText.text = pathDrawer.DrawAllPath();
        ModText.text = "����·��";
        PathCount.SetActive(true);
        NextAllPathButton.SetActive(true);
    }

    public void OnNextAllPathButtonClicked()
    {
        PathCountText.text = pathDrawer.NextAllPath();
    }

    public void BackToManagePanel()
    {
        ResetPanel();
        ManagePanel.SetActive(true);
        mapManager.RefreshMazeManagePanel();
        ModText.text = "�����Թ�";
        currentState = GameState.Manage;
    }

    public void OnSaveButtonClicked()
    {
        if (mapManager.SaveChanges())
        {
            ResetPanel();
            ManagePanel.SetActive(true);
            mapManager.RefreshMazeManagePanel();
            ModText.text = "�����Թ�";
            currentState = GameState.Manage;
        }
        else
        {
            NoPathErrorPanel.SetActive(true);
        }
    }

    public void OnAbortButtonClicked()
    {
        ResetPanel();
        ManagePanel.SetActive(true);
        mapManager.RefreshMazeManagePanel();
        ModText.text = "�����Թ�";
        currentState = GameState.Manage;
    }

    public void ResetPanel()
    {
        ModText.text = "";
        PlayPanel.SetActive(false);
        ManagePanel.SetActive(false);
        EditPanel.SetActive(false);
        PathPanel.SetActive(false);
        PathRenderer.SetActive(false);
        mapManager.ResetMap();
    }

}
