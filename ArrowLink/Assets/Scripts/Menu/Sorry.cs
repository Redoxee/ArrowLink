using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class Sorry : MonoBehaviour {

        [SerializeField]
        BaseUITween m_fakeSpecularTween = null;
        [SerializeField]
        float m_specularPeriod = 1f;
        private float m_specularTimer = 0f;

        private void Update()
        {
            m_specularTimer -= Time.deltaTime;
            if (m_specularTimer <= 0)
            {
                m_fakeSpecularTween.StartTween();
                m_specularTimer = m_specularPeriod;
            }
        }

        public void GoHome()
        {
            MainProcess.Instance.LoadMenuScene();
        }

        public void BuyGame()
        {
            var monetManager = MainProcess.Instance.MonetManager;
            monetManager.UnlockFullGame(OnPurchaseSuccess, OnpurchaseFail);
        }

        private void OnPurchaseSuccess()
        {
            MainProcess.Instance.RequestGameScene();
        }
        private void OnpurchaseFail()
        {
        } 
    }
}