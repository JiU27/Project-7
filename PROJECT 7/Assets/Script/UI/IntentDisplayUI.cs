using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardBattleGame.Data;

namespace CardBattleGame
{
    /// <summary>
    /// 行动意图显示组件 - 独立版本（更易于配置）
    /// </summary>
    public class IntentDisplayUI : MonoBehaviour
    {
        [Header("视觉组件")]
        public Image intentImage;                   // 意图图标
        
        [Header("文本显示")]
        public TextMeshProUGUI speedText;           // 速度值
        public TextMeshProUGUI classText;           // 类型（攻击/技能/能力）
        public TextMeshProUGUI typeText;            // 种类（Swift/Strong/Normal/None）
        public TextMeshProUGUI elementText;         // 属性（火/水/土/气等）
        public TextMeshProUGUI damageText;          // 伤害值

        [Header("图标资源（可选）")]
        public Sprite attackIcon;                   // 攻击类型图标
        public Sprite skillIcon;                    // 技能类型图标
        public Sprite abilityIcon;                  // 能力类型图标

        /// <summary>
        /// 设置意图信息
        /// </summary>
        public void SetIntent(EnemySkill skill)
        {
            Debug.Log($"设置意图：速度{skill.speedValue}, 类型{skill.cardClass}, 种类{skill.cardType}");

            // 设置速度
            if (speedText != null)
            {
                speedText.text = skill.speedValue.ToString();
            }
            else
            {
                Debug.LogWarning("Intent的Speed Text未连接！");
            }

            // 设置类型
            if (classText != null)
            {
                classText.text = GetClassDisplayText(skill.cardClass);
            }
            else
            {
                Debug.LogWarning("Intent的Class Text未连接！");
            }

            // 设置种类
            if (typeText != null)
            {
                typeText.text = GetTypeDisplayText(skill.cardType);
            }
            else
            {
                Debug.LogWarning("Intent的Type Text未连接！");
            }

            // 设置属性
            if (elementText != null)
            {
                elementText.text = GetElementDisplayText(skill.elementType);
            }
            else
            {
                Debug.LogWarning("Intent的Element Text未连接！");
            }

            // 设置伤害值
            if (damageText != null)
            {
                if (skill.dealsDamage)
                {
                    damageText.text = skill.damageAmount.ToString();
                    damageText.gameObject.SetActive(true);
                }
                else
                {
                    damageText.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Intent的Damage Text未连接！");
            }

            // 设置意图图标
            if (intentImage != null)
            {
                Sprite icon = GetIntentIcon(skill.cardClass);
                if (icon != null)
                {
                    intentImage.sprite = icon;
                }
            }

            Debug.Log($"Intent更新完成：{skill.cardClass} - 速度{skill.speedValue}");
        }

        /// <summary>
        /// 获取类型显示文字
        /// </summary>
        private string GetClassDisplayText(CardClass cardClass)
        {
            switch (cardClass)
            {
                case CardClass.Attack: return "Attack";
                case CardClass.Skill: return "Skill";
                case CardClass.Ability: return "Ability";
                default: return cardClass.ToString();
            }
        }

        /// <summary>
        /// 获取种类显示文字
        /// </summary>
        private string GetTypeDisplayText(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Swift: return "Swift";
                case CardType.Strong: return "Strong";
                case CardType.Normal: return "Nromal";
                case CardType.None: return "None";
                default: return cardType.ToString();
            }
        }

        /// <summary>
        /// 获取属性显示文字
        /// </summary>
        private string GetElementDisplayText(ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Fire: return "Fire";
                case ElementType.Water: return "Water";
                case ElementType.Earth: return "Earth";
                case ElementType.Air: return "Air";
                case ElementType.Physical: return "Physical";
                case ElementType.Chaos: return "Chaos";
                default: return elementType.ToString();
            }
        }

        /// <summary>
        /// 获取意图图标
        /// </summary>
        private Sprite GetIntentIcon(CardClass cardClass)
        {
            switch (cardClass)
            {
                case CardClass.Attack: return attackIcon;
                case CardClass.Skill: return skillIcon;
                case CardClass.Ability: return abilityIcon;
                default: return null;
            }
        }
    }
}

