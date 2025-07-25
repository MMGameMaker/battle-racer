using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleLockAt : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private ParticleSystem particle;
    private MainModule mainModule = new MainModule();
    private EmissionModule emisionModule = new EmissionModule();

    public float drift = 10f;
    public float deltaAngle = 90;
    [Range(0.1f, 1f)]
    public float lockAtTime = 1f;
    private float startLifetime = 5f;
    public float StartLifetime
    {
        get => startLifetime;
        set
        {
            startLifetime = value;
            mainModule.startLifetime = startLifetime;
        }
    }

    public float distanceToKill = 0.025f;
    public bool isRotation;
    public float posZ;

    private Vector3 nextPos;
    private Particle[] mParticles;
    public event Action<int> OnEmitDone = delegate { };

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        if (particle == null)
            particle = GetComponent<ParticleSystem>();
        if (particle != null)
        {
            mainModule = particle.main;
            emisionModule = particle.emission;
        }

        if (mParticles == null || mParticles.Length < mainModule.maxParticles)
            mParticles = new Particle[mainModule.maxParticles];

        StartLifetime = mainModule.startLifetimeMultiplier;

        //Debug.LogError("startLifetime: " + mSystem.main.startLifetimeMultiplier);
    }


    private int numParticlesAlive = 0;
    private int NumParticlesAlive = 0;
    private Vector3 targetPosition;
    private Vector3 pos;
    private void LateUpdate()
    {
        if (particle.isStopped)
            NumParticlesAlive = 0;

        if (target == null || particle.isStopped)
            return;

        targetPosition = target.position;
        targetPosition.z = posZ;
        pos = transform.localPosition;
        pos.z = target.position.z;
        transform.localPosition = pos;

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        numParticlesAlive = particle.GetParticles(mParticles);

        //Debug.Log(numParticlesAlive);
        // Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            if (mParticles[i].remainingLifetime < lockAtTime * startLifetime)
            {
                float delta = (lockAtTime - mParticles[i].remainingLifetime) / lockAtTime;
                mParticles[i].position = Vector3.Lerp(mParticles[i].position, targetPosition, delta);

                if (isRotation)
                {
                    Vector3 targ = target.position - transform.position;
                    targ.x = targ.x - mParticles[i].position.x;
                    targ.y = targ.y - mParticles[i].position.y;

                    float angle = Mathf.Atan2(-targ.y, targ.x) * Mathf.Rad2Deg + deltaAngle;
                    mParticles[i].rotation = Mathf.Lerp(mParticles[i].rotation, angle, delta * Time.deltaTime * drift);
                }

                if (Vector3.Distance(target.position, mParticles[i].position) < distanceToKill)
                {
                    mParticles[i].remainingLifetime = 0; //Kill the particle
                }

                // Apply the particle changes to the particle system
                particle.SetParticles(mParticles, numParticlesAlive);
            }

            if (mParticles[i].remainingLifetime <= 0.1f)
            {
                mParticles[i].remainingLifetime = 0;
                OnEmitDone(numParticlesAlive);
            }
        }
    }

    public void Emit(int count)
    {
        Emit(count, null, null);
    }

    public void Emit(int count, Vector3 fromPos)
    {
        SetFromPosition = fromPos;
        emisionModule.rateOverTime = count;
        NumParticlesAlive += count;
        particle.Play();
    }

    public void Emit(int count, Transform from, Transform target = null)
    {
        transform.LookAt(target, Vector3.up);
        SetTransfromParticle = from;
        SetTargetTransform = target;
        emisionModule.rateOverTime = count;
        NumParticlesAlive += count;
        particle.Play();
    }

    public void EmitLocalPosition(int count, Transform from, Transform target = null)
    {
        transform.LookAt(target, Vector3.up);
        transform.localPosition = from.localPosition;
        SetTargetTransform = target;
        emisionModule.rateOverTime = count;
        NumParticlesAlive += count;
        particle.Play();
    }

    //
    // Summary:
    //     Transform of start particle emit.
    public Transform SetTransfromParticle
    {
        get => transform;
        set
        {
            if (value != null)
                transform.position = new Vector3(value.position.x, value.position.y, transform.position.z);
        }
    }

    //
    // Summary:
    //     Position of start particle emit.
    public Vector3 SetFromPosition
    {
        get => transform.position;
        set
        {
            if (value != null)
                transform.position = value;
        }
    }

    public Vector3 SetTargetPosition
    {
        get => target.position;
        set
        {
            if (value != null)
                target.position = value;
        }
    }

    public Transform SetTargetTransform
    {
        get => target;
        set
        {
            if (value != null)
                target = value;
        }
    }
}