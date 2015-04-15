// agent_info.cs created with MonoDevelop
// User: joel at 11:33 PM 6/17/2009
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
	
	
//form for getting agent information (experiment.g. number of agents, spacing, orientation, etc.)
	public class agent_info : Form
	{
		//windows stuff
		private System.ComponentModel.IContainer components = null;
		public System.Windows.Forms.TextBox number_of_agents;
		public System.Windows.Forms.Label number_of_agents_label;
		
		public System.Windows.Forms.TextBox orientation;
		public System.Windows.Forms.Label orientation_label;

		public System.Windows.Forms.TextBox spacing;
		public System.Windows.Forms.Label spacing_label;

		public System.Windows.Forms.TextBox heading;
        public System.Windows.Forms.Label heading_label;		

		public System.Windows.Forms.TextBox substrate;
		public System.Windows.Forms.Label substrate_label;
		
		public System.Windows.Forms.Button okbutton;
		public System.Windows.Forms.Button cancelbutton;
        public CheckBox visibleCheckbox;
        public CheckBox collideCheckbox;
        public CheckBox homogeneousCheckbox;
        public CheckBox normalizeWeightsCheckBox1;
        public CheckBox adaptiveNetworkCheckBox;
        public CheckBox modulationCheckBox;
        private Label label1;
        public TextBox populationSizeTextBox;
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
		public agent_info() {
			good=true;
			InitializeComponent();
		}
		
		//boring gui stuff
		private void InitializeComponent()
        {
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.number_of_agents = new System.Windows.Forms.TextBox();
            this.orientation = new System.Windows.Forms.TextBox();
            this.spacing = new System.Windows.Forms.TextBox();
            this.heading = new System.Windows.Forms.TextBox();
            this.substrate = new System.Windows.Forms.TextBox();
            this.number_of_agents_label = new System.Windows.Forms.Label();
            this.orientation_label = new System.Windows.Forms.Label();
            this.spacing_label = new System.Windows.Forms.Label();
            this.heading_label = new System.Windows.Forms.Label();
            this.substrate_label = new System.Windows.Forms.Label();
            this.visibleCheckbox = new System.Windows.Forms.CheckBox();
            this.collideCheckbox = new System.Windows.Forms.CheckBox();
            this.homogeneousCheckbox = new System.Windows.Forms.CheckBox();
            this.normalizeWeightsCheckBox1 = new System.Windows.Forms.CheckBox();
            this.adaptiveNetworkCheckBox = new System.Windows.Forms.CheckBox();
            this.modulationCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.populationSizeTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // okbutton
            // 
            this.okbutton.Location = new System.Drawing.Point(6, 215);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 0;
            this.okbutton.Text = "Ok";
            this.okbutton.Click += new System.EventHandler(this.okClick);
            // 
            // cancelbutton
            // 
            this.cancelbutton.Location = new System.Drawing.Point(150, 215);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(75, 23);
            this.cancelbutton.TabIndex = 1;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.Click += new System.EventHandler(this.cancelClick);
            // 
            // number_of_agents
            // 
            this.number_of_agents.Location = new System.Drawing.Point(6, 23);
            this.number_of_agents.Name = "number_of_agents";
            this.number_of_agents.Size = new System.Drawing.Size(100, 20);
            this.number_of_agents.TabIndex = 3;
            this.number_of_agents.Text = "5";
            this.number_of_agents.TextChanged += new System.EventHandler(this.number_of_agents_TextChanged);
            // 
            // orientation
            // 
            this.orientation.Location = new System.Drawing.Point(6, 87);
            this.orientation.Name = "orientation";
            this.orientation.Size = new System.Drawing.Size(100, 20);
            this.orientation.TabIndex = 2;
            this.orientation.Text = "0";
            this.orientation.TextChanged += new System.EventHandler(this.orientation_TextChanged);
            // 
            // spacing
            // 
            this.spacing.Location = new System.Drawing.Point(6, 177);
            this.spacing.Name = "spacing";
            this.spacing.Size = new System.Drawing.Size(100, 20);
            this.spacing.TabIndex = 4;
            this.spacing.Text = "30";
            this.spacing.TextChanged += new System.EventHandler(this.spacing_TextChanged);
            // 
            // Heading
            // 
            this.heading.Location = new System.Drawing.Point(150, 87);
            this.heading.Name = "Heading";
            this.heading.Size = new System.Drawing.Size(100, 20);
            this.heading.TabIndex = 5;
            // 
            // substrate
            // 
            this.substrate.Location = new System.Drawing.Point(150, 177);
            this.substrate.Name = "substrate";
            this.substrate.Size = new System.Drawing.Size(100, 20);
            this.substrate.TabIndex = 7;
            // 
            // number_of_agents_label
            // 
            this.number_of_agents_label.Location = new System.Drawing.Point(6, 0);
            this.number_of_agents_label.Name = "number_of_agents_label";
            this.number_of_agents_label.Size = new System.Drawing.Size(100, 30);
            this.number_of_agents_label.TabIndex = 9;
            this.number_of_agents_label.Text = "Number of agents";
            this.number_of_agents_label.Click += new System.EventHandler(this.number_of_agents_label_Click);
            // 
            // orientation_label
            // 
            this.orientation_label.Location = new System.Drawing.Point(6, 60);
            this.orientation_label.Name = "orientation_label";
            this.orientation_label.Size = new System.Drawing.Size(115, 37);
            this.orientation_label.TabIndex = 8;
            this.orientation_label.Text = "Group Orientation";
            this.orientation_label.Click += new System.EventHandler(this.orientation_label_Click);
            // 
            // spacing_label
            // 
            this.spacing_label.Location = new System.Drawing.Point(6, 137);
            this.spacing_label.Name = "spacing_label";
            this.spacing_label.Size = new System.Drawing.Size(115, 37);
            this.spacing_label.TabIndex = 10;
            this.spacing_label.Text = "Spacing between agents";
            this.spacing_label.Click += new System.EventHandler(this.spacing_label_Click);
            // 
            // heading_label
            // 
            this.heading_label.Location = new System.Drawing.Point(150, 60);
            this.heading_label.Name = "heading_label";
            this.heading_label.Size = new System.Drawing.Size(100, 30);
            this.heading_label.TabIndex = 11;
            this.heading_label.Text = "Heading in degrees";
            // 
            // substrate_label
            // 
            this.substrate_label.Location = new System.Drawing.Point(150, 137);
            this.substrate_label.Name = "substrate_label";
            this.substrate_label.Size = new System.Drawing.Size(122, 37);
            this.substrate_label.TabIndex = 13;
            this.substrate_label.Text = "Substrate Filename";
            this.substrate_label.Click += new System.EventHandler(this.substrate_label_Click);
            // 
            // visibleCheckbox
            // 
            this.visibleCheckbox.AutoSize = true;
            this.visibleCheckbox.Checked = true;
            this.visibleCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.visibleCheckbox.Location = new System.Drawing.Point(267, 60);
            this.visibleCheckbox.Name = "visibleCheckbox";
            this.visibleCheckbox.Size = new System.Drawing.Size(161, 17);
            this.visibleCheckbox.TabIndex = 14;
            this.visibleCheckbox.Text = "Agents Visible to Each Other";
            this.visibleCheckbox.UseVisualStyleBackColor = true;
            this.visibleCheckbox.CheckedChanged += new System.EventHandler(this.visibleCheckbox_CheckedChanged);
            // 
            // collideCheckbox
            // 
            this.collideCheckbox.AutoSize = true;
            this.collideCheckbox.Checked = true;
            this.collideCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.collideCheckbox.Location = new System.Drawing.Point(267, 36);
            this.collideCheckbox.Name = "collideCheckbox";
            this.collideCheckbox.Size = new System.Drawing.Size(172, 17);
            this.collideCheckbox.TabIndex = 15;
            this.collideCheckbox.Text = "Agents Collide with Each Other";
            this.collideCheckbox.UseVisualStyleBackColor = true;
            this.collideCheckbox.CheckedChanged += new System.EventHandler(this.collideCheckbox_CheckedChanged);
            // 
            // homogeneousCheckbox
            // 
            this.homogeneousCheckbox.AutoSize = true;
            this.homogeneousCheckbox.Location = new System.Drawing.Point(267, 13);
            this.homogeneousCheckbox.Name = "homogeneousCheckbox";
            this.homogeneousCheckbox.Size = new System.Drawing.Size(131, 17);
            this.homogeneousCheckbox.TabIndex = 16;
            this.homogeneousCheckbox.Text = "Agents Homogeneous";
            this.homogeneousCheckbox.UseVisualStyleBackColor = true;
            this.homogeneousCheckbox.CheckedChanged += new System.EventHandler(this.homogeneousCheckbox_CheckedChanged);
            // 
            // normalizeWeightsCheckBox1
            // 
            this.normalizeWeightsCheckBox1.AutoSize = true;
            this.normalizeWeightsCheckBox1.Checked = true;
            this.normalizeWeightsCheckBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.normalizeWeightsCheckBox1.Location = new System.Drawing.Point(267, 84);
            this.normalizeWeightsCheckBox1.Name = "normalizeWeightsCheckBox1";
            this.normalizeWeightsCheckBox1.Size = new System.Drawing.Size(114, 17);
            this.normalizeWeightsCheckBox1.TabIndex = 17;
            this.normalizeWeightsCheckBox1.Text = "Normalize Weights";
            this.normalizeWeightsCheckBox1.UseVisualStyleBackColor = true;
            this.normalizeWeightsCheckBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // adaptiveNetworkCheckBox
            // 
            this.adaptiveNetworkCheckBox.AutoSize = true;
            this.adaptiveNetworkCheckBox.Location = new System.Drawing.Point(267, 108);
            this.adaptiveNetworkCheckBox.Name = "adaptiveNetworkCheckBox";
            this.adaptiveNetworkCheckBox.Size = new System.Drawing.Size(77, 17);
            this.adaptiveNetworkCheckBox.TabIndex = 18;
            this.adaptiveNetworkCheckBox.Text = "Adaptation";
            this.adaptiveNetworkCheckBox.UseVisualStyleBackColor = true;
            this.adaptiveNetworkCheckBox.CheckedChanged += new System.EventHandler(this.adaptiveNetworkCheckBox_CheckedChanged);
            // 
            // modulationCheckBox
            // 
            this.modulationCheckBox.AutoSize = true;
            this.modulationCheckBox.Location = new System.Drawing.Point(351, 108);
            this.modulationCheckBox.Name = "modulationCheckBox";
            this.modulationCheckBox.Size = new System.Drawing.Size(78, 17);
            this.modulationCheckBox.TabIndex = 19;
            this.modulationCheckBox.Text = "Modulation";
            this.modulationCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Population Size";
            // 
            // populationSizeTextBox
            // 
            this.populationSizeTextBox.Location = new System.Drawing.Point(150, 23);
            this.populationSizeTextBox.Name = "populationSizeTextBox";
            this.populationSizeTextBox.Size = new System.Drawing.Size(100, 20);
            this.populationSizeTextBox.TabIndex = 21;
            // 
            // agent_info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 303);
            this.Controls.Add(this.populationSizeTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.modulationCheckBox);
            this.Controls.Add(this.adaptiveNetworkCheckBox);
            this.Controls.Add(this.normalizeWeightsCheckBox1);
            this.Controls.Add(this.homogeneousCheckbox);
            this.Controls.Add(this.collideCheckbox);
            this.Controls.Add(this.visibleCheckbox);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.orientation);
            this.Controls.Add(this.number_of_agents);
            this.Controls.Add(this.spacing);
            this.Controls.Add(this.heading);
            this.Controls.Add(this.substrate);
            this.Controls.Add(this.orientation_label);
            this.Controls.Add(this.number_of_agents_label);
            this.Controls.Add(this.spacing_label);
            this.Controls.Add(this.heading_label);
            this.Controls.Add(this.substrate_label);
            this.Name = "agent_info";
            this.Text = "Agent Settings";
            this.Load += new System.EventHandler(this.agent_info_Load);
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

        private void collideCheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void homogeneousCheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void visibleCheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void adaptiveNetworkCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void size_TextChanged(object sender, EventArgs e)
        {

        }

        private void agent_info_Load(object sender, EventArgs e)
        {

        }

        private void spacing_TextChanged(object sender, EventArgs e)
        {

        }

        private void spacing_label_Click(object sender, EventArgs e)
        {

        }

        private void orientation_TextChanged(object sender, EventArgs e)
        {

        }

        private void orientation_label_Click(object sender, EventArgs e)
        {

        }

        private void number_of_agents_TextChanged(object sender, EventArgs e)
        {

        }

        private void number_of_agents_label_Click(object sender, EventArgs e)
        {

        }

        private void substrate_label_Click(object sender, EventArgs e)
        {

        }
		
	}
}