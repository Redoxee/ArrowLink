using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShineAndFlash : MonoBehaviour {

    [SerializeField]
    ParticleSystem m_shineparticles = null;
    [SerializeField]
    ParticleSystem m_flashparticles = null;

    [SerializeField]
    private int m_thresholdShine = 0;
    [SerializeField]
    private int m_thresholdBoom = 0;

    private bool m_shouldBoom = false;

    public void StartShine(int value)
    {
        if (value >= m_thresholdShine)
        {
            m_shineparticles.Play();
        }

        m_shouldBoom = value >= m_thresholdBoom;
    }

    public void StopShine()
    {
        if (m_shineparticles.isEmitting && m_shouldBoom)
        {
            m_flashparticles.Play();
        }
        m_shineparticles.Stop();
    }
}
