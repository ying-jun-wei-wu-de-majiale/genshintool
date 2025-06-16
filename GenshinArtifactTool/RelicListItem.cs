using System;
using System.Drawing;
using System.Windows.Forms;
using GenshinArtifactTool;
// 移除MonoBehaviour继承，改为普通类
public class RelicListItem : UserControl  // 或者继承Panel/其他控件
{
    public PictureBox relicIcon;
    public Label levelLabel;

    private Relic relic;
    private InventoryForm inventoryForm;

    public RelicListItem()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // 初始化控件
        this.Width = 100;
        this.Height = 100;

        relicIcon = new PictureBox
        {
            Width = 80,
            Height = 80,
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        levelLabel = new Label
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(100, 0, 0, 0),
            ForeColor = Color.White
        };

        this.Controls.Add(relicIcon);
        this.Controls.Add(levelLabel);

        // 添加点击事件

    }

    public void Initialize(Relic relic, InventoryForm form)
    {
        this.relic = relic;
        this.inventoryForm = form;

        // 设置图标和文本
        relicIcon.Image = GetRelicIcon(relic.type);
        levelLabel.Text = $"Lv.{relic.level}";
    }

    private Image GetRelicIcon(RelicType type)
    {
        // 从资源文件或磁盘加载图片
        // 示例：return Image.FromFile($"icons/{type}.png");
        return new Bitmap(80, 80); // 返回一个临时图片
    }

    
}