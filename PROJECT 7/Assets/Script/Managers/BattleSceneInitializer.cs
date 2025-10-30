using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Entities;
using CardBattleGame.Data;

namespace CardBattleGame.Managers
{
    /// <summary>
    /// 战斗场景初始化器 - 负责在场景中实例化玩家和敌人
    /// </summary>
    public class BattleSceneInitializer : MonoBehaviour
    {
        [Header("生成位置")]
        public Transform playerSpawnPoint;      // 玩家生成位置
        public Transform[] enemySpawnPoints;    // 敌人生成位置数组

        [Header("引用")]
        public BattleManager battleManager;

        private void Awake()
        {
            // 确保BattleManager在初始化之前先生成角色
            if (battleManager != null)
            {
                SpawnCharacters();
            }
        }

        /// <summary>
        /// 生成角色（玩家和敌人）
        /// </summary>
        public void SpawnCharacters()
        {
            // 生成玩家
            SpawnPlayer();

            // 生成敌人
            SpawnEnemies();
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            if (battleManager.playerCharacterData == null)
            {
                Debug.LogError("未设置玩家角色数据！");
                return;
            }

            if (battleManager.playerCharacterData.characterPrefab == null)
            {
                Debug.LogError("玩家角色数据中没有设置预制体！");
                return;
            }

            // 确定生成位置
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(-5f, 0f, 0f);
            Quaternion spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

            // 实例化玩家预制体
            GameObject playerObj = Instantiate(
                battleManager.playerCharacterData.characterPrefab,
                spawnPosition,
                spawnRotation
            );
            playerObj.name = "Player";

            // 获取Player组件
            Player player = playerObj.GetComponent<Player>();
            if (player == null)
            {
                player = playerObj.AddComponent<Player>();
            }

            // 初始化玩家
            player.Initialize(battleManager.playerCharacterData);

            // 将玩家引用传递给BattleManager
            battleManager.player = player;

            Debug.Log($"玩家已生成：{battleManager.playerCharacterData.characterName}，位置：{spawnPosition}");
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private void SpawnEnemies()
        {
            if (battleManager.encounterMonsters == null || battleManager.encounterMonsters.Count == 0)
            {
                Debug.LogError("未设置遭遇的敌人数据！");
                return;
            }

            battleManager.enemies.Clear();

            for (int i = 0; i < battleManager.encounterMonsters.Count; i++)
            {
                MonsterData monsterData = battleManager.encounterMonsters[i];

                if (monsterData == null)
                {
                    Debug.LogWarning($"敌人数据 {i} 为空，跳过");
                    continue;
                }

                if (monsterData.monsterPrefab == null)
                {
                    Debug.LogError($"敌人数据 {monsterData.monsterName} 中没有设置预制体！");
                    continue;
                }

                // 确定生成位置
                Vector3 spawnPosition;
                Quaternion spawnRotation;

                if (enemySpawnPoints != null && i < enemySpawnPoints.Length && enemySpawnPoints[i] != null)
                {
                    spawnPosition = enemySpawnPoints[i].position;
                    spawnRotation = enemySpawnPoints[i].rotation;
                }
                else
                {
                    // 默认位置：右侧排列
                    spawnPosition = new Vector3(5f, 0f + (i * 2f), 0f);
                    spawnRotation = Quaternion.identity;
                }

                // 实例化敌人预制体
                GameObject enemyObj = Instantiate(
                    monsterData.monsterPrefab,
                    spawnPosition,
                    spawnRotation
                );
                enemyObj.name = $"Enemy_{monsterData.monsterName}_{i}";

                // 获取Enemy组件
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy == null)
                {
                    enemy = enemyObj.AddComponent<Enemy>();
                }

                // 初始化敌人
                enemy.Initialize(monsterData);

                // 添加到BattleManager的敌人列表
                battleManager.enemies.Add(enemy);

                Debug.Log($"敌人已生成：{monsterData.monsterName}，位置：{spawnPosition}");
            }

            // 设置默认目标
            if (battleManager.enemies.Count > 0)
            {
                battleManager.currentTarget = battleManager.enemies[0];
            }
        }

        /// <summary>
        /// 手动触发生成（用于测试）
        /// </summary>
        [ContextMenu("生成角色")]
        public void SpawnCharactersManually()
        {
            SpawnCharacters();
        }
    }
}

