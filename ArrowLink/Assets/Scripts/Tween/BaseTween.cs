using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink

{
	public abstract class BaseTween: MonoBehaviour
	{
		public abstract void StartTween(Action endAction = null);
		public abstract bool IsTweening();
		public abstract void StopTween();
		public abstract void SkipToEnd();
		
	}

	[Serializable]
	public class TweenParameters
	{
		[SerializeField]
		GameObject m_optionalTarget= null;
		[NonSerialized]
		public Transform Target;

		public float Duration;

		[Header("Position")]
		public bool IsPosition = false;
		public bool IsLocalPosition = false;
		public Vector3 PositionStart;
		public Vector3 PositionEnd;
		[NonSerialized]
		public Vector3 PositionDelta;
		public AnimationCurve PositionCurve;

		[Header("Scale")]
		public bool IsScale = false;
		public Vector3 ScaleStart;
		public Vector3 ScaleEnd;
		[NonSerialized]
		public Vector3 ScaleDelta;
		public AnimationCurve ScaleCurve;

		[Header("Alpha")]
		public bool IsAlpha = false;
		public float AlphaStart;
		public float AlphaEnd;
		public AnimationCurve AlphaCurve;
		[NonSerialized]
		public float AlphaDelta;
		[NonSerialized]
		public SpriteRenderer SpriteRenderer;
		[NonSerialized]
		public Color BaseColor;

		bool m_isInitialized = false;
		public void Initialize(GameObject holder)
		{
			if (m_isInitialized)
				return;
			if (m_optionalTarget != null)
				Target = m_optionalTarget.transform;
			else
				Target = holder.transform;
			if (IsAlpha)
				SpriteRenderer = Target.GetComponent<SpriteRenderer>();
		}

		public void ComputeDeltas()
		{
			if (IsPosition)
				PositionDelta = PositionEnd - PositionStart;
			if (IsScale)
				ScaleDelta = ScaleEnd - ScaleStart;
			if (IsAlpha)
			{
				AlphaDelta = AlphaEnd - AlphaStart;
				BaseColor = SpriteRenderer.color;
			}
		}
	}
}