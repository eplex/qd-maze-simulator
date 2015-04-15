using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NetworkVisualization; 
using SharpNeatLib;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.CPPNs;

namespace MazeSimulator
{
    public partial class GenomeVisualizerForm : Form
    {

        private int currentSelectedIndex;
        private Point _savedMouseLocation;
        private int? _currentSelectedConnectionIndex;
        private NeatGenome _currentGenome;
        private SolidBrush brush = new SolidBrush(Color.Red);
        List<PointF> hiddenNeurons;
        bool updatePicture;
        ConnectionGeneList connections;

        public GenomeVisualizerForm(NeatGenome genome)
        {
            InitializeComponent();
            updatePicture = true;
            _currentGenome = genome;
            _currentSelectedConnectionIndex = -1;
            //currentSelectedIndex = null;

            ShowGenomeNetwork(genome);
            ShowGenomeConnections(genome);

            ShowHyperCubeSlide(genome);
        }

        void onPictureBoxClick()
        {

        }

        private void ShowHyperCubeSlide(NeatGenome genome)
        {
            float x = (float)numericUpDownX.Value;
            float y = (float)numericUpDownY.Value;

            float starty = -1.0f;
            float startx;

            INetwork net = genome.Decode(null);
            float[] coordinates = new float[4];
            coordinates[0] = x;
            coordinates[1] = y;
            double output;

            if (pictureBox1 != null) return;

            Graphics g = pictureBox1.CreateGraphics();

            while (starty < 1.0f)
            {            
                startx = -1.0f;

                coordinates[3] = starty;
                while (startx < 1.0f)
                {
                    coordinates[2] = startx;
                    net.ClearSignals();
                    net.SetInputSignals(coordinates);
                    net.MultipleSteps(3);
                    output = net.GetOutputSignal(0);
                    //HyberCubeSlidePanel.
                    if (output < 0.0f)
                    {
                        brush.Color = Color.FromArgb((int)(output* -255.0f), 0, 0);
                    }
                    else
                    {
                        brush.Color = Color.FromArgb(0,0,(int)(output * 255.0f));
                    }

                    g.FillRectangle(brush, new Rectangle((int)(startx * 100.0f)+100, 100-(int)(starty * 100.0f), 1, 1));
                    
                    //Show origin
                    startx += 0.01f;
                }
                starty += 0.01f;
            }
            brush.Color = Color.Green;
            g.FillEllipse(brush, new Rectangle((int)(x * 100.0f) +100, 100-(int)(y * 100.0f), 5, 5));
        }

        private void ShowGenomeNetwork(NeatGenome genome)
        {

            NetworkControl networkControl = new NetworkControl();
            networkControl.Dock = DockStyle.Fill;
            panelNetWorkViewer.Controls.Clear();
            panelNetWorkViewer.Controls.Add(networkControl);

            /* create network model to draw the network */
            NetworkModel networkModel = GenomeDecoder.DecodeToNetworkModel(genome);
            GridLayoutManager layoutManager = new GridLayoutManager();
            layoutManager.Layout(networkModel, networkControl.Size);
            networkControl.NetworkModel = networkModel;
        }


        void ShowGenomeConnections(NeatGenome genome)
        {
            int savedIndex = listBoxConnections.SelectedIndex;
            listBoxConnections.Items.Clear();

            /* show information about each connection in the listbox */
            foreach (ConnectionGene connection in genome.ConnectionGeneList)
            {
                NeuronGene sourceNeuron = genome.NeuronGeneList.GetNeuronById(connection.SourceNeuronId);
                NeuronGene destinationNeuron = genome.NeuronGeneList.GetNeuronById(connection.TargetNeuronId);
                string info = string.Format("{0}({1}) --> {2}({3}); weight:{4:F3})", connection.SourceNeuronId, sourceNeuron.NeuronType, connection.TargetNeuronId, destinationNeuron.NeuronType, connection.Weight);
                /* add the info text and the conection object itself in the listitem */
                ListItem item = new ListItem("", info, connection);
                listBoxConnections.Items.Add(item);
            }

            /* try to update selected savedIndex and refresh the drawed network */
            listBoxConnections.SelectedIndex = listBoxConnections.Items.Count > savedIndex ? savedIndex : -1;
           // ShowNetworkFromGenome(genome);
        }


