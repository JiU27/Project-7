using UnityEngine;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 输出卡牌系统数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Output System", menuName = "Card Battle Game/Output System Data")]
    public class OutputSystemData : ScriptableObject
    {
        [Header("基础信息")]
        public int id;
        public string systemName;
        
        [Header("视觉")]
        public Sprite systemSprite;
        
        [Header("槽位配置")]
        public int slotCount = 3;           // 槽位数量
        public SlotEffect[] slotEffects;    // 每个槽位的效果
    }
}

