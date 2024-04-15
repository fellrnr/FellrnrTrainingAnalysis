namespace FellrnrTrainingAnalysis
{
    partial class ActivityCorrelation
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
            label1 = new Label();
            label2 = new Label();
            tsXcomboBox = new ComboBox();
            tsYcomboBox = new ComboBox();
            r2label = new Label();
            slopeLabel = new Label();
            interceptYLabel = new Label();
            comboBoxFilterX = new ComboBox();
            comboBoxFilterY = new ComboBox();
            buttonExecute = new Button();
            labelYDetails = new Label();
            labelXDetails = new Label();
            dataGridView1 = new DataGridView();
            label3 = new Label();
            numericUpDownMinR2 = new NumericUpDown();
            checkBoxIgnore0X = new CheckBox();
            checkBoxIgnore0Y = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinR2).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 22);
            label1.Name = "label1";
            label1.Size = new Size(113, 25);
            label1.TabIndex = 0;
            label1.Text = "Time Value X";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 69);
            label2.Name = "label2";
            label2.Size = new Size(112, 25);
            label2.TabIndex = 1;
            label2.Text = "Time Value Y";
            // 
            // tsXcomboBox
            // 
            tsXcomboBox.FormattingEnabled = true;
            tsXcomboBox.Location = new Point(187, 18);
            tsXcomboBox.Name = "tsXcomboBox";
            tsXcomboBox.Size = new Size(182, 33);
            tsXcomboBox.TabIndex = 2;
            // 
            // tsYcomboBox
            // 
            tsYcomboBox.FormattingEnabled = true;
            tsYcomboBox.Location = new Point(187, 65);
            tsYcomboBox.Name = "tsYcomboBox";
            tsYcomboBox.Size = new Size(182, 33);
            tsYcomboBox.TabIndex = 3;
            // 
            // r2label
            // 
            r2label.AutoSize = true;
            r2label.Location = new Point(801, 26);
            r2label.Name = "r2label";
            r2label.Size = new Size(33, 25);
            r2label.TabIndex = 4;
            r2label.Text = "R2";
            // 
            // slopeLabel
            // 
            slopeLabel.AutoSize = true;
            slopeLabel.Location = new Point(801, 77);
            slopeLabel.Name = "slopeLabel";
            slopeLabel.Size = new Size(57, 25);
            slopeLabel.TabIndex = 5;
            slopeLabel.Text = "Slope";
            // 
            // interceptYLabel
            // 
            interceptYLabel.AutoSize = true;
            interceptYLabel.Location = new Point(801, 122);
            interceptYLabel.Name = "interceptYLabel";
            interceptYLabel.Size = new Size(82, 25);
            interceptYLabel.TabIndex = 6;
            interceptYLabel.Text = "Intercept";
            // 
            // comboBoxFilterX
            // 
            comboBoxFilterX.FormattingEnabled = true;
            comboBoxFilterX.Items.AddRange(new object[] { "All", "Virtual", "Recorded" });
            comboBoxFilterX.Location = new Point(422, 18);
            comboBoxFilterX.Name = "comboBoxFilterX";
            comboBoxFilterX.Size = new Size(182, 33);
            comboBoxFilterX.TabIndex = 7;
            comboBoxFilterX.Text = "All";
            // 
            // comboBoxFilterY
            // 
            comboBoxFilterY.FormattingEnabled = true;
            comboBoxFilterY.Items.AddRange(new object[] { "All", "Virtual", "Recorded" });
            comboBoxFilterY.Location = new Point(422, 65);
            comboBoxFilterY.Name = "comboBoxFilterY";
            comboBoxFilterY.Size = new Size(182, 33);
            comboBoxFilterY.TabIndex = 8;
            comboBoxFilterY.Text = "All";
            // 
            // buttonExecute
            // 
            buttonExecute.Location = new Point(30, 122);
            buttonExecute.Name = "buttonExecute";
            buttonExecute.Size = new Size(112, 34);
            buttonExecute.TabIndex = 9;
            buttonExecute.Text = "Execute";
            buttonExecute.UseVisualStyleBackColor = true;
            buttonExecute.Click += buttonExecute_Click;
            // 
            // labelYDetails
            // 
            labelYDetails.AutoSize = true;
            labelYDetails.Location = new Point(37, 255);
            labelYDetails.Name = "labelYDetails";
            labelYDetails.Size = new Size(80, 25);
            labelYDetails.TabIndex = 10;
            labelYDetails.Text = "Y Details";
            // 
            // labelXDetails
            // 
            labelXDetails.AutoSize = true;
            labelXDetails.Location = new Point(36, 194);
            labelXDetails.Name = "labelXDetails";
            labelXDetails.Size = new Size(81, 25);
            labelXDetails.TabIndex = 11;
            labelXDetails.Text = "X Details";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 296);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.RowTemplate.Height = 33;
            dataGridView1.Size = new Size(1976, 784);
            dataGridView1.TabIndex = 12;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(301, 127);
            label3.Name = "label3";
            label3.Size = new Size(68, 25);
            label3.TabIndex = 13;
            label3.Text = "Min R2";
            // 
            // numericUpDownMinR2
            // 
            numericUpDownMinR2.DecimalPlaces = 2;
            numericUpDownMinR2.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownMinR2.Location = new Point(424, 127);
            numericUpDownMinR2.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownMinR2.Name = "numericUpDownMinR2";
            numericUpDownMinR2.Size = new Size(180, 31);
            numericUpDownMinR2.TabIndex = 14;
            // 
            // checkBoxIgnore0X
            // 
            checkBoxIgnore0X.AutoSize = true;
            checkBoxIgnore0X.Location = new Point(623, 23);
            checkBoxIgnore0X.Name = "checkBoxIgnore0X";
            checkBoxIgnore0X.Size = new Size(139, 29);
            checkBoxIgnore0X.TabIndex = 15;
            checkBoxIgnore0X.Text = "Ignore Zeros";
            checkBoxIgnore0X.UseVisualStyleBackColor = true;
            // 
            // checkBoxIgnore0Y
            // 
            checkBoxIgnore0Y.AutoSize = true;
            checkBoxIgnore0Y.Location = new Point(623, 67);
            checkBoxIgnore0Y.Name = "checkBoxIgnore0Y";
            checkBoxIgnore0Y.Size = new Size(139, 29);
            checkBoxIgnore0Y.TabIndex = 16;
            checkBoxIgnore0Y.Text = "Ignore Zeros";
            checkBoxIgnore0Y.UseVisualStyleBackColor = true;
            // 
            // ActivityCorrelation
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2000, 1092);
            Controls.Add(checkBoxIgnore0Y);
            Controls.Add(checkBoxIgnore0X);
            Controls.Add(numericUpDownMinR2);
            Controls.Add(label3);
            Controls.Add(dataGridView1);
            Controls.Add(labelXDetails);
            Controls.Add(labelYDetails);
            Controls.Add(buttonExecute);
            Controls.Add(comboBoxFilterY);
            Controls.Add(comboBoxFilterX);
            Controls.Add(interceptYLabel);
            Controls.Add(slopeLabel);
            Controls.Add(r2label);
            Controls.Add(tsYcomboBox);
            Controls.Add(tsXcomboBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "ActivityCorrelation";
            Text = "ActivityCorrelation";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinR2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox tsXcomboBox;
        private ComboBox tsYcomboBox;
        private Label r2label;
        private Label slopeLabel;
        private Label interceptYLabel;
        private ComboBox comboBoxFilterX;
        private ComboBox comboBoxFilterY;
        private Button buttonExecute;
        private Label labelYDetails;
        private Label labelXDetails;
        private DataGridView dataGridView1;
        private Label label3;
        private NumericUpDown numericUpDownMinR2;
        private CheckBox checkBoxIgnore0X;
        private CheckBox checkBoxIgnore0Y;
    }
}