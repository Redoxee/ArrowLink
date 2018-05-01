using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerAlternative : MonoBehaviour
    {
        [SerializeField]
        [Range(0,1)]
        private float m_aboveRatio = 0f;
        [SerializeField]
        private float m_ratioThreshold = 1f;
        [SerializeField]
        [Range(0, 1)]
        private float m_bellowRatio = 1f;


        private CanvasScaler m_canvasScaler;
        void Awake()
        {
            m_canvasScaler = GetComponent<CanvasScaler>();

            Adjust();
        }

        private void Adjust()
        {
            var camera = GetComponent<Camera>();
            Rect safe = Screen.safeArea;
            var ratio = (float)safe.height / (float)safe.width;

            m_canvasScaler.matchWidthOrHeight = (ratio < m_ratioThreshold) ? m_bellowRatio : m_aboveRatio;
#if (UNITY_ANDROID || UNITY_IOS)
        }
#else
            res = new Vector2(Screen.width, Screen.height);
        }

        Vector2 res;
        private void Update()
        {
            if (res.x != Screen.width || res.y != Screen.height)
            {
                Adjust();
            }
        }
#endif
    }
}