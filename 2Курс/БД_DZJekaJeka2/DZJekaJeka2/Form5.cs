using Npgsql;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace DZJekaJeka2
{
    public partial class Form5 : Form
    {
        public static string connectStringAccess = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Database1.mdb;";
        public static string connectStringPostgre = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Buhgalteria;";
        private ToolTip toolTip;
        private OleDbConnection accessConnection;
        private NpgsqlConnection postgreConnection;

        public Form5()
        {
            try
            {
                accessConnection = new OleDbConnection(connectStringAccess);
                postgreConnection = new NpgsqlConnection(connectStringPostgre);
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

                toolTip.Show("Обновить не робит", this, 450, 50);
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

        private void LoadAttributes(string tableName)
        {
            if (comboBox3.SelectedItem.ToString() == "Access Database")
            {
                comboBox2.Items.Clear();

                if (!string.IsNullOrEmpty(tableName))
                {
                        accessConnection.Open();

                        DataTable schemaTable = accessConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                            new object[] { null, null, tableName, null });
                        if (schemaTable != null)
                        {
                            foreach (DataRow row in schemaTable.Rows)
                            {
                                string attributeName = row["COLUMN_NAME"].ToString();
                                comboBox2.Items.Add(attributeName);
                            }
                        }
                    

                }
            }
            else if (comboBox3.SelectedItem.ToString() == "PostgreSQL Database")
            {
                comboBox2.Items.Clear();

                if (!string.IsNullOrEmpty(tableName))
                {
                    try
                    {
                        postgreConnection.Open();

                        DataTable schemaTable = postgreConnection.GetSchema("Columns", new[] { null, null, tableName });

                        if (schemaTable != null)
                        {
                            foreach (DataRow row in schemaTable.Rows)
                            {
                                string attributeName = row["COLUMN_NAME"].ToString();
                                comboBox2.Items.Add(attributeName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке атрибутов: {ex.Message}");
                    }
                    finally
                    {
                        postgreConnection.Close();
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBox1.SelectedItem?.ToString();
            LoadAttributes(selectedTable);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedTable = comboBox1.SelectedItem?.ToString();
                string selectedAttribute = comboBox2.SelectedItem?.ToString();
                string newValue = textBox1.Text;

                if (!string.IsNullOrEmpty(selectedTable) && !string.IsNullOrEmpty(selectedAttribute) && !string.IsNullOrEmpty(newValue))
                {
                    if (comboBox3.SelectedItem.ToString() == "Access Database")
                    {
                        accessConnection.Open();

                        string updateQuery = $"UPDATE {selectedTable} SET {selectedAttribute} = @newValue";

                        using (OleDbCommand command = new OleDbCommand(updateQuery, accessConnection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (comboBox3.SelectedItem.ToString() == "PostgreSQL Database")
                    {
                        postgreConnection.Open();

                        string updateQuery = $"UPDATE \"{selectedTable}\" SET \"{selectedAttribute}\" = @newValue";

                        using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, postgreConnection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Данные успешно обновлены.");
                }
                else
                {
                    MessageBox.Show("Выберите таблицу, атрибут и введите новое значение.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}");
            }
            finally
            {
                accessConnection.Close();
                postgreConnection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null && !string.IsNullOrEmpty(textBox1.Text))
            {
                try
                {
                    string tableName = comboBox1.SelectedItem.ToString();
                    string attributeName = comboBox2.SelectedItem.ToString();
                    string newValue = textBox1.Text;

                    if (comboBox3.SelectedItem.ToString() == "Access Database")
                    {
                        accessConnection.Open();

                        string updateQuery = $"UPDATE [{tableName}] SET [{attributeName}] = @newValue";

                        using (OleDbCommand command = new OleDbCommand(updateQuery, accessConnection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.ExecuteNonQuery();
                        }
                        MessageBox.Show("Данные успешно сохранены.");
                    }
                    else if (comboBox3.SelectedItem.ToString() == "PostgreSQL Database")
                    {
                        postgreConnection.Open();

                        string updateQuery = $"UPDATE \"{tableName}\" SET \"{attributeName}\" = ";

                        // Проверяем тип данных newValue
                        if (int.TryParse(newValue.ToString(), out int intValue))
                        {
                            updateQuery += $"CAST(@newValue AS INTEGER)";
                        }
                        else if (newValue is string)
                        {
                            updateQuery += $"CAST(@newValue AS VARCHAR)";
                        }
                        else
                        {
                            // Обработка других типов данных, если необходимо
                            MessageBox.Show("Неподдерживаемый тип данных.");
                            return; // Завершаем выполнение метода, чтобы избежать выполнения запроса
                        }

                        using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, postgreConnection))
                        {
                            command.Parameters.AddWithValue("@newValue", newValue);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Данные успешно сохранены.");
                    }




                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                }
                finally
                {
                    accessConnection.Close();
                    postgreConnection.Close();
                }
            }
            else
            {
                MessageBox.Show("Выберите таблицу, атрибут и введите новое значение.");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTables();
        }
    }
}