using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarStats), typeof(Health))]
public class CarTurretAI : MonoBehaviour
{
    public enum Team { Team1, Team2 }

    [Header("Team Settings")]
    public Team team;

    [Header("Turret Setup")]
    public Transform turretPivot;
    public Transform firePoint;
    public HomingProjectile projectilePrefab;
    public ParticleSystem muzzleSparkPrefab;

    [Header("Combat Settings")]
    public float detectionRange = 50f;
    public float rotationSpeed = 120f;
    public float fireRate = 2f;
    public float shootingRange = 50f;
    public LayerMask zombieLayer;

    [Header("Buff Settings")]
    public float buffAmount = 10f;
    public float buffDuration = 10f;

    CarStats stats;
    Health health;
   public Transform target;
    float lastFireTime;

    [Header("Accuracy")]
    [Range(0, 100)]
    public float defaultMissRate = 20f;    // nếu không có CarCombatFSM thì dùng giá trị này

    CarCombatFSM combatFSM;
    void Awake()
    {
        combatFSM = GetComponent<CarCombatFSM>();
        stats = GetComponent<CarStats>();
        health = GetComponent<Health>();
        // Khi chết, disable ngay CarTurretAI
        health.onDie += () => enabled = false;
    }

    void Start()
    {
        projectilePrefab.CreatePool(10);
        muzzleSparkPrefab.CreatePool(3);
    }

    void OnDisable()
    {
        DOTween.Kill(gameObject.GetInstanceID());
    }

    void Update()
    {
        if (!enabled) return;

        target = GetPriorityTarget();

        // Bỏ qua nếu không có target hoặc nó đã inactive
        if (target == null || !target.gameObject.activeInHierarchy) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= shootingRange)
        {
            AimAt(target.position);
            TryShoot(target);
        }
    }

    Transform GetPriorityTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        Transform bestZombie = null;
        float bestDist = Mathf.Infinity;
        var seen = new HashSet<CarTurretAI>();

        foreach (var col in hits)
        {
            if (col.transform.IsChildOf(transform)) continue;

            // Ưu tiên AI đối phương
            var ai = col.GetComponentInParent<CarTurretAI>();
            if (ai != null && ai.team != team && !seen.Contains(ai))
            {
                seen.Add(ai);
                // Kiểm raycast full collider
                var dirA = (ai.transform.position - turretPivot.position).normalized;
                float dA = Vector3.Distance(turretPivot.position, ai.transform.position);
                if (Physics.Raycast(turretPivot.position, dirA, out RaycastHit hA, dA)
                    && hA.transform != ai.transform)
                    continue;
                return ai.transform;
            }

            // Thử zombie
            if ((zombieLayer.value & (1 << col.gameObject.layer)) != 0)
            {
                float dZ = Vector3.Distance(transform.position, col.transform.position);
                var dirZ = (col.transform.position - turretPivot.position).normalized;
                if (dZ < bestDist)
                {
                    if (Physics.Raycast(turretPivot.position, dirZ, out RaycastHit hZ, dZ)
                        && hZ.transform != col.transform)
                        continue;
                    bestDist = dZ;
                    bestZombie = col.transform;
                }
            }
        }

        return bestZombie;
    }

    void AimAt(Vector3 point)
    {
        var dir = (point - turretPivot.position).normalized;
        var look = Quaternion.LookRotation(dir);
        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            look,
            rotationSpeed * Time.deltaTime
        );
    }

   public void TryShoot(Transform tgt)
    {
        if (tgt == null) return;
        if (Time.time < lastFireTime + 1f / fireRate) return;

        lastFireTime = Time.time;
        // lấy miss rate từ FSM, nếu null thì dùng defaultMissRate
        float rate = combatFSM != null ? combatFSM.MissRate : defaultMissRate;
        bool isMiss = Random.Range(0f, 100f) < rate;
        float damageToApply = isMiss ? 0f : stats.currentAttack;
        var proj = projectilePrefab.Spawn(firePoint.position, turretPivot.rotation);
        if (proj.TryGetComponent<HomingProjectile>(out var h))
        {
            h.SetTarget(tgt);
            h.SetDamage(damageToApply);
        }

        if (muzzleSparkPrefab != null)
        {
            var spark = muzzleSparkPrefab.Spawn(firePoint.position, turretPivot.rotation);
            DOVirtual.DelayedCall(2f, () => spark.Recycle())
                     .SetId(gameObject.GetInstanceID());
        }
    }

}

