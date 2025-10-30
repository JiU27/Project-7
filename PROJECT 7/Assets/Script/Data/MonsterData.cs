using UnityEngine;
using System.Collections.Generic;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 怪物数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Monster", menuName = "Card Battle Game/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [Header("基础信息")]
        public int id;
        public string monsterName;
        
        [Header("属性")]
        public int maxHealth;
        public int initialArmor;
        
        [Header("预制体")]
        public GameObject monsterPrefab;
        
        [Header("音效")]
        public AudioData audioData;
        
        [Header("技能列表")]
        public List<EnemySkill> skillList = new List<EnemySkill>();
        
        [Header("行动模式")]
        public EnemyActionPattern actionPattern;
    }
}

