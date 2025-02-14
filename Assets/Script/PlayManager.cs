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
        armature = mouseTransform.Find("Armature"); // ������Ϊ Armature ���Ӷ���
        animationComponent = mouseTransform.GetComponent<Animation>(); // ��ȡ Animation ���
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
                    GameFinish("ʱ�䵽����Ϸ����");
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

    // �ж� mousePosition �Ƿ��� endPosition ������ 0.25 ��Χ��
    private bool IsInToleranceRange((int, int) mousePosition, (int, int) endPosition)
    {
        float tolerance = 0.25f; // �ݴ�Χ

        // ���� x �� y �����ϵĲ�ֵ
        float deltaX = Mathf.Abs(mousePosition.Item1 - endPosition.Item1);
        float deltaY = Mathf.Abs(mousePosition.Item2 - endPosition.Item2);

        // �ж������Ƿ����ݴ�Χ��
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

    private Vector3 getWorldPositonFromMousePosition((int,int) pos)
    {
        return mapManager.GridToWorldPosition(pos.Item1, pos.Item2, mapManager.gridSize)+Vector3.down*0.5f;
    }

    private bool isMoving = false; // �Ƿ������ƶ�
    private float moveSpeed = 0.2f; // �ƶ��ٶȣ�0.1���ƶ�����λ��
    private Animation animationComponent; // �洢 Animation �������


    private void Move()
    {
        
        if (isMoving) return; // ��������ƶ���ֱ�ӷ��أ���ֹ�ظ��ƶ�

        // ��ȡ WASD ��������
        float horizontal = 0;
        float vertical = 0;

        // ��� WASD ������W ��Ӧ�ϣ�S ��Ӧ�£�A ��Ӧ��D ��Ӧ��
        if (Input.GetKey(KeyCode.W)) horizontal = 1;
        if (Input.GetKey(KeyCode.S)) horizontal = -1;
        if (Input.GetKey(KeyCode.A)) vertical = -1;
        if (Input.GetKey(KeyCode.D)) vertical = 1;

        Vector3 dir = Vector3.zero;
        (int, int) tempPosition = mousePosition; // ��¼��ǰ��λ��

        // ����ˮƽ���� (A/D)
        if (horizontal != 0)
        {
            dir = new Vector3(0, 0, horizontal > 0 ? 1 : -1);
            tempPosition.Item2 += (horizontal > 0 ? 1 : -1); // �����µ�����
        }
        // ���ݴ�ֱ���� (W/S)
        else if (vertical != 0)
        {
            dir = new Vector3(vertical > 0 ? 1 : -1, 0, 0);
            tempPosition.Item1 += (vertical > 0 ? 1 : -1); // �����µ�����
        }else
        {
            // �ƶ�������ֹͣ����
            animationComponent.Stop(); // ֹͣ�����������Ҫ��
            animationComponent.Play("wait"); // ����"wait"����
        }

        // ������������£���Ŀ��λ�����Թ����ҿ�ͨ��
        if (dir != Vector3.zero && maze.IsInBounds(tempPosition) && !maze.GetCell(tempPosition.Item1, tempPosition.Item2))
        {
            mouseTransform.rotation = Quaternion.LookRotation(dir)*Quaternion.Euler(Vector3.down * 120f); // �����泯����


            // ��ʼ�ƶ�
            isMoving = true;
            animationComponent.Play("run"); // ����"Run"����
            StartCoroutine(MoveToPosition(tempPosition)); // ʹ��Э��ʵ��ƽ���ƶ�
        }
    }

    // Э��ʵ��ƽ���ƶ�
    private IEnumerator MoveToPosition((int, int) targetPosition)
    {
        Vector3 startPosition = mouseTransform.position; // ��ǰ��λ��
        Vector3 endPosition = getWorldPositonFromMousePosition(targetPosition); // Ŀ��λ��
        float journeyLength = Vector3.Distance(startPosition, endPosition); // �������
        float startTime = Time.time; // ��¼��ʼʱ��

        while (Time.time - startTime < moveSpeed)
        {
            // ���㾭����ʱ������ʱ��ı���
            float distanceCovered = (Time.time - startTime) * (journeyLength / moveSpeed);
            float fractionOfJourney = distanceCovered / journeyLength;

            // ��������λ��
            mouseTransform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
            yield return null; // �ȴ���һ֡
        }

        // ȷ����󵽴�Ŀ��λ��
        mouseTransform.position = endPosition;
        mousePosition = targetPosition; // ���µ�ǰ�Թ�����

        isMoving = false; // �����ٴ��ƶ�
    }

}
