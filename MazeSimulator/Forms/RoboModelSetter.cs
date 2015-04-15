using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MazeSimulator
{
    public partial class RobotModelSetter : Form
    {
        public Robot robot = null;
        List<string> models = new List<string>();
        public RobotModelSetter()
        {
            InitializeComponent();
        }

        public void setup(string robotModel)
        {
            foreach (Type t in this.GetType().Assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(Robot))) 
                {
                    functionComboBox.Items.Add(t.Name);
                }
            }

            robot = RobotModelFactory.getRobotModel(robotModel);
            if (robot != null)
            {
                for (int j = 0; j < functionComboBox.Items.Count; j++)
                    if (functionComboBox.Items[j].ToString().Equals(robot.Name))
                    {
                        functionComboBox.SelectedIndex = j;
                        break;
                    }
            }
        }


        private void functionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            robot = RobotModelFactory.getRobotModel(functionComboBox.SelectedItem.ToString());
            Invalidate();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
