using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinArtifactTool
{
    public partial class ArtifactUpgradeForm : Form
    {
        private string _position;
        private string _mainStat;
        private List<string> _subStats;
        private int _currentLevel = 0;
        private int _maxLevel = 20;
        private TableLayoutPanel mainLayoutPanel;
        private FlowLayoutPanel buttonLayoutPanel;
        private TableLayoutPanel bottomPanel;

        // 升级所需摩拉
        private readonly int[] _upgradeCosts = {
            0, 2000, 4000, 6000, 8000,    // 0-4级
            12000, 16000, 20000, 24000, 30000, // 5-9级
            36000, 42000, 50000, 60000, 70000, // 10-14级
            80000, 90000, 100000, 120000, 150000 // 15-20级
        };

        public ArtifactUpgradeForm()
        {
            InitializeComponent();
            InitializeUI(); // 确保UI始终初始化
        }

        public ArtifactUpgradeForm(string position, string mainStat, List<string> subStats)
        {
            InitializeComponent();
            InitializeUI(); // 确保UI始终初始化

            // 初始化数据
            _position = position;
            _mainStat = mainStat;
            _subStats = new List<string>(subStats);

            InitializeSubStats();
            UpdateUI();
        }

        private void ArtifactUpgradeForm_Resize(object sender, EventArgs e)
        {
            // 窗口大小变化时更新UI布局
            UpdateButtonLayout();
        }

        // 添加布局更新方法
        private void UpdateButtonLayout()
        {
            if (buttonLayoutPanel == null || buttonLayoutPanel.Controls.Count == 0) return;

            int availableWidth = buttonLayoutPanel.Width - buttonLayoutPanel.Padding.Horizontal;
            int buttonCount = buttonLayoutPanel.Controls.Count;

            if (buttonCount > 0)
            {
                int buttonWidth = availableWidth / buttonCount - 10; // 每个按钮的宽度
                int buttonPadding = (availableWidth - buttonCount * buttonWidth) / (buttonCount + 1); // 按钮之间的间距

                for (int i = 0; i < buttonCount; i++)
                {
                    Button button = (Button)buttonLayoutPanel.Controls[i];
                    button.Width = buttonWidth;
                    button.Margin = new Padding(buttonPadding, 5, 5, 5);
                }
            }
        }

        // 升级时副词条提升值（简化版）
        private readonly Dictionary<string, float[]> _statImprovements = new Dictionary<string, float[]>()
        {
            { "攻击力百分比", new float[] { 4.1f, 4.7f, 5.3f, 5.8f } },
            { "防御力百分比", new float[] { 5.1f, 5.8f, 6.6f, 7.3f } },
            { "生命值百分比", new float[] { 4.1f, 4.7f, 5.3f, 5.8f } },
            { "元素充能效率", new float[] { 4.5f, 5.2f, 5.8f, 6.5f } },
            { "元素精通", new float[] { 16, 19, 21, 23 } },
            { "暴击率", new float[] { 2.7f, 2.1f, 2.5f, 2.8f } },
            { "暴击伤害", new float[] { 5.4f, 6.2f, 7.0f, 7.8f } },
            { "攻击力", new float[] { 14, 16, 18, 19 } },
            { "防御力", new float[] { 16, 19, 21, 23 } },
            { "生命值", new float[] { 209, 239, 269, 299 } }
        };

        private readonly Dictionary<string, string> _statUnits = new Dictionary<string, string>()
        {
            { "攻击力百分比", "%" },
            { "防御力百分比", "%" },
            { "生命值百分比", "%" },
            { "元素充能效率", "%" },
            { "元素精通", "" },
            { "暴击率", "%" },
            { "暴击伤害", "%" },
            { "攻击力", "" },
            { "防御力", "" },
            { "生命值", "" }
        };

        // 副词条当前值
        private List<float> _subStatValues = new List<float>();
        // 副词条强化等级
        private List<int> _subStatUpgradeLevels = new List<int>();

        // 界面控件
        private PictureBox artifactImage;
        private Label lblPosition, lblLevel, lblMainStat, lblCost;
        private ListBox lstSubStats;
        private Button btnUpgrade;
        private ProgressBar pbUpgrade;
        private Button btnUpgrade4, btnUpgradeMax;

        // 类级别的随机数生成器
        private Random _random = new Random();

        // 可用副词条池
        private List<string> _availableSubstats = new List<string>();

        // 初始化UI值
        private void InitializeUI()
        {
            // 检查是否已经初始化，避免重复创建
            if (mainLayoutPanel != null) return;

            // 清除所有现有控件（避免重复添加）
            this.Controls.Clear();

            // 创建主布局面板
            mainLayoutPanel = new TableLayoutPanel();
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.ColumnCount = 1; // 简化为单栏布局，更易管理
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            this.Controls.Add(mainLayoutPanel);

            // 图片框（设置为居中对齐）
            artifactImage = new PictureBox();
            artifactImage.BackColor = Color.White;
            artifactImage.BorderStyle = BorderStyle.FixedSingle;
            artifactImage.SizeMode = PictureBoxSizeMode.Zoom;
            artifactImage.Anchor = AnchorStyles.None; // 居中锚定
            mainLayoutPanel.Controls.Add(artifactImage, 0, 0);

            // 居中图片框的替代方法
            mainLayoutPanel.GetControlFromPosition(0, 0).Location = new Point(
                (mainLayoutPanel.Width - artifactImage.Width) / 2,
                (mainLayoutPanel.GetRowHeights()[0] - artifactImage.Height) / 2);

            // 信息面板（合并位置、等级和主词条）
            var infoPanel = new Panel();
            infoPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Controls.Add(infoPanel, 0, 1);

            // 位置标签
            lblPosition = new Label();
            lblPosition.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            lblPosition.TextAlign = ContentAlignment.MiddleCenter;
            lblPosition.Dock = DockStyle.Top;
            lblPosition.Height = 30;
            infoPanel.Controls.Add(lblPosition);

            // 主词条标签
            lblMainStat = new Label();
            lblMainStat.Font = new Font("微软雅黑", 13F);
            lblMainStat.TextAlign = ContentAlignment.MiddleCenter;
            lblMainStat.Dock = DockStyle.Top;
            lblMainStat.Height = 30;
            infoPanel.Controls.Add(lblMainStat);

            // 等级标签
            lblLevel = new Label();
            lblLevel.Font = new Font("微软雅黑", 12F);
            lblLevel.TextAlign = ContentAlignment.MiddleCenter;
            lblLevel.Dock = DockStyle.Top;
            lblLevel.Height = 30;
            infoPanel.Controls.Add(lblLevel);



            // 副词条列表
            lstSubStats = new ListBox();
            lstSubStats.BackColor = this.BackColor;
            lstSubStats.BorderStyle = BorderStyle.None;
            lstSubStats.Font = new Font("微软雅黑", 11F);
            lstSubStats.ItemHeight = 30;
            lstSubStats.Dock = DockStyle.Fill;
            lstSubStats.DrawMode = DrawMode.OwnerDrawFixed; // 启用自定义绘制
            lstSubStats.DrawItem += LstSubStats_DrawItem; // 添加绘制事件处理
            mainLayoutPanel.Controls.Add(lstSubStats, 0, 2);

            // 底部面板
            bottomPanel = new TableLayoutPanel();
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.RowCount = 3;
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.Controls.Add(bottomPanel, 0, 3);

            // 进度条
            pbUpgrade = new ProgressBar();
            pbUpgrade.Maximum = 20;
            pbUpgrade.Dock = DockStyle.Fill;
            pbUpgrade.Width = 200;
            bottomPanel.Controls.Add(pbUpgrade, 0, 0);

            // 升级成本标签
            lblCost = new Label();
            lblCost.Font = new Font("微软雅黑", 10F);
            lblCost.TextAlign = ContentAlignment.MiddleCenter;
            lblCost.Dock = DockStyle.Fill;
            bottomPanel.Controls.Add(lblCost, 0, 1);

            // 按钮面板
            buttonLayoutPanel = new FlowLayoutPanel();
            buttonLayoutPanel.AutoSize = true;
            buttonLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonLayoutPanel.Padding = new Padding(5);
            buttonLayoutPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonLayoutPanel.WrapContents = false; // 禁止换行
            buttonLayoutPanel.HorizontalScroll.Visible = false; // 隐藏滚动条
            buttonLayoutPanel.Dock = DockStyle.Top;

            // 创建一个容器面板来居中按钮面板
            var buttonContainer = new Panel();
            buttonContainer.Dock = DockStyle.Fill;
            buttonContainer.AutoSize = true;
            buttonContainer.AutoScroll = false;
            buttonContainer.Controls.Add(buttonLayoutPanel);
            bottomPanel.Controls.Add(buttonContainer, 0, 2);

            // 升级按钮
            btnUpgrade = new Button();
            btnUpgrade.BackColor = Color.FromArgb(231, 76, 60);
            btnUpgrade.FlatAppearance.BorderSize = 0;
            btnUpgrade.FlatStyle = FlatStyle.Flat;
            btnUpgrade.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            btnUpgrade.ForeColor = Color.White;
            btnUpgrade.Text = "升级";
            btnUpgrade.Width = 100;
            btnUpgrade.Height = 30;
            btnUpgrade.Click += BtnUpgrade_Click;
            buttonLayoutPanel.Controls.Add(btnUpgrade);

            // 升四级按钮
            btnUpgrade4 = new Button();
            btnUpgrade4.BackColor = Color.FromArgb(52, 152, 219);
            btnUpgrade4.FlatAppearance.BorderSize = 0;
            btnUpgrade4.FlatStyle = FlatStyle.Flat;
            btnUpgrade4.Font = new Font("微软雅黑", 11F);
            btnUpgrade4.ForeColor = Color.White;
            btnUpgrade4.Text = "升四级";
            btnUpgrade4.Width = 120; // 调整宽度
            btnUpgrade4.Height = 30;
            btnUpgrade4.Click += BtnUpgrade4_Click;
            buttonLayoutPanel.Controls.Add(btnUpgrade4);

            // 升满级按钮
            btnUpgradeMax = new Button();
            btnUpgradeMax.BackColor = Color.FromArgb(46, 204, 113);
            btnUpgradeMax.FlatAppearance.BorderSize = 0;
            btnUpgradeMax.FlatStyle = FlatStyle.Flat;
            btnUpgradeMax.Font = new Font("微软雅黑", 11F);
            btnUpgradeMax.ForeColor = Color.White;
            btnUpgradeMax.Text = "升满级";
            btnUpgradeMax.Width = 100;
            btnUpgradeMax.Height = 30;
            btnUpgradeMax.Click += BtnUpgradeMax_Click;
            buttonLayoutPanel.Controls.Add(btnUpgradeMax);

            // 窗口大小变化时更新布局
            this.Resize += ArtifactUpgradeForm_Resize;
        }
        private void LstSubStats_DrawItem(object sender, DrawItemEventArgs e)
        {
            // 确保有要绘制的项
            if (e.Index >= 0 && e.Index < lstSubStats.Items.Count)
            {
                // 绘制背景
                e.DrawBackground();

                // 获取项的文本
                string itemText = lstSubStats.Items[e.Index].ToString();

                // 设置文本格式为居中对齐
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center; // 水平居中
                format.LineAlignment = StringAlignment.Center; // 垂直居中

                // 获取文本颜色
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? SystemColors.HighlightText
                    : lstSubStats.ForeColor;

                // 绘制文本
                e.Graphics.DrawString(itemText, lstSubStats.Font,
                    new SolidBrush(textColor), e.Bounds, format);

                // 绘制焦点矩形
                e.DrawFocusRectangle();
            }
        }
        // 更新UI显示
        private void UpdateUI()
        {
            // 更新副词条列表
            lstSubStats.Items.Clear();
            for (int i = 0; i < _subStats.Count; i++)
            {
                string stat = _subStats[i];
                float value = _subStatValues[i];
                int upgradeLevel = _subStatUpgradeLevels[i];

                string unit = _statUnits.ContainsKey(stat) ? _statUnits[stat] : "";
                string displayValue = $"{Math.Round(value, 1)}{unit}";

                string upgradeMark = upgradeLevel > 0 ? $" (+{upgradeLevel}次强化)" : "";
                bool isNewlyAdded = upgradeLevel == 0 && _subStats.Count > 4;
                string newMark = isNewlyAdded ? " [新解锁]" : "";

                lstSubStats.Items.Add($"{stat}: {displayValue}{upgradeMark}{newMark}");
            }

            // 更新其他显示文本
            lblPosition.Text = _position;
            lblLevel.Text = $"等级: +{_currentLevel}";
            lblMainStat.Text = $"主词条: {_mainStat}";
            pbUpgrade.Value = _currentLevel;

            // 更新升级成本
            UpdateUpgradeCost();
        }

        private void UpdateUpgradeCost()
        {
            if (_currentLevel >= _maxLevel)
            {
                lblCost.Text = "已达到最大等级，无法继续升级";
                btnUpgrade.Enabled = false;
                btnUpgrade.Text = "已满级";
                btnUpgrade4.Enabled = false;
                btnUpgradeMax.Enabled = false;
                return;
            }

            int nextLevel = _currentLevel + 1;
            if (nextLevel < _upgradeCosts.Length)
            {
                int cost = _upgradeCosts[nextLevel];
                lblCost.Text = $"升级到 +{nextLevel} 需要: {cost} 摩拉";
            }
            else
            {
                lblCost.Text = "已达到最大等级，无法继续升级";
            }

            btnUpgrade.Enabled = true;
            btnUpgrade.Text = "升级";
            btnUpgrade4.Enabled = true;
            btnUpgradeMax.Enabled = true;
        }

        private void InitializeSubStats()
        {
            _subStatValues.Clear();
            _subStatUpgradeLevels.Clear();

            // 重新初始化可用副词条池
            _availableSubstats = new List<string>(_statImprovements.Keys);
            _availableSubstats.Remove(_mainStat);

            foreach (string stat in _subStats)
            {
                if (_statImprovements.ContainsKey(stat))
                {
                    float[] possibleValues = _statImprovements[stat];
                    int randomIndex = _random.Next(possibleValues.Length);
                    float value = possibleValues[randomIndex];

                    _subStatValues.Add((float)Math.Round(value, 1));
                    _subStatUpgradeLevels.Add(0);

                    _availableSubstats.Remove(stat);
                }
            }
        }

        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel)
            {
                MessageBox.Show("圣遗物已达到最大等级，无法继续升级！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 安全升级，不超过最大等级
            _currentLevel = Math.Min(_currentLevel + 1, _maxLevel);
            ProcessLevelUp();
            UpdateUI();
        }

        private void BtnUpgrade4_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel) return;

            // 计算可升级的级数，确保不超过最大等级
            int maxUpgrade = _maxLevel - _currentLevel;
            int upgradeCount = Math.Min(4, maxUpgrade);

            _currentLevel += upgradeCount;
            ProcessLevelUp();
            UpdateUI();
        }

        // 升满级按钮点击事件
        private void BtnUpgradeMax_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel) return;

            // 逐步升级到满级，确保每个等级的逻辑都被处理
            while (_currentLevel < _maxLevel)
            {
                _currentLevel++;
                ProcessLevelUp();
            }
            UpdateUI();
        }

        private void ExpandSubstat(bool showPrompt = true)
        {
            if (_availableSubstats.Count == 0) return;

            int index = _random.Next(_availableSubstats.Count);
            string newStat = _availableSubstats[index];

            // 添加新词条
            _subStats.Add(newStat);

            // 初始化词条值（直接使用预设值）
            if (_statImprovements.ContainsKey(newStat))
            {
                float[] possibleValues = _statImprovements[newStat];
                int randomIndex = _random.Next(possibleValues.Length);
                float value = possibleValues[randomIndex];
                _subStatValues.Add((float)Math.Round(value, 1));
            }
            else
            {
                _subStatValues.Add(0);
            }

            _subStatUpgradeLevels.Add(0);
            _availableSubstats.RemoveAt(index);

            if (showPrompt)
            {
                MessageBox.Show($"恭喜！圣遗物解锁了新的副词条: {newStat}！", "圣遗物升级",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EnhanceSubstat(bool showPrompt = true)
        {
            int statIndex = _random.Next(_subStats.Count);
            string statToUpgrade = _subStats[statIndex];

            if (_statImprovements.ContainsKey(statToUpgrade))
            {
                float[] improvements = _statImprovements[statToUpgrade];
                float improvement = improvements[_random.Next(improvements.Length)];

                _subStatValues[statIndex] += improvement;
                _subStatUpgradeLevels[statIndex]++;

                // 显示强化提示
                if (showPrompt)
                {
                    MessageBox.Show($"恭喜！副词条 {statToUpgrade} 获得强化！", "圣遗物升级",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // 处理升级过程
        private void ProcessLevelUp()
        {
            // 检查是否需要扩展副词条
            if (_subStats.Count == 3 && IsLevelForExpansion(_currentLevel))
            {
                ExpandSubstat(false);
            }
            // 每4级有几率提升副词条（包括所有4的倍数等级）
            else if (_currentLevel % 4 == 0 && _subStats.Count > 0)
            {
                EnhanceSubstat(false);
            }
        }

        public void SetArtifactData(string position, string mainStat, List<string> subStats)
        {
            // 确保UI已初始化
            if (mainLayoutPanel == null)
            {
                InitializeUI();
            }

            // 重置所有相关变量
            _position = position;
            _mainStat = mainStat;
            _subStats = new List<string>(subStats);
            _currentLevel = 0;

            // 重置副词条数据
            InitializeSubStats();

            // 更新UI
            UpdateUI();

            // 重置进度条
            pbUpgrade.Value = 0;

            // 启用按钮
            btnUpgrade.Enabled = true;
            btnUpgrade4.Enabled = true;
            btnUpgradeMax.Enabled = true;
            btnUpgrade.Text = "升级";

            // 更新窗口标题
            this.Text = $"升级 {position}";
        }

        // 检查是否是扩展词条的等级
        private bool IsLevelForExpansion(int level)
        {
            return level == 4 || level == 8 || level == 12 || level == 16;
        }
    }
}