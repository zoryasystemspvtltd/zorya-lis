namespace ZoryaLIS.Controllers
{
    partial class BS240E
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
            splitContainer1 = new SplitContainer();
            btnClearLog = new Button();
            btnConnect = new Button();
            textBox1 = new TextBox();
            txtIP = new TextBox();
            txtPort = new TextBox();
            txtMachineKey = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(txtMachineKey);
            splitContainer1.Panel1.Controls.Add(txtPort);
            splitContainer1.Panel1.Controls.Add(txtIP);
            splitContainer1.Panel1.Controls.Add(btnClearLog);
            splitContainer1.Panel1.Controls.Add(btnConnect);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(textBox1);
            splitContainer1.Size = new Size(693, 488);
            splitContainer1.SplitterDistance = 45;
            splitContainer1.TabIndex = 0;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(573, 11);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 23);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Clear Log";
            btnClearLog.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(483, 11);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Black;
            textBox1.Dock = DockStyle.Fill;
            textBox1.ForeColor = Color.White;
            textBox1.Location = new Point(0, 0);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(693, 439);
            textBox1.TabIndex = 0;
            // 
            // txtIP
            // 
            txtIP.Location = new Point(11, 11);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(167, 23);
            txtIP.TabIndex = 2;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(184, 11);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(93, 23);
            txtPort.TabIndex = 3;
            // 
            // txtMachineKey
            // 
            txtMachineKey.Location = new Point(283, 11);
            txtMachineKey.Name = "txtMachineKey";
            txtMachineKey.Size = new Size(180, 23);
            txtMachineKey.TabIndex = 4;
            // 
            // BS240E
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "BS240E";
            Size = new Size(693, 488);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Button btnClearLog;
        private Button btnConnect;
        private TextBox textBox1;
        private TextBox txtMachineKey;
        private TextBox txtPort;
        private TextBox txtIP;
    }
}
