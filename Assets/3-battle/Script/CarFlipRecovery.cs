using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class CarFlipRecovery : MonoBehaviour
{
    [Header("Va chạm bên hông")]
    [Tooltip("Tốc độ tương đối tối thiểu để kích hoạt nghiêng/lật")]
    public float sideImpactThreshold = 10f;
    [Tooltip("Hệ số torque khi va chạm")]
    public float torqueMultiplier = 1f;

    [Header("Hồi phục sau lật")]
    [Tooltip("Tốc độ slerp để xoay thẳng lại")]
    public float recoverySpeed = 1f;
    [Tooltip("Ngưỡng dot(transform.up, Vector3.up) để coi như đã thẳng hoàn toàn")]
    public float uprightThreshold = 0.95f;

    private Rigidbody rb;
    private bool isRecovering = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tính vận tốc tương đối
        Vector3 relVel = collision.relativeVelocity;
        // Lấy thành phần ngang (loại bỏ phần trùng forward)
        Vector3 lateral = relVel - Vector3.Project(relVel, transform.forward);
        float latSpeed = lateral.magnitude;
        // Kiểm tra xem va chạm có phải từ bên hông (dot so với transform.right)
        float sideDot = Vector3.Dot(lateral.normalized, transform.right);

        if (Mathf.Abs(sideDot) > 0.5f && latSpeed > sideImpactThreshold)
        {
            // Áp dụng torque quanh trục forward để nghiêng xe
            Vector3 torqueDir = transform.forward * Mathf.Sign(sideDot);
            rb.AddTorque(torqueDir * latSpeed * torqueMultiplier, ForceMode.Impulse);
        }
    }

    void Update()
    {
        // Nếu nghiêng quá (up vector thấp), bắt đầu hồi phục
        if (Vector3.Dot(transform.up, Vector3.up) < 0.3f)
            isRecovering = true;

        if (isRecovering)
        {
            // Tính rotation mong muốn: giữ hướng ngang (forward), dựng up lên Vector3.up
            Vector3 flatFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Quaternion desired = Quaternion.LookRotation(flatFwd, Vector3.up);
            // Dùng MoveRotation + Slerp để xoay mượt mà
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, desired, recoverySpeed * Time.deltaTime));

            // Nếu đã gần thẳng lại, ngừng hồi phục
            if (Vector3.Dot(transform.up, Vector3.up) > uprightThreshold)
                isRecovering = false;
        }
    }
}
