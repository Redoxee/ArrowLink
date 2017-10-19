using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ArrowLink
{
    public class ColorGrabber : MonoBehaviour
    {
        [SerializeField]
        private ColorColection.GrabbableColor m_colorToGrab = ColorColection.GrabbableColor.Back;

        [SerializeField]
        private GrabMode m_mode = GrabMode.Sprite;

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
            Custom,
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
                default:
                    throw new System.NotSupportedException();
            }
        }

        void GrabSprite(Color col)
        {
            var s = GetComponent<SpriteRenderer>();
            s.material.color = col;
        }

        void GrabImage(Color col)
        {
            var image = GetComponent<UnityEngine.UI.Image>();
            image.color = col;
        }
        
        void GrabText(Color col)
        {
            var t = GetComponent<UnityEngine.UI.Text>();
            t.color = col;
        }

        void GrabCustom(Color col)
        {
            ((ICustomColorGrabber)m_customGrabber).GrabColor(col);
        }
    }
}