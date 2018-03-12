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
            SimpleFeedback();
        }

        public void OnRetryPressed()
        {
            MainProcess.Instance.LoadOrReloadGameScene();
        }


        #region Feedback
        const string c_feedbackEmail = "antonmakesgames@gmail.com";

        public static void SimpleFeedback()
        {
            SendFeedback("I have some feedback on Tile link!", "");
        }

        public static void SendFeedback(string header, string body = "")
        {
            string subject = EscapeURL(header);
            body = EscapeURL(body);
            Application.OpenURL("mailto:" + c_feedbackEmail + "?subject=" + subject + "&body=" + body);
        }

        static string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }

        #endregion
    }
}
