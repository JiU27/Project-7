using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Managers;

namespace CardBattleGame.Examples
{
    /// <summary>
    /// 示例：初始化玩家卡组
    /// 这个脚本展示了如何设置玩家的卡组
    /// </summary>
    public class ExampleDeckSetup : MonoBehaviour
    {
        [Header("卡组管理器")]
        public DeckManager deckManager;

        [Header("示例卡组（输入卡牌ID）")]
        public List<int> exampleDeckCardIds = new List<int>
        {
            1, 1, 1,  // 3张基础攻击（假设id=1）
            2, 2,     // 2张基础防御（假设id=2）
            3, 3, 3,  // 3张固有卡牌（假设id=3）
            4, 5, 6   // 其他技能卡牌
        };

        private void Start()
        {
            SetupDeck();
        }

        /// <summary>
        /// 设置卡组
        /// </summary>
        public void SetupDeck()
        {
            if (deckManager != null)
            {
                deckManager.SetDeck(exampleDeckCardIds);
                Debug.Log($"示例卡组已设置，共 {exampleDeckCardIds.Count} 张卡牌");
            }
            else
            {
                Debug.LogError("DeckManager 未分配！");
            }
        }

        /// <summary>
        /// 添加卡牌到卡组
        /// </summary>
        public void AddCardToDeck(int cardId)
        {
            if (deckManager != null)
            {
                deckManager.AddCardToDeck(cardId);
                Debug.Log($"添加卡牌 {cardId} 到卡组");
            }
        }

        /// <summary>
        /// 从卡组移除卡牌
        /// </summary>
        public void RemoveCardFromDeck(int cardId)
        {
            if (deckManager != null)
            {
                deckManager.RemoveCardFromDeck(cardId);
                Debug.Log($"从卡组移除卡牌 {cardId}");
            }
        }
    }
}

