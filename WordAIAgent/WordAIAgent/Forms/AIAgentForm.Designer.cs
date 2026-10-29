namespace WordAIAgent.Forms
{
    partial class AIAgentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelHeaderTop = new System.Windows.Forms.Panel();
            this.lblChatTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.chkTemplateMode = new System.Windows.Forms.CheckBox();
            this.panelHeaderBottom = new System.Windows.Forms.Panel();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblModel = new System.Windows.Forms.Label();
            this.cmbModels = new System.Windows.Forms.ComboBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.richBoxChat = new System.Windows.Forms.RichTextBox();
            this.panelTemplates = new System.Windows.Forms.Panel();
            this.lblTplTitle = new System.Windows.Forms.Label();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.panelTplButtons = new System.Windows.Forms.Panel();
            this.btnAddTemplate = new System.Windows.Forms.Button();
            this.btnApplyTemplate = new System.Windows.Forms.Button();
            this.btnRemoveTemplate = new System.Windows.Forms.Button();
            this.panelInput = new System.Windows.Forms.Panel();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();

            this.panelHeader.SuspendLayout();
            this.panelHeaderTop.SuspendLayout();
            this.panelHeaderBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.splitMain).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.panelTemplates.SuspendLayout();
            this.panelTplButtons.SuspendLayout();
            this.panelInput.SuspendLayout();
            this.SuspendLayout();

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(10, 10, 20);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 68;
            this.panelHeader.Controls.Add(this.panelHeaderBottom);
            this.panelHeader.Controls.Add(this.panelHeaderTop);

            // Row 1: title + status
            this.panelHeaderTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeaderTop.Height = 32;
            this.panelHeaderTop.BackColor = System.Drawing.Color.Transparent;
            this.panelHeaderTop.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.panelHeaderTop.Controls.Add(this.lblStatus);
            this.panelHeaderTop.Controls.Add(this.lblChatTitle);
            this.panelHeaderTop.Controls.Add(this.chkTemplateMode);

            this.lblChatTitle.AutoSize = true;
            this.lblChatTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblChatTitle.Font = new System.Drawing.Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
            this.lblChatTitle.ForeColor = System.Drawing.Color.FromArgb(120, 200, 255);
            this.lblChatTitle.Text = "⚡ Word AI Agent";
            this.lblChatTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblChatTitle.Padding = new System.Windows.Forms.Padding(0, 0, 14, 0);

            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lblStatus.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblStatus.Text = "● Нет связи";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.chkTemplateMode.AutoSize = true;
            this.chkTemplateMode.Dock = System.Windows.Forms.DockStyle.Right;
            this.chkTemplateMode.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
            this.chkTemplateMode.ForeColor = System.Drawing.Color.FromArgb(170, 170, 210);
            this.chkTemplateMode.Text = "Шаблон";
            this.chkTemplateMode.Padding = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.chkTemplateMode.CheckedChanged += new System.EventHandler(this.ChkTemplateMode_CheckedChanged);

            // Row 2: host + model
            this.panelHeaderBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeaderBottom.BackColor = System.Drawing.Color.Transparent;
            this.panelHeaderBottom.Padding = new System.Windows.Forms.Padding(8, 2, 8, 4);
            this.panelHeaderBottom.Controls.Add(this.cmbModels);
            this.panelHeaderBottom.Controls.Add(this.lblModel);
            this.panelHeaderBottom.Controls.Add(this.btnConnect);
            this.panelHeaderBottom.Controls.Add(this.txtHost);
            this.panelHeaderBottom.Controls.Add(this.lblHost);

            this.lblHost.AutoSize = true;
            this.lblHost.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblHost.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lblHost.ForeColor = System.Drawing.Color.FromArgb(140, 140, 175);
            this.lblHost.Text = "Host:";
            this.lblHost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHost.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);

            this.txtHost.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtHost.Width = 195;
            this.txtHost.BackColor = System.Drawing.Color.FromArgb(28, 28, 46);
            this.txtHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHost.ForeColor = System.Drawing.Color.White;
            this.txtHost.Font = new System.Drawing.Font("Segoe UI", 8.5f);

            this.btnConnect.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnConnect.Width = 56;
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(38, 38, 62);
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 60, 95);
            this.btnConnect.ForeColor = System.Drawing.Color.FromArgb(175, 175, 210);
            this.btnConnect.Text = "Apply";
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 7.5f);
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);

            this.lblModel.AutoSize = true;
            this.lblModel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblModel.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lblModel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 175);
            this.lblModel.Text = "  Модель:";
            this.lblModel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbModels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbModels.BackColor = System.Drawing.Color.FromArgb(28, 28, 46);
            this.cmbModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModels.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbModels.ForeColor = System.Drawing.Color.White;
            this.cmbModels.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.cmbModels.SelectedIndexChanged += new System.EventHandler(this.CmbModels_SelectedIndexChanged);

            // progressBar
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressBar.Height = 3;
            this.progressBar.MarqueeAnimationSpeed = 20;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.Visible = false;

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(18, 18, 30);

            // Panel1: chat
            this.richBoxChat.BackColor = System.Drawing.Color.FromArgb(18, 18, 30);
            this.richBoxChat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richBoxChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richBoxChat.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            this.richBoxChat.ForeColor = System.Drawing.Color.FromArgb(215, 215, 235);
            this.richBoxChat.ReadOnly = true;
            this.richBoxChat.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.splitMain.Panel1.Controls.Add(this.richBoxChat);

            // Panel2: templates sidebar
            this.panelTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTemplates.BackColor = System.Drawing.Color.FromArgb(12, 12, 22);

            this.lblTplTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTplTitle.Height = 30;
            this.lblTplTitle.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblTplTitle.ForeColor = System.Drawing.Color.FromArgb(100, 165, 255);
            this.lblTplTitle.Text = "  📄 Шаблоны";
            this.lblTplTitle.BackColor = System.Drawing.Color.FromArgb(9, 9, 18);
            this.lblTplTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lstTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTemplates.BackColor = System.Drawing.Color.FromArgb(16, 16, 28);
            this.lstTemplates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTemplates.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lstTemplates.ForeColor = System.Drawing.Color.FromArgb(185, 195, 220);
            this.lstTemplates.ItemHeight = 22;

            // Template buttons (bottom of sidebar)
            this.panelTplButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTplButtons.Height = 90;
            this.panelTplButtons.BackColor = System.Drawing.Color.FromArgb(9, 9, 18);
            this.panelTplButtons.Padding = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.panelTplButtons.Controls.Add(this.btnRemoveTemplate);
            this.panelTplButtons.Controls.Add(this.btnApplyTemplate);
            this.panelTplButtons.Controls.Add(this.btnAddTemplate);

            this.btnAddTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAddTemplate.Height = 26;
            this.btnAddTemplate.BackColor = System.Drawing.Color.FromArgb(22, 58, 22);
            this.btnAddTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddTemplate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(38, 85, 38);
            this.btnAddTemplate.ForeColor = System.Drawing.Color.FromArgb(145, 220, 145);
            this.btnAddTemplate.Text = "➕ Добавить";
            this.btnAddTemplate.Font = new System.Drawing.Font("Segoe UI", 8f);

            this.btnApplyTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnApplyTemplate.Height = 26;
            this.btnApplyTemplate.BackColor = System.Drawing.Color.FromArgb(20, 45, 100);
            this.btnApplyTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyTemplate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(35, 68, 145);
            this.btnApplyTemplate.ForeColor = System.Drawing.Color.FromArgb(150, 185, 255);
            this.btnApplyTemplate.Text = "▶ Применить";
            this.btnApplyTemplate.Font = new System.Drawing.Font("Segoe UI", 8f);

            this.btnRemoveTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRemoveTemplate.Height = 26;
            this.btnRemoveTemplate.BackColor = System.Drawing.Color.FromArgb(65, 18, 18);
            this.btnRemoveTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveTemplate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(95, 30, 30);
            this.btnRemoveTemplate.ForeColor = System.Drawing.Color.FromArgb(255, 150, 150);
            this.btnRemoveTemplate.Text = "🗑 Удалить";
            this.btnRemoveTemplate.Font = new System.Drawing.Font("Segoe UI", 8f);

            this.panelTemplates.Controls.Add(this.lstTemplates);
            this.panelTemplates.Controls.Add(this.panelTplButtons);
            this.panelTemplates.Controls.Add(this.lblTplTitle);
            this.splitMain.Panel2.Controls.Add(this.panelTemplates);

            // panelInput
            this.panelInput.BackColor = System.Drawing.Color.FromArgb(9, 9, 18);
            this.panelInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInput.Height = 76;
            this.panelInput.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.panelInput.Controls.Add(this.txtInput);
            this.panelInput.Controls.Add(this.btnSend);

            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.Width = 108;
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(35, 85, 190);
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 115, 240);
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Text = "➤ Отправить\n(Enter)";
            this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);

            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.BackColor = System.Drawing.Color.FromArgb(22, 22, 40);
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInput.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtInput.ForeColor = System.Drawing.Color.FromArgb(215, 215, 240);
            this.txtInput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtInput_KeyDown);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(18, 18, 30);
            this.ClientSize = new System.Drawing.Size(900, 700);
            this.MinimumSize = new System.Drawing.Size(480, 400);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.panelInput);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.panelHeader);
            this.Name = "AIAgentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Word AI Agent";
            this.Shown += new System.EventHandler(this.AIAgentForm_Shown);

            this.panelHeader.ResumeLayout(false);
            this.panelHeaderTop.ResumeLayout(false);
            this.panelHeaderTop.PerformLayout();
            this.panelHeaderBottom.ResumeLayout(false);
            this.panelHeaderBottom.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.splitMain).EndInit();
            this.splitMain.ResumeLayout(false);
            this.panelTemplates.ResumeLayout(false);
            this.panelTplButtons.ResumeLayout(false);
            this.panelInput.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelHeaderTop;
        private System.Windows.Forms.Panel panelHeaderBottom;
        private System.Windows.Forms.Label lblChatTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkTemplateMode;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cmbModels;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.RichTextBox richBoxChat;
        private System.Windows.Forms.Panel panelTemplates;
        private System.Windows.Forms.Label lblTplTitle;
        private System.Windows.Forms.ListBox lstTemplates;
        private System.Windows.Forms.Panel panelTplButtons;
        private System.Windows.Forms.Button btnAddTemplate;
        private System.Windows.Forms.Button btnRemoveTemplate;
        private System.Windows.Forms.Button btnApplyTemplate;
        private System.Windows.Forms.Panel panelInput;
        private System.Windows.Forms.RichTextBox txtInput;
        private System.Windows.Forms.Button btnSend;
    }
}
