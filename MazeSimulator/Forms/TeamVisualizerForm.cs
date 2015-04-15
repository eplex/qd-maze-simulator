using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;

namespace MazeSimulator
{
    public partial class TeamVisualizerForm : Form
    {
        ModularNetwork net;
        SolidBrush brush;
        Pen penConnection;
        float startX, startY;
        float dtx = 20;
        float dty = 100;
        float dtz = 500;
        float zdelta;
        bool drawHive = true;
        int index = 0;
        float w;
        Graphics g;

        public TeamVisualizerForm(ModularNetwork _net, int numAgents)
        {
            _net.UpdateNetworkEvent += networkUpdated;
            InitializeComponent();
            net = _net;
            this.Text = "Team";
            SetBounds(1, 1, 700, 400);
            brush = new SolidBrush(Color.Red);
            penConnection = new Pen(Color.Black);
            penConnection.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
            penConnection.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            startX = 700;
            startY = 150;
            zdelta = 1.0f / (numAgents - 1);

            //set up double buffering
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);
        }

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

                    penConnection.Color = Color.Black;
                    penConnection.Width = Math.Abs(net.connections[index].weight);
                    if (net.connections[index].weight == 0)
                    {
                        index++;
                        continue;
                    }

                    // penConnection.Width = 1;
                    if (net.connections[index].weight > 0.0f)
                    {
                        penConnection.Color = Color.Black;
                    }
                    else
                    {
                        penConnection.Color = Color.Red;
                    }

                    float z = gene.coordinates[5];
                    /*if (gene.coordinates[5] == 0)
                    {
                        z = gene.coordinates[4];
                    }
                    else if (gene.coordinates[5] == -1)
                    {
                        z -= zdelta * 2;
                    }
                    else
                    {
                        z += zdelta * 2;
                    }*/
                    g.DrawLine(penConnection, startX + (gene.coordinates[0]) * dtx + (gene.coordinates[4] * dtz), startY - gene.coordinates[1] * dty, startX + gene.coordinates[2] * dtx + (z * dtz), startY - gene.coordinates[3] * dty);
                    index++;
                }

                index = 0;
                //Draw neurons
                foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
                {
                    if (gene.coordinates.Length > 4)
                    {
                        //Check z values

                        if (!drawHive && gene.coordinates.Length > 5 && gene.coordinates[4] != gene.coordinates[5])
                        {
                            index++;
                            continue;
                        }
                    }
                    brush.Color = Color.Gray;
                    penConnection.Color = Color.Black;

                    w = Math.Abs(net.neuronSignals[net.connections[index].targetNeuronIdx]) * 12.0f;
                    float z = gene.coordinates[5];
                  /*  if (gene.coordinates[5] == 0)
                    {
                        z = gene.coordinates[4];
                    }
                    else if (gene.coordinates[5] == -1)
                    {
                        z -= zdelta * 2;
                    }
                    else
                    {
                        z += zdelta * 2;
                    }*/
                    g.FillEllipse(brush, startX + gene.coordinates[2] * dtx - (w / 2.0f) + (z * dtz), startY - gene.coordinates[3] * dty - (w / 2.0f), w, w);
                    // g.DrawEllipse(penConnection, 150 + gene.coordinates[2] * dtx - w / 2.0f, 150 - gene.coordinates[3] * dty - w / 2.0f, w, w);

                    w = Math.Abs(net.neuronSignals[net.connections[index].sourceNeuronIdx]) * 12.0f;
                    g.FillEllipse(brush, startX + gene.coordinates[0] * dtx - w / 2.0f + (gene.coordinates[4] * dtz), startY - gene.coordinates[1] * dty - w / 2.0f, w, w);
                    //  g.DrawEllipse(penConnection, 150 + gene.coordinates[0] * dtx - w / 2.0f, 150 - gene.coordinates[1] * dty - w / 2.0f, w, w);

                    index++;
                }
            }
        }

        private void NetworkVisualizerForm_SizeChanged(object sender, EventArgs e)
        {
            //dtx = this.Width / 2.5f;
            //dty = this.Height / 2.5f;
            //startX = 1.1f * dtx;
            //startY = 1.1f * dty;
            //Refresh();
        }

        private void NetworkVisualizerForm_Load(object sender, EventArgs e)
        {

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
