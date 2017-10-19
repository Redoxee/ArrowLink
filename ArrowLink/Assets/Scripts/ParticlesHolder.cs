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

        private void Awake()
        {
            Model.gameObject.SetActive(false);
        }

        public void SetArrows(ArrowFlag flags)
        {
            NbParticlesOn = 0;

            N.gameObject.SetActive((flags & ArrowFlag.N) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.N) != ArrowFlag.NONE) ? 1 : 0;

            NE.gameObject.SetActive((flags & ArrowFlag.NE) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.NE) != ArrowFlag.NONE) ? 1 : 0;

            E.gameObject.SetActive((flags & ArrowFlag.E) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.E) != ArrowFlag.NONE) ? 1 : 0;

            SE.gameObject.SetActive((flags & ArrowFlag.SE) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.SE) != ArrowFlag.NONE) ? 1 : 0;

            S.gameObject.SetActive((flags & ArrowFlag.S) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.S) != ArrowFlag.NONE) ? 1 : 0;

            SW.gameObject.SetActive((flags & ArrowFlag.SW) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.SW) != ArrowFlag.NONE) ? 1 : 0;

            W.gameObject.SetActive((flags & ArrowFlag.W) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.W) != ArrowFlag.NONE) ? 1 : 0;

            NW.gameObject.SetActive((flags & ArrowFlag.NW) != ArrowFlag.NONE);
            NbParticlesOn += ((flags & ArrowFlag.NW) != ArrowFlag.NONE) ? 1 : 0;
            SetCenter(NbParticlesOn > 0);
        }

        public void SetLinkParticles(ArrowFlag flag, bool enabled)
        {
            switch (flag)
            {
                case ArrowFlag.N:
                    _SetParticles(N.gameObject, enabled);
                    break;
                case ArrowFlag.NE:
                    _SetParticles(NE.gameObject, enabled);
                    break;
                case ArrowFlag.E:
                    _SetParticles(E.gameObject, enabled);
                    break;
                case ArrowFlag.SE:
                    _SetParticles(SE.gameObject, enabled);
                    break;
                case ArrowFlag.S:
                    _SetParticles(S.gameObject, enabled);
                    break;
                case ArrowFlag.SW:
                    _SetParticles(SW.gameObject, enabled);
                    break;
                case ArrowFlag.W:
                    _SetParticles(W.gameObject, enabled);
                    break;
                case ArrowFlag.NW:
                    _SetParticles(NW.gameObject, enabled);
                    break;
            }

        }

        private void _SetParticles(GameObject go, bool enabled)
        {
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
            _SetParticles(N.gameObject, other.N.gameObject.activeSelf);
            _SetParticles(NE.gameObject, other.NE.gameObject.activeSelf);
            _SetParticles(E.gameObject, other.E.gameObject.activeSelf);
            _SetParticles(SE.gameObject, other.SE.gameObject.activeSelf);
            _SetParticles(S.gameObject, other.S.gameObject.activeSelf);
            _SetParticles(SW.gameObject, other.SW.gameObject.activeSelf);
            _SetParticles(W.gameObject, other.W.gameObject.activeSelf);
            _SetParticles(NW.gameObject, other.NW.gameObject.activeSelf);
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