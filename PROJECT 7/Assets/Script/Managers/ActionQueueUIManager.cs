using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Systems;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 行动队列UI管理器 - 负责显示速度条上的行动节点
    /// </summary>
    public class ActionQueueUIManager : MonoBehaviour
    {
        [Header("行动节点预制体")]
        public GameObject actionNodePrefab;     // 行动节点UI预制体

        [Header("显示区域")]
        public Transform actionQueueParent;     // 行动队列显示的父对象

        private List<ActionNodeUI> currentNodes = new List<ActionNodeUI>();

        /// <summary>
        /// 显示行动队列
        /// </summary>
        public void DisplayActionQueue(List<ActionNode> actionQueue)
        {
            // 清除现有节点
            ClearActionQueue();

            if (actionQueue == null || actionQueue.Count == 0)
            {
                Debug.LogWarning("行动队列为空");
                return;
            }

            if (actionNodePrefab == null)
            {
                Debug.LogError("Action Node Prefab 未设置！请在 ActionQueueUIManager 中分配预制体。");
                return;
            }

            if (actionQueueParent == null)
            {
                Debug.LogError("Action Queue Parent 未设置！");
                return;
            }

            // 为每个行动创建UI节点（顺序：速度从高到低）
            for (int i = 0; i < actionQueue.Count; i++)
            {
                ActionNodeUI nodeUI = CreateActionNodeUI(actionQueue[i]);
                
                // 关键：设置节点的顺序（速度大的在上面）
                // 由于actionQueue已经按速度降序排列，第0个速度最大
                // SetSiblingIndex(0)会放在最上面
                if (nodeUI != null)
                {
                    nodeUI.transform.SetSiblingIndex(i);
                }
            }

            Debug.Log($"行动队列UI已显示：{actionQueue.Count} 个行动（速度从上到下：高→低）");
        }

        /// <summary>
        /// 创建单个行动节点UI
        /// </summary>
        private ActionNodeUI CreateActionNodeUI(ActionNode actionNode)
        {
            GameObject nodeObj = Instantiate(actionNodePrefab, actionQueueParent);
            ActionNodeUI nodeUI = nodeObj.GetComponent<ActionNodeUI>();

            if (nodeUI == null)
            {
                Debug.LogError("Action Node Prefab 上没有 ActionNodeUI 组件！");
                Destroy(nodeObj);
                return null;
            }

            // 初始化节点
            nodeUI.Initialize(actionNode);

            // 添加到列表
            currentNodes.Add(nodeUI);

            return nodeUI;
        }

        /// <summary>
        /// 清除所有行动节点UI
        /// </summary>
        public void ClearActionQueue()
        {
            foreach (var node in currentNodes)
            {
                if (node != null)
                {
                    Destroy(node.gameObject);
                }
            }
            currentNodes.Clear();
        }

        /// <summary>
        /// 移除指定行动的节点UI
        /// </summary>
        public void RemoveActionNode(ActionNode actionNode)
        {
            // 查找对应的ActionNodeUI
            ActionNodeUI nodeToRemove = null;
            
            for (int i = 0; i < currentNodes.Count; i++)
            {
                if (currentNodes[i] != null)
                {
                    // 这里需要判断节点是否对应该行动
                    // 简化处理：移除第一个节点（假设按顺序执行）
                    nodeToRemove = currentNodes[i];
                    currentNodes.RemoveAt(i);
                    break;
                }
            }

            if (nodeToRemove != null)
            {
                Destroy(nodeToRemove.gameObject);
                Debug.Log("移除了一个行动节点，剩余节点自动上移");
            }
        }

        /// <summary>
        /// 移除第一个行动节点（最上面的）
        /// </summary>
        public void RemoveFirstActionNode()
        {
            if (currentNodes.Count > 0 && currentNodes[0] != null)
            {
                ActionNodeUI nodeToRemove = currentNodes[0];
                currentNodes.RemoveAt(0);
                Destroy(nodeToRemove.gameObject);
                Debug.Log($"移除了最上方的行动节点，剩余 {currentNodes.Count} 个节点");
            }
        }

        /// <summary>
        /// 更新行动队列显示
        /// </summary>
        public void UpdateActionQueue(List<ActionNode> actionQueue)
        {
            DisplayActionQueue(actionQueue);
        }

        /// <summary>
        /// 移除指定索引的节点UI
        /// </summary>
        public void RemoveNodeAtIndex(int index)
        {
            if (index >= 0 && index < currentNodes.Count)
            {
                ActionNodeUI nodeToRemove = currentNodes[index];
                if (nodeToRemove != null)
                {
                    currentNodes.RemoveAt(index);
                    Destroy(nodeToRemove.gameObject);
                    Debug.Log($"移除了索引 {index} 的行动节点，剩余 {currentNodes.Count} 个节点");
                }
            }
            else
            {
                Debug.LogWarning($"无效的节点索引：{index}，当前节点数：{currentNodes.Count}");
            }
        }

        /// <summary>
        /// 获取当前节点数量
        /// </summary>
        public int GetNodeCount()
        {
            return currentNodes.Count;
        }
    }
}



