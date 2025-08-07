using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarTurretAI))]
public class CarFlipRecovery : MonoBehaviour
{
    [Header("Turret Lock When Tilted")]
    [Tooltip("Max tilt angle (deg) để cho phép bắn")]
    public float allowedTiltAngle = 20f;

    [Header("Auto Recovery Settings")]
    [Tooltip("Khi nghiêng ≥ angle này (deg), sẽ tự kích hoạt recovery")]
    public float flipRecoveryMinAngle = 80f;
    [Tooltip("Dot(transform.up, Vector3.up) ≥ threshold để coi như đã thẳng lại")]
    [Range(0f, 1f)]
    public float uprightDotThreshold = 0.8f;

    private Rigidbody rb;
    private CarTurretAI turretAI;
    private bool isRecovering = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        turretAI = GetComponent<CarTurretAI>();
    }

    void Update()
    {
        // Tính góc nghiêng giữa up của xe và up thế giới
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        // Vô hiệu hóa/bật turret dựa trên tilt và recovery
        if (turretAI != null)
        {
            bool canFire = !isRecovering && tiltAngle <= allowedTiltAngle;
            turretAI.enabled = canFire;
        }

        // Nếu tilt vượt ngưỡng recovery và chưa đang recovery thì bật recovery
        if (!isRecovering && tiltAngle >= flipRecoveryMinAngle)
        {
            isRecovering = true;
        }

        if (isRecovering)
        {
            PerformRecovery();

            // Khi đã gần thẳng (dot ≥ threshold), tắt recovery
            if (Vector3.Dot(transform.up, Vector3.up) >= uprightDotThreshold)
            {
                isRecovering = false;
            }
        }
    }

    /// <summary>
    /// Áp dụng một lực xoắn để lật xe trở lại tư thế thẳng.
    /// </summary>
    private void PerformRecovery()
    {
        // Tính vector trục để quay từ trạng thái hiện tại về up thế giới
        Vector3 axis = Vector3.Cross(transform.up, Vector3.up).normalized;
        // Lực xoắn tỉ lệ với góc lệch
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        float torqueStrength = tiltAngle * Mathf.Deg2Rad * rb.mass * 5f;
        rb.AddTorque(axis * torqueStrength, ForceMode.VelocityChange);
    }
}
