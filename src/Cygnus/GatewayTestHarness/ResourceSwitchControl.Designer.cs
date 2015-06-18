namespace Cygnus.GatewayTestHarness
{
    partial class ResourceSwitchControl
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
            this.m_button = new System.Windows.Forms.Button();
            this.m_groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_groupBox
            // 
            this.m_groupBox.Controls.Add(this.m_button);
            this.m_groupBox.Location = new System.Drawing.Point(4, 4);
            this.m_groupBox.Name = "m_groupBox";
            this.m_groupBox.Size = new System.Drawing.Size(118, 79);
            this.m_groupBox.TabIndex = 0;
            this.m_groupBox.TabStop = false;
            this.m_groupBox.Text = "groupBox1";
            // 
            // m_button
            // 
            this.m_button.Location = new System.Drawing.Point(21, 39);
            this.m_button.Name = "m_button";
            this.m_button.Size = new System.Drawing.Size(75, 23);
            this.m_button.TabIndex = 0;
            this.m_button.Text = "button1";
            this.m_button.UseVisualStyleBackColor = true;
            // 
            // ResourceSwitchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_groupBox);
            this.Name = "ResourceSwitchControl";
            this.Size = new System.Drawing.Size(125, 86);
            this.m_groupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_groupBox;
        private System.Windows.Forms.Button m_button;
    }
}
