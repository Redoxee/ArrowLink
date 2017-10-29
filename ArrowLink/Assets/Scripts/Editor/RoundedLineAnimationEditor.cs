using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoundedLineAnimation))]
public class RoundedLineAnimationInspector : Editor{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RoundedLineAnimation cTarget = (RoundedLineAnimation)target;

        if (GUILayout.Button("Compute points"))
        {
            cTarget.ComputePoints();
        }

        GUI.enabled = cTarget.IsReady();

        if (GUILayout.Button("Draw line"))
        {
            cTarget.SetLineRendererFull();
        }
        if (GUILayout.Button("StartAnimation"))
        {
            cTarget.StartAnimation();
        }
        GUI.enabled = true;
    }
}
