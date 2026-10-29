namespace JekaJeka0
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            Datatuda = new DataGridViewTextBoxColumn();
            dataGridView2 = new DataGridView();
            Data2 = new DataGridViewTextBoxColumn();
            dataGridView3 = new DataGridView();
            Data3 = new DataGridViewTextBoxColumn();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView3).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Datatuda });
            dataGridView1.Location = new Point(12, 12);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.Size = new Size(355, 265);
            dataGridView1.TabIndex = 0;
            // 
            // Datatuda
            // 
            Datatuda.HeaderText = "0";
            Datatuda.Name = "Datatuda";
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { Data2 });
            dataGridView2.Location = new Point(12, 387);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowTemplate.Height = 25;
            dataGridView2.Size = new Size(355, 265);
            dataGridView2.TabIndex = 1;
            // 
            // Data2
            // 
            Data2.HeaderText = "0";
            Data2.Name = "Data2";
            // 
            // dataGridView3
            // 
            dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView3.Columns.AddRange(new DataGridViewColumn[] { Data3 });
            dataGridView3.Location = new Point(483, 12);
            dataGridView3.Name = "dataGridView3";
            dataGridView3.RowTemplate.Height = 25;
            dataGridView3.Size = new Size(355, 265);
            dataGridView3.TabIndex = 2;
            // 
            // Data3
            // 
            Data3.HeaderText = "0";
            Data3.Name = "Data3";
            // 
            // button1
            // 
            button1.Location = new Point(483, 387);
            button1.Name = "button1";
            button1.Size = new Size(142, 59);
            button1.TabIndex = 3;
            button1.Text = "Сложить матрицы";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(696, 387);
            button2.Name = "button2";
            button2.Size = new Size(142, 59);
            button2.TabIndex = 4;
            button2.Text = "Вычесть матрицы";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(483, 477);
            button3.Name = "button3";
            button3.Size = new Size(142, 59);
            button3.TabIndex = 5;
            button3.Text = "Умножение матрицы";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(696, 477);
            button4.Name = "button4";
            button4.Size = new Size(142, 59);
            button4.TabIndex = 6;
            button4.Text = "Транспонирование матрицы";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(12, 303);
            button5.Name = "button5";
            button5.Size = new Size(144, 42);
            button5.TabIndex = 7;
            button5.Text = "Добавить столбец";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(483, 303);
            button6.Name = "button6";
            button6.Size = new Size(142, 42);
            button6.TabIndex = 8;
            button6.Text = "Добавить столбец";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.Location = new Point(696, 303);
            button7.Name = "button7";
            button7.Size = new Size(142, 42);
            button7.TabIndex = 9;
            button7.Text = "Удалить столбец";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // button8
            // 
            button8.Location = new Point(230, 303);
            button8.Name = "button8";
            button8.Size = new Size(137, 42);
            button8.TabIndex = 10;
            button8.Text = "Удалить столбец";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(873, 664);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(dataGridView3);
            Controls.Add(dataGridView2);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        private DataGridView dataGridView3;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private DataGridViewTextBoxColumn Data2;
        private DataGridViewTextBoxColumn Data3;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private DataGridViewTextBoxColumn Datatuda;
    }
}