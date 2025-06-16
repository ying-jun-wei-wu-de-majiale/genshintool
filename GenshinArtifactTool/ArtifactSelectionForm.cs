using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinArtifactTool
{
    public partial class ArtifactSelectionForm : Form
    {
        private List<Image> artifactImages;
        private FlowLayoutPanel flowLayoutPanel;

        // 用户选择的圣遗物索引
        public int SelectedIndex { get; private set; }

        public ArtifactSelectionForm(List<Image> images, int currentIndex)
        {
            InitializeComponent();
            this.artifactImages = images;
            this.SelectedIndex = currentIndex;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "选择圣遗物";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            // 创建FlowLayoutPanel用于显示所有圣遗物
            flowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.Padding = new Padding(10);
            // 使用FlowLayoutPanel的Padding和Margin代替Spacing
            this.Controls.Add(flowLayoutPanel);

            // 添加所有圣遗物图片到面板
            AddArtifactImages();

            // 创建确定按钮
            Button btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.Location = new Point(480, 340);
            btnOK.Size = new Size(100, 30);
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);
        }

        private void AddArtifactImages()
        {
            for (int i = 0; i < artifactImages.Count; i++)
            {
                int index = i; // 捕获循环变量
                PictureBox pb = new PictureBox();
                pb.Image = artifactImages[i];
                pb.Size = new Size(80, 80);
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                // 使用Padding代替Spacing
                pb.Margin = new Padding(5);
                pb.Cursor = Cursors.Hand;

                // 添加点击事件
                pb.Click += (sender, e) =>
                {
                    // 重置所有图片的边框
                    foreach (Control control in flowLayoutPanel.Controls)
                    {
                        if (control is PictureBox)
                        {
                            // 强制转换为PictureBox以访问BorderStyle属性
                            ((PictureBox)control).BorderStyle = BorderStyle.None;
                        }
                    }

                    // 设置当前选中图片的边框
                    pb.BorderStyle = BorderStyle.FixedSingle;
                    SelectedIndex = index;
                };

                flowLayoutPanel.Controls.Add(pb);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}