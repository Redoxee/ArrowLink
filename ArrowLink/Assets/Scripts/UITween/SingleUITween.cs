using System;
using System.Collections.Generic;
using UnityEngine;

public class SingleUITween : BaseUITween {

	[SerializeField]
	UITweenParameters m_parameters = null;
	public UITweenParameters Parameters { get { return m_parameters; } }

	Action m_endTweenAction;

	float m_timer = float.MaxValue / 2f;

	RectTransform m_rect;

	private void Awake()
	{
		m_rect = transform as RectTransform;
	}

	public override bool isAnimating()
	{
		return enabled;
	}

	#region MonoBehaviour

	private void Update()
	{
		UpdateTween();
	}

	#endregion

	public override void StartTween(Action onTweendEnd = null)
	{
		m_endTweenAction = onTweendEnd;
		m_timer = 0f;
		enabled = true;

		if (m_parameters.isPosition)
			m_parameters.PositionDelta = m_parameters.PositionEnd - m_parameters.PositionStart;
		if (m_parameters.isScale)
			m_parameters.ScaleDelta = m_parameters.ScaleEnd - m_parameters.ScaleStart;
	}

	public void UpdateTween()
	{
		if (m_timer > m_parameters.Duration)
		{
			enabled = false;
			return;
		}

		m_timer += Time.deltaTime;
		float progression = Mathf.Clamp01(m_timer / m_parameters.Duration);

		if (m_parameters.isPosition)
		{
			transform.position = m_parameters.PositionCurve.Evaluate(progression) * m_parameters.PositionDelta + m_parameters.PositionStart;
		}

		if (m_parameters.isScale)
		{
			var scale = m_parameters.ScaleStart + m_parameters.ScaleDelta * m_parameters.ScaleCurve.Evaluate(progression);
			m_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scale.x);
			m_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scale.y);
		}

		if (m_timer > m_parameters.Duration)
		{
			if(m_endTweenAction != null)
				m_endTweenAction();
			enabled = false;
			return;
		}
	}
}

[Serializable]
public class UITweenParameters
{
	public float Duration;

	[Header("Position")]
	public bool isPosition;
	public Vector3 PositionStart;
	public Vector3 PositionEnd;
	[NonSerialized]
	public Vector3 PositionDelta;
	public AnimationCurve PositionCurve;

	[Header("Scale")]
	public bool isScale;
	public Vector2 ScaleStart;
	public Vector2 ScaleEnd;
	[NonSerialized]
	public Vector2 ScaleDelta;
	public AnimationCurve ScaleCurve;
}