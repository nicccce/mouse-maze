using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

// 游戏状态枚举
public enum GameState
{
    Default,    // 戏默认界面
    Play,       // 游戏进行中
    Pause,      // 游戏暂停
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
    public MapManager mapManager;
    public GameObject EditPanel;

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
        if (currentState == GameState.Play || currentState == GameState.Pause)
        {
            return;
        }
        ResetPanel();
        PlayPanel.SetActive(true);
        currentState = GameState.Play;
        ModText.text = "游戏中";
    }

    public void OnManageButtonClicked()
    {
        if (currentState == GameState.Manage || currentState == GameState.Edit)
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
        Application.Quit();
    }

    public void OnEditButtonClicked()
    {
        ResetPanel();
        EditPanel.SetActive(true);
        ModText.text = "编辑迷宫";
        currentState = GameState.Edit;
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

    void ResetPanel()
    {
        PlayPanel.SetActive(false);
        ManagePanel.SetActive(false);
        EditPanel.SetActive(false);
        mapManager.ResetMap();
    }

}
