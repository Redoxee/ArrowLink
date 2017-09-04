using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class GameProcess : MonoBehaviour
	{
		private static GameProcess s_instance = null;
		public static GameProcess Instance {  get { return s_instance; } }

		const float c_comboDuration = 5f;
		const int c_comboMin = 4;
		BoardLogic m_boardLogic;

		[SerializeField]
		GameObject m_cardPrefab = null;
		[SerializeField]
		GameObject m_linkPrefab = null;
		
		[SerializeField]
		BoardInput m_board = null;

		[SerializeField]
		GameCamera m_gameCamera = null;

		[SerializeField]
		GUIManager m_guiManager = null;

		[SerializeField]
		FlagDistributor m_flagDistributor = null;

		public FlagDistributor FlagDistributor { get { return m_flagDistributor; } }

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

		private int m_currentScore = 0;
		public int CurrentScore { get { return m_currentScore; } }

		private int m_currentTileScore = 0;
		public int CurrentTileScore { get { return m_currentTileScore; } }

		int m_nbCardOnTheWay = 0;

		private void Awake()
		{
			if (s_instance != null)
			{
				Debug.LogError("Several instance of game process created !");
				Destroy(this);
				return;
			}
			s_instance = this;
		}

		private void Start()
		{
			m_boardLogic = new BoardLogic();
            InitializeStates();

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

			m_currentScore = 0;
			m_nbCardOnTheWay = 0;
		}

		private void Update()
		{
			m_currentState.ProcessPlayedSlot();
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

        private void DefaultProcessPlayedSlot()
        {
            if ((m_playedSlot != null) && !m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y))
            {
                var logicTile = m_boardLogic.AddTile(m_playedSlot.X, m_playedSlot.Y, m_currentCard.MultiFlags);
                logicTile.m_physicCardRef = m_currentCard;

                Action cardPlayedAction = () => { CardTweenToSlotEnd(logicTile); }; // garbage here

                m_currentCard.GoToSlot(m_playedSlot, cardPlayedAction);
                m_nbCardOnTheWay += 1;
                DrawNextCard();
            }
            m_playedSlot = null;
        }

        private void TileCrunchProcessSlot()
        {
            if (m_playedSlot == null)
                return;
            if (m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y))
            {
                LogicTile tile = m_boardLogic.GetTile(m_playedSlot.X, m_playedSlot.Y);
                foreach (var entry in tile.m_linkedTile)
                {
                    var direction = entry.Key.Reverse();
                    entry.Value.m_physicCardRef.SetLinkParticles(direction, false);
                    if (entry.Value.m_listLinkedTile.Count == 1)
                    {
                        entry.Value.m_physicCardRef.LinkedParticles.Stop();
                    }
                }
                m_boardLogic.RemoveTile(m_playedSlot.X, m_playedSlot.Y);
                Destroy(tile.m_physicCardRef.gameObject);
                m_currentState = DefaultState;
            }
            m_playedSlot = null;
        }


        void CardTweenToSlotEnd(LogicTile tile)
		{
			m_nbCardOnTheWay -= 1;

			var card = tile.m_physicCardRef;

			var pos = card.transform.position;
			pos.z = ArrowCard.c_firstLevel;
			card.transform.position = pos;

			HashSet<LogicTile> chain = new HashSet<LogicTile>();
			m_boardLogic.ComputeTileNeighbor(tile);
			tile.GetLinkedChain(ref chain);

			if (chain.Count >= c_comboMin)
			{
				m_comboTimer = c_comboDuration;
				m_currentCombo.UnionWith(chain);

				foreach (var comboCard in m_currentCombo)
				{
					comboCard.m_physicCardRef.ComboParticles.Play(true);
				}
			}

			card.m_tileLinks = new List<TileLink>(tile.m_listLinkedTile.Count);
			var p1 = tile.m_physicCardRef.transform.position;
			if (tile.m_listLinkedTile.Count > 0)
			{
				tile.m_physicCardRef.LinkedParticles.Play();

				foreach (var entry in tile.m_linkedTile)
				{
					ArrowFlag dir = entry.Key;
					tile.m_physicCardRef.SetLinkParticles(dir, true);
					entry.Value.m_physicCardRef.SetLinkParticles(dir.Reverse(), true);
					entry.Value.m_physicCardRef.LinkedParticles.Play();
				}
			}

			CheckEndGame();
		}


		TileLink CreateTileLink(Vector3 p1, Vector3 p2)
		{
			var linkObject = Instantiate(m_linkPrefab);
			var link = linkObject.GetComponent<TileLink>();

			var linkRotation = Quaternion.FromToRotation(Vector3.up, (p2 - p1));
			link.transform.rotation = linkRotation;
			var linkPosition = (p1 + p2) / 2;
			linkPosition.z -= 10;

			link.transform.position = linkPosition;
			if (p1.x != p2.x && p1.y != p2.y)
			{
				link.transform.localScale = link.c_diagonalScale;
			}

			return link;
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
					EndCombo();
				}
			}
		}

		void EndCombo()
		{
			int comboPoints = ComputeComboPoint();
			m_currentScore += comboPoints;
			m_currentTileScore += m_currentCombo.Count;

			m_guiManager.NotifyScoreChanged(m_currentScore, comboPoints);

			foreach (var tile in m_currentCombo)
			{
				m_boardLogic.RemoveTile(tile.X, tile.Y);
				var card = tile.m_physicCardRef;
				foreach (var link in card.m_tileLinks)
				{
					Destroy(link.gameObject);
				}

				Destroy(card.gameObject);
			}
			m_currentCombo.Clear();
		}

		const int c_baseComboPoints = 10;
		const int c_comboCurveStart = 5;

		int ComputeComboPoint()
		{
			int points = 0;

			int count = m_currentCombo.Count;

			points += count * c_baseComboPoints;
			count -= c_comboCurveStart;
			while (count > 0)
			{
				points += c_baseComboPoints * count;
				count--;
			}

			return points;
		}

		private void CheckEndGame()
		{
			if(m_boardLogic.IsBoardFull())
			{
				if (!(m_comboTimer > 0))
				{
					if(m_nbCardOnTheWay == 0)
						m_guiManager.NotifyEndGame();
				}
			}
		}

        public void RequestTileCrunchToggle()
        {
            if (m_currentState == DefaultState)
            {
                m_currentState = TileCrunchState;
            }
            else if (m_currentState == TileCrunchState)
            {
                m_currentState = DefaultState;
            }
        }

        #region FSM

        private GameState m_currentState;

        private GameState DefaultState;
        private GameState TileCrunchState;

        private struct GameState
		{
			public Action ProcessPlayedSlot;

            public override bool Equals(System.Object other)
            {
                if (!(typeof(GameState) == other.GetType()))
                    return false;
                return ProcessPlayedSlot == ((GameState)other).ProcessPlayedSlot;
            }
            public static bool operator ==(GameState a, GameState b)
            {
                return a.Equals(b);
            }
            public static bool operator !=(GameState a, GameState b)
            {
                return !a.Equals(b);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private void InitializeStates()
        {
            DefaultState = new GameState { ProcessPlayedSlot = DefaultProcessPlayedSlot };
            TileCrunchState = new GameState { ProcessPlayedSlot = TileCrunchProcessSlot };

            m_currentState = DefaultState;
        }

    }
		#endregion
}

