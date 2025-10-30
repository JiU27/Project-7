using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardBattleGame.Entities;

namespace CardBattleGame
{
    /// <summary>
    /// 状态效果图标UI - 处理状态图标的显示和鼠标悬停提示
    /// </summary>
    public class StatusEffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("视觉组件")]
        public Image iconImage;                     // 状态图标
        public TextMeshProUGUI stacksText;          // 层数文本

        [Header("描述面板")]
        public GameObject descriptionPanel;         // 描述面板（StatusDescriptionPanel）
        public TextMeshProUGUI descriptionText;     // 描述文本（StatusDescriptionText）

        [Header("设置")]
        public float tooltipDelay = 0.3f;           // 鼠标悬停多久后显示提示（秒）

        private StatusEffect statusEffect;
        private float hoverTime = 0f;
        private bool isHovering = false;

        /// <summary>
        /// 初始化状态图标
        /// </summary>
        public void Initialize(StatusEffect effect)
        {
            statusEffect = effect;
            UpdateDisplay();

            // 确保描述面板初始隐藏
            if (descriptionPanel != null)
            {
                descriptionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (statusEffect == null) return;

            // 更新图标
            if (iconImage != null && statusEffect.data.iconSprite != null)
            {
                iconImage.sprite = statusEffect.data.iconSprite;
            }

            // 更新层数文本
            if (stacksText != null)
            {
                if (statusEffect.data.canStack)
                {
                    stacksText.text = statusEffect.stacks.ToString();
                }
                else
                {
                    stacksText.text = "";
                }

                // 炸弹显示倒计时（红色）
                if (statusEffect.data.statusType == StatusEffectType.Bomb)
                {
                    stacksText.text = statusEffect.duration.ToString();
                    stacksText.color = Color.red;
                }
                else
                {
                    stacksText.color = Color.white;
                }
            }

            // 更新描述文本（但不显示面板）
            if (descriptionText != null)
            {
                descriptionText.text = statusEffect.data.effectDescription;
            }
        }

        /// <summary>
        /// 鼠标进入（开始悬停）
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            hoverTime = 0f;
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            hoverTime = 0f;
            HideDescription();
        }

        private void Update()
        {
            // 如果鼠标正在悬停
            if (isHovering)
            {
                hoverTime += Time.deltaTime;

                // 超过延迟时间，显示描述
                if (hoverTime >= tooltipDelay)
                {
                    ShowDescription();
                }
            }
        }

        /// <summary>
        /// 显示描述面板
        /// </summary>
        private void ShowDescription()
        {
            if (descriptionPanel != null && !descriptionPanel.activeSelf)
            {
                descriptionPanel.SetActive(true);
                Debug.Log($"显示状态效果描述：{statusEffect.data.statusName}");
            }
        }

        /// <summary>
        /// 隐藏描述面板
        /// </summary>
        private void HideDescription()
        {
            if (descriptionPanel != null && descriptionPanel.activeSelf)
            {
                descriptionPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            // 对象被禁用时，确保隐藏描述
            HideDescription();
        }
    }
}

