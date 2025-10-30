using UnityEngine;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 卡牌数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "Card Battle Game/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("基础信息")]
        public int id;
        public string cardName;
        
        [Header("卡牌属性")]
        public int baseSpeed;               // 基础速度值
        public CardClass cardClass;         // 类别
        public CardType cardType;           // 种类
        public ElementType elementType;     // 属性
        
        [Header("视觉")]
        public Sprite cardArt;              // 插画
        public Sprite fullCardSprite;       // 完整卡牌sprite
        
        [Header("效果")]
        public CardEffect cardEffect;
        
        [Header("描述")]
        [TextArea(3, 6)]
        public string effectDescription;    // 效果描述
        
        [Header("动画")]
        public AnimationTrigger animationTrigger;
    }
}

