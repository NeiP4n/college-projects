namespace JekaJeka10
{
    partial class Form3
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            label1 = new Label();
            label2 = new Label();
            button6 = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.HideSelection = false;
            textBox1.Location = new Point(315, 27);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(142, 35);
            textBox1.TabIndex = 0;
            textBox1.Text = "       Введите число:";
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(428, 99);
            button1.Name = "button1";
            button1.Size = new Size(118, 70);
            button1.TabIndex = 1;
            button1.Text = "Йены ¥";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(315, 230);
            button2.Name = "button2";
            button2.Size = new Size(118, 70);
            button2.TabIndex = 2;
            button2.Text = "Тенге 〒";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(230, 99);
            button3.Name = "button3";
            button3.Size = new Size(118, 70);
            button3.TabIndex = 3;
            button3.Text = "Юань ¥";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(131, 230);
            button4.Name = "button4";
            button4.Size = new Size(118, 70);
            button4.TabIndex = 4;
            button4.Text = "Доллар $";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(525, 230);
            button5.Name = "button5";
            button5.Size = new Size(118, 70);
            button5.TabIndex = 5;
            button5.Text = "Евро €";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(131, 334);
            label1.Name = "label1";
            label1.Size = new Size(512, 15);
            label1.TabIndex = 6;
            label1.Text = "_____________________________________________________________________________________________________";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(348, 379);
            label2.Name = "label2";
            label2.Size = new Size(60, 15);
            label2.TabIndex = 7;
            label2.Text = "Результат";
            label2.Click += label2_Click;
            // 
            // button6
            // 
            button6.Location = new Point(684, 27);
            button6.Name = "button6";
            button6.Size = new Size(94, 35);
            button6.TabIndex = 8;
            button6.Text = "Вариант 4";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button6);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Name = "Form3";
            Text = "Form3";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Label label1;
        private Label label2;
        private Button button6;
    }
}