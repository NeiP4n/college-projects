namespace BdManga
{
    partial class Login
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
            LoginButton = new CuoreUI.Controls.cuiButton();
            cuiBorder1 = new CuoreUI.Controls.cuiBorder();
            cuiLabel3 = new CuoreUI.Controls.cuiLabel();
            Passwordtextbox = new CuoreUI.Controls.cuiTextBox2();
            cuiLabel2 = new CuoreUI.Controls.cuiLabel();
            cuiLabel1 = new CuoreUI.Controls.cuiLabel();
            Logintextbox = new CuoreUI.Controls.cuiTextBox2();
            RegisterButton = new CuoreUI.Controls.cuiButton();
            cuiBorder1.SuspendLayout();
            SuspendLayout();
            // 
            // LoginButton
            // 
            LoginButton.Anchor = AnchorStyles.Bottom;
            LoginButton.BackColor = Color.FromArgb(64, 0, 0);
            LoginButton.CheckButton = false;
            LoginButton.Checked = false;
            LoginButton.CheckedBackground = Color.FromArgb(255, 106, 0);
            LoginButton.CheckedForeColor = Color.White;
            LoginButton.CheckedImageTint = Color.White;
            LoginButton.CheckedOutline = Color.FromArgb(255, 106, 0);
            LoginButton.Content = "Войти";
            LoginButton.DialogResult = DialogResult.None;
            LoginButton.Font = new Font("Microsoft Sans Serif", 16F);
            LoginButton.ForeColor = Color.White;
            LoginButton.HoverBackground = Color.FromArgb(200, 255, 106, 0);
            LoginButton.HoveredImageTint = Color.White;
            LoginButton.HoverForeColor = Color.White;
            LoginButton.HoverOutline = Color.Empty;
            LoginButton.Image = null;
            LoginButton.ImageAutoCenter = true;
            LoginButton.ImageExpand = new Point(0, 0);
            LoginButton.ImageOffset = new Point(0, 0);
            LoginButton.ImageTint = Color.White;
            LoginButton.Location = new Point(76, 359);
            LoginButton.Name = "LoginButton";
            LoginButton.NormalBackground = Color.FromArgb(192, 0, 0);
            LoginButton.NormalOutline = Color.Empty;
            LoginButton.OutlineThickness = 1.6F;
            LoginButton.PressedBackground = Color.FromArgb(255, 106, 0);
            LoginButton.PressedForeColor = Color.White;
            LoginButton.PressedImageTint = Color.White;
            LoginButton.PressedOutline = Color.Empty;
            LoginButton.Rounding = new Padding(32);
            LoginButton.Size = new Size(278, 61);
            LoginButton.TabIndex = 0;
            LoginButton.TextOffset = new Point(0, 0);
            LoginButton.Click += cuiButton1_Click;
            // 
            // cuiBorder1
            // 
            cuiBorder1.Anchor = AnchorStyles.None;
            cuiBorder1.Controls.Add(cuiLabel3);
            cuiBorder1.Controls.Add(Passwordtextbox);
            cuiBorder1.Controls.Add(cuiLabel2);
            cuiBorder1.Controls.Add(cuiLabel1);
            cuiBorder1.Controls.Add(Logintextbox);
            cuiBorder1.Controls.Add(RegisterButton);
            cuiBorder1.Controls.Add(LoginButton);
            cuiBorder1.Location = new Point(350, 41);
            cuiBorder1.Name = "cuiBorder1";
            cuiBorder1.OutlineThickness = 1F;
            cuiBorder1.PanelColor = Color.FromArgb(64, 0, 0);
            cuiBorder1.PanelOutlineColor = Color.FromArgb(64, 0, 0);
            cuiBorder1.Rounding = new Padding(64);
            cuiBorder1.Size = new Size(432, 583);
            cuiBorder1.TabIndex = 1;
            // 
            // cuiLabel3
            // 
            cuiLabel3.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel3.Content = "Пароль";
            cuiLabel3.Font = new Font("Segoe UI", 16F);
            cuiLabel3.ForeColor = Color.Maroon;
            cuiLabel3.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel3.Location = new Point(76, 218);
            cuiLabel3.Margin = new Padding(4, 5, 4, 5);
            cuiLabel3.Name = "cuiLabel3";
            cuiLabel3.Size = new Size(294, 35);
            cuiLabel3.TabIndex = 6;
            // 
            // Passwordtextbox
            // 
            Passwordtextbox.BackColor = Color.White;
            Passwordtextbox.BorderColor = Color.FromArgb(192, 0, 0);
            Passwordtextbox.BorderFocusColor = Color.FromArgb(255, 106, 0);
            Passwordtextbox.BorderSize = 5;
            Passwordtextbox.Content = "";
            Passwordtextbox.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Passwordtextbox.ForeColor = Color.Gray;
            Passwordtextbox.Location = new Point(52, 262);
            Passwordtextbox.Margin = new Padding(4);
            Passwordtextbox.Multiline = false;
            Passwordtextbox.Name = "Passwordtextbox";
            Passwordtextbox.Padding = new Padding(18, 19, 18, 0);
            Passwordtextbox.PasswordChar = false;
            Passwordtextbox.PlaceholderColor = Color.DimGray;
            Passwordtextbox.PlaceholderText = "";
            Passwordtextbox.Rounding = 32;
            Passwordtextbox.Size = new Size(332, 56);
            Passwordtextbox.TabIndex = 5;
            Passwordtextbox.TextOffset = new Size(0, 0);
            Passwordtextbox.UnderlinedStyle = false;
            // 
            // cuiLabel2
            // 
            cuiLabel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            cuiLabel2.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel2.Content = "Авторизация";
            cuiLabel2.Font = new Font("Segoe UI", 24F);
            cuiLabel2.ForeColor = Color.White;
            cuiLabel2.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel2.Location = new Point(76, 20);
            cuiLabel2.Margin = new Padding(4, 5, 4, 5);
            cuiLabel2.Name = "cuiLabel2";
            cuiLabel2.Size = new Size(294, 48);
            cuiLabel2.TabIndex = 4;
            // 
            // cuiLabel1
            // 
            cuiLabel1.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel1.Content = "Логин";
            cuiLabel1.Font = new Font("Segoe UI", 16F);
            cuiLabel1.ForeColor = Color.Maroon;
            cuiLabel1.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel1.Location = new Point(76, 102);
            cuiLabel1.Margin = new Padding(4, 5, 4, 5);
            cuiLabel1.Name = "cuiLabel1";
            cuiLabel1.Size = new Size(294, 35);
            cuiLabel1.TabIndex = 3;
            // 
            // Logintextbox
            // 
            Logintextbox.BackColor = Color.White;
            Logintextbox.BorderColor = Color.FromArgb(192, 0, 0);
            Logintextbox.BorderFocusColor = Color.FromArgb(255, 106, 0);
            Logintextbox.BorderSize = 5;
            Logintextbox.Content = "";
            Logintextbox.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Logintextbox.ForeColor = Color.Gray;
            Logintextbox.Location = new Point(52, 146);
            Logintextbox.Margin = new Padding(4);
            Logintextbox.Multiline = false;
            Logintextbox.Name = "Logintextbox";
            Logintextbox.Padding = new Padding(18, 19, 18, 0);
            Logintextbox.PasswordChar = false;
            Logintextbox.PlaceholderColor = Color.DimGray;
            Logintextbox.PlaceholderText = "";
            Logintextbox.Rounding = 32;
            Logintextbox.Size = new Size(332, 56);
            Logintextbox.TabIndex = 2;
            Logintextbox.TextOffset = new Size(0, 0);
            Logintextbox.UnderlinedStyle = false;
            // 
            // RegisterButton
            // 
            RegisterButton.Anchor = AnchorStyles.Bottom;
            RegisterButton.BackColor = Color.FromArgb(64, 0, 0);
            RegisterButton.CheckButton = false;
            RegisterButton.Checked = false;
            RegisterButton.CheckedBackground = Color.FromArgb(255, 106, 0);
            RegisterButton.CheckedForeColor = Color.White;
            RegisterButton.CheckedImageTint = Color.White;
            RegisterButton.CheckedOutline = Color.FromArgb(255, 106, 0);
            RegisterButton.Content = "Регистрация";
            RegisterButton.DialogResult = DialogResult.None;
            RegisterButton.Font = new Font("Microsoft Sans Serif", 16F);
            RegisterButton.ForeColor = Color.White;
            RegisterButton.HoverBackground = Color.FromArgb(200, 255, 106, 0);
            RegisterButton.HoveredImageTint = Color.White;
            RegisterButton.HoverForeColor = Color.White;
            RegisterButton.HoverOutline = Color.Empty;
            RegisterButton.Image = null;
            RegisterButton.ImageAutoCenter = true;
            RegisterButton.ImageExpand = new Point(0, 0);
            RegisterButton.ImageOffset = new Point(0, 0);
            RegisterButton.ImageTint = Color.White;
            RegisterButton.Location = new Point(76, 442);
            RegisterButton.Name = "RegisterButton";
            RegisterButton.NormalBackground = Color.FromArgb(192, 0, 0);
            RegisterButton.NormalOutline = Color.Empty;
            RegisterButton.OutlineThickness = 1.6F;
            RegisterButton.PressedBackground = Color.FromArgb(255, 106, 0);
            RegisterButton.PressedForeColor = Color.White;
            RegisterButton.PressedImageTint = Color.White;
            RegisterButton.PressedOutline = Color.Empty;
            RegisterButton.Rounding = new Padding(32);
            RegisterButton.Size = new Size(278, 61);
            RegisterButton.TabIndex = 1;
            RegisterButton.TextOffset = new Point(0, 0);
            RegisterButton.Click += cuiButton2_Click;
            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowText;
            ClientSize = new Size(1100, 662);
            Controls.Add(cuiBorder1);
            Name = "Login";
            Text = "Login";
            Load += Login_Load;
            cuiBorder1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private CuoreUI.Controls.cuiButton LoginButton;
        private CuoreUI.Controls.cuiBorder cuiBorder1;
        private CuoreUI.Controls.cuiButton RegisterButton;
        private CuoreUI.Controls.cuiTextBox2 Logintextbox;
        private CuoreUI.Controls.cuiLabel cuiLabel1;
        private CuoreUI.Controls.cuiLabel cuiLabel2;
        private CuoreUI.Controls.cuiLabel cuiLabel3;
        private CuoreUI.Controls.cuiTextBox2 Passwordtextbox;
    }
}