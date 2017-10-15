using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArrowLink;

[CustomEditor(typeof(ParticlesHolder))]
public class CustomInspectorParticleHolder : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Apply model"))
        {
            ((ParticlesHolder)target).ApplyModel() ;
        }

    }
}
