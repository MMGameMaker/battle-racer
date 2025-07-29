using UnityEngine;

public class SmartFollowTarget : MonoBehaviour
{
    public Transform vehicle;
    public Rigidbody vehicleRb;
    public Vector3 baseOffset = new Vector3(0, 3, -6);
    public float speedZoomFactor = 0.05f;
    public float airZoomOffset = 2f;
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (vehicle == null || vehicleRb == null) return;

        // Tính offset động theo tốc độ và trạng thái trên không
        float speed = vehicleRb.velocity.magnitude;
        bool isGrounded = Physics.Raycast(vehicle.position, -Vector3.up, 1.5f);
        float zoom = speed * speedZoomFactor + (!isGrounded ? airZoomOffset : 0f);

        // Gán vị trí
        Vector3 offset = baseOffset - new Vector3(0, 0, zoom);
        Vector3 desiredPosition = vehicle.position + vehicle.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // ❗ Gán xoay theo hướng của xe
        transform.rotation = Quaternion.Slerp(transform.rotation, vehicle.rotation, followSpeed * Time.deltaTime);
    }
}
