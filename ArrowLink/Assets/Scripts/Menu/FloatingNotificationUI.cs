using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class FloatingNotificationUI : MonoBehaviour
    {
        [SerializeField]
        BaseUITween m_dailyTween = null;

        private void Start()
        {
        }

        public void ShowDailyTween()
        {
            m_dailyTween.gameObject.SetActive(true);
            m_dailyTween.StartTween(_onDailyTweenOver);
        }

        private void _onDailyTweenOver()
        {
            m_dailyTween.gameObject.SetActive(false);
        }
    }
}