using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static GenshinArtifactTool.Artifact;
using System.Linq;
using static System.Windows.Forms.AxHost;

namespace GenshinArtifactTool
{
    public partial class ArtifactUpgradeForm : Form
    {
        private int _selectedSubstatsUpgradedCount = 0;
        public event Action<Artifact> UpgradeCompleted;
        public Action<Artifact> OnUpgradeComplete { get; set; }
        private ArtifactSource _artifactSource;
        private List<string> _selectedSubstats = new List<string>();
        // 当前圣遗物的 ID
        private string _artifactId;

        public Form1 MainForm { get; set; }
        // 圣遗物部位类型
        private readonly string[] artifactPositions = { "时之沙", "死之羽", "生之花", "空之杯", "理之冠" };
        // 存储圣遗物图片的字典
        private Dictionary<string, Image> artifactImages = new Dictionary<string, Image>();
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
            3000, 3728, 4422, 5150,    // 0-4级
            5900, 6675, 7500, 8350, 9225, // 5-9级
            10125, 11050, 12025, 13025, 15150, // 10-14级
            17600, 20375, 23500, 27050, 31050,35575 // 15-20级
        };

        public ArtifactUpgradeForm()
        {
            InitializeComponent();
            InitializeUI(); // 确保UI始终初始化
            LoadArtifactImages(); // 加载图片资源
        }

