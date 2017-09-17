using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class SingleTween : BaseTween
	{
		[SerializeField]
		public string m_name = "Default Name";

		[SerializeField]
		public TweenParameters m_parameters;

		Action m_endAction = null;

		float m_timer = -1;

		#region MonoBehaviour
		private void Awake()
		{
			m_parameters.Initialize(this.gameObject);
		}

		private void Update()
		{
			if (!IsTweening())
			{
				StopTween();
				return;
			}

			m_timer += Time.deltaTime;
			float progression = m_timer / m_parameters.Duration;
			ApplyTween(Mathf.Clamp01(progression));

			if (m_timer > m_parameters.Duration)
			{
				StopTween();
				if (m_endAction != null)
					m_endAction();
			}	
		}
		#endregion

		public override bool IsTweening()
		{
			return !(m_timer < 0);
		}

		public override void StartTween(Action endAction = null)
		{
			m_endAction = endAction;
			m_parameters.ComputeDeltas();
			m_timer = 0;
			enabled = true;
		}

		void ApplyTween(float progression)
		{
			if (m_parameters.IsPosition)
			{
				var pos = m_parameters.PositionStart + m_parameters.PositionCurve.Evaluate(progression) * m_parameters.PositionDelta;
				if (m_parameters.IsLocalPosition)
					m_parameters.Target.localPosition = pos;
				else
					m_parameters.Target.position = pos;
			}

			if (m_parameters.IsScale)
			{
				var sca = m_parameters.ScaleStart + m_parameters.ScaleCurve.Evaluate(progression) * m_parameters.ScaleDelta;
				m_parameters.Target.localScale = sca;
			}

			if (m_parameters.IsAlpha)
			{
				var alpha = m_parameters.AlphaStart + m_parameters.AlphaCurve.Evaluate(progression) * m_parameters.AlphaDelta;
				m_parameters.BaseColor.a = alpha;
				m_parameters.SpriteRenderer.color = m_parameters.BaseColor;
			}

            if (m_parameters.IsRotation)
            {
                var rotation = m_parameters.AngleStart + m_parameters.AngleCurve.Evaluate(progression) * m_parameters.AngleDelta;
                m_parameters.Target.localRotation = Quaternion.AngleAxis(rotation, Vector3.forward);
            }
		}

		public override void SkipToEnd()
		{
			StopTween();
			ApplyTween(1);
		}
		
		public override void StopTween()
		{
			m_timer = -1;
			enabled = false;
		}
	}
}
