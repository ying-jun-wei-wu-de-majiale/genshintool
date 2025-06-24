using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GenshinArtifactTool;

namespace GenshinArtifactTool
{

    public partial class ArtifactBrushForm : Form
    {
        private bool _hasUpgradableArtifact = false;
        private string _artifactId;
        public Form1 MainForm { get; set; }
        // 引用数据管理器单例
        private readonly ArtifactDataManager _dataManager = ArtifactDataManager.Instance;
        private Random random = new Random(Guid.NewGuid().GetHashCode());

        // 体力计数器
        private int _resinConsumed = 0;
        private Label _lblResin;
        // 圣遗物部位类型
        private readonly string[] artifactPositions = { "时之沙", "死之羽", "生之花", "空之杯", "理之冠" };
        // 存储圣遗物图片的字典
        private Dictionary<string, Image> artifactImages = new Dictionary<string, Image>();
        // 当前显示的圣遗物类型
        private string currentArtifactType = null;
        private InventoryForm inventoryForm;
        // 副词条可能的属性
        private readonly string[] substats = {
            "攻击力百分比", "防御力百分比", "生命值百分比",
            "元素充能效率", "元素精通", "暴击率", "暴击伤害",
            "攻击力", "防御力", "生命值"
        };
        private Form1 mainForm;
        // 圣遗物图片控件
        private PictureBox artifactPictureBox;
        // 属性标签
        private Label[] propertyLabels = new Label[6];
        // 获取和升级按钮
        private Button btnGet, btnUpgrade;
        // 主面板（包含滚动条）
        private Panel mainPanel;
        private string _position;
        private string _mainStat;
        private List<string> _subStats = new List<string>();
        // 当前圣遗物属性
        private string currentPosition;
        private string mainStat;
        private List<string> subStats = new List<string>();

        private void LogArtifactGeneration()
        {
            Debug.WriteLine("=== 圣遗物生成日志 ===");
            Debug.WriteLine($"当前时间: {DateTime.Now:HH:mm:ss.fff}");
            Debug.WriteLine($"部位列表: {string.Join(", ", artifactPositions)}");
            Debug.WriteLine($"随机数种子: {random.Next()}");
            Debug.WriteLine($"最终生成结果:");
            Debug.WriteLine($"- 部位: {_position}");
            Debug.WriteLine($"- 主词条: {_mainStat}");
            Debug.WriteLine($"- 副词条: {string.Join(", ", _subStats)}");
        }

