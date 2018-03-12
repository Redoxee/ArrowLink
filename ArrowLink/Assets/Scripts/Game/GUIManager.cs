using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class GUIManager : MonoBehaviour {

        [SerializeField]
        EndScreen m_endScreen = null;
        [SerializeField]
        private PauseUI m_pauseUI = null;

        [SerializeField]
        AnimatedTextNumber m_scoreText = null;
        [SerializeField]
        AnimatedTextNumber m_scoreDeltaText = null;
        [SerializeField]
        Transform m_deltaTarget = null;
        public Transform DeltaTransform { get { return m_deltaTarget; } }

        [SerializeField]
        Text m_scoreMultiplier = null;
        [SerializeField]
        Transform m_scoreMultiplierTarget = null;
        public Transform MultiplierTransform { get { return m_scoreMultiplierTarget; } }
        VeilText m_multiplierVeil = null;


        public Transform BonusCapsuleTransform = null;
        [SerializeField]
        Text m_bonusMultiplier = null;
        [SerializeField]
        Text m_bonusBank = null;
        [SerializeField]
        BaseUITween m_bonusIncrementAnim = null;
        [SerializeField]
        VeilSprite m_bonusCapsuleVeil = null;

        [SerializeField]
        private VeilText m_bankVeil = null;
        [SerializeField]
        private VeilSprite m_crunchVeil = null;
        [SerializeField]
        TweenToggle m_bankableTweenToggle = null;

        [SerializeField]
        private DayNightModule m_dayNightModule;

        GameProcess m_gameProcess;

        private void Start()
        {
            m_gameProcess = GameProcess.Instance;

            m_scoreText.SetDisplay(0);
            m_scoreDeltaText.SetDisplay(0);

            m_scoreDeltaText.ReachedAction = OnScoreDeltaTextAnimEnded;

            m_endScreen.Initialize(this);

            m_dayNightModule.ManagerRef = ColorManager.Instance;

            m_multiplierVeil = m_scoreMultiplier.GetComponent<VeilText>();
            InstantHideMultiplier();

            InitFMS();
            SetState(m_inGameState);
        }

        public void NotifyEndGame()
        {
            SetState(m_endGameState);
        }

        public void OnPausePressed()
        {
            if (!m_gameProcess.IsGameEnded)
            {
                SetState(m_pauseState);
            }
        }

        public void OnPauseCloseRequested()
        {
            SetState(m_inGameState);
        }

        public void OnHomePressed()
        {
            MainProcess.Instance.LoadMenuScene();
        }

        public void OnBankPressed()
        {
            m_gameProcess.RequestBank();
        }

        public void NotifyDeltaScoreChanged(int newDelta, float delay)
        {
            m_scoreDeltaText.gameObject.SetActive(true);
            m_scoreDeltaText.SetNumber(newDelta, delay);
        }

        public void SetScoreMultiplier(float multiplier)
        {
            m_multiplierVeil.SetVeilState(false);
            m_scoreMultiplier.text = string.Format("x {0:0.0}", multiplier);
        }

        public void SetCapsuleBonusValues(float multiplier, int bank, bool inhibitAnimation = false)
        {
            if (!inhibitAnimation)
            {
                m_bonusIncrementAnim.StartTween();
            }

            m_bonusMultiplier.text = string.Format("POINTS x{0}", multiplier);
            m_bonusBank.text = string.Format("BANK +{0}", bank);
        }

        public void InstantHideMultiplier()
        {
            m_multiplierVeil.SetDisplay(0f);
            m_multiplierVeil.SetVeilState(true);
        }

        public void ApplyMultiplier(int newDeltaScore)
        {
            m_multiplierVeil.SetVeilState(true);
            m_scoreDeltaText.SetNumber(newDeltaScore);
        }

        public void ApplyScoreDelta(int newScore)
        {
            m_scoreText.SetNumber(newScore);
            m_scoreDeltaText.SetNumber(0);
        }

        public void SetCapsuleBonusEnabled(bool enabled)
        {
            m_bonusCapsuleVeil.SetVeilState(!enabled);
        }

        private void OnScoreDeltaTextAnimEnded()
        {
            if (m_scoreDeltaText.CurrentTarget == 0)
            {
                m_scoreDeltaText.gameObject.SetActive(false);
            }
        }
        
        public void OnIntroCloseButtonPressed()
        {
            SetState(m_inGameState);
        }

        public void OnCrunchButtonPressed()
        {
            m_gameProcess.RequestTileCrunchToggle();
            
        }

        public void SetBankable(bool isBankable)
        {
            m_bankVeil.SetVeilState(!isBankable);
            m_bankableTweenToggle.IsOn = isBankable;
        }

        public void SetCrunchable(bool isCrunchable)
        {
            m_crunchVeil.SetVeilState(!isCrunchable);
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
			m_inGameState = new GUIFSMState(_InGameStart, _InGameEnd);
			m_endGameState = new GUIFSMState(_StartEndGame, null);
            m_pauseState = new GUIFSMState(_StartPause, _EndPause);
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

		#region State InGame

		private void _InGameStart()
		{
            m_gameProcess.IsGamePaused = false;
		}

		private void _InGameEnd()
        {
            m_gameProcess.IsGamePaused = true;
        }

		#endregion

		#region State EndGame

		GUIFSMState m_endGameState;

		private void _StartEndGame()
		{
			GameProcess gp = GameProcess.Instance;
			m_endScreen.DisplayEndScreen(gp.CurrentScore, gp.CurrentTileScore);
		}

        #endregion

        #region State Pause
        GUIFSMState m_pauseState;
        private void _StartPause()
        {
            GameProcess gp = GameProcess.Instance;
            m_pauseUI.Show(gp.CurrentScore, gp.CurrentTileScore);
        }

        private void _EndPause()
        {
            m_pauseUI.Hide();
        }

        #endregion

        #endregion

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
