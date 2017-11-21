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
            GameProcess.Instance.OnTilePressed(this);
        }
    }
}