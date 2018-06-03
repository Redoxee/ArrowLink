using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Arc))]
public class ArcInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(16);
        if (GUILayout.Button("Build"))
        {
            var arc = (Arc)target;
            arc.BuildArc();
        }
    }
}
