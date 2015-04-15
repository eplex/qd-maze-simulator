// sensor_info.cs created with MonoDevelop
// User: joel at 12:02 AM 6/18/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using SharpNeatLib;
using SharpNeatLib.AppConfig;
using SharpNeatLib.Evolution;
using SharpNeatLib.Evolution.Xml;
using SharpNeatLib.Experiments;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeuralNetwork.Xml;
using SharpNeatLib.CPPNs;

namespace MazeSimulator
{
	
	//form for getting/setting sensor configuration
	public class sensor_info : Form
	{
		//windows stuff
		private System.ComponentModel.IContainer components = null;
		public System.Windows.Forms.TextBox lower_range;
		public System.Windows.Forms.TextBox upper_range;
		public System.Windows.Forms.TextBox density;
		
		public System.Windows.Forms.Label lower_range_label;
		public System.Windows.Forms.Label upper_range_label;
		public System.Windows.Forms.Label density_label;
		
		public System.Windows.Forms.Button okbutton;
		public System.Windows.Forms.Button cancelbutton;
        private Label sensorNoiseLabel;
        public TrackBar sensorNoiseTrackBar;
        private Label sensorNoiseDataLabel;
        private Label label1;
        public TrackBar effectorNoiseTrackBar;
        public TrackBar headingNoiseTrackBar;
        private Label label2;
        private Label effectorNoiseDataLabel;
        private Label headingNoiseDataLabel;
		public bool good;
		
		//dunno copied this
		protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
		}

		public sensor_info() {
			good=true;
			InitializeComponent();
		}

        public void setTextValues()
        {
            sensorNoiseDataLabel.Text = sensorNoiseTrackBar.Value.ToString();
            effectorNoiseDataLabel.Text = effectorNoiseTrackBar.Value.ToString();
            headingNoiseDataLabel.Text = headingNoiseTrackBar.Value.ToString();
            Invalidate();
        }
		
