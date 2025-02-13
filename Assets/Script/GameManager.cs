using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

// ��Ϸ״̬ö��
public enum GameState
{
    Default,    // ϷĬ�Ͻ���
    Play,       // ��Ϸ������
    Pause,      // ��Ϸ��ͣ
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
        // ����Ƿ��� F11 ��
        if (Input.GetKeyDown(KeyCode.F11))
        {
            // ����UI��ȫ����ȡ��ȫ��
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
        ModText.text = "��Ϸ��";
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
        ModText.text = "�����Թ�";
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
        ModText.text = "�༭�Թ�";
        currentState = GameState.Edit;
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

    void ResetPanel()
    {
        PlayPanel.SetActive(false);
        ManagePanel.SetActive(false);
        EditPanel.SetActive(false);
        mapManager.ResetMap();
    }

}
