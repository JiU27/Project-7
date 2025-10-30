using UnityEngine;
using CardBattleGame.Data;

namespace CardBattleGame.Entities
{
    /// <summary>
    /// 状态效果实体类
    /// </summary>
    public class StatusEffect
    {
        public StatusEffectData data { get; private set; }
        public int stacks { get; private set; }
        public int duration { get; private set; }  // 持续回合数（某些效果使用）

        public StatusEffect(StatusEffectData data, int initialStacks = 1, int duration = -1)
        {
            this.data = data;
            this.stacks = Mathf.Clamp(initialStacks, 1, data.maxStacks);
            this.duration = duration;
        }

        /// <summary>
        /// 添加层数
        /// </summary>
        public void AddStacks(int amount)
        {
            if (!data.canStack)
            {
                stacks = 1;
                return;
            }

            stacks = Mathf.Clamp(stacks + amount, 1, data.maxStacks);
        }

        /// <summary>
        /// 移除层数
        /// </summary>
        public void RemoveStacks(int amount)
        {
            stacks = Mathf.Max(0, stacks - amount);
        }

        /// <summary>
        /// 设置层数
        /// </summary>
        public void SetStacks(int amount)
        {
            stacks = Mathf.Clamp(amount, 0, data.maxStacks);
        }

        /// <summary>
        /// 减少持续时间
        /// </summary>
        public void DecrementDuration()
        {
            if (duration > 0)
            {
                duration--;
            }
        }

        /// <summary>
        /// 是否应该被移除
        /// </summary>
        public bool ShouldBeRemoved()
        {
            return stacks <= 0 || duration == 0;
        }
    }
}

