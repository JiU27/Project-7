using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Entities;
using CardBattleGame.Data;

namespace CardBattleGame.Systems
{
    /// <summary>
    /// 元素反应系统
    /// </summary>
    public class ElementalReactionSystem : MonoBehaviour
    {
        [Header("状态效果数据库")]
        public List<StatusEffectData> statusEffectDatabase;

        private Dictionary<StatusEffectType, StatusEffectData> statusEffectDict;

        private void Awake()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            statusEffectDict = new Dictionary<StatusEffectType, StatusEffectData>();
            foreach (var data in statusEffectDatabase)
            {
                if (!statusEffectDict.ContainsKey(data.statusType))
                {
                    statusEffectDict.Add(data.statusType, data);
                }
            }
        }

        /// <summary>
        /// 检查并触发元素反应
        /// </summary>
        public bool TriggerElementalReaction(Character target, ElementType attackElement, Character attacker, ref int damage)
        {
            if (attackElement == ElementType.Physical || attackElement == ElementType.Chaos)
            {
                return false; // 物理和混沌不触发元素反应
            }

            // 检查目标是否有元素残留
            StatusEffectType residueType = GetResidueForElement(attackElement);
            StatusEffect existingResidue = GetExistingElementalResidue(target);

            if (existingResidue == null)
            {
                // 没有残留，直接挂上新的元素残留
                ApplyElementalResidue(target, attackElement);
                return false;
            }

            ElementType existingElement = GetElementFromResidue(existingResidue.data.statusType);
            
            // 如果是相同元素，只刷新残留
            if (existingElement == attackElement)
            {
                ApplyElementalResidue(target, attackElement);
                return false;
            }

            // 触发元素反应
            bool isTargetEnemy = target is Enemy;
            TriggerReaction(existingElement, attackElement, target, attacker, isTargetEnemy, ref damage);

            // 移除元素残留
            target.RemoveStatusEffect(existingResidue.data.statusType);

            return true;
        }

        /// <summary>
        /// 触发具体的元素反应
        /// </summary>
        private void TriggerReaction(ElementType element1, ElementType element2, Character target, Character attacker, bool targetIsEnemy, ref int damage)
        {
            // 火 x 水
            if ((element1 == ElementType.Fire && element2 == ElementType.Water) ||
                (element1 == ElementType.Water && element2 == ElementType.Fire))
            {
                if (targetIsEnemy)
                {
                    // 蒸汽爆炸 - 挂上炸弹
                    ApplyStatusEffect(target, StatusEffectType.Bomb, 1);
                    Debug.Log($"触发元素反应：蒸汽爆炸！{target.name}被挂上炸弹");
                }
                else
                {
                    // 雾影 - 获得隐匿
                    ApplyStatusEffect(target, StatusEffectType.Stealth, 1);
                    Debug.Log($"触发元素反应：雾影！{target.name}获得隐匿");
                }
            }
            // 火 x 土
            else if ((element1 == ElementType.Fire && element2 == ElementType.Earth) ||
                     (element1 == ElementType.Earth && element2 == ElementType.Fire))
            {
                if (targetIsEnemy)
                {
                    // 玻璃化 - 挂上脆弱
                    ApplyStatusEffect(target, StatusEffectType.Vulnerable, 1);
                    Debug.Log($"触发元素反应：玻璃化！{target.name}被挂上脆弱");
                }
                else
                {
                    // 熔甲 - 获得2层铸甲和4点护盾
                    ApplyStatusEffect(target, StatusEffectType.Fortify, 2);
                    target.GainArmor(4);
                    Debug.Log($"触发元素反应：熔甲！{target.name}获得2层铸甲和4点护盾");
                }
            }
            // 火 x 气
            else if ((element1 == ElementType.Fire && element2 == ElementType.Air) ||
                     (element1 == ElementType.Air && element2 == ElementType.Fire))
            {
                if (targetIsEnemy)
                {
                    // 爆燃 - 伤害提升50%，挂上1层虚弱
                    damage = Mathf.CeilToInt(damage * 1.5f);
                    ApplyStatusEffect(target, StatusEffectType.Weak, 1);
                    Debug.Log($"触发元素反应：爆燃！伤害提升50%，{target.name}被挂上虚弱");
                }
                else
                {
                    // 焰气 - 受到1点伤害，获得3层力量
                    target.TakeDamage(1);
                    ApplyStatusEffect(target, StatusEffectType.Strength, 3);
                    Debug.Log($"触发元素反应：焰气！{target.name}受到1点伤害，获得3层力量");
                }
            }
            // 水 x 土
            else if ((element1 == ElementType.Water && element2 == ElementType.Earth) ||
                     (element1 == ElementType.Earth && element2 == ElementType.Water))
            {
                if (targetIsEnemy)
                {
                    // 泥沼 - 挂上6层毒素
                    ApplyStatusEffect(target, StatusEffectType.Poison, 6);
                    Debug.Log($"触发元素反应：泥沼！{target.name}被挂上6层毒素");
                }
                else
                {
                    // 滋养 - 获得5层再生
                    ApplyStatusEffect(target, StatusEffectType.Regeneration, 5);
                    Debug.Log($"触发元素反应：滋养！{target.name}获得5层再生");
                }
            }
            // 水 x 气
            else if ((element1 == ElementType.Water && element2 == ElementType.Air) ||
                     (element1 == ElementType.Air && element2 == ElementType.Water))
            {
                if (targetIsEnemy)
                {
                    // 冰霜 - 30%概率冰冻
                    if (Random.value <= 0.3f)
                    {
                        ApplyStatusEffect(target, StatusEffectType.Frozen, 1);
                        Debug.Log($"触发元素反应：冰霜！{target.name}被冰冻");
                    }
                    else
                    {
                        Debug.Log($"触发元素反应：冰霜！冰冻判定失败");
                    }
                    // TODO: 下回合降低2点速度值（需要在行动队列中实现）
                }
                else
                {
                    // 霜甲 - 清除一种debuff并恢复4点HP
                    RemoveRandomDebuff(target);
                    target.Heal(4);
                    Debug.Log($"触发元素反应：霜甲！{target.name}清除一种debuff并恢复4点HP");
                }
            }
            // 土 x 气
            else if ((element1 == ElementType.Earth && element2 == ElementType.Air) ||
                     (element1 == ElementType.Air && element2 == ElementType.Earth))
            {
                if (targetIsEnemy)
                {
                    // 尘暴 - 挂上失准
                    ApplyStatusEffect(target, StatusEffectType.Miss, 1);
                    Debug.Log($"触发元素反应：尘暴！{target.name}被挂上失准");
                }
                else
                {
                    // 岩肤 - 获得坚固
                    ApplyStatusEffect(target, StatusEffectType.Sturdy, 1);
                    Debug.Log($"触发元素反应：岩肤！{target.name}获得坚固");
                }
            }
        }