        private void listBoxConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentSelectedConnectionIndex != null)
                listBoxConnections.SelectedIndex = _currentSelectedConnectionIndex.Value;   
        }

        private void listBoxConnections_MouseMove(object sender, MouseEventArgs e)
        {

            /* reset the selected index */
            if (_currentSelectedConnectionIndex != null)
                listBoxConnections.SelectedIndex = _currentSelectedConnectionIndex.Value;

            /* caclulate the new connection weight value */
            if (e.Button == MouseButtons.Left)
            {
                int diff = (int)((e.X - _savedMouseLocation.X));
                diff = hScrollBarConnectionWeights.Value + diff;
              //  diff = diff < -1000 ? -1000 : diff > 1000 ? 1000 : diff;
                hScrollBarConnectionWeights.Value = diff;
            }
            _savedMouseLocation = e.Location;
        }

        private void listBoxConnections_MouseUp(object sender, MouseEventArgs e)
        {
        //    _currentSelectedConnectionIndex = null;
        }

        private void hScrollBarConnectionWeights_ValueChanged(object sender, EventArgs e)
        {
            if (_currentGenome != null && _currentSelectedConnectionIndex.Value != -1)
            {
                _currentGenome.ConnectionGeneList[_currentSelectedConnectionIndex.Value].Weight = Double.Parse(weightTextBox.Text);
                //(double)hScrollBarConnectionWeights.Value / 100d;
                ShowGenomeConnections(_currentGenome);
              //  TryToUpdateImageFromNeatGenome(CurrentSelectedGenome);
            }
        }

        private void listBoxConnections_MouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentSelectedConnection != null)
            {
                _currentSelectedConnectionIndex = listBoxConnections.SelectedIndex;
                int value = (int)(CurrentSelectedConnection.Weight * 100);
                weightTextBox.Text = CurrentSelectedConnection.Weight.ToString();
                // value = value < -1000 ? -1000 : value > 1000 ? 1000 : value;
                hScrollBarConnectionWeights.Value = value;
            }
        }

        ConnectionGene CurrentSelectedConnection
        {
            get { return listBoxConnections.SelectedItem != null ? (listBoxConnections.SelectedItem as ListItem).Data as ConnectionGene : null; }
        }

        private void weightTextBox_KeyDown(object sender, KeyEventArgs e)
        {
           // Console.WriteLine(experiment.KeyCode);

            if (e.KeyCode == Keys.Return)
            {
                if (_currentGenome != null && _currentSelectedConnectionIndex.Value != -1)
                {
                    _currentGenome.ConnectionGeneList[_currentSelectedConnectionIndex.Value].Weight = Double.Parse(weightTextBox.Text);
                    ShowGenomeConnections(_currentGenome);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_currentGenome != null && _currentSelectedConnectionIndex.Value != -1)
            {
                _currentGenome.ConnectionGeneList[_currentSelectedConnectionIndex.Value].Weight = 0.0;
                weightTextBox.Text = "0.0";
                ShowGenomeConnections(_currentGenome);
            }
        }

        private void listBoxConnections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (_currentGenome != null && _currentSelectedConnectionIndex.Value != -1)
                {
                    _currentGenome.ConnectionGeneList[_currentSelectedConnectionIndex.Value].Weight = 0.0;
                    weightTextBox.Text = "0.0";
                    ShowGenomeConnections(_currentGenome);
                }
            }
        }

         private void numericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if (updatePicture)
                ShowHyperCubeSlide(_currentGenome);
        }

        private void numericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (updatePicture)
                ShowHyperCubeSlide(_currentGenome);
        }


        public void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {

          //  Console.WriteLine(experiment.X + " " + experiment.Y);
            float x = ((e.X / 200.0f) * 2.0f - 1);
            float y= ((e.Y / 200.0f) * 2.0f - 1);
            y *= -1.0f;
            if (x > 1.0f) x = 1.0f;
            if (y > 1.0f) y = 1.0f;
            updatePicture = false;
            numericUpDownX.Value = (decimal)x;
            updatePicture = true; //update after both variables have been changed
            numericUpDownY.Value = (decimal)y;
        }

        public void NetworkVisualizerForm_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1_MouseMove(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            float x = ((e.X / 200.0f) * 2.0f - 1);
            float y = ((e.Y / 200.0f) * 2.0f - 1);
            y *= -1.0f;
            if (x > 1.0f) x = 1.0f;
            if (y > 1.0f) y = 1.0f;
            label4.Text = "X="+x + "\type Y=" + y;
        }

        private void numericUpDownTolerance_ValueChanged(object sender, EventArgs e)
        {
            ShowHyperCubeSlide(_currentGenome);
        }
    }
}
