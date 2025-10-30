using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace CardBattleGame
{
    /// <summary>
    /// 即时释放区域UI - 检测Is Instant Cast卡牌的拖拽释放
    /// </summary>
    public class InstantCastZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("视觉反馈")]
        public Image zoneImage;                     // 区域背景图片
        public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);   // 正常颜色
        public Color highlightColor = new Color(0.3f, 0.6f, 0.3f, 0.7f); // 高亮颜色（拖拽时）
        public Color errorColor = new Color(0.6f, 0.2f, 0.2f, 0.7f);    // 错误颜色（非即时卡牌）

        [Header("提示文字")]
        public TextMeshProUGUI hintText;
        public string normalHint = "拖入即时卡牌以释放";
        public string highlightHint = "松开鼠标释放卡牌";
        public string errorHint = "此卡牌无法即时释放";

        [Header("引用")]
        public Managers.BattleManager battleManager;

        private bool isDraggingOverZone = false;
        private CardUI currentDraggingCard = null;

        private void Start()
        {
            // 设置初始状态
            SetZoneState(ZoneState.Normal);
        }

        /// <summary>
        /// 鼠标进入区域
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            isDraggingOverZone = true;
            
            // 检查是否正在拖拽卡牌
            CardUI draggingCard = GetDraggingCard(eventData);
            if (draggingCard != null)
            {
                currentDraggingCard = draggingCard;
                
                // 检查是否为即时释放卡牌
                if (draggingCard.GetCard().data.cardEffect.isInstantCast)
                {
                    SetZoneState(ZoneState.Highlight);
                }
                else
                {
                    SetZoneState(ZoneState.Error);
                }
            }
        }

        /// <summary>
        /// 鼠标离开区域
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            isDraggingOverZone = false;
            currentDraggingCard = null;
            SetZoneState(ZoneState.Normal);
        }

        /// <summary>
        /// 卡牌被拖放到区域
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            CardUI cardUI = GetDraggingCard(eventData);
            
            if (cardUI == null)
            {
                Debug.LogWarning("拖放到即时释放区域的不是卡牌");
                return;
            }

            Entities.Card card = cardUI.GetCard();

            // 检查是否为即时释放卡牌
            if (!card.data.cardEffect.isInstantCast)
            {
                Debug.LogWarning($"卡牌 {card.data.cardName} 不是即时释放卡牌！");
                SetZoneState(ZoneState.Error);
                
                // 短暂显示错误状态后恢复
                Invoke(nameof(ResetZoneState), 0.5f);
                return;
            }

            // 执行即时释放
            ExecuteInstantCast(cardUI, card);
            
            // 重置区域状态
            SetZoneState(ZoneState.Normal);
        }

        /// <summary>
        /// 执行即时释放
        /// </summary>
        private void ExecuteInstantCast(CardUI cardUI, Entities.Card card)
        {
            Debug.Log($"即时释放：{card.data.cardName}");

            if (battleManager == null)
            {
                Debug.LogError("BattleManager未设置，无法释放卡牌！");
                return;
            }

            // 确定目标
            Entities.Character target = DetermineTarget(card);

            if (target == null)
            {
                Debug.LogError("无法确定目标！");
                return;
            }

            // 执行卡牌效果（立即，不进入行动队列）
            ExecuteCardEffectImmediately(card, target);

            // 从手牌中移除这张卡
            if (battleManager.cardManager != null)
            {
                battleManager.cardManager.PlayCard(card);
            }

            // 销毁卡牌UI
            Destroy(cardUI.gameObject);

            Debug.Log($"卡牌 {card.data.cardName} 已即时释放并生效");
        }

        /// <summary>
        /// 确定目标（根据卡牌效果）
        /// </summary>
        private Entities.Character DetermineTarget(Entities.Card card)
        {
            // 如果是对自己的效果（治疗、护甲、Buff）
            if (card.data.cardEffect.healsHealth || 
                card.data.cardEffect.grantsArmor || 
                (card.data.cardEffect.grantsStatusEffect && !card.data.cardEffect.dealsDamage))
            {
                return battleManager.player;
            }

            // 如果是攻击或debuff，目标是当前选中的敌人
            if (card.data.cardEffect.dealsDamage || 
                card.data.cardEffect.grantsStatusEffect)
            {
                return battleManager.currentTarget;
            }

            // 默认目标是玩家自己
            return battleManager.player;
        }

        /// <summary>
        /// 立即执行卡牌效果
        /// </summary>
        private void ExecuteCardEffectImmediately(Entities.Card card, Entities.Character target)
        {
            // 造成伤害
            if (card.data.cardEffect.dealsDamage)
            {
                if (card.data.cardEffect.isAoE)
                {
                    // 群体伤害
                    foreach (var enemy in battleManager.enemies)
                    {
                        if (enemy != null && enemy.IsAlive())
                        {
                            enemy.TakeDamage(card.modifiedDamage, card.data.elementType);
                        }
                    }
                    Debug.Log($"即时释放造成群体伤害：{card.modifiedDamage}");
                }
                else
                {
                    // 单体伤害
                    target.TakeDamage(card.modifiedDamage, card.data.elementType);
                    Debug.Log($"即时释放对 {target.name} 造成 {card.modifiedDamage} 点伤害");
                }
            }

            // 恢复生命
            if (card.data.cardEffect.healsHealth)
            {
                battleManager.player.Heal(card.modifiedHeal);
                Debug.Log($"即时释放恢复 {card.modifiedHeal} 点生命");
            }

            // 获得护甲
            if (card.data.cardEffect.grantsArmor)
            {
                battleManager.player.GainArmor(card.modifiedArmor);
                Debug.Log($"即时释放获得 {card.modifiedArmor} 点护甲");
            }

            // 赋予状态效果
            if (card.data.cardEffect.grantsStatusEffect && card.data.cardEffect.statusType != StatusEffectType.None)
            {
                // TODO: 从数据库获取StatusEffectData
                Debug.Log($"即时释放赋予状态效果：{card.data.cardEffect.statusType}");
                // target.AddStatusEffect(card.data.cardEffect.statusType, card.modifiedStatusStacks, statusEffectData);
            }

            // 更新UI
            if (battleManager.battleUIUpdater != null)
            {
                battleManager.battleUIUpdater.UpdateAllCharacterUI();
            }
        }

        /// <summary>
        /// 获取正在拖拽的卡牌
        /// </summary>
        private CardUI GetDraggingCard(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                return eventData.pointerDrag.GetComponent<CardUI>();
            }
            return null;
        }

        /// <summary>
        /// 设置区域状态
        /// </summary>
        private void SetZoneState(ZoneState state)
        {
            if (zoneImage != null)
            {
                switch (state)
                {
                    case ZoneState.Normal:
                        zoneImage.color = normalColor;
                        break;
                    case ZoneState.Highlight:
                        zoneImage.color = highlightColor;
                        break;
                    case ZoneState.Error:
                        zoneImage.color = errorColor;
                        break;
                }
            }

            if (hintText != null)
            {
                switch (state)
                {
                    case ZoneState.Normal:
                        hintText.text = normalHint;
                        break;
                    case ZoneState.Highlight:
                        hintText.text = highlightHint;
                        break;
                    case ZoneState.Error:
                        hintText.text = errorHint;
                        break;
                }
            }
        }

        /// <summary>
        /// 重置区域状态（延迟调用）
        /// </summary>
        private void ResetZoneState()
        {
            SetZoneState(ZoneState.Normal);
        }

        /// <summary>
        /// 区域状态枚举
        /// </summary>
        private enum ZoneState
        {
            Normal,     // 正常
            Highlight,  // 高亮（可释放）
            Error       // 错误（不可释放）
        }
    }
}

