using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GenshinArtifactTool;
using static GenshinArtifactTool.Artifact;

public partial class InventoryForm : Form
{
    private readonly ArtifactDataManager _dataManager = ArtifactDataManager.Instance;
    public Form MainForm { get; set; }
    private System.ComponentModel.IContainer components = null;
    // 圣遗物类型图标数据
    private Dictionary<string, Image> artifactIcons = new Dictionary<string, Image>();
    private List<Artifact> artifacts = new List<Artifact>();
    private Panel detailPanel;
    private ListBox inventoryListBox;
    private FlowLayoutPanel flowLayoutPanel;
    public InventoryForm()
    {
        InitializeUI();
        LoadArtifactIcons();

        // 订阅数据管理器的事件

        ArtifactDataManager.Instance.ArtifactAdded += OnArtifactAdded;
        ArtifactDataManager.Instance.ArtifactUpdated += OnArtifactUpdated;
        ArtifactDataManager.Instance.ArtifactRemoved += OnArtifactRemoved;

        // 初始化加载现有数据
        LoadArtifacts();
    }
    private void LoadArtifacts()
    {
        artifacts.Clear();
        var allArtifacts = ArtifactDataManager.Instance.GetAllArtifacts();
        artifacts.AddRange(allArtifacts);
        RefreshArtifactDisplay();

        // 调试输出
        Debug.WriteLine($"背包加载圣遗物数量: {artifacts.Count}");
    }
    private void LoadArtifactsFromManager()
    {
        artifacts.Clear();
        artifacts.AddRange(ArtifactDataManager.Instance.GetAllArtifacts());
        RefreshArtifactDisplay();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 取消事件订阅
        ArtifactDataManager.Instance.ArtifactAdded -= OnArtifactAdded;
        ArtifactDataManager.Instance.ArtifactUpdated -= OnArtifactUpdated;
        ArtifactDataManager.Instance.ArtifactRemoved -= OnArtifactRemoved;

        base.OnFormClosing(e);
    }

