using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;
using System.IO;

namespace MazeSimulator
{
    public partial class NetworkDifferencesForm : Form
    {
        private ModularNetwork net;
        private SolidBrush brush;
        private Pen penConnection;
        private List<Robot> robots;
        private Graphics g;
        float dtx = 60;
        float dty = 60;
        private int incX = 200;
        private int incY = 140;
        private bool saveAsEPS;
        private StreamWriter SW;
        private String epsStroke;

        public NetworkDifferencesForm(ModularNetwork _net, List<Robot> _robots)
        {

            InitializeComponent();
            net = _net;
            robots = _robots;
            saveAsEPS = false;
            this.Text = "Network Difference Viewer";
            SetBounds(1, 1, 640, 480);
            brush = new SolidBrush(Color.Red);
            penConnection = new Pen(Color.Black);
            panel1.Width = (int)(_robots.Count * incX)+100;
            panel1.Height = (int)(_robots.Count * incY)+100;
        }

        //private void NetworkDifferencesForm_Paint(object sender, PaintEventArgs experiment)
        //{
        //    int index, x, y;
        //    double r2weight = -999.0, weight;

        //    Image image;

        //    if (net != null && net.genome.ConnectionGeneList != null)
        //    {

        //        if (!saveAsEPS)
        //            g = experiment.Graphics;

        //        y = 100;
        //        foreach (robot_obj robot1 in robots)
        //        {
        //            x = 100;
        //            foreach (robot_obj robot2 in robots)
        //            {
        //                index = 0;
        //                foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
        //                {
        //                    //Check z values
        //                    if (gene.coordinates[4] != robot2.ZStack)
        //                    {
        //                        index++;
        //                        continue;
        //                    }

        //                    // bool connectionExits = false;
        //                    r2weight = 0.0f;
        //                    //Check if there is also a connection in the other robots brain
        //                    foreach (ConnectionGene robot2gene in net.genome.ConnectionGeneList)
        //                    {
        //                        if (robot2gene.coordinates[4] != robot1.ZStack)
        //                        {
        //                            continue;
        //                        }
        //                        if (robot2gene.coordinates[0] == gene.coordinates[0] && robot2gene.coordinates[1] == gene.coordinates[1]
        //                            && robot2gene.coordinates[2] == gene.coordinates[2] && robot2gene.coordinates[3] == gene.coordinates[3])
        //                        {
        //                            r2weight = robot2gene.Weight;
        //                            // connectionExits = true;
        //                            break;
        //                        }
        //                    }

        //                    //if (!connectionExits)
        //                    //{
        //                    //    Console.WriteLine("That shouldn'type happen");
        //                    //    index++;
        //                    //    continue;
        //                    //}

        //                    penConnection.Color = Color.Black;
        //                    if (robot1.ZStack == robot2.ZStack)
        //                    {
        //                        weight = net.connections[index].weight;
        //                    }
        //                    else
        //                    {
        //                        weight = (float)Math.Abs(net.connections[index].weight - r2weight);

        //                    }

        //                    penConnection.Width = (float)weight;

        //                    // penConnection.Width = 1;
        //                    if (weight > 0.0f)
        //                    {
        //                        penConnection.Color = Color.Black;
        //                        epsStroke="\"blue\"";
        //                    }
        //                    else
        //                    {
        //                        penConnection.Color = Color.Red;
        //                        epsStroke = "\"red\"";
        //                    }

        //                    penConnection.Width = Math.Abs(penConnection.Width);

        //                    if (penConnection.Width != 0)
        //                    {
        //                        if (saveAsEPS)
        //                        {
        //                            SW.WriteLine("<line x1=\"" + (x + gene.coordinates[0] * dtx) + "\" y1=\"" + (y - gene.coordinates[1] * dty) + "\" x2=\"" + (x + gene.coordinates[2] * dtx) + "\" y2=\"" + 
        //                                (y - gene.coordinates[3] * dty) + "\" stroke-width=\"" + penConnection.Width + 
        //                                "\"  stroke="+epsStroke+"/>");

        //                        }
        //                        else
        //                        {
        //                            g.DrawLine(penConnection, x + gene.coordinates[0] * dtx, y - gene.coordinates[1] * dty, x + gene.coordinates[2] * dtx, y - gene.coordinates[3] * dty);
        //                        }
        //                    }
        //                    index++;
        //                }

        //                if (!saveAsEPS)
        //                {
                            
        //                    g.DrawString("[" + robot2.ZStack + "|" + robot1.ZStack + "]", new Font("Verdana", 10),
        //                           new SolidBrush(Color.Black), x - 60, y - 75);
        //                }

