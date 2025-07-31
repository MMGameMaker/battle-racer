using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CarTurretAI : MonoBehaviour
{
    public enum Team { Team1, Team2 }

    [Header("Team Settings")]
    public Team team;

    [Header("Turret Setup")]
    public Transform turretPivot;        // Gốc nòng súng
    public Transform firePoint;          // Vị trí spawn đạn
    public HomingProjectile projectilePrefab;  // Prefab đạn
    public ParticleSystem muzzleSparkPrefab; // Prefab particle tia lửa

    [Header("Combat Settings")]
    public float detectionRange = 50f;   // Tầm phát hiện
    public float rotationSpeed = 120f;   // Tốc độ quay turret (độ/giây)
    public float fireRate = 2f;          // Số phát bắn mỗi giây
    public float shootingRange = 50f;    // Tầm bắn
    public LayerMask zombieLayer;        // Layer zombie
    public LayerMask obstacleLayer;      // Layer chướng ngại

    private float lastFireTime;
    public Transform target;
    private void Start()
    {
        projectilePrefab.CreatePool(10);
        muzzleSparkPrefab.CreatePool(3);
    }
    private void OnDisable()
    {
        DOTween.Kill(gameObject.GetInstanceID());
    }
    void Update()
    {
        target = GetPriorityTarget();
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= shootingRange)
            {
                AimAt(target.position);
                TryShoot(target);
            }
        }
    }

    private Transform GetPriorityTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        Transform zombieTarget = null;
        float closestZombieDist = Mathf.Infinity;
        var seenAIs = new HashSet<CarTurretAI>();

        foreach (Collider col in hits)
        {
            // Bỏ qua collider của chính nó (và các con)
            if (col.transform.IsChildOf(transform))
                continue;

            // 1. Tìm CarTurretAI ở parent của collider này
            CarTurretAI ai = col.GetComponentInParent<CarTurretAI>();
            if (ai != null && ai != this && !seenAIs.Contains(ai))
            {
                seenAIs.Add(ai);
                // Chỉ chọn nếu khác team và không bị chắn
                if (ai.team != this.team)
                {
                    Vector3 dir = (ai.transform.position - turretPivot.position).normalized;
                    float d = Vector3.Distance(turretPivot.position, ai.transform.position);
                    if (!Physics.Raycast(turretPivot.position, dir, d, obstacleLayer))
                        return ai.transform;
                }
                continue;
            }

            // 2. Nếu là zombie (layer) thì xử lý như trước
            if ((zombieLayer.value & (1 << col.gameObject.layer)) != 0)
            {
                float d = Vector3.Distance(transform.position, col.transform.position);
                Vector3 dir = (col.transform.position - turretPivot.position).normalized;
                if (d < closestZombieDist && !Physics.Raycast(turretPivot.position, dir, d, obstacleLayer))
                {
                    closestZombieDist = d;
                    zombieTarget = col.transform;
                }
            }
        }

        return zombieTarget;
    }

    private void AimAt(Vector3 point)
    {
        Vector3 dir = (point - turretPivot.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    private void TryShoot(Transform target)
    {
        if (Time.time < lastFireTime + 1f / fireRate) return;
        lastFireTime = Time.time;

        // 1. Spawn projectile
        var projObj = projectilePrefab.Spawn(firePoint.position, turretPivot.rotation);
        if (projObj.TryGetComponent<HomingProjectile>(out var homing))
            homing.target = target;

        // 2. Spawn muzzle spark effect
        if (muzzleSparkPrefab != null)
        {
            var spark = muzzleSparkPrefab.Spawn(firePoint.position, turretPivot.rotation);
            DOVirtual.DelayedCall(2, () =>
            {
                spark.Recycle();
            }).SetId(gameObject.GetInstanceID());
        }
    }

    private void OnDrawGizmosSelected()
    {
        // detectionRange (vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // shootingRange (đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
