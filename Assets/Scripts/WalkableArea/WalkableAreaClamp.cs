using UnityEngine;

public class WalkableAreaClamp : MonoBehaviour
{
    [SerializeField] WalkableArea _area;

    public void SetArea(WalkableArea a) { _area = a; }

    void LateUpdate()
    {
        if (_area != null) transform.position = _area.ClampToBounds(transform.position);
    }
}
