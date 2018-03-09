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
        public bool IsHoldingCard { get { return m_holdedCard != null; } }

        [SerializeField]
        AnimatedLinePool m_animatedLinePool = null;
        public AnimatedLinePool AnimatedLinePool { get { return m_animatedLinePool; } }
        [SerializeField]
        AnimatedLinePool m_darkAnimatedLinePool = null;
        
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
        private const int c_maxCrunchPoints = 33;

        private int m_bankPointTarget = 2;
        private int m_bankPoints = 0;
        public const int c_maxBankPoints = 33;
        
        [SerializeField]
        private DotCollection m_bankDots = null;
        [SerializeField]
        private DotCollection m_crunchDots = null;
        [SerializeField]
        private DotCollection m_overlinkDotCollection = null;

        private DelayedActionCollection m_bankDelayedAction = new DelayedActionCollection();
        private DelayedActionCollection m_crunchDelayedAction = new DelayedActionCollection();
        private DelayedActionCollection m_overLinkDelayedAction = new DelayedActionCollection();
        
        private bool m_isGameEnded = false;
        private float m_gameStartTime = float.MaxValue;

        List<Action> m_onTilePlayedListeners = null;

        private const int c_dotBonusTarget = 4;
        private int m_dotBonusCurrent = c_dotBonusTarget;
        private int m_bonusLevel = 0;

        private const float c_multiplierPerBonus = 1.0f;

        private int m_nbCombo = 0;
        private int m_nbCrunchInCombo = 0;

        public bool IsGamePaused = false;


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
            m_nbCombo = 0;

            m_bankDots.SetNumberOfDots(m_bankPointTarget);
            m_crunchDots.SetNumberOfDots(m_crunchTarget);

            m_guiManager.SetBankable(false);
            m_guiManager.SetCrunchable(false);

            m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(0), ComputeBankBonus(0));
            
            if (MainProcess.IsReady)
            {
                Dictionary<string, object> startParams = new Dictionary<string, object>();
                startParams["date"] = DateTime.UtcNow.ToString();
                TrackingManager.TrackEvent("Game Start", 1, startParams);
            }
            m_gameStartTime = Time.time;

            m_overlinkDotCollection.SetNumberOfDots(c_dotBonusTarget);
            //m_overlinkDotCollection.LightDot(0, false);
            
        }

        private void Update()
        {
            if (!IsGamePaused)
            {
                ProcessPressedCard();
                m_currentState.ProcessPlayedSlot();
                m_bankDelayedAction.ManualUpdate();
                m_crunchDelayedAction.ManualUpdate();
                m_overLinkDelayedAction.ManualUpdate();
            }
        }

        void DrawNextCard()
        {
            ShootNewCurrentCard();
        }

        private void ShootNewCurrentCard()
        {
            GameObject currentCardObject = Instantiate(m_cardPrefab);
            currentCardObject.transform.SetParent(transform, false);
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
            nextCardObject.transform.SetParent(transform, false);
            nextCardObject.SetActive(true);
            m_nextCard = nextCardObject.GetComponent<ArrowCard>();
            m_nextCard.transform.position = m_nextPlayingCardTransform.position;
            m_nextCard.PrepareIntroductionTween();
            var introTween = m_nextCard.m_tweens.Introduction;
            introTween.StartTween();
        }

        public void OnTilePressed(BoardSlot slot)
        {
            if (IsGamePaused)
                return;
            m_playedSlot = slot;
        }

        public void OnArrowCardPressed(ArrowCard pressedCard)
        {
            if (IsGamePaused)
                return;
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

                foreach (var act in m_onTilePlayedListeners)
                {
                    act();
                }
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
                
                HashSet<LogicTile> chain = new HashSet<LogicTile>();
                List<LogicTile> chainAsList = new List<LogicTile>();
                List<float> depthList = new List<float>();
                List<LogicLinkStandalone> freeLinks = new List<LogicLinkStandalone>();
                m_boardLogic.ComputeTileNeighbor(tile);
                tile.ComputeLinkedChain(ref chain, ref chainAsList, ref depthList, ref freeLinks);
                chainAsList.Remove(tile);

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

                m_nbCrunchInCombo++;
                m_crunchPoints = 0;
                m_crunchTarget = Mathf.Min(m_crunchTarget + m_nbCrunchInCombo, c_maxCrunchPoints);

                m_crunchDots.StopAllDots();
                m_crunchDots.SetNumberOfDots(m_crunchTarget);

                m_guiManager.SetCrunchable(false);

                RewardTiles(chainAsList);
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

            var newLinkDirection = tile.m_linkedTile.Keys;

            RewardTiles(chainAsList);


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
        }

        private void RewardTiles(List<LogicTile> chainAsList, float baseAnimDelay = 0f)
        {
            int pointToBank = 0;
            int pointToOverLink = 0;
            int pointToCrunch = 0;
            float animationDelay = baseAnimDelay;

            int chainCount = chainAsList.Count;
            if (chainCount > 1)
            {
                for (int i = 0; i < chainCount; i++)
                {
                    var tile = chainAsList[i];
                    var flashingCard = tile.PhysicalCard;
                    FlashWithDelay(i * c_flashDelay, flashingCard);
                }


                pointToBank = Mathf.Clamp(chainCount, 0, m_bankPointTarget - m_bankPoints);

                pointToCrunch = chainCount - pointToBank;
                pointToCrunch = Mathf.Min(pointToCrunch, m_crunchTarget - m_crunchPoints);

                pointToOverLink = chainCount - pointToBank - pointToCrunch;


                for (int i = 0; i < pointToBank; ++i)
                {

                    int dotIndex = m_bankPoints + i;
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

                m_bankPoints += pointToBank;
                if (m_bankPoints >= m_bankPointTarget)
                {
                    m_guiManager.SetBankable(true);
                }


                for (int i = 0; i < pointToCrunch; ++i)
                {
                    int dotIndex = m_crunchPoints + i;
                    Transform dot = m_crunchDots.GetDot(dotIndex);
                    var tIndex = i + pointToBank;
                    var logicTile = chainAsList[tIndex];

                    animationDelay += c_flashDelay;

                    Action lightDot = () =>
                    {
                        m_crunchDots.LightDot(dotIndex);
                    };

                    AnimatedLineWithDelay(animationDelay + c_lineDelay, logicTile.PhysicalCard.transform.position, dot.position, lightDot, m_crunchDelayedAction);

                }
                m_crunchPoints += pointToCrunch;
                if (m_crunchPoints >= m_crunchTarget)
                {
                    m_guiManager.SetCrunchable(true);

                    if (m_bankPoints >= m_bankPointTarget)
                    {
                        m_guiManager.SetCapsuleBonusEnabled(true);
                    }
                }


                if (pointToOverLink > 0)
                {
                    for (int i = 0; i < pointToOverLink; ++i)
                    {
                        m_dotBonusCurrent++;
                        int tileIndex = pointToBank + pointToCrunch + i;
                        var logicTilePosition = chainAsList[tileIndex].PhysicalCard.transform.position;

                        if (m_dotBonusCurrent <=  c_dotBonusTarget)
                        {
                            int dotIndex = m_dotBonusCurrent - 1;
                            Transform dot = m_overlinkDotCollection.GetDot(dotIndex);
                            Action lightDots = () =>
                            {
                                m_overlinkDotCollection.LightDot(dotIndex);
                            };
                            AnimatedLineWithDelay(animationDelay + c_lineDelay, logicTilePosition, dot.position, lightDots, m_overLinkDelayedAction);
                        }
                        else
                        {
                            m_dotBonusCurrent = 0;
                            m_bonusLevel++;
                            int levelCopy = m_bonusLevel;
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

        void EndCombo()
        {

            float animatedLineDuration = m_animatedLinePool.GetLineAnimationDuration();

            int tileLinked = 0;

            int nextBankTarget = m_bankPointTarget + ComputeBankBonus(m_bonusLevel);
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
            float multiplier = ComputeMultiplierBonus(m_bonusLevel);

            int gainedPoints = Mathf.FloorToInt(basePoints * multiplier);
            m_currentScore += gainedPoints;
            m_currentTileScore += tileLinked;
            m_nbCombo++;

            m_guiManager.NotifyDeltaScoreChanged(basePoints, animatedLineDuration);

            if (multiplier > 1f)
            {
                m_bankDelayedAction.AddAction(multiplierDelay + 1.5f, () => { m_guiManager.ApplyMultiplier(Mathf.FloorToInt(basePoints * multiplier)); });
                m_bankDelayedAction.AddAction(multiplierDelay + 3.5f, () => { m_guiManager.ApplyScoreDelta(m_currentScore); });
            }
            else
            {
                m_bankDelayedAction.AddAction(multiplierDelay + 1.5f, () => { m_guiManager.ApplyScoreDelta(m_currentScore); });
            }

            if (m_currentCard == null)
            {
                DrawNextCard();
            }

            m_bankDots.StopAllDots();
            m_bankPoints = 0;
            m_bankPointTarget = nextBankTarget;
            m_guiManager.SetBankable(false);

            float displayDelay = (m_bonusLevel > 0) ? multiplierDelay - animatedLineDuration : 0;

            m_bankDelayedAction.AddAction(displayDelay, () => {
                m_guiManager.SetCapsuleBonusValues(ComputeMultiplierBonus(0), ComputeBankBonus(0),true);
                m_guiManager.SetCapsuleBonusEnabled(false);
            });

            m_dotBonusCurrent = c_dotBonusTarget;
            m_bonusLevel = 0;
            m_nbCrunchInCombo = 0;

            m_flagDistributor.NotifyBank();

            CheckEndGame();

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
                        foreach (var link in card.m_tileLinks)
                        {
                            Destroy(link.gameObject);
                        }

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

        private float BankApplyBonusesAnimations(float animationDelay)
        {

            Vector3 multiplierPos = m_guiManager.MultiplierTransform.position;

            float lineAnimDuration = m_animatedLinePool.GetLineAnimationDuration();
            
            Vector3 startPosition = m_guiManager.BonusCapsuleTransform.position;

            float multiplier = ComputeMultiplierBonus(m_bonusLevel);
            int nbParticle = Mathf.Min(m_bonusLevel, 100);

            for (int i = 0; i < m_bonusLevel; ++i)
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
           
            int bankBonus = ComputeBankBonus(m_bonusLevel);
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

        const int c_baseComboPoints = 10;

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
                if (m_crunchPoints < m_crunchTarget)
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

            m_guiManager.NotifyEndGame();

            Dictionary<string, object> endTrackingParameters = new Dictionary<string, object>();
            endTrackingParameters["Score"] = m_currentScore;
            endTrackingParameters["TileMatched"] = m_currentTileScore;
            endTrackingParameters["GameDuration"] = Time.time - m_gameStartTime;
            endTrackingParameters["BankTarget"] = m_bankPointTarget;
            endTrackingParameters["CrunchTarget"] = m_crunchTarget;
            endTrackingParameters["HasUsedHold"] = m_holdedCard != null;
            endTrackingParameters["NbCombo"] = m_nbCombo;
            TrackingManager.TrackEvent("GameEnd",1 , endTrackingParameters);
        }

        public void RequestBank()
        {
            if (IsGamePaused)
                return;
            if (m_bankPoints >= m_bankPointTarget)
            {
                EndCombo();
            }
        }

        public void RequestTileCrunchToggle()
        {
            if (IsGamePaused)
                return;

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
                m_holdedCard.m_tweens.ActivationVeil.StopTween();
                m_holdedCard.m_tweens.ActivationUnveil.StartTween();
            }
            m_currentCard.MoveToPosition(m_holdingCardTransform.position);
            m_currentCard.m_tweens.ActivationUnveil.StopTween();
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

