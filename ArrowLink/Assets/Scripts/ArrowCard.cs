using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCard : MonoBehaviour
	{
		public const int c_firstLevel = -10;
		public const int c_secondLevel = -20;
		public const int c_thirdLevel = -30;

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

		public CardTweenAnimations m_tweens;

		private void Awake()
		{
			RandomizeArrow();
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
		const float c_chancesDecreaseFactor = .75f;
		public void RandomizeArrow()
		{

			List<ArrowFlag> available = new List<ArrowFlag> { ArrowFlag.N, ArrowFlag.NE, ArrowFlag.E, ArrowFlag.SE, ArrowFlag.S, ArrowFlag.SW, ArrowFlag.W, ArrowFlag.NW };

			m_arrows = ArrowFlag.NONE;
			int direction = Random.Range(0, available.Count);
			m_arrows = m_arrows | available[direction];
			available.RemoveAt(direction);

			float chances = 1.0f;
			for (int i = 0; i < 7; ++i)
			{
				chances *= c_chancesDecreaseFactor;
				float pull = Random.Range(0f, 1f);
				if (pull > chances)
				{
					break;
				}

				direction = Random.Range(0, available.Count);
				m_arrows = m_arrows | available[direction];
				available.RemoveAt(direction);
			}

			SetArrows(m_arrows);
			//SetArrows(ArrowFlag.NE | ArrowFlag.NW | ArrowFlag.SW | ArrowFlag.SE);
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

		public void PreparePlayTween(Vector3 target)
		{
			var parameters = m_tweens.PlaySlide.m_parameters;
			parameters.PositionStart = transform.position;
			target.z = transform.position.z;
			parameters.PositionEnd = target;
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
	}

}