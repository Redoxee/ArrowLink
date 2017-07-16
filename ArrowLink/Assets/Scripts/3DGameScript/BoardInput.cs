using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class BoardInput : MonoBehaviour {

		[SerializeField]
		public BoardSlot[] m_slots;

		//public void OnTilePressed(int tileIndex)
		//{
		//	Debug.LogFormat("Tile Pressed {0}", tileIndex);
		//}

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
