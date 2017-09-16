using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    [RequireComponent(typeof(Image))]
    public class AnimatedProgressbar : MonoBehaviour
    {
        private Image m_image = null;

        public float Speed = .9f;

        private float m_currentValue;
        private float m_targetValue;

        void Awake()
        {
            m_image = GetComponent<Image>();
        }

        public void SetDisplay(float value)
        {
            m_currentValue = value;
            m_image.fillAmount = m_currentValue;
            enabled = true;
        }

        public void SetStarget(float target)
        {
            m_targetValue = target;
            enabled = true;
        }
        

        void Update()
        {
            float direction = m_currentValue > m_targetValue ? -1: 1;
            float nextValue = m_currentValue + direction * Speed * Time.deltaTime;
            if ((direction > 0 && nextValue >= m_targetValue) ||
               (direction < 0 && nextValue <= m_targetValue))
            {
                m_currentValue = nextValue;
                enabled = false;
            }
            else
            {
                m_currentValue = nextValue;
            }

            m_image.fillAmount = m_currentValue;
        }
    }
}
