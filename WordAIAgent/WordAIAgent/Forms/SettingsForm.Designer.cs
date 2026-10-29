namespace WordAIAgent.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.cmbModel = new System.Windows.Forms.ComboBox();
            this.btnRefreshModels = new System.Windows.Forms.Button();
            this.btnTestConn = new System.Windows.Forms.Button();
            this.lblConnStatus = new System.Windows.Forms.Label();
            this.grpParams = new System.Windows.Forms.GroupBox();
            this.lblTemp = new System.Windows.Forms.Label();
            this.trkTemp = new System.Windows.Forms.TrackBar();
            this.lblTempVal = new System.Windows.Forms.Label();
            this.lblTokens = new System.Windows.Forms.Label();
            this.numTokens = new System.Windows.Forms.NumericUpDown();
            this.grpPrompt = new System.Windows.Forms.GroupBox();
            this.txtSystemPrompt = new System.Windows.Forms.RichTextBox();
            this.btnResetPrompt = new System.Windows.Forms.Button();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            this.grpConnection.SuspendLayout();
            this.grpParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkTemp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTokens)).BeginInit();
            this.grpPrompt.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // Title
            this.lblTitle.Text = "⚙ Настройки Word AI Agent";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(120, 180, 255);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(15, 12);

            // grpConnection
            this.grpConnection.Text = "🌐 Подключение к Ollama";
            this.grpConnection.ForeColor = System.Drawing.Color.LightBlue;
            this.grpConnection.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.grpConnection.BackColor = System.Drawing.Color.FromArgb(28, 28, 42);
            this.grpConnection.Location = new System.Drawing.Point(15, 50);
            this.grpConnection.Size = new System.Drawing.Size(540, 130);

            this.lblHost.Text = "Хост Ollama:";
            this.lblHost.ForeColor = System.Drawing.Color.Silver;
            this.lblHost.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblHost.Location = new System.Drawing.Point(10, 28);
            this.lblHost.Size = new System.Drawing.Size(100, 20);

            this.txtHost.Location = new System.Drawing.Point(120, 25);
            this.txtHost.Size = new System.Drawing.Size(280, 23);
            this.txtHost.BackColor = System.Drawing.Color.FromArgb(38, 38, 55);
            this.txtHost.ForeColor = System.Drawing.Color.White;
            this.txtHost.Font = new System.Drawing.Font("Consolas", 9f);
            this.txtHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.lblModel.Text = "Модель:";
            this.lblModel.ForeColor = System.Drawing.Color.Silver;
            this.lblModel.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblModel.Location = new System.Drawing.Point(10, 60);
            this.lblModel.Size = new System.Drawing.Size(100, 20);

            this.cmbModel.Location = new System.Drawing.Point(120, 57);
            this.cmbModel.Size = new System.Drawing.Size(220, 23);
            this.cmbModel.BackColor = System.Drawing.Color.FromArgb(38, 38, 55);
            this.cmbModel.ForeColor = System.Drawing.Color.White;
            this.cmbModel.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.cmbModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnRefreshModels.Text = "🔄";
            this.btnRefreshModels.Location = new System.Drawing.Point(348, 56);
            this.btnRefreshModels.Size = new System.Drawing.Size(36, 26);
            this.btnRefreshModels.BackColor = System.Drawing.Color.FromArgb(40, 60, 100);
            this.btnRefreshModels.ForeColor = System.Drawing.Color.White;
            this.btnRefreshModels.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshModels.Click += BtnRefreshModels_Click;

            this.btnTestConn.Text = "Проверить связь";
            this.btnTestConn.Location = new System.Drawing.Point(10, 92);
            this.btnTestConn.Size = new System.Drawing.Size(150, 28);
            this.btnTestConn.BackColor = System.Drawing.Color.FromArgb(40, 80, 40);
            this.btnTestConn.ForeColor = System.Drawing.Color.White;
            this.btnTestConn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestConn.Click += BtnTestConn_Click;

            this.lblConnStatus.Text = "Нажмите 'Проверить связь'";
            this.lblConnStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblConnStatus.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblConnStatus.Location = new System.Drawing.Point(170, 98);
            this.lblConnStatus.Size = new System.Drawing.Size(350, 20);

            this.grpConnection.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblHost, txtHost, lblModel, cmbModel, btnRefreshModels, btnTestConn, lblConnStatus
            });

            // grpParams
            this.grpParams.Text = "⚙ Параметры генерации";
            this.grpParams.ForeColor = System.Drawing.Color.LightBlue;
            this.grpParams.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.grpParams.BackColor = System.Drawing.Color.FromArgb(28, 28, 42);
            this.grpParams.Location = new System.Drawing.Point(15, 195);
            this.grpParams.Size = new System.Drawing.Size(540, 100);

            this.lblTemp.Text = "Температура:";
            this.lblTemp.ForeColor = System.Drawing.Color.Silver;
            this.lblTemp.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblTemp.Location = new System.Drawing.Point(10, 28);
            this.lblTemp.Size = new System.Drawing.Size(100, 20);

            this.trkTemp.Location = new System.Drawing.Point(120, 25);
            this.trkTemp.Size = new System.Drawing.Size(300, 30);
            this.trkTemp.Minimum = 0;
            this.trkTemp.Maximum = 20;
            this.trkTemp.Value = 7;
            this.trkTemp.TickFrequency = 2;
            this.trkTemp.BackColor = System.Drawing.Color.FromArgb(28, 28, 42);
            this.trkTemp.Scroll += TrkTemp_Scroll;

            this.lblTempVal.Text = "0.7";
            this.lblTempVal.ForeColor = System.Drawing.Color.LightYellow;
            this.lblTempVal.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);
            this.lblTempVal.Location = new System.Drawing.Point(430, 28);
            this.lblTempVal.Size = new System.Drawing.Size(50, 20);

            this.lblTokens.Text = "Макс. токенов:";
            this.lblTokens.ForeColor = System.Drawing.Color.Silver;
            this.lblTokens.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblTokens.Location = new System.Drawing.Point(10, 62);
            this.lblTokens.Size = new System.Drawing.Size(110, 20);

            this.numTokens.Location = new System.Drawing.Point(125, 60);
            this.numTokens.Size = new System.Drawing.Size(120, 23);
            this.numTokens.Minimum = 100;
            this.numTokens.Maximum = 8192;
            this.numTokens.Value = 2048;
            this.numTokens.Increment = 256;
            this.numTokens.BackColor = System.Drawing.Color.FromArgb(38, 38, 55);
            this.numTokens.ForeColor = System.Drawing.Color.White;

            this.grpParams.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTemp, trkTemp, lblTempVal, lblTokens, numTokens
            });

            // grpPrompt
            this.grpPrompt.Text = "💬 Системный промпт";
            this.grpPrompt.ForeColor = System.Drawing.Color.LightBlue;
            this.grpPrompt.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.grpPrompt.BackColor = System.Drawing.Color.FromArgb(28, 28, 42);
            this.grpPrompt.Location = new System.Drawing.Point(15, 308);
            this.grpPrompt.Size = new System.Drawing.Size(540, 200);

            this.txtSystemPrompt.Location = new System.Drawing.Point(10, 22);
            this.txtSystemPrompt.Size = new System.Drawing.Size(520, 140);
            this.txtSystemPrompt.BackColor = System.Drawing.Color.FromArgb(35, 35, 52);
            this.txtSystemPrompt.ForeColor = System.Drawing.Color.FromArgb(200, 220, 200);
            this.txtSystemPrompt.Font = new System.Drawing.Font("Consolas", 8.5f);
            this.txtSystemPrompt.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;

            this.btnResetPrompt.Text = "Сбросить по умолчанию";
            this.btnResetPrompt.Location = new System.Drawing.Point(10, 168);
            this.btnResetPrompt.Size = new System.Drawing.Size(200, 26);
            this.btnResetPrompt.BackColor = System.Drawing.Color.FromArgb(60, 40, 40);
            this.btnResetPrompt.ForeColor = System.Drawing.Color.White;
            this.btnResetPrompt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetPrompt.Click += BtnResetPrompt_Click;

            this.grpPrompt.Controls.AddRange(new System.Windows.Forms.Control[] { txtSystemPrompt, btnResetPrompt });

            // panelButtons
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Height = 55;
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(20, 20, 32);
            this.panelButtons.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);

            this.btnSave.Text = "💾 Сохранить";
            this.btnSave.Location = new System.Drawing.Point(340, 12);
            this.btnSave.Size = new System.Drawing.Size(120, 32);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(40, 100, 60);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Click += BtnSave_Click;

            this.btnCancel.Text = "Отмена";
            this.btnCancel.Location = new System.Drawing.Point(470, 12);
            this.btnCancel.Size = new System.Drawing.Size(90, 32);
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(60, 40, 40);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.panelButtons.Controls.AddRange(new System.Windows.Forms.Control[] { btnSave, btnCancel });

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(22, 22, 35);
            this.ClientSize = new System.Drawing.Size(575, 575);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTitle, grpConnection, grpParams, grpPrompt, panelButtons
            });
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Настройки AI Agent";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            this.grpConnection.ResumeLayout(false);
            this.grpParams.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trkTemp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTokens)).EndInit();
            this.grpPrompt.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpConnection, grpParams, grpPrompt;
        private System.Windows.Forms.Label lblHost, lblModel, lblTemp, lblTempVal, lblTokens, lblConnStatus;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.ComboBox cmbModel;
        private System.Windows.Forms.Button btnRefreshModels, btnTestConn, btnResetPrompt, btnSave, btnCancel;
        private System.Windows.Forms.TrackBar trkTemp;
        private System.Windows.Forms.NumericUpDown numTokens;
        private System.Windows.Forms.RichTextBox txtSystemPrompt;
        private System.Windows.Forms.Panel panelButtons;
    }
}