    // 在InventoryForm.cs中
    public void RefreshArtifacts()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(RefreshArtifacts));
            return;
        }
        if (detailPanel == null || detailPanel.IsDisposed)
        {
            Debug.WriteLine("警告: detailPanel未初始化或已被释放");
            return;
        }
        // 清空现有显示
        detailPanel.Controls.Clear();

        // 重新从数据管理器加载
        var allArtifacts = ArtifactDataManager.Instance.GetAllArtifacts();

        // 重新创建显示面板
        foreach (var artifact in allArtifacts)
        {
            var panel = CreateArtifactDisplayPanel(artifact);
            detailPanel.Controls.Add(panel);
        }
    }
    private void AddArtifactToPanel(Artifact artifact)
    {
        // 创建圣遗物显示面板
        var panel = new Panel();
        panel.Width = 200;
        panel.Height = 120;
        panel.BorderStyle = BorderStyle.FixedSingle;
        panel.Margin = new Padding(5);

        // 添加圣遗物部位标签
        var lblPosition = new Label();
        lblPosition.Text = artifact.Position;
        lblPosition.Font = new Font("微软雅黑", 10, FontStyle.Bold);
        lblPosition.Location = new Point(10, 10);
        panel.Controls.Add(lblPosition);

        // 添加主词条标签
        var lblMainStat = new Label();
        lblMainStat.Text = artifact.MainStat;
        lblMainStat.Font = new Font("微软雅黑", 9);
        lblMainStat.Location = new Point(10, 35);
        panel.Controls.Add(lblMainStat);

        // 添加等级标签
        var lblLevel = new Label();
        lblLevel.Text = $"+{artifact.Level}";
        lblLevel.Font = new Font("微软雅黑", 9);
        lblLevel.Location = new Point(150, 10);
        panel.Controls.Add(lblLevel);

        // 添加按钮或其他控件...

        // 将面板添加到流式布局
        flowLayoutPanel.Controls.Add(panel);
    }


    private void OnArtifactAdded(Artifact artifact)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => HandleArtifactAdded(artifact)));
            return;
        }
        HandleArtifactAdded(artifact);
    }

    private void HandleArtifactAdded(Artifact artifact)
    {
        artifacts.Add(artifact);
        RefreshArtifactDisplay();

        // 调试输出
        Debug.WriteLine($"新增圣遗物: {artifact.Position} - {artifact.MainStat}");
        Debug.WriteLine($"当前圣遗物总数: {artifacts.Count}");
    }

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
        var index = artifacts.FindIndex(a => a.Id == artifact.Id);
        if (index >= 0)
        {
            artifacts[index] = artifact;
            RefreshArtifactDisplay();
        }
        Debug.WriteLine($"背包更新圣遗物: {artifact.Position} - {artifact.MainStat} - Level {artifact.Level}");
    }

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
        artifacts.RemoveAll(a => a.Id == artifact.Id);
        RefreshArtifactDisplay();
    }

    private Panel CreateArtifactDisplayPanel(Artifact artifact)
    {
        
        var panel = new Panel
        {
            Width = 300,  // 稍微减小宽度
            Height = 180,
            BorderStyle = BorderStyle.FixedSingle,
            Tag = artifact,
            BackColor = Color.FromArgb(236, 229, 216)
        };
        if (artifact.Source == ArtifactSource.Customized)
        {
            panel.BorderStyle = BorderStyle.Fixed3D;
            panel.BackColor = Color.FromArgb(240, 230, 255); // 浅蓝紫色背景
        }

        // 添加升级按钮
        var btnUpgrade = new Button
        {
            Text = artifact.Level < 20 ? "升级" : "已满级",
            Enabled = artifact.Level < 20,
            Tag = artifact.Id,
            Size = new Size(80, 30),
            BackColor= Color.FromArgb(236, 229, 216),
            Location = new Point(200, 130)
        };
        btnUpgrade.Click += (s, e) => UpgradeArtifact(artifact.Id);
        panel.Controls.Add(btnUpgrade);
        // 添加等级显示（新增部分）
        var lblLevel = new Label
        {
            Text = $"+{artifact.Level}",
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.Red,
            Location = new Point(250, 15),
            AutoSize = true
        };
        panel.Controls.Add(lblLevel);
        // 圣遗物图标
        var iconPb = new PictureBox
        {
            Image = GetArtifactIcon(artifact.Position),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(60, 60),
            Location = new Point(15, 15),
            BorderStyle = BorderStyle.None
        };
        panel.Controls.Add(iconPb);

        // 圣遗物部位标签
        var lblPosition = new Label
        {
            Text = artifact.Position,
            Font = new Font("微软雅黑", 12F, FontStyle.Bold),
            Location = new Point(85, 15),
            ForeColor = Color.FromArgb(51, 122, 183)
        };
        panel.Controls.Add(lblPosition);


        // 主词条标签
        var lblMainStat = new Label
        {
            Text = $"主词条: {artifact.MainStat}",
            Font = new Font("微软雅黑", 10F, FontStyle.Bold),
            Location = new Point(85, 45),
            AutoSize = true
        };
        panel.Controls.Add(lblMainStat);

        // 副词条区域
        int subY = 75;
        foreach (var substat in artifact.SubStats)
        {
            var lbl = new Label
            {
                Text = substat,
                Font = new Font("微软雅黑", 9F),
                Location = new Point(15, subY),
                AutoSize = true
            };
            // 如果是祝圣之霜圣遗物且是自选属性，设置特殊颜色
            string substatName = substat.Split(':')[0].Trim();
            if (artifact.Source == ArtifactSource.Customized &&
                artifact.SelectedSubstats != null &&
                artifact.SelectedSubstats.Any(s => substat.StartsWith(s)))
            {
                lbl.ForeColor = Color.Goldenrod; // 设置自选属性颜色为金色
                lbl.Font = new Font(lbl.Font, FontStyle.Bold); // 加粗显示
            }
            panel.Controls.Add(lbl);
            subY += 22;
        }

        return panel;
    }
    private void UpgradeArtifact(string artifactId)
    {
        var artifact = artifacts.FirstOrDefault(a => a.Id == artifactId);
        if (artifact == null || artifact.Level >= 20) return;

        // 打开升级界面
        if (MainForm is Form1 mainForm)
        {
            mainForm.ShowArtifactUpgrade(artifact);
        }
    }
    private Image GetArtifactIcon(string position)
    {
        if (artifactIcons.ContainsKey(position))
        {
            return artifactIcons[position];
        }
        else if (!string.IsNullOrEmpty(position))
        {
            return GeneratePlaceholderImage(position);
        }
        return null;
    }


    private void InitializeUI()
    {
        this.Text = "圣遗物背包";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        var mainFlowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10)
        };
        this.Controls.Add(mainFlowPanel);
        
        InitializeArtifactDisplay(mainFlowPanel);
    }
    private void InitializeArtifactDisplay(FlowLayoutPanel container)
    {
        // 清空现有控件
        container.Controls.Clear();

        // 添加所有圣遗物面板
        foreach (var artifact in artifacts)
        {
            var panel = CreateArtifactDisplayPanel(artifact);
            panel.Margin = new Padding(10); // 设置间距
            container.Controls.Add(panel);
        }
    }

    private void RefreshArtifactDisplay()
    {
        // 直接使用主FlowLayoutPanel刷新
        if (this.Controls.Count > 0 && this.Controls[0] is FlowLayoutPanel mainPanel)
        {
            InitializeArtifactDisplay(mainPanel);
        }
    }

    private void ShowArtifactDetails(string artifactType)
    {
        if (this.Controls.Count > 0 && this.Controls[0] is FlowLayoutPanel mainPanel)
        {
            mainPanel.Controls.Clear();

            var typeArtifacts = artifacts.Where(a => a.Position == artifactType).ToList();
            foreach (var artifact in typeArtifacts)
            {
                var panel = CreateArtifactDisplayPanel(artifact);
                panel.Margin = new Padding(10);
                mainPanel.Controls.Add(panel);
            }
        }
    }

    private void LoadArtifactIcons()
    {
        try
        {
            // 直接从项目资源中加载图标
            artifactIcons["生之花"] = GenshinArtifactTool.Properties.Resources.生之花;
            artifactIcons["死之羽"] = GenshinArtifactTool.Properties.Resources.死之羽;
            artifactIcons["时之沙"] = GenshinArtifactTool.Properties.Resources.时之沙;
            artifactIcons["空之杯"] = GenshinArtifactTool.Properties.Resources.空之杯;
            artifactIcons["理之冠"] = GenshinArtifactTool.Properties.Resources.理之冠;

            Debug.WriteLine("成功从项目资源加载所有圣遗物图标");
        }
        catch (Exception ex)
        {
            // 加载失败时使用占位图
            LoadPlaceholderIcons();
            Debug.WriteLine($"从项目资源加载图标失败: {ex.Message}");
        }
    }

    private Image LoadIconFromFile(string directory, string fileName)
    {
        string filePath = Path.Combine(directory, fileName);
        if (File.Exists(filePath))
        {
            return Image.FromFile(filePath);
        }
        return null;
    }

    private Image LoadIconIfExists(string resourcesPath, string fileName)
    {
        string filePath = Path.Combine(resourcesPath, fileName);
        if (File.Exists(filePath))
        {
            return Image.FromFile(filePath);
        }
        Debug.WriteLine($"图标文件不存在: {filePath}");
        return null;
    }

    private void LoadPlaceholderIcons()
    {
        foreach (var type in new[] { "生之花", "死之羽", "时之沙", "空之杯", "理之冠" })
        {
            artifactIcons[type] = GeneratePlaceholderImage(type);
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
}