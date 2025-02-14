using UnityEngine;

public class FlollwiPlayerCamera : MonoBehaviour
{
    public GameObject Player;  //������Ҫ��������
    private Vector3 offset;   //��ֵ
    private Transform playerTransform;  //������ҵ�Transform���  
    private Transform cameraTransform;  //���������Transform��� 
    public float distance = 0;
    public float scrollSpeed = 10;
    private Camera thisCamera;

    void Start()
    {
        playerTransform = Player.GetComponent<Transform>(); //�õ���ҵ�Transform���
        cameraTransform = this.GetComponent<Transform>();   //�õ������Transform���
        thisCamera = cameraTransform.GetComponent<Camera>();
        offset = cameraTransform.position - playerTransform.position;  //�õ���������λ�õĲ�ֵ
    }

    void Update()
    {
        if (thisCamera.depth == 1) {
            this.transform.position = playerTransform.position + offset;  //��ҵ�λ�ü��ϲ�ֵ��ֵ�������λ��
            ScrollView();
        }
    }
    void ScrollView()
    {
        distance = offset.magnitude;
        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;  //��ǰ��������ֵ
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
