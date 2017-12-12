using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class HoldTile : MonoBehaviour
    {

        private const float c_holdNudgeIntervals = 7.5f;
        private float m_nextHoldNudge = c_holdNudgeIntervals * 3f;
        [SerializeField]
        BaseTween m_nudgeTween = null;


        public void Start()
        {
            GameProcess.Instance.RegisterTilePlayedListeners(_OnTilePressed);
        }

        public void OnDisable()
        {
            GameProcess.Instance.UnregisterTilePlayedListeners(_OnTilePressed);
        }

        void Update()
        {
            GameProcess gp = GameProcess.Instance;
            if (gp.IsHoldingCard)
            {
                enabled = false;
                return;
            }
            m_nextHoldNudge -= Time.deltaTime;
            if (m_nextHoldNudge <= 0)
            {
                m_nextHoldNudge = c_holdNudgeIntervals;
                m_nudgeTween.StartTween();
            }
        }

        private void _OnTilePressed()
        {
            m_nextHoldNudge = c_holdNudgeIntervals;
        }
    }
}
