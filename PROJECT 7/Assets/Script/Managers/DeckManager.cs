using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Data;
using CardBattleGame.Entities;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 卡组管理器
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [Header("卡牌数据库")]
        public List<CardData> cardDatabase;

        [Header("玩家卡组")]
        public List<int> playerDeckCardIds = new List<int>(); // 玩家的卡组（存储卡牌id）

        private Dictionary<int, CardData> cardDataDict;

        private void Awake()
        {
            InitializeCardDatabase();
        }

        /// <summary>
        /// 初始化卡牌数据库
        /// </summary>
        private void InitializeCardDatabase()
        {
            cardDataDict = new Dictionary<int, CardData>();
            
            if (cardDatabase == null || cardDatabase.Count == 0)
            {
                Debug.LogError("Card Database为空！请在DeckManager中添加CardData资源。");
                return;
            }

            for (int i = 0; i < cardDatabase.Count; i++)
            {
                var cardData = cardDatabase[i];
                
                if (cardData == null)
                {
                    Debug.LogError($"Card Database中的Element {i}为空（null）！请删除空元素或分配CardData资源。");
                    continue;
                }

                if (!cardDataDict.ContainsKey(cardData.id))
                {
                    cardDataDict.Add(cardData.id, cardData);
                    Debug.Log($"卡牌已加载：{cardData.cardName} (ID: {cardData.id})");
                }
                else
                {
                    Debug.LogWarning($"重复的卡牌ID：{cardData.id}，卡牌 {cardData.cardName} 未添加到数据库。");
                }
            }

            Debug.Log($"===== 卡牌数据库初始化完成，共 {cardDataDict.Count} 张卡牌 =====");
        }

        /// <summary>
        /// 根据id获取CardData
        /// </summary>
        public CardData GetCardData(int cardId)
        {
            // 确保字典已初始化
            if (cardDataDict == null)
            {
                Debug.LogError("卡牌数据库字典未初始化！正在尝试初始化...");
                InitializeCardDatabase();
            }

            if (cardDataDict == null)
            {
                Debug.LogError("卡牌数据库初始化失败！请检查Card Database是否已配置。");
                return null;
            }

            if (cardDataDict.TryGetValue(cardId, out CardData data))
            {
                return data;
            }
            
            Debug.LogWarning($"Card data not found for id: {cardId}");
            return null;
        }

        /// <summary>
        /// 添加卡牌到卡组
        /// </summary>
        public void AddCardToDeck(int cardId)
        {
            if (cardDataDict.ContainsKey(cardId))
            {
                playerDeckCardIds.Add(cardId);
                Debug.Log($"添加卡牌 {cardId} 到卡组");
            }
            else
            {
                Debug.LogWarning($"无法添加卡牌，id {cardId} 不存在");
            }
        }

        /// <summary>
        /// 从卡组移除卡牌
        /// </summary>
        public void RemoveCardFromDeck(int cardId)
        {
            if (playerDeckCardIds.Contains(cardId))
            {
                playerDeckCardIds.Remove(cardId);
                Debug.Log($"从卡组移除卡牌 {cardId}");
            }
        }

        /// <summary>
        /// 获取卡组中的所有卡牌数据
        /// </summary>
        public List<CardData> GetDeckCards()
        {
            List<CardData> deckCards = new List<CardData>();
            
            if (playerDeckCardIds == null || playerDeckCardIds.Count == 0)
            {
                Debug.LogWarning("玩家卡组ID列表为空！");
                return deckCards;
            }

            foreach (int cardId in playerDeckCardIds)
            {
                CardData data = GetCardData(cardId);
                if (data != null)
                {
                    deckCards.Add(data);
                }
                else
                {
                    Debug.LogWarning($"卡牌ID {cardId} 未找到对应的CardData，已跳过。");
                }
            }
            return deckCards;
        }

        /// <summary>
        /// 创建卡牌实体
        /// </summary>
        public Card CreateCard(int cardId)
        {
            CardData data = GetCardData(cardId);
            if (data != null)
            {
                return new Card(data);
            }
            return null;
        }

        /// <summary>
        /// 创建卡牌实体列表
        /// </summary>
        public List<Card> CreateCardsFromDeck()
        {
            List<Card> cards = new List<Card>();
            foreach (int cardId in playerDeckCardIds)
            {
                Card card = CreateCard(cardId);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }

        /// <summary>
        /// 设置卡组（用于外部系统设置玩家的卡组）
        /// </summary>
        public void SetDeck(List<int> cardIds)
        {
            playerDeckCardIds = new List<int>(cardIds);
            Debug.Log($"设置卡组，共 {playerDeckCardIds.Count} 张卡牌");
        }

        /// <summary>
        /// 清空卡组
        /// </summary>
        public void ClearDeck()
        {
            playerDeckCardIds.Clear();
        }
    }
}

