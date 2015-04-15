using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MazeSimulator;

namespace MazeSimulator
{
    public partial class SettingsDialog : Form
    {
        public bool good;
        public Robot robot = null;
        List<string> models = new List<string>();
        public IFitnessFunction fitFun = null;
        public IBehaviorCharacterization bc = null;
        List<string> behaviors = new List<string>();

        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        public void setTextValues()
        {
            sensorNoiseDataLabel.Text = sensorNoiseTrackBar.Value.ToString();
            effectorNoiseDataLabel.Text = effectorNoiseTrackBar.Value.ToString();
            headingNoiseDataLabel.Text = headingNoiseTrackBar.Value.ToString();
            Invalidate();
        }

        private void okbutton_Click(object sender, EventArgs e)
        {
            good = true;
            this.Close();
        }

        private void effectorNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void sensorNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void headingNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void functionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            robot = RobotModelFactory.getRobotModel(robotModelComboBox.SelectedItem.ToString());
        }

        public void setupRobots(string robotModel)
        {
            foreach (Type t in this.GetType().Assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(Robot)))
                {
                    robotModelComboBox.Items.Add(t.Name);
                }
            }

            robot = RobotModelFactory.getRobotModel(robotModel);
            if (robot != null)
            {
                for (int j = 0; j < robotModelComboBox.Items.Count; j++)
                    if (robotModelComboBox.Items[j].ToString().Equals(robot.Name))
                    {
                        robotModelComboBox.SelectedIndex = j;
                        break;
                    }
            }
        }

        public void setupFitnessFunction(string fitnessFunction)
        {
            foreach (Type t in this.GetType().Assembly.GetTypes())
            {
                if (t.GetInterface("IFitnessFunction", true) != null)
                    this.functionComboBox.Items.Add(t.Name);
            }

            fitFun = FitnessFunctionFactory.getFitnessFunction(fitnessFunction);
            if (fitFun != null)
            {
                for (int j = 0; j < functionComboBox.Items.Count; j++)
                    if (functionComboBox.Items[j].ToString().Equals(fitFun.name))
                    {
                        functionComboBox.SelectedIndex = j;
                        break;
                    }
            }
            fillFitnessDescription();
        }

        private void fillFitnessDescription()
        {
            if (fitFun != null)
                textBox2.Text = fitFun.description;
            else
                textBox2.Text = "Not Found.";
            textBox2.Invalidate();
        }

        private void fillTexBehaviorTextBox()
        {
            if (bc != null)
                behaviorTextBox.Text = bc.description;
            else
                behaviorTextBox.Text = "Not Found.";
            behaviorTextBox.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fitFun = FitnessFunctionFactory.getFitnessFunction(this.functionComboBox.SelectedItem.ToString());
            fillFitnessDescription();
        }

        public void setupBehaviorCharazterization(string behavior)
        {
            foreach (Type t in this.GetType().Assembly.GetTypes())
            {
                if (t.GetInterface("IBehaviorCharacterization", true) != null)
                    behaviorComboBox.Items.Add(t.Name);
            }

            bc = BehaviorCharacterizationFactory.getBehaviorCharacterization(behavior);
            if (bc != null)
            {
                for (int j = 0; j < behaviorComboBox.Items.Count; j++)
                    if (behaviorComboBox.Items[j].ToString().Equals(bc.name))
                    {
                        behaviorComboBox.SelectedIndex = j;
                        break;
                    }
            }
            fillTexBehaviorTextBox();
        }

        private void behaviorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bc = BehaviorCharacterizationFactory.getBehaviorCharacterization(behaviorComboBox.SelectedItem.ToString());
            fillTexBehaviorTextBox();
            Invalidate();
        }

        private void cancelbutton_Click(object sender, EventArgs e)
        {
            good = false;
            Close();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {

        }

        private void EScheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void number_of_agents_TextChanged(object sender, EventArgs e)
        {
            if (!number_of_agents.Text.Equals(""))
            {
                if (Convert.ToInt32(number_of_agents.Text) > 1) this.EScheckBox.Enabled = false;
                else this.EScheckBox.Enabled = true;
            }
        }

        private void modulationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (modulationCheckBox.Checked)
            {
                adaptiveNetworkCheckBox.Checked = true;
            }
        }
    }
}
