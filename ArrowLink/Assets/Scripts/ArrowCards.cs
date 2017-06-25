using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCards: MonoBehaviour
	{
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

		ArrowFlags m_arrows = ArrowFlags.NONE;

		[SerializeField]
		SingleUITween m_goToSlot = null;
		public SingleUITween GoToSlotTween { get { return m_goToSlot; } }

		private void Awake()
		{
			RandomizeArrow();
		}

		public void SetArrows(ArrowFlags flags)
		{
			m_arrows = flags;
			m_N .SetActive((flags & ArrowFlags.N ) == ArrowFlags.N );
			m_NE.SetActive((flags & ArrowFlags.NE) == ArrowFlags.NE);
			m_E .SetActive((flags & ArrowFlags.E ) == ArrowFlags.E);
			m_SE.SetActive((flags & ArrowFlags.SE) == ArrowFlags.SE);
			m_S .SetActive((flags & ArrowFlags.S ) == ArrowFlags.S);
			m_SW.SetActive((flags & ArrowFlags.SW) == ArrowFlags.SW);
			m_W .SetActive((flags & ArrowFlags.W ) == ArrowFlags.W );
			m_NW.SetActive((flags & ArrowFlags.NW) == ArrowFlags.NW);
		}
		const float c_chancesDecreaseFactor = .75f;
		public void RandomizeArrow()
		{

			List<ArrowFlags> available = new List<ArrowFlags> { ArrowFlags.N,ArrowFlags.NE,ArrowFlags.E,ArrowFlags.SE,ArrowFlags.S,ArrowFlags.SW,ArrowFlags.W,ArrowFlags.NW};

			m_arrows = ArrowFlags.NONE;
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
		}

	}
	[System.Flags]
	public enum ArrowFlags
	{
		NONE = 0,
		N  = 1,
		NE = 2,
		E  = 4,
		SE = 8,
		S  = 16,
		SW = 32,
		W  = 64,
		NW = 128,
	}
}