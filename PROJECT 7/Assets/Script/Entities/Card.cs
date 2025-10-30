using UnityEngine;
using CardBattleGame.Data;

namespace CardBattleGame.Entities
{
    /// <summary>
    /// 卡牌实体类
    /// </summary>
    public class Card
    {
        public CardData data { get; private set; }
        public int currentSpeed { get; private set; }
        public int modifiedDamage { get; private set; }
        public int modifiedHeal { get; private set; }
        public int modifiedArmor { get; private set; }
        public int modifiedStatusStacks { get; private set; }
        
        public Character targetEnemy { get; set; }  // 目标敌人
        public bool isInOutputSlot { get; set; }    // 是否在输出槽位中
        public int outputSlotIndex { get; set; }    // 在哪个输出槽位

        public Card(CardData cardData)
        {
            data = cardData;
            ResetModifiers();
        }

        /// <summary>
        /// 重置修正值为基础值
        /// </summary>
        public void ResetModifiers()
        {
            currentSpeed = data.baseSpeed;
            modifiedDamage = data.cardEffect.damageAmount;
            modifiedHeal = data.cardEffect.healAmount;
            modifiedArmor = data.cardEffect.armorAmount;
            modifiedStatusStacks = data.cardEffect.statusStacks;
            isInOutputSlot = false;
            outputSlotIndex = -1;
        }

        /// <summary>
        /// 应用槽位效果
        /// </summary>
        public void ApplySlotEffect(SlotEffect slotEffect)
        {
            currentSpeed += slotEffect.speedModifier;
            
            modifiedDamage = Mathf.Max(1, Mathf.RoundToInt(data.cardEffect.damageAmount * slotEffect.effectMultiplier));
            modifiedHeal = Mathf.Max(1, Mathf.RoundToInt(data.cardEffect.healAmount * slotEffect.effectMultiplier));
            modifiedArmor = Mathf.Max(1, Mathf.RoundToInt(data.cardEffect.armorAmount * slotEffect.effectMultiplier));
            modifiedStatusStacks = Mathf.Max(1, Mathf.RoundToInt(data.cardEffect.statusStacks * slotEffect.effectMultiplier));
        }

        /// <summary>
        /// 应用"无"类型加成（怀表效果4）
        /// </summary>
        public void ApplyNoneTypeBoost()
        {
            if (data.cardType != CardType.None) return;

            modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.25f);
            modifiedHeal = Mathf.CeilToInt(modifiedHeal * 1.25f);
            modifiedArmor = Mathf.CeilToInt(modifiedArmor * 1.25f);
            modifiedStatusStacks = Mathf.CeilToInt(modifiedStatusStacks * 1.25f);
        }
    }
}

