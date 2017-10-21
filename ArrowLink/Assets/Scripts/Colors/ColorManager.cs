using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class ColorManager : MonoBehaviour
    {
        private static ColorManager s_instance = null;
        public static ColorManager Instance { get { return s_instance; } }

        List<ColorGrabber> m_listeners = new List<ColorGrabber>();
        [SerializeField]
        private ColorCollection m_collection;
        public ColorCollection ColorCollection {  get { return m_collection; } }


        private void Awake()
        {
            if (ColorManager.s_instance != null)
            {
                Debug.LogError("Several Color manager instanciated !");
                Destroy(this);
                return;
            }
            s_instance = this;
        }

        public void RegisterListener(ColorGrabber listener)
        {
            if (m_listeners.Contains(listener))
            {
                return;
            }
            m_listeners.Add(listener);
        }

        public void UnregisterListener(ColorGrabber listener)
        {
            if (!m_listeners.Contains(listener))
                return;
            m_listeners.Remove(listener);
        }
    }
}