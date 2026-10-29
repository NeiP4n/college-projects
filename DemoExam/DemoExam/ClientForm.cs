using DemoExam.ShopDbDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoExam
{
    public partial class ClientForm : MetroFramework.Forms.MetroForm
    {
        private int time = 1000;
        public ClientForm()
        {
            InitializeComponent();

         
        }

        private void товарыBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.товарыBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.shopDbDataSet);

        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            товарыTableAdapter.Fill(shopDbDataSet.Товары);

            EventManager.ExecuteOnVisibled();
        }

        private void metroProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