		//boring gui stuff
		private void InitializeComponent()
        {
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.lower_range = new System.Windows.Forms.TextBox();
            this.upper_range = new System.Windows.Forms.TextBox();
            this.density = new System.Windows.Forms.TextBox();
            this.lower_range_label = new System.Windows.Forms.Label();
            this.upper_range_label = new System.Windows.Forms.Label();
            this.density_label = new System.Windows.Forms.Label();
            this.sensorNoiseLabel = new System.Windows.Forms.Label();
            this.sensorNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.sensorNoiseDataLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.effectorNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.headingNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.effectorNoiseDataLabel = new System.Windows.Forms.Label();
            this.headingNoiseDataLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.sensorNoiseTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.effectorNoiseTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.headingNoiseTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // okbutton
            // 
            this.okbutton.Location = new System.Drawing.Point(8, 196);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 0;
            this.okbutton.Text = "Ok";
            this.okbutton.Click += new System.EventHandler(this.okClick);
            // 
            // cancelbutton
            // 
            this.cancelbutton.Location = new System.Drawing.Point(111, 196);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(75, 23);
            this.cancelbutton.TabIndex = 1;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.Click += new System.EventHandler(this.cancelClick);
            // 
            // lower_range
            // 
            this.lower_range.Location = new System.Drawing.Point(0, 0);
            this.lower_range.Name = "lower_range";
            this.lower_range.Size = new System.Drawing.Size(100, 20);
            this.lower_range.TabIndex = 0;
            // 
            // upper_range
            // 
            this.upper_range.Location = new System.Drawing.Point(0, 0);
            this.upper_range.Name = "upper_range";
            this.upper_range.Size = new System.Drawing.Size(100, 20);
            this.upper_range.TabIndex = 0;
            // 
            // density
            // 
            this.density.Location = new System.Drawing.Point(111, 167);
            this.density.Name = "density";
            this.density.Size = new System.Drawing.Size(74, 20);
            this.density.TabIndex = 4;
            this.density.Text = "5";
            // 
            // lower_range_label
            // 
            this.lower_range_label.Location = new System.Drawing.Point(0, 0);
            this.lower_range_label.Name = "lower_range_label";
            this.lower_range_label.Size = new System.Drawing.Size(100, 23);
            this.lower_range_label.TabIndex = 0;
            // 
            // upper_range_label
            // 
            this.upper_range_label.Location = new System.Drawing.Point(0, 0);
            this.upper_range_label.Name = "upper_range_label";
            this.upper_range_label.Size = new System.Drawing.Size(100, 23);
            this.upper_range_label.TabIndex = 0;
            // 
            // density_label
            // 
            this.density_label.Location = new System.Drawing.Point(5, 167);
            this.density_label.Name = "density_label";
            this.density_label.Size = new System.Drawing.Size(100, 37);
            this.density_label.TabIndex = 7;
            this.density_label.Text = "Number Sensors:";
            this.density_label.Click += new System.EventHandler(this.density_label_Click);
            // 
            // sensorNoiseLabel
            // 
            this.sensorNoiseLabel.AutoSize = true;
            this.sensorNoiseLabel.Location = new System.Drawing.Point(2, 7);
            this.sensorNoiseLabel.Name = "sensorNoiseLabel";
            this.sensorNoiseLabel.Size = new System.Drawing.Size(70, 13);
            this.sensorNoiseLabel.TabIndex = 8;
            this.sensorNoiseLabel.Text = "Sensor Noise";
            // 
            // sensorNoiseTrackBar
            // 
            this.sensorNoiseTrackBar.Location = new System.Drawing.Point(5, 20);
            this.sensorNoiseTrackBar.Maximum = 100;
            this.sensorNoiseTrackBar.Name = "sensorNoiseTrackBar";
            this.sensorNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.sensorNoiseTrackBar.TabIndex = 9;
            this.sensorNoiseTrackBar.Scroll += new System.EventHandler(this.sensorNoiseTrackBar_Scroll);
            // 
            // sensorNoiseDataLabel
            // 
            this.sensorNoiseDataLabel.AutoSize = true;
            this.sensorNoiseDataLabel.Location = new System.Drawing.Point(2, 28);
            this.sensorNoiseDataLabel.Name = "sensorNoiseDataLabel";
            this.sensorNoiseDataLabel.Size = new System.Drawing.Size(0, 13);
            this.sensorNoiseDataLabel.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Effector Noise";
            // 
            // effectorNoiseTrackBar
            // 
            this.effectorNoiseTrackBar.Location = new System.Drawing.Point(5, 71);
            this.effectorNoiseTrackBar.Maximum = 100;
            this.effectorNoiseTrackBar.Name = "effectorNoiseTrackBar";
            this.effectorNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.effectorNoiseTrackBar.TabIndex = 12;
            this.effectorNoiseTrackBar.Scroll += new System.EventHandler(this.effectorNoiseTrackBar_Scroll);
            // 
            // headingNoiseTrackBar
            // 
            this.headingNoiseTrackBar.Location = new System.Drawing.Point(5, 119);
            this.headingNoiseTrackBar.Maximum = 100;
            this.headingNoiseTrackBar.Name = "headingNoiseTrackBar";
            this.headingNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.headingNoiseTrackBar.TabIndex = 13;
            this.headingNoiseTrackBar.Scroll += new System.EventHandler(this.headingNoiseTrackBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Heading Noise";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // effectorNoiseDataLabel
            // 
            this.effectorNoiseDataLabel.AutoSize = true;
            this.effectorNoiseDataLabel.Location = new System.Drawing.Point(2, 77);
            this.effectorNoiseDataLabel.Name = "effectorNoiseDataLabel";
            this.effectorNoiseDataLabel.Size = new System.Drawing.Size(0, 13);
            this.effectorNoiseDataLabel.TabIndex = 15;
            // 
            // headingNoiseDataLabel
            // 
            this.headingNoiseDataLabel.AutoSize = true;
            this.headingNoiseDataLabel.Location = new System.Drawing.Point(2, 127);
            this.headingNoiseDataLabel.Name = "headingNoiseDataLabel";
            this.headingNoiseDataLabel.Size = new System.Drawing.Size(0, 13);
            this.headingNoiseDataLabel.TabIndex = 16;
            // 
            // sensor_info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 231);
            this.Controls.Add(this.headingNoiseDataLabel);
            this.Controls.Add(this.effectorNoiseDataLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.headingNoiseTrackBar);
            this.Controls.Add(this.effectorNoiseTrackBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sensorNoiseDataLabel);
            this.Controls.Add(this.sensorNoiseTrackBar);
            this.Controls.Add(this.sensorNoiseLabel);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.density);
            this.Controls.Add(this.density_label);
            this.Name = "sensor_info";
            this.Text = "Sensor Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.sensorNoiseTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.effectorNoiseTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.headingNoiseTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		
		private void okClick(object sender, EventArgs e)
		{
			good=true;
			this.Close();
		}
		private void cancelClick(object sender, EventArgs e)
		{
			good=false;
			this.Close();
		}

        private void sensorNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void effectorNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void headingNoiseTrackBar_Scroll(object sender, EventArgs e)
        {
            setTextValues();
        }

        private void density_label_Click(object sender, EventArgs e)
        {

        }
	}
	
}
