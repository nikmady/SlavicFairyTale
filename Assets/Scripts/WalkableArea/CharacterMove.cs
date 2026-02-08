using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    [SerializeField] float _speed = 5f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h == 0f && v == 0f) return;
        Vector3 d = new Vector3(h, v, 0f).normalized * (_speed * Time.deltaTime);
        transform.position += d;
    }
}
