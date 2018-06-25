//#define AMG_NO_HIGHLIGHT_CIRCLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class HightLightCircle : MonoBehaviour
    {

        [SerializeField]
        private float m_spinSpeed = 100f;

        [SerializeField]
        BaseTween m_showTween = null;

        [SerializeField]
        BaseTween m_hideTween = null;

        [SerializeField]
        SingleTween[] m_tweenToInitializeBeforeHide = null;

        private Vector3 m_axis = new Vector3(0, 0, 1);

        [SerializeField]
        private float m_timeBeforeHighLight = 10f;

        private float m_timer = 0f;

        public void Show()
        {
#if !AMG_NO_HIGHLIGHT_CIRCLE
            gameObject.SetActive(true);
            m_timer = m_timeBeforeHighLight;

            transform.rotation = Quaternion.AngleAxis(360 * Random.Range(0f,1f), m_axis);
#endif
        }

        public void CancelHighLight()
        {
            if(!gameObject.activeSelf)
                return;

            if (m_timer <= 0f)
                m_hideTween.StartTween(HideComplete);
            else
                HideComplete();
            m_timer = -1f;
        }

        private void HideComplete()
        {
            gameObject.SetActive(false);
        }
        
        void Update()
        {
            if (m_timer >= 0f)
            {
                m_timer -= Time.deltaTime;
                if (m_timer < 0f)
                {
                    m_showTween.StartTween();
                }
            }

            transform.rotation *= Quaternion.AngleAxis(m_spinSpeed * Time.deltaTime, m_axis);
        }
        
        private void InitializeBeforeHide()
        {
            foreach (SingleTween bt in m_tweenToInitializeBeforeHide)
            {
                bt.m_parameters.PositionStart = bt.transform.localPosition;
            }
        }
    }
}