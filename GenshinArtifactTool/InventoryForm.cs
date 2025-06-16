using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GenshinArtifactTool
{
    public partial class InventoryForm : Form
    {
        // 存储圣遗物的列表
        private List<(string position, string mainStat, List<string> subStats)> artifacts = new List<(string, string, List<string>)>();
        private Panel artifactsPanel;
        private Form1 mainForm;

        // 圣遗物类定义（如果尚未定义）
        public class Relic
        {
            public string name { get; set; }
            public int level { get; set; }
            public Stat mainStat { get; set; }
            public List<Stat> subStats { get; set; }

            public void LevelUp()
            {
                level += 4; // 简单示例，实际应根据游戏规则实现
            }
        }

        // 圣遗物属性类定义
        public class Stat
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        // 单例库存管理器（如果尚未定义）
        public class InventoryManager
        {
            private static InventoryManager instance = new InventoryManager();
            public static InventoryManager Instance => instance;

            public List<Relic> Relics { get; set; } = new List<Relic>();

            public void UpdateRelic(Relic relic)
            {
                // 更新圣遗物逻辑
            }
        }

        // 主窗体引用属性
        public Form1 MainForm
        {
            get => mainForm;
            set => mainForm = value;
        }

        private ListBox relicListBox;
        private Label relicNameLabel;
        private Label relicLevelLabel;
        private Label mainStatLabel;
        private TextBox subStatsTextBox;
        private Button levelUpButton;
        private Relic selectedRelic;

        private IContainer components = null;
        public InventoryForm()
        {
            InitializeComponent();
            RefreshRelicList();
        }



        // 添加圣遗物到背包
        public void AddArtifact((string position, string mainStat, List<string> subStats) artifact)
        {
            artifacts.Add(artifact);
            DisplayArtifacts();
        }

        private void DisplayArtifacts()
        {
            // 清空现有控件
            artifactsPanel.Controls.Clear();

            int yPos = 10;
            foreach (var artifact in artifacts)
            {
                // 创建圣遗物卡片
                Panel card = new Panel();
                card.Size = new Size(730, 120);
                card.Location = new Point(10, yPos);
                card.BorderStyle = BorderStyle.FixedSingle;
                card.BackColor = Color.FromArgb(255, 250, 240);
                artifactsPanel.Controls.Add(card);

                // 显示圣遗物部位
                Label positionLabel = new Label();
                positionLabel.Text = $"部位: {artifact.position}";
                positionLabel.Location = new Point(10, 10);
                positionLabel.Font = new Font("微软雅黑", 10, FontStyle.Bold);
                card.Controls.Add(positionLabel);

                // 显示主词条
                Label mainStatLabel = new Label();
                mainStatLabel.Text = $"主词条: {artifact.mainStat}";
                mainStatLabel.Location = new Point(10, 35);
                mainStatLabel.Font = new Font("微软雅黑", 10);
                card.Controls.Add(mainStatLabel);

                // 显示副词条
                for (int i = 0; i < artifact.subStats.Count; i++)
                {
                    Label subStatLabel = new Label();
                    subStatLabel.Text = $"副词条 {i + 1}: {artifact.subStats[i]}";
                    subStatLabel.Location = new Point(10, 60 + i * 20);
                    subStatLabel.Font = new Font("微软雅黑", 9);
                    card.Controls.Add(subStatLabel);
                }

                // 添加升级按钮
                Button upgradeBtn = new Button();
                upgradeBtn.Text = "升级";
                upgradeBtn.Location = new Point(620, 80);
                upgradeBtn.Size = new Size(100, 30);
                upgradeBtn.Tag = artifact; // 存储圣遗物信息
                upgradeBtn.Click += UpgradeBtn_Click;
                card.Controls.Add(upgradeBtn);

                yPos += 130;
            }
        }

        private void UpgradeBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            var artifactInfo = (ValueTuple<string, string, List<string>>)btn.Tag;

            if (mainForm != null)
            {
                // 先显示升级标签页
               

                // 创建升级窗体实例
                var upgradeForm = new ArtifactUpgradeForm(
                    artifactInfo.Item1,
                    artifactInfo.Item2,
                    artifactInfo.Item3);

                // 将新窗体显示在升级标签页中
                // 注意：此处需要实现将窗体添加到标签页的逻辑
            }
        }

        private void RefreshRelicList()
        {
            if (relicListBox != null) // 添加空引用检查
            {
                relicListBox.Items.Clear();
                foreach (var relic in InventoryManager.Instance.Relics)
                {
                    relicListBox.Items.Add($"{relic.name} (Lv.{relic.level})");
                }

                if (relicListBox.Items.Count > 0)
                {
                    relicListBox.SelectedIndex = 0;
                }
            }
        }

        private void RelicListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (relicListBox.SelectedIndex >= 0 && relicListBox.SelectedIndex < InventoryManager.Instance.Relics.Count)
            {
                selectedRelic = InventoryManager.Instance.Relics[relicListBox.SelectedIndex];
                UpdateRelicDetails();
            }
        }

        private void UpdateRelicDetails()
        {
            if (selectedRelic == null) return;

            relicNameLabel.Text = selectedRelic.name;
            relicLevelLabel.Text = $"等级: {selectedRelic.level}";
            mainStatLabel.Text = $"主属性: {selectedRelic.mainStat.type} {selectedRelic.mainStat.value}";

            var sb = new StringBuilder();
            foreach (var subStat in selectedRelic.subStats)
            {
                sb.AppendLine($"{subStat.type}: {subStat.value}");
            }
            subStatsTextBox.Text = sb.ToString();
        }

        private void LevelUpButton_Click(object sender, EventArgs e)
        {
            if (selectedRelic != null)
            {
                selectedRelic.LevelUp();
                InventoryManager.Instance.UpdateRelic(selectedRelic);
                UpdateRelicDetails();
                RefreshRelicList();
            }
        }

        
    }
}