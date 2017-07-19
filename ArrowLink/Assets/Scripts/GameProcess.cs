using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class GameProcess : MonoBehaviour
	{
		BoardLogic m_boardLogic;

		[SerializeField]
		GameObject m_cardPrefab = null;
		
		[SerializeField]
		BoardInput m_board = null;

		[SerializeField]
		GameCamera m_gameCamera = null;

		[SerializeField]
		Transform m_playingCardTransform = null;
		[SerializeField]
		Transform m_nextPlayingCardTransform = null;

		ArrowCard m_currentCard = null;
		ArrowCard m_nextCard = null;

		private void Start()
		{
			m_boardLogic = new BoardLogic();

			m_gameCamera.Initialize();
			m_board.Initialize(this);

			var pos = m_playingCardTransform.position;
			pos.z = ArrowCard.c_secondLevel; ;
			m_playingCardTransform.position = pos;

			pos = m_nextPlayingCardTransform.position;
			pos.z = ArrowCard.c_firstLevel;
			m_nextPlayingCardTransform.position = pos;

			DrawNextCard();
		}

		void DrawNextCard()
		{

			GameObject nextCardObject = Instantiate(m_cardPrefab);
			GameObject currentCardObject;
			
			BaseTween currentCardTween = null;

			if (m_nextCard == null) {
				currentCardObject = Instantiate (m_cardPrefab);
				m_currentCard = currentCardObject.GetComponent<ArrowCard> ();
				m_currentCard.transform.position = m_playingCardTransform.position;
				m_currentCard.PrepareIntroductionTween();
				currentCardTween = m_currentCard.m_tweens.Introduction;

				var unveil = m_currentCard.m_tweens.ActivationUnveil;
				unveil.StartTween(null);

			} else {
				m_currentCard = m_nextCard;
				currentCardObject = m_currentCard.gameObject;

				currentCardTween = m_currentCard.m_tweens.Activation;
				m_currentCard.PrepareActivationTween(m_playingCardTransform.position);
			}

			m_nextCard = nextCardObject.GetComponent<ArrowCard> ();
			m_nextCard.transform.position = m_nextPlayingCardTransform.position ;

			BaseTween nextCardTween = m_nextCard.m_tweens.Introduction;
			m_nextCard.PrepareIntroductionTween();


			currentCardTween.StartTween(null);
			nextCardTween.StartTween(null);
		}

		public void OnTilePressed(BoardSlot slot, int x, int y)
		{
			if (m_boardLogic.IsFilled(x, y))
				return;
			var logicTile = m_boardLogic.AddTile(x, y, m_currentCard.MultiFlags);
			logicTile.m_physicCardRef = m_currentCard;

			var position = slot.transform.position;
			position.z = ArrowCard.c_thirdLevel;
			
			m_currentCard.PreparePlayTween(position);

			var cardRef = m_currentCard;
			Action cardPlayedAction = ()=> { CardTweenToSlotEnd(logicTile); }; // garbage here

			m_currentCard.m_tweens.Play.StartTween(cardPlayedAction);

			DrawNextCard();
		}

		void CardTweenToSlotEnd(BoardLogic.LogicTile tile)
		{

			var card = tile.m_physicCardRef;

			var pos = card.transform.position;
			pos.z = ArrowCard.c_firstLevel;
			card.transform.position = pos;

			HashSet<BoardLogic.LogicTile> chain = new HashSet<BoardLogic.LogicTile>();
			tile.GetLinkedChain(ref chain);
			Debug.Log(chain.Count);

		}

	}
}
