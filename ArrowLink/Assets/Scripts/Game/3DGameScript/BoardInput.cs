using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArrowLink
{
	public class BoardInput : MonoBehaviour {

		[SerializeField]
		public BoardSlot[] m_slots;

		public void Initialize(GameProcess gp)
		{
			for (int i = 0; i < m_slots.Length; ++i)
			{
				m_slots[i].X = i % BoardLogic.c_col;
				m_slots[i].Y = i / BoardLogic.c_col;
			}
		}


        public BoardSlot GetSlot(int x, int y)
        {
            return (m_slots[y * BoardLogic.c_col + x]);
        }
	}
}
