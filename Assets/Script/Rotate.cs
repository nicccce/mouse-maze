using UnityEngine;

public class Rotate : MonoBehaviour
{
    // ÿ����ת�ĽǶ�
    public float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        // �� Y ����ת
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