        private void LoadArtifactImages()
        {
            try
            {
                // 为每种圣遗物类型加载对应的图片
                foreach (string position in artifactPositions)
                {
                    Image image = GetImageByPosition(position);
                    if (image != null)
                    {
                        artifactImages[position] = image;
                    }
                    else
                    {
                        // 如果找不到对应图片，使用默认图片
                        artifactImages[position] = Properties.Resources.artifact_placeholder;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载圣遗物图片时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Image GetImageByPosition(string position)
        {
            try
            {
                switch (position)
                {
                    case "生之花":
                        return Properties.Resources.生之花;
                    case "死之羽":
                        return Properties.Resources.死之羽;
                    case "时之沙":
                        return Properties.Resources.时之沙;
                    case "空之杯":
                        return Properties.Resources.空之杯;
                    case "理之冠":
                        return Properties.Resources.理之冠;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取图片 {position} 时出错: {ex.Message}");
                return null;
            }
        }
        public ArtifactUpgradeForm(string artifactId, string position, string mainStat, List<string> subStats)
        {
            InitializeComponent();
            InitializeUI();
            LoadArtifactImages();

            _artifactId = artifactId;
            _position = position;
            _mainStat = mainStat;
            _subStats = subStats;

            // 确保_subStats有数据再进行操作
            if (_subStats != null && _subStats.Count > 0)
            {
                UpdateUI();
                SetArtifactImage(position);
            }
            else
            {
                MessageBox.Show("圣遗物副词条数据无效", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void SetArtifactImage(string position)
        {
            if (artifactImages.ContainsKey(position))
            {
                artifactImage.Image = artifactImages[position];
            }
            else
            {
                artifactImage.Image = Properties.Resources.artifact_placeholder;
            }

            // 更新图片位置
            UpdateArtifactImagePosition();
        }
        // 添加图片位置更新方法
        private void UpdateArtifactImagePosition()
        {
            if (mainLayoutPanel != null && artifactImage != null)
            {
                // 计算居中位置
                int centerX = (mainLayoutPanel.Width - artifactImage.Width) / 2;
                int centerY = (mainLayoutPanel.GetRowHeights()[0] - artifactImage.Height) / 2;

                // 设置图片位置
                artifactImage.Location = new Point(centerX, centerY);
            }
        }
        private void ArtifactUpgradeForm_Resize(object sender, EventArgs e)
        {
            // 窗口大小变化时更新UI布局
            UpdateButtonLayout();

            // 同时更新图片位置
            UpdateArtifactImagePosition();
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
            { "暴击率", new float[] { 2.7f, 3.1f, 3.5f, 3.9f } },
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
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            this.Controls.Add(mainLayoutPanel);

            // 图片框（设置为居中对齐）
            artifactImage = new PictureBox();
            artifactImage.BackColor = Color.White;
            artifactImage.BorderStyle = BorderStyle.FixedSingle;
            artifactImage.SizeMode = PictureBoxSizeMode.Zoom; // 保持缩放模式
            artifactImage.Anchor = AnchorStyles.None; // 居中锚定
            artifactImage.Size = new Size(200, 200); // 
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
            try
            {
                // 清空列表
                lstSubStats.Items.Clear();

                // 安全更新副词条列表
                int minCount = Math.Min(
                    Math.Min(_subStats.Count, _subStatValues.Count),
                    _subStatUpgradeLevels.Count
                );

                for (int i = 0; i < minCount; i++)
                {
                    string stat = _subStats[i];
                    float value = _subStatValues[i];
                    int upgradeLevel = _subStatUpgradeLevels[i];

                    string unit = _statUnits.TryGetValue(stat, out var u) ? u : "";
                    string displayValue = unit == "%" ? $"{value:F1}{unit}" : $"{(int)value}{unit}";

                    string upgradeMark = upgradeLevel > 0 ? $" (+{upgradeLevel}次强化)" : "";
                    bool isNewlyAdded = upgradeLevel == 0 && _subStats.Count > 4;
                    string newMark = isNewlyAdded ? " [新解锁]" : "";

                    lstSubStats.Items.Add($"{stat}: {displayValue}{upgradeMark}{newMark}");
                }

                // 处理可能的缺失数据
                if (_subStats.Count > minCount)
                {
                    Debug.WriteLine($"警告: 有{_subStats.Count - minCount}个副词条没有对应数值");
                }

                // 更新其他UI元素
                lblPosition.Text = _position ?? "未知部位";
                lblLevel.Text = $"等级: +{Math.Max(0, _currentLevel)}";
                lblMainStat.Text = $"主词条: {_mainStat ?? "未知"}";
                pbUpgrade.Value = Math.Min(pbUpgrade.Maximum, Math.Max(0, _currentLevel));

                UpdateUpgradeCost();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新UI时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"UI更新错误: {ex}");
            }
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



        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel) return;

            _currentLevel = Math.Min(_currentLevel + 1, _maxLevel);
            ProcessLevelUp();
            UpdateUI();

            var upgradedArtifact = new Artifact
            {
                Id = _artifactId,
                Position = _position,
                MainStat = _mainStat,
                SubStats = GetFormattedSubStats(),
                Level = _currentLevel,
                Source = _artifactSource,
                SelectedSubstats = _selectedSubstats,
                SelectedSubstatsUpgradedCount = _selectedSubstatsUpgradedCount
            };

            UpgradeCompleted?.Invoke(upgradedArtifact);
            ArtifactDataManager.Instance.UpdateArtifact(upgradedArtifact);
        }
        private List<string> GetFormattedSubStats()
        {
            var result = new List<string>();
            for (int i = 0; i < _subStats.Count; i++)
            {
                string stat = _subStats[i];
                float value = _subStatValues[i];
                string unit = _statUnits.TryGetValue(stat, out var u) ? u : "";
                string displayValue = unit == "%" ? $"{value:F1}{unit}" : $"{(int)value}{unit}";
                result.Add($"{stat}: {displayValue}");
            }
            return result;
        }
        private void BtnUpgrade4_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel) return;

            // 计算可升级的级数，确保不超过最大等级
            int maxUpgrade = _maxLevel - _currentLevel;
            int upgradeCount = Math.Min(4, maxUpgrade);

            for (int i = 0; i < upgradeCount; i++)
            {
                _currentLevel++;
                ProcessLevelUp();
            }

            UpdateUI();

            // 创建升级后的圣遗物对象并触发事件
            var upgradedArtifact = new Artifact
            {
                Id = _artifactId,
                Position = _position,
                MainStat = _mainStat,
                SubStats = GetFormattedSubStats(),
                SelectedSubstatsUpgradedCount = _selectedSubstatsUpgradedCount,
                Source = _artifactSource, // 保留原始来源
                SelectedSubstats = _selectedSubstats, // 保留自选属性
                Level = _currentLevel
            };

            // 触发升级完成事件
            UpgradeCompleted?.Invoke(upgradedArtifact);

            // 更新数据管理器中的圣遗物
            ArtifactDataManager.Instance.UpdateArtifact(upgradedArtifact);
        }

        // 升满级按钮点击事件
        private void BtnUpgradeMax_Click(object sender, EventArgs e)
        {
            if (_currentLevel >= _maxLevel) return;

            btnUpgradeMax.Enabled = false;

            try
            {
                int startLevel = _currentLevel;

                for (int targetLevel = startLevel + 1; targetLevel <= _maxLevel; targetLevel++)
                {
                    _currentLevel = targetLevel;
                    ProcessLevelUp();
                }

                UpdateUI();

                // 创建升级后的圣遗物对象
                var upgradedArtifact = new Artifact
                {
                    Id = _artifactId,
                    Position = _position,
                    MainStat = _mainStat,
                    SubStats = GetFormattedSubStats(),
                    Source = _artifactSource, // 保留原始来源
                    SelectedSubstats = _selectedSubstats, // 保留自选属性
                    SelectedSubstatsUpgradedCount = _selectedSubstatsUpgradedCount,
                    Level = _currentLevel
                };

                // 触发升级完成事件
                UpgradeCompleted?.Invoke(upgradedArtifact);

                // 更新数据管理器中的圣遗物
                ArtifactDataManager.Instance.UpdateArtifact(upgradedArtifact);
            }
            finally
            {
                btnUpgradeMax.Enabled = true;
            }

        }


        // 处理升级过程
        private void ProcessLevelUp()
        {
            // 3词条圣遗物首次到+4级时优先添加新词条
            if (_subStats.Count == 3 && _currentLevel == 4)
            {
                ExpandSubstatWithRandomValue(false);
                return; // 跳过本次强化
            }

            // 每4级强化一次
            if (_currentLevel % 4 == 0 && _currentLevel <= 20)
            {
                if (_artifactSource == ArtifactSource.Customized &&
                    _selectedSubstats.Count > 0 &&
                    _selectedSubstatsUpgradedCount < 2)
                {
                    // 祝圣之霜圣遗物优先强化自选副词条（总共至少2次）
                    EnhanceSelectedSubstats();
                }
                else
                {
                    // 普通圣遗物随机强化
                    EnhanceRandomSubstat();
                }
            }
        }
        private void EnhanceSelectedSubstats()
        {
            if (_selectedSubstats.Count == 0) return;

            // 随机选择一条自选副词条
            int selectedIndex = _random.Next(_selectedSubstats.Count);
            string substat = _selectedSubstats[selectedIndex];

            if (_statImprovements.TryGetValue(substat, out float[] improvements))
            {
                // 找到对应的副词条索引
                int statIndex = _subStats.FindIndex(s => s == substat);
                if (statIndex >= 0)
                {
                    float gain = improvements[_random.Next(improvements.Length)];
                    _subStatValues[statIndex] += gain;
                    _subStatUpgradeLevels[statIndex]++;
                    _selectedSubstatsUpgradedCount++;

                    Debug.WriteLine($"强化自选副词条: {substat} (+{gain}) 总强化次数: {_selectedSubstatsUpgradedCount}/2");
                }
            }
        }
        private void ExpandSubstatWithRandomValue(bool showPrompt = true)
        {
            if (_availableSubstats.Count == 0) return;

            int index = _random.Next(_availableSubstats.Count);
            string newStat = _availableSubstats[index];
            float[] possibleValues = _statImprovements[newStat];
            float randomValue = possibleValues[_random.Next(possibleValues.Length)];

            _subStats.Add(newStat);
            _subStatValues.Add(randomValue);
            _subStatUpgradeLevels.Add(0);
            _availableSubstats.RemoveAt(index);

            if (showPrompt)
            {
                string unit = _statUnits[newStat];
                string valueStr = unit == "%" ? $"{randomValue:F1}{unit}" : $"{(int)randomValue}{unit}";
               
            }
        }
        private void EnhanceRandomSubstat(bool showPrompt = true)
        {
            if (_subStats.Count == 0) return;

            int statIndex = _random.Next(_subStats.Count);
            string stat = _subStats[statIndex];

            if (_statImprovements.TryGetValue(stat, out float[] improvements))
            {
                // 随机选择一个强化值（包含最低档）
                float gain = improvements[_random.Next(improvements.Length)];

                _subStatValues[statIndex] += gain;
                _subStatUpgradeLevels[statIndex]++;

                if (showPrompt)
                {
                    string unit = _statUnits[stat];
                    string valueStr = unit == "%" ? $"{gain:F1}{unit}" : $"+{(int)gain}{unit}";
                    
                }
            }
        }
        private void ExpandSubstat(bool showPrompt = true)
        {
            if (_availableSubstats.Count == 0) return;

            int index = _random.Next(_availableSubstats.Count);
            string newStat = _availableSubstats[index];

            // 添加新词条（取该词条的最低档数值）
            float[] possibleValues = _statImprovements[newStat];
            float baseValue = possibleValues[0]; // 取最小值作为初始值

            _subStats.Add(newStat);
            _subStatValues.Add(baseValue);
            _subStatUpgradeLevels.Add(0);
            _availableSubstats.RemoveAt(index);

            if (showPrompt)
            {
                MessageBox.Show($"解锁新副词条: {newStat} +{baseValue}{_statUnits[newStat]}",
                    "圣遗物升级", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Debug.WriteLine($"新增词条: {newStat} 初始值: {baseValue}");
            
        }


        public void SetArtifactData(Artifact artifact)
        {

            _availableSubstats = new List<string>(_statImprovements.Keys);
            _artifactId = artifact.Id;
            _position = artifact.Position;
            _mainStat = artifact.MainStat;
            _artifactSource = artifact.Source;
            _selectedSubstats = artifact.SelectedSubstats ?? new List<string>();
            _selectedSubstatsUpgradedCount = artifact.SelectedSubstatsUpgradedCount;
            _currentLevel = artifact.Level;

            // 解析副词条数据
            _subStats = new List<string>();
            _subStatValues = new List<float>();
            _subStatUpgradeLevels = new List<int>();
            if (_subStats != null && _mainStat != null)
            {
                _availableSubstats.RemoveAll(s => _subStats.Contains(s) || s == _mainStat);
            }
            else
            {
                // 处理意外情况
                if (_subStats == null)
                {
                    Debug.WriteLine("警告: _subStats 为 null");
                    _subStats = new List<string>();
                }
                if (_mainStat == null)
                {
                    Debug.WriteLine("警告: _mainStat 为 null");
                    _mainStat = string.Empty;
                }
                InitializeAvailableSubstats();
            }
            foreach (var statStr in artifact.SubStats)
            {
                var parts = statStr.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    string statName = parts[0].Trim();
                    string valueStr = parts[1].Trim();

                    if (float.TryParse(valueStr.Replace("%", ""), out float value))
                    {
                        _subStats.Add(statName);
                        _subStatValues.Add(value);
                        _subStatUpgradeLevels.Add(0);
                    }
                }
            }

            UpdateUI();
            SetArtifactImage(_position);
        }

        private void InitializeAvailableSubstats()
        {
            try
            {
                // 确保基础数据已加载
                if (_statImprovements == null || _statImprovements.Count == 0)
                {
                    Debug.WriteLine("错误: _statImprovements 未初始化");
                    return;
                }

                // 创建新的列表
                _availableSubstats = new List<string>(_statImprovements.Keys);

                // 安全移除 - 使用普通循环替代LINQ
                for (int i = _availableSubstats.Count - 1; i >= 0; i--)
                {
                    string s = _availableSubstats[i];
                    if ((_subStats != null && _subStats.Contains(s)) ||
                        (!string.IsNullOrEmpty(_mainStat) && s == _mainStat))
                    {
                        _availableSubstats.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初始化可用副词条时出错: {ex.Message}");
                _availableSubstats = new List<string>();
            }
        }
        private void UpdateArtifactInManager()
        {
            if (!string.IsNullOrEmpty(_artifactId))
            {
                var updatedArtifact = new Artifact
                {
                    Id = _artifactId,
                    Position = _position,
                    MainStat = _mainStat,
                    SubStats = new List<string>()
                };

                // 构建更新后的副词条数据
                for (int i = 0; i < _subStats.Count; i++)
                {
                    string stat = _subStats[i].Split(':')[0].Trim();
                    float value = _subStatValues[i];
                    string unit = GetStatUnit(stat);
                    string displayValue = $"{value}{unit}";
                    int upgradeLevel = _subStatUpgradeLevels[i];
                    string upgradeMark = upgradeLevel > 0 ? $" (+{upgradeLevel}次强化)" : "";
                    updatedArtifact.SubStats.Add($"{stat}: {displayValue}{upgradeMark}");
                }

                ArtifactDataManager.Instance.UpdateArtifact(updatedArtifact);
            }
        }
        private string GetStatUnit(string stat)
        {
            switch (stat)
            {
                case "攻击力百分比":
                case "防御力百分比":
                case "生命值百分比":
                case "元素充能效率":
                case "暴击率":
                case "暴击伤害":
                    return "%";
                default:
                    return "";
            }
        }
        // 检查是否是扩展词条的等级
        private bool IsLevelForExpansion(int level)
        {
            // 4/8/12/16级时扩展副词条
            return level == 4 || level == 8 || level == 12 || level == 16;
        }

        
    }
}