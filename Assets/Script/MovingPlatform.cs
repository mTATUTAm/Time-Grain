// MovingPlatform.cs
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 moveDirection = Vector3.up;

    void Update()
    {
        // BoardDeltaTime
        // 逆行中は自動的にマイナスになるので逆方向に動く
        float dt = TimeManager.Instance.BoardDeltaTime;
        transform.Translate(moveDirection * speed * dt);
    }
}