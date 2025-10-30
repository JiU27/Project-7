using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardBattleGame.Entities;
using CardBattleGame.Managers;

namespace CardBattleGame
{
    /// <summary>
    /// 卡牌UI组件 - 处理卡牌的显示和交互
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("视觉组件")]
        public Image cardImage;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI descriptionText;

        [Header("交互")]
        public bool isInteractable = true;
        public float hoverOffsetY = 20f;

        private Card card;
        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;
        private Canvas canvas;
        private UIManager uiManager;
        private BattleManager battleManager;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// 初始化卡牌UI
        /// </summary>
        public void Initialize(Card cardData, UIManager ui, BattleManager battle)
        {
            card = cardData;
            uiManager = ui;
            battleManager = battle;
            
            UpdateCardDisplay();
        }

        /// <summary>
        /// 更新卡牌显示
        /// </summary>
        public void UpdateCardDisplay()
        {
            if (card == null) return;

            if (cardImage != null && card.data.fullCardSprite != null)
            {
                cardImage.sprite = card.data.fullCardSprite;
            }

            if (speedText != null)
            {
                speedText.text = card.currentSpeed.ToString();
            }

            if (descriptionText != null)
            {
                descriptionText.text = card.data.effectDescription;
            }
        }

        /// <summary>
        /// 鼠标进入（悬停效果）
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;

            transform.localPosition += new Vector3(0, hoverOffsetY, 0);
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;

            transform.localPosition -= new Vector3(0, hoverOffsetY, 0);
        }

        /// <summary>
        /// 鼠标点击（检视卡牌）
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || isDragging) return;

            // 显示卡牌检视面板
            if (uiManager != null && card != null)
            {
                int displaySpeed = card.isInOutputSlot ? card.currentSpeed : card.data.baseSpeed;
                uiManager.ShowCardInspectPanel(
                    card.data.fullCardSprite,
                    card.data.cardName,
                    card.data.effectDescription,
                    displaySpeed
                );
            }
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isDragging = true;
            originalPosition = transform.position;
            originalParent = transform.parent;

            // 将卡牌移到Canvas的顶层
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();

            // 禁用Raycast以便检测下方的槽位
            if (cardImage != null)
            {
                cardImage.raycastTarget = false;
            }
        }

        /// <summary>
        /// 拖拽中
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!isInteractable || !isDragging) return;

            // 跟随鼠标
            transform.position = eventData.position;
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isDragging = false;

            // 恢复Raycast
            if (cardImage != null)
            {
                cardImage.raycastTarget = true;
            }

            // 检测是否拖到了输出槽位
            OutputSlotUI slot = GetSlotUnderCursor(eventData);
            
            if (slot != null && slot.IsEmpty())
            {
                // 放入槽位
                bool success = battleManager.PlaceCardInSlot(card, slot.slotIndex);
                if (success)
                {
                    slot.PlaceCard(this);
                    transform.SetParent(slot.transform);
                    transform.localPosition = Vector3.zero;
                    
                    // 更新UI显示卡牌速度
                    if (uiManager != null)
                    {
                        uiManager.UpdateSlotSpeed(slot.slotIndex, card.currentSpeed);
                    }
                    
                    UpdateCardDisplay(); // 更新卡牌显示（速度可能已改变）
                    return;
                }
            }

            // 如果没有放入槽位，返回原位置
            ReturnToOriginalPosition();
        }

        /// <summary>
        /// 返回原位置
        /// </summary>
        public void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }

        /// <summary>
        /// 获取鼠标下的槽位
        /// </summary>
        private OutputSlotUI GetSlotUnderCursor(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                OutputSlotUI slot = result.gameObject.GetComponent<OutputSlotUI>();
                if (slot != null)
                {
                    return slot;
                }
            }

            return null;
        }

        public Card GetCard()
        {
            return card;
        }
    }
}

