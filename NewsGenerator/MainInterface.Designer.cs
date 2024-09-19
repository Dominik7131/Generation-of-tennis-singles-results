
namespace NewsGenerator
{
    partial class NewsGenerator
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(235, 192);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(89, 15);
            this.label21.TabIndex = 20;
            this.label21.Text = "Unforced errors";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(226, 167);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(98, 15);
            this.label22.TabIndex = 19;
            this.label22.Text = "Break points won";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(211, 142);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(138, 15);
            this.label23.TabIndex = 18;
            this.label23.Text = "Second serve points won";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(220, 117);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(121, 15);
            this.label24.TabIndex = 17;
            this.label24.Text = "First serve points won";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(220, 92);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(129, 15);
            this.label25.TabIndex = 16;
            this.label25.Text = "First serve successful %";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(242, 67);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(77, 15);
            this.label26.TabIndex = 15;
            this.label26.Text = "Double faults";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(244, 41);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(32, 15);
            this.label27.TabIndex = 14;
            this.label27.Text = "Aces";
            // 
            // NewsGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Name = "NewsGenerator";
            this.Text = "News Generator";
            this.Load += new System.EventHandler(this.NewsGenerator_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
    }
}

