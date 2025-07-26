using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseWhenStuck : MonoBehaviour
{
    private AiCarContrtoller AiCarContrtoller;
    private Rigidbody rb;
    public float minVelocityToReverse;
    public Transform groundCheck;
    private float speedValue;
    public float reverseTime;
    private float velocity;
    private float timeToStartReverse;
    private float reverseStartTime = 3;


    void Start()
    {
        AiCarContrtoller = GetComponent<AiCarContrtoller>();
        speedValue = AiCarContrtoller.speed * Time.deltaTime * 1000 * AiCarContrtoller.speedCurve.Evaluate(Mathf.Abs(AiCarContrtoller.carVelocity.z) / 100);
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        velocity = rb.velocity.magnitude;

        
        if (velocity < minVelocityToReverse)
        {
            timeToStartReverse += Time.deltaTime;
        }
        else
        {
            timeToStartReverse = 0;
        }

        if(timeToStartReverse > reverseStartTime)
        {
            StartCoroutine(startReverse());
            timeToStartReverse = 0;
        }

    }

    public void TakeReverse()
    {
        AiCarContrtoller.carVelocity = transform.InverseTransformDirection(rb.velocity);
        AiCarContrtoller.tireVisuals();
        rb.AddForceAtPosition(Vector3.ProjectOnPlane((-groundCheck.forward * speedValue/2),Vector3.up), groundCheck.position);
    }
    IEnumerator startReverse()
    {
        //Debug.Log("started");
        float timePassed = 0;
        while (timePassed < reverseTime)
        {
            rb.drag = 5f;
            rb.angularDrag = 5f;
            AiCarContrtoller.enabled = false;
            TakeReverse();

            timePassed += Time.deltaTime;

            yield return null;
        }

        AiCarContrtoller.enabled = true;
        //Debug.Log("ended");
        timeToStartReverse = 0f;
    }

}
