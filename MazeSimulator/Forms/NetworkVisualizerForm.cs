using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;
using System.Drawing;
using SharpNeatLib.NeatGenome;

namespace MazeSimulator
{
    class NetworkVisualizerForm : Form
    {
        private Robot selectedRobot;
        SolidBrush brush;
        Pen penConnection;
        ModularNetwork net;
        float dtx = 100;
        float dty = 100;
        float w;
        float startX;
        float startY;// = 1.1f * dty;
        Graphics g;
        int index;
        public GenomeVisualizerForm genomeVisualizerForm;
        bool drawHive = true;

        public NetworkVisualizerForm(Robot _selectedRobot, ModularNetwork _net) 
        {
            _net.UpdateNetworkEvent += networkUpdated;
            InitializeComponent();
            net = _net;
            selectedRobot = _selectedRobot;
            this.Text = "Network Visualizer [z="+ selectedRobot.ZStack+"]";
            SetBounds(1, 1, 320, 320);
            brush = new SolidBrush(Color.Red);
            penConnection = new Pen(Color.Black);
            penConnection.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
            penConnection.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            startX = 1.1f * dtx;
            startY = 1.1f * dty;

            //set up double buffering
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);
        }

        //This function gets called when the current simulated network sends an update event
        public void networkUpdated(ModularNetwork _net)
        {
            //net = _net;
            Refresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NetworkVisualizerForm
            // 
            this.ClientSize = new System.Drawing.Size(524, 435);
            this.Name = "NetworkVisualizerForm";
            this.Text = "?";
            this.Load += new System.EventHandler(this.NetworkVisualizerForm_Load);
            this.SizeChanged += new System.EventHandler(this.NetworkVisualizerForm_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.NetworkVisualizerForm_Paint);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NetworkVisualizerForm_KeyPress);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NetworkVisualizerForm_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NetworkVisualizerForm_MouseMove);
            this.ResumeLayout(false);

        }

        private void NetworkVisualizerForm_Paint(object sender, PaintEventArgs e)
        {

            if (net != null && net.genome.ConnectionGeneList != null)
            {

                g = e.Graphics;
                index = 0;
                foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
                {
                    //Check z values
                    if (gene.coordinates.Length > 4)
                    {
                        if (gene.coordinates[4] != selectedRobot.ZStack)        //Only valid if robot has z-values
                        {
                            index++;
                            continue;
                        }
                    }

                    penConnection.Color = Color.Black;
                    penConnection.Width = Math.Abs(net.connections[index].weight);

                    // penConnection.Width = 1;
                    if (net.connections[index].weight > 0.0f)
                    {
                        penConnection.Color = Color.Black;
                    }
                    else
                    {
                        penConnection.Color = Color.Red;
                    }

                    if (gene.coordinates.Length > 5 && gene.coordinates[5] != 0) 
                    {
                        if (drawHive)
                            penConnection.Color = Color.Blue;
                        else
                        {
                            index++;
                            continue;
                        }
                    }

                    g.DrawLine(penConnection, startX + (gene.coordinates[0]) * dtx, startY - gene.coordinates[1] * dty, startX + gene.coordinates[2] * dtx, startY - gene.coordinates[3] * dty);
                    index++;
                }

                index = 0;
                //Draw neurons
                foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
                {
                    if (gene.coordinates.Length > 4)
                    {
                        //Check z values
                        if (gene.coordinates[4] != selectedRobot.ZStack)
                        {
                            index++;
                            continue;
                        }

                        if (drawHive && gene.coordinates.Length > 5 && gene.coordinates[4] != gene.coordinates[5])
                        {
                            index++;
                            continue;
                        }
                    }
                    brush.Color = Color.Gray;
                    penConnection.Color = Color.Black;
                 
                    w = Math.Abs(net.neuronSignals[net.connections[index].targetNeuronIdx]) * 12.0f;
                    g.FillEllipse(brush, startX + gene.coordinates[2] * dtx - (w / 2.0f), startY - gene.coordinates[3] * dty - (w / 2.0f), w, w);
                   // g.DrawEllipse(penConnection, 150 + gene.coordinates[2] * dtx - w / 2.0f, 150 - gene.coordinates[3] * dty - w / 2.0f, w, w);
                  
                    w = Math.Abs(net.neuronSignals[net.connections[index].sourceNeuronIdx]) * 12.0f;
                    g.FillEllipse(brush, startX + gene.coordinates[0] * dtx - w / 2.0f, startY - gene.coordinates[1] * dty - w / 2.0f, w, w);
                  //  g.DrawEllipse(penConnection, 150 + gene.coordinates[0] * dtx - w / 2.0f, 150 - gene.coordinates[1] * dty - w / 2.0f, w, w);

                    index++;
                }
            }
        }

        private void NetworkVisualizerForm_SizeChanged(object sender, EventArgs e)
        {
            dtx = this.Width / 2.5f;
            dty = this.Height / 2.5f;
            startX = 1.1f * dtx;
            startY = 1.1f * dty;
            Refresh();
        }

        private void NetworkVisualizerForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (genomeVisualizerForm != null)
            {
                MouseEventArgs m = new MouseEventArgs(MouseButtons.Left, 0, 
                    (int)((float)(e.X) / (startX+dtx) * 200.0f), 
                    (int)((float)(e.Y) / (startY+dty) * 200.0f), 0);
               
                genomeVisualizerForm.NetworkVisualizerForm_MouseMove(sender, m);
            }
        }

        private void NetworkVisualizerForm_Load(object sender, EventArgs e)
        {

        }

        private void NetworkVisualizerForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (genomeVisualizerForm != null)
            {
                MouseEventArgs m = new MouseEventArgs(MouseButtons.Left, 0,
                    (int)((float)(e.X) / (startX + dtx) * 200.0f),
                    (int)((float)(e.Y) / (startY + dty) * 200.0f), 0);
                genomeVisualizerForm.pictureBox1_MouseClick(sender, m);
            }
        }

        private void NetworkVisualizerForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'H' || e.KeyChar == 'h')
            {
                drawHive = !drawHive;
            }
        }
    }
}
