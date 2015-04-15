using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MazeSimulator
{
    public partial class ObjectRenamer : Form
    {
        public ObjectRenamer()
        {
            InitializeComponent();
        }

        public ObjectRenamer(string name)
        {
            InitializeComponent();
            this.textBox1.Text = name;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.Close();
        }

    }
}
