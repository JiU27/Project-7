using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Entities;
using CardBattleGame.Systems;
using CardBattleGame.Data;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 战斗管理器 - 核心战斗逻辑控制
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("管理器引用")]
        public CardManager cardManager;
        public DeckManager deckManager;
        public UIManager uiManager;
        public HandUIManager handUIManager;
        public ActionQueueUIManager actionQueueUIManager;
        public BattleUIUpdater battleUIUpdater;

        [Header("系统引用")]
        public TurnSystem turnSystem;
        public ActionQueueSystem actionQueueSystem;
        public StatusEffectSystem statusEffectSystem;
        public ElementalReactionSystem elementalReactionSystem;

        [Header("角色")]
        public Player player;
        public List<Enemy> enemies = new List<Enemy>();

        [Header("战斗配置")]
        public CharacterData playerCharacterData;
        public List<MonsterData> encounterMonsters;
        public OutputSystemData currentOutputSystem;
        public List<WatchResultData> watchResultDatabase;
        public int sceneBackgroundId;

        [Header("输出槽位")]
        public List<Card> outputSlots = new List<Card>();
        public Enemy currentTarget;

        private WatchResultData currentWatchResult;

        private void Start()
        {
            InitializeBattle();
        }

        /// <summary>
        /// 初始化战斗
        /// </summary>
        public void InitializeBattle()
        {
            // 初始化系统（注意：ActionQueueSystem需要BattleManager引用）
            actionQueueSystem.Initialize(statusEffectSystem, elementalReactionSystem, this);
            turnSystem.Initialize(actionQueueSystem, statusEffectSystem);
            cardManager.Initialize(deckManager);

            // 初始化玩家
            InitializePlayer();

            // 初始化敌人
            InitializeEnemies();

            // 初始化输出槽位
            InitializeOutputSlots();

            // 初始化UI更新器
            if (battleUIUpdater != null)
            {
                battleUIUpdater.InitializeOutputSlots();
                battleUIUpdater.UpdateAllCharacterUI();
            }
            else
            {
                Debug.LogWarning("BattleUIUpdater 未设置，部分UI功能可能无法使用");
            }

            // 开始第一回合
            StartCoroutine(StartBattleSequence());
        }

        /// <summary>
        /// 初始化玩家（角色已由BattleSceneInitializer生成）
        /// </summary>
        private void InitializePlayer()
        {
            if (player != null && playerCharacterData != null)
            {
                // 如果玩家还未初始化，则初始化
                if (player.currentHealth == 0)
                {
                    player.Initialize(playerCharacterData);
                }
                Debug.Log($"玩家初始化：{playerCharacterData.characterName}");
            }
            else
            {
                Debug.LogError("玩家未设置或玩家数据为空！请确保BattleSceneInitializer已正确设置。");
            }
        }

        /// <summary>
        /// 初始化敌人（角色已由BattleSceneInitializer生成）
        /// </summary>
        private void InitializeEnemies()
        {
            // 敌人已由BattleSceneInitializer实例化
            // 这里只需要确认敌人已存在
            if (enemies.Count == 0)
            {
                Debug.LogError("没有敌人！请确保BattleSceneInitializer已正确生成敌人。");
                return;
            }

            foreach (var enemy in enemies)
            {
                Debug.Log($"初始化敌人：{enemy.GetMonsterData().monsterName}");
            }

            // 设置默认目标为第一个敌人
            if (enemies.Count > 0)
            {
                currentTarget = enemies[0];
            }
        }

        /// <summary>
        /// 初始化输出槽位
        /// </summary>
        private void InitializeOutputSlots()
        {
            outputSlots.Clear();
            if (currentOutputSystem != null)
            {
                for (int i = 0; i < currentOutputSystem.slotCount; i++)
                {
                    outputSlots.Add(null);
                }
            }
        }

        /// <summary>
        /// 开始战斗序列
        /// </summary>
        private IEnumerator StartBattleSequence()
        {
            turnSystem.StartNewTurn();

            // 0. 初始化战斗牌库（重要！）
            InitializeBattleDeck();

            // 1. 展示怀表
            yield return StartCoroutine(ShowWatchPanel());

            // 2. 抽取初始手牌
            List<Card> initialHand = cardManager.GetInitialHand(3);
            
            // 显示手牌UI
            if (handUIManager != null)
            {
                handUIManager.DisplayHand(initialHand);
            }
            else
            {
                Debug.LogWarning("HandUIManager 未设置，无法显示手牌UI");
            }

            // 3. 触发怀表效果
            ApplyWatchEffect();

            // 4. 显示敌人行动意图
            ShowEnemyIntents();

            // 5. 进入规划阶段
            turnSystem.AdvancePhase();
            
            Debug.Log("准备阶段完成，等待玩家规划");
        }

        /// <summary>
        /// 显示敌人行动意图
        /// </summary>
        private void ShowEnemyIntents()
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive()) continue;

                // 获取敌人本回合的行动
                List<EnemySkill> skills = enemy.GetActionsForTurn(turnSystem.currentTurn);

                // 在敌人UI上显示行动意图
                if (enemy.enemyUI != null)
                {
                    enemy.enemyUI.UpdateIntents(skills);
                    Debug.Log($"显示 {enemy.name} 的行动意图：{skills.Count} 个行动");
                }
                else
                {
                    Debug.LogWarning($"{enemy.name} 没有 EnemyUI 组件，无法显示行动意图");
                }
            }
        }

        /// <summary>
        /// 初始化战斗牌库
        /// </summary>
        private void InitializeBattleDeck()
        {
            // 从DeckManager创建卡牌实例
            List<Card> battleDeck = deckManager.CreateCardsFromDeck();
            
            if (battleDeck == null || battleDeck.Count == 0)
            {
                Debug.LogError("战斗牌库为空！无法开始战斗。请检查卡组配置。");
                return;
            }

            // 初始化CardManager的战斗牌库
            cardManager.InitializeBattleDeck(battleDeck);
            
            Debug.Log($"战斗牌库初始化完成：共 {battleDeck.Count} 张卡牌");
        }

        /// <summary>
        /// 展示怀表面板
        /// </summary>
        private IEnumerator ShowWatchPanel()
        {
            // 随机选择怀表结果
            if (watchResultDatabase.Count > 0)
            {
                currentWatchResult = watchResultDatabase[Random.Range(0, watchResultDatabase.Count)];
                turnSystem.SetWatchResult(currentWatchResult.resultType);
                
                Debug.Log($"怀表结果：{currentWatchResult.resultType} - {currentWatchResult.effectDescription}");
                
                // 显示怀表Panel
                if (uiManager != null)
                {
                    uiManager.ShowWatchPanel(currentWatchResult.watchSprite, currentWatchResult.effectDescription);
                    
                    // 等待玩家点击继续或自动关闭
                    yield return new WaitForSeconds(3f);
                    
                    uiManager.HideWatchPanel();
                }
                else
                {
                    Debug.LogWarning("UIManager 未设置，无法显示怀表面板");
                    yield return new WaitForSeconds(2f);
                }
            }
        }

        /// <summary>
        /// 应用怀表效果
        /// </summary>
        private void ApplyWatchEffect()
        {
            if (currentWatchResult == null) return;

            switch (currentWatchResult.resultType)
            {
                case WatchResult.DrawCard:
                    // 额外抽取一张卡牌
                    Card drawnCard = cardManager.DrawCard();
                    if (drawnCard != null)
                    {
                        Debug.Log($"怀表效果：额外抽取1张卡牌 - {drawnCard.data.cardName}");
                        
                        // 更新手牌UI显示
                        if (handUIManager != null)
                        {
                            handUIManager.AddCardUI(drawnCard);
                        }
                    }
                    break;

                case WatchResult.ApplyRandomElement:
                    // 为没有元素残留的敌人挂上随机元素残留
                    ApplyRandomElementToEnemies();
                    break;

                case WatchResult.DealDamage:
                    // 对所有敌人造成2点随机元素伤害
                    DealRandomDamageToAllEnemies();
                    break;

                case WatchResult.BoostNoneType:
                    // 本回合"无"类型卡牌效果提升25%
                    turnSystem.ActivateNoneTypeBoost();
                    Debug.Log("怀表效果：本回合'无'类型卡牌效果提升25%");
                    break;

                case WatchResult.GrantTemporaryCard:
                    // 获得一张临时卡牌
                    if (currentWatchResult.temporaryCardId > 0)
                    {
                        Card tempCard = cardManager.AddTemporaryCard(currentWatchResult.temporaryCardId);
                        if (tempCard != null)
                        {
                            Debug.Log($"怀表效果：获得临时卡牌 - {tempCard.data.cardName}");
                            
                            // 更新手牌UI显示
                            if (handUIManager != null)
                            {
                                handUIManager.AddCardUI(tempCard);
                            }
                        }
                    }
                    break;

                case WatchResult.GrantRandomBuff:
                    // 获得随机增益状态
                    GrantRandomBuff();
                    break;
            }
        }

        /// <summary>
        /// 为敌人挂上随机元素残留
        /// </summary>
        private void ApplyRandomElementToEnemies()
        {
            ElementType[] elements = { ElementType.Fire, ElementType.Water, ElementType.Earth, ElementType.Air };
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive()) continue;

                // 检查是否已有元素残留
                bool hasResidue = enemy.HasStatusEffect(StatusEffectType.FireResidue) ||
                                 enemy.HasStatusEffect(StatusEffectType.WaterResidue) ||
                                 enemy.HasStatusEffect(StatusEffectType.EarthResidue) ||
                                 enemy.HasStatusEffect(StatusEffectType.AirResidue);

                if (!hasResidue)
                {
                    ElementType randomElement = elements[Random.Range(0, elements.Length)];
                    
                    // 获取对应的状态效果类型
                    StatusEffectType residueType = GetResidueTypeFromElement(randomElement);
                    
                    // 从数据库获取StatusEffectData
                    StatusEffectData residueData = GetStatusEffectData(residueType);
                    
                    if (residueData != null)
                    {
                        enemy.AddStatusEffect(residueType, 1, residueData);
                        Debug.Log($"怀表效果：{enemy.name} 被挂上 {randomElement} 元素残留");
                    }
                    else
                    {
                        Debug.LogWarning($"未找到 {residueType} 的StatusEffectData！请检查数据库配置。");
                    }
                }
            }
        }
        
        private StatusEffectType GetResidueTypeFromElement(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire: return StatusEffectType.FireResidue;
                case ElementType.Water: return StatusEffectType.WaterResidue;
                case ElementType.Earth: return StatusEffectType.EarthResidue;
                case ElementType.Air: return StatusEffectType.AirResidue;
                default: return StatusEffectType.FireResidue;
            }
        }

        /// <summary>
        /// 对所有敌人造成随机元素伤害
        /// </summary>
        private void DealRandomDamageToAllEnemies()
        {
            ElementType[] elements = { ElementType.Fire, ElementType.Water, ElementType.Earth, ElementType.Air };
            ElementType randomElement = elements[Random.Range(0, elements.Length)];

            Debug.Log($"怀表效果：对所有敌人造成2点 {randomElement} 伤害");
            
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive())
                {
                    enemy.TakeDamage(2, randomElement);
                    // TakeDamage内部会自动更新UI
                }
            }
            
            // 额外确保UI更新
            if (battleUIUpdater != null)
            {
                battleUIUpdater.UpdateAllCharacterUI();
            }
        }

        /// <summary>
        /// 获得随机增益
        /// </summary>
        private void GrantRandomBuff()
        {
            StatusEffectType[] buffs = { StatusEffectType.Strength, StatusEffectType.Fortify, StatusEffectType.Regeneration };
            StatusEffectType randomBuff = buffs[Random.Range(0, buffs.Length)];
            
            Debug.Log($"怀表效果：玩家获得1层 {randomBuff}");
            
            // 从数据库获取StatusEffectData
            StatusEffectData buffData = GetStatusEffectData(randomBuff);
            
            if (buffData != null && player != null)
            {
                player.AddStatusEffect(randomBuff, 1, buffData);
                Debug.Log($"成功为玩家添加 {randomBuff} 效果");
            }
            else
            {
                if (buffData == null)
                {
                    Debug.LogWarning($"未找到 {randomBuff} 的StatusEffectData！请检查数据库配置。");
                }
                if (player == null)
                {
                    Debug.LogError("玩家对象为空！");
                }
            }
        }

        /// <summary>
        /// 将卡牌放入输出槽位
        /// </summary>
        public bool PlaceCardInSlot(Card card, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= outputSlots.Count)
            {
                Debug.LogWarning("无效的槽位索引");
                return false;
            }

            if (outputSlots[slotIndex] != null)
            {
                Debug.LogWarning("槽位已被占用");
                return false;
            }

            // 应用槽位效果
            card.isInOutputSlot = true;
            card.outputSlotIndex = slotIndex;
            card.ApplySlotEffect(currentOutputSystem.slotEffects[slotIndex]);

            // 如果怀表效果4激活且卡牌为"无"类型，应用加成
            if (turnSystem.HasNoneTypeBoost() && card.data.cardType == CardType.None)
            {
                card.ApplyNoneTypeBoost();
            }

            // 设置目标
            card.targetEnemy = currentTarget;

            outputSlots[slotIndex] = card;
            Debug.Log($"卡牌 {card.data.cardName} 放入槽位 {slotIndex}，速度：{card.currentSpeed}");
            
            return true;
        }

        /// <summary>
        /// 从输出槽位移除卡牌
        /// </summary>
        public Card RemoveCardFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= outputSlots.Count)
            {
                return null;
            }

            Card card = outputSlots[slotIndex];
            if (card != null)
            {
                card.ResetModifiers();
                outputSlots[slotIndex] = null;
                Debug.Log($"卡牌 {card.data.cardName} 从槽位 {slotIndex} 移除");
            }

            return card;
        }

        /// <summary>
        /// 检查是否所有槽位都已填满
        /// </summary>
        public bool AreAllSlotsFilled()
        {
            return outputSlots.All(slot => slot != null);
        }

        /// <summary>
        /// 结束回合（玩家点击END按钮）
        /// </summary>
        public void EndPlayerTurn()
        {
            if (!AreAllSlotsFilled())
            {
                // TODO: 显示确认窗口
                Debug.LogWarning("槽位未填满，需要确认");
                return;
            }

            // 进入战斗阶段
            StartCoroutine(ExecuteCombatPhase());
        }

        /// <summary>
        /// 执行战斗阶段
        /// </summary>
        private IEnumerator ExecuteCombatPhase()
        {
            turnSystem.AdvancePhase(); // 进入Combat阶段

            // 1. 清空行动队列
            actionQueueSystem.ClearQueue();

            // 2. 添加玩家的行动
            foreach (var card in outputSlots)
            {
                if (card != null)
                {
                    actionQueueSystem.AddPlayerAction(player, card, card.targetEnemy);
                }
            }

            // 3. 添加敌人的行动
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive()) continue;

                List<EnemySkill> skills = enemy.GetActionsForTurn(turnSystem.currentTurn);
                foreach (var skill in skills)
                {
                    actionQueueSystem.AddEnemyAction(enemy, skill, player);
                }
            }

            // 4. 排序行动队列
            actionQueueSystem.SortActionQueue();

            // 显示行动队列UI
            if (actionQueueUIManager != null)
            {
                List<ActionNode> queuedActions = actionQueueSystem.GetActionQueue();
                actionQueueUIManager.DisplayActionQueue(queuedActions);
            }
            else
            {
                Debug.LogWarning("ActionQueueUIManager 未设置，无法显示行动队列UI");
            }

            yield return new WaitForSeconds(1f); // 给玩家时间查看行动顺序

            // 5. 执行所有行动（包括Perfect Deflect检测）
            yield return StartCoroutine(ExecuteAllActions());

            // 6. 清空输出槽位和UI
            ClearOutputSlotsAndUI();

            // 7. 清空行动队列UI
            if (actionQueueUIManager != null)
            {
                actionQueueUIManager.ClearActionQueue();
            }

            // 8. 进入结算阶段
            StartCoroutine(ExecuteResolutionPhase());
        }

        /// <summary>
        /// 清空输出槽位和UI显示
        /// </summary>
        private void ClearOutputSlotsAndUI()
        {
            for (int i = 0; i < outputSlots.Count; i++)
            {
                Card card = outputSlots[i];
                if (card != null)
                {
                    cardManager.PlayCard(card);
                    outputSlots[i] = null;
                }
                
                // 清空对应槽位的速度显示
                if (battleUIUpdater != null)
                {
                    battleUIUpdater.outputSlots[i]?.ClearSlot();
                }
            }

            Debug.Log("输出槽位已清空");
        }

        /// <summary>
        /// 执行结算阶段
        /// </summary>
        private IEnumerator ExecuteResolutionPhase()
        {
            turnSystem.AdvancePhase(); // 进入Resolution阶段

            Debug.Log("===== 结算阶段开始 =====");

            // 1. 处理所有状态效果
            List<Character> allCharacters = new List<Character> { player };
            allCharacters.AddRange(enemies.Cast<Character>());
            statusEffectSystem.ProcessEndOfTurnEffects(allCharacters);

            // 2. 护甲不清除（根据文档）

            // 3. 弃掉非固有手牌并更新UI
            cardManager.DiscardNonInherentCards();
            
            // 清除手牌UI显示
            if (handUIManager != null)
            {
                handUIManager.ClearHandUI();
                Debug.Log("手牌UI已清除");
            }

            yield return new WaitForSeconds(1f);

            // 4. 检查战斗是否结束
            if (turnSystem.CheckBattleEnd(player, enemies))
            {
                yield return StartCoroutine(EndBattle());
                yield break;
            }

            // 5. 开始新回合
            Debug.Log("===== 结算阶段结束，准备开始新回合 =====");
            yield return StartCoroutine(StartBattleSequence());
        }

        /// <summary>
        /// 执行所有行动（包括Perfect Deflect处理）
        /// </summary>
        private IEnumerator ExecuteAllActions()
        {
            List<ActionNode> actions = actionQueueSystem.GetActionQueue();
            int i = 0;

            while (i < actions.Count)
            {
                ActionNode currentAction = actions[i];

                // 检查Perfect Deflect
                bool perfectDeflectTriggered = false;
                ActionNode deflectingAction = null;

                // 查找与当前行动速度相同的下一个行动
                for (int j = i + 1; j < actions.Count; j++)
                {
                    if (actions[j].speed == currentAction.speed)
                    {
                        // 检查是否有克制关系
                        if (actionQueueSystem.CheckPerfectDeflect(currentAction, actions[j]))
                        {
                            perfectDeflectTriggered = true;
                            
                            // 确定谁克制谁
                            bool currentCountersNext = CheckCounterRelationship(currentAction.cardType, actions[j].cardType);
                            
                            if (currentCountersNext)
                            {
                                // 当前行动克制下一个 - 下一个被打断
                                deflectingAction = currentAction;
                                Debug.Log($"=== Perfect Deflect! ===");
                                Debug.Log($"克制方：{currentAction.character.name}（{currentAction.cardType}）");
                                Debug.Log($"被克制方：{actions[j].character.name}（{actions[j].cardType}）");
                                Debug.Log($"结果：{actions[j].character.name} 的行动被取消");
                                
                                // 播放被克制方的动画（开始行动但被打断）
                                actions[j].character.PlayAnimation(AnimationTrigger.TakeDamage);
                                
                                // 显示Perfect Deflect弹窗
                                if (uiManager != null)
                                {
                                    uiManager.ShowPerfectDeflectPanel(currentAction.character.name, actions[j].character.name);
                                    yield return new WaitForSeconds(1.5f);
                                    uiManager.HidePerfectDeflectPanel();
                                }
                                else
                                {
                                    yield return new WaitForSeconds(0.5f);
                                }
                                
                                // UI处理：先移除被克制方的UI节点（在索引j的位置）
                                if (actionQueueUIManager != null)
                                {
                                    actionQueueUIManager.RemoveNodeAtIndex(j);
                                    Debug.Log($"移除被克制方的UI节点（索引{j}）");
                                }
                                
                                // 从数据列表移除被克制的行动
                                actions.RemoveAt(j);
                            }
                            else
                            {
                                // 下一个行动克制当前 - 当前被打断
                                deflectingAction = actions[j];
                                Debug.Log($"=== Perfect Deflect! ===");
                                Debug.Log($"克制方：{actions[j].character.name}（{actions[j].cardType}）");
                                Debug.Log($"被克制方：{currentAction.character.name}（{currentAction.cardType}）");
                                Debug.Log($"结果：{currentAction.character.name} 的行动被取消，{actions[j].character.name} 提前执行");
                                
                                // 播放被克制方的动画（当前行动）
                                currentAction.character.PlayAnimation(AnimationTrigger.TakeDamage);
                                
                                // 显示Perfect Deflect弹窗
                                if (uiManager != null)
                                {
                                    uiManager.ShowPerfectDeflectPanel(actions[j].character.name, currentAction.character.name);
                                    yield return new WaitForSeconds(1.5f);
                                    uiManager.HidePerfectDeflectPanel();
                                }
                                else
                                {
                                    yield return new WaitForSeconds(0.5f);
                                }
                                
                                // UI处理：移除被克制方的节点（当前，第一个）
                                if (actionQueueUIManager != null)
                                {
                                    actionQueueUIManager.RemoveFirstActionNode();
                                    Debug.Log("移除被克制方的UI节点（当前行动）");
                                }
                                
                                // 执行克制方的行动（actions[j]）
                                actionQueueSystem.ExecuteAction(deflectingAction);
                                
                                // UI处理：移除克制方的节点（现在是第一个，因为被克制方已删除）
                                if (actionQueueUIManager != null)
                                {
                                    actionQueueUIManager.RemoveFirstActionNode();
                                    Debug.Log("移除克制方的UI节点（提前执行）");
                                }
                                
                                // 从数据列表移除两个行动
                                actions.RemoveAt(j);  // 先移除j（索引大的）
                                actions.RemoveAt(i);  // 再移除i
                                i--;  // 因为删除了当前，索引需要调整
                            }
                            
                            break; // 只处理第一个Perfect Deflect
                        }
                    }
                    else
                    {
                        break; // 速度不同，后面的更慢，不需要继续检查
                    }
                }

                // 处理行动执行和UI更新
                if (!perfectDeflectTriggered)
                {
                    // 没有Perfect Deflect，正常执行
                    if (i < actions.Count)
                    {
                        actionQueueSystem.ExecuteAction(currentAction);
                        
                        // 行动执行后，移除对应的UI节点（第一个，因为是当前行动）
                        if (actionQueueUIManager != null)
                        {
                            actionQueueUIManager.RemoveFirstActionNode();
                        }
                    }
                }
                else
                {
                    // 发生了Perfect Deflect
                    if (deflectingAction == currentAction)
                    {
                        // 当前行动克制了下一个，当前正常执行
                        if (i < actions.Count)
                        {
                            actionQueueSystem.ExecuteAction(currentAction);
                            
                            // 移除当前行动的UI节点（第一个）
                            if (actionQueueUIManager != null)
                            {
                                actionQueueUIManager.RemoveFirstActionNode();
                            }
                        }
                        // 被克制的actions[j]已经从数据列表移除，UI也需要移除对应节点
                        // 但它不是第一个，需要单独处理（暂时跳过，下次循环会处理）
                    }
                    else
                    {
                        // 当前行动被克制，已经在上面执行了克制方的行动并移除了
                        // 这里不需要再执行，只需要移除UI节点
                        if (actionQueueUIManager != null)
                        {
                            // 被克制方的节点（当前，第一个）
                            actionQueueUIManager.RemoveFirstActionNode();
                        }
                        // 克制方的行动已在上面执行，但UI节点还在，需要移除
                        // 注意：克制方的节点现在在第二个位置（因为当前被删了）
                        if (actionQueueUIManager != null && actionQueueUIManager.GetNodeCount() > 0)
                        {
                            actionQueueUIManager.RemoveFirstActionNode();
                        }
                    }
                }

                // 检查战斗是否结束
                if (turnSystem.CheckBattleEnd(player, enemies))
                {
                    yield return StartCoroutine(EndBattle());
                    yield break;
                }

                yield return new WaitForSeconds(1f); // 动画等待时间

                i++;
            }
        }

        /// <summary>
        /// 检查克制关系（与ActionQueueSystem保持一致）
        /// </summary>
        private bool CheckCounterRelationship(CardType attacker, CardType defender)
        {
            if (attacker == CardType.Swift && defender == CardType.Strong) return true;
            if (attacker == CardType.Strong && defender == CardType.Normal) return true;
            if (attacker == CardType.Normal && defender == CardType.Swift) return true;
            return false;
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        private IEnumerator EndBattle()
        {
            if (turnSystem.currentPhase == BattlePhase.Victory)
            {
                Debug.Log("===== 战斗胜利 =====");
                // TODO: 显示胜利Panel
            }
            else if (turnSystem.currentPhase == BattlePhase.Defeat)
            {
                Debug.Log("===== 战斗失败 =====");
                // TODO: 显示失败Panel
            }

            yield return null;
        }

        /// <summary>
        /// 从数据库获取StatusEffectData
        /// </summary>
        private StatusEffectData GetStatusEffectData(StatusEffectType type)
        {
            if (elementalReactionSystem == null)
            {
                Debug.LogError("ElementalReactionSystem 未设置！");
                return null;
            }

            if (elementalReactionSystem.statusEffectDatabase == null || 
                elementalReactionSystem.statusEffectDatabase.Count == 0)
            {
                Debug.LogError("Status Effect Database 为空！请在 ElementalReactionSystem 中配置。");
                return null;
            }

            foreach (var data in elementalReactionSystem.statusEffectDatabase)
            {
                if (data != null && data.statusType == type)
                {
                    return data;
                }
            }

            Debug.LogWarning($"在数据库中未找到类型 {type} 的StatusEffectData");
            return null;
        }

        /// <summary>
        /// 切换目标敌人
        /// </summary>
        public void SetTargetEnemy(Enemy enemy)
        {
            if (enemies.Contains(enemy) && enemy.IsAlive())
            {
                currentTarget = enemy;
                Debug.Log($"目标切换为：{enemy.name}");
                // TODO: 更新瞄准图标UI
            }
        }
    }
}

