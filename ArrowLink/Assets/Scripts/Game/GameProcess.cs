using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AntonMakesGames;



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
        BoardInput m_board = null;

        [SerializeField]
        GameCamera m_gameCamera = null;
        Camera m_camera = null;
        public Camera GameCamera { get { return m_camera; } }

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
        Transform m_holdingCardTransform = null;

        [SerializeField]
        bool m_previewNextTile = false;

        private ArrowCard m_nextPlayedCard = null;
        private ArrowCard m_holdedCard = null;
        private ArrowCard m_currentCard = null;
        public bool IsHoldingCard { get { return m_holdedCard != null; } }

        [SerializeField]
        AnimatedLinePool m_animatedLinePool = null;
        public AnimatedLinePool AnimatedLinePool { get { return m_animatedLinePool; } }
        [SerializeField]
        AnimatedLinePool m_darkAnimatedLinePool = null;


        BoardSlot m_playedSlot;

        private int m_currentScore = 0;
        public int CurrentScore { get { return m_currentScore; } }

        private int m_currentTileScore = 0;
        public int CurrentTileScore { get { return m_currentTileScore; } }

        int m_nbCardOnTheWay = 0;

        public const int c_crunchCooldown = 1;
        private int m_crunchCoolDown = 0;
        public bool CanCrunch { get { return (m_crunchCoolDown < 1) && !m_boardLogic.IsBoardEmpty(); } }

        private int m_bankPointTarget = 3;
        private int m_bankPoints = 0;
        public const int c_maxBankPoints = 33;
        
        [SerializeField]
        private DotCollection m_bankDots = null;
        [SerializeField]
        private DotCollection m_overlinkDotCollection = null;

        private DelayedActionCollection m_bankDelayedAction = new DelayedActionCollection();
        private DelayedActionCollection m_crunchDelayedAction = new DelayedActionCollection();
        private DelayedActionCollection m_overLinkDelayedAction = new DelayedActionCollection();
        
        private bool m_isGameEnded = false;
        public bool IsGameEnded { get { return m_isGameEnded; } }
        private float m_gameStartTime = float.MaxValue;

        List<Action> m_onTilePlayedListeners = null;

        private const int c_dotBonusTarget = 5;
        private int m_overLinkCurrent = c_dotBonusTarget - 2;
        private int BonusLevel { get { return (m_overLinkCurrent + 1) / c_dotBonusTarget; } }
        private int DotBonusLocalIndex { get { return m_overLinkCurrent % c_dotBonusTarget; } }
        private bool IsSpecialDotBonus { get { return DotBonusLocalIndex == c_dotBonusTarget - 1; } }

        private const int c_baseComboPoints = 10;

        private const float c_multiplierPerBonus = 1.0f;

        private int m_nbCombo = 0;
        private int m_nbCrunch = 0;

        public bool IsGamePaused = false;

        private ArrowFlag m_firstFlag = (ArrowFlag)255;

        #region Reusable Collection

        private HashSet<LogicTile> m_reusableTileSet = new HashSet<LogicTile>();
        private List<LogicTile> m_resusableTileChain = new List<LogicTile>();
        private List<float> m_reusableDepthList = new List<float>();
        private List<LogicLinkStandalone> m_reusableFreeLinks = new List<LogicLinkStandalone>();

        #endregion

        [NonSerialized]
        private GameSaver m_gameSaver;

        [SerializeField]
        private GridBlinker m_gridBlinker = null;

        [SerializeField]
        private HightLightCircle m_boardHighlight = null;

        [SerializeField]
        private HightLightCircle m_bankHiglight = null;

        [SerializeField]
        private HightLightCircle m_crunchHighLight = null;

        private void Awake()
        {
            if (s_instance != null)
            {
                Debug.LogError("Several instance of game process created !");
                Destroy(this);
                return;
            }
            s_instance = this;

            m_onTilePlayedListeners = new List<Action>();

            m_gameSaver = new GameSaver();
        }

        private void Start()
        {
            m_boardLogic = new BoardLogic();
            InitializeStates();

            m_gameCamera.Initialize();
            m_camera = m_gameCamera.GetComponent<Camera>();
            m_board.Initialize(this);

            var pos = m_playingCardTransform.position;
            pos.z = ArrowCard.c_secondLevel; ;
            m_playingCardTransform.position = pos;

            pos = m_popingCardTransform.position;
            pos.z = ArrowCard.c_firstLevel;
            m_popingCardTransform.position = pos;

            m_overlinkDotCollection.SetNumberOfDots(c_dotBonusTarget - 1);

            bool hasGameSaved = m_gameSaver.Load();
            if(!hasGameSaved)
            {
                InitGame();
            }
            else
            {
                LoadFromSave(); 
            }
            

            if (MainProcess.IsReady)
            {
                Dictionary<string, object> startParams = new Dictionary<string, object>();
                startParams["date"] = DateTime.UtcNow.ToString();
                TrackingManager.TrackEvent("Game Start", 1, startParams);
            }
            m_gameStartTime = Time.time;
        }

        private void InitGame()
        {
            if (m_previewNextTile)
            {
                ShootNewNextCard(m_firstFlag);
            }
            DrawNextCards();

            m_currentScore = 0;
            m_nbCardOnTheWay = 0;

            m_crunchCoolDown = c_crunchCooldown;
            m_nbCombo = 0;
            m_nbCrunch = 0;

            m_bankDots.SetNumberOfDots(m_bankPointTarget);

            m_guiManager.SetBankable(false);
            m_guiManager.SetCrunchable(false);

            m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(0), ComputeBankBonus(0));
            m_gridBlinker.StartAnimation(1);
            m_boardHighlight.Show();
        }

        private void Update()
        {
            if (!IsGamePaused)
            {
                m_currentState.ProcessPlayedSlot();
                m_bankDelayedAction.ManualUpdate();
                m_crunchDelayedAction.ManualUpdate();
                m_overLinkDelayedAction.ManualUpdate();
            }
        }

        void DrawNextCards()
        {
            if (!m_previewNextTile)
            {
                ShootNewNextCard();
            }
            UseNextCardAsCurrent();
            if (m_previewNextTile)
            {
                ShootNewNextCard();
            }
        }

        private void UseNextCardAsCurrent()
        {
            m_currentCard = m_nextPlayedCard;
            m_currentCard.IsCurrentCard = true;
            var unveilTween = m_currentCard.m_tweens.ActivationUnveil;
            m_currentCard.MoveToPosition(m_playingCardTransform.position);
            unveilTween.StartTween();
        }

        private void ShootNewNextCard(ArrowFlag forcedFlags = ArrowFlag.NONE)
        {
            GameObject nextCardObject = Instantiate(m_cardPrefab);
            nextCardObject.transform.SetParent(transform, false);
            nextCardObject.SetActive(true);
            m_nextPlayedCard = nextCardObject.GetComponent<ArrowCard>();
            m_nextPlayedCard.transform.position = m_popingCardTransform.position;
#if AMG_PREDETERMINED_TILES
            forcedFlags = ArrowFlag.NONE;
#endif
            if (forcedFlags == ArrowFlag.NONE)
            {
                forcedFlags = m_flagDistributor.PickRandomFlags();
            }

            m_nextPlayedCard.SetupArrows(forcedFlags);
            m_nextPlayedCard.IsCurrentCard = false;
        }

        public void OnTilePressed(BoardSlot slot)
        {
            if (IsGamePaused)
                return;
            m_playedSlot = slot;
        }

        private void DefaultProcessPlayedSlot()
        {
            if ((m_playedSlot != null) && !m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y) && m_currentCard != null)
            {

                var logicTile = m_boardLogic.AddTile(m_playedSlot.X, m_playedSlot.Y, m_currentCard.MultiFlags);

                logicTile.PhysicalCard = m_currentCard;
                logicTile.PlayedFrame = (uint)Time.frameCount;

                m_boardLogic.ComputeTileNeighbor(logicTile);

                List<LogicTile> tileChain = new List<LogicTile>();
                m_reusableFreeLinks.Clear();
                logicTile.ComputeLinkedChain(ref m_reusableTileSet, ref tileChain, ref m_reusableDepthList, ref m_reusableFreeLinks);
                int pointToBank, pointToOverLink;
                int baseBank = m_bankPoints;
                int baseOverLink = m_overLinkCurrent;
                RewardTilesLogic(tileChain, out pointToBank, out pointToOverLink);

                Action cardPlayedAction = () => {
                    CardTweenToSlotEnd(logicTile, tileChain, baseBank, pointToBank, baseOverLink, pointToOverLink);
                }; // garbage here
                m_currentCard.GoToSlot(m_playedSlot, cardPlayedAction);
                m_nbCardOnTheWay += 1;
                m_currentCard.IsCurrentCard = false;

                m_currentCard = null;
                DrawNextCards();

                foreach (var act in m_onTilePlayedListeners)
                {
                    act();
                }
                AntonMakesGames.AchievementManager.NotifyEventIncrement("TotalTilePlaced");

                Save();
                m_boardHighlight.CancelHighLight();
            }
            m_playedSlot = null;
        }

        private void ExitDefaultState()
        {
            if (m_currentCard != null && m_currentCard.IsDragging)
            {
                m_currentCard.RequestCancelDrag();
                m_currentCard.MoveToPosition(m_playingCardTransform.position);
            }
        }
        
        private void TileCrunchProcessSlot()
        {
            if (m_playedSlot == null)
                return;
            if (m_boardLogic.IsFilled(m_playedSlot.X, m_playedSlot.Y))
            {
                LogicTile tile = m_boardLogic.GetTile(m_playedSlot.X, m_playedSlot.Y);
                
                m_boardLogic.ComputeTileNeighbor(tile);

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

                m_crunchDelayedAction.Clear();

                Vector3 targetPos = tile.PhysicalCard.transform.position;

                tile.PhysicalCard.SoftDestroy();

                SetState(DefaultState);

                m_crunchCoolDown = c_crunchCooldown;

                m_guiManager.SetCrunchable(false);

                ++m_nbCrunch;
                Save();
            }
            m_playedSlot = null;
        }

        private void CrunchCurrentTile()
        {
            if (m_currentState != TileCrunchState)
                return;

            m_currentCard.SoftDestroy();

            m_currentCard = null;
            DrawNextCards();

            SetState(DefaultState);
            m_crunchCoolDown = c_crunchCooldown;
            m_guiManager.SetCrunchable(false);
            ++m_nbCrunch;
            Save();
        }

        public const float c_flashDelay = .15f;
        public const float c_lineDelay = .25f;

        void CardTweenToSlotEnd(LogicTile tile, List<LogicTile> tileChain, int baseBank, int pointToBank, int baseOverLink, int pointToOverLink)
        {
            m_nbCardOnTheWay -= 1;

            var placedCard = tile.PhysicalCard;

            var pos = placedCard.transform.position;
            pos.z = ArrowCard.c_firstLevel;
            placedCard.transform.position = pos;

            var newLinkDirection = tile.m_linkedTile.Keys;

            RewardTileVisually(tileChain, baseBank, pointToBank, baseOverLink, pointToOverLink);


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
            
            AchievementManager.NotifyEventMaxing("BestTileChain", tileChain.Count);
            if (m_boardLogic.IsBoardFull())
            {
                AchievementManager.NotifyEventIncrement("BoardFilled");
                if (CanBank)
                {
                    if (m_nbCombo == 0)
                    {
                        m_bankHiglight.Show();
                    }
                } else if (CanCrunch)
                {
                    m_guiManager.SetFocusCrunch(true);
                }
            }
        }

        private void GetRewardForChain(List<LogicTile> chainAsList, out int pointToBank, out int pointToOverLink)
        {
            int chainCount = chainAsList.Count;
            if (chainCount > 1)
            {
                pointToBank = Mathf.Clamp(chainCount, 0, m_bankPointTarget - m_bankPoints);
                pointToOverLink = chainCount - pointToBank;
            }
            else
            {
                pointToBank = pointToOverLink = 0;
            }
        }

        private void RewardTilesLogic(List<LogicTile> chainAsList, out int pointToBank, out int pointToOverLink)
        {
            GetRewardForChain(chainAsList, out pointToBank, out pointToOverLink);

            m_bankPoints += pointToBank;
            m_overLinkCurrent += pointToOverLink;
        }


        private void RewardTileVisually(List<LogicTile> chainAsList, int baseBank, int pointToBank, int baseOverLink ,int pointToOverLink)
        {
            float animationDelay = 0f;

            int chainCount = chainAsList.Count;
            if (chainCount > 1)
            {
                for (int i = 0; i < chainCount; i++)
                {
                    var tile = chainAsList[i];
                    var flashingCard = tile.PhysicalCard;
                    FlashWithDelay(i * c_flashDelay, flashingCard);
                }

                for (int i = 0; i < pointToBank; ++i)
                {

                    int dotIndex = baseBank + i;
                    Transform dot = m_bankDots.GetDot(dotIndex);
                    var logicTile = chainAsList[i];
                    animationDelay += c_flashDelay;
                    Action lightDotAction = () =>
                    {
                        m_bankDots.LightDot(dotIndex);
                    };
                    AnimatedLineWithDelay(animationDelay + c_lineDelay, logicTile.PhysicalCard.transform.position, dot.position, lightDotAction
                        , m_bankDelayedAction
                        );
                }
                
                if (baseBank + pointToBank>= m_bankPointTarget)
                {
                    m_guiManager.SetBankable(true);
                }
                

                if (pointToOverLink > 0)
                {
                    m_guiManager.SetCapsuleBonusEnabled(true);

                    for (int i = 0; i < pointToOverLink; ++i)
                    {
                        int overLink = baseOverLink + i + 1;
                        int tileIndex = pointToBank + i;
                        var logicTilePosition = chainAsList[tileIndex].PhysicalCard.transform.position;

                        int bonusIndex = (overLink % c_dotBonusTarget);
                        bool isBonus = bonusIndex == c_dotBonusTarget - 1;
                        if (!isBonus)
                        {
                            Transform dot = m_overlinkDotCollection.GetDot(bonusIndex);
                            Action lightDots = () =>
                            {
                                m_overlinkDotCollection.LightDot(bonusIndex);
                            };
                            AnimatedLineWithDelay(animationDelay + c_lineDelay, logicTilePosition, dot.position, lightDots, m_overLinkDelayedAction);
                        }
                        else
                        {
                            int levelCopy = BonusLevel;
                            Action incrementBonus = () =>
                            {
                                m_overlinkDotCollection.StopAllDots();
                                m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(levelCopy), ComputeBankBonus(levelCopy));
                            };
                            AnimatedLineWithDelay(animationDelay + c_lineDelay, logicTilePosition,m_guiManager.BonusCapsuleTransform.position, incrementBonus, m_overLinkDelayedAction);
                        }

                        animationDelay += c_flashDelay;
                    }
                }
            }

            if (animationDelay > 0)
            {
                animationDelay += c_lineDelay + AnimatedLinePool.GetLineAnimationDuration();
            }
            animationDelay += .25f;
            m_bankDelayedAction.AddAction(animationDelay, CheckEndGame);
        }

        private void FlashWithDelay(float delay, ArrowCard card)
        {

            m_bankDelayedAction.AddAction(delay,
                () =>
                {

                    card.FlashIntoSuperMode();
                }
                );
        }

        private void AnimatedLineWithDelay(float delay, Vector3 source, Vector3 target, Action endAction, DelayedActionCollection delayManager)
        {
            delayManager.AddAction(delay,
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
            delayManager.AddAction(delay + lineDuration,endAction);
        }
        
        void EndCombo()
        {

            float animatedLineDuration = m_animatedLinePool.GetLineAnimationDuration();

            int tileLinked = 0;

            int nextBankTarget = m_bankPointTarget + ComputeBankBonus(BonusLevel);
            nextBankTarget = Mathf.Min(nextBankTarget, c_maxBankPoints);

            float pointsAnimationDelay = m_bankPoints > 1 ? c_bankPointAnimationShotTime / (m_bankPoints - 1) : 0;
            pointsAnimationDelay = Mathf.Min(pointsAnimationDelay, c_bankPointMaxAnimationDelay);

            m_bankDelayedAction.Clear();
            m_overLinkDelayedAction.Clear();

            m_overlinkDotCollection.StopAllDots();

            AnimationDestroyCards(out tileLinked);
            BankToScoreLineAnimation(pointsAnimationDelay);
            float bonusDelay = pointsAnimationDelay * (m_bankPoints - 1);
            float multiplierDelay = BankApplyBonusesAnimations(bonusDelay + animatedLineDuration);
            
            int basePoints = ComputebasePoints(m_bankPoints);
            float multiplier = ComputeMultiplierBonus(BonusLevel);

            int gainedPoints = Mathf.FloorToInt(basePoints * multiplier);
            m_currentScore += gainedPoints;
            m_currentTileScore += tileLinked;
            m_nbCombo++;

            m_guiManager.NotifyDeltaScoreChanged(basePoints, animatedLineDuration);

            int delta = Mathf.FloorToInt(basePoints * multiplier);
            if (multiplier > 1f)
            {
                m_bankDelayedAction.AddAction(multiplierDelay + 1.5f, () => { m_guiManager.ApplyMultiplier(delta); });
                m_bankDelayedAction.AddAction(multiplierDelay + 3.5f, () => { m_guiManager.ApplyScoreDelta(m_currentScore, delta); });
            }
            else
            {
                m_bankDelayedAction.AddAction(multiplierDelay + 1.5f, () => { m_guiManager.ApplyScoreDelta(m_currentScore, delta); });
            }

            if (m_currentCard == null)
            {
                DrawNextCards();
            }

            m_bankDots.StopAllDots();
            m_bankPoints = 0;
            m_bankPointTarget = nextBankTarget;
            m_guiManager.SetBankable(false);

            float displayDelay = (BonusLevel > 0) ? multiplierDelay - animatedLineDuration : 0;

            m_bankDelayedAction.AddAction(displayDelay, () => {
                m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(0), ComputeBankBonus(0),true);
                m_guiManager.SetCapsuleBonusEnabled(false);
            });

            m_overLinkCurrent = c_dotBonusTarget - 2;
            --m_crunchCoolDown;
            if (m_crunchCoolDown < 1)
            {
                m_guiManager.SetCrunchable(true);
            }

            m_bankHiglight.CancelHighLight();

            m_flagDistributor.NotifyBank();
            AchievementManager.NotifyEventIncrement("TotalNbBank");
            AchievementManager.NotifyEventMaxing("OneGamebank", m_nbCombo);
            if (m_boardLogic.IsBoardEmpty())
            {
                AchievementManager.NotifyEventIncrement("BoardCleared");
            }
            if (tileLinked == 16)
            {
                AchievementManager.NotifyEventIncrement("FullBoardBank");
            }
            CheckEndGame();
            Save();
        }

        private List<LogicLinkStandalone> m_trashList = new List<LogicLinkStandalone>(16 * 8);

        private float AnimationDestroyCards(out int tileLinked)
        {
            
            var allChains = m_boardLogic.GetAllChains(ref m_trashList);
            int allChainCount = allChains.Count;

            Vector3 targetPos = m_guiManager.DeltaTransform.position;

            tileLinked = 0;

            float animationDelay = 0f;

            for (int chIndex = 0; chIndex < allChainCount; ++chIndex)
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
                        card.SoftDestroy();

                        tileLinked++;
                    }
                }
            }

            return animationDelay;
        }

        public const float c_bankPointAnimationShotTime = 1f;
        public const float c_bankPointMaxAnimationDelay = .15f;

        private void BankToScoreLineAnimation(float animationSteps)
        {

            Vector3 targetPos = m_guiManager.DeltaTransform.position;

            float animationDelay = 0f;
            

            for (int i = 0; i < m_bankPoints; ++i)
            {
                var dot = m_bankDots.GetDot(i);

                GameObject lineObject;
                RoundedLineAnimation lineAnimation;

                m_animatedLinePool.GetInstance(out lineObject, out lineAnimation);
                float decalRandom = UnityEngine.Random.Range(-1f, 1f);
                lineAnimation.SetUpLine(dot.transform.position, targetPos, decalRandom, () => { m_animatedLinePool.FreeInstance(lineObject); });

                m_bankDelayedAction.AddAction(animationDelay, () =>
                {
                    lineAnimation.StartAnimation();
                });
                animationDelay += animationSteps;
            }

        }

        public void OnArrowCardDragReleased(ArrowCard card)
        {
            Vector3 pos = card.transform.position;
            var slot = m_board.GetSlotFromWorldPosition(pos);
            if (slot != null && !m_boardLogic.IsFilled(slot.X,slot.Y) && m_currentState == DefaultState)
            {
                m_playedSlot = slot;
            }
            else
            {
                m_currentCard.MoveToPosition(m_playingCardTransform.position);
            }
        }

        private float BankApplyBonusesAnimations(float animationDelay)
        {

            Vector3 multiplierPos = m_guiManager.MultiplierTransform.position;

            float lineAnimDuration = m_animatedLinePool.GetLineAnimationDuration();
            
            Vector3 startPosition = m_guiManager.BonusCapsuleTransform.position;

            float multiplier = ComputeMultiplierBonus(BonusLevel);

            for (int i = 0; i < BonusLevel; ++i)
            {
                animationDelay += .05f;
                Action freeLine;
                
                Vector3 target = multiplierPos;
                GameObject go;
                RoundedLineAnimation lineAnimation;
                float factor = UnityEngine.Random.Range(-1f, 1f);

                m_animatedLinePool.GetInstance(out go, out lineAnimation);


                freeLine = () => { m_animatedLinePool.FreeInstance(go); };
                m_bankDelayedAction.AddAction(animationDelay + lineAnimDuration, () =>
                    {
                        m_guiManager.SetScoreMultiplier(multiplier);
                    });

                lineAnimation.SetUpLine(startPosition, target, factor, freeLine);
                m_bankDelayedAction.AddAction(animationDelay, () => { lineAnimation.StartAnimation(); });
            }
           
            int bankBonus = ComputeBankBonus(BonusLevel);
            m_bankDelayedAction.AddAction(animationDelay, () =>
            {
                m_bankDots.SetNumberOfDots(m_bankPointTarget);
            });

            animationDelay += .25f;

            for (int i = 0; i < bankBonus; ++i)
            {
                animationDelay += .05f;
                Action freeLine;
                
                GameObject go;
                RoundedLineAnimation lineAnimation;
                float factor = UnityEngine.Random.Range(-1f, 1f);
                int index = m_bankPointTarget + i;
                m_darkAnimatedLinePool.GetInstance(out go, out lineAnimation);

                freeLine = () => { m_darkAnimatedLinePool.FreeInstance(go); };
                
                m_bankDelayedAction.AddAction(animationDelay, () => {
                    Vector3 target = m_bankDots.GetDot(index).position;
                    lineAnimation.SetUpLine(startPosition, target, factor, freeLine);
                    lineAnimation.StartAnimation();
                });
            }
            animationDelay += lineAnimDuration;


            return animationDelay;
        }

        IEnumerator _animatedLineFrameDelayed(Vector3 start, Transform target, float animationDelay)
        {
            yield return null;
            GameObject go; RoundedLineAnimation lineAnimation;
            m_darkAnimatedLinePool.GetInstance(out go, out lineAnimation);
            Action freeLine = () => { m_darkAnimatedLinePool.FreeInstance(go); };
            float factor = UnityEngine.Random.Range(-1f, 1f);
            lineAnimation.SetUpLine(start, target.position, factor, freeLine);
            m_bankDelayedAction.AddAction(animationDelay, () => { lineAnimation.StartAnimation(); });
        }
        
        int ComputebasePoints(int nbTile)
        {
            return nbTile * c_baseComboPoints;
        }
        
        float ComputeMultiplierBonus(int bonusLevel)
        {
            float fullBonus = m_bankPointTarget >= c_maxBankPoints ? 1f : 0;
            return 1f + bonusLevel * c_multiplierPerBonus + fullBonus;
        }

        private int ComputeBankBonus(int bonusLevel)
        {
            int projected = m_bankPointTarget + bonusLevel;
            return Mathf.Min(projected,c_maxBankPoints) - m_bankPointTarget;
        }

        private void CheckEndGame()
        {
            if (m_bankPoints >= m_bankPointTarget)
                return;
            if (m_nbCardOnTheWay > 0)
                return;

            if (m_boardLogic.IsBoardFull())
            {
                if (!CanCrunch)
                {
                    EndGame();
                }
                return;
            }
        }

        private void EndGame()
        {
            if (m_isGameEnded)
                return;
            m_isGameEnded = true;

            MainProcess mp = MainProcess.Instance;
            AchievementManager am = mp.Achievements;
            am._NotifyEventIncrement("GameFinished");
            am._NotifyEventMaxing("BestScore", m_currentScore);
            am._NotifyEventIncrement("TotalScore", m_currentScore);
            am._NotifyEventIncrement("TotalTileLinked", m_currentTileScore);
            am._NotifyEventIncrement("TotalTileCrunched", m_nbCrunch);
            am.Save();
            mp.DisplayCompletedAchievements();

            m_guiManager.NotifyEndGame();

            Dictionary<string, object> endTrackingParameters = new Dictionary<string, object>();
            endTrackingParameters["Score"] = m_currentScore;
            endTrackingParameters["TileMatched"] = m_currentTileScore;
            endTrackingParameters["GameDuration"] = Time.time - m_gameStartTime;
            endTrackingParameters["BankTarget"] = m_bankPointTarget;
            endTrackingParameters["HasUsedHold"] = m_holdedCard != null;
            endTrackingParameters["NbCombo"] = m_nbCombo;
            TrackingManager.TrackEvent("GameEnd",1 , endTrackingParameters);

            GameSaver.DeleteGameSaved();
        }

        public bool CanBank
        {
            get {
                return m_bankPoints >= m_bankPointTarget;
            }
        }

        public void RequestBank()
        {
            if (IsGamePaused)
                return;
            if (CanBank && m_nbCardOnTheWay == 0)
            {
                EndCombo();
                UntoggleCrunch();
            }
        }

        public void RequestTileCrunchToggle()
        {
            if (IsGamePaused)
                return;

            if (m_currentState == DefaultState)
            {
                if (CanCrunch && m_nbCardOnTheWay < 1)
                {
                    SetState(TileCrunchState);
                    m_guiManager.SetFocusCrunch(false);
                }
            }
            else if (m_currentState == TileCrunchState)
            {
                SetState(DefaultState);
            }
        }

        private void UntoggleCrunch()
        {
            if (m_currentState == TileCrunchState)
            {
                SetState(DefaultState);
            }
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
            DefaultState = new GameState { ProcessPlayedSlot = DefaultProcessPlayedSlot, End = ExitDefaultState };
            TileCrunchState = new GameState { ProcessPlayedSlot = TileCrunchProcessSlot, Start = _TileCrunchStateStart, End = _TileCrunchStateEnd };
            m_currentState = DefaultState;
        }

        private void _TileCrunchStateStart()
        {
            var allTilePlaced = m_boardLogic.AllTilePlaced;
            bool shouldHighlight = (m_nbCrunch == 0) && (allTilePlaced.Count == 16);
            if (shouldHighlight)
            {
                m_crunchHighLight.Show();
            }
            foreach (var tile in allTilePlaced)
            {
                tile.PhysicalCard.IsWigglingAnimation = true;
                BoardSlot slot = m_board.GetSlot(tile.X, tile.Y);
                slot.IsFlashing = true;
            }
        }

        private void _TileCrunchStateEnd()
        {

            foreach (var tile in m_boardLogic.AllTilePlaced)
            {
                tile.PhysicalCard.IsWigglingAnimation = false;
            }
            foreach (var slot in m_board.m_slots)
            {
                slot.IsFlashing = false;
            }
            m_crunchHighLight.CancelHighLight();
        }

        public void OnHoldButtonPressed()
        {
            if (m_holdedCard != null)
            {
                m_holdedCard.MoveToPosition(m_playingCardTransform.position);
                m_holdedCard.m_tweens.ActivationVeil.StopTween();
                m_holdedCard.m_tweens.ActivationUnveil.StartTween();
                m_holdedCard.IsCurrentCard = true;
            }
            m_currentCard.MoveToPosition(m_holdingCardTransform.position);
            m_currentCard.m_tweens.ActivationUnveil.StopTween();
            m_currentCard.m_tweens.ActivationVeil.StartTween();
            m_currentCard.IsCurrentCard = false;
            m_currentCard.RequestCancelDrag();

            UntoggleCrunch();
            var temp = m_holdedCard;
            m_holdedCard = m_currentCard;
            if (temp != null)
                m_currentCard = temp;
            else
                DrawNextCards();

            AchievementManager.NotifyEventIncrement("TileKeeped");
        }

        public void OnNextButtonPressed()
        {
            if (m_currentState == DefaultState)
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
            else if (m_currentState == TileCrunchState)
            {
                SetState(DefaultState);
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

        public void RegisterTilePlayedListeners(Action act)
        {
            if (!m_onTilePlayedListeners.Contains(act))
                m_onTilePlayedListeners.Add(act);
        }

        public void UnregisterTilePlayedListeners(Action act)
        {
            if (m_onTilePlayedListeners.Contains(act))
                m_onTilePlayedListeners.Remove(act);
        }

        public void OnApplicationPause(bool pause)
        {
            if (pause)
            {
#if !UNITY_EDITOR
                if (!IsGamePaused)
                {
                    if (m_currentState == TileCrunchState)
                    {
                        RequestTileCrunchToggle();
                    }
                    m_guiManager.OnPausePressed();
                    
                }
#endif
            }
        }
        #endregion

        public void NotifyLeavingGame()
        {
            GameSaver.DeleteGameSaved();
        }

        #region Save

        private void Save()
        {
            m_gameSaver.Score = m_currentScore;
            m_gameSaver.TileScore = m_currentTileScore;
            m_gameSaver.BankTarget = m_bankPointTarget;
            m_gameSaver.BankState = m_bankPoints;
            m_gameSaver.OverLinkState = m_overLinkCurrent;
            m_gameSaver.CrunchState = m_crunchCoolDown;
            m_gameSaver.ComboCounter = m_nbCombo;
            m_gameSaver.CrunchCounter = m_nbCrunch;

            m_gameSaver.CurrentTile = (int)m_currentCard.MultiFlags;
            m_gameSaver.NextTile = m_nextPlayedCard == null ? 0 : (int)m_nextPlayedCard.MultiFlags;
            m_gameSaver.HoldTile = m_holdedCard == null ? 0 : (int)m_holdedCard.MultiFlags;
            m_boardLogic.FillArray(ref m_gameSaver.BoardState);

            var dirtyDistrib = (RationalDistributor)m_flagDistributor;
            m_gameSaver.DistributorDifficultyLevel = dirtyDistrib.DifficultyLevel;
            dirtyDistrib.FillPrecedence(ref m_gameSaver.DistributorPrecedence);

            m_gameSaver.Save();
        }

        private void LoadFromSave()
        {
            m_currentScore = m_gameSaver.Score;
            m_currentTileScore = m_gameSaver.TileScore;
            m_bankPointTarget = m_gameSaver.BankTarget;
            m_bankPoints = m_gameSaver.BankState;
            m_overLinkCurrent = m_gameSaver.OverLinkState;
            m_crunchCoolDown = m_gameSaver.CrunchState ;
            m_nbCombo = m_gameSaver.ComboCounter ;
            m_nbCrunch = m_gameSaver.CrunchCounter;

            m_guiManager.SetCrunchable(m_crunchCoolDown < 1);

            ArrowFlag currentFlag = (ArrowFlag)m_gameSaver.CurrentTile;

            GameObject currentCard = Instantiate(m_cardPrefab);
            currentCard.transform.SetParent(transform, false);
            currentCard.SetActive(true);
            m_currentCard = currentCard.GetComponent<ArrowCard>();
            m_currentCard.transform.position = m_playingCardTransform.position;
            m_currentCard.SetupArrows(currentFlag);
            m_currentCard.m_tweens.ActivationUnveil.StartTween();
            m_currentCard.IsCurrentCard = true;

            ((RationalDistributor)m_flagDistributor).Setup(m_gameSaver.DistributorDifficultyLevel, m_gameSaver.DistributorPrecedence);

            m_bankDots.SetNumberOfDots(m_bankPointTarget);
            for (int i = 0; i < m_bankPoints; ++i)
            {
                m_bankDots.LightDot(i);
            }

            m_guiManager.SetBankable(m_bankPoints >= m_bankPointTarget);

            m_guiManager.SetCapsuleBonusEnabled(m_bankPoints >= m_bankPointTarget);

            m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(BonusLevel), ComputeBankBonus(BonusLevel));
            if (BonusLevel > 1)
            {
                int nbDots = Math.Min(DotBonusLocalIndex + 1, c_dotBonusTarget - 1);
                for (int i = 0; i < nbDots; ++i)
                {
                    m_overlinkDotCollection.LightDot(i);
                }
            }

            m_guiManager.InstantSetScore(m_currentScore);

            ArrowFlag nextFlag = (ArrowFlag)m_gameSaver.NextTile;
            if (nextFlag != ArrowFlag.NONE)
            {
                GameObject nextCard = Instantiate(m_cardPrefab);
                nextCard.transform.SetParent(transform, false);
                nextCard.SetActive(true);
                m_nextPlayedCard = nextCard.GetComponent<ArrowCard>();
                m_nextPlayedCard.transform.position = m_popingCardTransform.position;
                m_nextPlayedCard.SetupArrows(nextFlag);
                m_nextPlayedCard.IsCurrentCard = false;
            }

            ArrowFlag holdFlag = (ArrowFlag)m_gameSaver.HoldTile;
            if (holdFlag != ArrowFlag.NONE)
            {
                GameObject holdCard = Instantiate(m_cardPrefab);
                holdCard.transform.SetParent(transform, false);
                holdCard.SetActive(true);
                m_holdedCard = holdCard.GetComponent<ArrowCard>();
                m_holdedCard.transform.position = m_holdingCardTransform.position;
                m_holdedCard.SetupArrows(holdFlag);
                m_holdedCard.IsCurrentCard = false;
            }

            for(int i = 0; i < m_gameSaver.BoardState.Length; ++i)
            {
                int f = m_gameSaver.BoardState[i];
                if (f > 0)
                {
                    ArrowFlag flags = (ArrowFlag)f;

                    int x = i % BoardLogic.c_col;
                    int y = i / BoardLogic.c_col;

                    GameObject go = Instantiate(m_cardPrefab);
                    go.transform.SetParent(transform, false);
                    go.SetActive(true);
                    ArrowCard card = go.GetComponent<ArrowCard>();
                    card.SetupArrows(flags);
                    LogicTile logicTile = m_boardLogic.AddTile(x, y, flags);
                    card.m_tweens.ActivationUnveil.StartTween();

                    var slot = m_board.GetSlot(x, y);
                    Vector3 target = slot.transform.position;
                    target.z = ArrowCard.c_firstLevel;
                    card.transform.position = target;

                    logicTile.PhysicalCard = card;
                    logicTile.PlayedFrame = (uint)Time.frameCount;
                    card.IsCurrentCard = false;

                    m_boardLogic.ComputeTileNeighbor(logicTile);

                    if (logicTile.m_listLinkedTile.Count > 0)
                    {

                        logicTile.PhysicalCard.LigthArrows(logicTile.m_linkedTile.Keys);
                        logicTile.PhysicalCard.SwitchCenter(true);
                        logicTile.PhysicalCard.IsSuperMode = true;
                        foreach (var entry in logicTile.m_linkedTile)
                        {
                            var neighbor = entry.Value;
                            var dir = entry.Key.Reverse();
                            neighbor.PhysicalCard.SwitchArrow(dir, true);
                            neighbor.PhysicalCard.SwitchCenter(true);
                            neighbor.PhysicalCard.IsSuperMode = true;
                        }

                    }
                }
            }


        }

        #endregion
    }


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

