namespace LabaBond1
{
    partial class Form1
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SearchButton = new System.Windows.Forms.Button();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.SumButton = new System.Windows.Forms.Button();
            this.topReaderButton = new System.Windows.Forms.Button();
            this.SumBook = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(924, 628);
            this.dataGridView1.TabIndex = 0;
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchButton.Location = new System.Drawing.Point(624, 120);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(327, 48);
            this.SearchButton.TabIndex = 1;
            this.SearchButton.Text = "Поиск";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(624, 92);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(327, 22);
            this.textBoxSearch.TabIndex = 2;
            // 
            // SumButton
            // 
            this.SumButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SumButton.Location = new System.Drawing.Point(624, 174);
            this.SumButton.Name = "SumButton";
            this.SumButton.Size = new System.Drawing.Size(327, 54);
            this.SumButton.TabIndex = 3;
            this.SumButton.Text = "Сумма";
            this.SumButton.UseVisualStyleBackColor = true;
            this.SumButton.Click += new System.EventHandler(this.SumButton_Click);
            // 
            // topReaderButton
            // 
            this.topReaderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topReaderButton.Location = new System.Drawing.Point(624, 234);
            this.topReaderButton.Name = "topReaderButton";
            this.topReaderButton.Size = new System.Drawing.Size(327, 60);
            this.topReaderButton.TabIndex = 4;
            this.topReaderButton.Text = "Топ";
            this.topReaderButton.UseVisualStyleBackColor = true;
            this.topReaderButton.Click += new System.EventHandler(this.topReaderButton_Click);
            // 
            // SumBook
            // 
            this.SumBook.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SumBook.Location = new System.Drawing.Point(624, 300);
            this.SumBook.Name = "SumBook";
            this.SumBook.Size = new System.Drawing.Size(327, 57);
            this.SumBook.TabIndex = 5;
            this.SumBook.Text = "Сумма Книг";
            this.SumBook.UseVisualStyleBackColor = true;
            this.SumBook.Click += new System.EventHandler(this.SumBook_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(624, 387);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(327, 24);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 628);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.SumBook);
            this.Controls.Add(this.topReaderButton);
            this.Controls.Add(this.SumButton);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Button SumButton;
        private System.Windows.Forms.Button topReaderButton;
        private System.Windows.Forms.Button SumBook;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

