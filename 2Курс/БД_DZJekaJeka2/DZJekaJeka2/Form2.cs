using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using Npgsql;

namespace DZJekaJeka2
{
    public partial class Form2 : Form
    {
        public static string connectString = "";
        private OleDbConnection accessConnection;
        private NpgsqlConnection postgreConnection;

        public Form2()
        {
            InitializeComponent();
            try
            {
                accessConnection = new OleDbConnection();
                postgreConnection = new NpgsqlConnection();
                LoadDatabases();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadDatabases()
        {
            comboBox2.Items.Add("Access Database");
            comboBox2.Items.Add("PostgreSQL Database");
        }

        private void LoadTables()
        {
            try
            {
                if (comboBox2.SelectedItem == null)
                    return;

                comboBox1.Items.Clear();

                if (comboBox2.SelectedItem.ToString() == "Access Database")
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

                    accessConnection.Close();
                }
                else if (comboBox2.SelectedItem.ToString() == "PostgreSQL Database")
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

                    postgreConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                    return;

                if (comboBox2.SelectedItem.ToString() == "Access Database")
                {
                    OleDbDataAdapter da = new OleDbDataAdapter($"SELECT * FROM [{comboBox1.SelectedItem}]", accessConnection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
                else if (comboBox2.SelectedItem.ToString() == "PostgreSQL Database")
                {
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter($"SELECT * FROM \"{comboBox1.SelectedItem}\"", postgreConnection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
 