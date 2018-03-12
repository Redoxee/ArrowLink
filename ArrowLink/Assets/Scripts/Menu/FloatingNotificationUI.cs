using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class FloatingNotificationUI : MonoBehaviour
    {
        [SerializeField]
        BaseUITween m_dailyTween = null;

        [SerializeField]
        SimpleFloatingNotification m_floatingMessage = null;

        private class WaitingMessage
        {
            public string Title;
            public string Subtitle;
            public Action OnEnd;
        }

        private List<WaitingMessage> m_pendingMessages = new List<WaitingMessage>();

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

        public void ShowFloatingMessage(string title, string subtitle, Action onEnd = null)
        {
            m_pendingMessages.Add(new WaitingMessage { Title = title,  Subtitle = subtitle, OnEnd = onEnd});
            if (!m_floatingMessage.IsDisplayingAMessage())
            {
                DisplayNextMessageInQueue();
            }
        }

        private void DisplayNextMessageInQueue()
        {
            WaitingMessage message = m_pendingMessages[0];
            m_floatingMessage.ShowMessage(message.Title, message.Subtitle, _OnMessageDisplayEnd);
        }

        private void _OnMessageDisplayEnd()
        {
            WaitingMessage message = m_pendingMessages[0];
            m_pendingMessages.RemoveAt(0);
            if (message.OnEnd != null)
                message.OnEnd();
            if (m_pendingMessages.Count > 0)
            {
                DisplayNextMessageInQueue();
            }
        }
    }
}