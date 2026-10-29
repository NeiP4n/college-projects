using System;
using System.Drawing;
using System.Windows.Forms;

namespace JSc1
{
    public partial class Form2 : Form
    {
        string NameFile;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Multiline = true;
            textBox1.Clear();

            button1.Text = "Открыть";
            button1.TabIndex = 0;

            button2.Text = "Сохранить";

            this.Text = "Здесь кодировка Unicode";

            NameFile = @"D:\Text1.txt";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var Reader = new System.IO.StreamReader(NameFile);

                textBox1.Text = Reader.ReadToEnd();
                Reader.Close();
            }
            catch (System.IO.FileNotFoundException Situation)
            {
                MessageBox.Show(Situation.Message + "\n" +
                              "Нет такого файла", "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var Writer = new System.IO.StreamWriter(NameFile, false);

                Writer.Write(textBox1.Text);
                Writer.Close();
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
