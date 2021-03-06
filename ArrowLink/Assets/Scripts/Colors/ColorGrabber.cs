﻿using System.Collections;
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
        private ColorCollection.GrabbableColor m_colorToGrab = ColorCollection.GrabbableColor.Back;
        
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

        public enum GrabMode
        {
            Sprite,
            Image,
            Text,
            Camera,
            Custom,
            Particles,
            LineRenderer,
            Mesh,
        }

        void GrabColor()
        {
            ApplyColor(ColorManager.Instance.ColorCollection);
        }

        public void ApplyColor(ColorCollection colorCollection)
        {
            Color col = colorCollection.GetColor(m_colorToGrab);
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
                case GrabMode.LineRenderer:
                    GrabLineRenderer(col);
                    break;
                case GrabMode.Mesh:
                    GrabMesh(col);
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

        void GrabLineRenderer(Color col)
        {
            var lr = GetComponent<LineRenderer>();
            col.a = 1;
            lr.startColor = col;
            lr.endColor = col;
        }

        void GrabMesh(Color col)
        {
            var mr = GetComponent<MeshRenderer>();
            var c = mr.sharedMaterial.color;
            col.a = c.a;
            mr.sharedMaterial.color = col;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (GetComponent<Sprite>() != null)
                m_mode = GrabMode.Sprite;
            else if (GetComponent<UnityEngine.UI.Image>() != null)
                m_mode = GrabMode.Image;
            else if (GetComponent<UnityEngine.UI.Text>() != null)
                m_mode = GrabMode.Text;
            else if (GetComponent<Camera>() != null)
                m_mode = GrabMode.Camera;
            else if (GetComponent<ParticleSystem>() != null)
                m_mode = GrabMode.Particles;
            else if (GetComponent<LineRenderer>() != null)
                m_mode = GrabMode.LineRenderer;
        }
#endif
    }
}