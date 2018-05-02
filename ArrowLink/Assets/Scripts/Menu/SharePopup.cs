using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SharePopup : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup m_canvasGroup = null;

        public void Show()
        {
            m_canvasGroup.alpha = 1f;
            m_canvasGroup.interactable = true;
            m_canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            m_canvasGroup.alpha = 0f;
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }
    }
}