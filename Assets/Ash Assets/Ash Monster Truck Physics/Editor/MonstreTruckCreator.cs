using UnityEngine;
using UnityEditor;
using System;

public class MonstreTruckCreator : EditorWindow
{

    GameObject preset;
    Transform VehicleBody;
    Transform wheelFL;
    Transform wheelFR;
    Transform wheelRL;
    Transform wheelRR;
    Transform AxelFront;
    Transform AxelBack;

    MeshRenderer bodyMesh;
    MeshRenderer wheelMesh;

    private GameObject NewVehicle;


    [MenuItem("Tools/Ash Monster Truck Physics")]

    static void OpenWindow()
    {
        MonstreTruckCreator vehicleCreatorWindow = (MonstreTruckCreator)GetWindow(typeof(MonstreTruckCreator));
        vehicleCreatorWindow.minSize = new Vector2(400, 300);
        vehicleCreatorWindow.Show();
    }

    private void OnGUI()
    {
        var style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = Color.green;
        GUILayout.Label("Monster Truck Creator", style);
        preset      = EditorGUILayout.ObjectField("Monster Truck Preset", preset, typeof(GameObject), true) as GameObject;
        GUILayout.Label("Your Monster Truck", style);
        VehicleBody = EditorGUILayout.ObjectField("Monster Truck Body", VehicleBody, typeof(Transform), true) as Transform;
        GUILayout.Label("Wheels", style);
        wheelFL     = EditorGUILayout.ObjectField("wheel FL", wheelFL, typeof(Transform), true) as Transform;
        wheelFR     = EditorGUILayout.ObjectField("wheel FR", wheelFR, typeof(Transform), true) as Transform;
        wheelRL     = EditorGUILayout.ObjectField("wheel RL", wheelRL, typeof(Transform), true) as Transform;
        wheelRR     = EditorGUILayout.ObjectField("wheel RR", wheelRR, typeof(Transform), true) as Transform;
        GUILayout.Label("Axels", style);
        AxelFront   = EditorGUILayout.ObjectField("Axel Front", AxelFront, typeof(Transform), true) as Transform;
        AxelBack    = EditorGUILayout.ObjectField("Axel Back", AxelBack, typeof(Transform), true) as Transform;
        GUILayout.Label("Meshes", style);
        bodyMesh = EditorGUILayout.ObjectField("Body Mesh", bodyMesh, typeof(MeshRenderer), true) as MeshRenderer;
        wheelMesh = EditorGUILayout.ObjectField("Wheel Mesh", wheelMesh, typeof(MeshRenderer), true) as MeshRenderer;

        if (GUILayout.Button("Create Monster Truck"))
        {
            createVehicle();
        }

    }


