using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionJoints : MonoBehaviour
{
    public float suspensionForce = 600;
    public float damperForce = 10;
    public float anchorOffset = 0.4f;
    public ConfigurableJoint[] Suspension_Cj = new ConfigurableJoint[4];
    
    private void Awake()
    {
        foreach (ConfigurableJoint Cj in Suspension_Cj)
        {
            var ydrive = Cj.yDrive;
            ydrive.positionSpring = suspensionForce;
            ydrive.positionDamper = damperForce;
            Cj.yDrive = ydrive;

            var Angular_xdrive = Cj.angularXDrive;
            Angular_xdrive.positionSpring = suspensionForce*2;
            Angular_xdrive.positionDamper = damperForce;
            Cj.angularXDrive = Angular_xdrive;


        }
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Vector3 offset = new Vector3(anchorOffset, 0, 0);
            Suspension_Cj[0].anchor = -offset;
            Suspension_Cj[1].anchor = offset;
            Suspension_Cj[2].anchor = -offset;
            Suspension_Cj[3].anchor = offset;
        }

        foreach (ConfigurableJoint Cj in Suspension_Cj)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Cj.transform.position + Cj.transform.right*Cj.anchor.x, 0.2f);
        }

    }
}
