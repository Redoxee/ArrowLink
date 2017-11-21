using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class GUIManager : MonoBehaviour {

        [SerializeField]
        GameObject m_popupUI = null;
        public GameObject PopupUI { get { return m_popupUI; } }
        [SerializeField]
        GameObject m_introUI = null;
        [SerializeField]
        EndScreen m_endScreen = null;

        [SerializeField]
        AnimatedTextNumber m_scoreText = null;
        [SerializeField]
        AnimatedTextNumber m_scoreDeltaText = null;

        [SerializeField]
        SequenceUITween m_introFadeOutTween = null;

        [SerializeField]
        AnimatedProgressbar m_bankProgressbar = null;

        [SerializeField]
        ParticleSystem m_bankParticles = null;

        [SerializeField]
        AnimatedProgressbar m_crunchProgressBar = null;

        [SerializeField]
        Text m_AvailableTile = null;
        [SerializeField]
        BaseTween m_dispenserAnimation = null;
        bool m_isDispenserWigling = false;

        [SerializeField]
        private DayNightModule m_dayNightModule;

        GameProcess m_gameProcess;

        private void Start()
        {
            m_gameProcess = GameProcess.Instance;

            m_scoreText.SetDisplay(0);
            m_scoreDeltaText.SetDisplay(0);
            m_scoreDeltaText.gameObject.SetActive(false);
            m_scoreDeltaText.ReachedAction = OnScoreDeltaTextAnimEnded;
            
            m_endScreen.Initialize(this);

            m_dayNightModule.ManagerRef = ColorManager.Instance;

            InitFMS();
            if (m_introUI.activeSelf)
                SetState(m_introState);
            else
                SetState(m_inGameState);
        }

        public void NotifyEndGame()
        {
            SetState(m_endGameState);
        }

        public void OnPausePressed()
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        }

        public void OnBankPressed()
        {
            m_gameProcess.RequestBank();
        }

        public void NotifyDeltaScoreChanged(int newDelta)
        {
            m_scoreDeltaText.gameObject.SetActive(true);
            m_scoreDeltaText.SetNumber(newDelta);
        }

        public void NotifyScoreChanged(int newScore, int scoreDelta)
        {
            m_scoreText.SetNumber(newScore, .25f);
            m_scoreDeltaText.gameObject.SetActive(true);
            m_scoreDeltaText.SetDisplay(scoreDelta);
            m_scoreDeltaText.SetNumber(0, .25f);
        }

        private void OnScoreDeltaTextAnimEnded()
        {
            if (m_scoreDeltaText.CurrentTarget == 0)
            {
                m_scoreDeltaText.gameObject.SetActive(false);
            }
        }
        
        

        public void NotifyAvailableTileCountChanged(int availableTiles)
        {
            m_AvailableTile.text = availableTiles.ToString();
            m_isDispenserWigling = availableTiles < 1;
            if (m_isDispenserWigling)
            {
                m_dispenserAnimation.StartTween(OnDispenserWiggleEnd);
            }
        }

        public void OnIntroCloseButtonPressed()
        {
            Debug.Log("Intro close button pressed");
            m_introFadeOutTween.StartTween(_OnIntroFadedOut);
        }

        private void _OnIntroFadedOut()
        {
            SetState(m_inGameState);
        }

        public void OnCrunchButtonPressed()
        {
            m_gameProcess.RequestTileCrunchToggle();
        }

        GUIFSMState m_currentState;

        public void OnDayNightButtonPressed()
        {
            m_dayNightModule.ToggleColors();
        }

		#region FSM
		GUIFSMState m_inGameState; 

		private void InitFMS()
		{
			m_introState = new GUIFSMState(IntroStart, IntroEnd);
			m_inGameState = new GUIFSMState(_InGameStart, _InGameEnd);
			m_endGameState = new GUIFSMState(_StartEndGame, null);
		}

		private void SetState(GUIFSMState state)
		{
			if (m_currentState.End != null)
				m_currentState.End();
			m_currentState = state;
			if (m_currentState.Start != null)
				m_currentState.Start();
		}

		private struct GUIFSMState
		{
			public Action Start;
			public Action End;

			public GUIFSMState(Action strt = null,Action nd = null)
			{
				Start = strt;
				End = nd;
			}
		}

		#region State Intro

		private GUIFSMState m_introState;

		private void IntroStart()
		{

		}

		private void IntroEnd()
		{
			m_popupUI.SetActive(false);
			m_introUI.SetActive(false);
		}

		#endregion

		#region State InGame

		private void _InGameStart()
		{

		}

		private void _InGameEnd()
		{

		}

		#endregion

		#region State EndGame

		GUIFSMState m_endGameState;

		private void _StartEndGame()
		{
			GameProcess gp = GameProcess.Instance;
			m_endScreen.DisplayEndScreen(gp.CurrentScore, gp.CurrentTileScore);
            m_isDispenserWigling = false;
		}

        #endregion

        #endregion


        private void OnDispenserWiggleEnd()
        {
            if (m_isDispenserWigling)
            {
                m_dispenserAnimation.StartTween(OnDispenserWiggleEnd);
            }
        }

        [Serializable]
        struct DayNightModule
        {
            public ColorCollection NightCollection;
            public ColorCollection DayCollection;
            [NonSerialized]
            public ColorManager ManagerRef;
            private ColorCollection m_currentCollection;

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
