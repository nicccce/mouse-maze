using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

// 游戏状态枚举
public enum GameState
{
    Default,    // 戏默认界面
    Play,       // 游戏进行中
    Pause,      // 游戏暂停
    Win,        //游戏胜利
    Manage,     // 管理迷宫
    Edit,     // 修改迷宫
    Path,       // 查看路径
}

// 游戏管理器
public class GameManager : MonoBehaviour
{
    public GameState currentState { get; set; } // 当前游戏状态

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
        // 检测是否按下 F11 键
        if (Input.GetKeyDown(KeyCode.F11))
        {
            // 隐藏UI，全屏或取消全屏
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
        ModText.text = "游戏中";
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
        ModText.text = "管理迷宫";
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
        ModText.text = "编辑迷宫";
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
        ModText.text = "查看路径";
    }

    public void OnShortestPathButtonClicked()
    {
        pathDrawer.DrawShortestPath();
        PathCount.SetActive(false);
        NextAllPathButton.SetActive(false);
        ModText.text = "最短路径";
    }

    public void OnAllPathButtonClicked()
    {
        PathCountText.text = pathDrawer.DrawAllPath();
        ModText.text = "所有路径";
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
        ModText.text = "管理迷宫";
        currentState = GameState.Manage;
    }

    public void OnSaveButtonClicked()
    {
        if (mapManager.SaveChanges())
        {
            ResetPanel();
            ManagePanel.SetActive(true);
            mapManager.RefreshMazeManagePanel();
            ModText.text = "管理迷宫";
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
        ModText.text = "管理迷宫";
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
