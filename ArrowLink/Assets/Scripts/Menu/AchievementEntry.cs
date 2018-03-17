using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AntonMakesGames;

namespace ArrowLink
{
    public class AchievementEntry : MonoBehaviour
    {
        [SerializeField]
        private Text m_title = null;
        [SerializeField]
        private Text m_subtitle = null;
        [SerializeField]
        private Image m_border = null;

        public void Setup(Achievement achievement)
        {

            bool displayable = achievement.PrevAchievementCompleted;
            gameObject.SetActive(displayable);
            if (!displayable)
                return;

            m_title.text = achievement.Title;
            bool completed = achievement.IsComplet;
            if (completed)
            {
                m_subtitle.text = "COMPLETED !";
                m_border.color = ColorManager.Instance.ColorCollection.GetColor(ColorCollection.GrabbableColor.ButtonLight);
            }
            else
            {
                string subtitle = string.Format(achievement.SubTitle, achievement.CurrentValue);
                m_subtitle.text = subtitle;
                m_border.color = ColorManager.Instance.ColorCollection.GetColor(ColorCollection.GrabbableColor.ButtonDark);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
