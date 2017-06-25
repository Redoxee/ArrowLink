using UnityEngine.UI;
using UnityEngine;
using System;

namespace ArrowLink
{
	public class CardSlot : MonoBehaviour
	{

		Button m_button;
		public Button Button
		{
			get
			{
				if (m_button == null)
				{
					m_button = GetComponent<Button>();
				}
				return m_button;
			}
		}

	}
}