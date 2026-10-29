using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace KG_lab1
{
    public partial class Form1 : Form
    {
        Points[] Zvezd = new Points[500];
        bool fl = false;
        struct Points
        {
            public float x, y;
            public Color color;
        }
        public Form1()
        {
            InitializeComponent();
            zvezdi();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        void zvezdi()
        {
            Random rnd = new Random();
            for (int i = 0; i < 500; i++)
            {
                Zvezd[i].x = (float)rnd.Next() % pictureBox1.Width;
                Zvezd[i].y = (float)rnd.Next() % pictureBox1.Height;
                Zvezd[i].color = Color.FromArgb((int)rnd.Next(0, 255), (int)rnd.Next(0, 255), (int)rnd.Next(0, 255));
            }
        }
        
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 500; i++)
            {
                e.Graphics.FillEllipse(new SolidBrush(Zvezd[i].color), Zvezd[i].x, Zvezd[i].y, 3, 3);
                pictureBox1.Invalidate();
                if (fl) vcentr();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fl = true;
            pictureBox1.Invalidate();
        }
        float dvig(float x1, float y1, float x)
        {
            float y = (y1 - pictureBox1.Height / 2) * (x - pictureBox1.Width / 2) / (x1 - pictureBox1.Width / 2) + pictureBox1.Height / 2;
            return y;
        }
        void vcentr()
        {
            for (int i = 0; i < 500; i++)
            {
                if (Zvezd[i].x < pictureBox1.Width / 2)
                    Zvezd[i].y = dvig(Zvezd[i].x, Zvezd[i].y, Zvezd[i].x += 0.0001f);
                else
                    Zvezd[i].y = dvig(Zvezd[i].x, Zvezd[i].y, Zvezd[i].x -= 0.001f);
            }
            pictureBox1.Invalidate();
        }
    }
}
