using OmniVehicleAi;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarTurretAI))]
[RequireComponent(typeof(AIVehicleController))]
[RequireComponent(typeof(Health))]
public class CarCombatFSM : MonoBehaviour
{
    public enum State { Patrolling, Looting, Approaching, Attacking, KillTarget, Retreating, Healing, Dying }
    State currentState = State.Patrolling;

    // ------------- Difficulty Config -------------
    public enum Difficulty { Easy, Normal, Hard }
    public Difficulty difficulty = Difficulty.Normal;

    [Header("Detection Ranges")]
    public float detectionRangeEasy = 20f;
    public float detectionRangeNormal = 40f;
    public float detectionRangeHard = 60f;

    [Header("Reaction & Decision")]
    public float reactionDelayEasy = 1.5f;
    public float reactionDelayNormal = 0.8f;
    public float reactionDelayHard = 0.2f;
    public float decisionIntervalEasy = 3f;
    public float decisionIntervalNormal = 2f;
    public float decisionIntervalHard = 1f;

    [Header("Combat")]
    [Range(0, 100)]
    public float missedRateEasy = 30f;
    [Range(0, 100)]
    public float missedRateNormal = 20f;
    [Range(0, 100)]
    public float missedRateHard = 5f;
    public float abilityCooldownEasy = 5f;
    public float abilityCooldownNormal = 3f;
    public float abilityCooldownHard = 1f;

    [Header("Retreat & Healing")]
    [Range(0, 1)]
    public float retreatThresholdEasy = 0.2f;
    [Range(0, 1)]
    public float retreatThresholdNormal = 0.3f;
    [Range(0, 1)]
    public float retreatThresholdHard = 0.5f;
    public float healingDuration = 3f;

    [Header("Patrol & Loot")]
    public float patrolRadius = 50f;
    public float lootSearchRange = 30f;

    // ------------- Runtime vars -------------
    CarTurretAI turret;
    AIVehicleController vehicleAI;
    Health health;

    Transform combatTarget;
    Transform lootTarget;

    float detectionRange;
    float reactionDelay;
    float decisionInterval;
    float missedRate;
    float abilityCooldown;
    float retreatThreshold;

    float lastDecisionTime;
    float lastAttackTime;

    float hpPercent => health.currentHealth / (float)health.maxHealth;

    void Awake()
    {
        turret = GetComponent<CarTurretAI>();
        vehicleAI = GetComponent<AIVehicleController>();
        health = GetComponent<Health>();
        ApplyDifficultyConfig();
        TransitionTo(State.Patrolling);
    }

