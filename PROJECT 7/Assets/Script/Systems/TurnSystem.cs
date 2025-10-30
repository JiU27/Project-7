using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Entities;
using CardBattleGame.Data;

namespace CardBattleGame.Systems
{
    /// <summary>
    /// 回合系统
    /// </summary>
    public class TurnSystem : MonoBehaviour
    {
        public BattlePhase currentPhase { get; private set; }
        public int currentTurn { get; private set; }

        private ActionQueueSystem actionQueueSystem;
        private StatusEffectSystem statusEffectSystem;
        private WatchResult currentWatchResult;
        private bool noneTypeBoostActive;

        public void Initialize(ActionQueueSystem actionQueue, StatusEffectSystem statusSystem)
        {
            actionQueueSystem = actionQueue;
            statusEffectSystem = statusSystem;
            currentTurn = 0;
            currentPhase = BattlePhase.Preparation;
        }

        /// <summary>
        /// 开始新回合
        /// </summary>
        public void StartNewTurn()
        {
            currentTurn++;
            currentPhase = BattlePhase.Preparation;
            noneTypeBoostActive = false;
            Debug.Log($"===== 回合 {currentTurn} 开始 =====");
        }

        /// <summary>
        /// 设置怀表结果
        /// </summary>
        public void SetWatchResult(WatchResult result)
        {
            currentWatchResult = result;
        }

        /// <summary>
        /// 获取当前怀表结果
        /// </summary>
        public WatchResult GetWatchResult()
        {
            return currentWatchResult;
        }

        /// <summary>
        /// 是否有"无"类型加成
        /// </summary>
        public bool HasNoneTypeBoost()
        {
            return noneTypeBoostActive;
        }

        /// <summary>
        /// 激活"无"类型加成
        /// </summary>
        public void ActivateNoneTypeBoost()
        {
            noneTypeBoostActive = true;
        }

        /// <summary>
        /// 进入下一阶段
        /// </summary>
        public void AdvancePhase()
        {
            switch (currentPhase)
            {
                case BattlePhase.Preparation:
                    currentPhase = BattlePhase.Planning;
                    Debug.Log("进入规划阶段");
                    break;

                case BattlePhase.Planning:
                    currentPhase = BattlePhase.Combat;
                    Debug.Log("进入战斗阶段");
                    break;

                case BattlePhase.Combat:
                    currentPhase = BattlePhase.Resolution;
                    Debug.Log("进入结算阶段");
                    break;

                case BattlePhase.Resolution:
                    // 准备下一回合
                    StartNewTurn();
                    break;
            }
        }

        /// <summary>
        /// 设置阶段
        /// </summary>
        public void SetPhase(BattlePhase phase)
        {
            currentPhase = phase;
            Debug.Log($"战斗阶段变更为：{phase}");
        }

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        public bool CheckBattleEnd(Player player, List<Enemy> enemies)
        {
            // 玩家死亡
            if (!player.IsAlive())
            {
                currentPhase = BattlePhase.Defeat;
                Debug.Log("玩家战败");
                return true;
            }

            // 所有敌人死亡
            bool allEnemiesDead = true;
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive())
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead)
            {
                currentPhase = BattlePhase.Victory;
                Debug.Log("玩家胜利");
                return true;
            }

            return false;
        }
    }
}

