namespace MazeSimulator
{
    partial class SimulatorVisualizer
	{
		//windows stuff
		private System.ComponentModel.IContainer components = null;
		//timer for animation...called 20x sec
		private System.Windows.Forms.Timer animation;

				//windows forms dialogs menus etc.
        private System.Windows.Forms.OpenFileDialog fileOpenDialog;
		private System.Windows.Forms.SaveFileDialog fileSaveDialog;

        private System.Windows.Forms.MenuStrip menuStrip;
		
		
		private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
		private System.Windows.Forms.ToolStripMenuItem evolveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleAnimation;
		private System.Windows.Forms.ToolStripMenuItem displayFOVItem;
		
		private System.Windows.Forms.ToolStripMenuItem modesItem;
		private System.Windows.Forms.ToolStripMenuItem wallModeItem;
		private System.Windows.Forms.ToolStripMenuItem startModeItem;
        private System.Windows.Forms.ToolStripMenuItem goalModeItem;
		private System.Windows.Forms.ToolStripMenuItem selectModeItem;
		
		//dunno copied this
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
		
				//boring gui stuff
		//sets all the menus gui elements etc...
		private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimulatorVisualizer));
            this.animation = new System.Windows.Forms.Timer(this.components);
            this.fileSaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.fileOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newExperimentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newEnvironmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.environmentToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.genomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.environmentToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveCPPNAsDOTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDecodedANNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wallModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goalModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pOIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.areaOfInterestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evolveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleDisplayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayLoadedGenomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.readmeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.playButton = new System.Windows.Forms.ToolStripButton();
            this.pauseButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSelectButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripWallButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripGoalButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripStartButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripPOIButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripAOIButton = new System.Windows.Forms.ToolStripButton();
            this.resetStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // animation
            // 
            this.animation.Enabled = true;
            this.animation.Interval = 20;
            this.animation.Tick += new System.EventHandler(this.clocktick);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem,
            this.modesItem,
            this.animationMenuItem,
            this.evolveMenuItem,
            this.toolStripMenuItem2});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(700, 24);
            this.menuStrip.TabIndex = 0;
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator7,
            this.exitToolStripMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newExperimentMenuItem,
            this.newEnvironmentToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // newExperimentMenuItem
            // 
            this.newExperimentMenuItem.Name = "newExperimentMenuItem";
            this.newExperimentMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newExperimentMenuItem.Text = "Experiment";
            this.newExperimentMenuItem.Click += new System.EventHandler(this.newExperimentMenuItem_Click);
            // 
            // newEnvironmentToolStripMenuItem
            // 
            this.newEnvironmentToolStripMenuItem.Name = "newEnvironmentToolStripMenuItem";
            this.newEnvironmentToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newEnvironmentToolStripMenuItem.Text = "Environment";
            this.newEnvironmentToolStripMenuItem.Click += new System.EventHandler(this.newEnvironmentToolStripMenuItem_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.experimentToolStripMenuItem,
            this.environmentToolStripMenuItem1,
            this.genomeToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.loadToolStripMenuItem.Text = "Open";
            // 
            // experimentToolStripMenuItem
            // 
            this.experimentToolStripMenuItem.Name = "experimentToolStripMenuItem";
            this.experimentToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.experimentToolStripMenuItem.Text = "Experiment";
            this.experimentToolStripMenuItem.Click += new System.EventHandler(this.loadExperimentToolStripMenuItem_Click);
            // 
            // environmentToolStripMenuItem1
            // 
            this.environmentToolStripMenuItem1.Name = "environmentToolStripMenuItem1";
            this.environmentToolStripMenuItem1.Size = new System.Drawing.Size(142, 22);
            this.environmentToolStripMenuItem1.Text = "Environment";
            this.environmentToolStripMenuItem1.Click += new System.EventHandler(this.loadEnvironmentClick);
            // 
            // genomeToolStripMenuItem
            // 
            this.genomeToolStripMenuItem.Name = "genomeToolStripMenuItem";
            this.genomeToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.genomeToolStripMenuItem.Text = "Genome";
            this.genomeToolStripMenuItem.Click += new System.EventHandler(this.loadGenomeClick);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.experimentToolStripMenuItem1,
            this.environmentToolStripMenuItem2,
            this.toolStripMenuItem4,
            this.saveCPPNAsDOTToolStripMenuItem,
            this.saveDecodedANNToolStripMenuItem});
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // experimentToolStripMenuItem1
            // 
            this.experimentToolStripMenuItem1.Name = "experimentToolStripMenuItem1";
            this.experimentToolStripMenuItem1.Size = new System.Drawing.Size(146, 22);
            this.experimentToolStripMenuItem1.Text = "Experiment";
            this.experimentToolStripMenuItem1.Click += new System.EventHandler(this.saveExperimentToolStripMenuItem_Click);
            // 
            // environmentToolStripMenuItem2
            // 
            this.environmentToolStripMenuItem2.Name = "environmentToolStripMenuItem2";
            this.environmentToolStripMenuItem2.Size = new System.Drawing.Size(146, 22);
            this.environmentToolStripMenuItem2.Text = "Environment";
            this.environmentToolStripMenuItem2.Click += new System.EventHandler(this.saveEnvironmentClick);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(143, 6);
            // 
            // saveCPPNAsDOTToolStripMenuItem
            // 
            this.saveCPPNAsDOTToolStripMenuItem.Enabled = false;
            this.saveCPPNAsDOTToolStripMenuItem.Name = "saveCPPNAsDOTToolStripMenuItem";
            this.saveCPPNAsDOTToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveCPPNAsDOTToolStripMenuItem.Text = "CPPN as DOT";
            // 
            // saveDecodedANNToolStripMenuItem
            // 
            this.saveDecodedANNToolStripMenuItem.Enabled = false;
            this.saveDecodedANNToolStripMenuItem.Name = "saveDecodedANNToolStripMenuItem";
            this.saveDecodedANNToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveDecodedANNToolStripMenuItem.Text = "ANN as XML";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // modesItem
            // 
            this.modesItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectModeItem,
            this.wallModeItem,
            this.startModeItem,
            this.goalModeItem,
            this.pOIToolStripMenuItem,
            this.areaOfInterestToolStripMenuItem});
            this.modesItem.Name = "modesItem";
            this.modesItem.Size = new System.Drawing.Size(50, 20);
            this.modesItem.Text = "Mode";
            // 
            // selectModeItem
            // 
            this.selectModeItem.Image = ((System.Drawing.Image)(resources.GetObject("selectModeItem.Image")));
            this.selectModeItem.Name = "selectModeItem";
            this.selectModeItem.Size = new System.Drawing.Size(154, 22);
            this.selectModeItem.Text = "Select";
            this.selectModeItem.Click += new System.EventHandler(this.selectModeClick);
            // 
            // wallModeItem
            // 
            this.wallModeItem.Image = ((System.Drawing.Image)(resources.GetObject("wallModeItem.Image")));
            this.wallModeItem.Name = "wallModeItem";
            this.wallModeItem.Size = new System.Drawing.Size(154, 22);
            this.wallModeItem.Text = "Wall";
            this.wallModeItem.Click += new System.EventHandler(this.wallModeClick);
            // 
            // startModeItem
            // 
            this.startModeItem.Image = ((System.Drawing.Image)(resources.GetObject("startModeItem.Image")));
            this.startModeItem.Name = "startModeItem";
            this.startModeItem.Size = new System.Drawing.Size(154, 22);
            this.startModeItem.Text = "Start Point";
            this.startModeItem.Click += new System.EventHandler(this.startModeClick);
            // 
            // goalModeItem
            // 
            this.goalModeItem.Image = ((System.Drawing.Image)(resources.GetObject("goalModeItem.Image")));
            this.goalModeItem.Name = "goalModeItem";
            this.goalModeItem.Size = new System.Drawing.Size(154, 22);
            this.goalModeItem.Text = "Goal Point";
            this.goalModeItem.Click += new System.EventHandler(this.goalModeClick);
            // 
            // pOIToolStripMenuItem
            // 
            this.pOIToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pOIToolStripMenuItem.Image")));
            this.pOIToolStripMenuItem.Name = "pOIToolStripMenuItem";
            this.pOIToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.pOIToolStripMenuItem.Text = "POI";
            this.pOIToolStripMenuItem.Click += new System.EventHandler(this.pOIToolStripMenuItem_Click);
            // 
            // areaOfInterestToolStripMenuItem
            // 
            this.areaOfInterestToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("areaOfInterestToolStripMenuItem.Image")));
            this.areaOfInterestToolStripMenuItem.Name = "areaOfInterestToolStripMenuItem";
            this.areaOfInterestToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.areaOfInterestToolStripMenuItem.Text = "Area of Interest";
            this.areaOfInterestToolStripMenuItem.Click += new System.EventHandler(this.areaOfInterestToolStripMenuItem_Click);
            // 
            // animationMenuItem
            // 
            this.animationMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleAnimation,
            this.resetToolStripMenuItem,
            this.toolStripMenuItem3,
            this.settingsToolStripMenuItem});
            this.animationMenuItem.Name = "animationMenuItem";
            this.animationMenuItem.Size = new System.Drawing.Size(76, 20);
            this.animationMenuItem.Text = "Simulation";
            this.animationMenuItem.Click += new System.EventHandler(this.animationMenuItem_Click);
            // 
            // toggleAnimation
            // 
            this.toggleAnimation.Image = ((System.Drawing.Image)(resources.GetObject("toggleAnimation.Image")));
            this.toggleAnimation.Name = "toggleAnimation";
            this.toggleAnimation.Size = new System.Drawing.Size(178, 22);
            this.toggleAnimation.Text = "Run";
            this.toggleAnimation.Click += new System.EventHandler(this.toggleAnimationClick);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("resetToolStripMenuItem.Image")));
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(175, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.settingsToolStripMenuItem.Text = "Experiment Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // evolveMenuItem
            // 
            this.evolveMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.toolStripSeparator4,
            this.toggleDisplayToolStripMenuItem,
            this.displayLoadedGenomeToolStripMenuItem});
            this.evolveMenuItem.Name = "evolveMenuItem";
            this.evolveMenuItem.Size = new System.Drawing.Size(69, 20);
            this.evolveMenuItem.Text = "Evolution";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(189, 6);
            // 
            // toggleDisplayToolStripMenuItem
            // 
            this.toggleDisplayToolStripMenuItem.Name = "toggleDisplayToolStripMenuItem";
            this.toggleDisplayToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.toggleDisplayToolStripMenuItem.Text = "Display Best Individual";
            this.toggleDisplayToolStripMenuItem.Click += new System.EventHandler(this.toggleDisplayToolStripMenuItem_Click);
            // 
            // displayLoadedGenomeToolStripMenuItem
            // 
            this.displayLoadedGenomeToolStripMenuItem.Enabled = false;
            this.displayLoadedGenomeToolStripMenuItem.Name = "displayLoadedGenomeToolStripMenuItem";
            this.displayLoadedGenomeToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.displayLoadedGenomeToolStripMenuItem.Text = "Genome Viewer";
            this.displayLoadedGenomeToolStripMenuItem.Click += new System.EventHandler(this.displayLoadedGenomeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readmeToolStripMenuItem,
            this.keyCommandsToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutToolStripMenuItem});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(44, 20);
            this.toolStripMenuItem2.Text = "Help";
            // 
            // readmeToolStripMenuItem
            // 
            this.readmeToolStripMenuItem.Name = "readmeToolStripMenuItem";
            this.readmeToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.readmeToolStripMenuItem.Text = "Readme";
            this.readmeToolStripMenuItem.Click += new System.EventHandler(this.readmeToolStripMenuItem_Click);
            // 
            // keyCommandsToolStripMenuItem
            // 
            this.keyCommandsToolStripMenuItem.Name = "keyCommandsToolStripMenuItem";
            this.keyCommandsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.keyCommandsToolStripMenuItem.Text = "Key Commands";
            this.keyCommandsToolStripMenuItem.Click += new System.EventHandler(this.keyCommandsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(155, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playButton,
            this.pauseButton,
            this.resetStripButton1,
            this.toolStripSeparator1,
            this.toolStripSelectButton,
            this.toolStripWallButton,
            this.toolStripGoalButton,
            this.toolStripStartButton,
            this.toolStripPOIButton,
            this.toolStripAOIButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(700, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // playButton
            // 
            this.playButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.playButton.Image = ((System.Drawing.Image)(resources.GetObject("playButton.Image")));
            this.playButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(23, 22);
            this.playButton.Text = "Play";
            this.playButton.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // pauseButton
            // 
            this.pauseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pauseButton.Enabled = false;
            this.pauseButton.Image = ((System.Drawing.Image)(resources.GetObject("pauseButton.Image")));
            this.pauseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(23, 22);
            this.pauseButton.Text = "Pause";
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSelectButton
            // 
            this.toolStripSelectButton.Checked = true;
            this.toolStripSelectButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripSelectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSelectButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSelectButton.Image")));
            this.toolStripSelectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSelectButton.Name = "toolStripSelectButton";
            this.toolStripSelectButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripSelectButton.Text = "Select";
            this.toolStripSelectButton.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // toolStripWallButton
            // 
            this.toolStripWallButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripWallButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripWallButton.Image")));
            this.toolStripWallButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripWallButton.Name = "toolStripWallButton";
            this.toolStripWallButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripWallButton.Text = "Wall";
            this.toolStripWallButton.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // toolStripGoalButton
            // 
            this.toolStripGoalButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripGoalButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripGoalButton.Image")));
            this.toolStripGoalButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripGoalButton.Name = "toolStripGoalButton";
            this.toolStripGoalButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripGoalButton.Text = "Goal";
            this.toolStripGoalButton.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripStartButton
            // 
            this.toolStripStartButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStartButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStartButton.Image")));
            this.toolStripStartButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStartButton.Name = "toolStripStartButton";
            this.toolStripStartButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripStartButton.Text = "Start";
            this.toolStripStartButton.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripPOIButton
            // 
            this.toolStripPOIButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripPOIButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripPOIButton.Image")));
            this.toolStripPOIButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripPOIButton.Name = "toolStripPOIButton";
            this.toolStripPOIButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripPOIButton.Text = "Point of Interest";
            this.toolStripPOIButton.Click += new System.EventHandler(this.toolStripButton6_Click);
            // 
            // toolStripAOIButton
            // 
            this.toolStripAOIButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripAOIButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripAOIButton.Image")));
            this.toolStripAOIButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripAOIButton.Name = "toolStripAOIButton";
            this.toolStripAOIButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripAOIButton.Text = "Area of Interest";
            this.toolStripAOIButton.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // resetStripButton1
            // 
            this.resetStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.resetStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("resetStripButton1.Image")));
            this.resetStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.resetStripButton1.Name = "resetStripButton1";
            this.resetStripButton1.Size = new System.Drawing.Size(23, 22);
            this.resetStripButton1.Text = "toolStripButton1";
            this.resetStripButton1.ToolTipText = "Restart";
            this.resetStripButton1.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // SimulatorVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 600);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "SimulatorVisualizer";
            this.Text = "MultiAgent-HyperSharpNEAT Simulator";
            this.Load += new System.EventHandler(this.SimulatorVisualizer_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressHandler);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MouseClickHandler);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MouseWheelHandler);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.ToolStripMenuItem pOIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem areaOfInterestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleDisplayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayPathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hidePathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem keyCommandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton playButton;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripButton pauseButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripWallButton;
        private System.Windows.Forms.ToolStripButton toolStripGoalButton;
        private System.Windows.Forms.ToolStripButton toolStripStartButton;
        private System.Windows.Forms.ToolStripButton toolStripAOIButton;
        private System.Windows.Forms.ToolStripButton toolStripSelectButton;
        private System.Windows.Forms.ToolStripButton toolStripPOIButton;
        private System.Windows.Forms.ToolStripMenuItem readmeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newExperimentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newEnvironmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem environmentToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem genomeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem environmentToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem displayLoadedGenomeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCPPNAsDOTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveDecodedANNToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripButton resetStripButton1;


	}
}