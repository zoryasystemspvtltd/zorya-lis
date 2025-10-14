namespace LisConsole
{
    partial class PortSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortSetting));
            this.bSave = new System.Windows.Forms.Button();
            this.bCancle = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cbRunOnStartup = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.cbAutoConnect = new System.Windows.Forms.CheckBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.tPriority = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.tStopBits = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.tDataBits = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.tRate = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.tPort = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.txtApiStatus = new System.Windows.Forms.Label();
            this.btnValidate = new System.Windows.Forms.Button();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.ddlEquipmentType = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // bSave
            // 
            this.bSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bSave.Location = new System.Drawing.Point(80, 313);
            this.bSave.Margin = new System.Windows.Forms.Padding(4);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(112, 28);
            this.bSave.TabIndex = 22;
            this.bSave.Text = "SAVE";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bCancle
            // 
            this.bCancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bCancle.Location = new System.Drawing.Point(225, 313);
            this.bCancle.Margin = new System.Windows.Forms.Padding(4);
            this.bCancle.Name = "bCancle";
            this.bCancle.Size = new System.Drawing.Size(112, 28);
            this.bCancle.TabIndex = 21;
            this.bCancle.Text = "CANCEL";
            this.bCancle.UseVisualStyleBackColor = true;
            this.bCancle.Click += new System.EventHandler(this.bCancle_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(351, 294);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.ddlEquipmentType);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.cbRunOnStartup);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(343, 268);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General Settings";
            // 
            // cbRunOnStartup
            // 
            this.cbRunOnStartup.AutoSize = true;
            this.cbRunOnStartup.Location = new System.Drawing.Point(164, 66);
            this.cbRunOnStartup.Margin = new System.Windows.Forms.Padding(4);
            this.cbRunOnStartup.Name = "cbRunOnStartup";
            this.cbRunOnStartup.Size = new System.Drawing.Size(15, 14);
            this.cbRunOnStartup.TabIndex = 25;
            this.cbRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 66);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(107, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "RUN ON STARTUP:";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.cbAutoConnect);
            this.tabPage2.Controls.Add(this.Label6);
            this.tabPage2.Controls.Add(this.tPriority);
            this.tabPage2.Controls.Add(this.Label5);
            this.tabPage2.Controls.Add(this.tStopBits);
            this.tabPage2.Controls.Add(this.Label4);
            this.tabPage2.Controls.Add(this.tDataBits);
            this.tabPage2.Controls.Add(this.Label3);
            this.tabPage2.Controls.Add(this.tRate);
            this.tabPage2.Controls.Add(this.Label2);
            this.tabPage2.Controls.Add(this.tPort);
            this.tabPage2.Controls.Add(this.Label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(343, 268);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "PORT Settings";
            // 
            // cbAutoConnect
            // 
            this.cbAutoConnect.AutoSize = true;
            this.cbAutoConnect.Location = new System.Drawing.Point(156, 241);
            this.cbAutoConnect.Margin = new System.Windows.Forms.Padding(4);
            this.cbAutoConnect.Name = "cbAutoConnect";
            this.cbAutoConnect.Size = new System.Drawing.Size(15, 14);
            this.cbAutoConnect.TabIndex = 45;
            this.cbAutoConnect.UseVisualStyleBackColor = true;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(25, 241);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(95, 13);
            this.Label6.TabIndex = 44;
            this.Label6.Text = "AUTO CONNECT:";
            // 
            // tPriority
            // 
            this.tPriority.Location = new System.Drawing.Point(156, 195);
            this.tPriority.Margin = new System.Windows.Forms.Padding(4);
            this.tPriority.Name = "tPriority";
            this.tPriority.Size = new System.Drawing.Size(165, 20);
            this.tPriority.TabIndex = 43;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(71, 199);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(49, 13);
            this.Label5.TabIndex = 42;
            this.Label5.Text = "PARITY:";
            // 
            // tStopBits
            // 
            this.tStopBits.Location = new System.Drawing.Point(156, 150);
            this.tStopBits.Margin = new System.Windows.Forms.Padding(4);
            this.tStopBits.Name = "tStopBits";
            this.tStopBits.Size = new System.Drawing.Size(165, 20);
            this.tStopBits.TabIndex = 41;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(53, 153);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(66, 13);
            this.Label4.TabIndex = 40;
            this.Label4.Text = "STOP BITS:";
            // 
            // tDataBits
            // 
            this.tDataBits.Location = new System.Drawing.Point(156, 102);
            this.tDataBits.Margin = new System.Windows.Forms.Padding(4);
            this.tDataBits.Name = "tDataBits";
            this.tDataBits.Size = new System.Drawing.Size(165, 20);
            this.tDataBits.TabIndex = 39;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(52, 105);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(66, 13);
            this.Label3.TabIndex = 38;
            this.Label3.Text = "DATA BITS:";
            // 
            // tRate
            // 
            this.tRate.Location = new System.Drawing.Point(156, 58);
            this.tRate.Margin = new System.Windows.Forms.Padding(4);
            this.tRate.Name = "tRate";
            this.tRate.Size = new System.Drawing.Size(165, 20);
            this.tRate.TabIndex = 37;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(48, 62);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(72, 13);
            this.Label2.TabIndex = 36;
            this.Label2.Text = "BAUD RATE:";
            // 
            // tPort
            // 
            this.tPort.Location = new System.Drawing.Point(156, 13);
            this.tPort.Margin = new System.Windows.Forms.Padding(4);
            this.tPort.Name = "tPort";
            this.tPort.Size = new System.Drawing.Size(165, 20);
            this.tPort.TabIndex = 35;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(48, 17);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(74, 13);
            this.Label1.TabIndex = 34;
            this.Label1.Text = "PORT NAME:";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.txtApiStatus);
            this.tabPage3.Controls.Add(this.btnValidate);
            this.tabPage3.Controls.Add(this.txtKey);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.txtServer);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(343, 268);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Server Settings";
            // 
            // txtApiStatus
            // 
            this.txtApiStatus.AutoSize = true;
            this.txtApiStatus.Location = new System.Drawing.Point(156, 146);
            this.txtApiStatus.Name = "txtApiStatus";
            this.txtApiStatus.Size = new System.Drawing.Size(0, 13);
            this.txtApiStatus.TabIndex = 43;
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(156, 106);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(75, 23);
            this.btnValidate.TabIndex = 42;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(156, 66);
            this.txtKey.Margin = new System.Windows.Forms.Padding(4);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(165, 20);
            this.txtKey.TabIndex = 41;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(61, 70);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 40;
            this.label8.Text = "API Key:";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(156, 21);
            this.txtServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(165, 20);
            this.txtServer.TabIndex = 39;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(48, 25);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 13);
            this.label9.TabIndex = 38;
            this.label9.Text = "Server URL:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(51, 36);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(74, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "EQUIPMENT:";
            // 
            // ddlEquipmentType
            // 
            this.ddlEquipmentType.FormattingEnabled = true;
            this.ddlEquipmentType.Location = new System.Drawing.Point(164, 36);
            this.ddlEquipmentType.Name = "ddlEquipmentType";
            this.ddlEquipmentType.Size = new System.Drawing.Size(121, 21);
            this.ddlEquipmentType.TabIndex = 27;
            // 
            // PortSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 356);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.bSave);
            this.Controls.Add(this.bCancle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PortSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Port Setting";
            this.Load += new System.EventHandler(this.LISSettings_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Button bSave;
        internal System.Windows.Forms.Button bCancle;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        internal System.Windows.Forms.CheckBox cbRunOnStartup;
        internal System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabPage2;
        internal System.Windows.Forms.CheckBox cbAutoConnect;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.TextBox tPriority;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.TextBox tStopBits;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.TextBox tDataBits;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.TextBox tRate;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox tPort;
        internal System.Windows.Forms.Label Label1;
        private System.Windows.Forms.TabPage tabPage3;
        internal System.Windows.Forms.TextBox txtKey;
        internal System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox txtServer;
        internal System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.Label txtApiStatus;
        private System.Windows.Forms.ComboBox ddlEquipmentType;
        internal System.Windows.Forms.Label label10;
    }
}