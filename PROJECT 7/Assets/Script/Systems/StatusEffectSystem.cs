using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Entities;

namespace CardBattleGame.Systems
{
    /// <summary>
    /// 状态效果系统 - 处理状态效果的回合结算
    /// </summary>
    public class StatusEffectSystem : MonoBehaviour
    {
        /// <summary>
        /// 回合结束时处理所有状态效果
        /// </summary>
        public void ProcessEndOfTurnEffects(List<Character> allCharacters)
        {
            foreach (var character in allCharacters)
            {
                if (!character.IsAlive()) continue;

                ProcessCharacterEndOfTurnEffects(character);
            }
        }

        /// <summary>
        /// 处理单个角色的回合结束效果
        /// </summary>
        private void ProcessCharacterEndOfTurnEffects(Character character)
        {
            // 复制列表以避免在迭代时修改
            List<StatusEffect> effectsToProcess = new List<StatusEffect>(character.statusEffects);

            foreach (var effect in effectsToProcess)
            {
                switch (effect.data.statusType)
                {
                    case StatusEffectType.FireResidue:
                    case StatusEffectType.WaterResidue:
                    case StatusEffectType.EarthResidue:
                    case StatusEffectType.AirResidue:
                        // 元素残留：下个回合结束时失去1层
                        effect.RemoveStacks(1);
                        break;

                    case StatusEffectType.TempStrength:
                        // 临时力量：回合结束时失去所有
                        effect.SetStacks(0);
                        break;

                    case StatusEffectType.Weak:
                        // 虚弱：回合结束时失去1层
                        effect.RemoveStacks(1);
                        break;

                    case StatusEffectType.Fortify:
                        // 铸甲：回合结束时失去1层
                        effect.RemoveStacks(1);
                        break;

                    case StatusEffectType.Fragile:
                        // 碎甲：回合结束时失去1层
                        effect.RemoveStacks(1);
                        break;

                    case StatusEffectType.Regeneration:
                        // 再生：根据层数恢复生命值，然后失去1层
                        character.Heal(effect.stacks);
                        effect.RemoveStacks(1);
                        Debug.Log($"{character.name} 再生恢复了 {effect.stacks} 点生命");
                        break;

                    case StatusEffectType.Poison:
                        // 毒素：根据层数扣除生命值，然后失去1层
                        character.TakeDamage(effect.stacks);
                        effect.RemoveStacks(1);
                        Debug.Log($"{character.name} 受到 {effect.stacks} 点毒素伤害");
                        break;

                    case StatusEffectType.Frozen:
                        // 冰冻：回合结束时失去
                        effect.SetStacks(0);
                        Debug.Log($"{character.name} 的冰冻效果解除");
                        break;

                    case StatusEffectType.Miss:
                        // 失准：下回合结束时失去
                        effect.DecrementDuration();
                        if (effect.duration <= 0)
                        {
                            effect.SetStacks(0);
                        }
                        break;

                    case StatusEffectType.Sturdy:
                        // 坚固：下回合结束时失去1层
                        // 注意：坚固应该在下回合结束，不是当前回合
                        effect.DecrementDuration();
                        if (effect.duration <= 0)
                        {
                            effect.RemoveStacks(1);
                            if (effect.stacks <= 0)
                            {
                                Debug.Log($"{character.name} 的坚固效果消失");
                            }
                        }
                        break;

                    case StatusEffectType.Vulnerable:
                        // 脆弱：受到攻击时消耗（在Character.TakeDamage中处理）
                        // 下回合结束时清理未被触发的脆弱
                        effect.DecrementDuration();
                        if (effect.duration <= 0)
                        {
                            effect.SetStacks(0);
                            Debug.Log($"{character.name} 的脆弱效果到期（未被触发）");
                        }
                        break;

                    case StatusEffectType.Stealth:
                        // 隐匿：下回合结束时失去，若未被触发则重新挂上水元素残留
                        effect.DecrementDuration();
                        if (effect.duration <= 0)
                        {
                            // TODO: 检查是否被触发，如果没有则挂上水元素残留
                            effect.SetStacks(0);
                        }
                        break;

                    case StatusEffectType.Bomb:
                        // 炸弹：每回合倒计时
                        effect.DecrementDuration();
                        if (effect.duration <= 0)
                        {
                            // 爆炸！造成当前生命值20%的伤害（最低6点）
                            int bombDamage = Mathf.Max(6, Mathf.CeilToInt(character.currentHealth * 0.2f));
                            character.TakeDamage(bombDamage);
                            effect.SetStacks(0);
                            Debug.Log($"{character.name} 的炸弹爆炸了！造成 {bombDamage} 点伤害");
                        }
                        break;
                }
            }

            // 移除所有应该被移除的状态效果
            character.statusEffects.RemoveAll(e => e.ShouldBeRemoved());
            
            // 更新UI
            if (character.characterUI != null)
            {
                character.characterUI.UpdateStatusEffects(character.statusEffects);
            }
        }

        /// <summary>
        /// 计算攻击伤害（考虑力量、临时力量、虚弱等效果）
        /// </summary>
        public int CalculateAttackDamage(Character attacker, int baseDamage)
        {
            float damage = baseDamage;

            // 力量效果
            StatusEffect strengthEffect = attacker.GetStatusEffect(StatusEffectType.Strength);
            if (strengthEffect != null)
            {
                damage += strengthEffect.stacks;
            }

            // 临时力量效果
            StatusEffect tempStrengthEffect = attacker.GetStatusEffect(StatusEffectType.TempStrength);
            if (tempStrengthEffect != null)
            {
                damage += tempStrengthEffect.stacks;
            }

            // 虚弱效果
            StatusEffect weakEffect = attacker.GetStatusEffect(StatusEffectType.Weak);
            if (weakEffect != null)
            {
                damage = Mathf.Floor(damage * 0.75f);
            }

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// 检查失准效果（攻击是否命中）
        /// </summary>
        public bool CheckMissEffect(Character attacker)
        {
            StatusEffect missEffect = attacker.GetStatusEffect(StatusEffectType.Miss);
            if (missEffect != null)
            {
                // 50%概率失准
                return Random.value <= 0.5f;
            }
            return false;
        }

        /// <summary>
        /// 检查隐匿效果（是否闪避攻击）
        /// </summary>
        public bool CheckStealthEffect(Character target)
        {
            StatusEffect stealthEffect = target.GetStatusEffect(StatusEffectType.Stealth);
            if (stealthEffect != null)
            {
                // 闪避成功，移除隐匿
                target.RemoveStatusEffect(StatusEffectType.Stealth);
                Debug.Log($"{target.name} 触发隐匿，闪避了攻击！");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查炸弹受到元素伤害（减少倒计时）
        /// </summary>
        public void ProcessBombElementDamage(Character target)
        {
            StatusEffect bombEffect = target.GetStatusEffect(StatusEffectType.Bomb);
            if (bombEffect != null && bombEffect.duration > 0)
            {
                bombEffect.DecrementDuration();
                Debug.Log($"{target.name} 的炸弹倒计时减少，剩余 {bombEffect.duration} 回合");
            }
        }
    }
}

