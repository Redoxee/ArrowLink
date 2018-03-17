using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArrowLink;

[CustomEditor(typeof(AchievementPopup))]
public class AchievementPopupInspector : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("generateEntries"))
        {
            ((AchievementPopup)target).GenerateEntries();
        }
    }
}
