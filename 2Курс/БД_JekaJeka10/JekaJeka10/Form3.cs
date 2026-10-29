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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            double kurs = 91.15;
            double n1 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / kurs;

            label2.Text = answer.ToString("0.00") + " Доллар(а)";

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            double kurs = 12.65;
            double n1 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / kurs;

            label2.Text = answer.ToString("0.00") + " Юань(я)";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double kurs = 0.200513;
            double n1 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / kurs;

            label2.Text = answer.ToString("0.00") + " Тенге";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double kurs = 0.616513;
            double n1 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / kurs;

            label2.Text = answer.ToString("0.00") + " Йена(ы)";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double kurs = 98.09;
            double n1 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / kurs;

            label2.Text = answer.ToString("0.00") + " Евро";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form4 NewForm4 = new Form4();
            NewForm4.Show();
        }
    }
}
