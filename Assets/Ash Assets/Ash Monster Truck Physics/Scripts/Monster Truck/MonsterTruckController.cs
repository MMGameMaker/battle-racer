using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTruckController : MonoBehaviour
{
    [Header("Vehicle Stats")]
    public float torque;
    public float angularSpeed = 100;
    public float MaxTurnAngle;
    public float downForce;
    public bool allWheelDrive = true;
    public bool allWheelTurn = false;
    public bool allWheelBrake = true;
    public float Raydistance;
    public float skidWidth = 0.5f;
    public LayerMask drivable;
    public Transform centerOfMass;

    public HingeJoint[] steeringJoint = new HingeJoint[4];
    public HingeJoint[] wheelJoints = new HingeJoint[4];

    


    [Header("Audio settings")]
    public AudioSource engineSound;
    [Range(0, 1)]
    public float minPitch;
    [Range(1, 3)]
    public float maxPitch;

    

    private Rigidbody rb;
    private float horizontalInput, verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        GameObject.FindObjectOfType<Skidmarks>().GetComponent<Skidmarks>().SkidmarkWidth = skidWidth;
    }
    private void Update()
    {
        soundManager();
    }

    void FixedUpdate()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        turnLogic();

        accelarationLogic();

        brakeLogic();
        

        if (grounded())
        {
            AddDownForce();
        }

    }

    //stabalize vehicle when its about to flip -logic starts from here  
    private void OnCollisionEnter(Collision collision)
    {
        rb.centerOfMass = centerOfMass.localPosition ;
    }
    private void OnCollisionExit(Collision collision)
    {
        rb.centerOfMass = Vector3.zero;
    }
    //stabalize vehicle when its about to flip -Logic ends here


    public bool grounded() //check if vehicle is grounded or not(by shooting a ray downwards)
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Raydistance, drivable))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void accelarationLogic()
    {
        if (allWheelDrive)
        {
            foreach (HingeJoint wheel_jt in wheelJoints)
            {
                var motar = wheel_jt.motor;
                motar.targetVelocity = angularSpeed * verticalInput;
                if (Mathf.Abs(verticalInput) > 0.05f && Input.GetAxis("Jump") < 0.1f)
                {
                    motar.force = torque;
                }
                else
                {
                    motar.force = 0;
                }
                wheel_jt.motor = motar;
            }
        }
        else
        {
            var motar = wheelJoints[3].motor;
            motar.targetVelocity = angularSpeed * verticalInput;
            if (Mathf.Abs(verticalInput) > 0.05f && Input.GetAxis("Jump") < 0.1f)
            {
                motar.force = torque;
            }
            else
            {
                motar.force = 0;
            }
            wheelJoints[2].motor = motar;
            wheelJoints[3].motor = motar;
        }
    }
    void brakeLogic()
    {
        if (allWheelBrake)
        {
            if (Input.GetAxis("Jump") > 0.1f)
            {
                foreach (HingeJoint wheel_jt in wheelJoints)
                {
                    wheel_jt.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX;
                }
            }
            else
            {
                foreach (HingeJoint wheel_jt in wheelJoints)
                {
                    wheel_jt.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                }
            }
        }
        else
        {
            if (Input.GetAxis("Jump") > 0.1f)
            {
                wheelJoints[2].transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX;
                wheelJoints[3].transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX;
            }
            else
            {
                wheelJoints[2].transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                wheelJoints[3].transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }

    void turnLogic()
    {
        var steerjointSpring_F = steeringJoint[1].spring;
        steerjointSpring_F.targetPosition = horizontalInput * MaxTurnAngle;
        steeringJoint[0].spring = steerjointSpring_F;
        steeringJoint[1].spring = steerjointSpring_F;

        if (allWheelTurn)
        {
            var steerjointSpring_B = steeringJoint[2].spring;
            steerjointSpring_B.targetPosition = horizontalInput * -MaxTurnAngle / 2;
            steeringJoint[2].spring = steerjointSpring_B;
            steeringJoint[3].spring = steerjointSpring_B;
        }
    }


    void AddDownForce()
    {
        rb.AddForce(Vector3.down * downForce);
    }
    void soundManager()
    {
        
        float maxspeed = 50;
        float speed = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;
        float angularSpeed = GetComponent<Rigidbody>().angularVelocity.magnitude;
        
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(speed + (verticalInput + angularSpeed/10) * 20) / maxspeed);
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            engineSound.volume = Mathf.MoveTowards(engineSound.volume, 1,0.01f);
        }
        else
        {
            engineSound.volume = Mathf.MoveTowards(engineSound.volume, 0.5f, 0.01f);
        }
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * Raydistance);
        
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            BoxCollider collider = transform.Find("Body Collider").GetComponent<BoxCollider>();
            Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);

            Vector3 groundCheckPlane = new Vector3(collider.GetComponent<BoxCollider>().size.x * 1.5f, 0.05f, collider.GetComponent<BoxCollider>().size.z);

            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(transform.position + Vector3.down * Raydistance, 1.5f*groundCheckPlane);
            Gizmos.DrawWireCube(transform.position + Vector3.down * Raydistance, 1.5f * groundCheckPlane);

        }
        
    }

}
