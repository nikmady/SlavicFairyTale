using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Stats")]
    public string displayName = "Enemy";
    public float maxHp = 100f;
    public float maxMana = 0f;

    [Header("AI")]
    public float moveSpeed = 2f;
    public float runSpeed = 5f;
    public float aggressionRadius = 5f;
    public float attackRange = 0.5f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;
    [Tooltip("Радиус отталкивания от других врагов (0 = выкл). Виден в превью и в сцене.")]
    public float separationRadius = 1f;
    [Tooltip("Сила отталкивания (0 = выкл).")]
    public float separationStrength = 1f;
    [Tooltip("Слой врагов для разведения. По умолчанию (Nothing) используется слой Enemy.")]
    public LayerMask separationEnemyLayer = 0;
    [Tooltip("Длительность передышки в секундах (0 = без передышки).")]
    public float restDuration = 2f;
    [Tooltip("Вероятность встать на передышку, добравшись до точки блуждания (0..1).")]
    [Range(0f, 1f)]
    public float restChance = 0.4f;

    [Header("Reward (on kill)")]
    public int money;
    public int exp;
    public float deathAnimationDelay = 1.5f;
}
