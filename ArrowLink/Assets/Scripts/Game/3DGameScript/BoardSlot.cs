#define NO_SHADE_PREVIEW

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
        SingleTween m_flashTween = null;

        [SerializeField]
        BoxCollider2D m_box2D = null;

        private bool m_isFlashing = false;
        public bool IsFlashing
        {
            get
            {
                return m_isFlashing;
            }
            set
            {
                m_isFlashing = value;
                if (value && !m_flashTween.IsTweening())
                {
                    m_flashTween.StartTween(OnFlashEnd);
                }
            }
        }
        public void Flash()
        {
            if (!m_isFlashing)
                m_flashTween.StartTween(OnFlashEnd);
        }

		public int X, Y;
        

		public void OnFocus()
		{
#if !NO_SHADE_PREVIEW
            m_unfocusTween.StopTween();
			m_focusTween.StartTween(null);
#endif
		}

		public void OnUnfocus()
		{
#if !NO_SHADE_PREVIEW
            m_focusTween.StopTween();
			m_unfocusTween.StartTween(null);
#endif
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

        private void OnFlashEnd()
        {
            if (m_isFlashing)
                m_flashTween.StartTween(OnFlashEnd);
        }

        public bool IsPositionIn(Vector2 pos)
        {
            return m_box2D.OverlapPoint(pos);
        }
    }
}