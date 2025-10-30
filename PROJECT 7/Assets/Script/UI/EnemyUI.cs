using UnityEngine;
using TMPro;
using System.Collections.Generic;
using CardBattleGame.Entities;

namespace CardBattleGame
{
    /// <summary>
    /// 敌人UI组件 - 继承自CharacterUI，增加行动意图显示
    /// </summary>
    public class EnemyUI : CharacterUI
    {
        [Header("行动意图")]
        public Transform intentParent;
        public GameObject intentPrefab;

        [Header("瞄准图标")]
        public GameObject aimIcon;

        private List<GameObject> currentIntents = new List<GameObject>();

        /// <summary>
        /// 更新行动意图显示
        /// </summary>
        public void UpdateIntents(List<EnemySkill> skills)
        {
            // 清除现有意图
            foreach (var intent in currentIntents)
            {
                Destroy(intent);
            }
            currentIntents.Clear();

            // 创建新意图（最多3个）
            if (intentPrefab != null && intentParent != null)
            {
                int count = Mathf.Min(skills.Count, 3);
                for (int i = 0; i < count; i++)
                {
                    GameObject intentObj = Instantiate(intentPrefab, intentParent);
                    
                    // 优先使用IntentDisplayUI（独立脚本）
                    IntentDisplayUI intentDisplayUI = intentObj.GetComponent<IntentDisplayUI>();
                    if (intentDisplayUI != null)
                    {
                        intentDisplayUI.SetIntent(skills[i]);
                    }
                    else
                    {
                        // 兼容旧的IntentDisplay（嵌套类）
                        IntentDisplay intentDisplay = intentObj.GetComponent<IntentDisplay>();
                        if (intentDisplay != null)
                        {
                            intentDisplay.SetIntent(skills[i]);
                        }
                        else
                        {
                            Debug.LogWarning($"IntentPrefab上没有IntentDisplayUI或IntentDisplay脚本！");
                        }
                    }

                    currentIntents.Add(intentObj);
                }
            }
            else
            {
                Debug.LogWarning($"无法显示 {gameObject.name} 的行动意图：Intent Prefab或Intent Parent未设置");
            }
        }

        /// <summary>
        /// 显示瞄准图标
        /// </summary>
        public void ShowAimIcon(bool show)
        {
            if (aimIcon != null)
            {
                aimIcon.SetActive(show);
            }
        }
    }

    /// <summary>
    /// 行动意图显示组件
    /// </summary>
    public class IntentDisplay : MonoBehaviour
    {
        public UnityEngine.UI.Image intentImage;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI classText;
        public TextMeshProUGUI typeText;
        public TextMeshProUGUI elementText;
        public TextMeshProUGUI damageText;

        public void SetIntent(EnemySkill skill)
        {
            // TODO: 根据技能类型设置意图图标
            
            if (speedText != null)
            {
                speedText.text = skill.speedValue.ToString();
            }

            if (classText != null)
            {
                classText.text = skill.cardClass.ToString();
            }

            if (typeText != null)
            {
                typeText.text = skill.cardType.ToString();
            }

            if (elementText != null)
            {
                elementText.text = skill.elementType.ToString();
            }

            if (damageText != null && skill.dealsDamage)
            {
                damageText.text = skill.damageAmount.ToString();
            }
            else if (damageText != null)
            {
                damageText.gameObject.SetActive(false);
            }
        }
    }
}

