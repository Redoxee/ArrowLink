using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class EndScreen : MonoBehaviour
    {

        const string c_endScore = "Final score\n<color=#547E48FF>{0}</color>";
        [SerializeField]
        Text m_EndScore = null;

        const string c_endTileScore = "Tiles matched\n<color=#547E48FF>{0}</color>";
        [SerializeField]
        Text m_endTileScore = null;


        [SerializeField]
        VeilSprite m_veil = null;

        [SerializeField]
        BaseUITween m_slideInTween = null;

        GUIManager m_GUIManager = null;

        public void Initialize(GUIManager parent)
        {
            gameObject.SetActive(false);
            m_GUIManager = parent;
        }


        public void DisplayEndScreen(int score, int tileScore)
        {
            gameObject.SetActive(true);
            m_veil.SetDisplay(0);
            m_veil.SetVeilState(true);
            m_EndScore.text = string.Format(c_endScore, score);
            m_endTileScore.text = string.Format(c_endTileScore, tileScore);
            m_slideInTween.StartTween();
        }

        public void OnFeedBackPressed()
        {
            MainProcess.SimpleFeedback();
        }

        public void OnRetryPressed()
        {
            GameProcess.Instance.NotifyLeavingGame();
            MainProcess.Instance.LoadOrReloadGameScene();
        }


    }
}
