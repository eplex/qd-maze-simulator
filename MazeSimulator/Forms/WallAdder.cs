using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MazeSimulator
{
    public partial class wallAdder : Form
    {
        public wallAdder()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // try
            {
                ((SimulatorVisualizer)Owner).experiment.environment.walls.Add(new Wall(double.Parse(X1textBox.Text), double.Parse(Y1textBox.Text), double.Parse(X2textBox.Text), double.Parse(Y2textBox.Text), true, ""));
            }
            //catch (Exception ex)
           // {
           //     MessageBox.Show("Invaild wall parameters");
           // }
        }
    }
}
