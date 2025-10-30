using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Managers;

namespace CardBattleGame.Examples
{
    /// <summary>
    /// 快速战斗设置 - 用于快速配置测试战斗
    /// 挂载到场景中的任意GameObject上，在BattleManager初始化之前执行
    /// </summary>
    public class QuickBattleSetup : MonoBehaviour
    {
        [Header("卡组管理器")]
        public DeckManager deckManager;

        [Header("快速测试卡组（输入卡牌ID）")]
        [Tooltip("如果为空，将使用DeckManager中已有的卡组")]
        public List<int> testDeckCardIds = new List<int>();

        [Header("是否自动设置")]
        public bool autoSetupOnAwake = true;

        private void Start()
        {
            // 改为Start而不是Awake，确保DeckManager的Awake已经完成
            if (autoSetupOnAwake)
            {
                SetupDeck();
            }
        }

        /// <summary>
        /// 设置卡组
        /// </summary>
        public void SetupDeck()
        {
            if (deckManager == null)
            {
                Debug.LogError("DeckManager未分配！");
                return;
            }

            // 如果没有设置测试卡组，检查DeckManager中是否有卡组
            if (testDeckCardIds == null || testDeckCardIds.Count == 0)
            {
                if (deckManager.playerDeckCardIds.Count == 0)
                {
                    Debug.LogError("没有卡组配置！请在DeckManager的Player Deck Card Ids列表中添加卡牌ID，或在QuickBattleSetup的Test Deck Card Ids中添加测试卡牌。");
                    Debug.LogWarning("提示：创建CardData ScriptableObject后，检查其ID，然后将ID添加到卡组列表中。");
                    return;
                }
                
                Debug.Log($"使用DeckManager中的卡组，共 {deckManager.playerDeckCardIds.Count} 张卡牌");
            }
            else
            {
                // 使用测试卡组
                deckManager.SetDeck(testDeckCardIds);
                Debug.Log($"快速测试卡组已设置，共 {testDeckCardIds.Count} 张卡牌");
            }

            // 显示卡组内容（用于调试）
            ShowDeckInfo();
        }

        /// <summary>
        /// 显示卡组信息
        /// </summary>
        private void ShowDeckInfo()
        {
            Debug.Log("===== 当前卡组配置 =====");
            
            // 显示配置的卡牌ID
            List<int> configuredIds = testDeckCardIds != null && testDeckCardIds.Count > 0 
                ? testDeckCardIds 
                : deckManager.playerDeckCardIds;
                
            Debug.Log($"配置的卡牌ID：[{string.Join(", ", configuredIds)}]");
            
            // 显示实际找到的卡牌
            var deckCards = deckManager.GetDeckCards();
            
            if (deckCards.Count == 0)
            {
                Debug.LogError("❌ 卡组为空！没有找到任何卡牌！");
                Debug.LogError("可能的原因：");
                Debug.LogError("1. CardData的ID字段与卡组中的ID不匹配");
                Debug.LogError("2. Card Database为空或包含null元素");
                Debug.LogError("3. CardData资源未正确创建");
                Debug.LogError("---");
                Debug.LogError("请检查：");
                Debug.LogError($"- 卡组使用的ID：{string.Join(", ", configuredIds)}");
                Debug.LogError("- 打开每个CardData资源，确认其ID字段值");
                Debug.LogError("- 确保ID匹配");
                return;
            }

            Debug.Log($"✓ 成功加载 {deckCards.Count} 张卡牌：");
            foreach (var card in deckCards)
            {
                string inherent = card.cardEffect.isInherent ? "[固有]" : "";
                Debug.Log($"  - {inherent} {card.cardName} (ID:{card.id}, 速度:{card.baseSpeed}, 类型:{card.cardType})");
            }
            
            // 检查是否有ID未匹配
            int missingCount = configuredIds.Count - deckCards.Count;
            if (missingCount > 0)
            {
                Debug.LogWarning($"⚠ 警告：卡组配置了{configuredIds.Count}张卡，但只找到了{deckCards.Count}张！");
                Debug.LogWarning("某些ID可能没有对应的CardData资源。");
            }
            
            Debug.Log("======================");
        }

        /// <summary>
        /// 手动触发设置（用于测试）
        /// </summary>
        [ContextMenu("设置卡组")]
        public void SetupDeckManually()
        {
            SetupDeck();
        }

        /// <summary>
        /// 创建示例测试卡组
        /// </summary>
        [ContextMenu("创建示例测试卡组（需要先创建对应的CardData）")]
        public void CreateExampleDeck()
        {
            testDeckCardIds.Clear();
            
            // 假设你已经创建了ID为1-10的卡牌
            // 请根据实际创建的CardData的ID来修改这里
            testDeckCardIds.Add(1);
            testDeckCardIds.Add(1);
            testDeckCardIds.Add(1);
            testDeckCardIds.Add(2);
            testDeckCardIds.Add(2);
            testDeckCardIds.Add(3);
            testDeckCardIds.Add(4);
            testDeckCardIds.Add(5);
            
            Debug.Log("示例测试卡组已创建（ID: 1,1,1,2,2,3,4,5）");
            Debug.LogWarning("注意：这只是示例ID，请确保你已经创建了对应ID的CardData资源！");
        }
    }
}

