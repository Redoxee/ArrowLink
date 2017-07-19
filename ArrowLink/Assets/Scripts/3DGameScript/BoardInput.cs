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
		}

		public void OnTilePressed(int tileIndex)
		{
			int x = tileIndex % BoardLogic.c_col;
			int y = tileIndex / BoardLogic.c_col;
			m_gameProcess.OnTilePressed(m_slots[tileIndex], x, y);
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
