using UnityEngine;

public class Rotate : MonoBehaviour
{
    // 每秒旋转的角度
    public float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        // 绕 Y 轴旋转
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
