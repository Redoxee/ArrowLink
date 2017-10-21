using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ArrowLink
{
    public class ColorGrabber : MonoBehaviour
    {
        [SerializeField]
        private GrabMode m_mode = GrabMode.Sprite;

        [SerializeField]
        private ColorColection.GrabbableColor m_colorToGrab = ColorColection.GrabbableColor.Back;
        
        public interface ICustomColorGrabber{ void GrabColor(Color col); }
        [SerializeField]
        private MonoBehaviour m_customGrabber = null;

        private void Start()
        {
            ColorManager.Instance.RegisterListener(this);

            GrabColor();
        }

        private void OnDestroy()
        {
            if(ColorManager.Instance != null)
                ColorManager.Instance.UnregisterListener(this);
        }

        void Update()
        {
            enabled = false;
        }

        public enum GrabMode
        {
            Sprite,
            Image,
            Text,
            Camera,
            Custom,
            Particles,
        }

        void GrabColor()
        {
            Color col = ColorManager.Instance.ColorCollection.GetColor(m_colorToGrab);
            switch (m_mode)
            {
                case GrabMode.Sprite:
                    GrabSprite(col);
                    break;
                case GrabMode.Image:
                    GrabImage(col);
                    break;
                case GrabMode.Text:
                    GrabText(col);
                    break;
                case GrabMode.Custom:
                    GrabCustom(col);
                    break;
                case GrabMode.Camera:
                    GrabCamera(col);
                    break;
                case GrabMode.Particles:
                    GrabParticles(col);
                    break;
                default:
                    throw new System.NotSupportedException();
            }
        }

        void GrabSprite(Color col)
        {
            var s = GetComponent<SpriteRenderer>();
            col.a = s.color.a;
            s.color = col;
        }

        void GrabImage(Color col)
        {
            var image = GetComponent<UnityEngine.UI.Image>();
            col.a = image.color.a;
            image.color = col;
        }
        
        void GrabText(Color col)
        {
            var t = GetComponent<UnityEngine.UI.Text>();
            col.a = t.color.a;
            t.color = col;
        }

        void GrabCamera(Color col)
        {
            var c = GetComponent<Camera>();
            c.backgroundColor = col;
        }

        void GrabCustom(Color col)
        {
            ((ICustomColorGrabber)m_customGrabber).GrabColor(col);
        }

        void GrabParticles(Color col)
        {
            var parts = GetComponent<ParticleSystem>();
            col.a = 1;
            var colt = parts.colorOverLifetime;
            var c = colt.color;
            GradientColorKey[] gck = new GradientColorKey[1];
            gck[0] = new GradientColorKey(col, 0);
            c.gradient.SetKeys(gck, c.gradient.alphaKeys);
            colt.color = c;
        }
    }
}