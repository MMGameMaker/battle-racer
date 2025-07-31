using UnityEngine;
using System.Linq;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class ZombieRigidbodyAI : MonoBehaviour
{
    public enum ZombieType { RandomSpawn, Guard }
    [Header("Loại Zombie")]
    public ZombieType type = ZombieType.RandomSpawn;
    [Tooltip("Chỉ dùng nếu type = Guard")]
    public Transform guardPoint;

    [Header("Health")]
    [Tooltip("Máu tối đa")]
    public int maxHealth = 100;
    [Tooltip("Máu trừ mỗi lần bắn")]
    public int damagePerShot = 10;
    public int currentHealth;

    [Header("Collision Damage")]
    [Tooltip("Hệ số nhân cho tốc độ va chạm thành máu trừ")]
    public float collisionDamageMultiplier = 1f;
    [Tooltip("Tốc độ va chạm tối thiểu để gây sát thương và ragdoll")]
    public float minRagdollImpactSpeed = 5f;
    [Tooltip("Thời gian ragdoll trước khi bắt đầu get-up (s)")]
    public float ragdollRecoverTime = 2f;

    [Header("Get-Up Animation")]
    [Tooltip("Trigger khi get-up từ tư thế úp (face-down)")]
    public string getUpFromDownTrigger = "getUp_FrontToBack";
    [Tooltip("Trigger khi get-up từ tư thế ngửa (face-up)")]
    public string getUpFromUpTrigger = "getUp_BackToFront";
    [Tooltip("Độ dài animation get-up (giây)")]
    public float getUpDuration = 1.5f;
    [Tooltip("Bone để xác định tư thế nằm (chọn bone gần ngực)")]
    public Transform spineBone;

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

    [Header("Vehicle Attack")]
    [Tooltip("Damage gây ra mỗi lần attack vào xe")]
    public int vehicleDamagePerHit = 20;

    [Header("Sensor Avoidance")]
    public Transform[] frontSensors;
    public float sensorLength = 2f;
    public LayerMask obstacleMask;

    [Header("Animator")]
    public Animator animator;

    [Header("References")]
    [Tooltip("Gán Transform Player trong Inspector")]
    public Transform player;

    // internals
    private Rigidbody rb;
    private Vector3 idlePosition;
    private bool provoked = false;
    private bool isRagdoll = false;
    private bool isRecovering = false;
    private Coroutine recoverRoutine;

    private Transform currentTarget; // có thể là player hoặc vehicle

    enum State { Idle, Chasing, Returning }
    private State state = State.Idle;

    // Ragdoll parts
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;
    private void OnDisable()
    {
        DOTween.Kill(gameObject.GetInstanceID());
    }
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
        // Xác định tư thế nằm: faceUp = true nếu lưng hướng lên trời
        bool faceUp = true;
        if (spineBone != null)
            faceUp = Vector3.Dot(spineBone.up, Vector3.up) > 0f;
    }

    void Start()
    {
        if (player == null) Debug.LogError("[ZombieAI] Player Transform chưa được gán!");
        idlePosition = (type == ZombieType.Guard && guardPoint != null) ? guardPoint.position : transform.position;
        roamTarget = transform.position;
        roamTimer = 0f;
        currentTarget = player;
    }

    void Update()
    {
        if (isRagdoll || isRecovering || currentHealth <= 0 || currentTarget == null) return;
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        switch (state)
        {
            case State.Idle: UpdateIdle(dist); break;
            case State.Chasing: UpdateChasing(dist); break;
            case State.Returning: UpdateReturning(); break;
        }
    }

    public void OnShot() { TakeDamage(damagePerShot, Vector3.zero, Vector3.zero, false); }

    void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitForce, bool isCollision)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0) { Die(hitPoint, hitForce); }
        else if (isCollision)
        {
            if (recoverRoutine != null) StopCoroutine(recoverRoutine);
            recoverRoutine = StartCoroutine(RecoverFromRagdoll(hitPoint, hitForce));
        }
        else provoked = true;
    }

    void Die(Vector3 hitPoint, Vector3 hitForce)
    {
        SetRagdoll(true);
        if (hitForce.sqrMagnitude > 0f)
        {
            var target = ragdollBodies.OrderBy(r => Vector3.Distance(r.position, hitPoint)).FirstOrDefault();
            target?.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);
        }
        enabled = false;
        DOVirtual.DelayedCall(5, () =>
        {
            Destroy(gameObject);
        }).SetId(gameObject.GetInstanceID());
    }

    IEnumerator RecoverFromRagdoll(Vector3 hitPoint, Vector3 hitForce)
    {
        provoked = true;
        isRagdoll = true;
        SetRagdoll(true);
        if (hitForce.sqrMagnitude > 0f)
        {
            var target = ragdollBodies.OrderBy(r => Vector3.Distance(r.position, hitPoint)).FirstOrDefault();
            target?.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(ragdollRecoverTime);

        // Di chuyển root GameObject đến vị trí ragdoll: tính trung bình vị trí các bone
        Vector3 avgPos = Vector3.zero;
        foreach (var rbPart in ragdollBodies)
            avgPos += rbPart.position;
        avgPos /= ragdollBodies.Length;
        // Giữ nguyên chiều cao ban đầu nếu cần hoặc dùng avgPos.y để đặt đúng
        transform.position = avgPos;

        SetRagdoll(false);
        isRecovering = true;

        bool faceUp = true;
        if (spineBone != null)
            faceUp = Vector3.Dot(spineBone.up, Vector3.up) > 0f;
        animator.SetTrigger(faceUp ? getUpFromUpTrigger : getUpFromDownTrigger);
        yield return new WaitForSeconds(getUpDuration);
        isRecovering = false;
        isRagdoll = false;

        // Sau khi đứng dậy, chase lại target
        state = State.Chasing;
        currentTarget = (currentTarget != player) ? currentTarget : player;
        animator.SetBool("isRunning", true);
        // reset cooldown
        lastAttackTime = Time.time - attackCooldown;
    }

    /// <summary>
    /// Bật/tắt chế độ ragdoll: bật physics cho các bone, tắt Animator và collider chính
    /// </summary>
    private void SetRagdoll(bool active)
        {
            // Bones
            foreach (var rbPart in ragdollBodies)
                rbPart.isKinematic = !active;
            foreach (var col in ragdollColliders)
                col.enabled = active;

            // Main
            animator.enabled = !active;
            mainCollider.enabled = !active;
            rb.isKinematic = active;
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.transform.root.CompareTag("Vehicle"))
            {
                float speed = col.relativeVelocity.magnitude;
                Vector3 force = col.impulse;
                Vector3 pt = col.contacts[0].point;
                if (speed < minRagdollImpactSpeed)
                {
                    // Va chạm nhẹ: zombie bắt đầu chase xe
                    currentTarget = col.transform.root;
                    provoked = true;
                    state = State.Chasing;
                    animator.SetBool("isRunning", true);
                    lastAttackTime = Time.time - attackCooldown;
                    return;
                }
                // Va chạm mạnh: ragdoll và damage
                int dmg = Mathf.RoundToInt(speed * collisionDamageMultiplier);
                TakeDamage(dmg, pt, force, true);
            }
        }

        void UpdateIdle(float dist)
        {
            if (type == ZombieType.RandomSpawn) Wander(); else animator.SetBool("isWalking", false);
            if (provoked && dist <= detectionRadius)
            {
                state = State.Chasing;
                animator.SetBool("isRunning", true);
                lastAttackTime = Time.time - attackCooldown;
            }
        }

        void UpdateChasing(float dist)
        {
            if (dist > chaseRadius)
            {
                provoked = false;
                if (type == ZombieType.Guard)
                {
                    state = State.Returning;
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                }
                else state = State.Idle;
                // revert target to player
                currentTarget = player;
                return;
            }
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("attack");
                // TODO: apply damage to vehicle if currentTarget isn't player
            }
            else
            {
                Vector3 dir = (currentTarget.position - transform.position).normalized;
                if (IsObstacleAhead(out Vector3 av)) dir = av;
                MoveAndRotate(dir, runSpeed);
            }
        }

        void UpdateReturning()
        {
            Vector3 to = idlePosition - transform.position;
            if (to.magnitude <= 0.5f)
            {
                state = State.Idle;
                return;
            }
            Vector3 dir = to.normalized;
            if (IsObstacleAhead(out Vector3 av)) dir = av;
            MoveAndRotate(dir, walkSpeed);
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
            if (IsObstacleAhead(out Vector3 av)) dir = av;
            MoveAndRotate(dir, walkSpeed);
        }

        void MoveAndRotate(Vector3 dir, float speed)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            dir.Normalize();
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(dir), rotateSpeed * Time.deltaTime));
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
            bool run = speed > walkSpeed + 0.1f;
            animator.SetBool("isRunning", run);
            animator.SetBool("isWalking", !run);
        }

        bool IsObstacleAhead(out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            foreach (var s in frontSensors)
            {
                if (Physics.Raycast(s.position, s.forward, out RaycastHit h, sensorLength, obstacleMask))
                {
                    Vector3 p = Vector3.Cross(h.normal, Vector3.up).normalized;
                    if (Vector3.Dot(p, transform.forward) < 0f) p = -p;
                    avoidDir = p;
                    return true;
                }
            }
            return false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, chaseRadius);
            if (frontSensors != null) foreach (var s in frontSensors) Gizmos.DrawRay(s.position, s.forward * sensorLength);
            if (type == ZombieType.Guard && guardPoint != null) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(guardPoint.position, 0.2f); }
        }
    }
