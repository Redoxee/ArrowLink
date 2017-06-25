using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class GameProcess : MonoBehaviour
	{

		[SerializeField]
		GameObject m_cardPrefab = null;

		[SerializeField]
		GameObject m_playingCanvas = null;

		[SerializeField]
		Board m_board = null;

		[SerializeField]
		Transform m_playingCardTransform = null;

		ArrowCards m_currentCard = null;

		private void Start()
		{
			m_board.OnTilePressed = OnTilePressed;
			DrawNextCard();
		}

		void DrawNextCard()
		{
			var cardObject = Instantiate(m_cardPrefab);
			m_currentCard = cardObject.GetComponent<ArrowCards>();
			m_currentCard.transform.SetParent(m_playingCanvas.transform, false);

			m_currentCard.transform.position = m_playingCardTransform.position;
		}

		void OnTilePressed(CardSlot slot, int slotId)
		{
			//m_currentCard.transform.position = slot.transform.position;
			var tween = m_currentCard.GoToSlotTween;
			tween.Parameters.PositionStart = m_playingCardTransform.position;
			tween.Parameters.PositionEnd = slot.transform.position;
			tween.StartTween();

			DrawNextCard();
		}

	}
}
