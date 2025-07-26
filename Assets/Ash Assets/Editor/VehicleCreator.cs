using UnityEngine;
using UnityEditor;
using System;

public class VehicleCreator : EditorWindow
{

    GameObject preset;
    Transform VehicleBody;
    Transform wheelFL;
    Transform wheelFR;
    Transform wheelRL;
    Transform wheelRR;

    MeshRenderer bodyMesh;
    MeshRenderer wheelMesh;

    private GameObject NewVehicle;


    [MenuItem("Tools/Ash Vehicle Physics/Vehicle Creator")]

    static void OpenWindow()
    {
        VehicleCreator vehicleCreatorWindow = (VehicleCreator)GetWindow(typeof(VehicleCreator));
        vehicleCreatorWindow.minSize = new Vector2(400, 300);
        vehicleCreatorWindow.Show();
    }


    private void OnGUI()
    {
        var style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = Color.green;
        GUILayout.Label("Ash Vehicle Creator", style);
        preset      = EditorGUILayout.ObjectField("Vehicle preset", preset, typeof(GameObject), true) as GameObject;
        GUILayout.Label("Your Vehicle", style);
        VehicleBody = EditorGUILayout.ObjectField("Vehicle Body", VehicleBody, typeof(Transform), true) as Transform;
        wheelFL     = EditorGUILayout.ObjectField("wheel FL", wheelFL, typeof(Transform), true) as Transform;
        wheelFR     = EditorGUILayout.ObjectField("wheel FR", wheelFR, typeof(Transform), true) as Transform;
        wheelRL     = EditorGUILayout.ObjectField("wheel RL", wheelRL, typeof(Transform), true) as Transform;
        wheelRR     = EditorGUILayout.ObjectField("wheel RR", wheelRR, typeof(Transform), true) as Transform;

        if(GUILayout.Button("Create Vehicle"))
        {
            createVehicle();
        }

        bodyMesh = EditorGUILayout.ObjectField("body Mesh", bodyMesh, typeof(MeshRenderer), true) as MeshRenderer;
        wheelMesh = EditorGUILayout.ObjectField("wheel Mesh", wheelMesh, typeof(MeshRenderer), true) as MeshRenderer;

        if (GUILayout.Button("Adjust Colliders"))
        {
            adjustColliders();
        }

    }

    private void adjustColliders()
    {
        if (NewVehicle.GetComponent<BoxCollider>())
        {
            NewVehicle.GetComponent<BoxCollider>().center = Vector3.zero;
            NewVehicle.GetComponent<BoxCollider>().size = bodyMesh.bounds.size;
        }

        if (NewVehicle.GetComponent<CapsuleCollider>())
        {
            NewVehicle.GetComponent<CapsuleCollider>().center = Vector3.zero;
            NewVehicle.GetComponent<CapsuleCollider>().height = bodyMesh.bounds.size.z;
            NewVehicle.GetComponent<CapsuleCollider>().radius = bodyMesh.bounds.size.x/2;

        }

        Vector3 SpheareColliderOffset = new Vector3(wheelMesh.bounds.extents.x, 0, 0);


        if (NewVehicle.transform.Find("wheels").Find("FL rb"))
        {
            NewVehicle.transform.Find("wheels").Find("FL rb").GetComponent<SphereCollider>().radius = wheelMesh.bounds.extents.y;
            NewVehicle.transform.Find("wheels").Find("FL rb").GetComponent<SphereCollider>().center = SpheareColliderOffset;
        }
        if (NewVehicle.transform.Find("wheels").Find("FR rb"))
        {
            NewVehicle.transform.Find("wheels").Find("FR rb").GetComponent<SphereCollider>().radius = wheelMesh.bounds.extents.y;
            NewVehicle.transform.Find("wheels").Find("FR rb").GetComponent<SphereCollider>().center = -SpheareColliderOffset;
        }
        if (NewVehicle.transform.Find("wheels").Find("RL rb"))
        {
            NewVehicle.transform.Find("wheels").Find("RL rb").GetComponent<SphereCollider>().radius = wheelMesh.bounds.extents.y;
            NewVehicle.transform.Find("wheels").Find("RL rb").GetComponent<SphereCollider>().center = SpheareColliderOffset;
        }
        if (NewVehicle.transform.Find("wheels").Find("RR rb"))
        {
            NewVehicle.transform.Find("wheels").Find("RR rb").GetComponent<SphereCollider>().radius = wheelMesh.bounds.extents.y;
            NewVehicle.transform.Find("wheels").Find("RR rb").GetComponent<SphereCollider>().center = -SpheareColliderOffset;
        }
        
    }

    private void createVehicle()
    {
        NewVehicle = Instantiate(preset, VehicleBody.position, VehicleBody.rotation);

        GameObject.DestroyImmediate(NewVehicle.transform.Find("body").Find("mesh body").GetChild(0).gameObject);
        if (NewVehicle.transform.Find("wheels").Find("FL rb"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("wheels").Find("FL rb").Find("FL").Find("Wmesh.FL").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("wheels").Find("FR rb"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("wheels").Find("FR rb").Find("FR").Find("Wmesh.FR").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("wheels").Find("RL rb"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("wheels").Find("RL rb").Find("RL").Find("Wmesh.RL").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("wheels").Find("RR rb"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("wheels").Find("RR rb").Find("RR").Find("Wmesh.RR").GetChild(0).gameObject);
        }

        VehicleBody.parent = NewVehicle.transform.Find("body").Find("mesh body");
        VehicleBody.localPosition = Vector3.zero;
        NewVehicle.transform.Find("wheels").position = VehicleBody.position;

        if (NewVehicle.transform.Find("wheels").Find("FL rb"))
        {
            NewVehicle.transform.Find("wheels").Find("FL rb").position = wheelFL.position;
            wheelFL.parent = NewVehicle.transform.Find("wheels").Find("FL rb").Find("FL").Find("Wmesh.FL");
        }
        if (NewVehicle.transform.Find("wheels").Find("FR rb"))
        {
            NewVehicle.transform.Find("wheels").Find("FR rb").position = wheelFR.position;
            wheelFR.parent = NewVehicle.transform.Find("wheels").Find("FR rb").Find("FR").Find("Wmesh.FR");
        }
        if (NewVehicle.transform.Find("wheels").Find("RL rb"))
        {
            NewVehicle.transform.Find("wheels").Find("RL rb").position = wheelRL.position;
            wheelRL.parent = NewVehicle.transform.Find("wheels").Find("RL rb").Find("RL").Find("Wmesh.RL");
        }
        if (NewVehicle.transform.Find("wheels").Find("RR rb"))
        {
            NewVehicle.transform.Find("wheels").Find("RR rb").position = wheelRR.position;
            wheelRR.parent = NewVehicle.transform.Find("wheels").Find("RR rb").Find("RR").Find("Wmesh.RR");
        }


    }


}
