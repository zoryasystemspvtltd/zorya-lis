using LIS.Com.Businesslogic;
using LIS.DtoModel;
using LIS.Logger;
using LisTCPIPConsole.Properties;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LisTCPIPConsole
{
    public partial class Home : Form
    {
        string comPortName = ":: Status - Equipment not connected.";
        EquipmentType selectedEquipment;
        ToolStripMenuItem connectItem;
        HeartBeatProxy heartBeatProxy;

        public Home()
        {
            Logger.LogInstance.LogDebug("Lis Console Home method started");
            InitializeComponent();

            var settings = new TCPIPSettings
            {
                AutoConnect = Settings.Default.AUTO_CONNECT,
                IPAddress = Settings.Default.IP_ADDRESS,
                PortNo = Settings.Default.PORT_NO,
                ProtocolName = Settings.Default.PROTOCOL_NAME
            };
            selectedEquipment = (EquipmentType)Enum.Parse(typeof(EquipmentType), Settings.Default.EQUIPMENT_TYPE);
            LisContext.LisDOM.InitTCPIPCommand(settings, selectedEquipment);
            LisContext.LisDOM.InitAPI(Settings.Default.SERVER_URL, Settings.Default.API_KEY);
            TrayMenuContext();
            Logger.LogInstance.LogDebug($"Home Autoconnect {settings.AutoConnect}.");
            heartBeatProxy = new HeartBeatProxy();
            InitLIS();

            Logger.LogInstance.LogDebug("Lis Console Home method completed");
        }


        private void TrayMenuContext()
        {
            Logger.LogInstance.LogDebug("Lis Console TrayMenuContext method started");
            this.lisNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.lisNotifyIcon.ContextMenuStrip.Items.Add("Open " + selectedEquipment, null, this.MenuShow_Click);
            connectItem = new ToolStripMenuItem("Connect", null, this.MenuConnect_Click);
            this.lisNotifyIcon.ContextMenuStrip.Items.Add(connectItem);
            this.lisNotifyIcon.ContextMenuStrip.Items.Add("Quit", null, this.MenuQuit_Click);
            Logger.LogInstance.LogDebug("Lis Console TrayMenuContext method completed");
        }

        private void InitLIS()
        {
            Logger.LogInstance.LogDebug("Lis Console InitLIS method started");
            var context = LisContext.LisDOM;
            var isASTM = Settings.Default.PROTOCOL_NAME == "ASTM";
            var isReady = isASTM ? context.TcpIpASTMCommand.IsReady : context.TcpIpHL7Command.IsReady;

            var statusText = isReady ? "Disconnect" : "Connect";
            var statusMessage = isReady ? " :: Status - Equipment connected." : " :: Status - Equipment not connected.";

            //Todo known bug, diconnect not working,henace disable the button
            if (isReady)
                ConnectToolStripMenuItem.Enabled = false;
            
            ConnectToolStripMenuItem.Text = statusText;
            connectItem.Text = statusText;
            comPortName = statusMessage;

            heartBeatProxy.WatchHeartBeat();
            this.Text = $"{selectedEquipment} {comPortName}";
            Logger.LogInstance.LogDebug("Lis Console InitLIS method completed");
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ConnectTCPIP();
        }

        private void ConnectTCPIP()
        {
            try
            {
                var context = LisContext.LisDOM;
                Logger.LogInstance.LogDebug("LisConsole ConnectTCPIP started.");
                if (Settings.Default.PROTOCOL_NAME == "HL7")
                {
                    if (!context.TcpIpHL7Command.IsReady)
                    {
                        context.TcpIpHL7Command.ConnectToTCPIP();
                        if (!context.TcpIpHL7Command.IsReady)
                        {
                            Logger.LogInstance.LogInfo(context.TcpIpHL7Command.FullMessage);
                            MessageBox.Show(this, context.TcpIpHL7Command.FullMessage, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address connected.");
                        }
                    }
                    else
                    {
                        context.TcpIpHL7Command.DisconnectToTCPIP();
                        Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address disconnected.");

                    }
                }
                else if (Settings.Default.PROTOCOL_NAME == "ASTM")
                {
                    if (!context.TcpIpASTMCommand.IsReady)
                    {
                        context.TcpIpASTMCommand.ConnectToTCPIP();
                        if (!context.TcpIpASTMCommand.IsReady)
                        {
                            Logger.LogInstance.LogInfo(context.TcpIpASTMCommand.FullMessage);
                            MessageBox.Show(this, context.TcpIpASTMCommand.FullMessage, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address connected.");
                        }
                    }
                    else
                    {
                        context.TcpIpASTMCommand.DisconnectToTCPIP();
                        Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address disconnected.");
                    }
                }
                this.InitLIS();
                Logger.LogInstance.LogDebug("LisConsole ConnectTCPIP completed.");


            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisconnectIP();
            Application.Exit();
        }

        private void DisconnectIP()
        {
            if (Settings.Default.PROTOCOL_NAME == "ASTM")
            {
                LisContext.LisDOM.TcpIpASTMCommand.DisconnectToTCPIP();
            }
            else
            {
                LisContext.LisDOM.TcpIpHL7Command.DisconnectToTCPIP();
            }
        }

        private void AboutUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new AboutUs();
            about.ShowDialog(this);
        }

        private void PortSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var portsetting = new PortSetting();

            if (portsetting.ShowDialog(this) == DialogResult.OK)
            {
                //this.ConnectCOM();
            }
        }

        private void Home_SizeChanged(object sender, EventArgs e)
        {

        }

        private void AddUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.Default.SERVER_URL);
        }

        private void Home_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                lisNotifyIcon.Visible = true;
                lisNotifyIcon.ShowBalloonTip(500);
                this.executeCOM1.txtLog.Text = "";
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                lisNotifyIcon.Visible = false;
            }
        }

        void MenuShow_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        void MenuConnect_Click(object sender, EventArgs e)
        {
            this.ConnectTCPIP();
        }

        void MenuQuit_Click(object sender, EventArgs e)
        {
            DisconnectIP();
            Application.Exit();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            Logger.LogInstance.LogInfo(selectedEquipment + " Started.");
        }

        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisconnectIP();
            Application.Exit();
        }
    }
}