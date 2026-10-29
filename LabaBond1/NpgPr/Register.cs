using System.Text;
using Newtonsoft.Json;
using Npgsql;
using NpgPr;
using System.Net.Sockets;
using System.Net;

namespace LabaBond1
{
    public partial class Register : Form
    {
        private static string connectionString = @"Server=localhost;Port=5432;User Id=postgres;Password=YOUR_PASSWORD;Database=postgres;";

        public Register()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            webView22.Dock = DockStyle.Fill;

            TestDatabaseConnection();

            webView22.CoreWebView2InitializationCompleted += (s, e) =>
            {
                webView22.CoreWebView2.WebMessageReceived += (sender, args) =>
                {
                    string message = args.TryGetWebMessageAsString();
                    dynamic data = JsonConvert.DeserializeObject(message);

                    string lastName = data.nickname;
                    string numBilet = data.password;

                    if (CheckUser(lastName, numBilet))
                    {
                        Form1 form = new Form1();
                        form.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Неверная фамилия или номер читательского билета.");
                    }
                };
            };
        }


        private async void Register_Load(object sender, EventArgs e)
        {
            await webView22.EnsureCoreWebView2Async(null);
            string filePath = Path.Combine(Application.StartupPath, @"Resources\Index.html");
            string htmlContent = File.ReadAllText(filePath, Encoding.UTF8);
            webView22.NavigateToString(htmlContent);
        }

        private void TestDatabaseConnection()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Подключение к базе данных успешно!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения: " + ex.Message);
                }
            }
        }

        private bool CheckUser(string lastName, string numBilet)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*)  
                                    FROM Читатели
                                    WHERE Фамилия = @Фамилия 
                                    AND 
                                    НомерБилета = @НомерБилета";

                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Фамилия", lastName);
                    command.Parameters.AddWithValue("@НомерБилета", numBilet);

                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка проверки пользователя: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
