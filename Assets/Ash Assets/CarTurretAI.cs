using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
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

    private float lastFireTime;
    public Transform target;
    private CarStats stats;
    private Health health;

    private void Awake()
    {
        stats = GetComponent<CarStats>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        projectilePrefab.CreatePool(10);
        muzzleSparkPrefab.CreatePool(3);
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject.GetInstanceID());
    }

    private void Update()
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
            if (col.transform.IsChildOf(transform)) continue;

            var ai = col.GetComponentInParent<CarTurretAI>();
            if (ai != null && ai != this && !seenAIs.Contains(ai))
            {
                seenAIs.Add(ai);
                if (ai.team != team)
                {
                    Vector3 dir = (ai.transform.position - turretPivot.position).normalized;
                    float d = Vector3.Distance(turretPivot.position, ai.transform.position);
                    if (Physics.Raycast(turretPivot.position, dir, out RaycastHit hitAi, d)
                        && hitAi.transform != ai.transform)
                        continue;
                    return ai.transform;
                }
                continue;
            }

            if ((zombieLayer.value & (1 << col.gameObject.layer)) != 0)
            {
                float d = Vector3.Distance(transform.position, col.transform.position);
                Vector3 dir = (col.transform.position - turretPivot.position).normalized;
                if (d < closestZombieDist)
                {
                    if (Physics.Raycast(turretPivot.position, dir, out RaycastHit hitZ, d)
                        && hitZ.transform != col.transform)
                        continue;
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

        var projObj = projectilePrefab.Spawn(firePoint.position, turretPivot.rotation);
        if (projObj.TryGetComponent<HomingProjectile>(out var homing))
        {
            homing.SetTarget(target);
            homing.SetDamage(stats.currentAttack);
        }

        if (muzzleSparkPrefab != null)
        {
            var spark = muzzleSparkPrefab.Spawn(firePoint.position, turretPivot.rotation);
            DOVirtual.DelayedCall(2f, () => spark.Recycle())
                     .SetId(gameObject.GetInstanceID());
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }

}
