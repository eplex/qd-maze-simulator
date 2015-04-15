namespace MazeSimulator
{
    partial class wallAdder
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.X1textBox = new System.Windows.Forms.TextBox();
            this.Y1textBox = new System.Windows.Forms.TextBox();
            this.X2textBox = new System.Windows.Forms.TextBox();
            this.Y2textBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "X2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Y2";
            // 
            // X1textBox
            // 
            this.X1textBox.Location = new System.Drawing.Point(40, 10);
            this.X1textBox.Name = "X1textBox";
            this.X1textBox.Size = new System.Drawing.Size(50, 20);
            this.X1textBox.TabIndex = 4;
            // 
            // Y1textBox
            // 
            this.Y1textBox.Location = new System.Drawing.Point(40, 43);
            this.Y1textBox.Name = "Y1textBox";
            this.Y1textBox.Size = new System.Drawing.Size(50, 20);
            this.Y1textBox.TabIndex = 5;
            // 
            // X2textBox
            // 
            this.X2textBox.Location = new System.Drawing.Point(39, 69);
            this.X2textBox.Name = "X2textBox";
            this.X2textBox.Size = new System.Drawing.Size(50, 20);
            this.X2textBox.TabIndex = 6;
            // 
            // Y2textBox
            // 
            this.Y2textBox.Location = new System.Drawing.Point(40, 100);
            this.Y2textBox.Name = "Y2textBox";
            this.Y2textBox.Size = new System.Drawing.Size(50, 20);
            this.Y2textBox.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 126);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 24);
            this.button1.TabIndex = 8;
            this.button1.Text = "Add Wall";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // wallAdder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(116, 158);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Y2textBox);
            this.Controls.Add(this.X2textBox);
            this.Controls.Add(this.Y1textBox);
            this.Controls.Add(this.X1textBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "wallAdder";
            this.Text = "Wall Adder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox X1textBox;
        private System.Windows.Forms.TextBox Y1textBox;
        private System.Windows.Forms.TextBox X2textBox;
        private System.Windows.Forms.TextBox Y2textBox;
        private System.Windows.Forms.Button button1;
    }
}