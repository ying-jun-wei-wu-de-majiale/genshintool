using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static GenshinArtifactTool.Artifact;

namespace GenshinArtifactTool
{
    public partial class ArtifactSelectionForm : Form
    {
        public Form1 MainForm { get; set; }
        // 类成员声明
        private CheckBox[] chkSubstats = new CheckBox[10];
        private readonly string[] artifactPositions = { "时之沙", "死之羽", "生之花", "空之杯", "理之冠" };
        private readonly Dictionary<string, string[]> mainStatOptions = new Dictionary<string, string[]>
        {
            { "时之沙", new string[] { "攻击力百分比", "防御力百分比", "生命值百分比", "元素充能效率", "元素精通" } },
            { "死之羽", new string[] { "攻击力" } },
            { "生之花", new string[] { "生命值" } },
            { "空之杯", new string[] { "攻击力百分比", "防御力百分比", "生命值百分比",  "风元素伤害加成", "火元素伤害加成",
                         "水元素伤害加成", "雷元素伤害加成", "冰元素伤害加成", "草元素伤害加成", "岩元素伤害加成", "物理伤害加成", "元素精通" } },
            { "理之冠", new string[] { "攻击力百分比", "防御力百分比", "生命值百分比", "暴击率", "暴击伤害", "治疗加成", "元素精通" } }
        };
        private readonly string[] substats = {
            "攻击力百分比", "防御力百分比", "生命值百分比",
            "元素充能效率", "元素精通", "暴击率", "暴击伤害",
            "攻击力", "防御力", "生命值"
        };
        private Dictionary<string, Image> artifactImages = new Dictionary<string, Image>();
        private string selectedPosition = "";
        private string selectedMainStat = "";
        private List<string> selectedSubstats = new List<string>();
        private List<string> generatedSubstats = new List<string>();
        // 替换原有的 generatedSubstats 定义
        private List<string> selectedSubstatNames = new List<string>();  // 只存储选中的属性名
        private Dictionary<string, float> allSubstatValues = new Dictionary<string, float>(); // 最终所有副词条及值
        private Random random = new Random();
        private const int MAX_SELECTED_SUBSTATS = 2;
        //计数器
        private Label lblConsumedCount;
        private int consumedCount = 0;
        // UI控件声明
        private PictureBox pbArtifact;
        private ComboBox cmbPosition;
        private ComboBox cmbMainStat;
        private ListBox lbGeneratedSubstats;
        private Button btnConfirm;
        private Button btnCancel;
        private Panel posContainer;
        private Panel mainStatContainer;
        private Panel substatsContainer;
        private Panel generatedContainer;
        public string SelectedPosition { get { return selectedPosition; } }
        public string SelectedMainStat { get { return selectedMainStat; } }
        public List<string> FinalSubstats { get { return new List<string>(selectedSubstats.Concat(generatedSubstats)); } }

        public ArtifactSelectionForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadArtifactImages();
        }

