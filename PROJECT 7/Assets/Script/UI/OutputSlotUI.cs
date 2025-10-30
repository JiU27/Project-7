using UnityEngine;
using TMPro;
using CardBattleGame.Managers;

namespace CardBattleGame
{
    /// <summary>
    /// 输出槽位UI组件
    /// </summary>
    public class OutputSlotUI : MonoBehaviour
    {
        [Header("槽位信息")]
        public int slotIndex;

        [Header("视觉组件")]
        public TextMeshProUGUI effectDescriptionText;
        public TextMeshProUGUI speedText;

        private CardUI currentCard;
        private BattleManager battleManager;
        private UIManager uiManager;

        public void Initialize(int index, string effectDescription, BattleManager battle, UIManager ui)
        {
            slotIndex = index;
            battleManager = battle;
            uiManager = ui;

            if (effectDescriptionText != null)
            {
                effectDescriptionText.text = effectDescription;
            }

            ClearSlot();
        }

        /// <summary>
        /// 放置卡牌
        /// </summary>
        public void PlaceCard(CardUI cardUI)
        {
            currentCard = cardUI;
            
            if (speedText != null)
            {
                speedText.text = cardUI.GetCard().currentSpeed.ToString();
            }
        }

        /// <summary>
        /// 清空槽位
        /// </summary>
        public void ClearSlot()
        {
            currentCard = null;
            
            if (speedText != null)
            {
                speedText.text = "";
            }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty()
        {
            return currentCard == null;
        }

        /// <summary>
        /// 移除卡牌（Revert按钮）
        /// </summary>
        public CardUI RemoveCard()
        {
            CardUI removedCard = currentCard;
            
            if (removedCard != null)
            {
                // 从BattleManager移除
                battleManager.RemoveCardFromSlot(slotIndex);
                
                // 更新UI
                if (uiManager != null)
                {
                    uiManager.ClearSlotSpeed(slotIndex);
                }
                
                ClearSlot();
            }

            return removedCard;
        }

        public CardUI GetCurrentCard()
        {
            return currentCard;
        }
    }
}

