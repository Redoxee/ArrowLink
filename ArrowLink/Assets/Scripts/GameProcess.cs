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
        Transform m_popingCardTransform = null;
        [SerializeField]
        Transform m_playingCardTransform = null;
        [SerializeField]
        Transform m_nextPlayingCardTransform = null;

        [SerializeField]
        Transform m_holdingCardTransform = null;

        private ArrowCard m_holdedCard = null;

        [SerializeField]
        AnimatedLinePool m_animatedLinePool = null;
        public AnimatedLinePool AnimatedLinePool { get { return m_animatedLinePool; } }
        [SerializeField]
        AnimatedLinePool m_darkAnimatedLinePool = null;

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

        private int m_crunchTarget = 2;
        private int m_crunchPoints = 0;
        private const int c_maxCrunchPoints = 22;

        private int m_bankPointTarget = 2;
        private int m_bankPoints = 0;
        public const int c_maxBankPoints = 33;

        [SerializeField]
        private Transform m_feedbackCapsule = null;

        [SerializeField]
        private DotCollection m_bankDots = null;
        [SerializeField]
        private DotCollection m_crunchDots = null;

        private DelayedActionCollection m_placementTileDelayedActions = new DelayedActionCollection();

        private OverLinkModule m_overLinkModule = new OverLinkModule();
        

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

            m_bankDots.SetNumberOfDots(m_bankPointTarget);
            m_crunchDots.SetNumberOfDots(m_crunchTarget);

            m_guiManager.SetBankable(false);
            m_guiManager.SetCrunchable(false);

            m_guiManager.SetOverLinkCapsuleState(m_overLinkModule);
        }

        private void Update()
        {
            ProcessPressedCard();
            m_currentState.ProcessPlayedSlot();

            m_placementTileDelayedActions.ManualUpdate();
        }

        void DrawNextCard()
        {
            ShootNewCurrentCard();
        }

        private void ShootNewCurrentCard()
        {
            GameObject currentCardObject = Instantiate(m_cardPrefab);
            currentCardObject.SetActive(true);
            m_currentCard = currentCardObject.GetComponent<ArrowCard>();
            m_currentCard.transform.position = m_popingCardTransform.position;
            var unveilTween = m_currentCard.m_tweens.ActivationUnveil;
            m_currentCard.MoveToPosition(m_playingCardTransform.position);
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

                Vector3 targetPos = tile.PhysicalCard.transform.position;

                float delay = m_darkAnimatedLinePool.GetLineAnimationDuration();

                tile.PhysicalCard.SoftDestroy(delay);

                for (int i = 0; i < m_crunchTarget; ++i)
                {
                    GameObject go; RoundedLineAnimation animation;
                    m_darkAnimatedLinePool.GetInstance(out go, out animation);
                    Vector3 start = m_crunchDots.GetDot(i).transform.position;
                    animation.SetUpLine(start, targetPos, 1f, ()=> { m_darkAnimatedLinePool.FreeInstance(go); });
                    animation.StartAnimation();
                }

                SetState(DefaultState);
                m_crunchPoints = 0;
                m_crunchTarget = Mathf.Min(m_crunchTarget + 1, c_maxCrunchPoints);

                m_crunchDots.StopAllDots();
                m_crunchDots.SetNumberOfDots(m_crunchTarget);

                m_guiManager.SetCrunchable(false);

                
            }
            m_playedSlot = null;
        }

        public const float c_flashDelay = .15f;
        public const float c_lineDelay = .25f;

        void CardTweenToSlotEnd(LogicTile tile)
        {
            m_nbCardOnTheWay -= 1;
            tile.IsPlaced = true;

            var placedCard = tile.PhysicalCard;

            var pos = placedCard.transform.position;
            pos.z = ArrowCard.c_firstLevel;
            placedCard.transform.position = pos;

            HashSet<LogicTile> chain = new HashSet<LogicTile>();
            List<LogicTile> chainAsList = new List<LogicTile>();
            List<float> depthList = new List<float>();
            List<LogicLinkStandalone> freeLinks = new List<LogicLinkStandalone>();
            m_boardLogic.ComputeTileNeighbor(tile);
            tile.ComputeLinkedChain(ref chain, ref chainAsList, ref depthList, ref freeLinks);
            int chainCount = chain.Count;

            var newLinkDirection = tile.m_linkedTile.Keys;

            int pointToBank = 0;
            int pointToBonus = 0;
            int pointToCrunch = 0;

            if (chainCount > 1)
            {
                for (int i = 0; i < chainCount; i++)
                {
                    var flashingCard = chainAsList[i].PhysicalCard;
                    FlashWithDelay(i * c_flashDelay, flashingCard);
                }


                pointToBank = Mathf.Clamp(chainCount, 0, m_bankPointTarget - m_bankPoints);
                
                for (int i = 0 ; i < pointToBank; ++i)
                {
                    int delay = i;
                    int dotIndex = m_bankPoints + delay;
                    Transform dot = m_bankDots.GetDot(dotIndex);
                    var logicTile = chainAsList[delay];

                    AnimatedLineWithDelay(delay * c_flashDelay + c_lineDelay, logicTile.PhysicalCard.transform.position, dot.position,
                        () =>
                        {
                            m_bankDots.LightDot(dotIndex);
                        }
                        );
                }
                m_bankPoints += pointToBank;
                if (m_bankPoints >= m_bankPointTarget)
                {
                    m_guiManager.SetBankable(true);
                }

                pointToCrunch = chainCount - pointToBank;

                pointToCrunch = Mathf.Min(pointToCrunch, m_crunchTarget - m_crunchPoints);

                for (int i = 0; i < pointToCrunch; ++i)
                {
                    int delay = i;
                    int dotIndex = m_crunchPoints + delay;
                    Transform dot = m_crunchDots.GetDot(dotIndex);
                    var tIndex = delay + pointToBank;
                    var logicTile = chainAsList[tIndex];

                    AnimatedLineWithDelay(tIndex * c_flashDelay + c_lineDelay, logicTile.PhysicalCard.transform.position, dot.position,
                        () =>
                        {
                            m_crunchDots.LightDot(dotIndex);
                        }
                        );
                }
                m_crunchPoints += pointToCrunch;
                if (m_crunchPoints >= m_crunchTarget)
                {
                    m_guiManager.SetCrunchable(true);
                }


                pointToBonus = chainCount - pointToBank - pointToCrunch;
                if (pointToBonus > 0)
                {
                    for (int i = 0;  i < pointToBonus; ++i)
                    {
                        int delay = i;
                        var target = m_feedbackCapsule;
                        var tIndex = pointToBank + pointToCrunch + delay;
                        var logicTile = chainAsList[tIndex];
                        int baseBonus = m_overLinkModule.OverLinkCounter ;
                        AnimatedLineWithDelay(tIndex * c_flashDelay + c_lineDelay, logicTile.PhysicalCard.transform.position, target.position,
                            () =>
                            {
                                m_guiManager.OverLinkGUICapsule.FlashTween.StartTween();
                                int currentBonus = baseBonus + delay + 1;
                                m_guiManager.OverLinkGUICapsule.DotBonus.text = string.Format("+{0}", m_overLinkModule.GetDotBonusForCounter(currentBonus));
                                m_guiManager.OverLinkGUICapsule.ScoreBonus.text = string.Format("+{0}", m_overLinkModule.GetScoreBonusForCounter(currentBonus));
                            }
                            );
                    }
                    m_overLinkModule.OverLinkCounter += pointToBonus;

                    float overLinkDelay = pointToBonus * c_flashDelay + AnimatedLinePool.GetLineAnimationDuration();
                    m_placementTileDelayedActions.AddAction(overLinkDelay, () =>
                     {
                         m_guiManager.OverLinkGUICapsule.DotBonus.text = string.Format("+{0}", m_overLinkModule.GetDotBonus());
                         m_guiManager.OverLinkGUICapsule.ScoreBonus.text = string.Format("+{0}", m_overLinkModule.GetScoreBonus());
                     });

                }
            }


            placedCard.m_tileLinks = new List<TileLink>(tile.m_listLinkedTile.Count);
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


            m_placementTileDelayedActions.AddAction((pointToBank + pointToCrunch + pointToBonus) * c_flashDelay + c_lineDelay + AnimatedLinePool.GetLineAnimationDuration() + .25f, CheckEndGame);
            //CheckEndGame();

        }

        private void FlashWithDelay(float delay, ArrowCard card)
        {

            m_placementTileDelayedActions.AddAction(delay,
                () =>
                {

                    card.FlashIntoSuperMode();
                }
                );
        }

        private void AnimatedLineWithDelay(float delay, Vector3 source, Vector3 target, Action endAction)
        {
            m_placementTileDelayedActions.AddAction(delay,
                () => {
                    GameObject lineObject;
                    RoundedLineAnimation lineAnimation;
                    AnimatedLinePool.GetInstance(out lineObject, out lineAnimation);
                    float decalRandom = UnityEngine.Random.Range(-1f, 1f);
                    float lineTime = lineAnimation.AnimationDuration;
                    
                    lineAnimation.SetUpLine(source, target, decalRandom,()=> {
                        AnimatedLinePool.FreeInstance(lineObject);
                    });

                    lineAnimation.StartAnimation();
                });
            float lineDuration = AnimatedLinePool.GetLineAnimationDuration();
            m_placementTileDelayedActions.AddAction(delay + lineDuration,endAction);
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

            m_placementTileDelayedActions.Clear();

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

                        m_placementTileDelayedActions.AddAction(tileIndex * lineGap, () =>
                        {
                            lineAnimation.StartAnimation();
                        });
                        
                        card.SoftDestroy();

                        tileLinked++;
                    }
                }
            }

            int comboPoints = ComputeComboPoint(tileLinked);
            
            m_currentScore += comboPoints + m_overLinkModule.GetScoreBonus();
            m_currentTileScore += tileLinked;

            m_guiManager.NotifyScoreChanged(m_currentScore, comboPoints);

            if (m_currentCard == null)
            {
                DrawNextCard();
            }

            m_bankDots.StopAllDots();
            m_bankPoints = 0;
            m_bankPointTarget += m_overLinkModule.GetDotBonus();
            m_bankPointTarget = Mathf.Min(m_bankPointTarget, c_maxBankPoints);
            m_bankDots.SetNumberOfDots(m_bankPointTarget);
            m_guiManager.SetBankable(false);

            bool flashOverLink = m_overLinkModule.OverLinkCounter > 0;
            m_overLinkModule.OverLinkCounter = 0;
            m_guiManager.SetOverLinkCapsuleState(m_overLinkModule,flashOverLink);

            m_flagDistributor.NotifyBonusRequested();

            CheckEndGame();

        }

        const int c_baseComboPoints = 10;
        const int c_comboCurveStart = 5;

        int ComputeComboPoint(int nbTile)
        {
            return nbTile * c_baseComboPoints;
        }



        private void CheckEndGame()
        {
            if (m_bankPoints >= m_bankPointTarget)
                return;
            if (m_nbCardOnTheWay > 0)
                return;

            if (m_boardLogic.IsBoardFull())
            {
                if (m_crunchPoints < m_crunchTarget)
                {
                    m_guiManager.NotifyEndGame();
                }
                return;
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
            if (m_crunchPoints < m_crunchTarget)
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
                tile.PhysicalCard.IsWigglingAnimation = true;
            }
        }

        private void _TileCrunchStateEnd()
        {

            foreach (var tile in m_boardLogic.AllTilePlaced)
            {
                tile.PhysicalCard.IsWigglingAnimation = false;
            }
        }

        public void OnHoldButtonPressed()
        {
            if (m_holdedCard != null)
            {
                m_holdedCard.MoveToPosition(m_playingCardTransform.position);
                m_holdedCard.m_tweens.ActivationUnveil.StartTween();
            }
            m_currentCard.MoveToPosition(m_holdingCardTransform.position);
            m_currentCard.m_tweens.ActivationVeil.StartTween();

            var temp = m_holdedCard;
            m_holdedCard = m_currentCard;
            if (temp != null)
                m_currentCard = temp;
            else
                DrawNextCard();
        }

        public void OnNextButtonPressed()
        {
            for (int x = 0; x < BoardLogic.c_col; ++x)
            {
                for (int y = 0; y < BoardLogic.c_row; ++y)
                {
                    if (!m_boardLogic.IsFilled(x, y))
                    {
                        m_board.GetSlot(x, y).Flash();
                    }
                }
            }
        }

        public void OnDispenserPressed()
        {
            if (!m_currentCard.IsWigglingAnimation)
            {
                m_currentCard.IsWigglingAnimation = true;
                m_currentCard.IsWigglingAnimation = false;
            }
        }
    }
    #endregion

    #region Delayed Action

    public class DelayedActionCollection
    {
        public class TimedAction
        {
            public float TimeOfActivation = 0f;
            public Action Action;

            public static int Comparer(TimedAction a, TimedAction b)
            {
                return (a.TimeOfActivation == b.TimeOfActivation) ? 0 : (a.TimeOfActivation < b.TimeOfActivation) ? -1: 1;
            }
        }
        List<TimedAction> m_actionList = new List<TimedAction>(100);

        float m_timer = 0f;

        

        public void AddAction(float delay, Action action)
        {
            var newinstance = new TimedAction();
            newinstance.TimeOfActivation = delay + m_timer;
            newinstance.Action = action;
            m_actionList.Add(newinstance);
            m_actionList.Sort(TimedAction.Comparer);
        }

        public void Clear()
        {
            m_actionList.Clear();
            m_timer = 0f;
        }

        public void ManualUpdate()
        {
            if (m_actionList.Count == 0)
                return;
            m_timer += Time.deltaTime;
            if (m_timer < m_actionList[0].TimeOfActivation)
                return;
            int index = 0;
            for (; index < m_actionList.Count; index++)
            {
                if (m_actionList[index].TimeOfActivation > m_timer)
                    break;
                m_actionList[index].Action();
            }
            m_actionList.RemoveRange(0, index);

        }
    }

    #endregion
}

