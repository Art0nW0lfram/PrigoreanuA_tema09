namespace OpenTK_winforms_z02
{
    partial class Form1
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
            this.GlControl1 = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // GlControl1
            // 
            this.GlControl1.BackColor = System.Drawing.Color.Black;
            this.GlControl1.Location = new System.Drawing.Point(13, 13);
            this.GlControl1.Margin = new System.Windows.Forms.Padding(4);
            this.GlControl1.Name = "GlControl1";
            this.GlControl1.Size = new System.Drawing.Size(881, 776);
            this.GlControl1.TabIndex = 0;
            this.GlControl1.VSync = false;
            this.GlControl1.Load += new System.EventHandler(this.GlControl1_Load);
            this.GlControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.GlControl1_Paint);
            this.GlControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlControl1_MouseDown);
            this.GlControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlControl1_MouseMove);
            this.GlControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GlControl1_MouseUp);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.GlControl1);
            this.Name = "Form1";
            this.Text = "EGC Slot Machine Bonus";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl GlControl1;
    }
}