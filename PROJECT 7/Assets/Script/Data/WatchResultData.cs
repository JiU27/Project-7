using UnityEngine;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 怀表结果数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Watch Result", menuName = "Card Battle Game/Watch Result Data")]
    public class WatchResultData : ScriptableObject
    {
        [Header("基础信息")]
        public WatchResult resultType;
        
        [Header("视觉")]
        public Sprite watchSprite;          // 怀表sprite
        public Sprite numberSprite;         // 数字sprite
        public string animationTrigger;     // 动画触发器名称
        
        [Header("效果参数")]
        public int temporaryCardId;         // 临时卡牌id（结果5使用）
        
        [Header("描述")]
        [TextArea(2, 4)]
        public string effectDescription;    // 效果描述
    }
}

