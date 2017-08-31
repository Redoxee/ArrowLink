using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrabbableParticles))]
public class GrabbableParticleInspector : Editor {

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Start particles"))
		{
			StartParticles();
		}
	}

	private void StartParticles()
	{
		var t = (GrabbableParticles)target;
		t.StartEffect();
	}
}
