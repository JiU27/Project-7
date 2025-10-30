using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Data;

namespace CardBattleGame.Entities
{
    /// <summary>
    /// 角色/敌人基类
    /// </summary>
    public abstract class Character : MonoBehaviour
    {
        [Header("数据引用")]
        public int characterId;
        
        [Header("当前状态")]
        public int currentHealth;
        public int maxHealth;
        public int currentArmor;
        
        [Header("状态效果")]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        [Header("组件引用")]
        public Animator animator;
        public CharacterUI characterUI;
        
        protected AudioData audioData;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(int maxHp, int initialArmor, AudioData audio)
        {
            maxHealth = maxHp;
            currentHealth = maxHp;
            currentArmor = initialArmor;
            audioData = audio;
            
            // 初始化后立即更新UI
            if (characterUI != null)
            {
                characterUI.UpdateHealthBar(currentHealth, maxHealth);
                characterUI.UpdateArmorBar(currentArmor);
                characterUI.UpdateStatusEffects(statusEffects);
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage, ElementType elementType = ElementType.Physical)
        {
            if (damage <= 0) return;

            // 计算状态效果的影响
            damage = CalculateDamageWithStatusEffects(damage, elementType);

            int totalDamage = damage;
            
            // 先扣除护甲
            if (currentArmor > 0)
            {
                int armorDamage = Mathf.Min(currentArmor, damage);
                currentArmor -= armorDamage;
                damage -= armorDamage;
                
                PlaySound(audioData.takeDamageWithArmor);
                Debug.Log($"{name} 护甲吸收了 {armorDamage} 点伤害，剩余护甲：{currentArmor}");
            }

            // 再扣除生命值
            if (damage > 0)
            {
                currentHealth = Mathf.Max(0, currentHealth - damage);
                PlaySound(audioData.takeDamageNoArmor);
                Debug.Log($"{name} 受到了 {damage} 点生命伤害，剩余生命：{currentHealth}/{maxHealth}");
            }

            // 播放受伤动画
            PlayAnimation(AnimationTrigger.TakeDamage);

            // 立即更新UI
            if (characterUI != null)
            {
                characterUI.UpdateHealthBar(currentHealth, maxHealth);
                characterUI.UpdateArmorBar(currentArmor);
                characterUI.ShowDamageNumber(totalDamage);
            }
            else
            {
                Debug.LogWarning($"{name} 的 CharacterUI 为空，无法更新显示");
            }

            // 脆弱效果在受到伤害后消耗（文档：受到的下一次攻击）
            StatusEffect vulnerableEffect = GetStatusEffect(StatusEffectType.Vulnerable);
            if (vulnerableEffect != null && totalDamage > 0)
            {
                RemoveStatusEffect(StatusEffectType.Vulnerable);
                Debug.Log($"{name} 的脆弱效果已消耗");
            }

            // 检查死亡
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 恢复生命
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (amount <= 0) return;

            // 毒素效果影响恢复
            StatusEffect poisonEffect = GetStatusEffect(StatusEffectType.Poison);
            if (poisonEffect != null)
            {
                amount = Mathf.Max(1, Mathf.FloorToInt(amount * 0.75f));
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            PlaySound(audioData.heal);
            characterUI?.UpdateHealthBar(currentHealth, maxHealth);
            characterUI?.ShowHealNumber(amount);
        }

        /// <summary>
        /// 获得护甲
        /// </summary>
        public virtual void GainArmor(int amount)
        {
            if (amount <= 0) return;

            // 计算铸甲和碎甲效果
            StatusEffect fortifyEffect = GetStatusEffect(StatusEffectType.Fortify);
            StatusEffect fragileEffect = GetStatusEffect(StatusEffectType.Fragile);

            if (fortifyEffect != null)
            {
                amount = Mathf.CeilToInt(amount * 1.25f * fortifyEffect.stacks);
            }

            if (fragileEffect != null)
            {
                amount = Mathf.Max(1, Mathf.FloorToInt(amount * 0.75f));
            }

            currentArmor += amount;
            
            PlaySound(audioData.gainArmor);
            characterUI?.UpdateArmorBar(currentArmor);
            characterUI?.ShowArmorNumber(amount);
        }

        /// <summary>
        /// 添加状态效果
        /// </summary>
        public virtual void AddStatusEffect(StatusEffectType type, int stacks, StatusEffectData data)
        {
            // 坚固状态下无法挂上元素残留
            if (HasStatusEffect(StatusEffectType.Sturdy))
            {
                if (type == StatusEffectType.FireResidue || type == StatusEffectType.WaterResidue ||
                    type == StatusEffectType.EarthResidue || type == StatusEffectType.AirResidue)
                {
                    Debug.Log($"{name} 拥有坚固状态，无法挂上元素残留");
                    return;
                }
            }

            StatusEffect existing = GetStatusEffect(type);
            if (existing != null)
            {
                existing.AddStacks(stacks);
                Debug.Log($"{name} 的 {type} 效果层数增加到 {existing.stacks}");
            }
            else
            {
                statusEffects.Add(new StatusEffect(data, stacks));
                Debug.Log($"{name} 获得了 {type} 效果，层数：{stacks}");
            }

            // 立即更新UI
            if (characterUI != null)
            {
                characterUI.UpdateStatusEffects(statusEffects);
            }
            else
            {
                Debug.LogWarning($"{name} 的 CharacterUI 为空，无法更新状态效果显示");
            }
        }

        /// <summary>
        /// 移除状态效果
        /// </summary>
        public virtual void RemoveStatusEffect(StatusEffectType type)
        {
            statusEffects.RemoveAll(s => s.data.statusType == type);
            characterUI?.UpdateStatusEffects(statusEffects);
        }

        /// <summary>
        /// 获取状态效果
        /// </summary>
        public StatusEffect GetStatusEffect(StatusEffectType type)
        {
            return statusEffects.FirstOrDefault(s => s.data.statusType == type);
        }

        /// <summary>
        /// 是否拥有状态效果
        /// </summary>
        public bool HasStatusEffect(StatusEffectType type)
        {
            return GetStatusEffect(type) != null;
        }

        /// <summary>
        /// 计算状态效果对伤害的影响
        /// </summary>
        protected int CalculateDamageWithStatusEffects(int baseDamage, ElementType elementType)
        {
            float damage = baseDamage;

            // 坚固效果 - 受到的伤害减少25%，向下取整（让防御更有效），但至少受到1点
            StatusEffect sturdyEffect = GetStatusEffect(StatusEffectType.Sturdy);
            if (sturdyEffect != null)
            {
                float reducedDamage = damage * 0.75f;
                damage = Mathf.Max(1, Mathf.FloorToInt(reducedDamage));
                Debug.Log($"{name} 的坚固效果减少了伤害（原{baseDamage} → 现{damage}）");
            }

            // 脆弱效果 - 受到的下一次攻击伤害提升
            StatusEffect vulnerableEffect = GetStatusEffect(StatusEffectType.Vulnerable);
            if (vulnerableEffect != null)
            {
                if (elementType == ElementType.Physical)
                {
                    // 物理攻击伤害提升100%
                    damage = Mathf.Ceil(damage * 2f);
                    Debug.Log($"{name} 的脆弱效果使物理伤害翻倍");
                }
                else
                {
                    // 其他攻击伤害提升50%
                    damage = Mathf.Ceil(damage * 1.5f);
                    Debug.Log($"{name} 的脆弱效果使伤害提升50%");
                }
                
                // 脆弱效果在受到攻击后消耗（文档：受到的下一次攻击）
                // 注意：这里不直接移除，而是在TakeDamage结束后移除，以确保UI更新
            }

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        public void PlayAnimation(AnimationTrigger trigger)
        {
            if (animator != null)
            {
                animator.SetTrigger(trigger.ToString());
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        protected void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        protected virtual void Die()
        {
            PlayAnimation(AnimationTrigger.Death);
            // 由BattleManager处理死亡逻辑
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }
}

