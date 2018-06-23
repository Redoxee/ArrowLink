using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	[RequireComponent(typeof(Camera))]
	public class GameCamera : MonoBehaviour
	{
		const float TargetWidth = 16f / 2f;

        float m_minRatio = 1.7f;
        float m_maxRatio = 2.0f;
        float m_maxRatioVerticalBump = -1.5f;

		[SerializeField]
		GameObject m_boardRef = null;
        
        public void Initialize()
        {
            Debug.Assert(m_boardRef != null);

            var camera = GetComponent<Camera>();
            Rect safe = Screen.safeArea;
            var ratio = (float)safe.height / (float)safe.width;
            //Debug.LogFormat("Ratio : {0}", ratio);
            ratio = Mathf.Max(m_minRatio, ratio);

            var halfHeight = (TargetWidth) * ratio;
            camera.orthographicSize = halfHeight;

            var camPos = camera.transform.position;
            if (ratio > m_maxRatio)
            {
                halfHeight += m_maxRatioVerticalBump;
            }
            camera.transform.position = new Vector3(camPos.x, halfHeight, camPos.z);
#if (UNITY_ANDROID || UNITY_IOS)
        }
#else
            res = new Vector2(Screen.width,Screen.height);
        }

        Vector2 res;
        private void Update()
        {
            if (res.x != Screen.width || res.y != Screen.height)
            {
                Initialize();
            }
        }
#endif
    }
}
