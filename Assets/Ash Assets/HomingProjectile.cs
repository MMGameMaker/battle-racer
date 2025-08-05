using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("Thiết lập chung")]
    [Tooltip("Tốc độ bay của đạn")]
    public float speed = 30f;

    [Header("Mục tiêu")]
    [Tooltip("Gán Transform của mục tiêu khi spawn")]
    public Transform target;

    
    public float damage = 20f;
    public void SetTarget(Transform t)
    {
        target = t;
    }

    // Gọi từ CarTurretAI để gán sát thương
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }
    void Update()
    {


        // Nếu không còn target thì huỷ
        if (target == null)
        {
            this.Recycle();
            return;
        }
        var targetPos = target.GetComponent<PositionReceiveDamage>() ? target.GetComponent<PositionReceiveDamage>().postitionReceiveDmg : target;
        // Homing về target
        Vector3 dir = (targetPos.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.forward = dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ trigger khi đúng target
        if (other.transform == target|| other.transform.parent == target)
        {
            Health ch = other.GetComponentInParent<Health>();
            if (ch != null)
            {
                ch.TakeDamage(damage);
            }
            ZombieRigidbodyAI zombie = other.GetComponent<ZombieRigidbodyAI>();
            if (zombie != null )
            {
                if (zombie.currentHealth > 0)
                    zombie.OnShot();
                else
                {
                    target=null;
                    return;
                }
            }

            // Tính điểm va chạm gần nhất
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            target.GetComponent<PositionReceiveDamage>().CallExplosion(hitPoint);

            // Huỷ ngay viên đạn
            this.Recycle();
        }
    }
}
