using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Lab2_Dudchenko_tomogram_visualizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        enum Mode { Quads, Texture2D, QuadStrip };
        private Mode mode = Mode.Quads;
        private Bin bin = new Bin();
        private View view = new View();
        private bool loaded = false;
        private int currentLayer;
        private DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        private int FrameCount;
        private bool needReload = false;

        private int min;
        private int width;

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBIN(str);
                trackBar1.Maximum = Bin.Z - 1;
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint_1(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                switch (mode)
                {
                    case Mode.Quads:
                        view.DrawQuads(currentLayer, min, width);
                        glControl1.SwapBuffers();
                        break;
                    case Mode.Texture2D:
                        if (needReload)
                        {
                            view.generateTextureImage(currentLayer, min, width);
                            view.Load2DTexture();
                            needReload = false;
                        }
                        view.DrawTexture();
                        glControl1.SwapBuffers();
                        break;
                    case Mode.QuadStrip:
                        view.DrawQuadStrip(currentLayer, min, width);
                        glControl1.SwapBuffers();
                        break;
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
        }
        void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CT Visualaiser (fps={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            mode = Mode.Quads;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            mode = Mode.Texture2D;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            mode = Mode.QuadStrip;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            min = trackBar2.Value;
            needReload = true;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            width = trackBar3.Value;
            needReload = true;
        }
    }
}
