using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace JJLaba1C_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void CheckList(object sender, EventArgs e)
        {
            string message = "Выбраны следующие флаги:";
            if (checkBox1.Checked)
                message += "- Да,";
            if (checkBox2.Checked)
                message += " Нет,";
            if (checkBox3.Checked)
                message += " Возможно";
            MessageBox.Show(message);

        }

        private void NUD(object sender, EventArgs e)
        {
            string Number;
            Number = numeric.Value.ToString();
            MessageBox.Show(Number);
        }


        private void TextTB(object sender, EventArgs e)
        {
            string Text;
            Text = textBox1.Text;
            MessageBox.Show(Text);
        }

        private void Molodec(object sender, EventArgs e)
        {
            string textss = "Ты молодец";
            MessageBox.Show(textss);
                if (LabelJJ.Text != textss)
               {
                LabelJJ.Text = "Ты молодец";
            }
            
                
        }
    }
}
