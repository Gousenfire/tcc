using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NuclearThrone))]
[CanEditMultipleObjects]
public class NuclearThroneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PCGAlgorithm myScript = (PCGAlgorithm)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Cave"))
        {
            myScript.GenerateCave();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Cave"))
        {
            myScript.ClearCave();
        }
    }
}
