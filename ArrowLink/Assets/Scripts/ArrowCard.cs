using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCard : MonoBehaviour
	{
		public const int c_firstLevel = -10;
		public const int c_secondLevel = -20;
		public const int c_thirdLevel = -30;

		public TileState m_currentState = TileState.Activated;

		[Header("Arrows")]
		[SerializeField]
		GameObject m_N = null;
		[SerializeField]
		GameObject m_NE = null;
		[SerializeField]
		GameObject m_E = null;
		[SerializeField]
		GameObject m_SE = null;
		[SerializeField]
		GameObject m_S = null;
		[SerializeField]
		GameObject m_SW = null;
		[SerializeField]
		GameObject m_W = null;
		[SerializeField]
		GameObject m_NW = null;

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		public List<TileLink> m_tileLinks = null;

		[SerializeField]
		private ParticleSystem m_combo_Particles = null;

		public ParticleSystem ComboParticles { get { return m_combo_Particles; } }

		[SerializeField]
		private ParticleSystem m_linkedparticles = null;

		public ParticleSystem LinkedParticles { get { return m_linkedparticles; } }


		public CardTweenAnimations m_tweens;

		private void Awake()
		{
			FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;
			SetArrows(distribuor.PickRandomFlags());
			distribuor.RegisterUsedFlag(m_arrows);
		}

		private void OnDestroy()
		{
			FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;
			distribuor.UnregisterUsedFlag(m_arrows);
		}

		public void SetArrows(ArrowFlag flags)
		{
			m_arrows = flags;
			m_N.SetActive((flags & ArrowFlag.N) == ArrowFlag.N);
			m_NE.SetActive((flags & ArrowFlag.NE) == ArrowFlag.NE);
			m_E.SetActive((flags & ArrowFlag.E) == ArrowFlag.E);
			m_SE.SetActive((flags & ArrowFlag.SE) == ArrowFlag.SE);
			m_S.SetActive((flags & ArrowFlag.S) == ArrowFlag.S);
			m_SW.SetActive((flags & ArrowFlag.SW) == ArrowFlag.SW);
			m_W.SetActive((flags & ArrowFlag.W) == ArrowFlag.W);
			m_NW.SetActive((flags & ArrowFlag.NW) == ArrowFlag.NW);
		}
		



		public void PrepareIntroductionTween()
		{
			var parameters = m_tweens.IntroductionSlide.m_parameters;
			var currentposition = transform.position;

			parameters.PositionStart = currentposition + parameters.PositionStart;
			parameters.PositionEnd = currentposition;

			transform.position = parameters.PositionStart;
		}

		public void PrepareActivationTween(Vector3 target)
		{
			var parameters = m_tweens.ActivationSlide.m_parameters;

			var currentPos = transform.position;
			currentPos.z = target.z;

			parameters.PositionStart = currentPos;
			parameters.PositionEnd = target;
		}

		private Action m_onGoToSlotEnd = null;

		public void GoToSlot(BoardSlot slot, Action onArriveToSlot)
		{
			m_currentState = TileState.SlidingToPosition;
			m_onGoToSlotEnd = onArriveToSlot;

			Vector3 target = slot.transform.position;

			var parameters = m_tweens.PlaySlide.m_parameters;
			parameters.PositionStart = transform.position;
			target.z = transform.position.z;
			parameters.PositionEnd = target;

			m_tweens.Play.StartTween(OnGoToSlotEnd);
		}

		private void OnGoToSlotEnd()
		{
			m_currentState = TileState.Played;
			if (m_onGoToSlotEnd != null)
				m_onGoToSlotEnd();
		}
		
		[System.Serializable]
		public struct CardTweenAnimations
		{
			[Header("Tween To Play")]
			public BaseTween Introduction;
			public BaseTween Activation;
			public BaseTween Play;

			[Header("Details")]
			public SingleTween IntroductionSlide;
			public SingleTween ActivationSlide;
			public SingleTween ActivationUnveil;
			public SingleTween PlaySlide;
		}

		public enum TileState
		{
			FadingIn,
			WaitingForActivation,
			Activating,
			Activated,
			SlidingToPosition,
			Played,
			ComboActivated,
			Destroyed,
		}
	}

}