using UnityEngine;
using System.Collections.Generic;
using CardBattleGame.Data;

namespace CardBattleGame.Entities
{
    /// <summary>
    /// 敌人类
    /// </summary>
    public class Enemy : Character
    {
        private MonsterData monsterData;
        private int currentSkillIndex = 0;
        private bool hasUsedFirstTurnSpecial = false;

        public EnemyUI enemyUI;  // 敌人特有的UI（显示行动意图等）

        private void Awake()
        {
            // 自动同步UI引用：如果enemyUI已设置但characterUI未设置，自动赋值
            if (enemyUI != null && characterUI == null)
            {
                characterUI = enemyUI;
                Debug.Log($"{name} 自动将 EnemyUI 赋值给 CharacterUI");
            }
        }

        /// <summary>
        /// 使用MonsterData初始化敌人
        /// </summary>
        public void Initialize(MonsterData data)
        {
            monsterData = data;
            characterId = data.id;
            
            // 再次确保UI引用同步
            if (enemyUI != null && characterUI == null)
            {
                characterUI = enemyUI;
            }
            
            base.Initialize(data.maxHealth, data.initialArmor, data.audioData);
        }

        /// <summary>
        /// 获取当前回合的行动列表
        /// </summary>
        public List<EnemySkill> GetActionsForTurn(int turnNumber)
        {
            List<EnemySkill> actions = new List<EnemySkill>();
            
            // 第一回合特殊技能
            if (turnNumber == 1 && monsterData.actionPattern.hasFirstTurnSpecial && !hasUsedFirstTurnSpecial)
            {
                int specialIndex = monsterData.actionPattern.firstTurnSkillIndex;
                if (specialIndex >= 0 && specialIndex < monsterData.skillList.Count)
                {
                    actions.Add(monsterData.skillList[specialIndex]);
                    hasUsedFirstTurnSpecial = true;
                    return actions;
                }
            }

            // 根据行动次数添加技能
            int actionsPerTurn = monsterData.actionPattern.actionsPerTurn;
            for (int i = 0; i < actionsPerTurn; i++)
            {
                EnemySkill skill = GetNextSkill();
                actions.Add(skill);
            }

            return actions;
        }

        /// <summary>
        /// 获取下一个技能
        /// </summary>
        private EnemySkill GetNextSkill()
        {
            if (monsterData.skillList.Count == 0)
            {
                Debug.LogError($"Enemy {monsterData.monsterName} has no skills!");
                return new EnemySkill();
            }

            // 过滤掉第一回合特殊技能（如果已使用）
            List<EnemySkill> availableSkills = new List<EnemySkill>(monsterData.skillList);
            if (hasUsedFirstTurnSpecial && monsterData.actionPattern.hasFirstTurnSpecial)
            {
                int specialIndex = monsterData.actionPattern.firstTurnSkillIndex;
                if (specialIndex >= 0 && specialIndex < availableSkills.Count)
                {
                    availableSkills.RemoveAt(specialIndex);
                }
            }

            if (availableSkills.Count == 0)
            {
                availableSkills = new List<EnemySkill>(monsterData.skillList);
            }

            EnemySkill selectedSkill;

            if (monsterData.actionPattern.randomSkillOrder)
            {
                // 随机选择
                selectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            }
            else
            {
                // 循环选择
                selectedSkill = availableSkills[currentSkillIndex % availableSkills.Count];
                currentSkillIndex++;
            }

            return selectedSkill;
        }

        public MonsterData GetMonsterData()
        {
            return monsterData;
        }
    }
}

