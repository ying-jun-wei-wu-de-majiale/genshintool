using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static GenshinArtifactTool.ArtifactBrushForm;

namespace GenshinArtifactTool
{
    /// <summary>
    /// 圣遗物工具主窗体，提供圣遗物刷取、升级、自定义和背包管理功能
    /// </summary>
    public partial class Form1 : Form
    {
        // 功能界面
        private ArtifactBrushForm artifactBrushForm;
        private ArtifactUpgradeForm artifactUpgradeForm;
        private CustomArtifactForm customArtifactForm;
        private InventoryForm inventoryForm;
        public TabControl tabControl;
       
        // 主界面面板
        private Panel mainPanel;
        private Panel upgradePanel;

        // 圣遗物数据
        private string currentPosition;
        private string currentMainStat;
        private List<string> currentSubStats;
        private int currentLevel = 0;
        
        // 升级系统数据
        private int[] upgradeCosts;
        private Dictionary<string, float[]> statImprovements;
        private List<float> subStatValues;
        private List<int> subStatUpgradeLevels;
        private List<string> availableSubstats;
        private readonly Random random = new Random();

        // 控件
        private ListBox artifactsListBox;
        private Button upgradeButton;
        private Label upgradePositionLabel;
        private Label upgradeLevelLabel;
        private Label upgradeMainStatLabel;
        private ProgressBar upgradeProgressBar;
        private ListBox upgradeSubStatsListBox;
        private Label upgradeCostLabel;
        private Button upgradeLevelButton;
        private Button backButton;
        public Form1()
        {
            InitializeComponent();
            InitializeTabControl();
            InitializeAllForms();
            AddDefaultTabPages();

            // 订阅事件
            artifactBrushForm.UpgradeRequested += ArtifactBrushForm_UpgradeRequested;
        }
        private void ArtifactBrushForm_UpgradeRequested(object sender, UpgradeRequestedEventArgs e)
        {
            if (artifactUpgradeForm != null)
            {
                artifactUpgradeForm.SetArtifactData(e.Position, e.MainStat, e.SubStats);
                SwitchToTab("圣遗物升级");
            }
        }
        private void InitializeArtifactBrushForm()
        {
            artifactBrushForm = new ArtifactBrushForm();
            artifactBrushForm.MainForm = this;
            AddFormToTabPage(artifactBrushForm, "刷圣遗物");
        }
        private void btnArtifactBrush_Click(object sender, EventArgs e)
        {
            if (tabControl != null)
            {
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                {
                    if (tabControl.TabPages[i].Text == "刷圣遗物")
                    {
                        tabControl.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("TabControl 未初始化，请检查！", "错误");
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (artifactBrushForm != null && !artifactBrushForm.IsDisposed)
                {
                    artifactBrushForm.Dispose();
                }
                // 其他窗体的释放操作...
            }
            catch (Exception ex)
            {
                MessageBox.Show($"释放资源时发生错误: {ex.Message}", "错误");
            }

            base.OnFormClosing(e);
        }
        private void InitializeArtifactUpgradeForm()
        {
            artifactUpgradeForm = new ArtifactUpgradeForm();
        }
        private void InitializeFeatureForms()
        {
            inventoryForm = new InventoryForm();
            inventoryForm.MainForm = this;

            // 使用无参构造函数创建 ArtifactBrushForm
            artifactBrushForm = new ArtifactBrushForm();
            artifactBrushForm.MainForm = this;
            artifactBrushForm.SetInventoryForm(inventoryForm); // 通过公共方法设置InventoryForm引用

            // 其他窗体初始化...
            artifactUpgradeForm = new ArtifactUpgradeForm("", "", new List<string>())
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None
            };

            customArtifactForm = new CustomArtifactForm();

            // 添加到 TabControl
            AddFormToTabPage(artifactBrushForm, "刷圣遗物");
            AddFormToTabPage(artifactUpgradeForm, "圣遗物升级");
            AddFormToTabPage(customArtifactForm, "自定义圣遗物");
            AddFormToTabPage(inventoryForm, "背包");

            // 确保 TabControl 可见
            
        }
        private void InitializeCustomArtifactForm()
        {
            customArtifactForm = new CustomArtifactForm();
        }

        private void InitializeInventoryForm()
        {
            inventoryForm = new InventoryForm();
        }
        public ArtifactUpgradeForm ArtifactUpgradeForm
        {
            get { return artifactUpgradeForm; }
        }
        // 添加默认标签页
        private void AddDefaultTabPages()
        {
            // 刷圣遗物标签页
            AddFormToTabPage(artifactBrushForm, "刷圣遗物");

            // 圣遗物升级标签页
            AddFormToTabPage(artifactUpgradeForm, "圣遗物升级");

            // 自定义圣遗物标签页
            AddFormToTabPage(customArtifactForm, "自定义圣遗物");

            // 背包标签页
            AddFormToTabPage(inventoryForm, "背包");

            // 显示第一个标签页
            if (tabControl.TabPages.Count > 0)
            {
                tabControl.SelectedIndex = 0;
            }
        }
        private void SwitchToTab(string tabText)
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Text == tabText)
                {
                    tabControl.SelectedIndex = i;
                    return;
                }
            }
        }


        private void ToggleFullscreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.None;

                // 强制更新所有子窗体布局

            }
        }

        // 确保标签页正确添加和显示
        private void AddTabPage(Form form, string tabText)
        {
            TabPage tabPage = new TabPage(tabText);
            tabPage.Name = "tabPage" + tabText;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true; // 显式设置窗体可见

            tabPage.Controls.Add(form);
           

            // 强制布局更新
            form.BringToFront();
            tabPage.BringToFront();
            form.Show();
        }



        private string GetRandomPosition()
        {
            string[] positions = { "生之花", "死之羽", "时之沙", "空之杯", "理之冠" };
            return positions[random.Next(positions.Length)];
        }

        private string GetRandomMainStat()
        {
            string[] mainStats = { "生命值", "攻击力", "防御力", "元素精通", "暴击率", "暴击伤害" };
            return mainStats[random.Next(mainStats.Length)];
        }

        private List<string> GetRandomSubStats(int count)
        {
            string[] possibleSubStats = {
                "暴击率", "暴击伤害", "攻击力百分比",
                "生命值百分比", "防御力百分比", "元素精通",
                "元素充能效率", "攻击力", "防御力", "生命值"
            };

            var subStats = new List<string>();

            while (subStats.Count < count && subStats.Count < possibleSubStats.Length)
            {
                string stat = possibleSubStats[random.Next(possibleSubStats.Length)];
                if (!subStats.Contains(stat))
                {
                    subStats.Add(stat);
                }
            }

            return subStats;
        }

        private void InitializeUI()
        {
            try
            {
                Console.WriteLine("开始初始化主界面...");
                this.Size = new Size(800, 600);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Text = "原神圣遗物工具";

                //tabControlMain = new TabControl();
                //tabControlMain.Dock = DockStyle.Fill;
                //this.Controls.Add(tabControlMain);

                //InitializeAllForms();

                //// 先添加标签页，再显示
                //AddTabPage(artifactBrushForm, "刷圣遗物");
                //AddTabPage(artifactUpgradeForm, "圣遗物升级");
                //AddTabPage(customArtifactForm, "自定义圣遗物");
                //AddTabPage(inventoryForm, "背包");

                //Console.WriteLine("所有标签页添加完成，共" + tabControlMain.TabCount + "个标签页");

                //tabControlMain.Visible = true; // 显式设置

            }
            catch (Exception ex)
            {
                // 显示详细异常信息（调试时使用）
                MessageBox.Show($"初始化界面失败: {ex.Message}\n\n堆栈: {ex.StackTrace}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 同时记录到控制台
                Console.WriteLine(ex.ToString());
            }
        }

        private void InitializeMainPanel()
        {
            // 标题标签
            Label titleLabel = new Label();
            titleLabel.Text = "圣遗物背包";
            titleLabel.Location = new Point(200, 20);
            titleLabel.Font = new Font("微软雅黑", 16, FontStyle.Bold);
            mainPanel.Controls.Add(titleLabel);

            // 圣遗物列表
            artifactsListBox = new ListBox();
            artifactsListBox.Location = new Point(20, 70);
            artifactsListBox.Size = new Size(450, 200);
            artifactsListBox.Font = new Font("微软雅黑", 11);
            artifactsListBox.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(artifactsListBox);

            // 升级按钮
            upgradeButton = new Button();
            upgradeButton.Location = new Point(175, 290);
            upgradeButton.Size = new Size(150, 50);
            upgradeButton.Text = "升级选中圣遗物";
            upgradeButton.Font = new Font("微软雅黑", 12);
            upgradeButton.ForeColor = Color.White;
            upgradeButton.BackColor = Color.FromArgb(231, 76, 60);
            upgradeButton.FlatStyle = FlatStyle.Flat;
            upgradeButton.Click += UpgradeButton_Click;
            mainPanel.Controls.Add(upgradeButton);

            // 模拟加载圣遗物数据
            LoadSampleArtifacts();
        }

        private void LoadSampleArtifacts()
        {
            artifactsListBox.Items.Clear();
            artifactsListBox.Items.Add("花 - 攻击力百分比");
            artifactsListBox.Items.Add("羽 - 暴击率");
            artifactsListBox.Items.Add("沙 - 元素充能效率");
            artifactsListBox.Items.Add("杯 - 火元素伤害加成");
            artifactsListBox.Items.Add("头 - 暴击伤害");
        }

        private void GetSelectedArtifactData()
        {
            try
            {
                if (artifactsListBox.SelectedIndex < 0) return;

                string selectedText = artifactsListBox.SelectedItem.ToString();
                // 使用字符串数组作为分隔符
                string[] parts = selectedText.Split(new[] { " - " }, StringSplitOptions.None);

                if (parts.Length >= 2)
                {
                    currentPosition = parts[0].Trim();
                    currentMainStat = parts[1].Trim();
                    InitializeSubStats();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取圣遗物数据时发生错误: {ex.Message}", "错误");
            }
        }
        private void InitializeTabControl()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(tabControl);
        }
        private void InitializeSubStats()
        {
            // 升级所需摩拉
            upgradeCosts = new int[] {
                0, 2000, 4000, 6000, 8000,
                12000, 16000, 20000, 24000, 30000,
                36000, 42000, 50000, 60000, 70000,
                80000, 90000, 100000, 120000, 150000
            };

            // 副词条提升值
            statImprovements = new Dictionary<string, float[]>()
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

            subStatValues = new List<float>();
            subStatUpgradeLevels = new List<int>();
            availableSubstats = new List<string>(statImprovements.Keys);
            availableSubstats.Remove(currentMainStat);

            // 根据圣遗物部位设置初始副词条
            switch (currentPosition)
            {
                case "花":
                    currentSubStats = new List<string> { "生命值", "防御力", "攻击力百分比" };
                    break;
                case "羽":
                    currentSubStats = new List<string> { "暴击率", "攻击力", "元素精通" };
                    break;
                default:
                    currentSubStats = GetRandomSubStats(3);
                    break;
            }

            // 初始化副词条值
            foreach (string stat in currentSubStats)
            {
                if (statImprovements.ContainsKey(stat))
                {
                    float[] possibleValues = statImprovements[stat];
                    float value = possibleValues[random.Next(possibleValues.Length)];
                    subStatValues.Add((float)Math.Round(value, 1));
                    subStatUpgradeLevels.Add(0);
                    availableSubstats.Remove(stat);
                }
                else
                {
                    subStatValues.Add(0);
                    subStatUpgradeLevels.Add(0);
                }
            }
        }

        private void InitializeUpgradePanel()
        {
            // 圣遗物位置标签
            upgradePositionLabel = new Label();
            upgradePositionLabel.Location = new Point(20, 20);
            upgradePositionLabel.Size = new Size(450, 30);
            upgradePositionLabel.Font = new Font("微软雅黑", 14, FontStyle.Bold);
            upgradePanel.Controls.Add(upgradePositionLabel);

            // 圣遗物等级标签
            upgradeLevelLabel = new Label();
            upgradeLevelLabel.Location = new Point(20, 60);
            upgradeLevelLabel.Size = new Size(150, 25);
            upgradeLevelLabel.Font = new Font("微软雅黑", 12);
            upgradePanel.Controls.Add(upgradeLevelLabel);

            // 主词条标签
            upgradeMainStatLabel = new Label();
            upgradeMainStatLabel.Location = new Point(20, 90);
            upgradeMainStatLabel.Size = new Size(450, 25);
            upgradeMainStatLabel.Font = new Font("微软雅黑", 12);
            upgradePanel.Controls.Add(upgradeMainStatLabel);

            // 进度条
            upgradeProgressBar = new ProgressBar();
            upgradeProgressBar.Location = new Point(20, 120);
            upgradeProgressBar.Size = new Size(450, 20);
            upgradeProgressBar.Maximum = 20;
            upgradePanel.Controls.Add(upgradeProgressBar);

            // 副词条列表
            upgradeSubStatsListBox = new ListBox();
            upgradeSubStatsListBox.Location = new Point(20, 150);
            upgradeSubStatsListBox.Size = new Size(450, 150);
            upgradeSubStatsListBox.Font = new Font("微软雅黑", 11);
            upgradeSubStatsListBox.BorderStyle = BorderStyle.FixedSingle;
            upgradePanel.Controls.Add(upgradeSubStatsListBox);

            // 升级成本标签
            upgradeCostLabel = new Label();
            upgradeCostLabel.Location = new Point(20, 310);
            upgradeCostLabel.Size = new Size(450, 25);
            upgradeCostLabel.Font = new Font("微软雅黑", 12);
            upgradePanel.Controls.Add(upgradeCostLabel);

            // 升级按钮
            upgradeLevelButton = new Button();
            upgradeLevelButton.Location = new Point(175, 340);
            upgradeLevelButton.Size = new Size(150, 40);
            upgradeLevelButton.Text = "升级";
            upgradeLevelButton.Font = new Font("微软雅黑", 12);
            upgradeLevelButton.ForeColor = Color.White;
            upgradeLevelButton.BackColor = Color.FromArgb(46, 204, 113);
            upgradeLevelButton.FlatStyle = FlatStyle.Flat;
            upgradeLevelButton.Click += UpgradeLevelButton_Click;
            upgradePanel.Controls.Add(upgradeLevelButton);

            // 返回按钮
            backButton = new Button();
            backButton.Location = new Point(175, 390);
            backButton.Size = new Size(150, 40);
            backButton.Text = "返回";
            backButton.Font = new Font("微软雅黑", 12);
            backButton.ForeColor = Color.White;
            backButton.BackColor = Color.FromArgb(52, 152, 219);
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.Click += BackButton_Click;
            upgradePanel.Controls.Add(backButton);
        }

        private void UpgradeButton_Click(object sender, EventArgs e)
        {
            GetSelectedArtifactData();
            if (string.IsNullOrEmpty(currentPosition))
            {
                MessageBox.Show("请先选择一个圣遗物", "提示");
                return;
            }

            // 切换到标签页（确保TabControl可见）

           

            // 传递数据给升级界面
            artifactUpgradeForm.SetArtifactData(
                currentPosition.Trim(),  // 注意添加Trim()去除空格
                currentMainStat.Trim(),
                currentSubStats
            );
        }

        private void InitializeUpgradeUI()
        {
            currentLevel = 0; // 初始等级为0
            upgradePositionLabel.Text = currentPosition;
            upgradeLevelLabel.Text = $"等级: +{currentLevel}";
            upgradeMainStatLabel.Text = $"主词条: {currentMainStat}";
            upgradeProgressBar.Value = currentLevel;

            // 初始化副词条
            UpdateSubStatsUI();
            UpdateUpgradeCost();
        }

        private void UpdateSubStatsUI()
        {
            upgradeSubStatsListBox.Items.Clear();
            for (int i = 0; i < currentSubStats.Count; i++)
            {
                string stat = currentSubStats[i];
                float value = subStatValues[i];
                int upgradeLevel = subStatUpgradeLevels[i];

                string unit = GetStatUnit(stat);
                string displayValue = $"{value}{unit}";
                string upgradeMark = upgradeLevel > 0 ? $" (+{upgradeLevel}次强化)" : "";
                bool isNewlyAdded = upgradeLevel == 0 && currentSubStats.Count > 4;
                string newMark = isNewlyAdded ? " [新解锁]" : "";

                upgradeSubStatsListBox.Items.Add($"{stat}: {displayValue}{upgradeMark}{newMark}");
            }
        }

        private string GetStatUnit(string stat)
        {
            if (stat.Contains("百分比")) return "%";
            if (stat == "元素精通" || stat == "攻击力" || stat == "防御力" || stat == "生命值") return "点";
            return "";
        }

        private void UpdateUpgradeCost()
        {
            if (currentLevel >= 20)
            {
                upgradeCostLabel.Text = "已达到最大等级";
                upgradeLevelButton.Enabled = false;
                upgradeLevelButton.Text = "已满级";
                return;
            }

            int nextLevel = currentLevel + 1;
            if (nextLevel < upgradeCosts.Length)
            {
                int cost = upgradeCosts[nextLevel];
                upgradeCostLabel.Text = $"升级到 +{nextLevel} 需要: {cost} 摩拉";
            }
            else
            {
                upgradeCostLabel.Text = "已达到最大等级，无法继续升级";
            }

            upgradeLevelButton.Enabled = true;
            upgradeLevelButton.Text = "升级";
        }

        private void UpgradeLevelButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentLevel >= 20)
                {
                    MessageBox.Show("圣遗物已达到最大等级，无法继续升级！", "提示");
                    return;
                }

                currentLevel++;
                ProcessLevelUp();
                UpdateUIAfterUpgrade();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"升级圣遗物时发生错误: {ex.Message}", "错误");
            }
        }

        private void ProcessLevelUp()
        {
            // 每4级可能解锁新词条或强化已有词条
            if (currentSubStats.Count == 3 && (currentLevel == 4 || currentLevel == 8 || currentLevel == 12 || currentLevel == 16))
            {
                ExpandSubstat();
            }
            else if (currentLevel % 4 == 0 && currentLevel <= 20)
            {
                EnhanceSubstat();
            }
        }

        private void ExpandSubstat()
        {
            if (availableSubstats.Count == 0) return;

            int index = random.Next(availableSubstats.Count);
            string newStat = availableSubstats[index];
            currentSubStats.Add(newStat);

            if (statImprovements.ContainsKey(newStat))
            {
                float[] possibleValues = statImprovements[newStat];
                float value = possibleValues[random.Next(possibleValues.Length)];
                subStatValues.Add((float)Math.Round(value, 1));
            }
            else
            {
                subStatValues.Add(0);
            }

            subStatUpgradeLevels.Add(0);
            availableSubstats.RemoveAt(index);

            MessageBox.Show($"恭喜！圣遗物解锁了新的副词条: {newStat}！", "圣遗物升级");
        }

        private void EnhanceSubstat()
        {
            int statIndex = random.Next(currentSubStats.Count);
            string statToUpgrade = currentSubStats[statIndex];

            if (statImprovements.ContainsKey(statToUpgrade))
            {
                float[] improvements = statImprovements[statToUpgrade];
                float improvement = improvements[random.Next(improvements.Length)];

                subStatValues[statIndex] += improvement;
                subStatUpgradeLevels[statIndex]++;

                MessageBox.Show($"恭喜！副词条 {statToUpgrade} 获得强化！", "圣遗物升级");
            }
        }

        private void UpdateUIAfterUpgrade()
        {
            upgradeLevelLabel.Text = $"等级: +{currentLevel}";
            upgradeProgressBar.Value = currentLevel;
            UpdateSubStatsUI();
            UpdateUpgradeCost();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            upgradePanel.Visible = false;
            mainPanel.Visible = true;
        }

        private void AddFormToTabPage(Form form, string tabText)
        {
            TabPage tabPage = new TabPage(tabText);
            tabPage.Name = "tabPage" + tabText;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;

            tabPage.Controls.Add(form);
            tabControl.TabPages.Add(tabPage);

            form.Show();
        }



        // 在Form1类中添加这个方法
        public void SetArtifactData(string position, string mainStat, List<string> subStats)
        {
            // 确保artifactUpgradeForm已初始化
            if (artifactUpgradeForm == null)
            {
                artifactUpgradeForm = new ArtifactUpgradeForm(position, mainStat, subStats);
                AddFormToTabPage(artifactUpgradeForm, "圣遗物升级");
            }
            else
            {
                // 更新现有窗体的数据
                artifactUpgradeForm.SetArtifactData(position, mainStat, subStats);
            }

            // 切换到升级标签页
     
        }
        // 刷圣遗物按钮点击事件
        // 刷圣遗物按钮点击事件


        // 圣遗物升级按钮点击事件
        private void btnArtifactUpgrade_Click(object sender, EventArgs e)
        {
            // 若窗体未初始化或已释放，重新创建
            if (artifactUpgradeForm == null || artifactUpgradeForm.IsDisposed)
            {
                artifactUpgradeForm = new ArtifactUpgradeForm();
                // 设置为非顶级窗体，方便嵌入 TabPage
                artifactUpgradeForm.TopLevel = false;
                artifactUpgradeForm.FormBorderStyle = FormBorderStyle.None;
                artifactUpgradeForm.Dock = DockStyle.Fill;

                // 创建 TabPage 并添加窗体
                TabPage tabPage = new TabPage("圣遗物升级");
                tabPage.Controls.Add(artifactUpgradeForm);
                tabControl.TabPages.Add(tabPage);
            }

            // 切换到“圣遗物升级”对应的 TabPage
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                if (tabControl.TabPages[i].Text == "圣遗物升级")
                {
                    tabControl.SelectedIndex = i;
                    break;
                }
            }
            // 显示窗体（若需要，非顶级窗体添加到容器后一般自动显示，保险起见可调用）
            artifactUpgradeForm.Show();
        }

        // 自定义圣遗物按钮点击事件
        private void btnCustomArtifact_Click(object sender, EventArgs e)
        {
           
        }
        // 在Form1.cs中修改创建ArtifactBrushForm的代码

        // 背包按钮点击事件
        private void btnInventory_Click(object sender, EventArgs e)
        {
         
        }

       
     
    }
}