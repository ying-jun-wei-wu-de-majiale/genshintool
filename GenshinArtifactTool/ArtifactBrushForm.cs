using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinArtifactTool
{
    public partial class ArtifactBrushForm : Form
    {
        // 引用数据管理器单例
        private readonly ArtifactDataManager _dataManager = ArtifactDataManager.Instance;

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

        // 当前圣遗物属性
        private string currentPosition;
        private string mainStat;
        private List<string> subStats = new List<string>();

        public ArtifactBrushForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadArtifactImages();
            this.Resize += ArtifactBrushForm_Resize;
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
            try
            {
                _resinConsumed += 20;
                UpdateResinDisplay();

                GenerateRandomArtifact();
                UpdatePropertyDisplay();

                // 按钮点击动画效果
                btnGet.BackColor = Color.FromArgb(41, 128, 185);
                btnGet.Invalidate();
                System.Threading.Thread.Sleep(100);
                btnGet.BackColor = Color.FromArgb(52, 152, 219);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取圣遗物时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 去升级按钮点击事件
        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            try
            {
                var allArtifacts = _dataManager.GetAllArtifacts();
                if (allArtifacts.Count == 0)
                {
                    MessageBox.Show("背包中没有圣遗物可以升级", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 打开升级界面并传递圣遗物（这里简化为传递第一个圣遗物）
                var artifactToUpgrade = allArtifacts[0];
                var upgradeForm = new ArtifactUpgradeForm(artifactToUpgrade);
                upgradeForm.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开升级界面时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 生成随机圣遗物
        private void GenerateRandomArtifact()
        {
            Random random = new Random();

            // 随机选择圣遗物部位
            currentPosition = artifactPositions[random.Next(artifactPositions.Length)];
            currentArtifactType = currentPosition;

            // 根据部位确定主词条
            switch (currentPosition)
            {
                case "生之花":
                    mainStat = "生命值";
                    break;
                case "死之羽":
                    mainStat = "攻击力";
                    break;
                case "时之沙":
                    string[] sandsStats = { "攻击力百分比", "防御力百分比", "生命值百分比", "元素充能效率", "元素精通" };
                    mainStat = sandsStats[random.Next(sandsStats.Length)];
                    break;
                case "空之杯":
                    string[] gobletStats = { "攻击力百分比", "防御力百分比", "生命值百分比", "风元素伤害加成", "物理伤害加成百分比",
                        "元素精通","火元素伤害加成", "雷元素伤害加成", "水元素伤害加成","冰元素伤害加成","草元素伤害加成", "岩元素伤害加成",};
                    mainStat = gobletStats[random.Next(gobletStats.Length)];
                    break;
                case "理之冠":
                    string[] circletStats = { "攻击力百分比", "防御力百分比", "生命值百分比", "暴击率", "暴击伤害", "治疗加成", "元素精通" };
                    mainStat = circletStats[random.Next(circletStats.Length)];
                    break;
            }

            // 生成副词条
            subStats.Clear();
            int subStatCount = random.Next(2, 4) + 1; // 3或4条副词条
            List<string> availableSubstats = new List<string>(substats);
            availableSubstats.Remove(mainStat);

            for (int i = 0; i < subStatCount; i++)
            {
                if (availableSubstats.Count == 0) break;
                int index = random.Next(availableSubstats.Count);
                subStats.Add(availableSubstats[index]);
                availableSubstats.RemoveAt(index);
            }

            UpdateArtifactImage();
            AddArtifactToManager(); // 关键：将圣遗物添加到数据管理器
        }

        // 将生成的圣遗物添加到数据管理器
        private void AddArtifactToManager()
        {
            try
            {
                var newArtifact = new Artifact
                {
                    Id = Guid.NewGuid().ToString(),
                    Position = currentPosition,
                    MainStat = mainStat,
                    SubStats = new List<string>(subStats)
                };

                _dataManager.AddArtifact(newArtifact);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加圣遗物到背包时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 更新圣遗物图片
        private void UpdateArtifactImage()
        {
            if (currentArtifactType != null && artifactImages.ContainsKey(currentArtifactType))
            {
                artifactPictureBox.Image = artifactImages[currentArtifactType];
            }
            else
            {
                artifactPictureBox.Image = Properties.Resources.artifact_placeholder;
            }
        }

        // 更新属性显示
        private void UpdatePropertyDisplay()
        {
            if (string.IsNullOrEmpty(currentPosition))
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
                propertyLabels[0].Text = $"圣遗物部位: {currentPosition}";
                propertyLabels[1].Text = $"主词条: {mainStat}";

                for (int i = 0; i < 4; i++)
                {
                    if (i < subStats.Count)
                    {
                        propertyLabels[i + 2].Text = $"副词条 {i + 1}: {subStats[i]}";
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