    void ApplyDifficultyConfig()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                detectionRange = detectionRangeEasy;
                reactionDelay = reactionDelayEasy;
                decisionInterval = decisionIntervalEasy;
                missedRate = missedRateEasy;
                abilityCooldown = abilityCooldownEasy;
                retreatThreshold = retreatThresholdEasy;
                break;
            case Difficulty.Normal:
                detectionRange = detectionRangeNormal;
                reactionDelay = reactionDelayNormal;
                decisionInterval = decisionIntervalNormal;
                missedRate = missedRateNormal;
                abilityCooldown = abilityCooldownNormal;
                retreatThreshold = retreatThresholdNormal;
                break;
            case Difficulty.Hard:
                detectionRange = detectionRangeHard;
                reactionDelay = reactionDelayHard;
                decisionInterval = decisionIntervalHard;
                missedRate = missedRateHard;
                abilityCooldown = abilityCooldownHard;
                retreatThreshold = retreatThresholdHard;
                break;
        }
    }

    void Update()
    {
        if (Time.time - lastDecisionTime < decisionInterval) return;
        lastDecisionTime = Time.time;

        if (hpPercent <= 0f)
        {
            TransitionTo(State.Dying);
            return;
        }

        // ưu tiên đánh giá theo state
        switch (currentState)
        {
            case State.Patrolling: EvaluatePatrolling(); break;
            case State.Looting: EvaluateLooting(); break;
            case State.Approaching: EvaluateApproaching(); break;
            case State.Attacking: EvaluateAttacking(); break;
            case State.Retreating: EvaluateRetreating(); break;
            case State.Healing: EvaluateHealing(); break;
                // KillTarget và Dying do coroutine xử lý
        }
    }

    void TransitionTo(State next)
    {
        StopAllCoroutines();
        currentState = next;
        switch (next)
        {
            case State.Patrolling: StartCoroutine(CoroutinePatrolling()); break;
            case State.Looting: StartCoroutine(CoroutineLooting()); break;
            case State.Approaching: StartCoroutine(CoroutineApproaching()); break;
            case State.Attacking: StartCoroutine(CoroutineAttacking()); break;
            case State.KillTarget: StartCoroutine(CoroutineKillTarget()); break;
            case State.Retreating: StartCoroutine(CoroutineRetreating()); break;
            case State.Healing: StartCoroutine(CoroutineHealing()); break;
            case State.Dying: StartCoroutine(CoroutineDying()); break;
        }
    }

    #region Evaluators

    void EvaluatePatrolling()
    {
        // nếu thấy combat target
        Transform detected = turret.target;
        if (detected != null &&
            Vector3.Distance(transform.position, detected.position) <= detectionRange &&
            HasClearShot(detected))
        {
            combatTarget = detected;
            TransitionTo(State.Approaching);
            return;
        }

        // nếu tìm được loot và không có combat target
        if (FindNearestLoot(out lootTarget))
        {
            TransitionTo(State.Looting);
        }
    }

    void EvaluateLooting()
    {
        // ưu tiên combat nếu xuất hiện target
        Transform detected = turret.target;
        if (detected != null &&
            Vector3.Distance(transform.position, detected.position) <= detectionRange &&
            HasClearShot(detected))
        {
            combatTarget = detected;
            TransitionTo(State.Approaching);
            return;
        }
        // nếu HP xuống thấp khi đang loot
        if (hpPercent < retreatThreshold)
        {
            TransitionTo(State.Retreating);
            return;
        }
        // nếu không còn loot gần
        if (!FindNearestLoot(out lootTarget))
        {
            TransitionTo(State.Patrolling);
        }
    }

    void EvaluateApproaching()
    {
        float d = Vector3.Distance(transform.position, combatTarget.position);
        if (d <= turret.shootingRange && HasClearShot(combatTarget))
        {
            TransitionTo(State.Attacking);
            return;
        }
        if (d > detectionRange || !CanSee(combatTarget))
        {
            TransitionTo(State.Patrolling);
        }
    }

    void EvaluateAttacking()
    {
        // target chết
        var targetHealth = combatTarget.GetComponent<Health>();
        if (targetHealth != null && targetHealth.currentHealth <= 0)
        {
            TransitionTo(State.KillTarget);
            return;
        }
        // HP bot thấp
        if (hpPercent < retreatThreshold)
        {
            TransitionTo(State.Retreating);
            return;
        }
        // target chạy xa
        if (Vector3.Distance(transform.position, combatTarget.position) > turret.shootingRange)
        {
            TransitionTo(State.Approaching);
        }
    }

    void EvaluateRetreating()
    {
        // nếu HP xuống 0
        if (hpPercent <= 0f)
        {
            TransitionTo(State.Dying);
            return;
        }
        // nếu đã chạy đủ xa và an toàn
        if (hpPercent >= retreatThreshold)
        {
            TransitionTo(State.Healing);
        }
    }

    void EvaluateHealing()
    {
        // chờ coroutine healing tự chuyển về Patrolling
    }

    #endregion

    #region Coroutines

    IEnumerator CoroutinePatrolling()
    {
        while (currentState == State.Patrolling)
        {
            Vector3 roam = GetRandomPatrolPoint();
            vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
            vehicleAI.target = CreateDummyTransform(roam);
            yield return new WaitUntil(() =>
                Vector3.Distance(transform.position, roam) < 3f);
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    IEnumerator CoroutineLooting()
    {
        while (currentState == State.Looting)
        {
            vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
            vehicleAI.target = lootTarget;
            // di chuyển tới item
            yield return new WaitUntil(() =>
                Vector3.Distance(transform.position, lootTarget.position) < 2f);
            // TODO: nhặt lootTarget (cập nhật HP, đạn, v.v.)
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CoroutineApproaching()
    {
        yield return new WaitForSeconds(reactionDelay);
        while (currentState == State.Approaching)
        {
            vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
            vehicleAI.target = combatTarget;
            yield return null;
        }
    }

    IEnumerator CoroutineAttacking()
    {
        yield return new WaitForSeconds(reactionDelay);
        while (currentState == State.Attacking)
        {
            if (Time.time - lastAttackTime >= abilityCooldown)
            {
                lastAttackTime = Time.time;
                if (Random.Range(0f, 100f) >= missedRate)
                    turret.TryShoot(combatTarget);
            }
            yield return null;
        }
    }

    IEnumerator CoroutineKillTarget()
    {
        yield return new WaitForSeconds(1f);
        if (FindNearestEnemy(out combatTarget))
            TransitionTo(State.Approaching);
        else
            TransitionTo(State.Patrolling);
    }

    IEnumerator CoroutineRetreating()
    {
        // TODO: bật nitro nếu có
        Vector3 dir = GetSafeDirection();
        vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
        vehicleAI.target = CreateDummyTransform(transform.position + dir * 50f);
        while (currentState == State.Retreating)
            yield return null;
    }

    IEnumerator CoroutineHealing()
    {
        yield return new WaitForSeconds(healingDuration);
        TransitionTo(State.Patrolling);
    }

    IEnumerator CoroutineDying()
    {
        // TODO: hiệu ứng nổ
        Destroy(gameObject);
        yield break;
    }

    #endregion

    #region Helpers

    bool FindNearestLoot(out Transform loot)
    {
        loot = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, lootSearchRange, LayerMask.GetMask("Loot"));
        float best = lootSearchRange;
        foreach (var col in hits)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, dist))
                if (hit.collider != col) continue;
            if (dist < best)
            {
                best = dist;
                loot = col.transform;
            }
        }
        return loot != null;
    }

    bool FindNearestEnemy(out Transform enemy)
    {
        enemy = null;
        // TODO: tìm bot/zombie khác
        return false;
    }

    bool CanSee(Transform t)
    {
        Vector3 dir = (t.position - turret.firePoint.position).normalized;
        if (Physics.Raycast(turret.firePoint.position, dir, out RaycastHit hit, detectionRange))
            return hit.transform == t;
        return false;
    }

    bool HasClearShot(Transform t) => CanSee(t);

    Vector3 GetRandomPatrolPoint()
    {
        Vector2 c = Random.insideUnitCircle * patrolRadius;
        return transform.position + new Vector3(c.x, 0, c.y);
    }

    Vector3 GetSafeDirection()
    {
        // TODO: tính hướng trốn tránh
        return -transform.forward;
    }

    Transform CreateDummyTransform(Vector3 pos)
    {
        GameObject go = new GameObject("TempTarget");
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.position = pos;
        return go.transform;
    }

    #endregion
}
