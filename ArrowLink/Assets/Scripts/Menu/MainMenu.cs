using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class MainMenu : MonoBehaviour {

        private static MainMenu s_instance = null;

        public static MainMenu Instance { get { return s_instance; } }

        [SerializeField]
        FlagDistributor m_flagDistributor = null;

        public FlagDistributor FlagDistributor { get { return m_flagDistributor; } }

        private void Awake()
        {
            Debug.Assert(s_instance == null, "More than one instance of the main menu !!");
            s_instance = this;

            CheckIsItANewDay();
        }

        public const string c_lastConectionKey = "LastConection";

        private void CheckIsItANewDay()
        {
            long currentDay = DateTime.Today.Ticks;
            if (PlayerPrefs.HasKey(c_lastConectionKey))
            {
                string strDate = PlayerPrefs.GetString(c_lastConectionKey);
                long lastConection;
                long.TryParse(strDate,out lastConection);
                if (currentDay > lastConection)
                {
                    MainProcess mp = MainProcess.Instance;
                    mp.NotificationUI.ShowFloatingMessage("Hello","Welcome back!");
                    PlayerPrefs.SetString(c_lastConectionKey, currentDay.ToString());
                    mp.Achievements.NotifyEventIncrement("ConnectedDay");
                    mp.Achievements.Save();
                    mp.DisplayCompletedAchievements();
                    
                }

            }
            else
            {
                PlayerPrefs.SetString(c_lastConectionKey, currentDay.ToString());
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
    }
}