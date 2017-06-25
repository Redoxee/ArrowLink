using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUITween : MonoBehaviour {

	public string m_name = "UITween";


	public abstract void StartTween(Action onTweenEnd = null);

	public abstract bool isAnimating();
}
