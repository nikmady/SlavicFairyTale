using UnityEngine;

public class WalkableAreaClamp : MonoBehaviour
{
    [SerializeField] WalkableArea _area;

    void LateUpdate()
    {
        if (_area != null) transform.position = _area.ClampToBounds(transform.position);
    }
}
