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
		[SerializeField]
		Transform m_nextPlayingCardTransform = null;

		ArrowCards m_currentCard = null;
		ArrowCards m_nextCard = null;

		int m_waitingForAnimation = 0;

		private void Start()
		{
			m_board.OnTilePressed = OnTilePressed;
			DrawNextCard();
		}

		void DrawNextCard()
		{

			GameObject nextCardObject = Instantiate(m_cardPrefab);
			nextCardObject.transform.SetParent (m_playingCanvas.transform,false);
			GameObject currentCardObject;

			if (m_nextCard == null) {
				currentCardObject = Instantiate (m_cardPrefab);
				currentCardObject.transform.SetParent (m_playingCanvas.transform,false);
				m_currentCard = currentCardObject.GetComponent<ArrowCards> ();

			} else {
				m_currentCard = m_nextCard;
				currentCardObject = m_currentCard.gameObject;
			}

			m_nextCard = nextCardObject.GetComponent<ArrowCards> ();
			m_nextCard.Hide ();

			var currentCardParameters = m_currentCard.ToPlayableTween.GetParameters()[0];
			currentCardParameters.PositionStart = m_nextPlayingCardTransform.position;
			currentCardParameters.PositionEnd = m_playingCardTransform.position;
			m_waitingForAnimation -= 1;
			m_currentCard.ToPlayableTween.StartTween (FadeInNextCard);


			nextCardObject.transform.position = m_nextPlayingCardTransform.position;

		}

		void FadeInNextCard()
		{
			
			m_nextCard.FadeInTween.StartTween(_TweenEnded);
		}

		void _TweenEnded()
		{
			m_waitingForAnimation += 1;
			if(m_waitingForAnimation > 0)
			{
				Debug.LogWarning ("You need a FSM mate, problems in tween callbacks");
				m_waitingForAnimation = 0;
			}

		}

		void OnTilePressed(CardSlot slot, int slotId)
		{
			if (m_waitingForAnimation < 0)
				return;

			var tween = m_currentCard.GoToSlotTween;
			tween.Parameters.PositionStart = m_playingCardTransform.position;
			tween.Parameters.PositionEnd = slot.transform.position;
			m_waitingForAnimation -= 1;
			tween.StartTween(_TweenEnded);

			DrawNextCard();
		}

	}
}
