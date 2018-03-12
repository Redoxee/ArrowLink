using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class SimpleFloatingNotification : MonoBehaviour
    {
        [SerializeField]
        SingleUITween m_showTween = null;
        [SerializeField]
        SingleUITween m_hideTween = null;
        [SerializeField]
        BaseUITween m_fullAnimation = null;

        [SerializeField]
        Text m_title = null;
        [SerializeField]
        Text m_subTitle = null;

        private void Awake()
        {
            RectTransform rect = (RectTransform)transform;
            Vector3 onPosition = rect.anchoredPosition;
            float width = rect.rect.width;
            Vector3 offPosition = new Vector3(-width,onPosition.y,onPosition.z);

            m_showTween.Parameters.PositionStart = offPosition;
            m_showTween.Parameters.PositionEnd = onPosition;
            m_hideTween.Parameters.PositionStart = onPosition;
            m_hideTween.Parameters.PositionEnd = offPosition;
        }

        private Action m_endAction;

        public void ShowMessage(string title, string subtitle, Action onEnd = null)
        {
            m_title.text = title;
            m_subTitle.text = subtitle;
            gameObject.SetActive(true);
            m_endAction = onEnd;
            m_fullAnimation.StartTween(_OnAnimationEnd);
        }

        private void _OnAnimationEnd()
        {
            gameObject.SetActive(false);
            if (m_endAction != null)
                m_endAction();
        }

        public bool IsDisplayingAMessage()
        {
            return m_fullAnimation.isAnimating();
        }
    }
}