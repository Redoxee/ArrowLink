using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class PauseUI : MonoBehaviour
    {
        public BaseUITween ShowTween = null;

        public BaseUITween HideTween = null;

        [SerializeField]
        private Text m_scoreText = null;
        [SerializeField]
        private Text m_tileText = null;

        public void Show(int score, int tileLinked)
        {
            gameObject.SetActive(true);
            ShowTween.StartTween();
            m_scoreText.text = score.ToString();
            m_tileText.text = tileLinked.ToString();
        }


        public void Hide()
        {
            HideTween.StartTween(_OnHideEnd);
        }

        private void _OnHideEnd()
        {
            gameObject.SetActive(false);
        }
    }
}