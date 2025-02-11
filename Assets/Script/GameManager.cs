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
        ClearPanel();
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
        ClearPanel();
        ManagePanel.SetActive(true);
        mapManager.RefreshMazeManagePanel();
        ModText.text = "�����Թ�";
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
