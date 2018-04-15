using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArrowLink;

[CustomEditor(typeof(ColorCollection))]
public class ColorCollectionInspector : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(16);
        if (GUILayout.Button("Apply"))
        {
            var collection = ((ColorCollection)target);

            var grabbers = FindObjectsOfType<ColorGrabber>();
            foreach (var grabber in grabbers)
            {
                grabber.ApplyColor(collection);
            }
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
}
