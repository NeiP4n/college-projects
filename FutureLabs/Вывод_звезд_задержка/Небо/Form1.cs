using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Небо
{
    public partial class Form1 : Form
    {
        List<zvezda> zvlist = new List<zvezda>();
        
        public Form1()
        {
            InitializeComponent();
        }

        class zvezda
        {
            float x, y;
            bool p;
            Color c = new Color();
            float sk, razmer;
            public zvezda()
            {
                x = y = 0;
                p = false;
                sk = 0;
                razmer = 0;
            }
            public bool The_p
            {
                set
                {
                    p = value;
                }
                get
                {
                    return p;
                }

            }

            public float The_x
            {
                set
                {
                    x = value;
                }
                get
                {
                    return x;
                }
            }

            public float The_y
            {
                set
                {
                    y = value;
                }
                get
                {
                    return y;
                }
            }
            public Color The_c
            {
                set
                {
                    c = value;
                }
                get 
                {
                    return c;
                }
            }
            public float The_sk
            {
                set
                {
                    sk = value;
                }
                get
                {
                    return sk;
                }
            }
            public float The_razmer
            {
                set
                {
                    razmer = value;
                }
                get
                {
                    return razmer;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Risovanie();
            //Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
 
            Graphics gr = e.Graphics;
            Random rand = new Random();
            int dl = 0;
            foreach (zvezda zve in zvlist)
            {
                //Thread.Sleep(25);
                //System.Windows.Forms.Application.DoEvents();
                dl = (int)(rand.NextDouble() * (double)2) + 2;
                //System.Drawing.Drawing2D.LinearGradientBrush linrgb = new System.Drawing. LinearGradientBrush(zve.The_c);
                //System.Drawing.Drawing2D.LinearGradientBrush = new System.Drawing.Drawing2D
                gr.FillEllipse(Brushes.Purple, zve.The_x, zve.The_y, dl, dl); 
            }
           
        }

        void Risovanie()
        {
            zvlist.Clear();
            zvezda zv = new zvezda();
            Random rnd = new Random();
            Random rcolor = new Random();
            for (int i = 0; i < 500; i++)
            {
                zv = new zvezda();
                //rnd = new Random();
                zv.The_x = (float)(rnd.NextDouble() * (double)pictureBox1.Width);
                zv.The_y = (float)(rnd.NextDouble() * (double)pictureBox1.Height);
                zv.The_p = true;
                int r = (int)rcolor.Next(0, 255);
                int g = (int)rcolor.Next(0, 255);
                int b = (int)rcolor.Next(0, 255);
                Color c = new Color();
                c = Color.FromArgb(r, g, b);
                zv.The_c = c;
                zv.The_sk = (float)rnd.NextDouble() * 2 + 1;
                zv.The_razmer = (float)rnd.NextDouble() * 4;
                zvlist.Add(zv);
            }
            izm();
        }

        void izm()
        {
            Graphics gra = pictureBox1.CreateGraphics();
            Random rand = new Random();
            int dl = 0;
            foreach (zvezda zve in zvlist)
            {
                Thread.Sleep(10);
                System.Windows.Forms.Application.DoEvents();
                //dl = (int)(rand.NextDouble() * (double)2) + 2;
                Brush bru = new SolidBrush(zve.The_c);
                gra.FillEllipse(bru, zve.The_x, zve.The_y, zve.The_razmer, zve.The_razmer);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            float Tx = 0;
            Graphics gra = pictureBox1.CreateGraphics();
            Random rnd = new Random();
            for (int i = 0; i < 100000; i++)
            {
                foreach (zvezda zve in zvlist)
                {
                    if (zve.The_x <= pictureBox1.Width / 2 + 0.5 && zve.The_x >= pictureBox1.Width / 2 - 0.5)
                    {
                        zve.The_y = pictureBox1.Height / 2;
                        zve.The_x = pictureBox1.Width / 2;
                    }
                    else
                    {
                        if (zve.The_x <= pictureBox1.Width / 2)
                        {
                            zve.The_x += (float)0.5;
                            Tx = coord(zve.The_y, zve.The_x, zve.The_x + (float)0.5);
                        }
                        else
                        {
                            zve.The_x -= (float)0.5;
                            Tx = coord(zve.The_y, zve.The_x, zve.The_x - (float)0.5);
                        }
                        if (Tx >= 0.005 && Tx <= pictureBox1.Height)
                            zve.The_y = Tx;
                    }

                }
                Thread.Sleep(30);
                System.Windows.Forms.Application.DoEvents();
                pictureBox1.Refresh();
                foreach (zvezda zve in zvlist)
                {
                    Brush bru = new SolidBrush(zve.The_c);
                    gra.FillEllipse(bru, zve.The_x, zve.The_y, zve.The_razmer, zve.The_razmer);
                }
            }
        }

        private float coord(float y1, float x1, float x)
        {
            return (x * pictureBox1.Height / 2 - y1 * x - x1 * pictureBox1.Height / 2 + y1 * pictureBox1.Width / 2) / (pictureBox1.Width / 2 - x1);
        }


    }
}
