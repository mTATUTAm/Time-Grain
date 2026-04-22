using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 moveDirection = Vector3.up;

    private Transform _ridingPlayer = null;
    private bool _shouldDetach = false;

    void Update()
    {
        float dt = TimeManager.Instance.BoardDeltaTime;
        transform.Translate(moveDirection * speed * dt);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    _ridingPlayer = collision.transform;
                    _shouldDetach = false;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _shouldDetach = true;
        }
    }

    void LateUpdate()
    {
        if (_shouldDetach && _ridingPlayer != null)
        {
            _ridingPlayer.SetParent(null);
            _ridingPlayer = null;
            _shouldDetach = false;
        }
        else if (_ridingPlayer != null && _ridingPlayer.parent != transform)
        {
            _ridingPlayer.SetParent(transform);
        }
    }
}