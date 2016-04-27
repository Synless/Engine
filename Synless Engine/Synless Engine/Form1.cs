using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synless_Engine
{
    public partial class Form1 : Form
    {


        bool first = true;
        Engine motor = new Engine();

        public Form1()
        {
            // INITIALIZE COMPONENT + BLACK BACKGROUND
            InitializeComponent();
            Bitmap screen = new Bitmap(640, 360);
            Color b = Color.FromArgb(0, 0, 0);
            for (int y = 0; y < screen.Height; y++)
            { for (int x = 0; x < screen.Width; x++) { screen.SetPixel(x, y, b); } }
            pictureBox1.Image = screen;
            
        }

        private void TE_Tick(object sender, EventArgs e)
        {
            // WHEN TE TICK, GET THE IMAGE DISPLAYED EXCEPT THE FIRST TIME
            if (first) { first = false; }
            else { pictureBox1.Image = motor.GetScreen(); Application.DoEvents(); }
            textBox6.Text = motor.getAccX().ToString();
            textBox7.Text = motor.getSpeedX().ToString();
            textBox8.Text = motor.getPosX().ToString();
            textBox9.Text = motor.getFricX().ToString();
            textBox10.Text = motor.getExtX().ToString();
            textBox1.Text = motor.getAccY().ToString();
            textBox2.Text = motor.getSpeedY().ToString();
            textBox3.Text = motor.getPosY().ToString();
            textBox4.Text = motor.getFricY().ToString();
            textBox5.Text = motor.getExtY().ToString();
            textBox11.Text = motor.getG().ToString();
            textBox12.Text = motor.getP().ToString();

        }

        #region KeyHandler
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            motor.KeyUp(e);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            motor.KeyDown(e);
        }
        #endregion
    }
}
