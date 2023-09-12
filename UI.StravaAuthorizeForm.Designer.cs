namespace FellrnrTrainingAnalysis.UI
{
    partial class StravaAuthorizeForm
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
            this.stravaWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.stravaWebView)).BeginInit();
            this.SuspendLayout();
            // 
            // stravaWebView
            // 
            this.stravaWebView.AllowExternalDrop = true;
            this.stravaWebView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stravaWebView.CreationProperties = null;
            this.stravaWebView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.stravaWebView.Location = new System.Drawing.Point(12, 12);
            this.stravaWebView.Name = "stravaWebView";
            this.stravaWebView.Size = new System.Drawing.Size(1287, 1022);
            this.stravaWebView.TabIndex = 0;
            this.stravaWebView.ZoomFactor = 1D;
            this.stravaWebView.NavigationStarting += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs>(this.stravaWebView_NavigationStarting);
            // 
            // StravaAuthorizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1311, 1046);
            this.Controls.Add(this.stravaWebView);
            this.Name = "StravaAuthorizeForm";
            this.Text = "Strava Authorization";
            ((System.ComponentModel.ISupportInitialize)(this.stravaWebView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 stravaWebView;
    }
}