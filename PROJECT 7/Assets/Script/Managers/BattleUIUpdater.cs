using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CardBattleGame.Entities;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 战斗UI更新器 - 实时同步游戏数据到UI显示
    /// </summary>
    public class BattleUIUpdater : MonoBehaviour
    {
        [Header("管理器引用")]
        public BattleManager battleManager;
        public CardManager cardManager;
        public UIManager uiManager;

        [Header("卡牌数量显示")]
        public TextMeshProUGUI drawPileCountText;
        public TextMeshProUGUI discardPileCountText;

        [Header("回合显示")]
        public TextMeshProUGUI roundNumberText;

        [Header("按钮")]
        public Button endTurnButton;
        public Button revertButton;

        [Header("输出槽位")]
        public List<OutputSlotUI> outputSlots = new List<OutputSlotUI>();

        private void Start()
        {
            // 连接按钮事件
            SetupButtons();
        }

        private void Update()
        {
            // 每帧更新UI（可以优化为事件驱动）
            UpdateCardCounts();
            UpdateRoundNumber();
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveAllListeners();
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
                Debug.Log("END按钮事件已连接");
            }
            else
            {
                Debug.LogWarning("END Turn Button 未分配！");
            }

            if (revertButton != null)
            {
                revertButton.onClick.RemoveAllListeners();
                revertButton.onClick.AddListener(OnRevertClicked);
                Debug.Log("Revert按钮事件已连接");
            }
            else
            {
                Debug.LogWarning("Revert Button 未分配！");
            }
        }

        /// <summary>
        /// 更新卡牌数量显示
        /// </summary>
        public void UpdateCardCounts()
        {
            if (cardManager == null) return;

            if (drawPileCountText != null)
            {
                drawPileCountText.text = cardManager.GetDrawPileCount().ToString();
            }

            if (discardPileCountText != null)
            {
                discardPileCountText.text = cardManager.GetDiscardPileCount().ToString();
            }
        }

        /// <summary>
        /// 更新回合数显示
        /// </summary>
        public void UpdateRoundNumber()
        {
            if (battleManager == null || battleManager.turnSystem == null) return;

            if (roundNumberText != null)
            {
                roundNumberText.text = $"Round {battleManager.turnSystem.currentTurn}";
            }
        }

        /// <summary>
        /// 初始化输出槽位
        /// </summary>
        public void InitializeOutputSlots()
        {
            if (battleManager == null || battleManager.currentOutputSystem == null)
            {
                Debug.LogError("无法初始化输出槽位：BattleManager或OutputSystem未设置");
                return;
            }

            int slotCount = battleManager.currentOutputSystem.slotCount;

            for (int i = 0; i < outputSlots.Count && i < slotCount; i++)
            {
                if (outputSlots[i] != null)
                {
                    string description = i < battleManager.currentOutputSystem.slotEffects.Length
                        ? battleManager.currentOutputSystem.slotEffects[i].effectDescription
                        : "槽位效果";

                    outputSlots[i].Initialize(i, description, battleManager, uiManager);
                    Debug.Log($"输出槽位 {i} 已初始化");
                }
            }
        }

        /// <summary>
        /// END按钮点击
        /// </summary>
        private void OnEndTurnClicked()
        {
            Debug.Log("玩家点击了END按钮");

            if (battleManager == null)
            {
                Debug.LogError("BattleManager未设置！");
                return;
            }

            // 检查是否所有槽位都已填满
            if (!battleManager.AreAllSlotsFilled())
            {
                // 显示确认面板
                if (uiManager != null)
                {
                    uiManager.ShowConfirmEndTurnPanel();
                }
                else
                {
                    Debug.LogWarning("UIManager未设置，跳过确认直接结束回合");
                    battleManager.EndPlayerTurn();
                }
            }
            else
            {
                // 直接结束回合
                battleManager.EndPlayerTurn();
            }
        }

        /// <summary>
        /// Revert按钮点击
        /// </summary>
        private void OnRevertClicked()
        {
            Debug.Log("玩家点击了Revert按钮");

            if (battleManager == null)
            {
                Debug.LogError("BattleManager未设置！");
                return;
            }

            // 从所有槽位移除卡牌
            int revertedCount = 0;
            for (int i = 0; i < outputSlots.Count; i++)
            {
                if (outputSlots[i] != null && !outputSlots[i].IsEmpty())
                {
                    CardUI cardUI = outputSlots[i].RemoveCard();
                    if (cardUI != null)
                    {
                        // 卡牌返回手牌区
                        cardUI.ReturnToOriginalPosition();
                        revertedCount++;
                    }
                }
            }

            Debug.Log($"已退回 {revertedCount} 张卡牌");
        }

        /// <summary>
        /// 更新所有角色UI
        /// </summary>
        public void UpdateAllCharacterUI()
        {
            // 更新玩家UI
            if (battleManager != null && battleManager.player != null)
            {
                UpdateCharacterUI(battleManager.player);
            }

            // 更新所有敌人UI
            if (battleManager != null && battleManager.enemies != null)
            {
                foreach (var enemy in battleManager.enemies)
                {
                    if (enemy != null && enemy.IsAlive())
                    {
                        UpdateCharacterUI(enemy);
                    }
                }
            }
        }

        /// <summary>
        /// 更新单个角色的UI
        /// </summary>
        private void UpdateCharacterUI(Character character)
        {
            if (character.characterUI == null) return;

            // 更新血条
            character.characterUI.UpdateHealthBar(character.currentHealth, character.maxHealth);

            // 更新护甲条
            character.characterUI.UpdateArmorBar(character.currentArmor);

            // 更新状态效果
            character.characterUI.UpdateStatusEffects(character.statusEffects);
        }

        /// <summary>
        /// 手动调用更新所有UI（用于调试）
        /// </summary>
        [ContextMenu("更新所有UI")]
        public void RefreshAllUI()
        {
            UpdateCardCounts();
            UpdateRoundNumber();
            UpdateAllCharacterUI();
            Debug.Log("所有UI已更新");
        }
    }
}

