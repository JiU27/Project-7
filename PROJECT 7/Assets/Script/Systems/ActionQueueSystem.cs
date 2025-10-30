using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Entities;
using CardBattleGame.Data;

namespace CardBattleGame.Systems
{
    /// <summary>
    /// 行动节点 - 代表一个行动
    /// </summary>
    public class ActionNode
    {
        public Character character;         // 行动者
        public Card card;                   // 玩家卡牌（如果是玩家行动）
        public EnemySkill enemySkill;      // 敌人技能（如果是敌人行动）
        public int speed;                   // 速度值
        public CardType cardType;           // 卡牌种类
        public bool isPlayerAction;         // 是否为玩家行动
        public Character target;            // 目标

        public ActionNode(Character character, Card card, Character target)
        {
            this.character = character;
            this.card = card;
            this.speed = card.currentSpeed;
            this.cardType = card.data.cardType;
            this.isPlayerAction = true;
            this.target = target;
        }

        public ActionNode(Character character, EnemySkill skill, Character target)
        {
            this.character = character;
            this.enemySkill = skill;
            this.speed = skill.speedValue;
            this.cardType = skill.cardType;
            this.isPlayerAction = false;
            this.target = target;
        }
    }

    /// <summary>
    /// 行动队列系统
    /// </summary>
    public class ActionQueueSystem : MonoBehaviour
    {
        private List<ActionNode> actionQueue = new List<ActionNode>();
        private StatusEffectSystem statusEffectSystem;
        private ElementalReactionSystem elementalReactionSystem;
        private Managers.BattleManager battleManager;

        public void Initialize(StatusEffectSystem statusSystem, ElementalReactionSystem elementalSystem, Managers.BattleManager battle)
        {
            statusEffectSystem = statusSystem;
            elementalReactionSystem = elementalSystem;
            battleManager = battle;
        }

        /// <summary>
        /// 添加玩家行动
        /// </summary>
        public void AddPlayerAction(Character player, Card card, Character target)
        {
            ActionNode node = new ActionNode(player, card, target);
            actionQueue.Add(node);
        }

        /// <summary>
        /// 添加敌人行动
        /// </summary>
        public void AddEnemyAction(Character enemy, EnemySkill skill, Character target)
        {
            ActionNode node = new ActionNode(enemy, skill, target);
            actionQueue.Add(node);
        }

        /// <summary>
        /// 整理行动顺序
        /// </summary>
        public void SortActionQueue()
        {
            actionQueue = actionQueue.OrderByDescending(a => a.speed)
                                     .ThenBy(a => a.isPlayerAction ? 1 : 0) // 同速度敌方先行动
                                     .ThenBy(a => GetTypePriority(a.cardType)) // Swift > Normal > Strong > None
                                     .ToList();

            Debug.Log($"===== 行动队列已整理，共 {actionQueue.Count} 个行动 =====");
            Debug.Log("行动顺序（速度从高到低）：");
            for (int i = 0; i < actionQueue.Count; i++)
            {
                var action = actionQueue[i];
                string actionName = action.isPlayerAction ? action.card.data.cardName : "敌人行动";
                Debug.Log($"  {i + 1}. {action.character.name} - {actionName} (速度:{action.speed}, 种类:{action.cardType})");
            }
        }

        /// <summary>
        /// 获取卡牌种类优先级
        /// </summary>
        private int GetTypePriority(CardType type)
        {
            switch (type)
            {
                case CardType.Swift: return 0;
                case CardType.Normal: return 1;
                case CardType.Strong: return 2;
                case CardType.None: return 3;
                default: return 4;
            }
        }

        /// <summary>
        /// 获取行动队列
        /// </summary>
        public List<ActionNode> GetActionQueue()
        {
            return new List<ActionNode>(actionQueue);
        }

        /// <summary>
        /// 清空行动队列
        /// </summary>
        public void ClearQueue()
        {
            actionQueue.Clear();
        }

        /// <summary>
        /// 检查Perfect Deflect
        /// </summary>
        public bool CheckPerfectDeflect(ActionNode current, ActionNode next)
        {
            if (current.speed != next.speed) return false;

            // 检查克制关系
            bool currentCountersNext = CheckCounterRelationship(current.cardType, next.cardType);
            bool nextCountersCurrent = CheckCounterRelationship(next.cardType, current.cardType);

            return currentCountersNext || nextCountersCurrent;
        }

        /// <summary>
        /// 检查克制关系 (Swift > Strong > Normal > Swift)
        /// </summary>
        private bool CheckCounterRelationship(CardType attacker, CardType defender)
        {
            if (attacker == CardType.Swift && defender == CardType.Strong) return true;
            if (attacker == CardType.Strong && defender == CardType.Normal) return true;
            if (attacker == CardType.Normal && defender == CardType.Swift) return true;
            return false;
        }

        /// <summary>
        /// 执行行动
        /// </summary>
        public void ExecuteAction(ActionNode action)
        {
            if (!action.character.IsAlive())
            {
                Debug.Log($"{action.character.name} 已死亡，跳过行动");
                return;
            }

            // 检查冰冻状态
            if (action.character.HasStatusEffect(StatusEffectType.Frozen))
            {
                Debug.Log($"{action.character.name} 被冰冻，无法行动");
                return;
            }

            // 播放动画
            if (action.isPlayerAction)
            {
                action.character.PlayAnimation(action.card.data.animationTrigger);
            }
            else
            {
                // 敌人动画暂时使用攻击动画
                action.character.PlayAnimation(AnimationTrigger.MeleeAttack);
            }

            // 执行效果
            if (action.isPlayerAction)
            {
                ExecuteCardEffect(action);
            }
            else
            {
                ExecuteEnemySkill(action);
            }
        }

