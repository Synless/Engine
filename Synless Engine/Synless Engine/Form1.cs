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
        Engine engine = new Engine();
        bool first = true;

        public Form1()
        {
            // INITIALIZE COMPONENT + BLACK BACKGROUND
            InitializeComponent();
            Bitmap screen = new Bitmap(640, 360);
            Color b = Color.FromArgb(0, 0, 0);
            for (int y = 0; y < screen.Height; y++)
            { for (int x = 0; x < screen.Width; x++) { screen.SetPixel(x, y, b); } }
            pictureBox1.Image = screen;
            Timer TE = new Timer();
            TE.Interval = 40;
            TE.Tick += TE_Tick;
            TE.Enabled = true;
            TE.Start();
        }
        private void TE_Tick(object sender, EventArgs e)
        {
            // WHEN TE TICK, GET THE IMAGE DISPLAYED EXCEPT THE FIRST TIME
            if (first) { first = false; }
            else { pictureBox1.Image = engine.getScreen(); Application.DoEvents(); }
            textBox6.Text   = engine.getAccX().ToString();
            textBox7.Text   = engine.getSpeedX().ToString();
            textBox8.Text   = engine.getPosX().ToString();
            textBox9.Text   = engine.getFricX().ToString();
            textBox10.Text  = engine.getExtX().ToString();
            textBox1.Text   = engine.getAccY().ToString();
            textBox2.Text   = engine.getSpeedY().ToString();
            textBox3.Text   = engine.getPosY().ToString();
            textBox4.Text   = engine.getFricY().ToString();
            textBox5.Text   = engine.getExtY().ToString();
            textBox11.Text  = engine.getG().ToString();
            textBox12.Text  = engine.getP().ToString();

        }
        #region KeyHandler
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            engine.KeyUp(e);
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            engine.KeyDown(e);
        }
        #endregion
    }
}
