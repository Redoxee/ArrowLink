﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using AntonMakesGames;

namespace ArrowLink
{
    public class MainProcess : MonoBehaviour
    {

        private static MainProcess s_instance = null;
        public static MainProcess Instance { get { return s_instance; } }
        public static bool IsReady { get { return s_instance != null; } }
        private static int s_loadingScene = -1;

        public static void InvokMainSceneIfNecessary()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (s_instance == null)
            {
                s_loadingScene = sceneIndex;
                SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            }
        }

        private const int c_gameScene = 1;
        private const int c_menuScene = 2;

        private List<int> m_additionalSceneLoaded;

        private TrackingManager m_tracking;
        public TrackingManager TrackingManager { get { return m_tracking; } }

        [SerializeField]
        private TextAsset m_achievementConfiguration = null;
        private AchievementManager m_achievementsManager = null;
        public AchievementManager Achievements { get { return m_achievementsManager; } }

        [SerializeField]
        private FloatingNotificationUI m_notificationUI = null;
        public FloatingNotificationUI NotificationUI { get { return m_notificationUI; } }

        public ColorManager ColorManager;

        public ColorModule ColorModule;

        private void Awake()
        {
            s_instance = this;

            Application.targetFrameRate = 30;

            m_additionalSceneLoaded = new List<int>();

            m_tracking = new TrackingManager();

            m_achievementsManager = new AchievementManager(m_achievementConfiguration);

            CheckIsItANewDay();

            ColorModule.Initialize();

            m_adManager = new AdManager();

            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex != 0)
            {
                m_additionalSceneLoaded.Add(sceneIndex);
                return;
            }

            int nbGameFinished = m_achievementsManager.GetEventValue("GameFinished");
            if (nbGameFinished > 0)
            {
                LoadScene(c_menuScene);
            }
            else
            {
                LoadScene(c_gameScene);
            }

        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                FBInit();
            }
        }

        #region Facebook

        private void FBInit()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                FB.Init(() =>
                {
                    FB.ActivateApp();
                });
            }
        }

        #endregion

        #region Ads

        private AdManager m_adManager;

        #endregion

        private void LoadScene(int sceneIndex, Action onLoaded = null)
        {
            if (!m_additionalSceneLoaded.Contains(sceneIndex))
            {
                m_additionalSceneLoaded.Add(sceneIndex);
                StartCoroutine(_LoadSceneCoroutine(sceneIndex, onLoaded));
            }
            else if (onLoaded != null)
                onLoaded();
        }

        private IEnumerator _LoadSceneCoroutine(int sceneIndex, Action onLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));
            if (onLoaded != null)
                onLoaded();
        }

        private void UnloadScene(int sceneIndex, Action onUnloaded = null)
        {
            if (m_additionalSceneLoaded.Contains(sceneIndex))
            {
                m_additionalSceneLoaded.Remove(sceneIndex);
                StartCoroutine(_unloadSceneCoroutine(sceneIndex, onUnloaded));
            }
            else if (onUnloaded != null)
                onUnloaded();
        }


        private IEnumerator _unloadSceneCoroutine(int sceneIndex, Action onUnloaded)
        {
            yield return SceneManager.UnloadSceneAsync(sceneIndex);
            if (onUnloaded != null)
                onUnloaded();
        }

        private void UnloadAllAdditionalScene(Action onUnloaded)
        {
            if (m_additionalSceneLoaded.Count > 0)
            {
                StartCoroutine(_unloadAllAditionalScene(onUnloaded));
            }
            else
            {
                onUnloaded();
            }
        }

        private IEnumerator _unloadAllAditionalScene(Action OnUnloaded)
        {
            while (m_additionalSceneLoaded.Count > 0)
            {
                int sceneIndex = m_additionalSceneLoaded[0];
                m_additionalSceneLoaded.RemoveAt(0);
                yield return SceneManager.UnloadSceneAsync(sceneIndex);
            }
            OnUnloaded();
        }

        [SerializeField]
        private int m_interstitalToVideoAdCount = 2;
        private int m_nbAdPing = 0;
        public void RequestGameScene()
        {
            m_nbAdPing++;
            if (m_nbAdPing > 1)
            {
                m_adManager.RequestInterstitial();
            }
            else
            {
                LoadOrReloadGameScene();
            }
        }

        public void LoadOrReloadGameScene()
        {
            UnloadAllAdditionalScene(() => { LoadScene(c_gameScene); } );
        }

        public void LoadMenuScene()
        {
            UnloadAllAdditionalScene(() => { LoadScene(c_menuScene); });
        }

        #region Achievements

        public void DisplayCompletedAchievements()
        {
            List<Achievement> completedAchievements = m_achievementsManager.CheckNonCompletedAchievement();
            int count = completedAchievements.Count;
            for (int i = count - 1; i > -1; --i)
            {
                Achievement achievement = completedAchievements[i];
                m_notificationUI.AddMessageToQueue(achievement.Title, "Completed", true);
            }
            m_notificationUI.DisplayNextMessagesInQueue();
        }

        #endregion

        #region Daily check

        public const string c_lastConectionKey = "LastConection";

        private void CheckIsItANewDay()
        {
            long currentDay = DateTime.Today.Ticks;
            if (PlayerPrefs.HasKey(c_lastConectionKey))
            {
                string strDate = PlayerPrefs.GetString(c_lastConectionKey);
                long lastConection;
                long.TryParse(strDate, out lastConection);
                if (currentDay > lastConection)
                {
                    MainProcess mp = MainProcess.Instance;
                    mp.NotificationUI.ShowFloatingMessage("Hello", "Welcome back!");
                    PlayerPrefs.SetString(c_lastConectionKey, currentDay.ToString());
                    mp.Achievements._NotifyEventIncrement("ConnectedDay");

                }

            }
            else
            {
                PlayerPrefs.SetString(c_lastConectionKey, currentDay.ToString());
                MainProcess mp = MainProcess.Instance;
                mp.Achievements._NotifyEventIncrement("ConnectedDay");
                mp.Achievements.Save();
                mp.DisplayCompletedAchievements();
            }
        }

        #endregion
    }
}
