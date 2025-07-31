using OmniVehicleAi;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarTurretAI))]
[RequireComponent(typeof(AIVehicleController))]
public class CarCombatAI : MonoBehaviour
{
    [Header("Combat AI Settings")]
    [Tooltip("Khoảng cách để bắt đầu di chuyển vòng quanh thay vì tiếp tục tiến thẳng")]
    public float engagementRange = 0f;

    [Tooltip("Giá trị accelerationInputAi khi ở trong tầm (0..1) để di chuyển chậm")]
    [Range(0f, 1f)]
    public float slowAcceleration = 0.3f;

    [Tooltip("Tốc độ xoay vòng quanh mục tiêu (độ/giây)")]
    public float orbitSpeed = 30f;

    [Header("Roam Settings")]
    [Tooltip("Bán kính tìm mục tiêu ngẫu nhiên khi không có target trong tầm")]
    public float searchRange = 100f;
    [Tooltip("Khoảng cách để coi như đã đến điểm roamTarget")]
    public float arrivalThreshold = 2f;

    private CarTurretAI turret;
    private AIVehicleController vehicleAI;

    private Transform combatTarget;    // target hiện tại đang engage
    private Transform roamTarget;      // target ngẫu nhiên khi roam
    private GameObject orbitTarget;    // điểm orbit quanh combatTarget
    private float orbitAngle;

    void Awake()
    {
        turret = GetComponent<CarTurretAI>();
        vehicleAI = GetComponent<AIVehicleController>();

        if (engagementRange <= 0f)
            engagementRange = turret.detectionRange;

        orbitTarget = new GameObject("OrbitTarget");
        orbitTarget.transform.parent = transform;
    }

    void Update()
    {
        // 1. Combat: lấy target ưu tiên (Opponent > Zombie) dùng CarTurretAI
        combatTarget = turret.target;
        if (combatTarget != null)
        {
            roamTarget = null;  // bỏ roam nếu đang roam
            HandleCombat(combatTarget);
        }
        else
        {
            // 2. Roam khi không có combatTarget
            HandleRoam();
        }
    }

    void LateUpdate()
    {
        // Khi đang orbit combatTarget, giữ tốc độ slowAcceleration
        if (combatTarget != null)
        {
            float d = Vector3.Distance(transform.position, combatTarget.position);
            if (d <= engagementRange)
            {
                vehicleAI.accelerationInputAi = slowAcceleration;
                vehicleAI.handBrakeInputAi = 0f;
            }
        }
    }

    private void HandleCombat(Transform target)
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > engagementRange)
        {
            // tiến thẳng vào combatTarget
            vehicleAI.target = target;
            vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
            vehicleAI.handBrakeInputAi = 0f;
        }
        else
        {
            // orbit quanh combatTarget
            orbitAngle += orbitSpeed * Time.deltaTime;
            float rad = orbitAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * engagementRange;
            orbitTarget.transform.position = target.position + offset;

            vehicleAI.target = orbitTarget.transform;
            vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
        }
    }

    private void HandleRoam()
    {
        if (roamTarget == null)
        {
            roamTarget = FindRandomTargetOnMap();
            if (roamTarget != null)
            {
                vehicleAI.target = roamTarget;
                vehicleAI.AiMode = AIVehicleController.Ai_Mode.TargetFollow;
            }
            else
            {
                vehicleAI.AiMode = AIVehicleController.Ai_Mode.PathFollow;
            }
        }
        else
        {
            float d = Vector3.Distance(transform.position, roamTarget.position);
            // nếu đã đến hoặc roamTarget bị hủy
            if (d <= arrivalThreshold || roamTarget.gameObject == null)
            {
                roamTarget = null;
            }
        }
    }

    private Transform FindRandomTargetOnMap()
    {
        var candidates = new List<Transform>();
        var seenAIs = new HashSet<CarTurretAI>();

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        foreach (var col in hits)
        {
            // 1. Opponent: lấy CarTurretAI từ parent
            var ai = col.GetComponentInParent<CarTurretAI>();
            if (ai != null && ai != turret && !seenAIs.Contains(ai))
            {
                seenAIs.Add(ai);
                if (ai.team != turret.team)
                    candidates.Add(ai.transform);
                continue;
            }

            // 2. Zombie: dựa vào layer từ turret
            if ((turret.zombieLayer.value & (1 << col.gameObject.layer)) != 0)
            {
                candidates.Add(col.transform);
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    void OnDestroy()
    {
        if (orbitTarget != null)
            Destroy(orbitTarget);
    }
}
