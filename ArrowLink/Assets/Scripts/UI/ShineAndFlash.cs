using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShineAndFlash : MonoBehaviour {

    [SerializeField]
    ParticleSystem m_firstLevel = null;
    [SerializeField]
    ParticleSystem m_secondLevel = null;
    [SerializeField]
    ParticleSystem m_thirdLevel = null;
    [SerializeField]
    ParticleSystem m_fourthLevel = null;

    [SerializeField]
    private int m_firstThreshold = 0;
    [SerializeField]
    private int m_secondThreshold = 0;
    [SerializeField]
    private int m_thirdThreshold = 0;
    [SerializeField]
    private int m_fourthThreshold = 0;

    private bool m_shouldBoom = false;
    private bool m_shouldFourth = false;

    public void StartShine(int value)
    {
        if (value >= m_firstThreshold)
        {
            m_firstLevel.Play();
        }
        if (value >= m_thirdThreshold)
        {
            m_thirdLevel.Play();
        }

        m_shouldBoom = value >= m_secondThreshold;
        m_shouldFourth = value > m_fourthThreshold;
    }

    public void StopShine()
    {
        if (m_firstLevel.isEmitting)
        {    if (m_shouldBoom)
            {
                m_secondLevel.Play();
            }
            if (m_shouldFourth)
            {
                m_fourthLevel.Play();
            }
        }
        m_firstLevel.Stop();
        m_thirdLevel.Stop();
    }
}
