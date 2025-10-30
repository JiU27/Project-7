using UnityEngine;
using TMPro;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// UI管理器 - 管理所有UI元素
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("顶部信息栏")]
        public TextMeshProUGUI roundNumberText;

        [Header("底部卡牌区域")]
        public TextMeshProUGUI drawPileCountText;
        public TextMeshProUGUI discardPileCountText;

        [Header("手牌槽位")]
        public Transform inherentCardSlotParent;
        public Transform normalCardSlotParent;

        [Header("输出槽位")]
        public Transform outputSlotParent;
        public TextMeshProUGUI[] slotEffectTexts;
        public TextMeshProUGUI[] slotSpeedTexts;

        [Header("行动条")]
        public Transform actionQueueParent;

        [Header("面板")]
        public GameObject watchPanel;
        public GameObject cardInspectPanel;
        public GameObject victoryPanel;
        public GameObject defeatPanel;
        public GameObject confirmEndTurnPanel;
        public GameObject perfectDeflectPanel;

        [Header("怀表面板组件")]
        public UnityEngine.UI.Image watchResultImage;
        public TextMeshProUGUI watchEffectText;

        [Header("卡牌检视面板组件")]
        public UnityEngine.UI.Image inspectCardImage;
        public TextMeshProUGUI inspectCardNameText;
        public TextMeshProUGUI inspectCardDescriptionText;
        public TextMeshProUGUI inspectCardSpeedText;

        [Header("Perfect Deflect面板组件")]
        public TextMeshProUGUI perfectDeflectMessageText;

        private BattleManager battleManager;

        public void Initialize(BattleManager battle)
        {
            battleManager = battle;
            HideAllPanels();
        }

        /// <summary>
        /// 更新回合数显示
        /// </summary>
        public void UpdateRoundNumber(int round)
        {
            if (roundNumberText != null)
            {
                roundNumberText.text = $"Round {round}";
            }
        }

        /// <summary>
        /// 更新牌库数量显示
        /// </summary>
        public void UpdateDeckCounts(int drawPileCount, int discardPileCount)
        {
            if (drawPileCountText != null)
            {
                drawPileCountText.text = drawPileCount.ToString();
            }

            if (discardPileCountText != null)
            {
                discardPileCountText.text = discardPileCount.ToString();
            }
        }

        /// <summary>
        /// 更新输出槽位效果描述
        /// </summary>
        public void UpdateSlotEffectDescriptions(string[] descriptions)
        {
            for (int i = 0; i < slotEffectTexts.Length && i < descriptions.Length; i++)
            {
                if (slotEffectTexts[i] != null)
                {
                    slotEffectTexts[i].text = descriptions[i];
                }
            }
        }

        /// <summary>
        /// 更新输出槽位速度显示
        /// </summary>
        public void UpdateSlotSpeed(int slotIndex, int speed)
        {
            if (slotIndex >= 0 && slotIndex < slotSpeedTexts.Length && slotSpeedTexts[slotIndex] != null)
            {
                slotSpeedTexts[slotIndex].text = speed.ToString();
            }
        }

        /// <summary>
        /// 清除输出槽位速度显示
        /// </summary>
        public void ClearSlotSpeed(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slotSpeedTexts.Length && slotSpeedTexts[slotIndex] != null)
            {
                slotSpeedTexts[slotIndex].text = "";
            }
        }

        /// <summary>
        /// 显示怀表面板
        /// </summary>
        public void ShowWatchPanel(Sprite watchSprite, string effectDescription)
        {
            if (watchPanel != null)
            {
                watchPanel.SetActive(true);
                
                if (watchResultImage != null)
                {
                    watchResultImage.sprite = watchSprite;
                }

                if (watchEffectText != null)
                {
                    watchEffectText.text = effectDescription;
                }
            }
        }

        /// <summary>
        /// 隐藏怀表面板
        /// </summary>
        public void HideWatchPanel()
        {
            if (watchPanel != null)
            {
                watchPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示卡牌检视面板
        /// </summary>
        public void ShowCardInspectPanel(Sprite cardSprite, string cardName, string description, int speed)
        {
            if (cardInspectPanel != null)
            {
                cardInspectPanel.SetActive(true);

                if (inspectCardImage != null)
                {
                    inspectCardImage.sprite = cardSprite;
                }

                if (inspectCardNameText != null)
                {
                    inspectCardNameText.text = cardName;
                }

                if (inspectCardDescriptionText != null)
                {
                    inspectCardDescriptionText.text = description;
                }

                if (inspectCardSpeedText != null)
                {
                    inspectCardSpeedText.text = $"{speed}";
                }
            }
        }

        /// <summary>
        /// 隐藏卡牌检视面板
        /// </summary>
        public void HideCardInspectPanel()
        {
            if (cardInspectPanel != null)
            {
                cardInspectPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示胜利面板
        /// </summary>
        public void ShowVictoryPanel()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 显示失败面板
        /// </summary>
        public void ShowDefeatPanel()
        {
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 显示确认结束回合面板
        /// </summary>
        public void ShowConfirmEndTurnPanel()
        {
            if (confirmEndTurnPanel != null)
            {
                confirmEndTurnPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏确认结束回合面板
        /// </summary>
        public void HideConfirmEndTurnPanel()
        {
            if (confirmEndTurnPanel != null)
            {
                confirmEndTurnPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示Perfect Deflect面板
        /// </summary>
        public void ShowPerfectDeflectPanel(string attackerName, string defenderName)
        {
            if (perfectDeflectPanel != null)
            {
                perfectDeflectPanel.SetActive(true);

                if (perfectDeflectMessageText != null)
                {
                    perfectDeflectMessageText.text = $"Perfect Deflect!\n{attackerName} 克制了 {defenderName}！";
                }

                Debug.Log($"显示Perfect Deflect面板：{attackerName} vs {defenderName}");
            }
            else
            {
                Debug.LogWarning("Perfect Deflect Panel 未设置");
            }
        }

        /// <summary>
        /// 隐藏Perfect Deflect面板
        /// </summary>
        public void HidePerfectDeflectPanel()
        {
            if (perfectDeflectPanel != null)
            {
                perfectDeflectPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            HideWatchPanel();
            HideCardInspectPanel();
            HidePerfectDeflectPanel();
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            HideConfirmEndTurnPanel();
        }

        /// <summary>
        /// 按钮：确认结束回合
        /// </summary>
        public void OnConfirmEndTurnButton()
        {
            Debug.Log("玩家确认结束回合");
            HideConfirmEndTurnPanel();
            if (battleManager != null)
            {
                battleManager.EndPlayerTurn();
            }
            else
            {
                Debug.LogError("BattleManager 未设置！");
            }
        }

        /// <summary>
        /// 按钮：取消结束回合
        /// </summary>
        public void OnCancelEndTurnButton()
        {
            Debug.Log("玩家取消结束回合");
            HideConfirmEndTurnPanel();
        }
        
        /// <summary>
        /// 初始化确认面板按钮事件
        /// </summary>
        private void Start()
        {
            SetupConfirmPanelButtons();
        }

        private void SetupConfirmPanelButtons()
        {
            // 这里可以手动连接按钮事件，或者在Inspector中连接
            // 如果在Inspector中连接，这个方法可以为空
        }

        /// <summary>
        /// 按钮：关闭检视面板
        /// </summary>
        public void OnCloseInspectPanelButton()
        {
            HideCardInspectPanel();
        }

        /// <summary>
        /// 按钮：继续（关闭怀表面板）
        /// </summary>
        public void OnContinueButton()
        {
            HideWatchPanel();
        }
    }
}

