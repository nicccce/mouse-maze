using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public Text timeBoard;
    public GameManager gameManager;
    public MapManager mapManager;
    public Text PauseButtonText;
    public GameObject GameEndPanel;
    public Text GameEndPanelText;

    public Camera mouseCamera;
    public GameObject winCamera;

    public GameObject mouse;

    private MazeData maze;

    private (int, int) mousePosition;

    private Transform mouseTransform;
    private Transform armature;
    private float timer = 0;


    void Start()
    {
        mouseTransform = mouse.GetComponent<Transform>();
        armature = mouseTransform.Find("Armature"); // 查找名为 Armature 的子对象
        animationComponent = mouseTransform.GetComponent<Animation>(); // 获取 Animation 组件
        mouseTransform.rotation = Quaternion.LookRotation(Vector3.forward);
        mouseTransform.Rotate(Vector3.right * -90);
    }

    public void Initialize()
    {
        mouseTransform.rotation = Quaternion.LookRotation(Vector3.forward);
        mouseTransform.Rotate(Vector3.right * -90);
        maze = mapManager.GetCurrentMazeData();
        mousePosition = maze.start;
        mouseTransform.position = getWorldPositonFromMousePosition(mousePosition);
    }

    void Update()
    {
        switch (gameManager.currentState)
        {
            case GameState.Play:
                Move();
                timer += Time.deltaTime;
                if (timer > 900)
                {
                    GameFinish("时间到！游戏结束");
                }
                timeBoard.text = ConvertToTimeFormat(timer);
                if (IsInToleranceRange(mousePosition, mapManager.GetCurrentMazeData().end))
                {
                    gameManager.currentState = GameState.Win;
                    winCamera.SetActive(true);
                }
                break;
            case GameState.Win:
                if (Input.GetMouseButtonDown(0))
                {
                    winCamera.SetActive(false);
                    gameManager.currentState = GameState.Default;
                    gameManager.ResetPanel();
                    mouseCamera.depth = -1;
                }
                break;
        }
    }

    // 判断 mousePosition 是否在 endPosition 的正负 0.25 范围内
    private bool IsInToleranceRange((int, int) mousePosition, (int, int) endPosition)
    {
        float tolerance = 0.25f; // 容错范围

        // 计算 x 和 y 方向上的差值
        float deltaX = Mathf.Abs(mousePosition.Item1 - endPosition.Item1);
        float deltaY = Mathf.Abs(mousePosition.Item2 - endPosition.Item2);

        // 判断两者是否在容错范围内
        return deltaX <= tolerance && deltaY <= tolerance;
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

    private Vector3 getWorldPositonFromMousePosition((int,int) pos)
    {
        return mapManager.GridToWorldPosition(pos.Item1, pos.Item2, mapManager.gridSize)+Vector3.down*0.5f;
    }

    private bool isMoving = false; // 是否正在移动
    private float moveSpeed = 0.2f; // 移动速度，0.1秒移动到新位置
    private Animation animationComponent; // 存储 Animation 组件引用


    private void Move()
    {
        
        if (isMoving) return; // 如果正在移动，直接返回，防止重复移动

        // 获取 WASD 键的输入
        float horizontal = 0;
        float vertical = 0;

        // 检测 WASD 按键，W 对应上，S 对应下，A 对应左，D 对应右
        if (Input.GetKey(KeyCode.W)) horizontal = 1;
        if (Input.GetKey(KeyCode.S)) horizontal = -1;
        if (Input.GetKey(KeyCode.A)) vertical = -1;
        if (Input.GetKey(KeyCode.D)) vertical = 1;

        Vector3 dir = Vector3.zero;
        (int, int) tempPosition = mousePosition; // 记录当前的位置

        // 根据水平输入 (A/D)
        if (horizontal != 0)
        {
            dir = new Vector3(0, 0, horizontal > 0 ? 1 : -1);
            tempPosition.Item2 += (horizontal > 0 ? 1 : -1); // 计算新的坐标
        }
        // 根据垂直输入 (W/S)
        else if (vertical != 0)
        {
            dir = new Vector3(vertical > 0 ? 1 : -1, 0, 0);
            tempPosition.Item1 += (vertical > 0 ? 1 : -1); // 计算新的坐标
        }else
        {
            // 移动结束，停止动画
            animationComponent.Stop(); // 停止动画（如果需要）
            animationComponent.Play("wait"); // 播放"wait"动画
        }

        // 如果按键被按下，且目标位置在迷宫内且可通行
        if (dir != Vector3.zero && maze.IsInBounds(tempPosition) && !maze.GetCell(tempPosition.Item1, tempPosition.Item2))
        {
            mouseTransform.rotation = Quaternion.LookRotation(dir)*Quaternion.Euler(Vector3.down * 120f); // 物体面朝方向


            // 开始移动
            isMoving = true;
            animationComponent.Play("run"); // 播放"Run"动画
            StartCoroutine(MoveToPosition(tempPosition)); // 使用协程实现平滑移动
        }
    }

    // 协程实现平滑移动
    private IEnumerator MoveToPosition((int, int) targetPosition)
    {
        Vector3 startPosition = mouseTransform.position; // 当前的位置
        Vector3 endPosition = getWorldPositonFromMousePosition(targetPosition); // 目标位置
        float journeyLength = Vector3.Distance(startPosition, endPosition); // 计算距离
        float startTime = Time.time; // 记录开始时间

        while (Time.time - startTime < moveSpeed)
        {
            // 计算经过的时间与总时间的比例
            float distanceCovered = (Time.time - startTime) * (journeyLength / moveSpeed);
            float fractionOfJourney = distanceCovered / journeyLength;

            // 更新物体位置
            mouseTransform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
            yield return null; // 等待下一帧
        }

        // 确保最后到达目标位置
        mouseTransform.position = endPosition;
        mousePosition = targetPosition; // 更新当前迷宫坐标

        isMoving = false; // 允许再次移动
    }

}
