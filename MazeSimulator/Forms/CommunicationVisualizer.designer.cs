namespace MazeSimulator
{
    partial class CommunicationVisualizer
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
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.checkBoxAutoScroll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Location = new System.Drawing.Point(19, 9);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(347, 21);
            this.hScrollBar1.TabIndex = 1;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.refreshScreen);
            // 
            // checkBoxAutoScroll
            // 
            this.checkBoxAutoScroll.AutoSize = true;
            this.checkBoxAutoScroll.Checked = true;
            this.checkBoxAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoScroll.Location = new System.Drawing.Point(405, 9);
            this.checkBoxAutoScroll.Name = "checkBoxAutoScroll";
            this.checkBoxAutoScroll.Size = new System.Drawing.Size(91, 21);
            this.checkBoxAutoScroll.TabIndex = 2;
            this.checkBoxAutoScroll.Text = "autoscroll";
            this.checkBoxAutoScroll.UseVisualStyleBackColor = true;
            // 
            // CommunicationVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1077, 477);
            this.Controls.Add(this.checkBoxAutoScroll);
            this.Controls.Add(this.hScrollBar1);
            this.Name = "CommunicationVisualizer";
            this.Text = "CommunicationVisualizer";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CommunicationVisualizer_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.CheckBox checkBoxAutoScroll;
    }
}