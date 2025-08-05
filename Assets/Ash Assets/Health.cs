using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Nhận sát thương; tự động dùng defense từ CarStats nếu có.
    /// </summary>
    public void TakeDamage(float amount)
    {
        var stats = GetComponent<CarStats>();
        if (stats != null)
        {
            amount = Mathf.Max(amount - stats.currentDefense, 0f);
        }

        currentHealth -= amount;
        Debug.Log($"{name} bị trừ {amount} HP, còn {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Hồi HP (từ item).
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{name} được hồi {amount} HP, hiện tại {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{name} đã chết");
        gameObject.SetActive(false);
    }
}
