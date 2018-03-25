using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class FloatingNotificationUI : MonoBehaviour
    {
        [SerializeField]
        SimpleFloatingNotification[] m_floatingMessages = null;

        private class WaitingMessage
        {
            public string Title;
            public string Subtitle;
            public Action OnEnd;
            public bool IsBeeingDisplayed;
        }

        private List<WaitingMessage> m_pendingMessages = new List<WaitingMessage>();

        public void AddMessageToQueue(string title, string subtitle, Action onEnd = null)
        {
            m_pendingMessages.Add(new WaitingMessage { Title = title, Subtitle = subtitle, OnEnd = onEnd });
        }

        public void ShowFloatingMessage(string title, string subtitle, Action onEnd = null)
        {
            AddMessageToQueue(title, subtitle, onEnd);
            DisplayNextMessagesInQueue();
            
        }

        public const float c_delayBetweenMesages = .25f;
        public void DisplayNextMessagesInQueue()
        {
            int firstNonDisplayedMessage = 0;
            for (firstNonDisplayedMessage = 0; firstNonDisplayedMessage < m_pendingMessages.Count; ++firstNonDisplayedMessage)
            {
                if (!m_pendingMessages[firstNonDisplayedMessage].IsBeeingDisplayed)
                    break;
            }
            if (firstNonDisplayedMessage == m_pendingMessages.Count)
                return;

            int displayedMessage = 0;
            for (int i = 0; i < m_floatingMessages.Length; ++i)
            {
                if (!m_floatingMessages[i].IsDisplayingAMessage())
                {
                    WaitingMessage message = m_pendingMessages[firstNonDisplayedMessage + displayedMessage];
                    float delay = c_delayBetweenMesages * displayedMessage;
                    m_floatingMessages[i].ShowMessage(message.Title, message.Subtitle, delay, _OnMessageDisplayEnd);
                    message.IsBeeingDisplayed = true;
                    displayedMessage++;

                    if (firstNonDisplayedMessage + displayedMessage >= m_pendingMessages.Count)
                        break;
                }
            }
        }

        private SimpleFloatingNotification GetNextAvailableFloatingMessage()
        {
            for (int i = 0; i < m_floatingMessages.Length; ++i)
            {
                if (!m_floatingMessages[i].IsDisplayingAMessage())
                    return m_floatingMessages[i];
            }
            return null;
        }

        private void _OnMessageDisplayEnd()
        {
            WaitingMessage message = m_pendingMessages[0];
            m_pendingMessages.RemoveAt(0);
            if (message.OnEnd != null)
                message.OnEnd();
            if (m_pendingMessages.Count > 0)
            {
                DisplayNextMessagesInQueue();
            }
        }
    }
}