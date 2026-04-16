using UnityEngine;

public class MoveObject : MonoBehaviour
{
    
    public float speed = 5.0f;

    void Update()
    {
        // 右方向（Vector3.right = 1,0,0）に、スピードと時間をかけて移動
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
