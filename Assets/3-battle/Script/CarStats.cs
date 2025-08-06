using System.Collections;
using UnityEngine;

public class CarStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseSpeed = 8f;
    public float maxHP = 100f;

    [HideInInspector] public float currentAttack;
    [HideInInspector] public float currentDefense;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentHP;

    private void Awake()
    {
        ResetStats();
    }

    public void ResetStats()
    {
        currentAttack = baseAttack;
        currentDefense = baseDefense;
        currentSpeed = baseSpeed;
        currentHP = maxHP;
    }

    public void ApplyHPBuff(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public void ApplyTemporaryBuff(BuffType type, float amount, float duration)
    {
        switch (type)
        {
            case BuffType.Attack:
                StartCoroutine(TemporaryBuffCoroutine(
                    () => currentAttack += amount,
                    () => currentAttack -= amount,
                    duration));
                break;
            case BuffType.Defense:
                StartCoroutine(TemporaryBuffCoroutine(
                    () => currentDefense += amount,
                    () => currentDefense -= amount,
                    duration));
                break;
            case BuffType.Speed:
                StartCoroutine(TemporaryBuffCoroutine(
                    () => currentSpeed += amount,
                    () => currentSpeed -= amount,
                    duration));
                break;
        }
    }

    private IEnumerator TemporaryBuffCoroutine(System.Action apply, System.Action revert, float duration)
    {
        apply.Invoke();
        yield return new WaitForSeconds(duration);
        revert.Invoke();
    }
}

public enum BuffType
{
    Attack,
    Defense,
    Speed,
    HP
}