        //                x += 200;
        //            }
        //            y += 140;
        //        }
        //    }

        //}

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SW = File.CreateText("net_differences.xhtml");
            saveAsEPS = true;
            SW.WriteLine("<?xml version=\"1.0\" standalone=\"no\"?>");
            SW.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            SW.WriteLine("<svg width=\"12cm\" height=\"12cm\" viewBox=\"0 0 1200 800\" xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">");
            SW.WriteLine("<desc>Network differences</desc>");
            panel1_Paint(null, null);
            //SW.WriteLine("<rect x=\"1\" y=\"1\" width=\"1198\" height=\"398\" fill=\"none\" stroke=\"blue\" stroke-width=\"2\"/>");
            //SW.WriteLine("<rect x=\"400\" y=\"100\" width=\"400\" height=\"200\" fill=\"yellow\" stroke=\"navy\" stroke-width=\"10\"/>");
            SW.WriteLine("</svg>");
            saveAsEPS = false;
            SW.Close();
        }

        private void NetworkDifferencesForm_Scroll(object sender, ScrollEventArgs e)
        {
            this.Invalidate(true);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int index, x, y;
            double r2weight = -999.0, weight;

            if (net != null && net.genome.ConnectionGeneList != null)
            {

                if (!saveAsEPS)
                    g = e.Graphics;

                y = 100;
                foreach (Robot robot1 in robots)
                {
                    x = 100;
                    foreach (Robot robot2 in robots)
                    {
                        index = 0;
                        foreach (ConnectionGene gene in net.genome.ConnectionGeneList)
                        {
                            //Check z values
                            if (gene.coordinates[4] != robot2.ZStack)
                            {
                                index++;
                                continue;
                            }

                            // bool connectionExits = false;
                            r2weight = 0.0f;
                            //Check if there is also a connection in the other robots brain
                            foreach (ConnectionGene robot2gene in net.genome.ConnectionGeneList)
                            {
                                if (robot2gene.coordinates[4] != robot1.ZStack)
                                {
                                    continue;
                                }
                                if (robot2gene.coordinates[0] == gene.coordinates[0] && robot2gene.coordinates[1] == gene.coordinates[1]
                                    && robot2gene.coordinates[2] == gene.coordinates[2] && robot2gene.coordinates[3] == gene.coordinates[3])
                                {
                                    r2weight = robot2gene.Weight;
                                    // connectionExits = true;
                                    break;
                                }
                            }

                            //if (!connectionExits)
                            //{
                            //    Console.WriteLine("That shouldn'type happen");
                            //    index++;
                            //    continue;
                            //}

                            penConnection.Color = Color.Black;
                            if (robot1.ZStack == robot2.ZStack)
                            {
                                weight = net.connections[index].weight;
                            }
                            else
                            {
                                weight = (float)Math.Abs(net.connections[index].weight - r2weight);

                            }

                            penConnection.Width = (float)weight;

                            // penConnection.Width = 1;
                            if (weight > 0.0f)
                            {
                                penConnection.Color = Color.Black;
                                epsStroke = "\"black\"";
                            }
                            else
                            {
                                penConnection.Color = Color.Red;
                                epsStroke = "\"red\"";
                            }

                            penConnection.Width = Math.Abs(penConnection.Width);

                            if (penConnection.Width > 0.01f)
                            {
                                if (saveAsEPS)
                                {
                                    SW.WriteLine("<line x1=\"" + (x + gene.coordinates[0] * dtx) + "\" y1=\"" + (y - gene.coordinates[1] * dty) + "\" x2=\"" + (x + gene.coordinates[2] * dtx) + "\" y2=\"" +
                                        (y - gene.coordinates[3] * dty) + "\" stroke-width=\"" + penConnection.Width +
                                        "\"  stroke=" + epsStroke + "/>");

                                }
                                else
                                {
                                    g.DrawLine(penConnection, x + gene.coordinates[0] * dtx, y - gene.coordinates[1] * dty, x + gene.coordinates[2] * dtx, y - gene.coordinates[3] * dty);
                                }
                            }
                            index++;
                        }

                        if (!saveAsEPS)
                        {

                            g.DrawString("[" + robot2.ZStack + "|" + robot1.ZStack + "]", new Font("Verdana", 10),
                                   new SolidBrush(Color.Black), x - 60, y - 75);
                        }

                        x += incX;
                    }
                    y += incY;
                }
            }
        }


    }
}
