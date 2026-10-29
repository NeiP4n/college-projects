using System;
using System.Windows.Forms;
using Npgsql;

namespace BdManga
{
    public partial class Registration : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=YOUR_PASSWORD;Database=Manga";

        public Registration()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void cuiButton3_Click(object sender, EventArgs e)
        {
            string email = Mailtextbox.Content;
            string login = Logintextbox.Content;
            string password = Passwordtextbox.Content;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный адрес электронной почты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Random random = new Random();
            string activationCode = random.Next(100000, 999999).ToString();

            MessageBox.Show($"Сгенерирован код активации: {activationCode}. Отправьте его на почту.");

            string userInput = Microsoft.VisualBasic.Interaction.InputBox("Введите активационный код, который вы получили на почту:", "Активация аккаунта");

            if (userInput == activationCode)
            {
                if (RegisterUser(email, login, password))
                {
                    MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Login loginForm = new Login();
                    this.Hide();
                    loginForm.Show();
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации. Возможно, такой логин или email уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный активационный код.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool RegisterUser(string email, string login, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO Accounts (Email, Login, Password) VALUES (@Email, @Login, @Password)", conn))
                {
                    cmd.Parameters.AddWithValue("Email", email);
                    cmd.Parameters.AddWithValue("Login", login);
                    cmd.Parameters.AddWithValue("Password", password);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return mail.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
