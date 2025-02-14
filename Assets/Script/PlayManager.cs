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
                    GameFinish("时间到！游戏结束");
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
                PauseButtonText.text = "继续游戏";
                break;
            case GameState.Pause:
                gameManager.currentState = GameState.Play;
                PauseButtonText.text = "暂停游戏";
                break;
        }
    }

    public void OnEndButtonClicked()
    {
        GameFinish("游戏结束");
    }

    private string ConvertToTimeFormat(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);  // 获取分钟数
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);  // 获取秒数

        // 使用字符串格式化，确保秒数和分钟数都是两位数（如 02:05）
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

}
