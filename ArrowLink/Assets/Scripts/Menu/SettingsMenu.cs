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

        [SerializeField]
        private GameObject m_unlockGameButton = null;

        [SerializeField]
        private GameObject m_restorPurchaseButton = null;

        [SerializeField]
        private GameObject m_cancelIAPbutton = null;

        private void Start()
        {
            m_restorPurchaseButton.SetActive(MonetManager.RestorePurchaseAllowed);
            m_cancelIAPbutton.SetActive(MonetManager.CancelIAPAllowed && MainProcess.Instance.MonetManager.IsGameUnlocked);
            m_unlockGameButton.SetActive(!MainProcess.Instance.MonetManager.IsGameUnlocked);
        }

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

        public void OnBuyPressed()
        {
            MainProcess.Instance.MonetManager.UnlockFullGame(OnBuySuccess, null);
        }

        private void OnBuySuccess()
        {
            MainProcess.Instance.LoadMenuScene();
        }

        public void OnCancelPurchasePressed()
        {
            MainProcess.Instance.MonetManager.CancelIAPPurchase();
        }

        public void OnRestorePurchasePressed()
        {
            MainProcess.Instance.MonetManager.RestorePurchases(OnBuySuccess);
        }
    }
}