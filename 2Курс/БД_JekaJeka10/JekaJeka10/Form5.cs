using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JekaJeka10
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
                int n1 = Convert.ToInt32(textBox1.Text);

                int sum = 0;

                while (n1 > 0)
                {
                    sum += n1 % 10;
                    n1 /= 10;

                }

                if (sum % 3 == 0)
                {
                    label1.Text = n1.ToString("Кратна");
                }
                else
                {
                    label1.Text = n1.ToString("Не кратна");
                }
           
        }
    }
}
