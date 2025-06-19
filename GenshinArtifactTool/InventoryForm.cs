using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class InventoryForm : Form
{
    public Form MainForm { get; set; } 
    private System.ComponentModel.IContainer components = null;
    // 圣遗物类型图标数据
    private Dictionary<string, Image> artifactIcons = new Dictionary<string, Image>();
    private List<Artifact> artifacts = new List<Artifact>(); // 假设有Artifact类                                                           
    private Panel detailPanel;
    public InventoryForm()
    {
        
        InitializeUI();

        // 订阅数据管理器的事件
        ArtifactDataManager.Instance.ArtifactAdded += OnArtifactAdded;
        ArtifactDataManager.Instance.ArtifactUpdated += OnArtifactUpdated;
        ArtifactDataManager.Instance.ArtifactRemoved += OnArtifactRemoved;

        // 初始化加载现有数据
        LoadArtifactsFromManager();
    }
    private void LoadArtifactsFromManager()
    {
        // 清空现有数据
        artifacts.Clear();

        // 从单例获取所有圣遗物
        artifacts.AddRange(ArtifactDataManager.Instance.GetAllArtifacts());

        // 更新界面
        RefreshArtifactDisplay();
    }
    // 当添加新圣遗物时的回调
    private void OnArtifactAdded(Artifact artifact)
    {
        // 在UI线程上更新界面
        if (InvokeRequired)
        {
            Invoke(new Action(() => HandleArtifactAdded(artifact)));
            return;
        }

        HandleArtifactAdded(artifact);
    }

    private void HandleArtifactAdded(Artifact artifact)
    {
        // 将新圣遗物添加到本地列表
        artifacts.Add(artifact);

        // 更新界面显示（根据需要显示所有或特定类型的圣遗物）
        RefreshArtifactDisplay();
    }

    // 当圣遗物更新时的回调
    private void OnArtifactUpdated(Artifact artifact)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => HandleArtifactUpdated(artifact)));
            return;
        }

        HandleArtifactUpdated(artifact);
    }

    private void HandleArtifactUpdated(Artifact artifact)
    {
        // 找到并更新本地列表中的对应圣遗物
        var index = artifacts.FindIndex(a => a.Id == artifact.Id);
        if (index >= 0)
        {
            artifacts[index] = artifact;
            RefreshArtifactDisplay();
        }
    }

    // 当圣遗物被删除时的回调
    private void OnArtifactRemoved(Artifact artifact)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => HandleArtifactRemoved(artifact)));
            return;
        }

        HandleArtifactRemoved(artifact);
    }

    private void HandleArtifactRemoved(Artifact artifact)
    {
        // 从本地列表中移除圣遗物
        artifacts.RemoveAll(a => a.Id == artifact.Id);
        RefreshArtifactDisplay();
    }

    // 刷新圣遗物显示
    private void RefreshArtifactDisplay()
    {
        // 清除现有控件
        detailPanel.Controls.Clear();

        // 根据当前选中的类型显示圣遗物
        string selectedType = GetSelectedArtifactType();
        var filteredArtifacts = string.IsNullOrEmpty(selectedType)
            ? artifacts
            : artifacts.Where(a => a.Position == selectedType).ToList();

        // 显示筛选后的圣遗物
        int yPos = 10;
        foreach (var artifact in filteredArtifacts)
        {
            // 创建并添加圣遗物显示控件
            var artifactPanel = CreateArtifactDisplayPanel(artifact);
            artifactPanel.Location = new Point(10, yPos);
            detailPanel.Controls.Add(artifactPanel);
            yPos += 130;
        }
    }

    // 创建圣遗物显示面板（根据你的现有代码调整）
    private Panel CreateArtifactDisplayPanel(Artifact artifact)
    {
        var panel = new Panel
        {
            Width = detailPanel.Width - 20,
            Height = 120,
            BorderStyle = BorderStyle.FixedSingle,
            Tag = artifact
        };

        // 添加主词条标签
        var lblMainStat = new Label
        {
            Text = $"主词条: {artifact.MainStat}",
            Font = new Font("微软雅黑", 10F, FontStyle.Bold),
            Location = new Point(15, 15),
            AutoSize = true
        };
        panel.Controls.Add(lblMainStat);

        // 添加副词条标签
        int subY = 40;
        foreach (var substat in artifact.SubStats)
        {
            var lblSubStat = new Label
            {
                Text = substat,
                Font = new Font("微软雅黑", 9F),
                Location = new Point(15, subY),
                AutoSize = true
            };
            panel.Controls.Add(lblSubStat);
            subY += 20;
        }

        return panel;
    }

    // 获取当前选中的圣遗物类型
    private string GetSelectedArtifactType()
    {
        // 实现逻辑，根据你的UI设计获取当前选中的圣遗物类型
        // 例如，如果你有一个下拉框或选中的项
        return null; // 返回null表示显示所有类型
    }

    // 确保在窗体关闭时取消订阅事件，防止内存泄漏
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 取消订阅事件
        ArtifactDataManager.Instance.ArtifactAdded -= OnArtifactAdded;
        ArtifactDataManager.Instance.ArtifactUpdated -= OnArtifactUpdated;
        ArtifactDataManager.Instance.ArtifactRemoved -= OnArtifactRemoved;
    }
    private void InitializeUI()
    {
        this.Text = "圣遗物背包";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 主分割容器
        var mainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 150,
            FixedPanel = FixedPanel.Panel1
        };
        this.Controls.Add(mainSplitContainer);

        // 左侧图标列表
        InitializeLeftPanel(mainSplitContainer.Panel1);

        // 右侧属性详情
        InitializeRightPanel(mainSplitContainer.Panel2);
    }
    private void InitializeLeftPanel(Panel panel)
    {
        panel.BackColor = Color.FromArgb(240, 240, 240);

        // 滚动容器
        var scrollPanel = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill
        };
        panel.Controls.Add(scrollPanel);

        // 流式布局面板
        var flowPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Width = scrollPanel.ClientSize.Width - 20 // 留出滚动条空间
        };
        scrollPanel.Controls.Add(flowPanel);

        // 添加测试项
        foreach (var artifactType in new[] { "生之花", "死之羽", "时之沙", "空之杯", "理之冠" })
        {
            var itemPanel = new Panel
            {
                Width = flowPanel.Width - 5,
                Height = 60,
                Margin = new Padding(5),
                Tag = artifactType,
                Cursor = Cursors.Hand
            };

            // 图标
            var pb = new PictureBox
            {
                Image = artifactIcons.ContainsKey(artifactType) ? artifactIcons[artifactType] : null,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(50, 50),
                Location = new Point(5, 5)
            };

            // 类型名称
            var lbl = new Label
            {
                Text = artifactType,
                Font = new Font("微软雅黑", 10F),
                Location = new Point(60, 20),
                AutoSize = true
            };

            itemPanel.Controls.Add(pb);
            itemPanel.Controls.Add(lbl);

            // 点击事件
            itemPanel.Click += (s, e) =>
            {
                // 高亮选中项
                foreach (Control c in flowPanel.Controls)
                    c.BackColor = Color.Transparent;
                itemPanel.BackColor = Color.LightBlue;

                // 显示对应圣遗物
                ShowArtifactDetails(artifactType);
            };

            flowPanel.Controls.Add(itemPanel);
        }
    }
    private void InitializeRightPanel(Panel panel)
    {
        panel.BackColor = Color.White;
        panel.Padding = new Padding(20);

        // 详情容器（带滚动条）
        var detailScroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        panel.Controls.Add(detailScroll);

        // 详情内容面板
        detailPanel = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = detailScroll.ClientSize.Width - 20
        };
        detailScroll.Controls.Add(detailPanel);
    }

    private void ShowArtifactDetails(string artifactType)
    {
        detailPanel.Controls.Clear();

        // 获取该类型所有圣遗物
        var typeArtifacts = artifacts.Where(a => a.Position == artifactType).ToList();

        int yPos = 10;
        foreach (var artifact in typeArtifacts)
        {
            var artifactPanel = new Panel
            {
                Width = detailPanel.Width - 20,
                Height = 120,
                Location = new Point(10, yPos),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = artifact
            };

            // 添加圣遗物属性显示控件...
            var lblMainStat = new Label
            {
                Text = $"主词条: {artifact.MainStat}",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            // 副词条显示...
            int subY = 40;
            foreach (var substat in artifact.SubStats)
            {
                var lbl = new Label
                {
                    Text = substat,
                    Font = new Font("微软雅黑", 9F),
                    Location = new Point(15, subY),
                    AutoSize = true
                };
                artifactPanel.Controls.Add(lbl);
                subY += 20;
            }

            detailPanel.Controls.Add(artifactPanel);
            yPos += 130;
        }
    }
    private void LoadArtifactIcons()
    {
        try
        {
            // 使用绝对路径临时解决方案
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            artifactIcons["生之花"] = Image.FromFile(Path.Combine(basePath, "Resources", "生之花.png"));
            artifactIcons["死之羽"] = Image.FromFile(Path.Combine(basePath, "Resources", "死之羽.png"));
            artifactIcons["理之冠"] = Image.FromFile(Path.Combine(basePath, "Resources", "理之冠.png"));
            artifactIcons["空之杯"] = Image.FromFile(Path.Combine(basePath, "Resources", "空之杯.png"));
            artifactIcons["时之沙"] = Image.FromFile(Path.Combine(basePath, "Resources", "时之沙.png"));

            // 其他图标...
        }
        catch (Exception ex)
        {
            // 使用默认占位图
            artifactIcons["生之花"] = GeneratePlaceholderImage("花");
            artifactIcons["死之羽"] = GeneratePlaceholderImage("羽");
            // 其他图标...

            Debug.WriteLine($"加载图标失败: {ex.Message}");
        }
    }

    private Image GeneratePlaceholderImage(string text)
    {
        var bmp = new Bitmap(50, 50);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.LightGray);
            g.DrawRectangle(Pens.DarkGray, 0, 0, 49, 49);
            g.DrawString(text, new Font("微软雅黑", 8), Brushes.Black, 5, 15);
        }
        return bmp;
    }

    private void LoadSampleArtifacts()
    {
        // 测试数据
        artifacts.Add(new Artifact
        {
            Position = "生之花",
            MainStat = "生命值",
            SubStats = new List<string> { "攻击力百分比 5.8%", "暴击率 3.5%", "元素精通 23" }
        });
        // ...添加更多测试数据
    }
    public class Artifact
    {
        public string Position { get; set; } // 部位
        public string MainStat { get; set; } // 主词条
        public List<string> SubStats { get; set; } // 副词条
                                                   // 可以添加更多属性...
    }
}