using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class GameProcess : MonoBehaviour
    {
        private static GameProcess s_instance = null;
        public static GameProcess Instance { get { return s_instance; } }

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

        [SerializeField]
        AnimatedLinePool m_animatedLinePool = null;
        public AnimatedLinePool AnimatedLinePool { get { return m_animatedLinePool; } }

        [SerializeField]
        Transform m_scoreTransform = null;

        ArrowCard m_currentCard = null;
        ArrowCard m_nextCard = null;

        BoardSlot m_playedSlot;
        ArrowCard m_pressedCard = null;


        private int m_currentScore = 0;
        public int CurrentScore { get { return m_currentScore; } }

        private int m_currentTileScore = 0;
        public int CurrentTileScore { get { return m_currentTileScore; } }

        int m_nbCardOnTheWay = 0;

        private const int c_crunchTarget = 4;
        private int m_crunchPoints = 0;

        public const int c_startingAvailableTile = 24;
        public const int c_maxAvailableTile = 32;
        public const int c_minComboToGainTiles = 5;

        private int m_bankPointTarget = 4;
        private int m_bankPoints = 0;
        public const int c_maxLinkPoints = 20;

        [SerializeField]
        private DotCollection m_bankDots = null;
        [SerializeField]
        private DotCollection m_crunchDots = null;


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

            m_crunchPoints = 0;

            NbAvailableTile = c_startingAvailableTile;

            m_bankDots.SetNumberOfDots(m_bankPointTarget);
            m_crunchDots.SetNumberOfDots(c_crunchTarget);
        }

        private void Update()
        {
            ProcessPressedCard();

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
            GameObject currentCardObject = Instantiate(m_cardPrefab);
            currentCardObject.SetActive(true);
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
            nextCardObject.SetActive(true);
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

        public void OnArrowCardPressed(ArrowCard pressedCard)
        {
            m_pressedCard = pressedCard;
        }

        private void DefaultProcessPlayedSlot()
        {
            if ((m_playedSlot != null) && !m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y) && m_currentCard != null)
            {
                var logicTile = m_boardLogic.AddTile(m_playedSlot.X, m_playedSlot.Y, m_currentCard.MultiFlags);
                logicTile.PhysicalCard = m_currentCard;
                logicTile.IsPlaced = false;
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
                    entry.Value.PhysicalCard.SwitchArrow(direction, false);
                    if (entry.Value.m_listLinkedTile.Count == 1)
                    {
                        entry.Value.PhysicalCard.SwitchCenter(false);
                    }
                }
                m_boardLogic.RemoveTile(m_playedSlot.X, m_playedSlot.Y);
                Destroy(tile.PhysicalCard.gameObject);
                
                SetState(DefaultState);
            }
            m_playedSlot = null;
            m_crunchPoints = 0;
            m_crunchDots.StopAllDots();

        }

        public const float c_flashDelay = .15f;

        void CardTweenToSlotEnd(LogicTile tile)
        {
            m_nbCardOnTheWay -= 1;
            tile.IsPlaced = true;

            var card = tile.PhysicalCard;

            var pos = card.transform.position;
            pos.z = ArrowCard.c_firstLevel;
            card.transform.position = pos;

            HashSet<LogicTile> chain = new HashSet<LogicTile>();
            List<LogicTile> chainAsList = new List<LogicTile>();
            List<float> depthList = new List<float>();
            List<LogicLinkStandalone> freeLinks = new List<LogicLinkStandalone>();
            m_boardLogic.ComputeTileNeighbor(tile);
            tile.ComputeLinkedChain(ref chain, ref chainAsList, ref depthList, ref freeLinks);
            int chainCount = chain.Count;

            var newLinkDirection = tile.m_linkedTile.Keys;

            if (chainCount > 1)
            {
                for (int i = 0; i < chainCount; i++)
                {
                    StartCoroutine(FlashWithDelay(i * c_flashDelay, chainAsList[i].PhysicalCard));
                }


                int pointToBank =Mathf.Clamp(chainCount, 0, m_bankPointTarget - m_bankPoints);

                int delay = 0;
                for (; delay < pointToBank; ++delay)
                {
                    int dotIndex = m_bankPoints + delay;
                    Transform dot = m_bankDots.GetDot(dotIndex);
                    var logicTile = chainAsList[delay];

                    StartCoroutine(AnimatedLineWithDelay(delay * c_flashDelay + .25f, logicTile.PhysicalCard.transform.position, dot.position,
                        () =>
                        {
                            m_bankDots.LightDot(dotIndex);
                        }
                        ));
                }
                m_bankPoints += pointToBank;

                int pointToCrunch = chainCount - pointToBank;

                pointToCrunch = Mathf.Min(pointToCrunch, c_crunchTarget - m_crunchPoints);

                for (delay = 0; delay < pointToCrunch; ++delay)
                {
                    int dotIndex = m_crunchPoints + delay;
                    Transform dot = m_crunchDots.GetDot(dotIndex);
                    var logicTile = chainAsList[delay];

                    StartCoroutine(AnimatedLineWithDelay(delay * c_flashDelay + .25f, logicTile.PhysicalCard.transform.position, dot.position,
                        () =>
                        {
                            m_crunchDots.LightDot(dotIndex);
                        }
                        ));
                }
                m_crunchPoints += pointToCrunch;
            }


            card.m_tileLinks = new List<TileLink>(tile.m_listLinkedTile.Count);
            var p1 = tile.PhysicalCard.transform.position;

            if (tile.m_listLinkedTile.Count > 0)
            {

                tile.PhysicalCard.LigthArrows(tile.m_linkedTile.Keys);
                tile.PhysicalCard.SwitchCenter(true);

                foreach (var entry in tile.m_linkedTile)
                {
                    var neighbor = entry.Value;
                    var dir = entry.Key.Reverse();
                    neighbor.PhysicalCard.SwitchArrow(dir, true);
                    neighbor.PhysicalCard.SwitchCenter(true);
                }

            }

            CheckEndGame();

        }

        private IEnumerator FlashWithDelay(float delay, ArrowCard card)
        {
            yield return new WaitForSeconds(delay);
            if (card != null)
                card.FlashIntoSuperMode();
        }

        private IEnumerator AnimatedLineWithDelay(float delay, Vector3 source, Vector3 target, Action endAction)
        {
            yield return new WaitForSeconds(delay);

            GameObject lineObject;
            RoundedLineAnimation lineAnimation;
            AnimatedLinePool.GetInstance(out lineObject, out lineAnimation);
            float decalRandom = UnityEngine.Random.Range(-1f, 1f);
            lineAnimation.SetUpLine(source, target, decalRandom, () =>
            {
                AnimatedLinePool.FreeInstance(lineObject);
                endAction();
            });
            lineAnimation.StartAnimation();
        }

        private void ProcessPressedCard()
        {
            if (m_pressedCard != null)
            {
                if (m_pressedCard == m_nextCard)
                {
                    m_nextCard.MoveToPosition(m_playingCardTransform.position);
                    m_currentCard.MoveToPosition(m_nextPlayingCardTransform.position);
                    m_nextCard.m_tweens.ActivationUnveil.StartTween();
                    m_currentCard.m_tweens.ActivationVeil.StartTween();
                    var temp = m_nextCard;
                    m_nextCard = m_currentCard;
                    m_currentCard = temp;
                }

                m_pressedCard = null;
            }
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

            List<LogicLinkStandalone> trashList = new List<LogicLinkStandalone>();
            var allChains = m_boardLogic.GetAllChains(ref trashList);

            int allChainCount = allChains.Count;

            float lineGap = 0f;

            int tileLinked = 0;

            for (int chIndex = 0; chIndex < allChainCount; ++ chIndex)
            {
                var chain = allChains[chIndex];
                int chainLength = chain.Count;

                if (chainLength > 1)
                {
                    for (int tileIndex = 0; tileIndex < chainLength; ++tileIndex)
                    {
                        var tile = chain[tileIndex];

                        m_boardLogic.RemoveTile(tile.X, tile.Y);
                        var card = tile.PhysicalCard;
                        foreach (var link in card.m_tileLinks)
                        {
                            Destroy(link.gameObject);
                        }
                        GameObject lineObject;
                        RoundedLineAnimation lineAnimation;
                        m_animatedLinePool.GetInstance(out lineObject, out lineAnimation);
                        float decalRandom = UnityEngine.Random.Range(-1f, 1f);
                        lineAnimation.SetUpLine(card.transform, m_scoreTransform, decalRandom, () => { m_animatedLinePool.FreeInstance(lineObject); });
                        StartCoroutine(lineAnimation.StartAnimationDelayed(tileIndex * lineGap));

                        card.SoftDestroy();

                        tileLinked++;
                    }
                }
            }

            int comboPoints = ComputeComboPoint(tileLinked);


            m_currentScore += comboPoints;
            m_currentTileScore += tileLinked;

            m_guiManager.NotifyScoreChanged(m_currentScore, comboPoints);
            
            if (tileLinked >= c_minComboToGainTiles)
            {
                NbAvailableTile += tileLinked + 1;
            }

            if (m_currentCard == null)
            {
                DrawNextCard();
            }

            m_bankDots.StopAllDots();
            m_bankPoints = 0;
            m_bankPointTarget += 1;
            m_bankPointTarget = Mathf.Min(m_bankPointTarget, c_maxLinkPoints);
            m_bankDots.SetNumberOfDots(m_bankPointTarget);

            CheckEndGame();

        }

        const int c_baseComboPoints = 10;
        const int c_comboCurveStart = 5;

        int ComputeComboPoint(int nbTile)
        {
            int points = 0;

            int count = nbTile;

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
            if (m_bankPoints >= m_bankPointTarget)
                return;
            if (m_nbCardOnTheWay > 0)
                return;

            if (m_boardLogic.IsBoardFull())
            {
                if (m_crunchPoints < c_crunchTarget)
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
            if (m_bankPoints >= m_bankPointTarget)
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
            if (m_crunchPoints < c_crunchTarget)
                return false;
            if (m_boardLogic.IsBoardEmpty())
                return false;
            return true;
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
                tile.PhysicalCard.IsCrunchableAnimation = true;
            }
        }

        private void _TileCrunchStateEnd()
        {

            foreach (var tile in m_boardLogic.AllTilePlaced)
            {
                tile.PhysicalCard.IsCrunchableAnimation = false;
            }
        }

    }
    #endregion
}

