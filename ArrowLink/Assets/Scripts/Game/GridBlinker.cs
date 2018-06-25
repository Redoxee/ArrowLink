using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class GridBlinker : MonoBehaviour {

        [SerializeField]
        private BoardInput m_grid = null;
        [SerializeField]
        private float m_intervals = .5f;
        private float m_duration = 0f;

        private int m_blinkCounter = 0;

        private float[] m_introPattern ={
            00f,01f,02f,03f,
            01f,02f,03f,04f,
            02f,03f,04f,05f,
            03f,04f,05f,06f
        };
        
        

        public void StartAnimation(int nbBlink = 1)
        {
            enabled = true;
            m_timer = 0;
            float max = -1f;
            for (int i = 0; i < m_introPattern.Length; ++i)
            {
                if (m_introPattern[i] > max)
                    max = m_introPattern[i];
            }
            m_duration = (max+1) * m_intervals;
            m_blinkCounter = nbBlink;
        }

        private float m_timer = 0;
        private void Update()
        {
            float prev = m_timer;
            m_timer += Time.deltaTime;

            for (int i = 0; i < m_grid.m_slots.Length; ++i)
            {
                float ts = m_introPattern[i] * m_intervals;
                if (prev <= ts && m_timer > ts)
                {
                    var slot = m_grid.m_slots[i];
                    slot.Flash();
                }
            }

            if (m_timer > m_duration)
            {
                m_blinkCounter--;
                if (m_blinkCounter > 0)
                {
                    m_timer = 0;
                }
                else
                {
                    enabled = false;
                }
            }
        }
    }
}