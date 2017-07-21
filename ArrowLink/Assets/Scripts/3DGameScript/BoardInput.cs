using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class BoardInput : MonoBehaviour {

		[SerializeField]
		public BoardSlot[] m_slots;

		GameProcess m_gameProcess = null;

		public void Initialize(GameProcess gp)
		{
			m_gameProcess = gp;

			for (int i = 0; i < m_slots.Length; ++i)
			{
				m_slots[i].X = i % BoardLogic.c_col;
				m_slots[i].Y = i / BoardLogic.c_col;
			}
		}

		public void OnTilePressed(int tileIndex)
		{
			m_gameProcess.OnTilePressed(m_slots[tileIndex]);
		}

		public void OnTileDown(int index)
		{
		}

		public void OnTileUp(int index)
		{
		}

		public void OnTileIn(int index)
		{
			m_slots[index].OnFocus();
		}

		public void OnTileOut(int index)
		{
			m_slots[index].OnUnfocus();
		}
	}
}
