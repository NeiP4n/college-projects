using System;
using System.Windows.Forms;
using Npgsql;

namespace BdManga
{
    public partial class Login : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Manga";
        User user;
        public string Name;

        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            string login = Logintextbox.Content;
            string password = Passwordtextbox.Content;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (AuthenticateUser(login, password))
            {
                MessageBox.Show("Вход выполнен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Base mainForm = new Base(user);
                this.Hide();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AuthenticateUser(string login, string password)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT Login, Email FROM Accounts WHERE Login = @login AND Password = @password", conn))
                    {
                        cmd.Parameters.AddWithValue("login", login);
                        cmd.Parameters.AddWithValue("password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string loginFromDb = reader.GetString(0);
                                string emailFromDb = reader.GetString(1);

                                MessageBox.Show($"Логин из базы данных: {loginFromDb}, email: {emailFromDb}");

                                user = new User(loginFromDb, password, emailFromDb);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }




        private void cuiButton2_Click(object sender, EventArgs e)
        {
            Registration registrationForm = new Registration();
            this.Hide();
            registrationForm.Show();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
