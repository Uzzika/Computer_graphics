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
        //void Application_Idle(object sender, EventArgs e)
        //{
        //    while (glControl1.IsIdle)
        //    {
        //        displayFPS();
        //        glControl1.Invalidate();
        //    }
        //}

        private Bin bin = new Bin();
        private View view = new View();
        private bool loaded = false;
        private int currentLayer;

        private int min;
        private int width;

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBIN(str);
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint_1(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                view.DrawQuads(currentLayer);
                glControl1.SwapBuffers();
            }
        }
    }
}
