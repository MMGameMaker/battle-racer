using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAiBalence : MonoBehaviour
{
	public Rigidbody vehicleBody;
	public float LeanTorqueAmount;
	private float leanAngle;
	public AnimationCurve BalanceTorqueCurve;
	public bool AlwaysAddTorque;
	private AiCarContrtoller carController;

	Vector3 normalDir;

	private void Start()
	{
		carController = vehicleBody.GetComponent<AiCarContrtoller>();
	}

	void FixedUpdate()
	{
		normalDir = carController.normalDir;

		Vector3 projectedBikeUpDir = Vector3.ProjectOnPlane(vehicleBody.transform.up, Vector3.Cross(normalDir, vehicleBody.transform.right));

		leanAngle = Vector3.SignedAngle(normalDir, projectedBikeUpDir, Vector3.Cross(vehicleBody.transform.right, normalDir));

		Vector3 LeanTorque = new Vector3(0, 0, 100 * LeanTorqueAmount
			* (-leanAngle / 90)
			* BalanceTorqueCurve.Evaluate(Mathf.Abs(carController.carVelocity.z)));

		if (AlwaysAddTorque)
		{
			vehicleBody.AddRelativeTorque(LeanTorque);
		}
		else if (carController.grounded)
		{
			vehicleBody.AddRelativeTorque(LeanTorque);
		}

	}
}
