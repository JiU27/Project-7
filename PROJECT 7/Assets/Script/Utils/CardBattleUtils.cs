using UnityEngine;
using System.Collections.Generic;

namespace CardBattleGame.Utils
{
    /// <summary>
    /// 工具类 - 提供常用的辅助方法
    /// </summary>
    public static class CardBattleUtils
    {
        /// <summary>
        /// 获取元素的颜色（用于UI显示）
        /// </summary>
        public static Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire:
                    return new Color(1f, 0.3f, 0f); // 橙红色
                case ElementType.Water:
                    return new Color(0f, 0.5f, 1f); // 蓝色
                case ElementType.Earth:
                    return new Color(0.6f, 0.4f, 0.2f); // 棕色
                case ElementType.Air:
                    return new Color(0.8f, 1f, 1f); // 浅青色
                case ElementType.Physical:
                    return Color.white; // 白色
                case ElementType.Chaos:
                    return new Color(0.5f, 0f, 0.5f); // 紫色
                default:
                    return Color.gray;
            }
        }

        /// <summary>
        /// 获取卡牌种类的颜色
        /// </summary>
        public static Color GetCardTypeColor(CardType type)
        {
            switch (type)
            {
                case CardType.Swift:
                    return new Color(1f, 1f, 0f); // 黄色
                case CardType.Strong:
                    return new Color(1f, 0f, 0f); // 红色
                case CardType.Normal:
                    return new Color(0f, 1f, 0f); // 绿色
                case CardType.None:
                    return new Color(0.7f, 0.7f, 0.7f); // 灰色
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 获取状态效果的类型（增益/减益/元素残留）
        /// </summary>
        public static string GetStatusEffectCategory(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.None:
                    return "无";

                case StatusEffectType.FireResidue:
                case StatusEffectType.WaterResidue:
                case StatusEffectType.EarthResidue:
                case StatusEffectType.AirResidue:
                    return "元素残留";

                case StatusEffectType.Strength:
                case StatusEffectType.TempStrength:
                case StatusEffectType.Fortify:
                case StatusEffectType.Regeneration:
                case StatusEffectType.Sturdy:
                case StatusEffectType.Stealth:
                    return "增益状态";

                case StatusEffectType.Weak:
                case StatusEffectType.Fragile:
                case StatusEffectType.Poison:
                case StatusEffectType.Frozen:
                case StatusEffectType.Miss:
                case StatusEffectType.Vulnerable:
                case StatusEffectType.Bomb:
                    return "减益状态";

                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 计算百分比修正后的值（向上取整）
        /// </summary>
        public static int ApplyPercentageBonus(int baseValue, float percentage)
        {
            return Mathf.CeilToInt(baseValue * percentage);
        }

        /// <summary>
        /// 计算百分比减少后的值（向下取整，最低为1）
        /// </summary>
        public static int ApplyPercentagePenalty(int baseValue, float percentage)
        {
            return Mathf.Max(1, Mathf.FloorToInt(baseValue * percentage));
        }

        /// <summary>
        /// 洗牌（Fisher-Yates算法）
        /// </summary>
        public static void Shuffle<T>(List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// 获取随机元素
        /// </summary>
        public static ElementType GetRandomElement()
        {
            ElementType[] elements = { ElementType.Fire, ElementType.Water, ElementType.Earth, ElementType.Air };
            return elements[Random.Range(0, elements.Length)];
        }

        /// <summary>
        /// 获取随机增益状态
        /// </summary>
        public static StatusEffectType GetRandomBuff()
        {
            StatusEffectType[] buffs = { StatusEffectType.Strength, StatusEffectType.Fortify, StatusEffectType.Regeneration };
            return buffs[Random.Range(0, buffs.Length)];
        }

        /// <summary>
        /// 检查是否为元素残留
        /// </summary>
        public static bool IsElementalResidue(StatusEffectType type)
        {
            return type == StatusEffectType.FireResidue ||
                   type == StatusEffectType.WaterResidue ||
                   type == StatusEffectType.EarthResidue ||
                   type == StatusEffectType.AirResidue;
        }

        /// <summary>
        /// 检查是否为增益状态
        /// </summary>
        public static bool IsBuff(StatusEffectType type)
        {
            return type == StatusEffectType.Strength ||
                   type == StatusEffectType.TempStrength ||
                   type == StatusEffectType.Fortify ||
                   type == StatusEffectType.Regeneration ||
                   type == StatusEffectType.Sturdy ||
                   type == StatusEffectType.Stealth;
        }

        /// <summary>
        /// 检查是否为减益状态
        /// </summary>
        public static bool IsDebuff(StatusEffectType type)
        {
            return type == StatusEffectType.Weak ||
                   type == StatusEffectType.Fragile ||
                   type == StatusEffectType.Poison ||
                   type == StatusEffectType.Frozen ||
                   type == StatusEffectType.Miss ||
                   type == StatusEffectType.Vulnerable ||
                   type == StatusEffectType.Bomb;
        }
    }
}

