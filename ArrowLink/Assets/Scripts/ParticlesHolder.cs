using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class ParticlesHolder: MonoBehaviour, ColorGrabber.ICustomColorGrabber
    {
        int NbParticlesOn;

        public ParticleSystem Center;
        public ParticleSystem N;
        public ParticleSystem NE;
        public ParticleSystem E;
        public ParticleSystem SE;
        public ParticleSystem S;
        public ParticleSystem SW;
        public ParticleSystem W;
        public ParticleSystem NW;

        public ParticleSystem Model;

        private Dictionary<ArrowFlag, ParticleSystem> m_particlesDictionnary = new Dictionary<ArrowFlag, ParticleSystem>(8);
        private List<ParticleSystem> m_particlesAsList = new List<ParticleSystem>(8);
        private void Awake()
        {
            Initialize();
        }
        private bool m_isInitialized = false;
        private void Initialize()
        {
            if (m_isInitialized)
                return;
            Model.gameObject.SetActive(false);
            m_particlesDictionnary[ArrowFlag.N] = N;
            m_particlesAsList.Add(N);
            m_particlesDictionnary[ArrowFlag.NE] = NE;
            m_particlesAsList.Add(NE);
            m_particlesDictionnary[ArrowFlag.E] = E;
            m_particlesAsList.Add(E);
            m_particlesDictionnary[ArrowFlag.SE] = SE;
            m_particlesAsList.Add(SE);
            m_particlesDictionnary[ArrowFlag.S] = S;
            m_particlesAsList.Add(S);
            m_particlesDictionnary[ArrowFlag.SW] = SW;
            m_particlesAsList.Add(SW);
            m_particlesDictionnary[ArrowFlag.W] = W;
            m_particlesAsList.Add(W);
            m_particlesDictionnary[ArrowFlag.NW] = NW;
            m_particlesAsList.Add(NW);
            m_isInitialized = true;
        }

        ArrowFlag[] m_flagsArray = new ArrowFlag[8];
        public void SetArrows(ArrowFlag flags)
        {
            Initialize();
            foreach (var particles in m_particlesAsList)
            {
                particles.gameObject.SetActive(false);
            }

            NbParticlesOn = 0;
            flags.Split(ref m_flagsArray, ref NbParticlesOn);

            for (int i = 0; i < NbParticlesOn; ++i)
            {
                var particles = m_particlesDictionnary[m_flagsArray[i]];
                particles.gameObject.SetActive(true);
            }
            SetCenter(NbParticlesOn > 0);
        }

        public void SetLinkParticles(ArrowFlag flag, bool enabled)
        {
            _SetParticles(flag, enabled);
        }

        private void _SetParticles(ArrowFlag direction, bool enabled)
        {
            GameObject go = m_particlesDictionnary[direction].gameObject;
            bool prev = go.activeSelf;
            go.SetActive(enabled);
            if (prev != enabled)
                NbParticlesOn += enabled ? 1 : -1;
        }

        public void SetCenter(bool isOn)
        {
            Center.gameObject.SetActive(isOn);
        }

        public void SwapParticles(ParticlesHolder other)
        {

            _SetParticles(ArrowFlag.N, other.N.gameObject.activeSelf);
            _SetParticles(ArrowFlag.NE, other.NE.gameObject.activeSelf);
            _SetParticles(ArrowFlag.E, other.E.gameObject.activeSelf);
            _SetParticles(ArrowFlag.SE, other.SE.gameObject.activeSelf);
            _SetParticles(ArrowFlag.S, other.S.gameObject.activeSelf);
            _SetParticles(ArrowFlag.SW, other.SW.gameObject.activeSelf);
            _SetParticles(ArrowFlag.W, other.W.gameObject.activeSelf);
            _SetParticles(ArrowFlag.NW, other.NW.gameObject.activeSelf);
            other.SetArrows(ArrowFlag.NONE);
            other.SetCenter(false);
            SetCenter(NbParticlesOn > 0);
        }

        public void ApplyModel()
        {
            var color = Model.colorOverLifetime.color;
            var colorModul = N.colorOverLifetime;
            colorModul.color = color;
            colorModul = NE.colorOverLifetime;
            colorModul.color = color;
            colorModul = E.colorOverLifetime;
            colorModul.color = color;
            colorModul = SE.colorOverLifetime;
            colorModul.color = color;
            colorModul = S.colorOverLifetime;
            colorModul.color = color;
            colorModul = SW.colorOverLifetime;
            colorModul.color = color;
            colorModul = W.colorOverLifetime;
            colorModul.color = color;
            colorModul = NW.colorOverLifetime;
            colorModul.color = color;
            colorModul = Center.colorOverLifetime;
            colorModul.color = color;
        }

        public void CancelParticlesLoop()
        {
            var mainModule = Center.main;
            mainModule.loop = false;
            foreach (var particles in m_particlesAsList)
            {
                mainModule = particles.main;
                mainModule.loop = false;
            }
        }

        public void GrabColor(Color col)
        {
            col.a = 1;
            var cot = Model.colorOverLifetime;
            var c = cot.color;
            GradientColorKey[] cks = new GradientColorKey[1];
            cks[0] = new GradientColorKey(col, 0);
            c.gradient.SetKeys(cks, c.gradient.alphaKeys);
            cot.color = c;
            ApplyModel();
        }
    }
}