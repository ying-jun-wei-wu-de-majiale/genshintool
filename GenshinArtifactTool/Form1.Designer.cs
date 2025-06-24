using System.Windows.Forms;
using System;

namespace GenshinArtifactTool
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnArtifactUpgrade;
        private System.Windows.Forms.Button btnInventory;
        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
  
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(825, 759);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "原神圣遗物助手";
            this.ResumeLayout(false);

        }

        #endregion



        private void InitializeAllForms()
        {
            // 初始化所有功能窗体
            inventoryForm = new InventoryForm();
            inventoryForm.MainForm = this;

            // 刷圣遗物界面
            artifactBrushForm = new ArtifactBrushForm();
            artifactBrushForm.MainForm = this;
            artifactBrushForm.SetInventoryForm(inventoryForm);

            // 圣遗物升级界面
            artifactUpgradeForm = new ArtifactUpgradeForm();
            artifactUpgradeForm.TopLevel = false;
            artifactUpgradeForm.FormBorderStyle = FormBorderStyle.None;

            // 祝圣之霜界面 - 关键初始化
            artifactSelectionForm = new ArtifactSelectionForm();
            artifactSelectionForm.TopLevel = false;
            artifactSelectionForm.FormBorderStyle = FormBorderStyle.None;
            artifactUpgradeForm.UpgradeCompleted += (artifact) =>
            {
                // 更新数据管理器中的圣遗物
                ArtifactDataManager.Instance.UpdateArtifact(artifact);

                // 刷新背包显示
                if (inventoryForm != null)
                {
                    inventoryForm.RefreshArtifacts();
                }
            };
            // 其他窗体初始化...
        }



    }
}