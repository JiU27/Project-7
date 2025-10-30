using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CardBattleGame.Entities;

namespace CardBattleGame
{
    /// <summary>
    /// 角色UI组件 - 跟随角色的Canvas
    /// </summary>
    public class CharacterUI : MonoBehaviour
    {
        [Header("血条")]
        public Slider healthSlider;
        public TextMeshProUGUI healthText;

        [Header("护甲条")]
        public Slider armorSlider;
        public TextMeshProUGUI armorText;
        public GameObject armorBarObject; // 护甲条的父对象，没有护甲时隐藏

        [Header("状态效果图标")]
        public Transform statusEffectIconParent;
        public GameObject statusEffectIconPrefab;

        [Header("数字显示")]
        public TextMeshProUGUI floatingNumberText;

        private List<GameObject> currentStatusIcons = new List<GameObject>();

        /// <summary>
        /// 更新生命条
        /// </summary>
        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }

        /// <summary>
        /// 更新护甲条
        /// </summary>
        public void UpdateArmorBar(int currentArmor)
        {
            // 护甲大于0时显示护甲条
            if (armorBarObject != null)
            {
                bool shouldShow = currentArmor > 0;
                armorBarObject.SetActive(shouldShow);
            }

            if (armorSlider != null)
            {
                armorSlider.value = currentArmor;
                // 设置最大值为一个合理的值（可以根据需要调整）
                armorSlider.maxValue = Mathf.Max(100, currentArmor);
            }

            if (armorText != null)
            {
                armorText.text = currentArmor.ToString();
            }
        }

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(int damage)
        {
            ShowFloatingNumber($"-{damage}", Color.red);
        }

        /// <summary>
        /// 显示治疗数字
        /// </summary>
        public void ShowHealNumber(int heal)
        {
            ShowFloatingNumber($"+{heal}", Color.green);
        }

        /// <summary>
        /// 显示护甲数字
        /// </summary>
        public void ShowArmorNumber(int armor)
        {
            ShowFloatingNumber($"+{armor}", Color.cyan);
        }

        /// <summary>
        /// 显示浮动数字
        /// </summary>
        private void ShowFloatingNumber(string text, Color color)
        {
            if (floatingNumberText != null)
            {
                floatingNumberText.text = text;
                floatingNumberText.color = color;
                floatingNumberText.gameObject.SetActive(true);
                
                // TODO: 添加浮动动画
                StartCoroutine(HideFloatingNumberAfterDelay(1f));
            }
        }

        /// <summary>
        /// 延迟隐藏浮动数字
        /// </summary>
        private System.Collections.IEnumerator HideFloatingNumberAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (floatingNumberText != null)
            {
                floatingNumberText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 更新状态效果图标
        /// </summary>
        public void UpdateStatusEffects(List<StatusEffect> statusEffects)
        {
            // 清除现有图标
            foreach (var icon in currentStatusIcons)
            {
                Destroy(icon);
            }
            currentStatusIcons.Clear();

            // 创建新图标
            if (statusEffectIconPrefab != null && statusEffectIconParent != null)
            {
                foreach (var effect in statusEffects)
                {
                    GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectIconParent);
                    
                    // 获取StatusEffectIconUI组件
                    StatusEffectIconUI iconUI = iconObj.GetComponent<StatusEffectIconUI>();
                    if (iconUI != null)
                    {
                        // 使用StatusEffectIconUI初始化（推荐方式）
                        iconUI.Initialize(effect);
                    }
                    else
                    {
                        // 如果预制体没有StatusEffectIconUI脚本，使用旧方式
                        SetupIconManually(iconObj, effect);
                    }

                    currentStatusIcons.Add(iconObj);
                }
            }
        }

        /// <summary>
        /// 手动设置图标（兼容旧预制体）
        /// </summary>
        private void SetupIconManually(GameObject iconObj, StatusEffect effect)
        {
            // 设置图标
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null && effect.data.iconSprite != null)
            {
                iconImage.sprite = effect.data.iconSprite;
            }

            // 设置层数文本
            TextMeshProUGUI stackText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (stackText != null)
            {
                if (effect.data.canStack)
                {
                    stackText.text = effect.stacks.ToString();
                }
                else
                {
                    stackText.text = "";
                }

                // 炸弹效果显示倒计时（红色）
                if (effect.data.statusType == StatusEffectType.Bomb)
                {
                    stackText.text = effect.duration.ToString();
                    stackText.color = Color.red;
                }
            }
        }
    }
}

