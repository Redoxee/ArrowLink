using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class MainMenu : MonoBehaviour {

        private static MainMenu s_instance = null;

        public static MainMenu Instance { get { return s_instance; } }

        [SerializeField]
        private AchievementPopup m_achievementPopup = null;
        [SerializeField]
        private SharePopup m_sharePopup = null;

        [SerializeField]
        private FlagDistributor m_flagDistributor = null;

        public FlagDistributor FlagDistributor { get { return m_flagDistributor; } }

        [SerializeField]
        private Transform m_colorsButtonsTransform = null;
        [SerializeField]
        private GameObject m_colorButtonPrefab = null;

        [SerializeField]
        private Transform m_getTheGame = null;

        [SerializeField]
        private Text m_highScoreLabel = null;

        private void Awake()
        {
            Debug.Assert(s_instance == null, "More than one instance of the main menu !!");
            s_instance = this;

            m_achievementPopup.HidePage();
            m_sharePopup.Hide();


            if (MainProcess.Instance != null)
            {
                int bestScore = MainProcess.Instance.Achievements.GetEventValue("BestScore");
                m_highScoreLabel.text = string.Format("BEST SCORE : {0}", bestScore);

#if UNITY_ANDROID || UNITY_IOS
                m_colorsButtonsTransform.gameObject.SetActive(true);
                CreateColorButons();
                m_getTheGame.gameObject.SetActive(false);
#else
                m_colorsButtonsTransform.gameObject.SetActive(false);
                m_getTheGame.gameObject.SetActive(true);
#endif
            }

        }

        private void CreateColorButons()
        {
            ColorModule colorModule = MainProcess.Instance.ColorModule;

            for (int i = 0; i < colorModule.ColorColections.Count; ++i)
            {
                var go = Instantiate(m_colorButtonPrefab, m_colorsButtonsTransform);
                var button = go.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                int index = i;
                button.onClick.AddListener(() => { colorModule.SelectColors(index); });
                var icon = button.transform.GetChild(0).GetComponent<Image>();
                icon.sprite = colorModule.ColorColections[i].CollectionIcon;
            }
        }

        public void RequestPlay()
        {
            //var notifs = MainProcess.Instance.NotificationUI;
            //notifs.AddMessageToQueue("coucou", "coucou");
            //notifs.AddMessageToQueue("coucou", "coucou");
            //notifs.AddMessageToQueue("coucou", "coucou");
            //notifs.DisplayNextMessagesInQueue();

            MainProcess.Instance.LoadOrReloadGameScene();
        }

        public void RequestGetTheGame()
        {
            Application.OpenURL("https://antonoti.itch.io/8-links");
        }

        public void RequestFastCredits()
        {
            Application.OpenURL("https://twitter.com/Redoxee");
        }

        public void RequestAchievement()
        {
            m_achievementPopup.ShowPage();
        }

        public void RequestShare()
        {
            m_sharePopup.Show();
        }

        [Serializable]
        public struct DayNightModule
        {
            public ColorCollection NightCollection;
            public ColorCollection DayCollection;
            [NonSerialized]
            public ColorManager ManagerRef;

            public Image CurrentStateIcon;
            public Sprite NightIcon;
            public Sprite DayIcon;

            public void ToggleColors()
            {
                if (ManagerRef.ColorCollection != DayCollection)
                {
                    ManagerRef.ColorCollection = DayCollection;
                    CurrentStateIcon.sprite = NightIcon;
                }
                else
                {
                    ManagerRef.ColorCollection = NightCollection;
                    CurrentStateIcon.sprite = DayIcon;
                }
            }
        }
    }
}