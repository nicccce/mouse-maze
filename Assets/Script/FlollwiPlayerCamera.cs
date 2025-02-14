using UnityEngine;

public class FlollwiPlayerCamera : MonoBehaviour
{
    public GameObject Player;  //声明需要跟随的玩家
    private Vector3 offset;   //差值
    private Transform playerTransform;  //声明玩家的Transform组件  
    private Transform cameraTransform;  //声明相机的Transform组件 
    public float distance = 0;
    public float scrollSpeed = 10;
    private Camera thisCamera;

    void Start()
    {
        playerTransform = Player.GetComponent<Transform>(); //得到玩家的Transform组件
        cameraTransform = this.GetComponent<Transform>();   //得到相机的Transform组件
        thisCamera = cameraTransform.GetComponent<Camera>();
        offset = cameraTransform.position - playerTransform.position;  //得到相机和玩家位置的差值
    }

    void Update()
    {
        if (thisCamera.depth == 1) {
            this.transform.position = playerTransform.position + offset;  //玩家的位置加上差值赋值给相机的位置
            ScrollView();
        }
    }
    void ScrollView()
    {
        distance = offset.magnitude;
        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;  //往前滑动是正值
        if (distance > 8)
        {
            distance = 8;
        }
        if (distance < 2)
        {
            distance = 2;
        }
        offset = offset.normalized * distance;
    }
}
