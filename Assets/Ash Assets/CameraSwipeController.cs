using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class FreeLookTouchPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Drag speed điều chỉnh")]
    public float horizontalSpeed = 0.005f;
    public float verticalSpeed = 0.005f;

    [Header("Ref to FreeLook")]
    public CinemachineFreeLook freeLook;

    bool isDragging;
    Vector2 prevPos;

    // Khi chạm xuống
    public void OnPointerDown(PointerEventData e)
    {
        isDragging = true;
        prevPos = e.position;
    }

    // Khi rê tay
    public void OnDrag(PointerEventData e)
    {
        Vector2 delta = e.position - prevPos;
        prevPos = e.position;

        // Gán input value cho FreeLook axes
        freeLook.m_XAxis.m_InputAxisValue = delta.x * horizontalSpeed;
        // đảo chiều Y nếu cần, ở đây kéo lên → giá trị + thì camera cúi xuống, nên trừ ngược
        freeLook.m_YAxis.m_InputAxisValue = -delta.y * verticalSpeed;
    }

    // Khi nhả tay
    public void OnPointerUp(PointerEventData e)
    {
        isDragging = false;
        // Reset về 0 để recenter tự chạy (như đã cấu hình recenter ở Body→Heading và Y Axis)
        freeLook.m_XAxis.m_InputAxisValue = 0;
        freeLook.m_YAxis.m_InputAxisValue = 0;
    }

    void Update()
    {
        if (!isDragging)
        {
            // Đảm bảo không còn residual input
            freeLook.m_XAxis.m_InputAxisValue = 0;
            freeLook.m_YAxis.m_InputAxisValue = 0;
        }
    }
}
