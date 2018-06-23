using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup m_canvasGroup = null;

        [SerializeField]
        private BasicPopup m_moreGames = null;

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

        public void OnFeedBackButtonPressed()
        {
            MainProcess.SimpleFeedback();
        }

        public void OnMoreGamesPressed()
        {
            m_moreGames.Show();
        }
    }
}