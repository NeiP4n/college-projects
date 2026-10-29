using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LabaBond1
{
    public partial class Form1 : Form
    {
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\NameDataBase.mdf;Integrated Security=True"; // Connection String Syntax - ADO.NET | Microsoft Learn
        public Form1() {
        InitializeComponent();
        LoadTables();

        this.WindowState = FormWindowState.Maximized;

        }
        private void Form1_Load(object sender, EventArgs e) => LoadCatalog();
        private void LoadCatalog() => Query("SELECT * FROM Каталог", dataGridView1);
        private void SearchButton_Click(object sender, EventArgs e)
        {
            string query = @"
                SELECT Фамилия, Имя, Отчество, НомерБилета, Адрес
                FROM Читатели
                WHERE Фамилия LIKE '%' + @Фамилия + '%' 
                OR Имя LIKE '%' + @Имя + '%';";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Фамилия", textBoxSearch.Text),
                new SqlParameter("@Имя", textBoxSearch.Text)
            };

            Query(query, dataGridView1, parameters);
        }

        private void SumButton_Click(object sender, EventArgs e) =>
            Query(@"SELECT Тема, COUNT(*) AS КоличествоКниг
                FROM Каталог 
                GROUP BY Тема", dataGridView1);


        private void topReaderButton_Click(object sender, EventArgs e)
        {
            Query(@"
                SELECT TOP 1 Каталог.Название, COUNT(*) AS КоличествоВыдач
                FROM ВыдачаКниг
                INNER JOIN Книги ON ВыдачаКниг.КнигаID = Книги.КнигаID
                INNER JOIN Каталог ON Книги.КаталогID = Каталог.КаталогID
                GROUP BY Каталог.Название
                ORDER BY КоличествоВыдач DESC;", dataGridView1);
        }

        private void SumBook_Click(object sender, EventArgs e) =>
            Query(@"SELECT COUNT(*) AS ОбщееКоличествоКниг 
                FROM КнижныйФонд;", dataGridView1);

        private void Query(string query, DataGridView gridView, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    if (table.Rows.Count == 0)
                    {
                        MessageBox.Show("Нет данных по вашему запросу.", "Информация");
                    }

                    gridView.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка выполнения запроса: " + ex.Message, "Ошибка");
                }
            }
        }
        private void LoadTables()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable schemaTable = connection.GetSchema("Tables");

                foreach (DataRow row in schemaTable.Rows)
                {
                    comboBox1.Items.Add(row["TABLE_NAME"].ToString());
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Query($"SELECT * FROM {comboBox1.Text}", dataGridView1);
        }
    }
}
