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

        ParticlesHolder m_basicParticles;
        ParticlesHolder m_superParticles;

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		public List<TileLink> m_tileLinks = null;

		[SerializeField]
		private ParticleSystem m_basicCenterParticles = null;

        [SerializeField]
        private ParticleSystem m_superCenterParticles = null;

        [SerializeField]
        private ParticleSystem m_flash_particles = null;

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

        private void Awake()
        {

            FlagDistributor distribuor = GameProcess.Instance.FlagDistributor;


            List<ParticleSystem> tempList = new List<ParticleSystem>(2);
            m_basicParticles.Center = m_basicCenterParticles;
            m_superParticles.Center = m_superCenterParticles;

            m_N.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.N = tempList[0];
            m_superParticles.N = tempList[1];
            m_NE.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.NE = tempList[0];
            m_superParticles.NE = tempList[1];
            m_E.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.E = tempList[0];
            m_superParticles.E = tempList[1];
            m_SE.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.SE = tempList[0];
            m_superParticles.SE = tempList[1];
            m_S.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.S = tempList[0];
            m_superParticles.S = tempList[1];
            m_SW.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.SW = tempList[0];
            m_superParticles.SW = tempList[1];
            m_W.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.W = tempList[0];
            m_superParticles.W = tempList[1];
            m_NW.GetComponentsInChildren<ParticleSystem>(true, tempList);
            m_basicParticles.NW = tempList[0];
            m_superParticles.NW = tempList[1];

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

            if (dirCount == 0)
            {
                m_basicCenterParticles.gameObject.SetActive(false);
                m_superCenterParticles.gameObject.SetActive(false);
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
                m_tweens.Crunchable.StartTween(CrunchableAnimationEnd);
            }
        }

        public System.Collections.IEnumerator FlashWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_flash_particles.Play();
            StartCoroutine(SwitchToSuperMode(.25f));
        }

        private System.Collections.IEnumerator SwitchToSuperMode(float delay)
        {
            yield return new WaitForSeconds(delay);
            IsSuperMode = true;
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

        [Serializable]
        public struct ParticlesHolder
        {
            int NbParticlesOn;

            public ParticleSystem Center;
            public ParticleSystem N;
            public ParticleSystem NE;
            public ParticleSystem E;
            public ParticleSystem SE;
            public ParticleSystem S;
            public ParticleSystem SW;
            public ParticleSystem W;
            public ParticleSystem NW;

            public void SetArrows(ArrowFlag flags)
            {
                NbParticlesOn = 0;

                N.gameObject.SetActive((flags & ArrowFlag.N) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.N) != ArrowFlag.NONE) ? 1 : 0;

                NE.gameObject.SetActive((flags & ArrowFlag.NE) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.NE) != ArrowFlag.NONE) ? 1 : 0;

                E.gameObject.SetActive((flags & ArrowFlag.E) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.E) != ArrowFlag.NONE) ? 1 : 0;

                SE.gameObject.SetActive((flags & ArrowFlag.SE) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.SE) != ArrowFlag.NONE) ? 1 : 0;

                S.gameObject.SetActive((flags & ArrowFlag.S) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.S) != ArrowFlag.NONE) ? 1 : 0;

                SW.gameObject.SetActive((flags & ArrowFlag.SW) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.SW) != ArrowFlag.NONE) ? 1 : 0;

                W.gameObject.SetActive((flags & ArrowFlag.W) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.W) != ArrowFlag.NONE) ? 1 : 0;

                NW.gameObject.SetActive((flags & ArrowFlag.NW) != ArrowFlag.NONE);
                NbParticlesOn += ((flags & ArrowFlag.NW) != ArrowFlag.NONE) ? 1 : 0;

            }

            public void SetLinkParticles(ArrowFlag flag, bool enabled)
            {
                switch (flag)
                {
                    case ArrowFlag.N:
                        _SetParticles(N.gameObject, enabled);
                        break;
                    case ArrowFlag.NE:
                        _SetParticles(NE.gameObject, enabled);
                        break;
                    case ArrowFlag.E:
                        _SetParticles(E.gameObject, enabled);
                        break;
                    case ArrowFlag.SE:
                        _SetParticles(SE.gameObject, enabled);
                        break;
                    case ArrowFlag.S:
                        _SetParticles(S.gameObject, enabled);
                        break;
                    case ArrowFlag.SW:
                        _SetParticles(SW.gameObject, enabled);
                        break;
                    case ArrowFlag.W:
                        _SetParticles(W.gameObject, enabled);
                        break;
                    case ArrowFlag.NW:
                        _SetParticles(NW.gameObject, enabled);
                        break;
                }
                
            }

            private void _SetParticles(GameObject go, bool enabled)
            {
                bool prev = go.activeSelf;
                go.SetActive(enabled);
                if (prev != enabled)
                    NbParticlesOn += enabled ? 1 : -1;
            }

            public void SetCenter(bool isOn)
            {
                Center.gameObject.SetActive(isOn);
            }

            public void SwapParticles(ParticlesHolder other)
            {
                _SetParticles(N .gameObject, other.N.gameObject.activeSelf );
                _SetParticles(NE.gameObject, other.NE.gameObject.activeSelf);
                _SetParticles(E .gameObject, other.E.gameObject.activeSelf);
                _SetParticles(SE.gameObject, other.SE.gameObject.activeSelf);
                _SetParticles(S .gameObject, other.S.gameObject.activeSelf);
                _SetParticles(SW.gameObject, other.SW.gameObject.activeSelf);
                _SetParticles(W .gameObject, other.W.gameObject.activeSelf);
                _SetParticles(NW.gameObject, other.NW.gameObject.activeSelf);
                other.SetArrows(ArrowFlag.NONE);
                other.SetCenter(false);
                SetCenter(NbParticlesOn > 0);
            }
        }
    }

}