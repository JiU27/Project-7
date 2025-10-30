using System;
using UnityEngine;

namespace CardBattleGame
{
    /// <summary>
    /// 卡牌效果数据结构
    /// </summary>
    [Serializable]
    public struct CardEffect
    {
        public bool isInherent;             // 是否为基础固有卡牌
        public bool dealsDamage;            // 是否造成伤害
        public int damageAmount;            // 伤害数值
        public bool isAoE;                  // 是否为群体伤害
        public bool healsHealth;            // 是否恢复生命
        public int healAmount;              // 恢复生命数值
        public bool grantsArmor;            // 是否获得护甲
        public int armorAmount;             // 护甲数值
        public bool grantsStatusEffect;     // 是否获得状态效果
        public StatusEffectType statusType; // 状态效果类型
        public int statusStacks;            // 状态层数
        public bool isInstantCast;          // 是否可以直接释放（不进入战斗阶段）
    }

    /// <summary>
    /// 敌人技能数据结构
    /// </summary>
    [Serializable]
    public struct EnemySkill
    {
        public int cardId;                  // 对应卡牌id（用于显示技能信息）
        public int speedValue;              // 速度值
        public CardClass cardClass;         // 类型
        public CardType cardType;           // 种类
        public ElementType elementType;     // 属性
        public bool dealsDamage;            // 是否造成伤害
        public int damageAmount;            // 伤害数值
        public bool healsHealth;            // 是否恢复生命
        public int healAmount;              // 恢复生命数值
        public bool grantsArmor;            // 是否获得护甲
        public int armorAmount;             // 护甲数值
        public bool grantsStatusEffect;     // 是否获得状态效果
        public StatusEffectType statusType; // 状态效果类型
        public int statusStacks;            // 状态层数
    }

    /// <summary>
    /// 敌人行动模式数据结构
    /// </summary>
    [Serializable]
    public struct EnemyActionPattern
    {
        public bool hasFirstTurnSpecial;    // 第一回合是否释放特殊技能
        public int firstTurnSkillIndex;     // 第一回合释放的技能索引
        public int actionsPerTurn;          // 一回合行动次数
        public bool randomSkillOrder;       // 是否随机释放技能（false则按顺序循环）
    }

    /// <summary>
    /// 音效数据结构
    /// </summary>
    [Serializable]
    public struct AudioData
    {
        public AudioClip takeDamageNoArmor;     // 无护盾时受伤
        public AudioClip takeDamageWithArmor;   // 有护盾时受伤
        public AudioClip attack;                // 攻击
        public AudioClip heal;                  // 恢复生命
        public AudioClip gainArmor;             // 获得护盾
    }

    /// <summary>
    /// 输出槽位效果数据结构
    /// </summary>
    [Serializable]
    public struct SlotEffect
    {
        public int speedModifier;           // 速度值修正
        public float effectMultiplier;      // 效果倍率
        [TextArea(2, 4)]
        public string effectDescription;    // 效果描述
    }

    /// <summary>
    /// 属性反应效果数据结构
    /// </summary>
    [Serializable]
    public struct ElementalReactionEffect
    {
        public ElementType element1;            // 元素1
        public ElementType element2;            // 元素2
        public bool targetEnemy;                // 是否针对敌人
        public string reactionName;             // 反应名称
        [TextArea(2, 4)]
        public string description;              // 描述
        
        // 效果参数
        public float damageMultiplier;          // 伤害倍率
        public StatusEffectType grantedStatus;  // 赋予的状态效果
        public int statusStacks;                // 状态层数
        public int directDamage;                // 直接伤害
        public int armorGain;                   // 护甲获得
        public int healthGain;                  // 生命恢复
        public StatusEffectType removedStatus;  // 移除的状态
        public int speedReduction;              // 速度降低
        public float freezeChance;              // 冰冻概率
    }
}

