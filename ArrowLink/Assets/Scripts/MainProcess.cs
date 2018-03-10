using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.Unity;

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
        public TrackingManager TrackingManager {get{return m_tracking;} }

        private void Awake()
        {
            s_instance = this;

            Application.targetFrameRate = 30;

            m_additionalSceneLoaded = new List<int>();

            m_tracking = new TrackingManager();

            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex != 0)
            {
                m_additionalSceneLoaded.Add(sceneIndex);
                return;
            }

            //FBInit();


            LoadScene(c_menuScene);
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

        public void LoadOrReloadGameScene()
        {
            UnloadAllAdditionalScene(() => { LoadScene(c_gameScene); } );
        }

        public void LoadMenuScene()
        {
            UnloadAllAdditionalScene(() => { LoadScene(c_menuScene); });
        }
    }
}
