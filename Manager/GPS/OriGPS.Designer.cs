namespace LiveSplit.OriDE
{
    partial class OriGPS
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
            this.mapPanel = new LiveSplit.OriDE.GPS.MapPanel();
            this.SuspendLayout();
            // 
            // mapPanel
            // 
            this.mapPanel.Location = new System.Drawing.Point(12, 12);
            this.mapPanel.Name = "mapPanel";
            this.mapPanel.Size = new System.Drawing.Size(2206, 1118);
            this.mapPanel.TabIndex = 0;
            this.mapPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.mapPanel_Scroll);
            // 
            // OriGPS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2230, 1142);
            this.Controls.Add(this.mapPanel);
            this.Name = "OriGPS";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private GPS.MapPanel mapPanel;
    }
}