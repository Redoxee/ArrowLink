﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
	public class GUIManager : MonoBehaviour {

		[SerializeField]
		GameObject m_popupUI = null;
		public GameObject PopupUI { get { return  m_popupUI; } }
		[SerializeField]
		GameObject m_introUI = null;
		[SerializeField]
		EndScreen m_endScreen = null;

		[SerializeField]
		Text m_scoreText = null;

		[SerializeField]
		GraphicRaycaster m_gameplayGraphicRayCaster = null;

		[SerializeField]
		SequenceUITween m_introFadeOutTween = null;

		private void Start()
		{
			m_scoreText.text = "0";

			m_endScreen.Initialize(this);

			InitFMS();
			SetState(m_introState);
		}

		public void NotifyEndGame()
		{
			SetState(m_endGameState);
		}

		public void OnPausePressed()
		{
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
		}

		public void NotifyScoreChanged(int newScore, int scoreDelta)
		{
			m_scoreText.text = newScore.ToString();
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

		GUIFSMState m_currentState;

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
			m_gameplayGraphicRayCaster.enabled = false;
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
			m_gameplayGraphicRayCaster.enabled = true;
		}

		private void _InGameEnd()
		{
			m_gameplayGraphicRayCaster.enabled = false;
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

		#endregion

	}
}
