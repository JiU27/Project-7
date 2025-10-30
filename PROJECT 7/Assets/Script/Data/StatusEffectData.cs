using UnityEngine;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 状态效果数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Status Effect", menuName = "Card Battle Game/Status Effect Data")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("基础信息")]
        public StatusEffectType statusType;
        public string statusName;
        
        [Header("视觉")]
        public Sprite iconSprite;
        
        [Header("属性")]
        public bool canStack = true;        // 是否可以叠加层数
        public int maxStacks = 99;          // 最大层数
        
        [Header("描述")]
        [TextArea(3, 6)]
        public string effectDescription;    // 效果描述
    }
}