    private void createVehicle()
    {
        NewVehicle = Instantiate(preset, bodyMesh.bounds.center- VehicleBody.transform.up * bodyMesh.bounds.extents.y, VehicleBody.rotation);
        NewVehicle.transform.Find("Body Collider").position = bodyMesh.bounds.center;
        NewVehicle.transform.Find("Body Collider").GetComponent<BoxCollider>().size = bodyMesh.bounds.size;
        GameObject.DestroyImmediate(NewVehicle.transform.Find("Body").GetChild(0).gameObject);
        if (NewVehicle.transform.Find("Wheels").Find("wheelFL"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheelFL").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelFR"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheelFR").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelRL"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheelRL").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelRR"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheelRR").GetChild(0).gameObject);
        }

        //Axels
        if (NewVehicle.transform.Find("Axels").Find("Axel Front"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Axels").Find("Axel Front").GetChild(0).gameObject);
        }
        if (NewVehicle.transform.Find("Axels").Find("Axel Back"))
        {
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Axels").Find("Axel Back").GetChild(0).gameObject);
        }


        VehicleBody.parent = NewVehicle.transform.Find("Body");
        NewVehicle.transform.Find("Wheels").position = VehicleBody.position;
        NewVehicle.transform.Find("Axels").position = VehicleBody.position;

        //Axels
        if (NewVehicle.transform.Find("Axels").Find("Axel Front"))
        {
            if (AxelFront)
            {
                NewVehicle.transform.Find("Axels").Find("Axel Front").position = AxelFront.position;
                AxelFront.parent = NewVehicle.transform.Find("Axels").Find("Axel Front");
            }
            else
            {
                NewVehicle.transform.Find("Axels").Find("Axel Front").position = (wheelFL.position + wheelFR.position) / 2;
            }
            
        }
        if (NewVehicle.transform.Find("Axels").Find("Axel Back"))
        {
            if (AxelBack)
            {
                NewVehicle.transform.Find("Axels").Find("Axel Back").position = AxelBack.position;
                AxelBack.parent = NewVehicle.transform.Find("Axels").Find("Axel Back");
            }
            else
            {
                NewVehicle.transform.Find("Axels").Find("Axel Back").position = (wheelRL.position + wheelRR.position) / 2;
            }
           
        }

        //wheels
        if (NewVehicle.transform.Find("Wheels").Find("wheelFL"))
        {
            NewVehicle.transform.Find("Wheels").Find("wheelFL").position = wheelFL.position;
            wheelFL.parent = NewVehicle.transform.Find("Wheels").Find("wheelFL");
            wheelFL.SetSiblingIndex(0);
            NewVehicle.transform.Find("Wheels").Find("wheelFL").GetComponent<SphereCollider>().radius = wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Wheels").Find("wheelFL").Find("Smoke").localPosition = Vector3.zero + Vector3.down * wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Suspensions").Find("SuspensionFL").position = wheelFL.position + wheelFL.right * wheelMesh.bounds.size.x / 4;
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelFR"))
        {
            NewVehicle.transform.Find("Wheels").Find("wheelFR").position = wheelFR.position;
            wheelFR.parent = NewVehicle.transform.Find("Wheels").Find("wheelFR");
            wheelFR.SetSiblingIndex(0);
            NewVehicle.transform.Find("Wheels").Find("wheelFR").GetComponent<SphereCollider>().radius = wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Wheels").Find("wheelFR").Find("Smoke").localPosition = Vector3.zero + Vector3.down * wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Suspensions").Find("SuspensionFR").position = wheelFR.position - wheelFR.right * wheelMesh.bounds.size.x / 4;
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelRL"))
        {
            NewVehicle.transform.Find("Wheels").Find("wheelRL").position = wheelRL.position;
            wheelRL.parent = NewVehicle.transform.Find("Wheels").Find("wheelRL");
            wheelRL.SetSiblingIndex(0);
            NewVehicle.transform.Find("Wheels").Find("wheelRL").GetComponent<SphereCollider>().radius = wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Wheels").Find("wheelRL").Find("Smoke").localPosition = Vector3.zero + Vector3.down * wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Suspensions").Find("SuspensionRL").position = wheelRL.position + wheelRL.right * wheelMesh.bounds.size.x / 4;
        }
        if (NewVehicle.transform.Find("Wheels").Find("wheelRR"))
        {
            NewVehicle.transform.Find("Wheels").Find("wheelRR").position = wheelRR.position;
            wheelRR.parent = NewVehicle.transform.Find("Wheels").Find("wheelRR");
            wheelRR.SetSiblingIndex(0);
            NewVehicle.transform.Find("Wheels").Find("wheelRR").GetComponent<SphereCollider>().radius = wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Wheels").Find("wheelRR").Find("Smoke").localPosition = Vector3.zero + Vector3.down * wheelMesh.bounds.size.y / 2;
            NewVehicle.transform.Find("Suspensions").Find("SuspensionRR").position = wheelRR.position - wheelRR.right * wheelMesh.bounds.size.x / 4;
        }


        NewVehicle.GetComponent<MonsterTruckController>().Raydistance = 
            Mathf.Abs(NewVehicle.transform.position.y - wheelMesh.transform.position.y) + wheelMesh.bounds.size.y/2;

        NewVehicle.GetComponent<MonsterTruckController>().skidWidth = wheelMesh.bounds.size.x / 2;

        NewVehicle.transform.Find("COM").position =
            NewVehicle.transform.position - NewVehicle.transform.up * Mathf.Abs(NewVehicle.transform.position.y - wheelMesh.transform.position.y);



    }


}
