namespace CADCore
{
    partial class DocumentControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("2D");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("3D");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Тела");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Модель", new System.Windows.Forms.TreeNode[] {
            treeNode7,
            treeNode8,
            treeNode9});
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Импорт");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Узел0", new System.Windows.Forms.TreeNode[] {
            treeNode10,
            treeNode11});
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.DocumentPage = new System.Windows.Forms.TabPage();
            this.DocumentTree = new System.Windows.Forms.TreeView();
            this.PropertiesPage = new System.Windows.Forms.TabPage();
            this.ElementProperties = new System.Windows.Forms.PropertyGrid();
            this.RenderWorkPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.DocumentPage.SuspendLayout();
            this.PropertiesPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.RenderWorkPanel);
            this.splitContainer1.Size = new System.Drawing.Size(567, 415);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.DocumentPage);
            this.tabControl1.Controls.Add(this.PropertiesPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(181, 415);
            this.tabControl1.TabIndex = 0;
            // 
            // DocumentPage
            // 
            this.DocumentPage.Controls.Add(this.DocumentTree);
            this.DocumentPage.Location = new System.Drawing.Point(4, 22);
            this.DocumentPage.Name = "DocumentPage";
            this.DocumentPage.Padding = new System.Windows.Forms.Padding(3);
            this.DocumentPage.Size = new System.Drawing.Size(173, 389);
            this.DocumentPage.TabIndex = 0;
            this.DocumentPage.Text = "Документ";
            this.DocumentPage.UseVisualStyleBackColor = true;
            // 
            // DocumentTree
            // 
            this.DocumentTree.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.DocumentTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DocumentTree.Location = new System.Drawing.Point(3, 3);
            this.DocumentTree.Name = "DocumentTree";
            treeNode7.Name = "2D";
            treeNode7.Text = "2D";
            treeNode8.Name = "3D";
            treeNode8.Text = "3D";
            treeNode9.Name = "Bodies";
            treeNode9.Text = "Тела";
            treeNode10.Name = "Model";
            treeNode10.Text = "Модель";
            treeNode11.Name = "Import";
            treeNode11.Text = "Импорт";
            treeNode12.Name = "DocumentName";
            treeNode12.Text = "Узел0";
            this.DocumentTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode12});
            this.DocumentTree.Size = new System.Drawing.Size(167, 383);
            this.DocumentTree.TabIndex = 0;
            this.DocumentTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DocumentTree_NodeMouseDoubleClick);
            // 
            // PropertiesPage
            // 
            this.PropertiesPage.Controls.Add(this.ElementProperties);
            this.PropertiesPage.Location = new System.Drawing.Point(4, 22);
            this.PropertiesPage.Name = "PropertiesPage";
            this.PropertiesPage.Padding = new System.Windows.Forms.Padding(3);
            this.PropertiesPage.Size = new System.Drawing.Size(173, 389);
            this.PropertiesPage.TabIndex = 1;
            this.PropertiesPage.Text = "Свойства";
            this.PropertiesPage.UseVisualStyleBackColor = true;
            // 
            // ElementProperties
            // 
            this.ElementProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ElementProperties.Location = new System.Drawing.Point(3, 3);
            this.ElementProperties.Name = "ElementProperties";
            this.ElementProperties.Size = new System.Drawing.Size(167, 383);
            this.ElementProperties.TabIndex = 0;
            // 
            // RenderWorkPanel
            // 
            this.RenderWorkPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.RenderWorkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RenderWorkPanel.Location = new System.Drawing.Point(0, 0);
            this.RenderWorkPanel.Name = "RenderWorkPanel";
            this.RenderWorkPanel.Size = new System.Drawing.Size(382, 415);
            this.RenderWorkPanel.TabIndex = 0;
            // 
            // DocumentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DocumentControl";
            this.Size = new System.Drawing.Size(567, 415);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.DocumentPage.ResumeLayout(false);
            this.PropertiesPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage DocumentPage;
        private System.Windows.Forms.TabPage PropertiesPage;
        protected internal System.Windows.Forms.PropertyGrid ElementProperties;
        protected internal System.Windows.Forms.TreeView DocumentTree;
        protected internal System.Windows.Forms.Panel RenderWorkPanel;
    }
}
