namespace MathTrainer.Forms;

partial class WebAppForm
{
    private System.ComponentModel.IContainer components = null;
    private Microsoft.Web.WebView2.WinForms.WebView2 webView;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        ((System.ComponentModel.ISupportInitialize)this.webView).BeginInit();
        this.SuspendLayout();

        // webView
        this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
        this.webView.Location = new System.Drawing.Point(0, 0);
        this.webView.Name = "webView";
        this.webView.Size = new System.Drawing.Size(1200, 780);
        this.webView.TabIndex = 0;

        // WebAppForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(17, 20, 47);
        this.ClientSize = new System.Drawing.Size(1200, 780);
        this.Controls.Add(this.webView);
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "WebAppForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Вершина — тренажёр по математике";

        ((System.ComponentModel.ISupportInitialize)this.webView).EndInit();
        this.ResumeLayout(false);
    }
}
