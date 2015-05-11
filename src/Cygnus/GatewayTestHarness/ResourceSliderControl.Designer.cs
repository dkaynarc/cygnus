namespace Cygnus.GatewayTestHarness
{
    partial class ResourceSliderControl
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
            this.m_groupBox = new System.Windows.Forms.GroupBox();
            this.m_slider = new System.Windows.Forms.TrackBar();
            this.m_valueTextBox = new System.Windows.Forms.TextBox();
            this.m_groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_slider)).BeginInit();
            this.SuspendLayout();
            // 
            // m_groupBox
            // 
            this.m_groupBox.Controls.Add(this.m_valueTextBox);
            this.m_groupBox.Controls.Add(this.m_slider);
            this.m_groupBox.Location = new System.Drawing.Point(4, 0);
            this.m_groupBox.Name = "m_groupBox";
            this.m_groupBox.Size = new System.Drawing.Size(445, 70);
            this.m_groupBox.TabIndex = 0;
            this.m_groupBox.TabStop = false;
            this.m_groupBox.Text = "groupBox1";
            // 
            // m_slider
            // 
            this.m_slider.Location = new System.Drawing.Point(6, 19);
            this.m_slider.Name = "m_slider";
            this.m_slider.Size = new System.Drawing.Size(329, 45);
            this.m_slider.TabIndex = 0;
            // 
            // m_valueTextBox
            // 
            this.m_valueTextBox.Location = new System.Drawing.Point(339, 19);
            this.m_valueTextBox.Name = "m_valueTextBox";
            this.m_valueTextBox.Size = new System.Drawing.Size(100, 20);
            this.m_valueTextBox.TabIndex = 1;
            // 
            // ResourceSlider
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_groupBox);
            this.Name = "ResourceSlider";
            this.Size = new System.Drawing.Size(449, 73);
            this.m_groupBox.ResumeLayout(false);
            this.m_groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_slider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_groupBox;
        private System.Windows.Forms.TextBox m_valueTextBox;
        private System.Windows.Forms.TrackBar m_slider;
    }
}
