using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class BoardSlot : MonoBehaviour
	{
		[SerializeField]
		SingleTween m_focusTween = null;
		//TweenParameters m_focusParameters;
		[SerializeField]
		SingleTween m_unfocusTween = null;
		//TweenParameters m_unfocusParameters;

		public int X, Y;

		private void Awake()
		{
			//m_focusParameters = m_focusTween.m_parameters;
			//m_unfocusParameters = m_unfocusTween.m_parameters;
		}

		public void OnFocus()
		{
			//m_focusParameters.AlphaStart = m_veilMaterial.color.a;
			m_unfocusTween.StopTween();
			m_focusTween.StartTween(null);
		}
		public void OnUnfocus()
		{
			//m_unfocusParameters.AlphaStart = m_veilMaterial.color.a;
			m_focusTween.StopTween();
			m_unfocusTween.StartTween(null);
		}
	}
}