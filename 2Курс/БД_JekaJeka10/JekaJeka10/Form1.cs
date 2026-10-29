namespace JekaJeka10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 NewForm2 = new Form2();
            NewForm2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double n1 = Convert.ToDouble(textBox2.Text);
            double n2 = Convert.ToDouble(textBox1.Text);
            double answer = n1 + n2;

            Chisla.Text = answer.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            double n1 = Convert.ToDouble(textBox2.Text);
            double n2 = Convert.ToDouble(textBox1.Text);
            double answer = n1 - n2;

            Chisla.Text = answer.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double n1 = Convert.ToDouble(textBox2.Text);
            double n2 = Convert.ToDouble(textBox1.Text);
            double answer = n1 / n2;

            Chisla.Text = answer.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double n1 = Convert.ToDouble(textBox2.Text);
            double n2 = Convert.ToDouble(textBox1.Text);
            double answer = n1 * n2;

            Chisla.Text = answer.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form3 NewForm3 = new Form3();
            NewForm3.Show();
        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
