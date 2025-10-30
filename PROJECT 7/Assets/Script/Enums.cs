using System;

namespace CardBattleGame
{
    /// <summary>
    /// 卡牌类别 - 攻击/技能/能力
    /// </summary>
    public enum CardClass
    {
        Attack,     // 攻击
        Skill,      // 技能
        Ability     // 能力
    }

    /// <summary>
    /// 卡牌种类 - 迅捷/坚毅/中庸/无
    /// </summary>
    public enum CardType
    {
        Swift,      // 迅捷
        Strong,     // 坚毅
        Normal,     // 中庸
        None        // 无
    }

    /// <summary>
    /// 属性类型
    /// </summary>
    public enum ElementType
    {
        Physical,   // 物理
        Fire,       // 火
        Water,      // 水
        Earth,      // 土
        Air,        // 气
        Chaos       // 混沌
    }

    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        None = 0,           // 无状态效果
        
        // 元素残留
        FireResidue,        // 火元素残留
        WaterResidue,       // 水元素残留
        EarthResidue,       // 土元素残留
        AirResidue,         // 气元素残留
        
        // 增益状态
        Strength,           // 力量
        TempStrength,       // 临时力量
        Fortify,            // 铸甲
        Regeneration,       // 再生
        Sturdy,             // 坚固
        Stealth,            // 隐匿
        
        // 减益状态
        Weak,               // 虚弱
        Fragile,            // 碎甲
        Poison,             // 毒素
        Frozen,             // 冰冻
        Miss,               // 失准
        Vulnerable,         // 脆弱
        Bomb                // 炸弹
    }

    /// <summary>
    /// 战斗阶段
    /// </summary>
    public enum BattlePhase
    {
        Preparation,        // 准备阶段（怀表）
        Planning,           // 玩家规划阶段
        Combat,             // 战斗阶段
        Resolution,         // 结算阶段
        Victory,            // 胜利
        Defeat              // 失败
    }

    /// <summary>
    /// 动画触发器类型
    /// </summary>
    public enum AnimationTrigger
    {
        Idle,               // 待机
        MeleeAttack,        // 近战攻击
        RangedAttack,       // 远程攻击/释放法术
        TakeDamage,         // 受伤
        Death               // 死亡
    }

    /// <summary>
    /// 怀表结果类型（1-6）
    /// </summary>
    public enum WatchResult
    {
        DrawCard = 1,           // 额外抽取一张卡牌
        ApplyRandomElement = 2, // 为敌人挂上随机元素
        DealDamage = 3,         // 对所有敌人造成2点随机元素伤害
        BoostNoneType = 4,      // "无"类型卡牌效果提升25%
        GrantTemporaryCard = 5, // 获得一张临时卡牌
        GrantRandomBuff = 6     // 获得随机增益状态
    }
}

