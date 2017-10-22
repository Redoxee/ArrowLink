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
        

        [SerializeField]
        ParticlesHolder m_basicParticles = null;
        [SerializeField]
        ParticlesHolder m_superParticles = null;

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		public List<TileLink> m_tileLinks = null;

        [SerializeField]
        private ParticleSystem m_flash_particles = null;

		[SerializeField]
		private ParticleSystem m_inComboparticles = null;

		public ParticleSystem InComboparticles { get { return m_inComboparticles; } }

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

        private bool m_isSuperMode = false;
        public bool IsSuperMode { set
            {
                if (m_isSuperMode == value)
                    return;
                m_isSuperMode = value;
                if (m_isSuperMode)
                    m_superParticles.SwapParticles(m_basicParticles);
                else
                    m_basicParticles.SwapParticles(m_superParticles);
            }
        }

        public void SetLinkNudge(ArrowFlag direction, bool isNudging = true)
        {
            m_tweens.LinkTweens[direction].IsNudging = isNudging;
        }
        
        private void Awake()
        {

            FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;

            GrabLinkTweens();

			SetupArrows(distribuor.PickRandomFlags());
			distribuor.RegisterUsedFlag(m_arrows);
		}

        private void OnDestroy()
		{
			FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;
			distribuor.UnregisterUsedFlag(m_arrows);
		}

        public void SetupArrows(ArrowFlag flags)
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

            m_basicParticles.SetArrows(ArrowFlag.NONE);
            m_superParticles.SetArrows(ArrowFlag.NONE);
        }

        public void LigthArrows(IEnumerable<ArrowFlag> directions)
        {
            int dirCount = 0;
            ParticlesHolder holder = m_isSuperMode ? m_superParticles : m_basicParticles;
            foreach (var entry in directions)
            {
                dirCount += 1;
                holder.SetLinkParticles(entry, true);
            }
        }

        public void SwitchArrow(ArrowFlag flag, bool on)
        {
            ParticlesHolder holder = m_isSuperMode ? m_superParticles : m_basicParticles;

            holder.SetLinkParticles(flag, on);
        }

        public void SwitchCenter(bool on)
        {
            ParticlesHolder holder = m_isSuperMode ? m_superParticles : m_basicParticles;
            holder.SetCenter(on);
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
                m_tweens.Crunchable.StartTween(null,true);
            }
        }

        public System.Collections.IEnumerator FlashWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_flash_particles.Play();
            StartCoroutine(SwitchToSuperMode(.15f));
        }

        private System.Collections.IEnumerator SwitchToSuperMode(float delay)
        {
            yield return new WaitForSeconds(delay);
            IsSuperMode = true;
        }

        public void RemoveArrow(ArrowFlag direction)
        {
            m_tweens.LinkTweens[direction].Shrink.StartTween();

            FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;

            distribuor.UnregisterUsedFlag(m_arrows);
            m_arrows = (ArrowFlag)((int)m_arrows - (int)direction);
            distribuor.RegisterUsedFlag(m_arrows);

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
            
            public Dictionary<ArrowFlag, LinkTweens> LinkTweens;
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


        void GrabLinkTweens()
        {
            m_tweens.LinkTweens = new Dictionary<ArrowFlag, LinkTweens>(8);
            m_tweens.LinkTweens[ArrowFlag.N] = new LinkTweens(m_N);
            m_tweens.LinkTweens[ArrowFlag.NE] = new LinkTweens(m_NE);
            m_tweens.LinkTweens[ArrowFlag.E] = new LinkTweens(m_E);
            m_tweens.LinkTweens[ArrowFlag.SE] = new LinkTweens(m_SE);
            m_tweens.LinkTweens[ArrowFlag.S] = new LinkTweens(m_S);
            m_tweens.LinkTweens[ArrowFlag.SW] = new LinkTweens(m_SW);
            m_tweens.LinkTweens[ArrowFlag.W] = new LinkTweens(m_W);
            m_tweens.LinkTweens[ArrowFlag.NW] = new LinkTweens(m_NW);
            
        }



        [Serializable]
        public class LinkTweens
        {
            public BaseTween Shrink;
            public BaseTween Nudge;

            bool m_isNudging = false;
            public bool IsNudging
            { get { return m_isNudging; }
            set {
                    if (m_isNudging == value)
                        return;
                    m_isNudging = value;
                    if (value)
                        Nudge.StartTween(_OnNudgeEnd);
                }
            }

            public LinkTweens(GameObject source)
            {
                GrabTweens(source);
            }

            public void GrabTweens(GameObject source)
            {
                var tweens = source.GetComponents<BaseTween>();
                Shrink = tweens[0];
                Nudge = tweens[1];
            }

            void _OnNudgeEnd()
            {
                if (m_isNudging)
                    Nudge.StartTween(null, true);
            }
        }
    }

}