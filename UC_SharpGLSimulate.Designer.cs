namespace UC_SharpGLSimulate
{
    partial class UC_SharpGLSimulate
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GLC_Model = new SharpGL.OpenGLControl();
            this.lblFinished = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.GLC_Model)).BeginInit();
            this.SuspendLayout();
            // 
            // GLC_Model
            // 
            this.GLC_Model.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.GLC_Model.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GLC_Model.DrawFPS = false;
            this.GLC_Model.Location = new System.Drawing.Point(0, 0);
            this.GLC_Model.Name = "GLC_Model";
            this.GLC_Model.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            this.GLC_Model.RenderContextType = SharpGL.RenderContextType.DIBSection;
            this.GLC_Model.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            this.GLC_Model.Size = new System.Drawing.Size(800, 450);
            this.GLC_Model.TabIndex = 0;
            // 
            // lblFinished
            // 
            this.lblFinished.AutoSize = true;
            this.lblFinished.BackColor = System.Drawing.Color.Black;
            this.lblFinished.ForeColor = System.Drawing.Color.White;
            this.lblFinished.Location = new System.Drawing.Point(20, 11);
            this.lblFinished.Name = "lblFinished";
            this.lblFinished.Size = new System.Drawing.Size(59, 12);
            this.lblFinished.TabIndex = 4;
            this.lblFinished.Text = "Finished!";
            this.lblFinished.Visible = false;
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // UC_SharpGLSimulate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblFinished);
            this.Controls.Add(this.GLC_Model);
            this.Name = "UC_SharpGLSimulate";
            this.Size = new System.Drawing.Size(800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.GLC_Model)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SharpGL.OpenGLControl GLC_Model;
        private System.Windows.Forms.Label lblFinished;
        private System.Windows.Forms.Timer timer;
    }
}
