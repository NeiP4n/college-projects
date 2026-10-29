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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        
            string monthName = textBox1.Text.ToLower();
            int days;

            switch (monthName)
            {
                case "январь":
                case "март":
                case "май":
                case "июль":
                case "август":
                case "октябрь":
                case "декабрь":
                    days = 31;
                    break;
                case "апрель":
                case "июнь":
                case "сентябрь":
                case "ноябрь":
                    days = 30;
                    break;
                case "февраль":
                    days = 28;
                    break;
                default:
                    label1.Text = "Ошибка: Неверное название месяца.";
                    return;
            }

            label1.Text = ($"Количество дней в месяце: {days}");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
