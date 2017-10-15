using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	public class GameProcess : MonoBehaviour
	{
		private static GameProcess s_instance = null;
		public static GameProcess Instance {  get { return s_instance; } }

		//const float c_comboDuration = 5f;
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
		//float m_comboTimer = -1f;
		HashSet<LogicTile> m_currentCombo = new HashSet<LogicTile>();

		private int m_currentScore = 0;
		public int CurrentScore { get { return m_currentScore; } }

		private int m_currentTileScore = 0;
		public int CurrentTileScore { get { return m_currentTileScore; } }

		int m_nbCardOnTheWay = 0;

        private const int c_NbMatchToCrunch = 8;
        private int m_matchBeforeCrunch = 0;

        public const int c_startingAvailableTile = 24;
        public const int c_maxAvailableTile = 32;
        public const int c_minComboToGainTiles = 5;

        private int m_nbCurrentAvailableTile = c_startingAvailableTile;
        private int NbAvailableTile
        {
            get { return m_nbCurrentAvailableTile; }
            set
            {
                m_nbCurrentAvailableTile = value;
                if (m_nbCurrentAvailableTile > c_maxAvailableTile)
                    m_nbCurrentAvailableTile = c_maxAvailableTile;
                m_guiManager.NotifyAvailableTileCountChanged(m_nbCurrentAvailableTile);
            }
        }



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

			m_currentScore = 0;
			m_nbCardOnTheWay = 0;

            m_matchBeforeCrunch = 0;
            m_guiManager.NotifyCrunchProgressChanged(0);
            UpdateBankUI();
            NbAvailableTile = c_startingAvailableTile;
		}

		private void Update()
		{
			m_currentState.ProcessPlayedSlot();
		}

		void DrawNextCard()
		{
            if (m_nextCard != null)
            {
                m_currentCard = m_nextCard;

                m_currentCard.PrepareActivationTween(m_playingCardTransform.position);
                var currentCardTween = m_currentCard.m_tweens.Activation;
                var unveilTween = m_currentCard.m_tweens.ActivationUnveil;
                currentCardTween.StartTween(null);
                unveilTween.StartTween();
                m_nextCard = null;
            }

            if (m_nbCurrentAvailableTile < 1)
                return;

            if (m_currentCard == null)
            {
                ShootNewCurrentCard();
                if (NbAvailableTile > 0)
                    NbAvailableTile = NbAvailableTile - 1;
            }

            if (m_nbCurrentAvailableTile < 1)
                return;

            if (m_nextCard == null)
            {
                ShootNewNextCard();
                if (NbAvailableTile > 0)
                    NbAvailableTile = NbAvailableTile - 1;
            }
		}

        private void ShootNewCurrentCard()
        {
            GameObject currentCardObject = Instantiate (m_cardPrefab);
            m_currentCard = currentCardObject.GetComponent<ArrowCard>();
            m_currentCard.transform.position = m_playingCardTransform.position;
            m_currentCard.PrepareIntroductionTween();
            var introTween = m_currentCard.m_tweens.Introduction;
            var unveilTween = m_currentCard.m_tweens.ActivationUnveil;
            introTween.StartTween();
            unveilTween.StartTween();
        }

        private void ShootNewNextCard()
        {
            GameObject nextCardObject = Instantiate(m_cardPrefab);
            m_nextCard = nextCardObject.GetComponent<ArrowCard>();
            m_nextCard.transform.position = m_nextPlayingCardTransform.position;
            m_nextCard.PrepareIntroductionTween();
            var introTween = m_nextCard.m_tweens.Introduction;
            introTween.StartTween();
        }

        public void OnTilePressed(BoardSlot slot)
		{
			m_playedSlot = slot;
		}

        private void DefaultProcessPlayedSlot()
        {
            if ((m_playedSlot != null) && !m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y) && m_currentCard != null)
            {
                var logicTile = m_boardLogic.AddTile(m_playedSlot.X, m_playedSlot.Y, m_currentCard.MultiFlags);
                logicTile.m_physicCardRef = m_currentCard;

                Action cardPlayedAction = () => { CardTweenToSlotEnd(logicTile); }; // garbage here

                m_currentCard.GoToSlot(m_playedSlot, cardPlayedAction);
                m_nbCardOnTheWay += 1;
                m_currentCard = null;
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
                    entry.Value.m_physicCardRef.SwitchArrow(direction,false);
                    if (entry.Value.m_listLinkedTile.Count == 1)
                    {
                        entry.Value.m_physicCardRef.SwitchCenter(false);
                    }
                }
                m_boardLogic.RemoveTile(m_playedSlot.X, m_playedSlot.Y);
                Destroy(tile.m_physicCardRef.gameObject);
                SetState(DefaultState);
            }
            m_playedSlot = null;
            m_matchBeforeCrunch = c_NbMatchToCrunch;
            m_guiManager.NotifyCrunchProgressChanged(1);
        }

        public const float c_flashDelay = .1f;

        void CardTweenToSlotEnd(LogicTile tile)
		{
			m_nbCardOnTheWay -= 1;

			var card = tile.m_physicCardRef;

			var pos = card.transform.position;
			pos.z = ArrowCard.c_firstLevel;
			card.transform.position = pos;

			HashSet<LogicTile> chain = new HashSet<LogicTile>();
            List<LogicTile> chainAsList = new List<LogicTile>();
            List<float> depthList = new List<float>();

			m_boardLogic.ComputeTileNeighbor(tile);
            tile.ComputeLinkedChain(ref chain, ref chainAsList, ref depthList);
            int chainCount = chain.Count;
            
            //string s = ""; foreach (var d in depthList) s += d.ToString() + ",";
            //Debug.Log(s);
            

			if (chainCount >= c_comboMin)
			{
				m_currentCombo.UnionWith(chain);

                int combo = ComputeComboPoint();
                m_guiManager.NotifyDeltaScoreChanged(combo);

                foreach(LogicTile ct in m_currentCombo)
                {
                    int manhatanDistance = Mathf.Abs(tile.X - ct.X) + Mathf.Abs(tile.Y - ct.Y);
                    StartCoroutine(ct.m_physicCardRef.FlashWithDelay(manhatanDistance * c_flashDelay));
                }

            }

			card.m_tileLinks = new List<TileLink>(tile.m_listLinkedTile.Count);
			var p1 = tile.m_physicCardRef.transform.position;

			if (tile.m_listLinkedTile.Count > 0)
			{
				//tile.m_physicCardRef.InComboparticles.Play();

                tile.m_physicCardRef.LigthArrows(tile.m_linkedTile.Keys);
                tile.m_physicCardRef.SwitchCenter(true);

                foreach (var entry in tile.m_linkedTile)
                {
                    var neighbor = entry.Value;
                    var dir = entry.Key.Reverse();
                    neighbor.m_physicCardRef.SwitchArrow(dir,true);
                    neighbor.m_physicCardRef.SwitchCenter(true);
                }
                
			}

			CheckEndGame();
            UpdateBankUI();

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

		void EndCombo()
		{
			int comboPoints = ComputeComboPoint();
            int nbTileMatched = m_currentCombo.Count;

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
            if (m_matchBeforeCrunch > 0)
            {
                m_matchBeforeCrunch -= 1;

                m_guiManager.NotifyCrunchProgressChanged((float)m_matchBeforeCrunch / (float)c_NbMatchToCrunch);
            }

            if (nbTileMatched >= c_minComboToGainTiles)
            {
                NbAvailableTile += nbTileMatched + 1;
            }

            if (m_currentCard == null)
            {
                DrawNextCard();
            }

            UpdateBankUI();
            CheckEndGame();

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

            //if (m_comboTimer > 0)
            //    return;
            if (m_currentCombo.Count >= c_comboMin)
                return;
            if (m_nbCardOnTheWay > 0)
                return;

            if (m_boardLogic.IsBoardFull())
            {
                if (m_matchBeforeCrunch > 0)
                {
                    m_guiManager.NotifyEndGame();
                }
                return;
            }
            if (m_nbCurrentAvailableTile < 1)
            {
                if (m_currentCard != null || m_nextCard != null)
                {
                    return;
                }
                m_guiManager.NotifyEndGame();
            }

		}

        public void RequestBank()
        {
            if (m_currentCombo.Count >= c_comboMin)
            {
                EndCombo();
            }
        }

        public void RequestTileCrunchToggle()
        {
            if (m_currentState == DefaultState)
            {
                if (CheckCrunchAllowance())
                {
                    SetState(TileCrunchState);
                }
            }
            else if (m_currentState == TileCrunchState)
            {
                SetState(DefaultState);
            }
        }

        public bool CheckCrunchAllowance()
        {
            if (m_matchBeforeCrunch > 0)
                return false;
            //if (m_comboTimer > 0)
            //    return false;
            if (m_boardLogic.IsBoardEmpty())
                return false;
            return true;
        }

        private void UpdateBankUI()
        {
            var chains = m_boardLogic.GetAllChains();
            int maxChainCount = 0;
            foreach (var chain in chains)
            {
                if (chain.Count > maxChainCount)
                    maxChainCount = chain.Count;
            }


            var currentProgress = (float)(double)maxChainCount / c_comboMin;
            currentProgress = Mathf.Clamp01(1 - currentProgress);
            m_guiManager.NotifyBankVeilChanged(currentProgress);
        }

        #region FSM

        private GameState m_currentState;
        private void SetState(GameState nextState)
        {
            if (m_currentState.End != null)
                m_currentState.End();
            m_currentState = nextState;
            if (m_currentState.Start != null)
                m_currentState.Start();
        }

        private GameState DefaultState;
        private GameState TileCrunchState;

        private struct GameState
		{
			public Action ProcessPlayedSlot;
            public Action Start;
            public Action End;

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
            TileCrunchState = new GameState { ProcessPlayedSlot = TileCrunchProcessSlot, Start = _TileCrunchStateStart, End = _TileCrunchStateEnd };
            m_currentState = DefaultState;
        }

        private void _TileCrunchStateStart()
        {
            foreach (var tile in m_boardLogic.AllTilePlaced)
            {
                tile.m_physicCardRef.IsCrunchableAnimation = true;
            }
        }

        private void _TileCrunchStateEnd()
        {

            foreach (var tile in m_boardLogic.AllTilePlaced)
            {
                tile.m_physicCardRef.IsCrunchableAnimation = false;
            }
        }

    }
		#endregion
}

