using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    [SerializeField] EnemyConfig _config;
    float _hp;

    public EnemyConfig Config => _config;
    public float Hp => _hp;

    void Awake()
    {
        if (_config != null) _hp = _config.maxHp;
    }

    public void SetConfig(EnemyConfig config)
    {
        _config = config;
        if (config != null) _hp = config.maxHp;
    }

    public void TakeDamage(float amount)
    {
        _hp -= amount;
        if (_hp <= 0f) Kill();
    }

    void Kill()
    {
        var animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Dying");
        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;
        float delay = _config != null ? _config.deathAnimationDelay : 1.5f;
        Destroy(gameObject, delay);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_config == null) return;
        Vector3 pos = transform.position;
        const int segments = 48;
        float step = 2f * Mathf.PI / segments;
        // Красный — Aggression Radius
        float rAgg = _config.aggressionRadius;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * step;
            float a1 = (i + 1) * step;
            Vector3 p0 = pos + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * rAgg;
            Vector3 p1 = pos + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * rAgg;
            Gizmos.DrawLine(p0, p1);
        }
        // Сиреневый поверх — Attack Range
        float rAtk = _config.attackRange;
        Gizmos.color = new Color(0.7f, 0.3f, 1f, 0.6f);
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * step;
            float a1 = (i + 1) * step;
            Vector3 p0 = pos + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * rAtk;
            Vector3 p1 = pos + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * rAtk;
            Gizmos.DrawLine(p0, p1);
        }
        // Жёлтый поверх — Separation Radius
        float rSep = _config.separationRadius;
        if (rSep > 0f)
        {
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * step;
                float a1 = (i + 1) * step;
                Vector3 p0 = pos + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * rSep;
                Vector3 p1 = pos + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * rSep;
                Gizmos.DrawLine(p0, p1);
            }
        }
    }
#endif
}
