using UnityEngine;
using CardBattleGame.Data;

namespace CardBattleGame.Entities
{
    /// <summary>
    /// 玩家角色类
    /// </summary>
    public class Player : Character
    {
        private CharacterData playerData;

        private void Awake()
        {
            // 尝试自动获取CharacterUI（如果未设置）
            if (characterUI == null)
            {
                characterUI = GetComponentInChildren<CharacterUI>();
                if (characterUI != null)
                {
                    Debug.Log($"{name} 自动找到了 CharacterUI 组件");
                }
            }
        }

        /// <summary>
        /// 使用CharacterData初始化玩家
        /// </summary>
        public void Initialize(CharacterData data)
        {
            playerData = data;
            characterId = data.id;
            
            // 确保UI引用存在
            if (characterUI == null)
            {
                characterUI = GetComponentInChildren<CharacterUI>();
            }
            
            base.Initialize(data.maxHealth, data.initialArmor, data.audioData);
        }

        public CharacterData GetPlayerData()
        {
            return playerData;
        }
    }

}

