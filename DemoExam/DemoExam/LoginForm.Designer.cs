namespace DemoExam
{
    partial class LoginForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLogin = new MetroFramework.Controls.MetroButton();
            this.tbPassword = new MetroFramework.Controls.MetroTextBox();
            this.tbLogin = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.lblLogin = new MetroFramework.Controls.MetroLabel();
            this.lblPassword = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnLogin.Location = new System.Drawing.Point(101, 282);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(192, 42);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Вход";
            this.btnLogin.UseSelectable = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tbPassword
            // 
            this.tbPassword.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            // 
            // 
            // 
            this.tbPassword.CustomButton.Image = null;
            this.tbPassword.CustomButton.Location = new System.Drawing.Point(170, 1);
            this.tbPassword.CustomButton.Name = "";
            this.tbPassword.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbPassword.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbPassword.CustomButton.TabIndex = 1;
            this.tbPassword.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbPassword.CustomButton.UseSelectable = true;
            this.tbPassword.CustomButton.Visible = false;
            this.tbPassword.Lines = new string[0];
            this.tbPassword.Location = new System.Drawing.Point(101, 240);
            this.tbPassword.MaxLength = 32767;
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '\0';
            this.tbPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbPassword.SelectedText = "";
            this.tbPassword.SelectionLength = 0;
            this.tbPassword.SelectionStart = 0;
            this.tbPassword.ShortcutsEnabled = true;
            this.tbPassword.Size = new System.Drawing.Size(192, 23);
            this.tbPassword.TabIndex = 1;
            this.tbPassword.UseSelectable = true;
            this.tbPassword.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbPassword.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // tbLogin
            // 
            this.tbLogin.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            // 
            // 
            // 
            this.tbLogin.CustomButton.Image = null;
            this.tbLogin.CustomButton.Location = new System.Drawing.Point(170, 1);
            this.tbLogin.CustomButton.Name = "";
            this.tbLogin.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.tbLogin.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbLogin.CustomButton.TabIndex = 1;
            this.tbLogin.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbLogin.CustomButton.UseSelectable = true;
            this.tbLogin.CustomButton.Visible = false;
            this.tbLogin.Lines = new string[0];
            this.tbLogin.Location = new System.Drawing.Point(101, 169);
            this.tbLogin.MaxLength = 32767;
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.PasswordChar = '\0';
            this.tbLogin.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbLogin.SelectedText = "";
            this.tbLogin.SelectionLength = 0;
            this.tbLogin.SelectionStart = 0;
            this.tbLogin.ShortcutsEnabled = true;
            this.tbLogin.Size = new System.Drawing.Size(192, 23);
            this.tbLogin.TabIndex = 2;
            this.tbLogin.UseSelectable = true;
            this.tbLogin.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbLogin.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel1
            // 
            this.metroLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.metroLabel1.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.metroLabel1.Location = new System.Drawing.Point(101, 60);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(192, 30);
            this.metroLabel1.TabIndex = 3;
            this.metroLabel1.Text = "ООО \"МебельОрг\"";
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLogin
            // 
            this.lblLogin.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblLogin.AutoSize = true;
            this.lblLogin.Location = new System.Drawing.Point(164, 137);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(48, 20);
            this.lblLogin.TabIndex = 4;
            this.lblLogin.Text = "Логин";
            this.lblLogin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLogin.Click += new System.EventHandler(this.lblLogin_Click);
            // 
            // lblPassword
            // 
            this.lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(155, 208);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(57, 20);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Пароль";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPassword.Click += new System.EventHandler(this.lblPassword_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 361);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblLogin);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.btnLogin);
            this.Name = "LoginForm";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroButton btnLogin;
        private MetroFramework.Controls.MetroTextBox tbPassword;
        private MetroFramework.Controls.MetroTextBox tbLogin;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel lblLogin;
        private MetroFramework.Controls.MetroLabel lblPassword;
    }
}

