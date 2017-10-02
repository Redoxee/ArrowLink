using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCard : MonoBehaviour
	{
		public const int c_firstLevel = -10;
		public const int c_secondLevel = -30;
		public const int c_thirdLevel = -60;

		public TileState m_currentState = TileState.Activated;

		[Header("Arrows")]
		[SerializeField]
		GameObject m_N = null;
		ParticleSystem m_particle_N = null;
		[SerializeField]
		GameObject m_NE = null;
		ParticleSystem m_particle_NE = null;
		[SerializeField]
		GameObject m_E = null;
		ParticleSystem m_particle_E = null;
		[SerializeField]
		GameObject m_SE = null;
		ParticleSystem m_particle_SE = null;
		[SerializeField]
		GameObject m_S = null;
		ParticleSystem m_particle_S = null;
		[SerializeField]
		GameObject m_SW = null;
		ParticleSystem m_particle_SW = null;
		[SerializeField]
		GameObject m_W = null;
		ParticleSystem m_particle_W = null;
		[SerializeField]
		GameObject m_NW = null;
		ParticleSystem m_particle_NW = null;

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		public List<TileLink> m_tileLinks = null;

		[SerializeField]
		private ParticleSystem m_combo_Particles = null;

        [SerializeField]
        private ParticleSystem m_flash_particles = null;

		public ParticleSystem ComboParticles { get { return m_combo_Particles; } }

		[SerializeField]
		private ParticleSystem m_linkedparticles = null;

		public ParticleSystem LinkedParticles { get { return m_linkedparticles; } }

		public CardTweenAnimations m_tweens;

        private bool m_isCrunchable = false;
        public bool IsCrunchableAnimation { get { return m_isCrunchable; } set
            {
                m_isCrunchable = value;
                if (m_isCrunchable)
                {
                    m_tweens.Crunchable.StartTween(CrunchableAnimationEnd);
                }
            }
        }

		private void Awake()
		{
			FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;

			m_particle_N  = m_N .GetComponentInChildren<ParticleSystem>();
			m_particle_NE = m_NE.GetComponentInChildren<ParticleSystem>();
			m_particle_E  = m_E .GetComponentInChildren<ParticleSystem>();
			m_particle_SE = m_SE.GetComponentInChildren<ParticleSystem>();
			m_particle_S  = m_S .GetComponentInChildren<ParticleSystem>();
			m_particle_SW = m_SW.GetComponentInChildren<ParticleSystem>();
			m_particle_W  = m_W .GetComponentInChildren<ParticleSystem>();
			m_particle_NW = m_NW.GetComponentInChildren<ParticleSystem>();
			SetLinkParticles(ArrowFlag.N, false);
			SetLinkParticles(ArrowFlag.NE, false);
			SetLinkParticles(ArrowFlag.E, false);
			SetLinkParticles(ArrowFlag.SE, false);
			SetLinkParticles(ArrowFlag.S, false);
			SetLinkParticles(ArrowFlag.SW, false);
			SetLinkParticles(ArrowFlag.W, false);
			SetLinkParticles(ArrowFlag.NW, false);

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

		public void SetLinkParticles(ArrowFlag flag, bool enabled)
		{
			switch (flag)
			{
				case ArrowFlag.N:
					m_particle_N.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.NE:
					m_particle_NE.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.E:
					m_particle_E.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.SE:
					m_particle_SE.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.S:
					m_particle_S.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.SW:
					m_particle_SW.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.W:
					m_particle_W.gameObject.SetActive(enabled);
					break;
				case ArrowFlag.NW:
					m_particle_NW.gameObject.SetActive(enabled);
					break;
			}
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

            var currentPos = transform.position;
            currentPos.z = c_thirdLevel;
            transform.position = currentPos;

			var parameters = m_tweens.PlaySlide.m_parameters;
			parameters.PositionStart = transform.position;
			target.z = c_thirdLevel;
			parameters.PositionEnd = target;

			m_tweens.Play.StartTween(OnGoToSlotEnd);
		}

		private void OnGoToSlotEnd()
		{
			m_currentState = TileState.Played;
            var pos = transform.position;
            pos.z = c_firstLevel;
            transform.position = pos;

			if (m_onGoToSlotEnd != null)
				m_onGoToSlotEnd();
		}

        private void CrunchableAnimationEnd()
        {
            if (m_isCrunchable)
            {
                m_tweens.Crunchable.StartTween(CrunchableAnimationEnd);
            }
        }

        public System.Collections.IEnumerator FlashWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_flash_particles.Play();
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
            public SingleTween Crunchable;
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