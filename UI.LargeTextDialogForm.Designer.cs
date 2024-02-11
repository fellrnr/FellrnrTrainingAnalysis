namespace FellrnrTrainingAnalysis.UI
{
    partial class LargeTextDialogForm
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
            textBox1 = new TextBox();
            okayButton = new Button();
            cancelButton = new Button();
            copyButton = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(2, 1);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(1637, 1068);
            textBox1.TabIndex = 0;
            // 
            // okayButton
            // 
            okayButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            okayButton.Location = new Point(19, 1093);
            okayButton.Name = "okayButton";
            okayButton.Size = new Size(112, 34);
            okayButton.TabIndex = 1;
            okayButton.Text = "Ok";
            okayButton.UseVisualStyleBackColor = true;
            okayButton.Click += OkayButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cancelButton.Location = new Point(159, 1093);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(112, 34);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // copyButton
            // 
            copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            copyButton.Location = new Point(298, 1093);
            copyButton.Name = "copyButton";
            copyButton.Size = new Size(112, 34);
            copyButton.TabIndex = 3;
            copyButton.Text = "Copy";
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += CopyButton_Click;
            // 
            // LargeTextDialogForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(1640, 1148);
            Controls.Add(copyButton);
            Controls.Add(cancelButton);
            Controls.Add(okayButton);
            Controls.Add(textBox1);
            Name = "LargeTextDialogForm";
            Text = "LargeTextDialogForm1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button okayButton;
        private Button cancelButton;
        private Button copyButton;
    }
}