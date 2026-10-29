namespace BdManga
{
    partial class Registration
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
            cuiLabel3 = new CuoreUI.Controls.cuiLabel();
            Logintextbox = new CuoreUI.Controls.cuiTextBox2();
            cuiLabel2 = new CuoreUI.Controls.cuiLabel();
            cuiLabel1 = new CuoreUI.Controls.cuiLabel();
            cuiBorder1 = new CuoreUI.Controls.cuiBorder();
            RegisterButton = new CuoreUI.Controls.cuiButton();
            cuiLabel4 = new CuoreUI.Controls.cuiLabel();
            Passwordtextbox = new CuoreUI.Controls.cuiTextBox2();
            Mailtextbox = new CuoreUI.Controls.cuiTextBox2();
            cuiBorder1.SuspendLayout();
            SuspendLayout();
            // 
            // cuiLabel3
            // 
            cuiLabel3.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel3.Content = "Логин";
            cuiLabel3.Font = new Font("Segoe UI", 16F);
            cuiLabel3.ForeColor = Color.Maroon;
            cuiLabel3.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel3.Location = new Point(76, 204);
            cuiLabel3.Margin = new Padding(4, 5, 4, 5);
            cuiLabel3.Name = "cuiLabel3";
            cuiLabel3.Size = new Size(294, 35);
            cuiLabel3.TabIndex = 6;
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
            Logintextbox.Location = new Point(52, 248);
            Logintextbox.Margin = new Padding(4);
            Logintextbox.Multiline = false;
            Logintextbox.Name = "Logintextbox";
            Logintextbox.Padding = new Padding(18, 19, 18, 0);
            Logintextbox.PasswordChar = false;
            Logintextbox.PlaceholderColor = Color.DimGray;
            Logintextbox.PlaceholderText = "";
            Logintextbox.Rounding = 32;
            Logintextbox.Size = new Size(332, 56);
            Logintextbox.TabIndex = 5;
            Logintextbox.TextOffset = new Size(0, 0);
            Logintextbox.UnderlinedStyle = false;
            // 
            // cuiLabel2
            // 
            cuiLabel2.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel2.Content = "Регистрация";
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
            cuiLabel1.Content = "Почта";
            cuiLabel1.Font = new Font("Segoe UI", 16F);
            cuiLabel1.ForeColor = Color.Maroon;
            cuiLabel1.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel1.Location = new Point(76, 91);
            cuiLabel1.Margin = new Padding(4, 5, 4, 5);
            cuiLabel1.Name = "cuiLabel1";
            cuiLabel1.Size = new Size(294, 35);
            cuiLabel1.TabIndex = 3;
            // 
            // cuiBorder1
            // 
            cuiBorder1.Anchor = AnchorStyles.None;
            cuiBorder1.Controls.Add(RegisterButton);
            cuiBorder1.Controls.Add(cuiLabel4);
            cuiBorder1.Controls.Add(Passwordtextbox);
            cuiBorder1.Controls.Add(cuiLabel3);
            cuiBorder1.Controls.Add(Logintextbox);
            cuiBorder1.Controls.Add(cuiLabel1);
            cuiBorder1.Controls.Add(Mailtextbox);
            cuiBorder1.Controls.Add(cuiLabel2);
            cuiBorder1.Location = new Point(338, 59);
            cuiBorder1.Name = "cuiBorder1";
            cuiBorder1.OutlineThickness = 1F;
            cuiBorder1.PanelColor = Color.FromArgb(64, 0, 0);
            cuiBorder1.PanelOutlineColor = Color.FromArgb(64, 0, 0);
            cuiBorder1.Rounding = new Padding(64);
            cuiBorder1.Size = new Size(432, 583);
            cuiBorder1.TabIndex = 2;
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
            RegisterButton.Content = "Зарегистрироваться";
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
            RegisterButton.Location = new Point(62, 436);
            RegisterButton.Name = "RegisterButton";
            RegisterButton.NormalBackground = Color.FromArgb(192, 0, 0);
            RegisterButton.NormalOutline = Color.Empty;
            RegisterButton.OutlineThickness = 1.6F;
            RegisterButton.PressedBackground = Color.FromArgb(255, 106, 0);
            RegisterButton.PressedForeColor = Color.White;
            RegisterButton.PressedImageTint = Color.White;
            RegisterButton.PressedOutline = Color.Empty;
            RegisterButton.Rounding = new Padding(32);
            RegisterButton.Size = new Size(322, 61);
            RegisterButton.TabIndex = 9;
            RegisterButton.TextOffset = new Point(0, 0);
            RegisterButton.Click += cuiButton3_Click;
            // 
            // cuiLabel4
            // 
            cuiLabel4.BackColor = Color.FromArgb(64, 0, 0);
            cuiLabel4.Content = "Пароль";
            cuiLabel4.Font = new Font("Segoe UI", 16F);
            cuiLabel4.ForeColor = Color.Maroon;
            cuiLabel4.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            cuiLabel4.Location = new Point(76, 320);
            cuiLabel4.Margin = new Padding(4, 5, 4, 5);
            cuiLabel4.Name = "cuiLabel4";
            cuiLabel4.Size = new Size(294, 35);
            cuiLabel4.TabIndex = 8;
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
            Passwordtextbox.Location = new Point(52, 364);
            Passwordtextbox.Margin = new Padding(4);
            Passwordtextbox.Multiline = false;
            Passwordtextbox.Name = "Passwordtextbox";
            Passwordtextbox.Padding = new Padding(18, 19, 18, 0);
            Passwordtextbox.PasswordChar = false;
            Passwordtextbox.PlaceholderColor = Color.DimGray;
            Passwordtextbox.PlaceholderText = "";
            Passwordtextbox.Rounding = 32;
            Passwordtextbox.Size = new Size(332, 56);
            Passwordtextbox.TabIndex = 7;
            Passwordtextbox.TextOffset = new Size(0, 0);
            Passwordtextbox.UnderlinedStyle = false;
            // 
            // Mailtextbox
            // 
            Mailtextbox.BackColor = Color.White;
            Mailtextbox.BorderColor = Color.FromArgb(192, 0, 0);
            Mailtextbox.BorderFocusColor = Color.FromArgb(255, 106, 0);
            Mailtextbox.BorderSize = 5;
            Mailtextbox.Content = "";
            Mailtextbox.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Mailtextbox.ForeColor = Color.Gray;
            Mailtextbox.Location = new Point(52, 135);
            Mailtextbox.Margin = new Padding(4);
            Mailtextbox.Multiline = false;
            Mailtextbox.Name = "Mailtextbox";
            Mailtextbox.Padding = new Padding(18, 19, 18, 0);
            Mailtextbox.PasswordChar = false;
            Mailtextbox.PlaceholderColor = Color.DimGray;
            Mailtextbox.PlaceholderText = "";
            Mailtextbox.Rounding = 32;
            Mailtextbox.Size = new Size(332, 56);
            Mailtextbox.TabIndex = 2;
            Mailtextbox.TextOffset = new Size(0, 0);
            Mailtextbox.UnderlinedStyle = false;
            // 
            // Registration
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1142, 686);
            Controls.Add(cuiBorder1);
            Name = "Registration";
            Text = "Registration";
            cuiBorder1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private CuoreUI.Controls.cuiLabel cuiLabel3;
        private CuoreUI.Controls.cuiTextBox2 Logintextbox;
        private CuoreUI.Controls.cuiLabel cuiLabel2;
        private CuoreUI.Controls.cuiLabel cuiLabel1;
        private CuoreUI.Controls.cuiBorder cuiBorder1;
        private CuoreUI.Controls.cuiTextBox2 Mailtextbox;
        private CuoreUI.Controls.cuiLabel cuiLabel4;
        private CuoreUI.Controls.cuiTextBox2 Passwordtextbox;
        private CuoreUI.Controls.cuiButton RegisterButton;
    }
}