using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinArtifactTool
{


    public partial class ArtifactBrushForm : Form
    {
        // 圣遗物部位类型
        private readonly string[] artifactPositions = { "时之沙", "死之羽", "生之花", "空之杯", "理之冠" };
        private InventoryForm inventoryForm;
        // 副词条可能的属性
        private readonly string[] substats = {
            "攻击力百分比", "防御力百分比", "生命值百分比",
            "元素充能效率", "元素精通", "暴击率", "暴击伤害",
            "攻击力", "防御力", "生命值"
        };
        private Form1 mainForm;
        // 存储所有圣遗物图片的列表
        private List<Image> artifactImages = new List<Image>();
        // 当前显示的圣遗物索引
        private int currentIndex = 0;
        // 圣遗物图片控件
        private PictureBox artifactPictureBox;
        // 左右箭头按钮
        private Button btnLeft, btnRight;
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

        public event EventHandler<UpgradeRequestedEventArgs> UpgradeRequested;

        public class UpgradeRequestedEventArgs : EventArgs
        {
            public string Position { get; set; }
            public string MainStat { get; set; }
            public List<string> SubStats { get; set; }
        }

        // 窗体大小改变时重新布局
        private void ArtifactBrushForm_Resize(object sender, EventArgs e)
        {
            UpdateLayout(); 

        }
        private Form1 _mainForm;
        public Form1 MainForm
        {
            set { _mainForm = value; }
        }
        public void SetInventoryForm(InventoryForm inventoryForm)
        {
            this.inventoryForm = inventoryForm;
        }

        public ArtifactBrushForm(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }
        public ArtifactBrushForm()
        {
            // 如果没有设计器生成的代码，注释掉这一行
            InitializeComponent();

            // 调用自定义的界面初始化方法
            InitializeUI();

            // 加载图片资源
            LoadArtifactImages();

            // 添加窗体大小改变事件处理
            this.Resize += ArtifactBrushForm_Resize;
        }
        // 初始化界面
        private void InitializeUI()
        {
            this.Size = new Size(600, 800); 
            this.MinimumSize = new Size(400, 600); // 设置最小尺寸
            this.Text = "圣遗物抽取";
            this.BackColor = Color.FromArgb(236, 229, 216);


            // 创建主面板（带滚动条）
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true; // 启用自动滚动
            this.Controls.Add(mainPanel);

            // 创建圣遗物图片框
            artifactPictureBox = new PictureBox();
            artifactPictureBox.Size = new Size(200, 200);
            artifactPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            artifactPictureBox.BackColor = Color.White;
            artifactPictureBox.Click += ArtifactPictureBox_Click;
            artifactPictureBox.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(artifactPictureBox);

            // 创建左右箭头按钮
            btnLeft = new Button();
            btnLeft.Text = "<";
            btnLeft.Font = new Font("Arial", 20, FontStyle.Bold);
            btnLeft.Size = new Size(50, 50);
            btnLeft.Click += BtnLeft_Click;
            mainPanel.Controls.Add(btnLeft);

            btnRight = new Button();
            btnRight.Text = ">";
            btnRight.Font = new Font("Arial", 20, FontStyle.Bold);
            btnRight.Size = new Size(50, 50);
            btnRight.Click += BtnRight_Click;
            mainPanel.Controls.Add(btnRight);

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

            // 设置左右箭头位置
            btnLeft.Location = new Point(imageX - 30 - btnLeft.Width, imageY + (imageHeight - btnLeft.Height) / 2);
            btnRight.Location = new Point(imageX + imageWidth + 30, imageY + (imageHeight - btnRight.Height) / 2);

            // 设置属性标签位置
            int propertyX = centerX - 250; // 标签宽度的一半
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
            int buttonX = centerX - buttonWidth ;
            int buttonY = propertyY + 6 * (propertyLabels[0].Height + propertySpacing) + 30;

            btnGet.Size = new Size(buttonWidth, buttonHeight);
            btnGet.Location = new Point(buttonX, buttonY);

            btnUpgrade.Size = new Size(buttonWidth, buttonHeight);
            btnUpgrade.Location = new Point(buttonX+ buttonWidth+30, buttonY );

            // 设置主面板的自动滚动区域
            int totalHeight = this.Height;
            mainPanel.AutoScrollMinSize = new Size(0, totalHeight);
        }

 
        // 加载圣遗物图片
        private void LoadArtifactImages()
        {
            // 这里应该从资源或文件加载实际的圣遗物图片
            for (int i = 1; i <= 10; i++)
            {
                artifactImages.Add(Properties.Resources.artifact_placeholder);
            }

            // 显示第一张图片
            if (artifactImages.Count > 0)
            {
                artifactPictureBox.Image = artifactImages[currentIndex];
            }
        }

        // 左箭头点击事件
        private void BtnLeft_Click(object sender, EventArgs e)
        {
            if (artifactImages.Count == 0) return;

            currentIndex = (currentIndex - 1 + artifactImages.Count) % artifactImages.Count;
            artifactPictureBox.Image = artifactImages[currentIndex];
        }

        // 右箭头点击事件
        private void BtnRight_Click(object sender, EventArgs e)
        {
            if (artifactImages.Count == 0) return;

            currentIndex = (currentIndex + 1) % artifactImages.Count;
            artifactPictureBox.Image = artifactImages[currentIndex];
        }

        // 圣遗物图片点击事件
        private void ArtifactPictureBox_Click(object sender, EventArgs e)
        {
            // 创建并显示圣遗物选择对话框
            using (ArtifactSelectionForm selectionForm = new ArtifactSelectionForm(artifactImages, currentIndex))
            {
                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    currentIndex = selectionForm.SelectedIndex;
                    artifactPictureBox.Image = artifactImages[currentIndex];
                }
            }
        }

        // 获取按钮点击事件
        private void BtnGet_Click(object sender, EventArgs e)
        {
            GenerateRandomArtifact();
            UpdatePropertyDisplay();

            // 将圣遗物添加到背包


            // 添加按钮点击动画效果
            btnGet.BackColor = Color.FromArgb(41, 128, 185);
            btnGet.Invalidate();
            System.Threading.Thread.Sleep(100);
            btnGet.BackColor = Color.FromArgb(52, 152, 219);
        }

        // 去升级按钮点击事件
        private void BtnUpgrade_Click(object sender, EventArgs e)
        {
            var artifactInfo = GetCurrentArtifact();
            if (artifactInfo == default) return;

            // 触发事件
            UpgradeRequested?.Invoke(this, new UpgradeRequestedEventArgs
            {
                Position = artifactInfo.position,
                MainStat = artifactInfo.mainStat,
                SubStats = artifactInfo.subStats
            });
        }

        // 生成随机圣遗物
        private void GenerateRandomArtifact()
        {
            Random random = new Random();

            // 随机选择圣遗物部位
            currentPosition = artifactPositions[random.Next(artifactPositions.Length)];

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
                    string[] gobletStats = { "攻击力百分比", "防御力百分比", "生命值百分比", "元素伤害加成百分比", "物理伤害加成百分比", "元素精通" };
                    mainStat = gobletStats[random.Next(gobletStats.Length)];
                    break;
                case "理之冠":
                    string[] circletStats = { "攻击力百分比", "防御力百分比", "生命值百分比", "暴击率", "暴击伤害", "治疗加成", "元素精通" };
                    mainStat = circletStats[random.Next(circletStats.Length)];
                    break;
            }

            // 生成副词条（3条或4条，且不与主词条重复）
            subStats.Clear();

            // 随机决定副词条数量（3或4）
            int subStatCount = random.Next(2, 4) + 1; // 2或3，加1后为3或4

            // 复制所有可能的副词条选项
            List<string> availableSubstats = new List<string>(substats);

            // 移除与主词条相同的属性
            availableSubstats.Remove(mainStat);

            for (int i = 0; i < subStatCount; i++)
            {
                if (availableSubstats.Count == 0) break;

                int index = random.Next(availableSubstats.Count);
                subStats.Add(availableSubstats[index]);
                availableSubstats.RemoveAt(index);
            }
        }

        public (string position, string mainStat, List<string> subStats) GetCurrentArtifact()
        {
            return (currentPosition, mainStat, new List<string>(subStats));
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
        // 在刷圣遗物的代码中，获得新圣遗物时调用
       

        
    }
}