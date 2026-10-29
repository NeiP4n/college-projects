using DemoExam.ShopDbDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoExam
{
    public partial class LoginForm : MetroFramework.Forms.MetroForm
    {
        public static string connPath = Properties.Settings.Default.ShopDbConnectionString;
        public static ПользователиTableAdapter userTableAdapter = new ПользователиTableAdapter();
        public static ClientForm clientForm = new ClientForm();

        public LoginForm()
        {
            InitializeComponent();

            EventManager.OnVisibled += DisableVisibleElements(false);
        }

        private void TryLogin()
        {
            using (IDbConnection conn = new SqlConnection(connPath))
            {
                conn.Open();
                try
                {
                    var login = userTableAdapter.GetDataUser(tbLogin.Text, tbPassword.Text);

                    //userTableAdapter.Fill(login);

                    if (login.Rows.Count > 0)
                    {
                        ClientForm clientForm = new ClientForm();

                        string name = login[0].Логин;
                        string password = login[0].Пароль;
                        string role = login[0].Роль_сотрудника;

                        Permissions.roles = role;

                        switch (Permissions.roles)
                        {
                            case "Администратор":
                                MessageBox.Show($"Удалось авторизироваться {Permissions.roles}");

                                Permissions.AccessDvg(clientForm.товарыDataGridView, Permissions.IsAdmin);

                                MessageBox.Show(clientForm.товарыDataGridView.ReadOnly.ToString());

                                clientForm.ShowDialog();
                                return;
                            case "Авторизированный клиент":
                                MessageBox.Show($"Удалось авторизироваться {Permissions.roles}");

                                Permissions.AccessDvg(clientForm.товарыDataGridView, Permissions.IsClient);

                                MessageBox.Show(clientForm.товарыDataGridView.ReadOnly.ToString());

                                clientForm.ShowDialog();
                                return;
                            case "Менджер":
                                MessageBox.Show($"Удалось авторизироваться {Permissions.roles}");

                                Permissions.AccessDvg(clientForm.товарыDataGridView, Permissions.IsManager);

                                MessageBox.Show(clientForm.товарыDataGridView.ReadOnly.ToString());

                                clientForm.ShowDialog();
                                return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверные данные");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lblLogin_Click(object sender, EventArgs e)
        {

        }

        private void lblPassword_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            TryLogin();

            //ClientForm frm = new ClientForm();


        }


        private void DisableVisibleElements(bool isVisible)
        {
            isVisible = false;
        }
    }
}
