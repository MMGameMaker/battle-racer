using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionReceiveDamage : MonoBehaviour
{
    public Transform postitionReceiveDmg;

    [Header("Hiệu ứng nổ")]
    [Tooltip("Prefab ParticleSystem để làm hiệu ứng nổ (Play On Awake)")]
    public GameObject explosionPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CallExplosion(Vector3 hitPoint)
    {
        // Spawn explosion tại điểm va chạm
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(
                explosionPrefab,
                hitPoint,
                Quaternion.identity
            );
            Destroy(exp, 3f); // Huỷ explosion sau 3 giây
        }
    }
}
