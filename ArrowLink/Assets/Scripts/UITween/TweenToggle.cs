using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    [RequireComponent(typeof(BaseUITween))]
    public class TweenToggle : MonoBehaviour
    {
        BaseUITween m_tween = null;

        private bool m_isOn = false;
        public bool IsOn
        {
            get
            {
                return m_isOn;
            }
            set
            {
                m_isOn = value;
                if (!m_tween.isAnimating())
                    m_tween.StartTween(CheckRestart);
            }
        }

        private void Awake()
        {
            m_tween = GetComponent<BaseUITween>();
        }

        private void CheckRestart()
        {
            if (m_isOn)
                m_tween.StartTween(CheckRestart);
        }
    }
}