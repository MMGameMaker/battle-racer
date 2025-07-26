using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSkid : MonoBehaviour 
{
	public Rigidbody rb;
	public Skidmarks skidmarks;
	private SphereCollider sphereCollider;
	public TireSmoke smoke;
	public AudioSource skidSound;
	public AudioSource tireImpact;


	private Rigidbody wheel;

	private const float MaxSkidIntensity = 20.0f; 
	private int lastSkid = -1; 
	private float lastFixedUpdateTime;


	protected void Awake() 
	{
		lastFixedUpdateTime = Time.time;
		wheel = GetComponent<Rigidbody>();
		sphereCollider = GetComponent<SphereCollider>();
		skidSound.mute = true;
		skidmarks = FindObjectOfType<Skidmarks>();
	}

	protected void FixedUpdate() 
	{
		lastFixedUpdateTime = Time.time;
	}


    private void OnCollisionStay(Collision collision)
    {
		float wheelAngularVelocity = Mathf.Abs(transform.InverseTransformDirection( wheel.angularVelocity).x);
		float wheelForwardVel = Mathf.Abs( Vector3.Dot(transform.InverseTransformDirection(wheel.velocity), rb.transform.forward));
		float radius = sphereCollider.bounds.extents.y;

		float skidTotal = Mathf.Abs(wheelAngularVelocity* radius - transform.InverseTransformDirection(wheel.velocity).magnitude);


		if (wheelAngularVelocity* sphereCollider.bounds.extents.x > wheelForwardVel + 1 )
		{
		    skidSound.mute = false;
			float intensity = Mathf.Clamp01(skidTotal / MaxSkidIntensity);
		    skidSound.volume = intensity/3;

			Vector3 skidPoint = collision.contacts[0].point + (rb.velocity * (Time.time - lastFixedUpdateTime));
            if (!collision.rigidbody)
            {
		    	lastSkid = skidmarks.AddSkidMark(skidPoint, collision.contacts[0].normal, intensity, lastSkid);
		    }
		    	
            if (smoke && intensity > 0.5f)
            {
		    	smoke.playSmoke();
            }
		    else if(smoke)
		    {
		    	smoke.stopSmoke();
		    }
		}
		else if (wheelAngularVelocity * sphereCollider.bounds.extents.x < wheelForwardVel-1)
        {
		    skidSound.mute = false;
		    float intensity = Mathf.Clamp01(skidTotal / MaxSkidIntensity);
		    skidSound.volume = intensity / 3;

		    Vector3 skidPoint = collision.contacts[0].point + (rb.velocity * (Time.time - lastFixedUpdateTime));

		    if (!collision.rigidbody)
            {
		    	lastSkid = skidmarks.AddSkidMark(skidPoint, collision.contacts[0].normal, intensity, lastSkid);
		    }
		        
		    if (smoke && intensity > 0.5f)
		    {
		    	smoke.playSmoke();
		    }
		    else if (smoke)
		    {
		    	smoke.stopSmoke();
		    }

		}
		else
		{
		    skidSound.mute = true;
			lastSkid = -1;
		    if (smoke)
		    {
		    	smoke.stopSmoke();
		    }
		}
		
	}
    private void OnCollisionExit(Collision collision)
    {
		skidSound.mute = true;
		if (smoke)
		{
			smoke.stopSmoke();
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
		tireImpact.Play();

	}

}
