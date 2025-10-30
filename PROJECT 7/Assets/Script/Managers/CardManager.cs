using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleGame.Entities;
using CardBattleGame.Data;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 卡牌管理器 - 管理战斗中的卡牌（手牌、牌库、弃牌堆）
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("卡牌区域")]
        public List<Card> drawPile = new List<Card>();      // 抽牌堆
        public List<Card> hand = new List<Card>();          // 手牌
        public List<Card> discardPile = new List<Card>();   // 弃牌堆
        public List<Card> inherentCards = new List<Card>(); // 固有卡牌

        private DeckManager deckManager;
        private System.Random random = new System.Random();

        public void Initialize(DeckManager deck)
        {
            deckManager = deck;
        }

        /// <summary>
        /// 初始化战斗开始时的牌库
        /// </summary>
        public void InitializeBattleDeck(List<Card> cards)
        {
            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();
            inherentCards.Clear();

            // 分离固有卡牌和普通卡牌
            foreach (var card in cards)
            {
                if (card.data.cardEffect.isInherent)
                {
                    inherentCards.Add(card);
                }
                else
                {
                    drawPile.Add(card);
                }
            }

            // 洗牌
            ShuffleDrawPile();

            Debug.Log($"战斗牌库初始化：抽牌堆 {drawPile.Count} 张，固有卡牌 {inherentCards.Count} 张");
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        public void ShuffleDrawPile()
        {
            drawPile = drawPile.OrderBy(x => random.Next()).ToList();
        }

        /// <summary>
        /// 抽卡
        /// </summary>
        public Card DrawCard()
        {
            if (drawPile.Count == 0)
            {
                // 如果抽牌堆为空，将弃牌堆洗入抽牌堆
                if (discardPile.Count > 0)
                {
                    Debug.Log("抽牌堆为空，将弃牌堆洗入抽牌堆");
                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    ShuffleDrawPile();
                }
                else
                {
                    Debug.LogWarning("无牌可抽");
                    return null;
                }
            }

            Card card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
            return card;
        }

        /// <summary>
        /// 抽取指定数量的卡牌
        /// </summary>
        public List<Card> DrawCards(int count)
        {
            List<Card> drawnCards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                Card card = DrawCard();
                if (card != null)
                {
                    drawnCards.Add(card);
                }
            }
            return drawnCards;
        }

        /// <summary>
        /// 获取初始手牌（固有卡牌 + 抽取的卡牌）
        /// </summary>
        public List<Card> GetInitialHand(int normalCardCount = 3)
        {
            hand.Clear();

            // 添加所有固有卡牌
            foreach (var inherentCard in inherentCards.OrderBy(c => c.data.id))
            {
                hand.Add(inherentCard);
            }

            // 抽取普通卡牌
            DrawCards(normalCardCount);

            Debug.Log($"获得初始手牌：固有 {inherentCards.Count} 张，抽取 {normalCardCount} 张");
            return new List<Card>(hand);
        }

        /// <summary>
        /// 打出卡牌（放入弃牌堆）
        /// </summary>
        public void PlayCard(Card card)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
            }

            // 固有卡牌不进入弃牌堆，直接回到固有卡牌列表
            if (card.data.cardEffect.isInherent)
            {
                // 固有卡牌保留，不做处理
            }
            else
            {
                discardPile.Add(card);
            }

            card.ResetModifiers();
        }

        /// <summary>
        /// 弃掉所有非固有手牌
        /// </summary>
        public void DiscardNonInherentCards()
        {
            List<Card> cardsToDiscard = hand.Where(c => !c.data.cardEffect.isInherent).ToList();
            
            foreach (var card in cardsToDiscard)
            {
                hand.Remove(card);
                discardPile.Add(card);
                card.ResetModifiers();
            }

            Debug.Log($"弃掉 {cardsToDiscard.Count} 张非固有卡牌");
        }

        /// <summary>
        /// 丢弃指定卡牌
        /// </summary>
        public void DiscardCard(Card card)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                
                if (!card.data.cardEffect.isInherent)
                {
                    discardPile.Add(card);
                }
                
                card.ResetModifiers();
            }
        }

        /// <summary>
        /// 添加临时卡牌（怀表效果5）
        /// </summary>
        public Card AddTemporaryCard(int cardId)
        {
            Card tempCard = deckManager.CreateCard(cardId);
            if (tempCard != null)
            {
                hand.Add(tempCard);
                Debug.Log($"获得临时卡牌：{tempCard.data.cardName}");
                return tempCard;
            }
            return null;
        }

        /// <summary>
        /// 移除临时卡牌
        /// </summary>
        public void RemoveTemporaryCard(Card card)
        {
            hand.Remove(card);
            drawPile.Remove(card);
            discardPile.Remove(card);
            Debug.Log($"移除临时卡牌：{card.data.cardName}");
        }

        /// <summary>
        /// 获取手牌数量
        /// </summary>
        public int GetHandCount()
        {
            return hand.Count;
        }

        /// <summary>
        /// 获取抽牌堆数量
        /// </summary>
        public int GetDrawPileCount()
        {
            return drawPile.Count;
        }

        /// <summary>
        /// 获取弃牌堆数量
        /// </summary>
        public int GetDiscardPileCount()
        {
            return discardPile.Count;
        }
    }
}

