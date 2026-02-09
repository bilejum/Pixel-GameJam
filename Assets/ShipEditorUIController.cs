using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ShipEditorUIController : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Tab;

    private VisualElement _root;
    private VisualElement _leftPanel;
    private VisualElement _rightPanel;
    private VisualElement _inventoryGrid;
    private VisualElement _synthesisList; // 合成表列表容器
    private Button _toggleLeftBtn; // 左侧面板隐藏按钮

    // 功能按钮引用
    private Button _btnDeleteBlock;
    private Button _btnClearSlot;
    private Button _btnExitGame;

    private bool _isVisible = false;
    private bool _isLeftPanelShown = true; // 左侧面板是否显示

    // 【修改1】扩展ItemData结构体，增加图片参数
    private struct ItemData
    {
        public string name;     // 物品名称
        public int count;       // 物品数量
        public Sprite icon;     // 物品图片（Sprite类型）
    }
    private List<ItemData> _items = new List<ItemData>();

    // 【修改2】扩展合成表结构体，适配图片显示
    private struct SynthesisRecipe
    {
        public string name;     // 合成配方名称
        public string input1;   // 输入物品1名称
        public string input2;   // 输入物品2名称
        public string result;   // 合成结果名称
        // 可选：给合成表也添加图片参数（示例用名称匹配物品图片）
        public Sprite input1Icon;
        public Sprite input2Icon;
        public Sprite resultIcon;
    }
    private List<SynthesisRecipe> _recipes = new List<SynthesisRecipe>();

    // 示例：默认物品图片（用于图片缺失时的占位）
    private Sprite _defaultItemIcon;

    void OnEnable()
    {
        // 获取根元素
        _root = GetComponent<UIDocument>().rootVisualElement;

        // 初始化面板引用（收窄后的面板）
        _leftPanel = _root.Q<VisualElement>("left-panel-container");
        _rightPanel = _root.Q<VisualElement>("right-panel-container");
        _inventoryGrid = _root.Q<VisualElement>("inventory-grid");
        _synthesisList = _root.Q<VisualElement>("synthesis-list");

        // 初始化按钮引用
        _toggleLeftBtn = _root.Q<Button>("toggle-left-btn");
        _btnDeleteBlock = _root.Q<Button>("btn-delete-block");
        _btnClearSlot = _root.Q<Button>("btn-clear-slot");
        _btnExitGame = _root.Q<Button>("btn-exit-game");

        // 关键：确保按钮可拾取（防止picking-mode被覆盖）
        _toggleLeftBtn.pickingMode = PickingMode.Position;

        // 加载默认占位图片（需你在Resources/Icons下放置default_icon.png）
        _defaultItemIcon = Resources.Load<Sprite>("Icons/default_icon");

        // 绑定按钮事件
        BindButtonEvents();

        // 填充模拟数据（包含图片）
        FillMockData();
        // 生成合成表（多个，带图片）
        GenerateSynthesisRecipes();
        // 生成背包格子（显示图片）
        RefreshInventory();

        // 初始化UI状态
        UpdateUI();
    }

    void Update()
    {
        // 全局显示/隐藏UI
        if (Input.GetKeyDown(toggleKey))
        {
            _isVisible = !_isVisible;
            UpdateUI();
            // 全局显示时，恢复左侧面板的显示状态
            if (_isVisible)
            {
                if (_isLeftPanelShown)
                {
                    _leftPanel.RemoveFromClassList("panel-hidden-left");
                    _leftPanel.AddToClassList("panel-visible");
                    _toggleLeftBtn.text = "←";
                }
                else
                {
                    _leftPanel.RemoveFromClassList("panel-visible");
                    _leftPanel.AddToClassList("panel-hidden-left");
                    _toggleLeftBtn.text = "→";
                }
            }
        }
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        // 左侧面板隐藏/显示按钮（修复逻辑，确保双向切换）
        _toggleLeftBtn.clicked += () => {
            _isLeftPanelShown = !_isLeftPanelShown;

            // 移除所有相关类，避免样式冲突
            _leftPanel.RemoveFromClassList("panel-hidden-left");
            _leftPanel.RemoveFromClassList("panel-visible");

            if (_isLeftPanelShown)
            {
                _leftPanel.AddToClassList("panel-visible");
                _toggleLeftBtn.text = "←";
            }
            else
            {
                _leftPanel.AddToClassList("panel-hidden-left");
                _toggleLeftBtn.text = "→";
            }
        };

        // 右侧功能按钮
        _btnDeleteBlock.clicked += OnDeleteBlockClicked;
        _btnClearSlot.clicked += OnClearSlotClicked;
        _btnExitGame.clicked += OnExitGameClicked;
    }

    /// <summary>
    /// 填充模拟数据（包含图片）
    /// </summary>
    private void FillMockData()
    {
        _items.Clear();

        // 模拟物品数据（需你在Resources/Icons下放置对应名称的图片，如red_block.png）
        _items.Add(new ItemData
        {
            name = "红方块",
            count = 5,
            icon = Resources.Load<Tile>("Prefabs/Tiles/Red Tile").GetComponent<SpriteRenderer>().sprite ?? _defaultItemIcon
        });
        _items.Add(new ItemData
        {
            name = "蓝方块",
            count = 3,
            icon = Resources.Load<Sprite>("Icons/blue_block") ?? _defaultItemIcon
        });
        _items.Add(new ItemData
        {
            name = "黄方块",
            count = 8,
            icon = Resources.Load<Sprite>("Icons/yellow_block") ?? _defaultItemIcon
        });
        _items.Add(new ItemData
        {
            name = "铁",
            count = 12,
            icon = Resources.Load<Sprite>("Icons/iron") ?? _defaultItemIcon
        });
        _items.Add(new ItemData
        {
            name = "能量核心",
            count = 2,
            icon = Resources.Load<Sprite>("Icons/energy_core") ?? _defaultItemIcon
        });

        // 合成表数据（带图片）
        _recipes.Clear();
        _recipes.Add(new SynthesisRecipe
        {
            name = "基础炮台",
            input1 = "红方块",
            input2 = "铁",
            result = "激光炮",
            input1Icon = Resources.Load<Tile>("Prefabs/Tiles/Red Tile").GetComponent<SpriteRenderer>().sprite ?? _defaultItemIcon,
            input2Icon = Resources.Load<Sprite>("Icons/iron") ?? _defaultItemIcon,
            resultIcon = Resources.Load<Sprite>("Icons/laser_cannon") ?? _defaultItemIcon
        });
        _recipes.Add(new SynthesisRecipe
        {
            name = "护盾发生器",
            input1 = "蓝方块",
            input2 = "能量核心",
            result = "高级护盾",
            input1Icon = Resources.Load<Sprite>("Icons/blue_block") ?? _defaultItemIcon,
            input2Icon = Resources.Load<Sprite>("Icons/energy_core") ?? _defaultItemIcon,
            resultIcon = Resources.Load<Sprite>("Icons/advanced_shield") ?? _defaultItemIcon
        });
    }

    /// <summary>
    /// 生成多个合成表项（带图片显示）
    /// </summary>
    private void GenerateSynthesisRecipes()
    {
        _synthesisList.Clear();

        foreach (var recipe in _recipes)
        {
            // 合成表项容器
            VisualElement recipeItem = new VisualElement();
            recipeItem.style.flexDirection = FlexDirection.Row;
            recipeItem.style.justifyContent = Justify.Center;
            recipeItem.style.alignItems = Align.Center;

            // 设置内边距（兼容所有Unity版本）
            recipeItem.style.paddingTop = 10;
            recipeItem.style.paddingBottom = 10;
            recipeItem.style.paddingLeft = 10;
            recipeItem.style.paddingRight = 10;

            // 背景色
            recipeItem.style.backgroundColor = new Color(1, 1, 1, 0.05f);

            // 圆角
            recipeItem.style.borderTopLeftRadius = 4;
            recipeItem.style.borderTopRightRadius = 4;
            recipeItem.style.borderBottomLeftRadius = 4;
            recipeItem.style.borderBottomRightRadius = 4;

            // 输入格1（带图片）
            VisualElement input1 = CreateSlotWithIcon(recipe.input1Icon, recipe.input1);
            input1.style.marginRight = 10;

            // + 号
            Label plusLabel = new Label("+");
            plusLabel.style.fontSize = 16;
            plusLabel.style.color = Color.white;
            plusLabel.style.marginRight = 10;

            // 输入格2（带图片）
            VisualElement input2 = CreateSlotWithIcon(recipe.input2Icon, recipe.input2);
            input2.style.marginRight = 10;

            // = 号
            Label equalLabel = new Label("=");
            equalLabel.style.fontSize = 16;
            equalLabel.style.color = Color.white;
            equalLabel.style.marginRight = 10;

            // 结果格（带图片+高亮）
            VisualElement result = CreateSlotWithIcon(recipe.resultIcon, recipe.result);
            result.AddToClassList("highlight");

            // 组装合成表项
            recipeItem.Add(input1);
            recipeItem.Add(plusLabel);
            recipeItem.Add(input2);
            recipeItem.Add(equalLabel);
            recipeItem.Add(result);

            // 添加到合成表列表
            _synthesisList.Add(recipeItem);
        }
    }

    /// <summary>
    /// 刷新背包（显示物品图片）
    /// </summary>
    private void RefreshInventory()
    {
        _inventoryGrid.Clear();

        foreach (var item in _items)
        {
            // 创建带图片的格子
            VisualElement slot = CreateSlotWithIcon(item.icon, item.name);

            // 数量标签（仅数量>0时显示）
            if (item.count > 0)
            {
                Label countLabel = new Label(item.count.ToString());
                countLabel.AddToClassList("slot-count");
                slot.Add(countLabel);
            }

            // 点击事件
            slot.RegisterCallback<ClickEvent>(evt => OnSlotClicked(item));

            _inventoryGrid.Add(slot);
        }
    }

    /// <summary>
    /// 【工具方法】创建带图片的格子
    /// </summary>
    /// <param name="icon">格子显示的图片</param>
    /// <param name="labelText">图片下方的文本（可选）</param>
    private VisualElement CreateSlotWithIcon(Sprite icon, string labelText = "")
    {
        VisualElement slot = new VisualElement();
        slot.AddToClassList("slot-square");

        // 添加图片元素
        Image iconImage = new Image();
        iconImage.sprite = icon ?? _defaultItemIcon; // 图片为空时用默认占位图
        // 适配格子大小：图片填充整个格子，保持比例
        iconImage.style.width = Length.Percent(90);
        iconImage.style.height = Length.Percent(90);
        //iconImage.style.objectFit = ObjectFit.ScaleDown; // 保持图片比例
        slot.Add(iconImage);

        // 可选：添加文本标签（合成表用，背包格子可省略）
        if (!string.IsNullOrEmpty(labelText))
        {
            Label textLabel = new Label(labelText);
            textLabel.style.fontSize = 8;
            textLabel.style.color = Color.white;
            textLabel.style.position = Position.Absolute;
            textLabel.style.bottom = 0;
            textLabel.style.unityTextAlign = TextAnchor.LowerCenter;
            slot.Add(textLabel);
        }

        return slot;
    }

    /// <summary>
    /// 【新增接口】拾取物品并添加到物品栏（外部可调用）
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="count">添加数量</param>
    /// <param name="itemIcon">物品图片（可选）</param>
    public void AddItemToInventory(string itemName, int count, Sprite itemIcon = null)
    {
        if (count <= 0) return; // 数量≤0时不处理

        // 查找物品栏中是否已有该物品
        int existingIndex = -1;
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].name == itemName)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            // 已有该物品：叠加数量
            ItemData updatedItem = _items[existingIndex];
            updatedItem.count += count;
            _items[existingIndex] = updatedItem;
        }
        else
        {
            // 无该物品：新增
            _items.Add(new ItemData
            {
                name = itemName,
                count = count,
                icon = itemIcon ?? _defaultItemIcon
            });
        }

        // 刷新UI显示
        RefreshInventory();
        Debug.Log($"拾取物品：{itemName} × {count}，当前数量：{_items[existingIndex >= 0 ? existingIndex : _items.Count - 1].count}");
    }

    /// <summary>
    /// 更新整体UI显示/隐藏状态
    /// </summary>
    private void UpdateUI()
    {
        if (_isVisible)
        {
            _rightPanel.RemoveFromClassList("panel-hidden-right");
            _rightPanel.AddToClassList("panel-visible");
            // 左侧面板根据自身状态显示
            if (_isLeftPanelShown)
            {
                _leftPanel.AddToClassList("panel-visible");
            }
        }
        else
        {
            _leftPanel.RemoveFromClassList("panel-visible");
            _rightPanel.RemoveFromClassList("panel-visible");
            _leftPanel.AddToClassList("panel-hidden-left");
            _rightPanel.AddToClassList("panel-hidden-right");
        }
    }

    // 事件回调：背包格子点击
    private void OnSlotClicked(ItemData item)
    {
        Debug.Log($"Selected Item: {item.name}, Count: {item.count}");
    }

    // 事件回调：删除方块
    private void OnDeleteBlockClicked()
    {
        Debug.Log("进入删除方块模式");
        // 示例：调用拾取接口（测试）
        // AddItemToInventory("测试物品", 1, Resources.Load<Sprite>("Icons/test_icon"));
    }

    // 事件回调：清空格子
    private void OnClearSlotClicked()
    {
        Debug.Log("清空合成槽");
        // 这里添加清空合成槽的逻辑
    }

    // 事件回调：退出游戏
    private void OnExitGameClicked()
    {
        Debug.Log("退出游戏");
        // 编辑器下退出播放模式，打包后退出应用
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}