using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Entities;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 手牌UI管理器 - 负责实例化和管理手牌的UI显示
    /// </summary>
    public class HandUIManager : MonoBehaviour
    {
        [Header("卡牌预制体")]
        public GameObject cardPrefab;           // 卡牌UI预制体

        [Header("手牌显示区域")]
        public Transform inherentCardParent;    // 固有卡牌槽位父对象
        public Transform normalCardParent;      // 普通手牌槽位父对象

        [Header("引用")]
        public UIManager uiManager;
        public BattleManager battleManager;

        private List<CardUI> currentHandUI = new List<CardUI>();

        /// <summary>
        /// 显示手牌
        /// </summary>
        public void DisplayHand(List<Card> hand)
        {
            // 清除现有手牌UI
            ClearHandUI();

            if (hand == null || hand.Count == 0)
            {
                Debug.LogWarning("手牌为空，无法显示");
                return;
            }

            // 为每张卡创建UI
            foreach (var card in hand)
            {
                CreateCardUI(card);
            }

            Debug.Log($"手牌UI已显示：{hand.Count} 张卡牌");
        }

        /// <summary>
        /// 创建单张卡牌的UI
        /// </summary>
        private CardUI CreateCardUI(Card card)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("Card Prefab 未设置！请在 HandUIManager 中分配卡牌预制体。");
                return null;
            }

            // 确定父对象（固有卡牌或普通卡牌）
            Transform parent = card.data.cardEffect.isInherent ? inherentCardParent : normalCardParent;
            
            if (parent == null)
            {
                Debug.LogError("手牌父对象未设置！");
                parent = transform; // 使用自身作为默认父对象
            }

            // 实例化卡牌UI
            GameObject cardObj = Instantiate(cardPrefab, parent);
            CardUI cardUI = cardObj.GetComponent<CardUI>();

            if (cardUI == null)
            {
                Debug.LogError("Card Prefab 上没有 CardUI 组件！");
                Destroy(cardObj);
                return null;
            }

            // 检查引用
            if (uiManager == null)
            {
                Debug.LogWarning("UIManager 未设置，CardUI 功能可能不完整");
            }
            if (battleManager == null)
            {
                Debug.LogWarning("BattleManager 未设置，CardUI 无法拖拽到槽位");
            }

            // 初始化卡牌UI
            cardUI.Initialize(card, uiManager, battleManager);

            // 添加到列表
            currentHandUI.Add(cardUI);

            Debug.Log($"创建了卡牌UI：{card.data.cardName}");

            return cardUI;
        }

        /// <summary>
        /// 清除所有手牌UI
        /// </summary>
        public void ClearHandUI()
        {
            foreach (var cardUI in currentHandUI)
            {
                if (cardUI != null)
                {
                    Destroy(cardUI.gameObject);
                }
            }
            currentHandUI.Clear();
        }

        /// <summary>
        /// 移除指定卡牌的UI
        /// </summary>
        public void RemoveCardUI(Card card)
        {
            CardUI uiToRemove = currentHandUI.Find(ui => ui.GetCard() == card);
            if (uiToRemove != null)
            {
                currentHandUI.Remove(uiToRemove);
                Destroy(uiToRemove.gameObject);
            }
        }

        /// <summary>
        /// 添加单张卡牌UI（用于抽牌）
        /// </summary>
        public void AddCardUI(Card card)
        {
            CreateCardUI(card);
        }
    }
}

