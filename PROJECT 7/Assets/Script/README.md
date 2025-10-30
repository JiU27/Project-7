# Card Battle Game - 代码架构说明

## 项目概述
这是一个类似杀戮尖塔的2D回合制卡牌游戏，包含独特的怀表系统和元素反应机制。

## 文件夹结构

```
Script/
├── Enums.cs                    # 游戏枚举定义
├── DataStructures.cs           # 数据结构定义
├── Data/                       # ScriptableObject数据类
│   ├── CharacterData.cs        # 角色数据
│   ├── CardData.cs             # 卡牌数据
│   ├── MonsterData.cs          # 怪物数据
│   ├── StatusEffectData.cs     # 状态效果数据
│   ├── WatchResultData.cs      # 怀表结果数据
│   └── OutputSystemData.cs     # 输出系统数据
├── Entities/                   # 实体类
│   ├── Character.cs            # 角色基类
│   ├── Player.cs               # 玩家类
│   ├── Enemy.cs                # 敌人类
│   ├── Card.cs                 # 卡牌类
│   └── StatusEffect.cs         # 状态效果类
├── Managers/                   # 管理器
│   ├── GameManager.cs          # 游戏管理器（全局单例）
│   ├── BattleManager.cs        # 战斗管理器（核心战斗逻辑）
│   ├── CardManager.cs          # 卡牌管理器（手牌、牌库、弃牌堆）
│   ├── DeckManager.cs          # 卡组管理器（玩家卡组配置）
│   └── UIManager.cs            # UI管理器
├── Systems/                    # 系统类
│   ├── TurnSystem.cs           # 回合系统
│   ├── ActionQueueSystem.cs    # 行动队列系统
│   ├── ElementalReactionSystem.cs  # 元素反应系统
│   └── StatusEffectSystem.cs   # 状态效果系统
└── UI/                         # UI组件
    ├── CharacterUI.cs          # 角色UI
    ├── EnemyUI.cs              # 敌人UI
    ├── CardUI.cs               # 卡牌UI（支持拖拽）
    ├── OutputSlotUI.cs         # 输出槽位UI
    ├── ActionNodeUI.cs         # 行动节点UI
    └── WatchPanel.cs           # 怀表面板UI
```

## 核心系统说明

### 1. 战斗流程
战斗分为以下阶段（在`TurnSystem.cs`中定义）：
- **Preparation**: 准备阶段（展示怀表、抽牌）
- **Planning**: 规划阶段（玩家选择卡牌并放入输出槽位）
- **Combat**: 战斗阶段（按速度顺序执行所有行动）
- **Resolution**: 结算阶段（处理状态效果、弃牌）

### 2. 卡牌系统
- **抽牌堆、手牌、弃牌堆**：由`CardManager.cs`管理
- **固有卡牌**：每回合自动加入手牌，不进入弃牌堆
- **输出槽位**：玩家将卡牌拖入3个槽位，每个槽位有不同效果

### 3. 元素反应系统
4种元素（火、水、土、气）可以触发6种不同的元素反应：
- 火 x 水 = 蒸汽爆炸/雾影
- 火 x 土 = 玻璃化/熔甲
- 火 x 气 = 爆燃/焰气
- 水 x 土 = 泥沼/滋养
- 水 x 气 = 冰霜/霜甲
- 土 x 气 = 尘暴/岩肤

### 4. 状态效果系统
包含17种状态效果（详见`Enums.cs`中的`StatusEffectType`）：
- **元素残留**：火、水、土、气元素残留
- **增益状态**：力量、临时力量、铸甲、再生、坚固、隐匿
- **减益状态**：虚弱、碎甲、毒素、冰冻、失准、脆弱、炸弹

### 5. 怀表系统
每回合开始时随机抽取一个怀表结果（1-6），产生不同的战场效果：
1. 额外抽取一张卡牌
2. 为敌人挂上随机元素残留
3. 对所有敌人造成2点随机元素伤害
4. 本回合"无"类型卡牌效果提升25%
5. 获得一张临时卡牌
6. 获得随机增益状态

## 使用说明

### 创建数据资源
1. 在Unity编辑器中，右键 → Create → Card Battle Game
2. 创建以下ScriptableObject：
   - Character Data（角色数据）
   - Card Data（卡牌数据）
   - Monster Data（怪物数据）
   - Status Effect Data（状态效果数据）
   - Watch Result Data（怀表结果数据）
   - Output System Data（输出系统数据）

### 场景设置
1. 创建一个空GameObject命名为"GameManager"，挂载`GameManager.cs`
2. 创建一个空GameObject命名为"BattleManager"，挂载以下组件：
   - `BattleManager.cs`
   - `CardManager.cs`
   - `DeckManager.cs`
   - `TurnSystem.cs`
   - `ActionQueueSystem.cs`
   - `StatusEffectSystem.cs`
   - `ElementalReactionSystem.cs`
3. 创建UI Canvas，挂载`UIManager.cs`

### 角色/敌人预制体
角色和敌人的预制体应该包含：
- `Player.cs` 或 `Enemy.cs` 脚本
- `CharacterUI.cs` 或 `EnemyUI.cs` 组件（在子Canvas上）
- `Animator` 组件
- 血条、护甲条、状态图标等UI元素

### 卡牌预制体
卡牌预制体应该包含：
- `CardUI.cs` 脚本
- UI Image、TextMeshPro组件
- EventSystem支持（用于拖拽）

## 扩展功能

### 添加新卡牌
1. 创建新的CardData ScriptableObject
2. 设置卡牌属性（速度、类型、属性、效果等）
3. 将卡牌ID添加到玩家卡组中

### 添加新状态效果
1. 在`Enums.cs`中添加新的`StatusEffectType`
2. 创建StatusEffectData ScriptableObject
3. 在`StatusEffectSystem.cs`中添加该效果的逻辑
4. 在`Character.cs`中更新伤害/护甲计算逻辑（如需要）

### 添加新元素反应
1. 在`ElementalReactionSystem.cs`的`TriggerReaction`方法中添加新反应逻辑

## 注意事项

1. **命名空间**：所有脚本使用`CardBattleGame`命名空间
2. **TextMeshPro**：UI文本使用TextMeshPro（需要导入TMP包）
3. **Unity版本**：建议使用Unity 2021.3 LTS或更高版本
4. **TODO标记**：代码中有一些TODO标记，表示需要进一步实现的功能（如动画、音效等）

## 待完善功能

以下功能已在代码中预留接口，但需要进一步实现：
- [ ] 角色/敌人预制体的实例化
- [ ] 卡牌动画效果
- [ ] Perfect Deflect机制的完整实现
- [ ] 音效播放系统
- [ ] 战斗背景和特效
- [ ] 卡组编辑UI
- [ ] 技能树系统（遗物系统）
- [ ] 关卡选择和地图系统

## 联系方式
如有问题，请查阅文档或联系开发团队。

