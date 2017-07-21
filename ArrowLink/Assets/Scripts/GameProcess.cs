using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class GameProcess : MonoBehaviour
	{
		const float c_comboDuration = 5f;

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

		[SerializeField]
		ComboGauge m_comboGauge = null;

		ArrowCard m_currentCard = null;
		ArrowCard m_nextCard = null;

		BoardSlot m_playedSlot;
		float m_comboTimer = -1f;
		HashSet<LogicTile> m_currentCombo = new HashSet<LogicTile>();
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

			m_comboGauge.SetProgression(0);
		}

		private void Update()
		{
			ProcessPlayedSlot();
			UpdateComboMeter();
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

		public void OnTilePressed(BoardSlot slot)
		{
			m_playedSlot = slot;
		}

		void ProcessPlayedSlot()
		{
			if ((m_playedSlot != null) && !m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y))
			{
				var logicTile = m_boardLogic.AddTile(m_playedSlot.X, m_playedSlot.Y, m_currentCard.MultiFlags);
				logicTile.m_physicCardRef = m_currentCard;

				var position = m_playedSlot.transform.position;
				position.z = ArrowCard.c_thirdLevel;

				m_currentCard.PreparePlayTween(position);
				
				Action cardPlayedAction = () => { CardTweenToSlotEnd(logicTile); }; // garbage here

				m_currentCard.m_tweens.Play.StartTween(cardPlayedAction);

				DrawNextCard();
			}
			m_playedSlot = null;
		}

		void CardTweenToSlotEnd(LogicTile tile)
		{

			var card = tile.m_physicCardRef;

			var pos = card.transform.position;
			pos.z = ArrowCard.c_firstLevel;
			card.transform.position = pos;

			HashSet<LogicTile> chain = new HashSet<LogicTile>();
			m_boardLogic.ComputeTileNeighbor(tile);
			tile.GetLinkedChain(ref chain);

			if (chain.Count > 3)
			{
				m_comboTimer = c_comboDuration;
				m_currentCombo.UnionWith(chain);
			}
		}

		void UpdateComboMeter()
		{
			if (m_comboTimer > 0)
			{
				m_comboTimer -= Time.deltaTime;
				float progression = Mathf.Clamp01(m_comboTimer / c_comboDuration);
				m_comboGauge.SetProgression(progression);
				if (m_comboTimer <= 0)
				{
					Debug.Log("Combo end !");
					foreach (var tile in m_currentCombo)
					{
						m_boardLogic.RemoveTile(tile.X, tile.Y);
						var card = tile.m_physicCardRef;
						Destroy(card.gameObject);
					}
					m_currentCombo.Clear();
				}
			}
		}
	}
}
