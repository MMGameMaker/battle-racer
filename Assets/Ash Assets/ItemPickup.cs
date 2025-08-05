using System;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Buff Settings")]
    // Sử dụng enum BuffType từ CarStats
    public BuffType type;
    public float buffAmount = 10f;       // giá trị tăng thêm cho Attack hoặc Defense
    public float buffDuration = 10f;     // thời gian buff (giây)
    public float hpAmount = 30f;         // lượng HP hồi (chỉ dùng cho HP)

    private void Start()
    {
        // Nếu muốn random loại buff, uncomment dòng dưới:
        // type = (CarStats.BuffType)Random.Range(0, System.Enum.GetValues(typeof(CarStats.BuffType)).Length);
    }

    private void OnTriggerEnter(Collider other)
    {
        var stats = other.GetComponent<CarStats>();
        if (stats == null) return;

        switch (type)
        {
            case BuffType.Attack:
                stats.ApplyTemporaryBuff(BuffType.Attack, buffAmount, buffDuration);
                break;
            case BuffType.Defense:
                stats.ApplyTemporaryBuff(BuffType.Defense, buffAmount, buffDuration);
                break;
            case BuffType.Speed:
                stats.ApplyTemporaryBuff(BuffType.Speed, buffAmount, buffDuration);
                break;
            case BuffType.HP:
                stats.ApplyHPBuff(hpAmount);
                break;
        }

        // TODO: thêm hiệu ứng VFX/âm thanh ở đây nếu cần

        Destroy(gameObject);
    }
}
