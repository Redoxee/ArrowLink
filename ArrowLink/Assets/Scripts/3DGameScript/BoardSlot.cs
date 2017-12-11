using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class BoardSlot : MonoBehaviour
	{
		[SerializeField]
		SingleTween m_focusTween = null;
		[SerializeField]
		SingleTween m_unfocusTween = null;
        [SerializeField]
        ParticleSystem m_flashParticles = null;

		public int X, Y;
        

		public void OnFocus()
		{
			m_unfocusTween.StopTween();
			m_focusTween.StartTween(null);
		}

		public void OnUnfocus()
		{
			m_focusTween.StopTween();
			m_unfocusTween.StartTween(null);
		}

        public void Flash()
        {
            m_flashParticles.Play();
        }

        private void OnMouseEnter()
        {
            OnFocus();
        }

        private void OnMouseExit()
        {
            OnUnfocus();
        }

        private void OnMouseUpAsButton()
        {
            OnUnfocus();
            GameProcess.Instance.OnTilePressed(this);
        }
    }
}