        /// <summary>
        /// 应用元素残留
        /// </summary>
        private void ApplyElementalResidue(Character target, ElementType element)
        {
            StatusEffectType residueType = GetResidueForElement(element);
            if (residueType != StatusEffectType.FireResidue) // 检查是否有效
            {
                ApplyStatusEffect(target, residueType, 1);
            }
        }

        /// <summary>
        /// 获取元素对应的残留类型
        /// </summary>
        private StatusEffectType GetResidueForElement(ElementType element)
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
        /// 从残留类型获取元素
        /// </summary>
        private ElementType GetElementFromResidue(StatusEffectType residueType)
        {
            switch (residueType)
            {
                case StatusEffectType.FireResidue: return ElementType.Fire;
                case StatusEffectType.WaterResidue: return ElementType.Water;
                case StatusEffectType.EarthResidue: return ElementType.Earth;
                case StatusEffectType.AirResidue: return ElementType.Air;
                default: return ElementType.Physical;
            }
        }

        /// <summary>
        /// 获取目标身上的元素残留
        /// </summary>
        private StatusEffect GetExistingElementalResidue(Character target)
        {
            StatusEffect residue;
            residue = target.GetStatusEffect(StatusEffectType.FireResidue);
            if (residue != null) return residue;
            
            residue = target.GetStatusEffect(StatusEffectType.WaterResidue);
            if (residue != null) return residue;
            
            residue = target.GetStatusEffect(StatusEffectType.EarthResidue);
            if (residue != null) return residue;
            
            residue = target.GetStatusEffect(StatusEffectType.AirResidue);
            if (residue != null) return residue;

            return null;
        }

        /// <summary>
        /// 应用状态效果
        /// </summary>
        private void ApplyStatusEffect(Character target, StatusEffectType type, int stacks)
        {
            if (statusEffectDict.TryGetValue(type, out StatusEffectData data))
            {
                target.AddStatusEffect(type, stacks, data);
            }
            else
            {
                Debug.LogWarning($"Status effect data not found for type: {type}");
            }
        }

        /// <summary>
        /// 移除随机一个debuff
        /// </summary>
        private void RemoveRandomDebuff(Character target)
        {
            List<StatusEffectType> debuffs = new List<StatusEffectType>
            {
                StatusEffectType.Weak, StatusEffectType.Fragile, StatusEffectType.Poison,
                StatusEffectType.Frozen, StatusEffectType.Miss, StatusEffectType.Vulnerable, StatusEffectType.Bomb
            };

            List<StatusEffectType> existingDebuffs = new List<StatusEffectType>();
            foreach (var debuff in debuffs)
            {
                if (target.HasStatusEffect(debuff))
                {
                    existingDebuffs.Add(debuff);
                }
            }

            if (existingDebuffs.Count > 0)
            {
                StatusEffectType toRemove = existingDebuffs[Random.Range(0, existingDebuffs.Count)];
                target.RemoveStatusEffect(toRemove);
                Debug.Log($"移除了 {toRemove}");
            }
        }
    }
}

