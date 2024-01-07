namespace FellrnrTrainingAnalysis.UI
{
    partial class ActivityMap
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
            label2 = new Label();
            widthNumericUpDown = new NumericUpDown();
            label3 = new Label();
            alphaNumericUpDown = new NumericUpDown();
            gmap = new GMap.NET.WindowsForms.GMapControl();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)widthNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)alphaNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(showComboBox);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(widthNumericUpDown);
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Controls.Add(alphaNumericUpDown);
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1094, 52);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(56, 25);
            label1.TabIndex = 0;
            label1.Text = "Show";
            // 
            // showComboBox
            // 
            showComboBox.FormattingEnabled = true;
            showComboBox.Location = new Point(65, 3);
            showComboBox.Name = "showComboBox";
            showComboBox.Size = new Size(182, 33);
            showComboBox.TabIndex = 1;
            showComboBox.Text = "None";
            showComboBox.SelectedIndexChanged += showComboBox_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(253, 0);
            label2.Name = "label2";
            label2.Size = new Size(60, 25);
            label2.TabIndex = 2;
            label2.Text = "Width";
            // 
            // widthNumericUpDown
            // 
            widthNumericUpDown.Location = new Point(319, 3);
            widthNumericUpDown.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            widthNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            widthNumericUpDown.Name = "widthNumericUpDown";
            widthNumericUpDown.Size = new Size(63, 31);
            widthNumericUpDown.TabIndex = 3;
            widthNumericUpDown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            widthNumericUpDown.ValueChanged += widthNumericUpDown_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(388, 0);
            label3.Name = "label3";
            label3.Size = new Size(114, 25);
            label3.TabIndex = 4;
            label3.Text = "Transparency";
            // 
            // alphaNumericUpDown
            // 
            alphaNumericUpDown.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            alphaNumericUpDown.Location = new Point(508, 3);
            alphaNumericUpDown.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            alphaNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            alphaNumericUpDown.Name = "alphaNumericUpDown";
            alphaNumericUpDown.Size = new Size(65, 31);
            alphaNumericUpDown.TabIndex = 5;
            alphaNumericUpDown.Value = new decimal(new int[] { 255, 0, 0, 0 });
            alphaNumericUpDown.ValueChanged += numericUpDown1_ValueChanged;
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
            gmap.Location = new Point(6, 61);
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
            gmap.Size = new Size(1091, 668);
            gmap.TabIndex = 1;
            gmap.Zoom = 0D;
            // 
            // ActivityMap
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            Controls.Add(gmap);
            Controls.Add(flowLayoutPanel1);
            Name = "ActivityMap";
            Size = new Size(1100, 732);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)widthNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)alphaNumericUpDown).EndInit();
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
        private GMap.NET.WindowsForms.GMapControl gmap;
    }
}
