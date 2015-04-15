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
    public partial class CommunicationVisualizer : Form
    {
        ModularNetwork net;
        SolidBrush brush;
        Pen penConnection;
        Pen penRed;
        float startX, startY;
        float dtx = 20;
        float dty = 100;
        float dtz = 500;
        float zdelta;
        bool drawHive = true;
        int index = 0;
        float w;
        Graphics g;
        //List<float>[] activation;
        public class activationLevels
        {
            public List<float> values;
            public int ypos;
            public int xpos;
            public float z;
            public int neuronID;

            //For info purposes only
            public int number;//index in group
            public int groupID;
            public activationLevels()
            {
                values = new List<float>();
            }
        }

        activationLevels[] activation; //For each neuron that should be observed


      //  List<float>[] incommingActivation;
      //  List<float>[] outgoingActivation;

        float zlevel;
        List<uint> notAdded = new List<uint>();
        //List<uint> notAddedOutgoing = new List<uint>();

        System.Drawing.Font drawFont;
        System.Drawing.SolidBrush drawBrush;
        System.Drawing.StringFormat drawFormat;

        SharpNeatLib.CPPNs.SubstrateDescription sd;

        public CommunicationVisualizer(SharpNeatLib.CPPNs.SubstrateDescription _sd, ModularNetwork _net)
        {
            InitializeComponent();
            //zlevel = _zlevel;

            drawFont = new System.Drawing.Font("Arial", 8);
            drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

             drawFormat = new System.Drawing.StringFormat();

            sd = _sd;

            net = _net;
            _net.UpdateNetworkEvent += networkUpdated;
            activation = new activationLevels[200];
          //  outgoingActivation = new List<float>[200];

            for (int i = 0; i < activation.Length; i++)
            {
                activation[i] = new activationLevels();
                //outgoingActivation[i] = new List<float>();
                    

            }
            penConnection = new Pen(Color.Black);
            penRed = new Pen(Color.Red);

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

        private void CommunicationVisualizer_Paint(object sender, PaintEventArgs e)
        {
           // experiment.Graphics.DrawLine(penConnection, 1, 1, 100, 100);
                //          
            //return;
            if (net != null && net.genome.ConnectionGeneList != null)
            {

                notAdded.Clear();
               // notAddedOutgoing.Clear();

                g = e.Graphics;// this.pictureBox1.CreateGraphics();
                //g = experiment.Graphics;
                index = 0;
               // net.genome.NeuronGeneList.
                foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
                {
                    //Check z values
                    //Console.WriteLine(gene.coordinates[0]);

                 //   if (!gene.hive) continue;

                    //z1 gene.coordinates[4] 
                    //z2 gene.coordinates[5]

                  //TODO what if sending and receiving at the same time?

                    //float z = gene.coordinates[4];
                    int y=-1;

                  //  sd.visualizeNodes[0].x = 0;

                    foreach (SharpNeatLib.CPPNs.SubstrateDescription.vis_node vn in sd.visualizeNodes)
                    {
                      //  Console.WriteLine(vn.pos.Y + " " + vn.pos.X);
                        y = vn.ypos;

                        if (vn.pos.X == gene.coordinates[0] && vn.pos.Y == gene.coordinates[1] && gene.coordinates[4] == vn.zfilter)
                        {
                            
                            if (!notAdded.Contains(vn.id)) //incommingActivation[count].length < ?)
                            {
                                // Console.WriteLine(gene.TargetNeuronId);

                              //  activation[gene.SourceNeuronId].ypos = y;

                                activation[vn.id].ypos = vn.ypos;//(int)(z * 50);
                                activation[vn.id].xpos = vn.xpos;
                                activation[vn.id].z = vn.zfilter;
                                activation[vn.id].groupID = vn.groupID;
                                activation[vn.id].number = vn.number;
                                activation[vn.id].neuronID = (int)gene.SourceNeuronId;

                                activation[vn.id].values.Add(net.neuronSignals[gene.SourceNeuronId]);

                                notAdded.Add(vn.id);
                            }
                        }
                        else
                        {
                            //Outgoing activation
                            if (vn.pos.X == gene.coordinates[2] && vn.pos.Y == gene.coordinates[3] && gene.coordinates[5] == vn.zfilter)
                            {
                                if (!notAdded.Contains(vn.id)) //incommingActivation[count].length < ?)
                                {
                                    // Console.WriteLine(gene.TargetNeuronId);

                              // //     activation[gene.TargetNeuronId].ypos = vn.ypos;// (int)(z * 50);
                                   // activation[vn.ID].xpos = vn.xpos;
                                    activation[vn.id].ypos = vn.ypos;//(int)(z * 50);
                                    activation[vn.id].xpos = vn.xpos;
                                    activation[vn.id].z = vn.zfilter;
                                    activation[vn.id].groupID = vn.groupID;
                                    activation[vn.id].number = vn.number;
                                    activation[vn.id].neuronID = (int) gene.TargetNeuronId;

                                  //  Console.WriteLine(vn.ID+ " "+activation[vn.ID].groupID +
                                 //       " " + activation[vn.ID].number + " " + gene.TargetNeuronId);
                                      activation[vn.id].values.Add(net.neuronSignals[gene.TargetNeuronId]);

                                //    activation[gene.TargetNeuronId].values.Add(net.neuronSignals[gene.TargetNeuronId]);
                                    notAdded.Add(vn.id);//gene.TargetNeuronId);
                                }
                            }
                        }
                    }

                  

                  

                  //  if (gene.coordinates[5] == zlevel) //part of this robots incomming neurons?
                 //   {

                      


                    //}
                    //else  //Outgoing activation
                    //    if (gene.coordinates[4] == zlevel && (gene.coordinates[4] != gene.coordinates[5]))
                    //    {
                    //        if (!notAddedOutgoing.Contains(gene.SourceNeuronId)) //incommingActivation[count].length < ?)
                    //        {
                    ////            Console.WriteLine(gene.SourceNeuronId);

                    //            outgoingActivation[gene.SourceNeuronId].Add(net.neuronSignals[gene.SourceNeuronId]);
                    //            notAddedOutgoing.Add(gene.SourceNeuronId);
                    //        }
                    //    }
                    //    else
                    //        continue;
                  //  Console.WriteLine();
                    // float z = gene.coordinates[5];

                }
     

                float dy = 40;
                //return;
                for (int numHidden = 0; numHidden < 200; numHidden++)
                {
                    if (activation[numHidden].values.Count > 1)
                    {

                        g.DrawLine(penConnection, activation[numHidden].xpos, activation[numHidden].ypos,
                            activation[numHidden].xpos + 400, activation[numHidden].ypos);

                        g.DrawRectangle(penConnection, activation[numHidden].xpos, activation[numHidden].ypos - dy, 400, dy * 2);
                      //  Console.WriteLine(activation[numHidden].z);
                       
                        //String.Format("{0:0.00}" + activation[numHidden].z)
                        g.DrawString("z=" + Convert.ToString(activation[numHidden].z)+" groupID=" + activation[numHidden].groupID + " num=" + activation[numHidden].number+" ID="+activation[numHidden].neuronID, drawFont, drawBrush, activation[numHidden].xpos, activation[numHidden].ypos-dy-15, drawFormat);


                        hScrollBar1.Maximum = activation[numHidden].values.Count;
                        if (checkBoxAutoScroll.Checked)
                            if (hScrollBar1.Maximum>200) hScrollBar1.Value = hScrollBar1.Maximum-200;
                        int count = 0;
                        for (int i = hScrollBar1.Value; i < hScrollBar1.Value+200; i++)
                        {
                            if (i> 0 && i < activation[numHidden].values.Count)
                            {

                                g.DrawLine(penRed, activation[numHidden].xpos + count * 2, activation[numHidden].ypos- (activation[numHidden]).values[i - 1] * dy ,
                                    activation[numHidden].xpos + ((count + 1) * 2), activation[numHidden].ypos-activation[numHidden].values[i] * dy);
                            }
                            count++;
                        }

                        //if (activation[numHidden].values.Count > 250) activation[numHidden].values.RemoveAt(0);
                   
                    }
                }
                //count = 0;

                //for (int numHidden = 0; numHidden < 200; numHidden++)
                //{
                //    if (outgoingActivation[numHidden].Count > 1)
                //    {

                //        g.DrawLine(penConnection, 550, 1 * 40 + 50 + count * 70,
                //            1050, 1 * 40 + 50 + count * 70);


                //        for (int i = 1; i < outgoingActivation[numHidden].Count; i++)
                //            g.DrawLine(penRed, i*2+550, (outgoingActivation[numHidden])[i - 1] * 40 + 50 + count * 70,
                //                ((i + 1)*2)+550, outgoingActivation[numHidden][i] * 40 + 50 + count * 70);

                //        if (outgoingActivation[numHidden].Count > 250) outgoingActivation[numHidden].RemoveAt(0);
                //        count++;
                //    }
                //}
            }
        }

        private void refreshScreen(object sender, ScrollEventArgs e)
        {
            this.Refresh();
        }
    }
}
