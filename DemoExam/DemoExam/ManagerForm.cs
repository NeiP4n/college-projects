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
    public partial class ManagerForm : MetroFramework.Forms.MetroForm
    {

        public ManagerForm()
        {
            InitializeComponent();


            tbSearch.TextChanged += tbSearch_TextChanged;
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "shopDbDataSet.Товары". При необходимости она может быть перемещена или удалена.
            this.товарыTableAdapter.Fill(this.shopDbDataSet.Товары);
        }

        private void товарыBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.товарыBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.shopDbDataSet);

        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            using (IDbConnection conn = new SqlConnection(LoginForm.connPath)) {
                conn.Open();

                var search = товарыTableAdapter.GetDataSearch(tbSearch.Text);
                товарыDataGridView.DataSource = search;

                //товарыTableAdapter.Fill(search);
            }
        }
        private void GetPrice()
        {
            using (IDbConnection conn = new SqlConnection(LoginForm.connPath))
            {
                conn.Open();

                //var price = товарыTableAdapter.GetPrice();
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(LoginForm.connPath))
                {
                    conn.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT Фото FROM Товары", conn))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        товарыDataGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void товарыDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
