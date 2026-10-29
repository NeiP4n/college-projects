using System;
using System.Data;
using System.Windows.Forms;
using MyDatabaseLibrary;

namespace DZJekaJeka2
{
    public partial class Form3 : Form
    {
        private MyDatabaseLibrary.MyDatabaseLibrary databaseLibrary;

        public Form3()
        {
            InitializeComponent();
            try
            {
                string accessConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Database1.mdb;";
                string postgreConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Buhgalteria;";
                databaseLibrary = new MyDatabaseLibrary.MyDatabaseLibrary(accessConnectionString, postgreConnectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadTables()
        {
            try
            {
                if (comboBox10.SelectedItem == null)
                    return;

                comboBox1.Items.Clear();
                DataTable schemaTable = databaseLibrary.LoadTables(comboBox10.SelectedItem.ToString());
                if (schemaTable != null)
                {
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        comboBox1.Items.Add(row["TABLE_NAME"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
            }
        }

        private void LoadTables2()
        {
            try
            {
                if (comboBox2.SelectedItem == null)
                    return;

                ViborTablic.Items.Clear();
                DataTable schemaTable = databaseLibrary.LoadTables(comboBox2.SelectedItem.ToString());
                if (schemaTable != null)
                {
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        ViborTablic.Items.Add(row["TABLE_NAME"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Implement update logic using the selected table
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTables2();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
        private void button3_Click(object sender, EventArgs e)
        {

        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button4_Click_1(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void button3_Click_2(object sender, EventArgs e)
        {

        }
        private void button3_Click_1(object sender, EventArgs e)
        {

        }
    }
}
