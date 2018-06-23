using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AntonMakesGames;

namespace ArrowLink
{
    public class AchievementPopup : MonoBehaviour {
        [SerializeField]
        private List<AchievementEntry> m_entryList = null;
        [SerializeField]
        private Transform m_achievementEntryParent = null;
        [SerializeField]
        private GameObject m_entryPrefab = null;
        [SerializeField]
        private CanvasGroup m_canvasGroup = null;
        [SerializeField]
        private ScrollRect m_scrollRect = null;

        public void ShowPage()
        {
            List<Achievement> achievementList = MainProcess.Instance.Achievements.GetSortedAchievements();
            int count = m_entryList.Count;
            int ac = achievementList.Count;
            Debug.Assert(count >= ac, "not enought entry to display achievements");
            for (int i = 0; i < count; ++i)
            {
                if (i < ac)
                {
                    m_entryList[i].Setup(achievementList[i]);
                }
                else
                {
                    m_entryList[i].Hide();
                }
            }

            m_canvasGroup.alpha = 1;
            m_canvasGroup.blocksRaycasts = true;
            m_canvasGroup.interactable = true;
            m_scrollRect.verticalNormalizedPosition = 1;
        }

        public void HidePage()
        {
            m_canvasGroup.alpha = 0;
            m_canvasGroup.blocksRaycasts = false;
            m_canvasGroup.interactable = false;
        }

        private int m_expectedAchievements = 100;
        public void GenerateEntries()
        {
            for (int i = m_entryList.Count; i < m_expectedAchievements; ++i)
            {
                GameObject entry = Instantiate(m_entryPrefab, m_achievementEntryParent, false);
                AchievementEntry ae = entry.GetComponent<AchievementEntry>();
                m_entryList.Add(ae);
            }
        }
    }
}
