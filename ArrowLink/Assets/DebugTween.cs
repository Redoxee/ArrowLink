using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTween : MonoBehaviour {

	public BaseUITween m_tween = null;
	public void StartTween()
	{
		m_tween.StartTween();
	}
}