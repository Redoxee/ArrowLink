//#define NO_SHINE_ON_SCORE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShineAndFlash : MonoBehaviour {

    [System.Serializable]
    private struct ParticlesLevel
    {
        public ParticleSystem Particles;
        public int Threshold;
        public bool IsEndFX;

        public ParticlesLevel(ParticleSystem particles, int threshold, bool isEndFx)
        {
            Particles = particles;
            Threshold = threshold;
            IsEndFX = isEndFx;
        }
    }

    [SerializeField]
    private ParticlesLevel[] m_levels = null;

    private int m_firedValue = -1;

    public void StartShine(int value)
    {
#if !NO_SHINE_ON_SCORE
        m_firedValue = value;
        foreach (ParticlesLevel fx in m_levels)
        {
            if (!fx.IsEndFX && value >= fx.Threshold)
            {
                fx.Particles.Play();
            }

        }
#endif
    }

    public void StopShine()
    {
        foreach (ParticlesLevel fx in m_levels)
        {
            if (fx.IsEndFX)
            {
                if (m_firedValue >= fx.Threshold)
                {
                    fx.Particles.Play();
                }
            }
            else
            {
                if (fx.Particles.isPlaying)
                {
                    fx.Particles.Stop();
                }
            }
        }
    }
}