        public ArtifactBrushForm()
        {
            InitializeComponent();
            artifactPositions = new string[] { "生之花", "死之羽", "时之沙", "空之杯", "理之冠" };

            // 验证初始化
            if (artifactPositions == null || artifactPositions.Length == 0)
            {
                MessageBox.Show("圣遗物部位列表初始化失败", "严重错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            InitializeUI();
            LoadArtifactImages();
            this.Resize += ArtifactBrushForm_Resize;
        }
        public event EventHandler<UpgradeRequestedEventArgs> UpgradeRequested;

        // 触发事件的方法示例
        private void OnUpgradeRequested()
        {
            if (string.IsNullOrEmpty(_artifactId) || string.IsNullOrEmpty(_position) || string.IsNullOrEmpty(_mainStat) || _subStats.Count == 0)
            {
                MessageBox.Show("没有有效的圣遗物数据可供升级", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 确保MainForm已设置
            if (this.MainForm == null)
            {
                MessageBox.Show("无法访问主窗体", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 调用主窗体的升级方法，传递圣遗物 ID
            UpgradeRequested?.Invoke(this, new UpgradeRequestedEventArgs(_artifactId, _position, _mainStat, _subStats));
           
            
        }
        public class UpgradeRequestedEventArgs : EventArgs
        {
            public string Id { get; set; }
            public string Position { get; set; }
            public string MainStat { get; set; }
            public List<string> SubStats { get; set; }

            public UpgradeRequestedEventArgs(string id, string position, string mainStat, List<string> subStats)
            {
                Id = id;
                Position = position;
                MainStat = mainStat;
                SubStats = subStats;
            }
        }
        private void ArtifactBrushForm_Resize(object sender, EventArgs e)
        {
            UpdateLayout(); // 调用布局更新方法
            UpdateResinLabelPosition(); // 调用树脂标签位置更新方法
        }
        public ArtifactBrushForm(Form owner) : this()
        {
            this.Owner = owner;
        }

        // 设置InventoryForm引用
        public void SetInventoryForm(InventoryForm inventoryForm)
        {
            this.inventoryForm = inventoryForm;
        }

        // 初始化界面
        private void InitializeUI()
        {
            this.Size = new Size(600, 800);
            this.MinimumSize = new Size(400, 600);
            this.Text = "圣遗物抽取";
            this.BackColor = Color.FromArgb(236, 229, 216);

            // 创建主面板（带滚动条）
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            this.Controls.Add(mainPanel);

            // 创建圣遗物图片框
            artifactPictureBox = new PictureBox();
            artifactPictureBox.Size = new Size(200, 200);
            artifactPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            artifactPictureBox.BackColor = Color.Empty;
            artifactPictureBox.BorderStyle = BorderStyle.None;
            mainPanel.Controls.Add(artifactPictureBox);

            // 创建体力显示标签
            _lblResin = new Label();
            _lblResin.Font = new Font("微软雅黑", 13F, FontStyle.Bold);
            _lblResin.ForeColor = Color.FromArgb(231, 76, 60);
            _lblResin.TextAlign = ContentAlignment.MiddleRight;
            _lblResin.AutoSize = false;
            _lblResin.Size = new Size(190, 30);
            _lblResin.Location = new Point(mainPanel.Width - 190, 10);
            _lblResin.Text = $"已消耗的树脂: {_resinConsumed}";
            mainPanel.Controls.Add(_lblResin);

            // 创建属性标签
            for (int i = 0; i < 6; i++)
            {
                propertyLabels[i] = new Label();
                propertyLabels[i].Font = new Font("微软雅黑", 12);
                propertyLabels[i].TextAlign = ContentAlignment.MiddleLeft;
                propertyLabels[i].BackColor = Color.Transparent;
                mainPanel.Controls.Add(propertyLabels[i]);
            }

            // 创建按钮
            btnGet = new Button();
            btnGet.Text = "获取圣遗物";
            btnGet.Font = new Font("微软雅黑", 14, FontStyle.Bold);
            btnGet.ForeColor = Color.White;
            btnGet.BackColor = Color.FromArgb(52, 152, 219);
            btnGet.FlatStyle = FlatStyle.Flat;
            btnGet.FlatAppearance.BorderSize = 0;
            btnGet.Click += BtnGet_Click;
            mainPanel.Controls.Add(btnGet);

            btnUpgrade = new Button();
            btnUpgrade.Text = "去升级";
            btnUpgrade.Font = new Font("微软雅黑", 14, FontStyle.Bold);
            btnUpgrade.ForeColor = Color.White;
            btnUpgrade.BackColor = Color.FromArgb(231, 76, 60);
            btnUpgrade.FlatStyle = FlatStyle.Flat;
            btnUpgrade.FlatAppearance.BorderSize = 0;
            btnUpgrade.Click += BtnUpgrade_Click;
            mainPanel.Controls.Add(btnUpgrade);

            // 初始布局
            UpdateLayout();
            mainPanel.SizeChanged += (sender, e) => UpdateLayout();
            this.Resize += (sender, e) => UpdateResinLabelPosition();
        }

        // 更新体力标签位置
        private void UpdateResinLabelPosition()
        {
            if (_lblResin != null && mainPanel != null)
            {
                _lblResin.Location = new Point(mainPanel.Width - 190, 10);
            }
        }

        // 更新体力显示
        private void UpdateResinDisplay()
        {
            if (_lblResin != null)
            {
                _lblResin.Text = $"已消耗的树脂: {_resinConsumed}";
            }
        }

        // 更新布局
        private void UpdateLayout()
        {
            if (mainPanel.Width <= 0) return;
            int centerX = mainPanel.Width / 2;
            int imageWidth = 200;
            int imageHeight = 200;
            int imageX = centerX - imageWidth / 2;
            int imageY = (int)(mainPanel.Height * 0.15);

            // 设置图片位置
            artifactPictureBox.Location = new Point(imageX, imageY);

            // 设置属性标签位置
            int propertyX = centerX - 250;
            int propertyY = imageY + imageHeight + 20;
            int propertySpacing = 10;

            for (int i = 0; i < 6; i++)
            {
                propertyLabels[i].Size = new Size(500, 30);
                propertyLabels[i].Location = new Point(propertyX, propertyY + i * (propertyLabels[i].Height + propertySpacing));
            }

            // 设置按钮位置
            int buttonWidth = 200;
            int buttonHeight = 50;
            int buttonX = centerX - buttonWidth;
            int buttonY = propertyY + 6 * (propertyLabels[0].Height + propertySpacing) + 30;

            btnGet.Size = new Size(buttonWidth, buttonHeight);
            btnGet.Location = new Point(buttonX, buttonY);

            btnUpgrade.Size = new Size(buttonWidth, buttonHeight);
            btnUpgrade.Location = new Point(buttonX + buttonWidth + 30, buttonY);

            // 设置主面板的自动滚动区域
            int totalHeight = this.Height;
            mainPanel.AutoScrollMinSize = new Size(0, totalHeight);
        }

        // 加载圣遗物图片
        private void LoadArtifactImages()
        {
            try
            {
                foreach (string position in artifactPositions)
                {
                    Image image = GetImageByPosition(position);
                    if (image != null)
                    {
                        artifactImages[position] = image;
                    }
                    else
                    {
                        // 使用默认图片（假设Properties.Resources中有占位图）
                        artifactImages[position] = Properties.Resources.artifact_placeholder;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载圣遗物图片时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // 获取按钮点击事件
        private void BtnGet_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("=== 点击获取圣遗物按钮 ===");

            try
            {
                _resinConsumed += 20;
                UpdateResinDisplay();

                // 使用统一的生成方法
                GenerateRandomArtifact(); // 这个方法会更新所有字段并添加到管理器

                // 启用升级按钮
                _hasUpgradableArtifact = true;
                btnUpgrade.Enabled = true;

                // 调试输出
                Debug.WriteLine($"新圣遗物已添加 - ID: {_artifactId}, 部位: {_position}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取圣遗物时出错: {ex.Message}\n{ex.StackTrace}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LogArtifactGeneration();
        }

 
        private string GetRandomPosition()
        {
            // 确保列表已初始化
            if (artifactPositions == null || artifactPositions.Length == 0)
            {
                throw new InvalidOperationException("圣遗物部位列表未初始化");
            }

            // 获取安全随机索引
            int index = random.Next(0, artifactPositions.Length); // 明确指定范围
            string position = artifactPositions[index];

            // 空值检查
            if (string.IsNullOrWhiteSpace(position))
            {
                throw new InvalidOperationException($"生成了无效的部位值: '{position}' (索引:{index})");
            }

            Debug.WriteLine($"成功选择部位: {position} (索引:{index})");
            return position;
        }
        // 在ArtifactBrushForm或主窗体中
        
        private string GetMainStatByPosition(string position)
        {
            // 添加null检查
            if (string.IsNullOrEmpty(position))
                throw new ArgumentException("圣遗物部位不能为空");

            // 使用更安全的查找方式
            switch (position.Trim())
            {
                case "生之花": return "生命值";
                case "死之羽": return "攻击力";
                case "时之沙":
                    return new[] { "攻击力百分比", "防御力百分比", "生命值百分比", "元素充能效率", "元素精通" }
                        [random.Next(5)];
                case "空之杯":
                    return new[] { "攻击力百分比", "防御力百分比", "生命值百分比", "风元素伤害加成", "火元素伤害加成",
                         "水元素伤害加成", "雷元素伤害加成", "冰元素伤害加成", "草元素伤害加成", "岩元素伤害加成",
                         "物理伤害加成百分比", "元素精通" }
                        [random.Next(12)];
                case "理之冠":
                    return new[] { "攻击力百分比", "防御力百分比", "生命值百分比", "暴击率", "暴击伤害", "治疗加成", "元素精通" }
                        [random.Next(7)];
                default:
                    throw new ArgumentException($"未知的圣遗物部位: {position} (有效值: {string.Join(", ", artifactPositions)})");
            }
        }

        private List<string> GenerateSubStats(string mainStat)
        {
            var subStats = new List<string>();

            // 筛选可用的副词条（仅排除与主词条完全相同的属性）
            var availableSubstats = substats.Where(s => s != mainStat).ToList();

            int subStatCount = random.Next(3, 5); // 生成3-4个副词条

            for (int i = 0; i < subStatCount; i++)
            {
                if (availableSubstats.Count == 0) break;

                int index = random.Next(availableSubstats.Count);
                string substatType = availableSubstats[index];
                availableSubstats.RemoveAt(index);

                float value = GetRandomSubstatValue(substatType);
                string unit = _statUnits[substatType];
                string formattedValue = unit == "%" ? $"{value:F1}{unit}" : $"{(int)value}{unit}";

                subStats.Add($"{substatType}: {formattedValue}");
            }

            return subStats;
        }

        private float GetRandomSubstatValue(string substatType)
        {
            if (_statImprovements.TryGetValue(substatType, out float[] values))
            {
                return values[random.Next(values.Length)];
            }
            return 0f;
        }
        private void ShowUpgradeForm()
        {
            var upgradeForm = new ArtifactUpgradeForm(_artifactId, _position, _mainStat, _subStats);

            upgradeForm.UpgradeCompleted += upgradedArtifact =>
            {
                // 1. 更新数据管理器
                ArtifactDataManager.Instance.UpdateArtifact(upgradedArtifact);

                // 2. 通知背包界面刷新
                if (inventoryForm != null && !inventoryForm.IsDisposed)
                {
                    inventoryForm.RefreshArtifacts();
                }

                // 3. 更新当前界面显示（如果需要）
                UpdateArtifactDisplay(upgradedArtifact);
            };

            upgradeForm.ShowDialog();
        }
        // 去升级按钮点击事件
        // ArtifactBrushForm.cs 文件
        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            if (!_hasUpgradableArtifact)
            {
                MessageBox.Show("没有可升级的圣遗物", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_position) || string.IsNullOrEmpty(_mainStat) ||
                _subStats == null || _subStats.Count == 0)
            {
                MessageBox.Show("没有有效的圣遗物数据可供升级", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 触发升级请求事件
            UpgradeRequested?.Invoke(this, new UpgradeRequestedEventArgs(
                _artifactId, _position, _mainStat, _subStats));

            // 禁用升级按钮，防止重复升级
            _hasUpgradableArtifact = false;
            btnUpgrade.Enabled = false;
        }
        public void UpdateArtifactDisplay(Artifact artifact)
        {
            _artifactId = artifact.Id;
            _position = artifact.Position;
            _mainStat = artifact.MainStat;
            _subStats = artifact.SubStats;

            UpdateArtifactImage();
            UpdatePropertyDisplay();
        }
        private void GenerateRandomArtifact()
        {
            // 1. 随机选择有效部位
            _position = artifactPositions[random.Next(artifactPositions.Length)];
            Debug.WriteLine($"随机选择部位: {_position}");

            // 2. 根据部位生成合规主词条
            _mainStat = GetMainStatByPosition(_position);
            Debug.WriteLine($"生成主词条: {_mainStat}");

            // 3. 生成不重复的副词条
            _subStats = GenerateSubStats(_mainStat);
            Debug.WriteLine($"生成副词条: {string.Join(", ", _subStats)}");

            // 4. 生成唯一ID
            _artifactId = Guid.NewGuid().ToString();

            // 5. 更新显示
            UpdateArtifactImage();
            UpdatePropertyDisplay();

            // 6. 添加到数据管理器
            var newArtifact = new Artifact
            {
                Id = _artifactId,
                Position = _position,
                MainStat = _mainStat,
                SubStats = _subStats,
                Level = 0
            };
            ArtifactDataManager.Instance.AddArtifact(newArtifact);
        }
        // 更新圣遗物图片
        private void UpdateArtifactImage()
        {
            if (_position != null && artifactImages.ContainsKey(_position))
            {
                artifactPictureBox.Image = artifactImages[_position];
            }
            else
            {
                // 处理字典中不存在键的情况
                Console.WriteLine($"artifactImages 字典中不存在键: {_position}");
                artifactPictureBox.Image = Properties.Resources.artifact_placeholder;
            }
        }
        // 在 ArtifactBrushForm 类中添加


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
        // 更新属性显示
        private void UpdatePropertyDisplay()
        {
            if (string.IsNullOrEmpty(_position) || string.IsNullOrEmpty(_mainStat) || _subStats.Count == 0)
            {
                propertyLabels[0].Text = "圣遗物部位: 未选择";
                propertyLabels[1].Text = "主词条: 未选择";

                for (int i = 2; i < 6; i++)
                {
                    propertyLabels[i].Text = $"副词条 {i - 1}: 未选择";
                }
            }
            else
            {
                propertyLabels[0].Text = $"圣遗物部位: {_position}";
                propertyLabels[1].Text = $"主词条: {_mainStat}";

                for (int i = 0; i < 4; i++)
                {
                    if (i < _subStats.Count)
                    {
                        propertyLabels[i + 2].Text = $"副词条 {i + 1}: {_subStats[i]}";
                    }
                    else
                    {
                        propertyLabels[i + 2].Text = $"副词条 {i + 1}: --";
                    }
                }
            }
        }
    }
}