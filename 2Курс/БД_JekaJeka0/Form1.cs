using System;
using System.Windows.Forms;

namespace JekaJeka0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }







        private void button1_Click(object sender, EventArgs e) // Сложение матриц
        {

            if (dataGridView1.RowCount != dataGridView3.RowCount || dataGridView1.ColumnCount != dataGridView3.ColumnCount)
            {
                MessageBox.Show("количество не совпадает");
                return;
            }
            dataGridView2.Rows.Clear();
            dataGridView2.RowCount = dataGridView1.RowCount;
            dataGridView2.ColumnCount = dataGridView1.ColumnCount;


            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {

                    int chislo1 = Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Value);
                    int chislo3 = Convert.ToInt32(dataGridView3.Rows[i].Cells[j].Value);
                    int sum = chislo1 + chislo3;


                    dataGridView2.Rows[i].Cells[j].Value = sum;
                }
            }
        }











        private void dataGridView1_CellContentClick(object sender, EventArgs e)
        {

        }
        private void button5_Click(object sender, EventArgs e) // Добавить столбец
        {

            dataGridView1.Columns.Add("Column1", "Колонка");

        }

        private void button8_Click(object sender, EventArgs e) // Удалить столбец
        {
            dataGridView1.Columns.Remove("Column1");
        }

        private void button6_Click(object sender, EventArgs e) // Добавить столбец
        {
            dataGridView3.Columns.Add("Column1", "Колонка");
        }

        private void button7_Click(object sender, EventArgs e) // Удалить столбец
        {
            dataGridView3.Columns.Remove("Column1");
        }








        private void button2_Click(object sender, EventArgs e) // Вычесть матрицы
        {

            if (dataGridView1.RowCount != dataGridView3.RowCount || dataGridView1.ColumnCount != dataGridView3.ColumnCount)
            {
                MessageBox.Show("количество не совпадает");
                return;
            }
            dataGridView2.Rows.Clear();
            dataGridView2.RowCount = dataGridView1.RowCount;
            dataGridView2.ColumnCount = dataGridView1.ColumnCount;


            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {

                    int chislo1 = Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Value);
                    int chislo3 = Convert.ToInt32(dataGridView3.Rows[i].Cells[j].Value);
                    int sum = chislo1 - chislo3;


                    dataGridView2.Rows[i].Cells[j].Value = sum;
                }
            }
        }


        private void button3_Click(object sender, EventArgs e) // Умножение матрицы
        {

            // Очищаем результатную матрицу
            dataGridView2.Rows.Clear();
            dataGridView2.RowCount = dataGridView1.RowCount;
            dataGridView2.ColumnCount = dataGridView3.ColumnCount;

            // Выполняем умножение матриц
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView3.ColumnCount; j++)
                {
                    int sum = 0;
                    for (int k = 0; k < dataGridView1.ColumnCount; k++)
                    {
                        int value1 = Convert.ToInt32(dataGridView1.Rows[i].Cells[k].Value);
                        int value2 = Convert.ToInt32(dataGridView3.Rows[k].Cells[j].Value);
                        sum += value1 * value2;
                    }
                    dataGridView2.Rows[i].Cells[j].Value = sum;
                }
            }
        }




        private void button4_Click(object sender, EventArgs e) // Транспонирование матрицы 
        {

            dataGridView2.Rows.Clear();


            dataGridView2.RowCount = dataGridView1.ColumnCount; 
            dataGridView2.ColumnCount = dataGridView1.RowCount; 

   
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {

                    int value = Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Value);

 
                    dataGridView2.Rows[j].Cells[i].Value = value;
                }
            }
        }
    }
}