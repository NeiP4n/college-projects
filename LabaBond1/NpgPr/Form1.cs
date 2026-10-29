using Npgsql;
using System.Data;

namespace NpgPr
{
    public partial class Form1 : Form
    {
        private static string connectionString = @"Server=localhost;Port=5432;User Id=postgres;Password=YOUR_PASSWORD;Database=MangaBD;";
        public Form1()
        {
            InitializeComponent();
            LoadTables();
        }

        private void Form1_Load(object sender, EventArgs e) => LoadCatalog();
        private void LoadCatalog() => Query($"SELECT * FROM {comboBox1.Text}", dataGridView1);


        private void button1_Click(object sender, EventArgs e)
        {
            LoadCatalog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Query(textBox1.Text, dataGridView1);
        }


        private void LoadTables()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                DataTable schemaTable = connection.GetSchema("Tables");

                foreach (DataRow row in schemaTable.Rows)
                {
                    comboBox1.Items.Add(row["TABLE_NAME"].ToString());
                }
            }
        }
        private void Query(string query, DataGridView gridView)
        {

            {
                NpgsqlConnection npgsqlConn = new NpgsqlConnection(connectionString);
                try
                {
                    npgsqlConn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConn);

                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    if (table.Rows.Count == 0)
                    {
                        MessageBox.Show("    .", "");
                    }

                    gridView.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("  : " + ex.Message, "");
                }
            }
        }
    }
}
