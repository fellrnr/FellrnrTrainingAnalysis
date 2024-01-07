namespace FellrnrTrainingAnalysis.UI
{
    partial class OverviewMap
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            showComboBox = new ComboBox();
            label5 = new Label();
            sportComboBox = new ComboBox();
            label2 = new Label();
            widthNumericUpDown = new NumericUpDown();
            label3 = new Label();
            alphaNumericUpDown = new NumericUpDown();
            label4 = new Label();
            hillsComboBox = new ComboBox();
            hillOpComboBox = new ComboBox();
            hillNumericUpDown = new NumericUpDown();
            labelHillsCheckBox = new CheckBox();
            countLabel = new Label();
            gmap = new GMap.NET.WindowsForms.GMapControl();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)widthNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)alphaNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)hillNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(showComboBox);
            flowLayoutPanel1.Controls.Add(label5);
            flowLayoutPanel1.Controls.Add(sportComboBox);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(widthNumericUpDown);
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Controls.Add(alphaNumericUpDown);
            flowLayoutPanel1.Controls.Add(label4);
            flowLayoutPanel1.Controls.Add(hillsComboBox);
            flowLayoutPanel1.Controls.Add(hillOpComboBox);
            flowLayoutPanel1.Controls.Add(hillNumericUpDown);
            flowLayoutPanel1.Controls.Add(labelHillsCheckBox);
            flowLayoutPanel1.Controls.Add(countLabel);
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1481, 66);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(56, 25);
            label1.TabIndex = 2;
            label1.Text = "Show";
            // 
            // showComboBox
            // 
            showComboBox.FormattingEnabled = true;
            showComboBox.Location = new Point(65, 3);
            showComboBox.Name = "showComboBox";
            showComboBox.Size = new Size(182, 33);
            showComboBox.TabIndex = 3;
            showComboBox.Text = "None";
            showComboBox.SelectedIndexChanged += showComboBox_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(253, 0);
            label5.Name = "label5";
            label5.Size = new Size(56, 25);
            label5.TabIndex = 16;
            label5.Text = "Sport";
            // 
            // sportComboBox
            // 
            sportComboBox.FormattingEnabled = true;
            sportComboBox.Location = new Point(315, 3);
            sportComboBox.Name = "sportComboBox";
            sportComboBox.Size = new Size(182, 33);
            sportComboBox.TabIndex = 17;
            sportComboBox.SelectedIndexChanged += showComboBox_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(503, 0);
            label2.Name = "label2";
            label2.Size = new Size(60, 25);
            label2.TabIndex = 6;
            label2.Text = "Width";
            // 
            // widthNumericUpDown
            // 
            widthNumericUpDown.Location = new Point(569, 3);
            widthNumericUpDown.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            widthNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            widthNumericUpDown.Name = "widthNumericUpDown";
            widthNumericUpDown.Size = new Size(63, 31);
            widthNumericUpDown.TabIndex = 7;
            widthNumericUpDown.Value = new decimal(new int[] { 3, 0, 0, 0 });
            widthNumericUpDown.ValueChanged += widthNumericUpDown_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(638, 0);
            label3.Name = "label3";
            label3.Size = new Size(114, 25);
            label3.TabIndex = 8;
            label3.Text = "Transparency";
            // 
            // alphaNumericUpDown
            // 
            alphaNumericUpDown.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            alphaNumericUpDown.Location = new Point(758, 3);
            alphaNumericUpDown.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            alphaNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            alphaNumericUpDown.Name = "alphaNumericUpDown";
            alphaNumericUpDown.Size = new Size(65, 31);
            alphaNumericUpDown.TabIndex = 9;
            alphaNumericUpDown.Value = new decimal(new int[] { 255, 0, 0, 0 });
            alphaNumericUpDown.ValueChanged += alphaNumericUpDown_ValueChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(829, 0);
            label4.Name = "label4";
            label4.Size = new Size(45, 25);
            label4.TabIndex = 10;
            label4.Text = "Hills";
            // 
            // hillsComboBox
            // 
            hillsComboBox.FormattingEnabled = true;
            hillsComboBox.Location = new Point(880, 3);
            hillsComboBox.Name = "hillsComboBox";
            hillsComboBox.Size = new Size(182, 33);
            hillsComboBox.TabIndex = 11;
            hillsComboBox.SelectedIndexChanged += hillsComboBox_SelectedIndexChanged;
            // 
            // hillOpComboBox
            // 
            hillOpComboBox.FormattingEnabled = true;
            hillOpComboBox.Items.AddRange(new object[] { "All", "=", "<", ">" });
            hillOpComboBox.Location = new Point(1068, 3);
            hillOpComboBox.Name = "hillOpComboBox";
            hillOpComboBox.Size = new Size(182, 33);
            hillOpComboBox.TabIndex = 12;
            hillOpComboBox.Text = "All";
            hillOpComboBox.SelectedIndexChanged += hillOpComboBox_SelectedIndexChanged;
            // 
            // hillNumericUpDown
            // 
            hillNumericUpDown.Location = new Point(1256, 3);
            hillNumericUpDown.Name = "hillNumericUpDown";
            hillNumericUpDown.Size = new Size(70, 31);
            hillNumericUpDown.TabIndex = 13;
            hillNumericUpDown.ValueChanged += hillNumericUpDown_ValueChanged;
            // 
            // labelHillsCheckBox
            // 
            labelHillsCheckBox.AutoSize = true;
            labelHillsCheckBox.Location = new Point(1332, 3);
            labelHillsCheckBox.Name = "labelHillsCheckBox";
            labelHillsCheckBox.Size = new Size(117, 29);
            labelHillsCheckBox.TabIndex = 14;
            labelHillsCheckBox.Text = "Label Hills";
            labelHillsCheckBox.UseVisualStyleBackColor = true;
            labelHillsCheckBox.CheckedChanged += labelHillsCheckBox_CheckedChanged;
            // 
            // countLabel
            // 
            countLabel.AutoSize = true;
            countLabel.Location = new Point(3, 39);
            countLabel.Name = "countLabel";
            countLabel.Size = new Size(67, 25);
            countLabel.TabIndex = 15;
            countLabel.Text = "(count)";
            // 
            // gmap
            // 
            gmap.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gmap.Bearing = 0F;
            gmap.CanDragMap = true;
            gmap.EmptyTileColor = Color.Navy;
            gmap.GrayScaleMode = false;
            gmap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            gmap.LevelsKeepInMemory = 5;
            gmap.Location = new Point(6, 75);
            gmap.MarkersEnabled = true;
            gmap.MaxZoom = 2;
            gmap.MinZoom = 2;
            gmap.MouseWheelZoomEnabled = true;
            gmap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            gmap.Name = "gmap";
            gmap.NegativeMode = false;
            gmap.PolygonsEnabled = true;
            gmap.RetryLoadTile = 0;
            gmap.RoutesEnabled = true;
            gmap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            gmap.SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);
            gmap.ShowTileGridLines = false;
            gmap.Size = new Size(1478, 811);
            gmap.TabIndex = 1;
            gmap.Zoom = 0D;
            // 
            // OverviewMap
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(gmap);
            Controls.Add(flowLayoutPanel1);
            Name = "OverviewMap";
            Size = new Size(1487, 886);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)widthNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)alphaNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)hillNumericUpDown).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private ComboBox showComboBox;
        private Label label2;
        private NumericUpDown widthNumericUpDown;
        private Label label3;
        private NumericUpDown alphaNumericUpDown;
        private Label label4;
        private ComboBox hillsComboBox;
        private ComboBox hillOpComboBox;
        private NumericUpDown hillNumericUpDown;
        private GMap.NET.WindowsForms.GMapControl gmap;
        private CheckBox labelHillsCheckBox;
        private Label countLabel;
        private Label label5;
        private ComboBox sportComboBox;
    }
}
