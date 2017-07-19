using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class SequenceTween : BaseTween
	{
		[SerializeField]
		public string m_name = "Default Name";

		int m_currentIndex = -1;

		[SerializeField]
		TweenSequenceEntry[] m_tweenSequence = null;

		Action m_endAction;

		public override bool IsTweening()
		{
			return !(m_currentIndex < 0) && m_currentIndex < m_tweenSequence.Length;
		}

		public override void StartTween(Action endAction)
		{
			m_endAction = endAction;
			m_currentIndex = -1;
			FireNextTween();
		}

		void FireNextTween()
		{
			m_currentIndex++;
			if(m_currentIndex >= m_tweenSequence.Length)
			{
				if (m_endAction != null)
					m_endAction();
				return;
			}
			var te = m_tweenSequence[m_currentIndex];
			bool isBlocking = te.IsBlocking;
			Action stepAction = null;
			if (isBlocking)
				stepAction = FireNextTween;
			te.Tween.StartTween(stepAction);
			if (!isBlocking)
				FireNextTween();
		}

		public override void StopTween()
		{
			if (!IsTweening())
				return;
			m_tweenSequence[m_currentIndex].Tween.StopTween();
			m_currentIndex = -1;
			
		}

		public override void SkipToEnd()
		{
			for (int i = 0; i < m_tweenSequence.Length; ++i)
			{
				m_tweenSequence[i].Tween.SkipToEnd();
			}
		}
		
	}

	[Serializable]
	public class TweenSequenceEntry
	{
		public bool IsBlocking = false;
		public BaseTween Tween; 
	}
}