        private void InitializeUI()
        {
            // 主容器：使用TableLayoutPanel实现居中布局
            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(5, 5, 5, 5),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // 设置行样式
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // 图片行
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));     // 圣遗物类型行
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute,60));     // 主词条行
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));      // 副词条选择行
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));      // 生成副词条行
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));      // 按钮行

            this.Controls.Add(mainTable);

            //==== 1. 圣遗物图片（居中） ====
            var picPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            pbArtifact = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(200, 200),
                Anchor = AnchorStyles.None
            };
            CenterControl(pbArtifact, picPanel);
            mainTable.Controls.Add(picPanel, 0, 0);

            //==== 2. 圣遗物类型选择（居中） ====
            var posPanel = new FlowLayoutPanel
            {
                Width = 250,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.None
            };
            posPanel.Controls.Add(new Label
            {
                Text = "圣遗物类型:",
                Font = new Font("微软雅黑", 10F),
                Height = 50,
                Margin = new Padding(0, 5, 5, 0)
            });
            cmbPosition = new ComboBox
            {
                Font = new Font("微软雅黑", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                Margin = new Padding(0, 5, 0, 0)
            };
            cmbPosition.Items.AddRange(artifactPositions);
            cmbPosition.SelectedIndexChanged += CmbPosition_SelectedIndexChanged;
            posPanel.Controls.Add(cmbPosition);

            // 声明并初始化posContainer
            posContainer = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            CenterControl(posPanel, posContainer);
            mainTable.Controls.Add(posContainer, 0, 1);

            //==== 3. 主词条选择（居中） ====
            var mainStatPanel = new FlowLayoutPanel
            {
                Height = 50,
                Width = 250,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.None
            };
            mainStatPanel.Controls.Add(new Label
            {
                Text = "主词条:",
                Font = new Font("微软雅黑", 10F),
              Width =60,
                
                Margin = new Padding(0, 5, 5, 0)
            });
            cmbMainStat = new ComboBox
            {
                Font = new Font("微软雅黑", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Margin = new Padding(0, 5, 0, 0)
            };
            cmbMainStat.SelectedIndexChanged += CmbMainStat_SelectedIndexChanged;
            mainStatPanel.Controls.Add(cmbMainStat);

            // 声明并初始化mainStatContainer
            mainStatContainer = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            CenterControl(mainStatPanel, mainStatContainer);
            mainTable.Controls.Add(mainStatContainer, 0, 2);

            //==== 4. 副词条选择（居中 + 3列布局） ====
            var substatsGroup = new GroupBox
            {
                Text = "选择2个副词条",
                Font = new Font("微软雅黑", 10F),
                Width = 550,
                Height =140,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.None
            };

            var substatsGrid = new TableLayoutPanel
            {
                ColumnCount = 4,
                Width = 530,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(5, 5, 5, 5),
                Margin = new Padding(0, 0, 0, 0),
                Anchor = AnchorStyles.None,
                Location = new Point(10, 30)
            };

            chkSubstats = new CheckBox[substats.Length];
            for (int i = 0; i < substats.Length; i++)
            {
                chkSubstats[i] = new CheckBox
                {
                    Text = substats[i],
                    Font = new Font("微软雅黑", 9F),
                    Width =110,
                    Margin = new Padding(3, 3, 3, 3)
                };
                chkSubstats[i].CheckedChanged += ChkSubstats_CheckedChanged;
                substatsGrid.Controls.Add(chkSubstats[i], i % 4, i / 4);
            }

            substatsGroup.Controls.Add(substatsGrid);

            // 声明并初始化substatsContainer
            substatsContainer = new Panel
            {
                Width = 100,
                Dock = DockStyle.Fill
            };
            CenterControl(substatsGroup, substatsContainer);
            mainTable.Controls.Add(substatsContainer, 0, 3);

            //==== 5. 生-成的副词条（居中） ====
            var generatedGroup = new GroupBox
            {
                Text = "当前副词条",
                Font = new Font("微软雅黑", 10F),
                Width = 320,
                Height = 120,
               
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.None
            };

            lbGeneratedSubstats = new ListBox
            {
                Font = new Font("微软雅黑", 10F),
                IntegralHeight = false,
                Padding = new Padding(5, 15, 5, 5),
                Width = 300
            };

            var generatedPanel = new Panel
            {
                Width = 300,
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 15, 5, 5)
            };
            lbGeneratedSubstats.Location = new Point(10, 25);
            CenterControl(lbGeneratedSubstats, generatedPanel);
            generatedGroup.Controls.Add(generatedPanel);

            // 声明并初始化generatedContainer
            generatedContainer = new Panel
            {
                Width = 200,
                Dock = DockStyle.Fill
            };
            CenterControl(generatedGroup, generatedContainer);
            mainTable.Controls.Add(generatedContainer, 0, 4);

            //==== 6. 操作按钮（居中） ====
            var buttonPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.None
            };

            btnConfirm = new Button
            {
                Text = "确认",
                Font = new Font("微软雅黑", 12F),
                Size = new Size(100, 35),
                Margin = new Padding(0)

            };
            btnConfirm.Click += BtnConfirm_Click;

            buttonPanel.Controls.Add(btnConfirm);

            var buttonContainer = new Panel
            {
                Width = mainTable.Width - mainTable.Padding.Horizontal,
                Dock = DockStyle.Fill,
                Height = 50
            };
            
            
            buttonContainer.Controls.Add(buttonPanel);
            buttonPanel.Location = new Point(
                (buttonContainer.Width - buttonPanel.Width) / 2, 0); // 水平居中计算
            mainTable.Controls.Add(buttonContainer, 0, 5);

            lblConsumedCount = new Label
            {
                Text = "已消耗祝圣之霜：0",
                Font = new Font("微软雅黑", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
            BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 200, 10)
            };
            this.Controls.Add(lblConsumedCount);
            lblConsumedCount.BringToFront();

            // 确保窗口大小改变时位置不变
            this.Resize += (s, e) => {
                lblConsumedCount.Location = new Point(this.ClientSize.Width - 200, 10);
            };
            // 初始化默认值
            if (cmbPosition.Items.Count > 0)
            {
                cmbPosition.SelectedIndex = 0;
                UpdateMainStatOptions();
            }
            foreach (var chk in chkSubstats)
            {
                chk.Enabled = false;
            }

        }


        // 辅助方法：居中控件
        private void CenterControl(Control control, Panel container)
        {
            control.Anchor = AnchorStyles.None;
            container.Controls.Add(control);
            container.Resize += (s, e) =>
            {
                control.Location = new Point(
                    (container.ClientSize.Width - control.Width) / 2,
                    (container.ClientSize.Height - control.Height) / 2
                );
            };

            // 立即触发一次定位
            control.Location = new Point(
                (container.ClientSize.Width - control.Width) / 2,
                (container.ClientSize.Height - control.Height) / 2
            );
        }

        private void UpdateMainStatOptions()
        {
            cmbMainStat.BeginUpdate();
            cmbMainStat.Items.Clear();
            cmbMainStat.SelectedIndex = -1; // 清除当前选择
            selectedMainStat = ""; // 清空当前选择的主词条

            if (mainStatOptions.TryGetValue(selectedPosition, out var stats))
            {
                cmbMainStat.Items.AddRange(stats);
            }
            cmbMainStat.EndUpdate();

            // 重置副词条选择
            ResetSubstatsSelection();
        }

        private void CmbPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPosition.SelectedItem == null) return;

            selectedPosition = cmbPosition.SelectedItem.ToString();

            // 更新圣遗物图片
            if (artifactImages.ContainsKey(selectedPosition))
            {
                pbArtifact.Image = artifactImages[selectedPosition];
            }

            // 更新主词条选项
            UpdateMainStatOptions();

            // 重置副词条选择
            ResetSubstatsSelection();
        }

        private void CmbMainStat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMainStat.SelectedItem == null)
            {
                selectedMainStat = "";
            }
            else
            {
                selectedMainStat = cmbMainStat.SelectedItem.ToString();
            }

            // 主词条变化时强制更新副词条选项
            UpdateSubstatsOptions();
        }


        private void UpdateSubstatsOptions()
        {
            bool hasMainStat = !string.IsNullOrEmpty(selectedMainStat);

            foreach (var chk in chkSubstats)
            {
                if (chk != null)
                {
                    chk.Enabled = hasMainStat && (chk.Text != selectedMainStat);
                    if (!chk.Enabled) chk.Checked = false;
                }
            }

            // 如果主词条变化，清除不匹配的副词条
            selectedSubstats.RemoveAll(s => s == selectedMainStat);
            SafeClearListBox();
            generatedSubstats.Clear();

            // 刷新复选框状态
            foreach (var chk in chkSubstats)
            {
                chk.Checked = selectedSubstats.Contains(chk.Text);
            }
        }
        private void SafeClearListBox()
        {
            if (lbGeneratedSubstats != null && !lbGeneratedSubstats.IsDisposed)
            {
                lbGeneratedSubstats.Items.Clear();
            }
        }
        private void ResetSubstatsSelection()
        {
            for (int i = 0; i < substats.Length; i++)
            {
                chkSubstats[i].Checked = false;
                chkSubstats[i].Enabled = true;
            }

            selectedSubstatNames.Clear();
            allSubstatValues.Clear();
            lbGeneratedSubstats.Items.Clear();
            btnConfirm.Enabled = false;
        }

        private void LoadArtifactImages()
        {
            try
            {
                // 检查并加载所有圣遗物部位的图片
                if (Properties.Resources.生之花 != null)
                    artifactImages["生之花"] = Properties.Resources.生之花;

                if (Properties.Resources.死之羽 != null)
                    artifactImages["死之羽"] = Properties.Resources.死之羽;

                if (Properties.Resources.时之沙 != null)
                    artifactImages["时之沙"] = Properties.Resources.时之沙;

                if (Properties.Resources.空之杯 != null)
                    artifactImages["空之杯"] = Properties.Resources.空之杯;

                if (Properties.Resources.理之冠 != null)
                    artifactImages["理之冠"] = Properties.Resources.理之冠;

                // 如果圣遗物类型已选择，则更新图片显示
                if (!string.IsNullOrEmpty(selectedPosition) &&
                    artifactImages.ContainsKey(selectedPosition))
                {
                    pbArtifact.Image = artifactImages[selectedPosition];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载圣遗物图片时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChkSubstats_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk == null) return;

            if (string.IsNullOrEmpty(selectedMainStat))
            {
                chk.Checked = false;
                return;
            }

            if (chk.Checked)
            {
                if (selectedSubstatNames.Count < 2)
                {
                    selectedSubstatNames.Add(chk.Text);
                }
                else
                {
                    chk.Checked = false;
                    return;
                }
            }
            else
            {
                selectedSubstatNames.Remove(chk.Text);
            }

            // 更新按钮状态
            btnConfirm.Enabled = selectedSubstatNames.Count == 2;

            // 更新显示（只显示属性名）
            UpdateSubstatsDisplay();
        }

        private float GetRandomSubstatValue(string substat)
        {
            // 定义每种属性的可能值范围
            var valueRanges = new Dictionary<string, float[]>()
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

            if (valueRanges.TryGetValue(substat, out var values))
            {
                return values[random.Next(values.Length)];
            }
            return 0f;
        }

   

        private void UpdateSubstatsDisplay()
        {
            lbGeneratedSubstats.Items.Clear();

            // 只显示选中的属性名（不显示值）
            foreach (var stat in selectedSubstatNames)
            {
                lbGeneratedSubstats.Items.Add(stat);
            }
        }

        private string GetSubstatUnit(string substat)
        {
            return substat.Contains("百分比") || substat.Contains("率") || substat.Contains("伤害") ? "%" : "";
        }
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (selectedSubstatNames.Count < 2) return;

            // 1. 为已选属性生成值
            allSubstatValues.Clear();
            foreach (var stat in selectedSubstatNames)
            {
                allSubstatValues[stat] = GetRandomSubstatValue(stat);
            }

            // 2. 生成剩余副词条(1-2条)
            var available = substats.Except(selectedSubstatNames)
                                 .Where(s => s != selectedMainStat)
                                 .ToList();

            int toGenerate = random.Next(1, 3); // 随机1-2条
            for (int i = 0; i < Math.Min(toGenerate, available.Count); i++)
            {
                int index = random.Next(available.Count);
                string substat = available[index];
                allSubstatValues[substat] = GetRandomSubstatValue(substat);
                available.RemoveAt(index);
            }

            // 3. 更新显示完整信息
            ShowFinalSubstats();

            // 4. 创建圣遗物对象
            var newArtifact = CreateArtifactWithValues();

            // 5. 添加到数据管理器
            ArtifactDataManager.Instance.AddArtifact(newArtifact);

            // 重置UI
            consumedCount++;
            lblConsumedCount.Text = $"已消耗祝圣之霜：{consumedCount}";
            System.Media.SystemSounds.Beep.Play();
        }

        private void ShowFinalSubstats()
        {
            lbGeneratedSubstats.Items.Clear();
            foreach (var kvp in allSubstatValues)
            {
                string unit = GetSubstatUnit(kvp.Key);
                string valueStr = unit == "%" ? $"{kvp.Value:F1}{unit}" : $"{(int)kvp.Value}{unit}";
                lbGeneratedSubstats.Items.Add($"{kvp.Key}: {valueStr}");
            }
        }

        private Artifact CreateArtifactWithValues()
        {
            var substatStrings = new List<string>();
            foreach (var kvp in allSubstatValues)
            {
                string unit = GetSubstatUnit(kvp.Key);
                string valueStr = unit == "%" ? $"{kvp.Value:F1}{unit}" : $"{(int)kvp.Value}{unit}";
                substatStrings.Add($"{kvp.Key}: {valueStr}");
            }

            return new Artifact
            {
                Id = Guid.NewGuid().ToString(),
                Position = selectedPosition,
                MainStat = selectedMainStat,
                SubStats = substatStrings,
                Level = 0,
                SelectedSubstatsUpgradedCount = 0,
                Source = ArtifactSource.Customized,
                SelectedSubstats = new List<string>(selectedSubstatNames) // 保存最初选中的2条
            };
        }

    }
}