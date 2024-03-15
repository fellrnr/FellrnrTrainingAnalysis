namespace FellrnrTrainingAnalysis.UI
{
    partial class ActivityFilterDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            menuStrip1 = new MenuStrip();
            applyAndCloseToolStripMenuItem = new ToolStripMenuItem();
            applyToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            clearAndCloseToolStripMenuItem = new ToolStripMenuItem();
            tableLayoutPanel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.Controls.Add(label4, 3, 0);
            tableLayoutPanel1.Location = new Point(12, 55);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1137, 1134);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(49, 25);
            label1.TabIndex = 0;
            label1.Text = "Field";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(58, 0);
            label2.Name = "label2";
            label2.Size = new Size(50, 25);
            label2.TabIndex = 1;
            label2.Text = "Filter";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(114, 0);
            label3.Name = "label3";
            label3.Size = new Size(69, 25);
            label3.TabIndex = 2;
            label3.Text = "Value 1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(189, 0);
            label4.Name = "label4";
            label4.Size = new Size(69, 25);
            label4.TabIndex = 3;
            label4.Text = "Value 2";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { applyAndCloseToolStripMenuItem, applyToolStripMenuItem, closeToolStripMenuItem, clearAndCloseToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1161, 33);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // applyAndCloseToolStripMenuItem
            // 
            applyAndCloseToolStripMenuItem.Name = "applyAndCloseToolStripMenuItem";
            applyAndCloseToolStripMenuItem.Size = new Size(158, 29);
            applyAndCloseToolStripMenuItem.Text = "Apply and Close";
            applyAndCloseToolStripMenuItem.Click += applyAndCloseToolStripMenuItem_Click;
            // 
            // applyToolStripMenuItem
            // 
            applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            applyToolStripMenuItem.Size = new Size(75, 29);
            applyToolStripMenuItem.Text = "Apply";
            applyToolStripMenuItem.Click += applyToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(71, 29);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // clearAndCloseToolStripMenuItem
            // 
            clearAndCloseToolStripMenuItem.Name = "clearAndCloseToolStripMenuItem";
            clearAndCloseToolStripMenuItem.Size = new Size(153, 29);
            clearAndCloseToolStripMenuItem.Text = "Clear And Close";
            clearAndCloseToolStripMenuItem.Click += clearAndCloseToolStripMenuItem_Click;
            // 
            // ActivityFilterDialog
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1161, 1201);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "ActivityFilterDialog";
            Text = "ActivityFilter";
            FormClosing += ActivityFilterDialog_FormClosing;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem applyAndCloseToolStripMenuItem;
        private ToolStripMenuItem applyToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem clearAndCloseToolStripMenuItem;
    }
}