        /// <summary>
        /// 执行卡牌效果
        /// </summary>
        private void ExecuteCardEffect(ActionNode action)
        {
            Card card = action.card;
            Character target = action.target;

            // 检查失准
            if (statusEffectSystem.CheckMissEffect(action.character))
            {
                Debug.Log($"{action.character.name} 的攻击失准了！");
                return;
            }

            // 检查目标的隐匿
            if (card.data.cardEffect.dealsDamage && statusEffectSystem.CheckStealthEffect(target))
            {
                return; // 攻击被闪避
            }

            // 造成伤害
            if (card.data.cardEffect.dealsDamage)
            {
                // 判断是否为群体伤害（AoE）
                if (card.data.cardEffect.isAoE)
                {
                    // 对所有敌人造成伤害
                    List<Character> targets = GetAllEnemies(action.character);
                    foreach (var aoeTarget in targets)
                    {
                        if (aoeTarget.IsAlive())
                        {
                            DealDamageToTarget(action.character, aoeTarget, card.modifiedDamage, card.data.elementType);
                        }
                    }
                }
                else
                {
                    // 对单个目标造成伤害
                    DealDamageToTarget(action.character, target, card.modifiedDamage, card.data.elementType);
                }
            }

            // 恢复生命
            if (card.data.cardEffect.healsHealth)
            {
                action.character.Heal(card.modifiedHeal);
            }

            // 获得护甲
            if (card.data.cardEffect.grantsArmor)
            {
                action.character.GainArmor(card.modifiedArmor);
            }

            // 赋予状态效果
            if (card.data.cardEffect.grantsStatusEffect && card.data.cardEffect.statusType != StatusEffectType.None)
            {
                // TODO: 从数据库获取StatusEffectData
                // target.AddStatusEffect(card.data.cardEffect.statusType, card.modifiedStatusStacks, statusEffectData);
            }
        }

        /// <summary>
        /// 对目标造成伤害（按照文档规定的顺序）
        /// </summary>
        private void DealDamageToTarget(Character attacker, Character target, int baseDamage, ElementType elementType)
        {
            // 1. 计算攻击者的词条效果（力量、临时力量、虚弱）
            int damage = statusEffectSystem.CalculateAttackDamage(attacker, baseDamage);

            // 2. 先结算元素反应效果（可能修改伤害值）
            bool triggeredReaction = elementalReactionSystem.TriggerElementalReaction(
                target, elementType, attacker, ref damage);

            // 3. 如果是元素伤害且目标有炸弹，减少炸弹倒计时
            if (elementType != ElementType.Physical && target.HasStatusEffect(StatusEffectType.Bomb))
            {
                statusEffectSystem.ProcessBombElementDamage(target);
            }

            // 4. 最后造成伤害（TakeDamage内部会计算目标的防御词条）
            target.TakeDamage(damage, elementType);
            
            Debug.Log($"{attacker.name} 对 {target.name} 造成了 {damage} 点{elementType}伤害");
        }

        /// <summary>
        /// 获取所有敌人（用于群体伤害）
        /// </summary>
        private List<Character> GetAllEnemies(Character attacker)
        {
            List<Character> targets = new List<Character>();
            
            if (battleManager == null)
            {
                Debug.LogError("BattleManager引用未设置，无法获取目标列表");
                return targets;
            }

            // 如果攻击者是玩家，目标是所有敌人
            if (attacker is Player)
            {
                foreach (var enemy in battleManager.enemies)
                {
                    if (enemy != null)
                    {
                        targets.Add(enemy);
                    }
                }
            }
            // 如果攻击者是敌人，目标是玩家（单个）
            else
            {
                if (battleManager.player != null)
                {
                    targets.Add(battleManager.player);
                }
            }
            
            return targets;
        }

        /// <summary>
        /// 执行敌人技能
        /// </summary>
        private void ExecuteEnemySkill(ActionNode action)
        {
            EnemySkill skill = action.enemySkill;
            Character target = action.target;

            // 检查失准
            if (statusEffectSystem.CheckMissEffect(action.character))
            {
                Debug.Log($"{action.character.name} 的攻击失准了！");
                return;
            }

            // 检查目标的隐匿
            if (skill.dealsDamage && statusEffectSystem.CheckStealthEffect(target))
            {
                return; // 攻击被闪避
            }

            // 造成伤害（使用统一的伤害处理方法）
            if (skill.dealsDamage)
            {
                DealDamageToTarget(action.character, target, skill.damageAmount, skill.elementType);
            }

            // 恢复生命
            if (skill.healsHealth)
            {
                action.character.Heal(skill.healAmount);
            }

            // 获得护甲
            if (skill.grantsArmor)
            {
                action.character.GainArmor(skill.armorAmount);
            }

            // 赋予状态效果
            if (skill.grantsStatusEffect && skill.statusType != StatusEffectType.None)
            {
                // TODO: 从数据库获取StatusEffectData
                // target.AddStatusEffect(skill.statusType, skill.statusStacks, statusEffectData);
            }
        }
    }
}

