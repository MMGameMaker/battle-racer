using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class ZombieRigidbodyAI : MonoBehaviour
{
    public enum ZombieType { RandomSpawn, Guard }
    [Header("Loại Zombie")]
    public ZombieType type = ZombieType.RandomSpawn;
    [Tooltip("Chỉ dùng nếu type = Guard")]
    public Transform guardPoint;

    [Header("Layers")]
    [Tooltip("Layers chứa Player (gán trong Inspector)")]
    public LayerMask playerLayerMask;

    [Header("Health")]
    [Tooltip("Máu tối đa")]
    public int maxHealth = 100;
    [Tooltip("Máu trừ mỗi lần bắn")]
    public int damagePerShot = 10;
    private int currentHealth;

    [Header("Phạm vi & tốc độ")]
    public float detectionRadius = 10f;
    public float chaseRadius = 20f;
    public float attackRange = 2f;
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float rotateSpeed = 120f;

    [Header("Wander (RandomSpawn)")]
    public float roamRadius = 8f;
    public float roamInterval = 4f;
    private Vector3 roamTarget;
    private float roamTimer;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    private float lastAttackTime = -999f;

    [Header("Collision Kill")]
    [Tooltip("Tốc độ va chạm tối thiểu để zombie chết khi chạm Vehicle")]
    public float minImpactKillSpeed = 5f;

    [Header("Sensor Avoidance")]
    public Transform[] frontSensors;
    public float sensorLength = 2f;
    public LayerMask obstacleMask;

    [Header("Animator")]
    public Animator animator; // params: isWalking, isRunning, trigger "attack"

    [Header("References")]
    [Tooltip("Gán Transform Player trong Inspector (không dùng Tag)")]
    public Transform player;

    Rigidbody rb;
    Vector3 idlePosition;

    // Ragdoll parts
    Rigidbody[] ragdollBodies;
    Collider[] ragdollColliders;
    Collider mainCollider;

  public  bool provoked = false;
    enum State { Idle, Chasing, Returning }
    State state = State.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        currentHealth = maxHealth;

        ragdollBodies = GetComponentsInChildren<Rigidbody>().Where(r => r != rb).ToArray();
        ragdollColliders = GetComponentsInChildren<Collider>().Where(c => c != GetComponent<Collider>()).ToArray();
        mainCollider = GetComponent<Collider>();

        SetRagdoll(false);
    }

    void Start()
    {
        if (player == null)
            Debug.LogError("[ZombieAI] Player Transform chưa được gán!");

        idlePosition = (type == ZombieType.Guard && guardPoint != null)
            ? guardPoint.position
            : transform.position;
        roamTarget = transform.position;
        roamTimer = 0f;
    }

    void Update()
    {
        if (currentHealth <= 0 || player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Idle: UpdateIdle(distToPlayer); break;
            case State.Chasing: UpdateChasing(distToPlayer); break;
            case State.Returning: UpdateReturning(); break;
        }
    }

    public void OnShot()
    {
        TakeDamage(damagePerShot, Vector3.zero, Vector3.zero);
    }

    void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitForce)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die(hitPoint, hitForce);
        else
            provoked = true;
    }

    void Die(Vector3 hitPoint, Vector3 hitForce)
    {
        SetRagdoll(true);
        if (hitForce.sqrMagnitude > 0f)
        {
            var target = ragdollBodies.OrderBy(r => Vector3.Distance(r.position, hitPoint)).FirstOrDefault();
            if (target != null)
                target.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);
        }
        enabled = false;
    }

    void SetRagdoll(bool active)
    {
        foreach (var rbPart in ragdollBodies) rbPart.isKinematic = !active;
        foreach (var col in ragdollColliders) col.enabled = active;
        animator.enabled = !active;
        mainCollider.enabled = !active;
        rb.isKinematic = active;
    }

    void OnCollisionEnter(Collision col)
    {
        Transform otherRoot = col.transform.root;
        if (otherRoot.CompareTag("Vehicle"))
        {
            float impactSpeed = col.relativeVelocity.magnitude;
            if (impactSpeed >= minImpactKillSpeed)
            {
                Debug.Log($"[ZombieAI] Vehicle collision at speed {impactSpeed}");
                Vector3 force = col.impulse;
                Vector3 point = col.contacts[0].point;
                TakeDamage(currentHealth, point, force);
            }
        }
    }

    void UpdateIdle(float distToPlayer)
    {
        if (type == ZombieType.RandomSpawn)
            Wander();
        else
            animator.SetBool("isWalking", false);

        if (provoked && distToPlayer <= detectionRadius)
        {
            state = State.Chasing;
            animator.SetBool("isRunning", true);
        }
    }

    void Wander()
    {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f || Vector3.Distance(transform.position, roamTarget) < 0.5f)
        {
            Vector2 rnd = Random.insideUnitCircle * roamRadius;
            roamTarget = idlePosition + new Vector3(rnd.x, 0, rnd.y);
            roamTimer = roamInterval;
        }
        Vector3 dir = (roamTarget - transform.position).normalized;
        if (IsObstacleAhead(out Vector3 avoid)) dir = avoid;
        MoveAndRotate(dir, walkSpeed);
    }

    void UpdateChasing(float distToPlayer)
    {
        if (distToPlayer > chaseRadius)
        {
            provoked = false;
            if (type == ZombieType.Guard)
            {
                state = State.Returning;
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", true);
            }
            else
                state = State.Idle;
            return;
        }
        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("attack");
            return;
        }
        Vector3 dir = (player.position - transform.position).normalized;
        if (IsObstacleAhead(out Vector3 avoidDir)) dir = avoidDir;
        MoveAndRotate(dir, runSpeed);
    }

    void UpdateReturning()
    {
        Vector3 toHome = idlePosition - transform.position;
        if (toHome.magnitude <= 0.5f) { state = State.Idle; return; }
        Vector3 dir = toHome.normalized;
        if (IsObstacleAhead(out Vector3 avoid)) dir = avoid;
        MoveAndRotate(dir, walkSpeed);
    }

    void MoveAndRotate(Vector3 dir, float speed)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotateSpeed * Time.deltaTime));

        Vector3 move = dir * speed * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        bool isRun = speed > walkSpeed + 0.1f;
        animator.SetBool("isRunning", isRun);
        animator.SetBool("isWalking", !isRun);
    }

    bool IsObstacleAhead(out Vector3 avoidDir)
    {
        avoidDir = Vector3.zero;
        foreach (var s in frontSensors)
        {
            if (Physics.Raycast(s.position, s.forward, out RaycastHit hit, sensorLength, obstacleMask))
            {
                Vector3 perp = Vector3.Cross(hit.normal, Vector3.up).normalized;
                if (Vector3.Dot(perp, transform.forward) < 0f) perp = -perp;
                avoidDir = perp;
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        if (frontSensors != null)
            foreach (var s in frontSensors)
                Gizmos.DrawRay(s.position, s.forward * sensorLength);
        if (type == ZombieType.Guard && guardPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(guardPoint.position, 0.2f);
        }
    }
}
