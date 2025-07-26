using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class AiCarContrtoller : MonoBehaviour
{
	[Header("Suspension")]
	[Range(0, 5)]
	public float SuspensionDistance = 0.2f;
	public float suspensionForce = 30000f;
	public float suspensionDamper = 200f;
	public Transform groundCheck;
	public Transform fricAt;
	public Transform CentreOfMass;

	private Rigidbody rb;

	//private CinemachineVirtualCamera cinemachineVirtualCamera;
	[Header("Car Stats")]
	public float speed = 200f;
	public float turn = 100f;
	public float brake = 150f;
	public float friction = 70f;
	public float dragAmount = 4f;
	public float TurnAngle = 30f;
	//[HideInInspector]
	public float maxRayLength = 0.8f, slerpTime = 0.2f;
	[HideInInspector]
	public bool grounded;

	public Transform TargetTransform;
	[Header("Visuals")]
	public Transform[] TireMeshes;
	public Transform[] TurnTires;

	[Header("Curves")]
	public AnimationCurve frictionCurve;
	public AnimationCurve speedCurve;
	public AnimationCurve turnCurve;
	public AnimationCurve driftCurve;
	public AnimationCurve engineCurve;

	

	private float speedValue, fricValue, turnValue, curveVelocity, brakeInput;
	[HideInInspector]
	public Vector3 carVelocity;
	[HideInInspector]
	public RaycastHit hit;
	//public bool drftSndMachVel;
	[Header("Other Settings")]
	public AudioSource[] engineSounds;
	public bool airDrag;
	public float SkidEnable = 20f;
	public float skidWidth = 0.12f;
	private float frictionAngle;
	//Ai stuff
	[HideInInspector]
	public float TurnAI = 1f;
	[HideInInspector]
	public float SpeedAI = 1f;
	[HideInInspector]
	public float brakeAI = 0f;
	private Vector3 targetPosition;
	//private float timeOutOfRoad = 0;
	public float brakeAngle = 30f;
	public float RoadWidth = 15;

	public Sensors sensorScript;

	private float desiredTurning;


	[HideInInspector]
	public Vector3 normalDir;

	private void Awake()
	{
		//cinemachineVirtualCamera = transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();

		rb = GetComponent<Rigidbody>();
		grounded = false;
		engineSounds[1].mute = true;
		rb.centerOfMass = CentreOfMass.localPosition;
	}
	

	void FixedUpdate()
	{
		carVelocity = transform.InverseTransformDirection(rb.velocity); //local velocity of car
        
		curveVelocity = Mathf.Abs(carVelocity.magnitude) / 100;

		//inputs
		float turnInput;
        if (sensorScript.obstacleInPath == true)
        {
			turnInput = turn * -sensorScript.turnmultiplyer * Time.fixedDeltaTime * 1000;
        }
        else
        {
			turnInput = turn * TurnAI * Time.fixedDeltaTime * 1000;

		}

		float speedInput = speed * SpeedAI * Time.fixedDeltaTime * 1000;
		brakeInput = brake * -brakeAI * Time.fixedDeltaTime * 1000;

		//helping veriables
		speedValue = speedInput * speedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
		fricValue = friction * frictionCurve.Evaluate(carVelocity.magnitude / 100);
		//turnValue = turnInput * turnCurve.Evaluate(carVelocity.magnitude / 100);


		// the new method of calculating turn value
		Vector3 aimedPoint = TargetTransform.position;
		aimedPoint.y = transform.position.y;
		Vector3 aimedDir = (aimedPoint - transform.position).normalized;
		Vector3 myDir = transform.forward;
		myDir.y = 0;
		myDir.Normalize();
		desiredTurning = Mathf.Abs(Vector3.Angle(myDir, aimedDir));
		turnValue = turnInput * turnCurve.Evaluate(desiredTurning / TurnAngle);


		//grounded check
		if (Physics.Raycast(groundCheck.position, -transform.up, out hit, maxRayLength))
		{
			accelarationLogic();
			turningLogic();
			frictionLogic();
			brakeLogic();
			//for drift behaviour
			rb.angularDrag = dragAmount * driftCurve.Evaluate(Mathf.Abs(carVelocity.x) / 70);

			//draws green ground checking ray ....ingnore
			Debug.DrawLine(groundCheck.position, hit.point, Color.green);
			grounded = true;
			if(hit.transform.tag == "road")
			{
				Vector3 rodePos = transform.position;
				rb.drag = 0.1f;
			}
			else { rb.drag = 0.1f; }

			rb.centerOfMass = Vector3.zero;

			normalDir = hit.normal;

		}
		else
		{
			grounded = false;
			rb.drag = 0.1f;
			rb.centerOfMass = CentreOfMass.localPosition;
			if (!airDrag)
			{
				rb.angularDrag = 0.1f;
			}

		}
	}

	void Update()
	{
		tireVisuals();
		//ShakeCamera(1.2f, 10f);
		audioControl();
		SetTargetPosition(TargetTransform.position);

		float reachedTargetDistance = 1f;
		float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
		Vector3 dirToMovePosition = (targetPosition - transform.position).normalized;
		float dot = Vector3.Dot(transform.forward, dirToMovePosition);
		float angleToMove = Vector3.Angle(transform.forward, dirToMovePosition);
		if (angleToMove > brakeAngle || sensorScript.obstacleInPath)
		{
			if(carVelocity.z > 15)
            {
				brakeAI = -1;
            }
            else
            {
				brakeAI = 0;
            }
			
        }
        else { brakeAI = 0; }

		if (distanceToTarget > reachedTargetDistance)
		{
			//Target is still far , keep acelarating 
			
			if(dot > 0)
            {
				SpeedAI = 1f;

				float stoppingDistance = 5f;
				float stoppingSpeed = 5f;
				if(distanceToTarget < stoppingDistance && curveVelocity > stoppingSpeed)
                {
					//brakeAI = -1f;
                }
                else 
				{ 
					//brakeAI = 0f;
				}
            }
            else
            {
				float reverseDistance = 5f;
				if(distanceToTarget > reverseDistance)
                {
					SpeedAI = 1f;
                }
                else
                {
					//brakeAI = -1f;
                }
            }

			float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

			if(angleToDir > 0)
            {
				TurnAI = 1f * turnCurve.Evaluate(desiredTurning / TurnAngle);
            }
            else
            {
				TurnAI = -1f * turnCurve.Evaluate(desiredTurning / TurnAngle);
            }

        }
        else // reached target
        {
			if(carVelocity.z > 1f)
            {
				//brakeAI = -1f;
            }
            else
            {
				//brakeAI = 0f;
            }
			TurnAI = 0f;
        }
		
		

	}

	public void SetTargetPosition(Vector3 TargetPos)
    {
		targetPosition = TargetPos;
    }

	public void ShakeCamera(float amplitude, float frequency)
	{
		// CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
		//cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

		//cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = curveVelocity * amplitude;
		//cinemachineBasicMultiChannelPerlin.m_FrequencyGain = curveVelocity * frequency;
	}

	public void audioControl()
	{
		//audios
		if (grounded)
		{
			if (Mathf.Abs(carVelocity.x) > SkidEnable - 0.1f)
			{
				engineSounds[1].mute = false;
			}
			else { engineSounds[1].mute = true; }
		}
		else
		{
			engineSounds[1].mute = true;
		}

		/*if (drftSndMachVel) 
		{ 
		engineSounds[1].pitch = (0.7f * (Mathf.Abs(carVelocity.x) + 10f) / 40);
		}
		else { engineSounds[1].pitch = 1f; }*/

		engineSounds[1].pitch = 1f;

		engineSounds[0].pitch = 2 * engineCurve.Evaluate(curveVelocity);
		if (engineSounds.Length == 2)
		{
			return;
		}
		else { engineSounds[2].pitch = 2 * engineCurve.Evaluate(curveVelocity); }

        

	}

	public void tireVisuals()
	{
		//Tire mesh rotate
		foreach (Transform mesh in TireMeshes)
		{
			mesh.transform.RotateAround(mesh.transform.position, mesh.transform.right, carVelocity.z/3);
		}

		//TireTurn
		foreach (Transform FM in TurnTires)
		{

			if (sensorScript.obstacleInPath == true)
			{
				FM.localRotation = Quaternion.Slerp(FM.localRotation, Quaternion.Euler(FM.localRotation.eulerAngles.x,
				Mathf.Clamp(desiredTurning, desiredTurning, TurnAngle) * -sensorScript.turnmultiplyer, FM.localRotation.eulerAngles.z), slerpTime);
			}
			else
			{
				FM.localRotation = Quaternion.Slerp(FM.localRotation, Quaternion.Euler(FM.localRotation.eulerAngles.x,
				Mathf.Clamp(desiredTurning, desiredTurning, TurnAngle) * TurnAI, FM.localRotation.eulerAngles.z), slerpTime);

			}
		}
	}

	public void accelarationLogic()
	{
		//speed control
		if (SpeedAI > 0.1f)
		{
			rb.AddForceAtPosition(transform.forward * speedValue, groundCheck.position);
		}
		if (SpeedAI < -0.1f)
		{
			rb.AddForceAtPosition(transform.forward * speedValue, groundCheck.position);
		}
	}

	public void turningLogic()
	{
		//turning
		if (carVelocity.z > 0.1f)
		{
			rb.AddTorque(transform.up * turnValue);
		}
		
		if (carVelocity.z < -0.1f)
		{
			rb.AddTorque(transform.up * -turnValue);
		}
	}


	public void frictionLogic()
	{
		//Friction
		if (carVelocity.magnitude > 1)
		{
			frictionAngle = (-Vector3.Angle(transform.up, Vector3.up)/90f) + 1 ;
			rb.AddForceAtPosition(transform.right * fricValue * frictionAngle * 100 * -carVelocity.normalized.x, fricAt.position);
		}
	}

	public void brakeLogic()
	{
		//brake
		if (carVelocity.z > 0.1f)
		{
			rb.AddForceAtPosition(transform.forward * -brakeInput, groundCheck.position);
		}
		if (carVelocity.z < -0.1f)
		{
			rb.AddForceAtPosition(transform.forward * brakeInput, groundCheck.position);
		}
	}

	private void OnDrawGizmos()
	{

		if (!Application.isPlaying)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(groundCheck.position, groundCheck.position - maxRayLength * groundCheck.up);
			Gizmos.DrawWireCube(groundCheck.position - maxRayLength * (groundCheck.up.normalized), new Vector3(5, 0.02f, 10));
			Gizmos.color = Color.magenta;
			if (GetComponent<BoxCollider>())
			{
				Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
			}



			Gizmos.color = Color.red;
			foreach (Transform mesh in TireMeshes)
			{
				var ydrive = mesh.parent.parent.GetComponent<ConfigurableJoint>().yDrive;
				ydrive.positionDamper = suspensionDamper;
				ydrive.positionSpring = suspensionForce;


				mesh.parent.parent.GetComponent<ConfigurableJoint>().yDrive = ydrive;

				var jointLimit = mesh.parent.parent.GetComponent<ConfigurableJoint>().linearLimit;
				jointLimit.limit = SuspensionDistance;
				mesh.parent.parent.GetComponent<ConfigurableJoint>().linearLimit = jointLimit;

				Handles.color = Color.red;
				Handles.ArrowHandleCap(0, mesh.position, mesh.rotation * Quaternion.LookRotation(Vector3.up), jointLimit.limit, EventType.Repaint);
				Handles.ArrowHandleCap(0, mesh.position, mesh.rotation * Quaternion.LookRotation(Vector3.down), jointLimit.limit, EventType.Repaint);
				//Gizmos.DrawLine(mesh.position + jointLimit.limit * mesh.up, mesh.position - jointLimit.limit * mesh.up);


			}
			float wheelRadius = TurnTires[0].parent.GetComponent<SphereCollider>().radius;
			float wheelYPosition = TurnTires[0].parent.parent.localPosition.y + TurnTires[0].parent.localPosition.y;
			maxRayLength = (groundCheck.localPosition.y - wheelYPosition + (0.05f + wheelRadius));

		}

	}

}

