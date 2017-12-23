using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	bool m_isInitialized = false;
	void Initialize()
	{
		if (m_isInitialized)
			return;

		if (m_parameters.isPosition)
			m_parameters.PositionDelta = m_parameters.PositionEnd - m_parameters.PositionStart;
		if (m_parameters.isScale)
			m_parameters.ScaleDelta = m_parameters.ScaleEnd - m_parameters.ScaleStart;
		if (m_parameters.isAlpha)
		{
			m_parameters.AlphaDelta = m_parameters.AlphaEnd - m_parameters.AlphaStart;
			if (m_parameters.isCanvasGroup)
				m_parameters.CanvasGroup = transform.GetComponent<CanvasGroup>();
			else
			{
				m_parameters.Image = transform.GetComponent<Image>();
				m_parameters.ImageColor = m_parameters.Image.color;
			}
		}
	}

	public override void StartTween(Action onTweendEnd = null)
	{
		Initialize();
		m_endTweenAction = onTweendEnd;
		m_timer = 0f;
		enabled = true;
        ApplyTween(0f);
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

		ApplyTween(progression);

		if (m_timer > m_parameters.Duration)
		{
			if(m_endTweenAction != null)
				m_endTweenAction();
			enabled = false;
			return;
		}
	}

	void ApplyTween(float progression)
	{

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

		if (m_parameters.isAlpha)
		{
			var alpha = m_parameters.AlphaStart + m_parameters.AlphaCurve.Evaluate(progression) * m_parameters.AlphaDelta;
			if (m_parameters.isCanvasGroup)
			{
				m_parameters.CanvasGroup.alpha = alpha;
			}
			else
			{

				m_parameters.ImageColor.a = alpha;
				m_parameters.Image.color = m_parameters.ImageColor;
			}
		}
	}

	public override UITweenParameters[] GetParameters ()
	{
		return new UITweenParameters[]{ m_parameters };
	}

	public override void Stop(bool fireEvents)
	{
		if (!isAnimating())
			return;
		m_timer = m_parameters.Duration + 1;
		if (m_endTweenAction != null && fireEvents)
			m_endTweenAction();
		enabled = false;
	}

	public override void SkipToEnd()
	{
		Initialize();
		ApplyTween(1);
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


	[Header("Alpha")]
	public bool isAlpha;
	public bool isCanvasGroup;
	public float AlphaStart;
	public float AlphaEnd;
	[NonSerialized]
	public float AlphaDelta;
	public AnimationCurve AlphaCurve;

	[NonSerialized]
	public CanvasGroup CanvasGroup;
	[NonSerialized]
	public Image Image;
	[NonSerialized]
	public Color ImageColor;
}