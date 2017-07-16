using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class ArrowCards : MonoBehaviour
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

		ArrowFlag m_arrows = ArrowFlag.NONE;
		public ArrowFlag MultiFlags { get { return m_arrows; } }

		[SerializeField]
		SingleUITween m_goToSlot = null;
		public SingleUITween GoToSlotTween { get { return m_goToSlot; } }

		[SerializeField]
		BaseUITween m_fadeInTween = null;
		public BaseUITween FadeInTween { get { return m_fadeInTween; } }

		[SerializeField]
		BaseUITween m_toPlayableTween = null;
		public BaseUITween ToPlayableTween { get { return m_toPlayableTween; } }


		CanvasGroup m_cGroupe = null;
		public void Hide()
		{
			if (m_cGroupe == null) {
				m_cGroupe = GetComponent<CanvasGroup>();
			}
			m_cGroupe.alpha = 0f;
		}

		private void Awake()
		{
			RandomizeArrow();
		}

		public void SetArrows(ArrowFlag flags)
		{
			m_arrows = flags;
			m_N .SetActive((flags & ArrowFlag.N ) == ArrowFlag.N );
			m_NE.SetActive((flags & ArrowFlag.NE) == ArrowFlag.NE);
			m_E .SetActive((flags & ArrowFlag.E ) == ArrowFlag.E);
			m_SE.SetActive((flags & ArrowFlag.SE) == ArrowFlag.SE);
			m_S .SetActive((flags & ArrowFlag.S ) == ArrowFlag.S);
			m_SW.SetActive((flags & ArrowFlag.SW) == ArrowFlag.SW);
			m_W .SetActive((flags & ArrowFlag.W ) == ArrowFlag.W );
			m_NW.SetActive((flags & ArrowFlag.NW) == ArrowFlag.NW);
		}
		const float c_chancesDecreaseFactor = .75f;
		public void RandomizeArrow()
		{

			List<ArrowFlag> available = new List<ArrowFlag> { ArrowFlag.N,ArrowFlag.NE,ArrowFlag.E,ArrowFlag.SE,ArrowFlag.S,ArrowFlag.SW,ArrowFlag.W,ArrowFlag.NW};

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
		}

	}
	[System.Flags]
	public enum ArrowFlag
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


public static class ArrowFlagExtension
	{

		public static ArrowFlag[] AllFlags = { ArrowFlag.N, ArrowFlag.NE, ArrowFlag.E, ArrowFlag.SE, ArrowFlag.S, ArrowFlag.SW, ArrowFlag.W, ArrowFlag.NW };

		public static bool Connect(this ArrowFlag a, ArrowFlag b)
		{
			switch (a)
			{
				case ArrowFlag.N:
					return b == ArrowFlag.S;
				case ArrowFlag.NE:
					return b == ArrowFlag.SW;
				case ArrowFlag.E:
					return b == ArrowFlag.W;
				case ArrowFlag.SE:
					return b == ArrowFlag.NW;
				case ArrowFlag.S:
					return b == ArrowFlag.N;
				case ArrowFlag.SW:
					return b == ArrowFlag.NE;
				case ArrowFlag.W:
					return b == ArrowFlag.E;
				case ArrowFlag.NW:
					return b == ArrowFlag.SE;
			}
			return false;
		}

		public static ArrowFlag Reverse(this ArrowFlag a)
		{
			switch (a)
			{
				case ArrowFlag.N:
					return ArrowFlag.S;
				case ArrowFlag.NE:
					return ArrowFlag.SW;
				case ArrowFlag.E:
					return ArrowFlag.W;
				case ArrowFlag.SE:
					return  ArrowFlag.NW;
				case ArrowFlag.S:
					return  ArrowFlag.N;
				case ArrowFlag.SW:
					return  ArrowFlag.NE;
				case ArrowFlag.W:
					return  ArrowFlag.E;
				case ArrowFlag.NW:
					return  ArrowFlag.SE;
			}
			return ArrowFlag.NONE;
		}

		public static void GetDecal(this ArrowFlag arrow, ref int x, ref int y)
		{
			switch (arrow)
			{
				case ArrowFlag.N:
					y ++;
					break;
				case ArrowFlag.NE:
					x ++; y ++;
					break;
				case ArrowFlag.E:
					x ++;
					break;
				case ArrowFlag.SE:
					x ++; y --;
					break;
				case ArrowFlag.S:
					y --;
					break;
				case ArrowFlag.SW:
					x--; y--;
					break;
				case ArrowFlag.W:
					x--;
					break;
				case ArrowFlag.NW:
					x--; y++;
					break;
			}
		}

		public static void Split(this ArrowFlag flags, ref ArrowFlag[] array, ref int count)
		{
			count = 0;
			if ((flags & ArrowFlag.N) == ArrowFlag.N)
				array[count++] = ArrowFlag.N;
			if ((flags & ArrowFlag.NE) == ArrowFlag.NE)
				array[count++] = ArrowFlag.NE;
			if ((flags & ArrowFlag.E) == ArrowFlag.E)
				array[count++] = ArrowFlag.E;
			if ((flags & ArrowFlag.SE) == ArrowFlag.SE)
				array[count++] = ArrowFlag.SE;
			if ((flags & ArrowFlag.S) == ArrowFlag.S)
				array[count++] = ArrowFlag.S;
			if ((flags & ArrowFlag.SW) == ArrowFlag.SW)
				array[count++] = ArrowFlag.SW;
			if ((flags & ArrowFlag.W) == ArrowFlag.W)
				array[count++] = ArrowFlag.W;
			if ((flags & ArrowFlag.NW) == ArrowFlag.NW)
				array[count++] = ArrowFlag.NW;
		}

		public static bool DoHave(this ArrowFlag a,ArrowFlag b)
		{
			return (a & b) == b;
		}
	}
}