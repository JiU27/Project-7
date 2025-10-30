using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardBattleGame.Systems;

namespace CardBattleGame
{
    /// <summary>
    /// 行动节点UI - 显示在速度条上
    /// </summary>
    public class ActionNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("视觉组件")]
        public Image characterIconImage;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI actionNameText;

        [Header("卡牌预览")]
        public GameObject cardPreviewObject;
        public Image cardPreviewImage;

        private ActionNode actionNode;

        /// <summary>
        /// 初始化节点
        /// </summary>
        public void Initialize(ActionNode node)
        {
            actionNode = node;

            if (speedText != null)
            {
                speedText.text = node.speed.ToString();
            }

            if (node.isPlayerAction && node.card != null)
            {
                if (actionNameText != null)
                {
                    actionNameText.text = node.card.data.cardName;
                }

                if (cardPreviewImage != null)
                {
                    cardPreviewImage.sprite = node.card.data.fullCardSprite;
                }
            }
            else if (!node.isPlayerAction)
            {
                if (actionNameText != null)
                {
                    actionNameText.text = "Enemy Action";
                }

                // TODO: 显示敌人技能对应的卡牌
            }

            // TODO: 设置角色头像
            // if (characterIconImage != null)
            // {
            //     characterIconImage.sprite = node.character.iconSprite;
            // }

            if (cardPreviewObject != null)
            {
                cardPreviewObject.SetActive(false);
            }
        }

        /// <summary>
        /// 鼠标进入（显示卡牌预览）
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (cardPreviewObject != null)
            {
                cardPreviewObject.SetActive(true);
            }
        }

        /// <summary>
        /// 鼠标离开（隐藏卡牌预览）
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (cardPreviewObject != null)
            {
                cardPreviewObject.SetActive(false);
            }
        }
    }
}

