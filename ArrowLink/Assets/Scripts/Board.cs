using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class Board : MonoBehaviour
	{
		const int c_nbSlot = 16;
		const int c_nbColumn = 4;

		List<CardSlot> m_slots;

		Action<CardSlot, int> m_onTilePressed = null;
		public Action<CardSlot, int> OnTilePressed { set { m_onTilePressed = value; } }

		private void Awake()
		{
			m_slots = new List<CardSlot>(c_nbSlot);

			for (int i = 0; i < transform.childCount; ++i)
			{
				int index = i;
				var slot = transform.GetChild(i).GetComponent<CardSlot>();

				slot.Button.onClick.RemoveAllListeners();
				slot.Button.onClick.AddListener(() => OnSlotPressed(slot, index));
				m_slots.Add(slot);
			}
		}

		void OnSlotPressed(CardSlot slot, int id)
		{
			m_onTilePressed(slot, id);
		}
	}
}