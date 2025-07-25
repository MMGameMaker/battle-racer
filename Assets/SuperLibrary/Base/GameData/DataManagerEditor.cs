#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// This is sample, DO NOT MODIFY IT -> Duplicate then change namespace new game ex: Yogame.DancingBall
/// </summary>
[ExecuteInEditMode]
[CustomEditor(typeof(DataManager))]
#pragma warning disable CS0618 // Type or member is obsolete
public class DataManagerEditor : Editor, IPreprocessBuild
#pragma warning restore CS0618 // Type or member is obsolete
{
    public int callbackOrder { get { return 0; } }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var data = (DataManager)target;

        if (GUILayout.Button("Load"))
        {
            DataManager.Load();
        }

        if (GUILayout.Button("Save"))
        {
            DataManager.Save();
        }

        if (GUILayout.Button("Reset"))
        {
            data.Reset();
        }
        if (GUILayout.Button("RESET + UPDATE data to BUILD"))
        {
            if (data != null)
                data.ResetAndUpdateData();
        }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        Debug.Log("OnPreprocessBuild: " + target.ToString());
    }
}
#endif