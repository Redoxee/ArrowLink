using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArrowLink;

[CustomEditor(typeof(ColorGrabber))]
public class ColorGrabberInspector : Editor {
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ColorGrabber)target), typeof(ColorGrabber), false);
        GUI.enabled = true;

        var sp = serializedObject.FindProperty("m_colorToGrab");
        EditorGUILayout.PropertyField(sp);
        sp = serializedObject.FindProperty("m_mode");
        EditorGUILayout.PropertyField(sp);
        var mode = (ColorGrabber.GrabMode)sp.enumValueIndex;
        if (mode == ColorGrabber.GrabMode.Custom)
        {
            sp = serializedObject.FindProperty("m_customGrabber");
            EditorGUILayout.PropertyField(sp);
        }
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Apply Colors"))
        {
            var manager = GameObject.FindObjectOfType<ColorManager>();
            if (manager != null)
            {
                ((ColorGrabber)target).ApplyColor(manager.ColorCollection);
            }
        }
    }
}
