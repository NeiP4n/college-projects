using Npgsql;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace DZJekaJeka2
{
    public partial class Form4 : Form
    {
        public ToolTip toolTip;
        public static string connectString = "";
        private OleDbConnection accessConnection;
        private NpgsqlConnection postgreConnection;

        public Form4()
        {
            try
            {
                accessConnection = new OleDbConnection();
                postgreConnection = new NpgsqlConnection();
                InitializeComponent();
                LoadDatabases();
                toolTip = new ToolTip();
                this.KeyPreview = true;
                this.KeyDown += Form1_KeyDown;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {

                toolTip.Show("Прикольно", this, 450, 150);
            }
        }
        private void LoadDatabases()
        {
            // Добавление элементов в комбобокс
            comboBox3.Items.Add("Access Database");
            comboBox3.Items.Add("PostgreSQL Database");
        }

        private void LoadTables()
        {
            try
            {
                if (comboBox3.SelectedItem == null)
                    return;
                comboBox1.Items.Clear();

                if (comboBox3.SelectedItem.ToString() == "Access Database")
                {
                    connectString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Database1.mdb;";
                    accessConnection.ConnectionString = connectString;
                    accessConnection.Open();

                    DataTable schemaTable = accessConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    if (schemaTable != null)
                    {
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            comboBox1.Items.Add(row["TABLE_NAME"]);
                        }
                    }
                }
                else if (comboBox3.SelectedItem.ToString() == "PostgreSQL Database")
                {
                    connectString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Buhgalteria;";
                    postgreConnection.ConnectionString = connectString;
                    postgreConnection.Open();

                    DataTable schemaTable = postgreConnection.GetSchema("Tables");
                    if (schemaTable != null)
                    {
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            comboBox1.Items.Add(row["TABLE_NAME"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
            }
            finally
            {
                accessConnection.Close();
                postgreConnection.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox3.SelectedItem == null || comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите базу данных и таблицу.");
                    return;
                }

                string tableName = comboBox1.SelectedItem.ToString();

                if (comboBox3.SelectedItem.ToString() == "Access Database")
                {
                    accessConnection.Open();

                    if (!string.IsNullOrEmpty(textBox1.Text))
                    {
                        int id = Convert.ToInt32(textBox1.Text);
                        using (OleDbCommand command = new OleDbCommand($"DELETE FROM [{tableName}] WHERE ID_{tableName} = @ID", accessConnection))
                        {
                            command.Parameters.AddWithValue("@ID", id);
                            command.ExecuteNonQuery();
                            MessageBox.Show("Запись успешно удалена.");
                        }
                    }
                    else
                    {
                        using (OleDbCommand command = new OleDbCommand($"DROP TABLE [{tableName}]", accessConnection))
                        {
                            command.ExecuteNonQuery();
                            MessageBox.Show($"Таблица '{tableName}' успешно удалена.");
                        }
                    }
                }
                else if (comboBox3.SelectedItem.ToString() == "PostgreSQL Database")
                {
                    postgreConnection.Open();

                    if (!string.IsNullOrEmpty(textBox1.Text))
                    {
                        int id = Convert.ToInt32(textBox1.Text);
                        using (NpgsqlCommand npgsqlcommand = new NpgsqlCommand($"DELETE FROM \"{tableName}\" WHERE ID_{tableName} = @ID", postgreConnection))
                        {
                            npgsqlcommand.Parameters.AddWithValue("@ID", id);
                            npgsqlcommand.ExecuteNonQuery();
                            MessageBox.Show("Запись успешно удалена.");
                        }
                    }
                    else
                    {
                        using (NpgsqlCommand npgsqlcommand = new NpgsqlCommand($"DROP TABLE \"{tableName}\"", postgreConnection))
                        {
                            npgsqlcommand.ExecuteNonQuery();
                            MessageBox.Show($"Таблица '{tableName}' успешно удалена.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
            finally
            {
                accessConnection.Close();
                postgreConnection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
