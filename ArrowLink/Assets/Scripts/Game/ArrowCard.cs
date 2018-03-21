using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCard : MonoBehaviour
	{
		public const int c_firstLevel = 0;
		public const int c_secondLevel = -10;
		public const int c_thirdLevel = -20;

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

        private Dictionary<ArrowFlag, GameObject> m_branchDictionary = new Dictionary<ArrowFlag, GameObject>(8);

        [SerializeField]
        ParticlesHolder m_basicParticles = null;
        [SerializeField]
        ParticlesHolder m_superParticles = null;

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		public List<TileLink> m_tileLinks = null;

        [SerializeField]
        GameObject m_centerRef = null;

        [SerializeField]
        private ParticleSystem m_flash_particles = null;

		[SerializeField]
		private ParticleSystem m_inComboparticles = null;

		public ParticleSystem InComboparticles { get { return m_inComboparticles; } }

		public CardTweenAnimations m_tweens;

        [SerializeField]
        private BoxCollider2D m_trigger = null;

        private bool m_isWiggling = false;
        public bool IsWigglingAnimation { get { return m_isWiggling; } set
            {
                m_isWiggling = value;
                if (m_isWiggling)
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
        
        private FlagDistributor FlagDistributor
        {
            get
            {

                if (GameProcess.Instance)
                    return GameProcess.Instance.FlagDistributor;
                else if (MainMenu.Instance)
                    return MainMenu.Instance.FlagDistributor;
                else if (TestFlag.Instance)
                    return TestFlag.Instance.Distributor;
                return null;
            }
        }

        private void Awake()
        {

            FlagDistributor distribuor = FlagDistributor;

            GrabLinkTweens();
		}

        private void OnDestroy()
		{
			FlagDistributor distribuor = FlagDistributor;
            if(distribuor)
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
            
            FlagDistributor distribuor = FlagDistributor;
            if (distribuor)
                distribuor.RegisterUsedFlag(m_arrows);
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

            m_trigger.enabled = false;

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
            if (m_isWiggling)
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

        public void FlashIntoSuperMode()
        {
            m_flash_particles.Play();
            StartCoroutine(SwitchToSuperMode(.2f));
        }

        private System.Collections.IEnumerator SwitchToSuperMode(float delay)
        {
            yield return new WaitForSeconds(delay);
            IsSuperMode = true;
        }

        public void RemoveArrow(ArrowFlag direction)
        {
            m_tweens.LinkTweens[direction].Shrink.StartTween();

            FlagDistributor distribuor = FlagDistributor;

            distribuor.UnregisterUsedFlag(m_arrows);
            m_arrows = (ArrowFlag)((int)m_arrows - (int)direction);
            distribuor.RegisterUsedFlag(m_arrows);

        }

        public void FadeOutNonLink()
        {
            m_tweens.BackFadeOut.StartTween();
            m_tweens.CenterFadeOut.StartTween();
            var allFlags = ArrowFlagExtension.AllFlags;
            foreach (var arrow in allFlags)
            {
                if ((arrow & m_arrows) != ArrowFlag.NONE)
                {
                    m_tweens.LinkTweens[arrow].FadeOut.StartTween();
                }
            }

        }

        public void SoftStopParticles()
        {
            m_basicParticles.CancelParticlesLoop();
            m_superParticles.CancelParticlesLoop();
        }

        private float m_linkedRemanance = .5f;
        private float m_timeBeforeDeath = 5f;
        public void SoftDestroy()
        {
            FadeOutNonLink();
            StartCoroutine(SoftStopParticleDelayed(m_linkedRemanance));
            StartCoroutine(DelayedDestroy(m_timeBeforeDeath));
        }

        public void SoftDestroy(float delay)
        {
            StartCoroutine(_SoftDestroyCoroutine(delay));
        }

        private IEnumerator _SoftDestroyCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            SoftDestroy();
        }

        private IEnumerator SoftStopParticleDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SoftStopParticles();
        }

        private IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        public void MoveToPosition(Vector3 target,Action onMoved = null)
        {
            var currentPos = transform.position;
            currentPos.z = target.z;
            transform.position = currentPos;

            var tween = m_tweens.MoveTween;
            tween.StopTween();
            tween.m_parameters.PositionStart = currentPos;
            tween.m_parameters.PositionEnd = target;
            tween.StartTween(onMoved);
        }

        public GameObject GetArrowObject(ArrowFlag direction)
        {
            return m_branchDictionary[direction];
        }

        [System.Serializable]
		public struct CardTweenAnimations
		{
			[Header("Tween To Play")]
			public BaseTween Introduction;
			public BaseTween Activation;
			public BaseTween Play;
            public SingleTween MoveTween;

			[Header("Details")]
			public SingleTween IntroductionSlide;
			public SingleTween ActivationSlide;
			public SingleTween ActivationUnveil;
            public SingleTween ActivationVeil;
			public SingleTween PlaySlide;
            public SingleTween Crunchable;
            
            public Dictionary<ArrowFlag, LinkTweens> LinkTweens;

            public BaseTween BackFadeOut;
            public BaseTween CenterFadeOut;
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
            m_branchDictionary[ArrowFlag.N] = m_N;
            m_branchDictionary[ArrowFlag.NE] = m_NE;
            m_branchDictionary[ArrowFlag.E] = m_E;
            m_branchDictionary[ArrowFlag.SE] = m_SE;
            m_branchDictionary[ArrowFlag.S] = m_S;
            m_branchDictionary[ArrowFlag.SW] = m_SW;
            m_branchDictionary[ArrowFlag.W] = m_W;
            m_branchDictionary[ArrowFlag.NW] = m_NW;

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

        private void OnMouseUpAsButton()
        {
            if(GameProcess.Instance)
                GameProcess.Instance.OnArrowCardPressed(this);
        }

        [Serializable]
        public class LinkTweens
        {
            public BaseTween Shrink;
            public SingleTween Nudge;
            public BaseTween FadeOut;

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
                Nudge = (SingleTween)tweens[1];
                FadeOut = tweens[2];
            }

            void _OnNudgeEnd()
            {
                if (m_isNudging)
                    Nudge.StartTween(null, true);
            }
        }
    }

}