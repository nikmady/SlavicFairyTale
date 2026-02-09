using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyAI : MonoBehaviour
{
    enum Anim { Idle, Walking, Running }
    const string TrigIdle = "Idle";
    const string TrigWalking = "Walking";
    const string TrigRuning = "Runing";
    const string TrigAttack1 = "Attack_1";

    EnemyInstance _instance;
    Animator _animator;
    Transform _root;
    WalkableArea _walkableArea;
    Vector2 _homeCenter;
    float _homeRadius;
    Transform _player;
    IDamageable _playerDamageable;
    Vector2 _wanderTarget;
    float _attackCooldown;
    float _restTimer;
    bool _chasing;
    Vector2 _faceDir;
    Anim _lastAnim = Anim.Idle;
    bool _dealtDamageThisFrame;

    static readonly Collider2D[] _overlapBuffer = new Collider2D[16];
    static readonly RaycastHit2D[] _raycastBuffer = new RaycastHit2D[12];
    static readonly IComparer<RaycastHit2D> _raycastHitComparer = Comparer<RaycastHit2D>.Create((a, b) => a.distance.CompareTo(b.distance));

    public void SetSpawnArea(Vector2 center, float radius)
    {
        _homeCenter = center;
        _homeRadius = Mathf.Max(0.1f, radius);
        _wanderTarget = center + Random.insideUnitCircle * radius;
    }

    const float MinWanderDistance = 0.5f;
    static readonly int[] _avoidAngles = { -45, -25, -10, 10, 25, 45 };

    static Vector2 RotateDir(Vector2 d, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(d.x * c - d.y * s, d.x * s + d.y * c);
    }

    bool IsDirBlocked(Vector2 from, Vector2 dirNorm, float check, LayerMask mask)
    {
        if (mask != 0 && Physics2D.Raycast(from, dirNorm, check, mask)) return true;
        if (_walkableArea != null && _walkableArea.IsInObstacle(from + dirNorm * check)) return true;
        return false;
    }

    Vector2 GetAvoidanceDir(Vector2 from, Vector2 dir, float dist, LayerMask mask)
    {
        if (mask == 0 && _walkableArea == null) return dir.normalized;
        float check = dist + 0.25f;
        if (dir.sqrMagnitude < 0.0001f) return dir;
        Vector2 n = dir.normalized;
        if (!IsDirBlocked(from, n, check, mask)) return n;
        float bestDot = -1f;
        float bestDist = 0f;
        Vector2 best = n;
        for (int i = 0; i < _avoidAngles.Length; i++)
        {
            Vector2 d = RotateDir(n, _avoidAngles[i]).normalized;
            bool blocked = IsDirBlocked(from, d, check, mask);
            float dot = Vector2.Dot(d, n);
            float hitD = 0f;
            if (mask != 0) { RaycastHit2D hit = Physics2D.Raycast(from, d, check, mask); hitD = hit ? hit.distance : 0f; }
            if (!blocked)
            {
                if (dot > bestDot) { bestDot = dot; best = d; bestDist = check; }
            }
            else if (hitD > bestDist || (hitD >= bestDist - 0.01f && dot > bestDot))
            {
                bestDist = hitD;
                bestDot = dot;
                best = d;
            }
        }
        return best.normalized;
    }

    void PickNewWanderTarget(Vector2 currentPos, EnemyConfig c)
    {
        for (int i = 0; i < 8; i++)
        {
            _wanderTarget = _homeCenter + Random.insideUnitCircle * _homeRadius;
            if (Vector2.Distance(currentPos, _wanderTarget) >= MinWanderDistance)
                return;
        }
    }

    Vector2 GetSeparationOffset(Vector2 myPos)
    {
        var c = _instance != null ? _instance.Config : null;
        if (c == null) return Vector2.zero;
        float radius = c.separationRadius;
        float strength = c.separationStrength;
        if (strength <= 0f || radius <= 0f) return Vector2.zero;
        int layerMask = c.separationEnemyLayer != 0 ? c.separationEnemyLayer : LayerMask.GetMask("Enemy");
        int n = layerMask != 0
            ? Physics2D.OverlapCircleNonAlloc(myPos, radius, _overlapBuffer, layerMask)
            : Physics2D.OverlapCircleNonAlloc(myPos, radius, _overlapBuffer);
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < n; i++)
        {
            var col = _overlapBuffer[i];
            if (col == null) continue;
            Transform otherRoot = col.transform.root;
            if (otherRoot == _root) continue;
            if (layerMask == 0)
            {
                if (otherRoot.GetComponent<EnemyInstance>() == null) continue;
            }
            Vector2 otherPos = otherRoot.position;
            Vector2 delta = myPos - otherPos;
            float distSq = delta.sqrMagnitude;
            if (distSq < 0.0001f) continue;
            float dist = Mathf.Sqrt(distSq);
            float weight = 1f - (dist / radius);
            if (weight <= 0f) continue;
            sum += delta * (weight / distSq);
        }
        if (sum.sqrMagnitude > 1f) sum = sum.normalized;
        return sum * strength * Time.deltaTime;
    }

    void Awake()
    {
        _instance = GetComponent<EnemyInstance>();
        _animator = GetComponentInChildren<Animator>();
        _root = transform.root;
        var clamp = GetComponent<WalkableAreaClamp>();
        if (clamp == null) clamp = gameObject.AddComponent<WalkableAreaClamp>();
        _walkableArea = Object.FindObjectOfType<WalkableArea>();
        if (_walkableArea != null) clamp.SetArea(_walkableArea);
        var go = GameObject.FindWithTag("Player");
        if (go != null)
        {
            _player = go.transform;
            _playerDamageable = go.GetComponent<IDamageable>();
        }
    }

    void Update()
    {
        EnemyConfig c = _instance != null ? _instance.Config : null;
        if (c == null) return;
        _dealtDamageThisFrame = false;
        Vector2 pos = transform.position;
        float aggR = c.aggressionRadius;
        float moveSpeed = c.moveSpeed * Time.deltaTime;
        bool seePlayer = false;
        if (_player != null && aggR > 0f)
        {
            float distToPlayer = Vector2.Distance(pos, _player.position);
            if (distToPlayer <= aggR)
            {
                Vector2 dirToPlayer = (Vector2)_player.position - pos;
                int hitCount = Physics2D.RaycastNonAlloc(pos, dirToPlayer.normalized, _raycastBuffer, distToPlayer);
                System.Array.Sort(_raycastBuffer, 0, hitCount, _raycastHitComparer);
                for (int i = 0; i < hitCount; i++)
                {
                    var hit = _raycastBuffer[i];
                    if (hit.collider == null) continue;
                    if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;
                    if (hit.collider.transform == _player) seePlayer = true;
                    break;
                }
            }
        }
        if (seePlayer)
        {
            _chasing = true;
            float dist = Vector2.Distance(pos, _player.position);
            if (dist <= c.attackRange)
            {
                _faceDir = ((Vector2)_player.position - pos).normalized;
                if (_attackCooldown <= 0f)
                {
                    if (_playerDamageable != null)
                    {
                        _playerDamageable.TakeDamage(c.attackDamage);
                        _dealtDamageThisFrame = true;
                    }
                    _attackCooldown = c.attackInterval;
                }
                _attackCooldown -= Time.deltaTime;
            }
            else
            {
                Vector2 dir = ((Vector2)_player.position - pos).normalized;
                dir = GetAvoidanceDir(pos, dir, c.runSpeed * Time.deltaTime, c.movementObstacleLayer);
                _faceDir = dir;
                float runSpeedDt = c.runSpeed * Time.deltaTime;
                pos += dir * runSpeedDt;
                pos += GetSeparationOffset(pos);
                transform.position = pos;
            }
        }
        else
        {
            _chasing = false;
            float distToHome = Vector2.Distance(pos, _homeCenter);
            if (distToHome > _homeRadius)
            {
                Vector2 toHome = (_homeCenter - pos).normalized;
                toHome = GetAvoidanceDir(pos, toHome, moveSpeed, c.movementObstacleLayer);
                _faceDir = toHome;
                pos += toHome * moveSpeed;
                pos += GetSeparationOffset(pos);
                transform.position = pos;
            }
            else
            {
                if (_restTimer > 0f)
                {
                    _restTimer -= Time.deltaTime;
                    pos += GetSeparationOffset(pos);
                    pos = _homeCenter + Vector2.ClampMagnitude(pos - _homeCenter, _homeRadius);
                    transform.position = pos;
                }
                else
                {
                    float distToWander = Vector2.Distance(pos, _wanderTarget);
                    bool atTarget = distToWander < 0.15f;
                    if (atTarget)
                    {
                        if (c.restDuration > 0f && Random.value < c.restChance)
                            _restTimer = c.restDuration;
                        else
                            PickNewWanderTarget(pos, c);
                    }
                    Vector2 toWander = (_wanderTarget - pos).normalized;
                    toWander = GetAvoidanceDir(pos, toWander, moveSpeed, c.movementObstacleLayer);
                    _faceDir = toWander;
                    pos += toWander * moveSpeed;
                    pos = _homeCenter + Vector2.ClampMagnitude(pos - _homeCenter, _homeRadius);
                    pos += GetSeparationOffset(pos);
                    transform.position = pos;
                }
            }
        }
        // Стоим в зоне атаки — всё равно применяем разведение, чтобы не наслаиваться
        if (seePlayer && Vector2.Distance(pos, _player.position) <= c.attackRange)
        {
            Vector2 sep = GetSeparationOffset(pos);
            if (sep.sqrMagnitude > 0.0001f)
                transform.position = pos + sep;
        }
        if (_faceDir.sqrMagnitude > 0.01f)
        {
            Vector3 s = transform.localScale;
            float ax = Mathf.Abs(s.x);
            if (_faceDir.x < 0f) s.x = -ax; else s.x = ax;
            transform.localScale = s;
        }
        Vector2 posFinal = transform.position;
        float distToPlayerFinal = _player != null ? Vector2.Distance(posFinal, _player.position) : float.MaxValue;
        Anim state = Anim.Idle;
        if (seePlayer)
            state = distToPlayerFinal <= c.attackRange ? Anim.Idle : Anim.Running;
        else if (_restTimer > 0f)
            state = Anim.Idle;
        else
            state = Anim.Walking;
        if (state != _lastAnim && _animator != null)
        {
            _lastAnim = state;
            if (state == Anim.Idle) _animator.SetTrigger(TrigIdle);
            else if (state == Anim.Walking) _animator.SetTrigger(TrigWalking);
            else _animator.SetTrigger(TrigRuning);
        }
        if (_dealtDamageThisFrame && _animator != null)
            _animator.SetTrigger(TrigAttack1);
    }
}
