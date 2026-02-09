using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp = 100f;

    public void TakeDamage(float amount)
    {
        _hp -= amount;
        if (_hp <= 0f) Destroy(gameObject);
    }
}
