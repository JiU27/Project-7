using UnityEngine;

namespace CardBattleGame.Data
{
    /// <summary>
    /// 角色数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "Card Battle Game/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基础信息")]
        public int id;
        public string characterName;
        
        [Header("属性")]
        public int maxHealth;
        public int initialArmor;
        
        [Header("预制体")]
        public GameObject characterPrefab;
        
        [Header("音效")]
        public AudioData audioData;
    }
}

