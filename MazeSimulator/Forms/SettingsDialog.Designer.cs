namespace MazeSimulator
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.populationSizeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.substrate = new System.Windows.Forms.TextBox();
            this.substrate_label = new System.Windows.Forms.Label();
            this.lower_range = new System.Windows.Forms.TextBox();
            this.upper_range = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.robotModelComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.heading = new System.Windows.Forms.TextBox();
            this.heading_label = new System.Windows.Forms.Label();
            this.homogeneousCheckbox = new System.Windows.Forms.CheckBox();
            this.collideCheckbox = new System.Windows.Forms.CheckBox();
            this.visibleCheckbox = new System.Windows.Forms.CheckBox();
            this.orientation = new System.Windows.Forms.TextBox();
            this.number_of_agents = new System.Windows.Forms.TextBox();
            this.spacing = new System.Windows.Forms.TextBox();
            this.orientation_label = new System.Windows.Forms.Label();
            this.number_of_agents_label = new System.Windows.Forms.Label();
            this.spacing_label = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.multiBrainCheckBox = new System.Windows.Forms.CheckBox();
            this.EScheckBox = new System.Windows.Forms.CheckBox();
            this.modulationCheckBox = new System.Windows.Forms.CheckBox();
            this.adaptiveNetworkCheckBox = new System.Windows.Forms.CheckBox();
            this.normalizeWeightsCheckBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.headingNoiseDataLabel = new System.Windows.Forms.Label();
            this.effectorNoiseDataLabel = new System.Windows.Forms.Label();
            this.sensorNoiseDataLabel = new System.Windows.Forms.Label();
            this.density_label = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.headingNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.effectorNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.sensorNoiseTrackBar = new System.Windows.Forms.TrackBar();
            this.sensorNoiseLabel = new System.Windows.Forms.Label();
            this.density = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.NoveltySearchCheckBox = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.functionComboBox = new System.Windows.Forms.ComboBox();
            this.behaviorTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.behaviorComboBox = new System.Windows.Forms.ComboBox();
            this.multiObjectiveCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headingNoiseTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.effectorNoiseTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sensorNoiseTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // populationSizeTextBox
            // 
            this.populationSizeTextBox.Location = new System.Drawing.Point(390, 218);
            this.populationSizeTextBox.Name = "populationSizeTextBox";
            this.populationSizeTextBox.Size = new System.Drawing.Size(170, 20);
            this.populationSizeTextBox.TabIndex = 41;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(284, 218);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Population Size";
            // 
            // okbutton
            // 
            this.okbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okbutton.Location = new System.Drawing.Point(12, 283);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 22;
            this.okbutton.Text = "Ok";
            this.okbutton.Click += new System.EventHandler(this.okbutton_Click);
            // 
            // cancelbutton
            // 
            this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelbutton.Location = new System.Drawing.Point(118, 283);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(75, 23);
            this.cancelbutton.TabIndex = 23;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.Click += new System.EventHandler(this.cancelbutton_Click);
            // 
            // substrate
            // 
            this.substrate.Location = new System.Drawing.Point(390, 254);
            this.substrate.Name = "substrate";
            this.substrate.Size = new System.Drawing.Size(170, 20);
            this.substrate.TabIndex = 28;
            // 
            // substrate_label
            // 
            this.substrate_label.Location = new System.Drawing.Point(284, 254);
            this.substrate_label.Name = "substrate_label";
            this.substrate_label.Size = new System.Drawing.Size(122, 37);
            this.substrate_label.TabIndex = 33;
            this.substrate_label.Text = "Substrate Filename";
            // 
            // lower_range
            // 
            this.lower_range.Location = new System.Drawing.Point(114, 414);
            this.lower_range.Name = "lower_range";
            this.lower_range.Size = new System.Drawing.Size(100, 20);
            this.lower_range.TabIndex = 44;
            // 
            // upper_range
            // 
            this.upper_range.Location = new System.Drawing.Point(272, 391);
            this.upper_range.Name = "upper_range";
            this.upper_range.Size = new System.Drawing.Size(100, 20);
            this.upper_range.TabIndex = 42;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.robotModelComboBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.heading);
            this.groupBox1.Controls.Add(this.heading_label);
            this.groupBox1.Controls.Add(this.homogeneousCheckbox);
            this.groupBox1.Controls.Add(this.collideCheckbox);
            this.groupBox1.Controls.Add(this.visibleCheckbox);
            this.groupBox1.Controls.Add(this.orientation);
            this.groupBox1.Controls.Add(this.number_of_agents);
            this.groupBox1.Controls.Add(this.spacing);
            this.groupBox1.Controls.Add(this.orientation_label);
            this.groupBox1.Controls.Add(this.number_of_agents_label);
            this.groupBox1.Controls.Add(this.spacing_label);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(244, 256);
            this.groupBox1.TabIndex = 59;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Agent Settings";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // robotModelComboBox
            // 
            this.robotModelComboBox.FormattingEnabled = true;
            this.robotModelComboBox.Location = new System.Drawing.Point(106, 157);
            this.robotModelComboBox.Name = "robotModelComboBox";
            this.robotModelComboBox.Size = new System.Drawing.Size(132, 21);
            this.robotModelComboBox.TabIndex = 44;
            this.robotModelComboBox.SelectedIndexChanged += new System.EventHandler(this.functionComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 13);
            this.label4.TabIndex = 43;
            this.label4.Text = "Robot Model Name";
            // 
            // Heading
            // 
            this.heading.Location = new System.Drawing.Point(106, 127);
            this.heading.Name = "Heading";
            this.heading.Size = new System.Drawing.Size(44, 20);
            this.heading.TabIndex = 41;
            // 
            // heading_label
            // 
            this.heading_label.Location = new System.Drawing.Point(6, 128);
            this.heading_label.Name = "heading_label";
            this.heading_label.Size = new System.Drawing.Size(100, 30);
            this.heading_label.TabIndex = 42;
            this.heading_label.Text = "Heading in degrees";
            // 
            // homogeneousCheckbox
            // 
            this.homogeneousCheckbox.AutoSize = true;
            this.homogeneousCheckbox.Location = new System.Drawing.Point(9, 182);
            this.homogeneousCheckbox.Name = "homogeneousCheckbox";
            this.homogeneousCheckbox.Size = new System.Drawing.Size(131, 17);
            this.homogeneousCheckbox.TabIndex = 40;
            this.homogeneousCheckbox.Text = "Agents Homogeneous";
            this.homogeneousCheckbox.UseVisualStyleBackColor = true;
            // 
            // collideCheckbox
            // 
            this.collideCheckbox.AutoSize = true;
            this.collideCheckbox.Checked = true;
            this.collideCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.collideCheckbox.Location = new System.Drawing.Point(9, 203);
            this.collideCheckbox.Name = "collideCheckbox";
            this.collideCheckbox.Size = new System.Drawing.Size(172, 17);
            this.collideCheckbox.TabIndex = 39;
            this.collideCheckbox.Text = "Agents Collide with Each Other";
            this.collideCheckbox.UseVisualStyleBackColor = true;
            // 
            // visibleCheckbox
            // 
            this.visibleCheckbox.AutoSize = true;
            this.visibleCheckbox.Checked = true;
            this.visibleCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.visibleCheckbox.Location = new System.Drawing.Point(9, 223);
            this.visibleCheckbox.Name = "visibleCheckbox";
            this.visibleCheckbox.Size = new System.Drawing.Size(161, 17);
            this.visibleCheckbox.TabIndex = 38;
            this.visibleCheckbox.Text = "Agents Visible to Each Other";
            this.visibleCheckbox.UseVisualStyleBackColor = true;
            // 
            // orientation
            // 
            this.orientation.Location = new System.Drawing.Point(106, 56);
            this.orientation.Name = "orientation";
            this.orientation.Size = new System.Drawing.Size(44, 20);
            this.orientation.TabIndex = 32;
            this.orientation.Text = "0";
            // 
            // number_of_agents
            // 
            this.number_of_agents.Location = new System.Drawing.Point(106, 27);
            this.number_of_agents.Name = "number_of_agents";
            this.number_of_agents.Size = new System.Drawing.Size(44, 20);
            this.number_of_agents.TabIndex = 33;
            this.number_of_agents.Text = "5";
            this.number_of_agents.TextChanged += new System.EventHandler(this.number_of_agents_TextChanged);
            // 
            // spacing
            // 
            this.spacing.Location = new System.Drawing.Point(106, 92);
            this.spacing.Name = "spacing";
            this.spacing.Size = new System.Drawing.Size(44, 20);
            this.spacing.TabIndex = 34;
            this.spacing.Text = "30";
            // 
            // orientation_label
            // 
            this.orientation_label.Location = new System.Drawing.Point(6, 58);
            this.orientation_label.Name = "orientation_label";
            this.orientation_label.Size = new System.Drawing.Size(115, 18);
            this.orientation_label.TabIndex = 35;
            this.orientation_label.Text = "Group Orientation";
            // 
            // number_of_agents_label
            // 
            this.number_of_agents_label.Location = new System.Drawing.Point(6, 30);
            this.number_of_agents_label.Name = "number_of_agents_label";
            this.number_of_agents_label.Size = new System.Drawing.Size(100, 30);
            this.number_of_agents_label.TabIndex = 36;
            this.number_of_agents_label.Text = "Number of agents";
            // 
            // spacing_label
            // 
            this.spacing_label.Location = new System.Drawing.Point(6, 89);
            this.spacing_label.Name = "spacing_label";
            this.spacing_label.Size = new System.Drawing.Size(115, 37);
            this.spacing_label.TabIndex = 37;
            this.spacing_label.Text = "Spacing between agents";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.multiBrainCheckBox);
            this.groupBox2.Controls.Add(this.EScheckBox);
            this.groupBox2.Controls.Add(this.modulationCheckBox);
            this.groupBox2.Controls.Add(this.adaptiveNetworkCheckBox);
            this.groupBox2.Controls.Add(this.normalizeWeightsCheckBox1);
            this.groupBox2.Location = new System.Drawing.Point(569, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(140, 143);
            this.groupBox2.TabIndex = 61;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Network Settings";
            // 
            // multiBrainCheckBox
            // 
            this.multiBrainCheckBox.AutoSize = true;
            this.multiBrainCheckBox.Location = new System.Drawing.Point(6, 120);
            this.multiBrainCheckBox.Name = "multiBrainCheckBox";
            this.multiBrainCheckBox.Size = new System.Drawing.Size(75, 17);
            this.multiBrainCheckBox.TabIndex = 65;
            this.multiBrainCheckBox.Text = "Multi-Brain";
            this.multiBrainCheckBox.UseVisualStyleBackColor = true;
            // 
            // EScheckBox
            // 
            this.EScheckBox.AutoSize = true;
            this.EScheckBox.Location = new System.Drawing.Point(6, 98);
            this.EScheckBox.Name = "EScheckBox";
            this.EScheckBox.Size = new System.Drawing.Size(121, 17);
            this.EScheckBox.TabIndex = 64;
            this.EScheckBox.Text = "Evolvable-Substrate";
            this.EScheckBox.UseVisualStyleBackColor = true;
            this.EScheckBox.CheckedChanged += new System.EventHandler(this.EScheckBox_CheckedChanged);
            // 
            // modulationCheckBox
            // 
            this.modulationCheckBox.AutoSize = true;
            this.modulationCheckBox.Location = new System.Drawing.Point(6, 71);
            this.modulationCheckBox.Name = "modulationCheckBox";
            this.modulationCheckBox.Size = new System.Drawing.Size(106, 17);
            this.modulationCheckBox.TabIndex = 63;
            this.modulationCheckBox.Text = "Neuromodulation";
            this.modulationCheckBox.UseVisualStyleBackColor = true;
            this.modulationCheckBox.CheckedChanged += new System.EventHandler(this.modulationCheckBox_CheckedChanged);
            // 
            // adaptiveNetworkCheckBox
            // 
            this.adaptiveNetworkCheckBox.AutoSize = true;
            this.adaptiveNetworkCheckBox.Location = new System.Drawing.Point(6, 48);
            this.adaptiveNetworkCheckBox.Name = "adaptiveNetworkCheckBox";
            this.adaptiveNetworkCheckBox.Size = new System.Drawing.Size(57, 17);
            this.adaptiveNetworkCheckBox.TabIndex = 62;
            this.adaptiveNetworkCheckBox.Text = "Plastic";
            this.adaptiveNetworkCheckBox.UseVisualStyleBackColor = true;
            // 
            // normalizeWeightsCheckBox1
            // 
            this.normalizeWeightsCheckBox1.AutoSize = true;
            this.normalizeWeightsCheckBox1.Checked = true;
            this.normalizeWeightsCheckBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.normalizeWeightsCheckBox1.Location = new System.Drawing.Point(6, 24);
            this.normalizeWeightsCheckBox1.Name = "normalizeWeightsCheckBox1";
            this.normalizeWeightsCheckBox1.Size = new System.Drawing.Size(114, 17);
            this.normalizeWeightsCheckBox1.TabIndex = 61;
            this.normalizeWeightsCheckBox1.Text = "Normalize Weights";
            this.normalizeWeightsCheckBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.headingNoiseDataLabel);
            this.groupBox3.Controls.Add(this.effectorNoiseDataLabel);
            this.groupBox3.Controls.Add(this.sensorNoiseDataLabel);
            this.groupBox3.Controls.Add(this.density_label);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.headingNoiseTrackBar);
            this.groupBox3.Controls.Add(this.effectorNoiseTrackBar);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.sensorNoiseTrackBar);
            this.groupBox3.Controls.Add(this.sensorNoiseLabel);
            this.groupBox3.Controls.Add(this.density);
            this.groupBox3.Location = new System.Drawing.Point(276, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(284, 197);
            this.groupBox3.TabIndex = 63;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Sensors Settings";
            // 
            // headingNoiseDataLabel
            // 
            this.headingNoiseDataLabel.AutoSize = true;
            this.headingNoiseDataLabel.Location = new System.Drawing.Point(42, 165);
            this.headingNoiseDataLabel.Name = "headingNoiseDataLabel";
            this.headingNoiseDataLabel.Size = new System.Drawing.Size(13, 13);
            this.headingNoiseDataLabel.TabIndex = 78;
            this.headingNoiseDataLabel.Text = "?";
            // 
            // effectorNoiseDataLabel
            // 
            this.effectorNoiseDataLabel.AutoSize = true;
            this.effectorNoiseDataLabel.Location = new System.Drawing.Point(42, 124);
            this.effectorNoiseDataLabel.Name = "effectorNoiseDataLabel";
            this.effectorNoiseDataLabel.Size = new System.Drawing.Size(13, 13);
            this.effectorNoiseDataLabel.TabIndex = 77;
            this.effectorNoiseDataLabel.Text = "?";
            // 
            // sensorNoiseDataLabel
            // 
            this.sensorNoiseDataLabel.AutoSize = true;
            this.sensorNoiseDataLabel.Location = new System.Drawing.Point(42, 83);
            this.sensorNoiseDataLabel.Name = "sensorNoiseDataLabel";
            this.sensorNoiseDataLabel.Size = new System.Drawing.Size(13, 13);
            this.sensorNoiseDataLabel.TabIndex = 76;
            this.sensorNoiseDataLabel.Text = "?";
            // 
            // density_label
            // 
            this.density_label.Location = new System.Drawing.Point(8, 27);
            this.density_label.Name = "density_label";
            this.density_label.Size = new System.Drawing.Size(100, 21);
            this.density_label.TabIndex = 75;
            this.density_label.Text = "Number Sensors:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 72;
            this.label2.Text = "Heading Noise";
            // 
            // headingNoiseTrackBar
            // 
            this.headingNoiseTrackBar.Location = new System.Drawing.Point(93, 149);
            this.headingNoiseTrackBar.Maximum = 100;
            this.headingNoiseTrackBar.Name = "headingNoiseTrackBar";
            this.headingNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.headingNoiseTrackBar.TabIndex = 71;
            this.headingNoiseTrackBar.Scroll += new System.EventHandler(this.headingNoiseTrackBar_Scroll);
            // 
            // effectorNoiseTrackBar
            // 
            this.effectorNoiseTrackBar.Location = new System.Drawing.Point(93, 106);
            this.effectorNoiseTrackBar.Maximum = 100;
            this.effectorNoiseTrackBar.Name = "effectorNoiseTrackBar";
            this.effectorNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.effectorNoiseTrackBar.TabIndex = 70;
            this.effectorNoiseTrackBar.Scroll += new System.EventHandler(this.effectorNoiseTrackBar_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 69;
            this.label3.Text = "Effector Noise";
            // 
            // sensorNoiseTrackBar
            // 
            this.sensorNoiseTrackBar.Location = new System.Drawing.Point(93, 67);
            this.sensorNoiseTrackBar.Maximum = 100;
            this.sensorNoiseTrackBar.Name = "sensorNoiseTrackBar";
            this.sensorNoiseTrackBar.Size = new System.Drawing.Size(180, 45);
            this.sensorNoiseTrackBar.TabIndex = 67;
            this.sensorNoiseTrackBar.Scroll += new System.EventHandler(this.sensorNoiseTrackBar_Scroll);
            // 
            // sensorNoiseLabel
            // 
            this.sensorNoiseLabel.AutoSize = true;
            this.sensorNoiseLabel.Location = new System.Drawing.Point(8, 67);
            this.sensorNoiseLabel.Name = "sensorNoiseLabel";
            this.sensorNoiseLabel.Size = new System.Drawing.Size(70, 13);
            this.sensorNoiseLabel.TabIndex = 66;
            this.sensorNoiseLabel.Text = "Sensor Noise";
            // 
            // density
            // 
            this.density.Location = new System.Drawing.Point(114, 28);
            this.density.Name = "density";
            this.density.Size = new System.Drawing.Size(159, 20);
            this.density.TabIndex = 65;
            this.density.Text = "5";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(715, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 13);
            this.label5.TabIndex = 67;
            this.label5.Text = "Fitness Function Name";
            // 
            // NoveltySearchCheckBox
            // 
            this.NoveltySearchCheckBox.AutoSize = true;
            this.NoveltySearchCheckBox.Location = new System.Drawing.Point(718, 153);
            this.NoveltySearchCheckBox.Name = "NoveltySearchCheckBox";
            this.NoveltySearchCheckBox.Size = new System.Drawing.Size(99, 17);
            this.NoveltySearchCheckBox.TabIndex = 65;
            this.NoveltySearchCheckBox.Text = "Novelty Search";
            this.NoveltySearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(718, 59);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(269, 67);
            this.textBox2.TabIndex = 66;
            // 
            // functionComboBox
            // 
            this.functionComboBox.FormattingEnabled = true;
            this.functionComboBox.Location = new System.Drawing.Point(836, 32);
            this.functionComboBox.Name = "functionComboBox";
            this.functionComboBox.Size = new System.Drawing.Size(151, 21);
            this.functionComboBox.TabIndex = 68;
            this.functionComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // behaviorTextBox
            // 
            this.behaviorTextBox.Location = new System.Drawing.Point(718, 196);
            this.behaviorTextBox.Multiline = true;
            this.behaviorTextBox.Name = "behaviorTextBox";
            this.behaviorTextBox.ReadOnly = true;
            this.behaviorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.behaviorTextBox.Size = new System.Drawing.Size(269, 67);
            this.behaviorTextBox.TabIndex = 69;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(715, 177);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 13);
            this.label6.TabIndex = 70;
            this.label6.Text = "Behavior Characterization";
            // 
            // behaviorComboBox
            // 
            this.behaviorComboBox.FormattingEnabled = true;
            this.behaviorComboBox.Location = new System.Drawing.Point(849, 169);
            this.behaviorComboBox.Name = "behaviorComboBox";
            this.behaviorComboBox.Size = new System.Drawing.Size(138, 21);
            this.behaviorComboBox.TabIndex = 71;
            this.behaviorComboBox.SelectedIndexChanged += new System.EventHandler(this.behaviorComboBox_SelectedIndexChanged);
            // 
            // multiObjectiveCheckBox
            // 
            this.multiObjectiveCheckBox.AutoSize = true;
            this.multiObjectiveCheckBox.Location = new System.Drawing.Point(718, 132);
            this.multiObjectiveCheckBox.Name = "multiObjectiveCheckBox";
            this.multiObjectiveCheckBox.Size = new System.Drawing.Size(96, 17);
            this.multiObjectiveCheckBox.TabIndex = 72;
            this.multiObjectiveCheckBox.Text = "Multi-Objective";
            this.multiObjectiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 315);
            this.Controls.Add(this.multiObjectiveCheckBox);
            this.Controls.Add(this.behaviorComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.behaviorTextBox);
            this.Controls.Add(this.functionComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.NoveltySearchCheckBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lower_range);
            this.Controls.Add(this.upper_range);
            this.Controls.Add(this.populationSizeTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.substrate);
            this.Controls.Add(this.substrate_label);
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headingNoiseTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.effectorNoiseTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sensorNoiseTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox populationSizeTextBox;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button okbutton;
        public System.Windows.Forms.Button cancelbutton;
        public System.Windows.Forms.TextBox substrate;
        public System.Windows.Forms.Label substrate_label;
        public System.Windows.Forms.TextBox lower_range;
        public System.Windows.Forms.TextBox upper_range;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox heading;
        public System.Windows.Forms.Label heading_label;
        public System.Windows.Forms.CheckBox homogeneousCheckbox;
        public System.Windows.Forms.CheckBox collideCheckbox;
        public System.Windows.Forms.CheckBox visibleCheckbox;
        public System.Windows.Forms.TextBox orientation;
        public System.Windows.Forms.TextBox number_of_agents;
        public System.Windows.Forms.TextBox spacing;
        public System.Windows.Forms.Label orientation_label;
        public System.Windows.Forms.Label number_of_agents_label;
        public System.Windows.Forms.Label spacing_label;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox EScheckBox;
        public System.Windows.Forms.CheckBox modulationCheckBox;
        public System.Windows.Forms.CheckBox adaptiveNetworkCheckBox;
        public System.Windows.Forms.CheckBox normalizeWeightsCheckBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.Label density_label;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TrackBar headingNoiseTrackBar;
        public System.Windows.Forms.TrackBar effectorNoiseTrackBar;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TrackBar sensorNoiseTrackBar;
        private System.Windows.Forms.Label sensorNoiseLabel;
        public System.Windows.Forms.TextBox density;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.CheckBox NoveltySearchCheckBox;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ComboBox functionComboBox;
        private System.Windows.Forms.TextBox behaviorTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox behaviorComboBox;
        public System.Windows.Forms.CheckBox multiObjectiveCheckBox;
        public System.Windows.Forms.CheckBox multiBrainCheckBox;
        public System.Windows.Forms.ComboBox robotModelComboBox;
        private System.Windows.Forms.Label sensorNoiseDataLabel;
        private System.Windows.Forms.Label effectorNoiseDataLabel;
        private System.Windows.Forms.Label headingNoiseDataLabel;
    }
}