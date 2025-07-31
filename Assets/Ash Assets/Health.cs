using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Thiết lập máu")]
    public float maxHealth = 100f;      // Máu tối đa
    private float currentHealth;        // Máu hiện tại

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Gọi khi bị trúng đạn
    /// </summary>
    /// <param name="amount">Lượng damage</param>
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} bị trừ {amount} máu, còn {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Xử lý khi máu <= 0
    /// </summary>
    private void Die()
    {
        // TODO: phát hiệu ứng nổ, âm thanh, v.v.
        Destroy(gameObject);
    }
}
