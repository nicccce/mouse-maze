using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public Text timeBoard;
    public GameManager gameManager;
    public Text PauseButtonText;
    public GameObject GameEndPanel;
    public Text GameEndPanelText;

    public Camera mouseCamera;

    private float timer = 0;

    void Start()
    {
    }

    void Update()
    {
        switch (gameManager.currentState)
        {
            case GameState.Play:
                timer += Time.deltaTime;
                if (timer > 900)
                {
                    GameFinish("ʱ�䵽����Ϸ����");
                }
                timeBoard.text = ConvertToTimeFormat(timer);
                break;
        }
    }

    public void OnChangeButtonClicked()
    {
        if (gameManager.currentState == GameState.Play || gameManager.currentState == GameState.Pause)
        {
            mouseCamera.depth = -mouseCamera.depth;
        }
    }

    void GameFinish(string text)
    {
        timer = 0;
        gameManager.currentState = GameState.Default;
        gameManager.ResetPanel();
        GameEndPanelText.text = text;
        GameEndPanel.SetActive(true);
    }

    public void OnPauseButtonClicked()
    {
        switch (gameManager.currentState)
        {
            case GameState.Play:
                gameManager.currentState = GameState.Pause;
                PauseButtonText.text = "������Ϸ";
                break;
            case GameState.Pause:
                gameManager.currentState = GameState.Play;
                PauseButtonText.text = "��ͣ��Ϸ";
                break;
        }
    }

    public void OnEndButtonClicked()
    {
        GameFinish("��Ϸ����");
    }

    private string ConvertToTimeFormat(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);  // ��ȡ������
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);  // ��ȡ����

        // ʹ���ַ�����ʽ����ȷ�������ͷ�����������λ������ 02:05��
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

}
