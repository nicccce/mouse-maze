using UnityEngine;
using UnityEngine.UI;

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
        ClearPanel();
        PlayPanel.SetActive(true);
        currentState = GameState.Play;
        ModText.text = "游戏中";
        Debug.Log("6666");
    }

    public void OnManageButtonClicked()
    {
        if (currentState == GameState.Manage || currentState == GameState.Edit)
        {
            return;
        }
        ClearPanel();
        ManagePanel.SetActive(true);
        currentState = GameState.Manage;
        ModText.text = "管理迷宫";
    }

    public void ExitClicked()
    {
        Application.Quit();
    }

    void ClearPanel()
    {
        PlayPanel.SetActive(false);
        ManagePanel.SetActive(false);
    }



}
