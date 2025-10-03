using LisTCPIPConsole.Properties;
using LIS.Com.Businesslogic;
using LIS.DtoModel;
using LIS.Logger;
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

            var settings = new TCPIPSettings();

            settings.AutoConnect = Settings.Default.AUTO_CONNECT;
            settings.IPAddress = Settings.Default.IP_ADDRESS;
            settings.PortNo = Settings.Default.PORT_NO;
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

            if (LisContext.LisDOM.TcpIpCommand.IsReady)
            {
                ConnectToolStripMenuItem.Text = "Disconnect";
                connectItem.Text = "Disconnect";
                comPortName = " :: Status - Equipment connected.";
            }
            else
            {
                ConnectToolStripMenuItem.Text = "Connect";
                connectItem.Text = "Connect";
                comPortName = " :: Status - Equipment not connected.";
            }


            heartBeatProxy.WatchHeartBeat();
            this.Text = $"{selectedEquipment} {comPortName}";
            Logger.LogInstance.LogDebug("Lis Console InitLIS method completed");
        }

        private void CMain_OnLisControlEvent(object sender, LisEventArgs e)
        {
            switch (e.Name)
            {
                case "CANCEL":
                    this.InitLIS();
                    break;
                case "OK":
                    this.InitLIS();
                    break;
                default:
                    this.InitLIS();
                    break;
            }

        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ConnectTCPIP();
        }

        private void ConnectTCPIP()
        {
            Logger.LogInstance.LogDebug("LisConsole ConnectTCPIP started.");
            Logger.LogInstance.LogDebug($"ConnectTCPIP IsReady : {LisContext.LisDOM.TcpIpCommand.IsReady}");
            if (!LisContext.LisDOM.TcpIpCommand.IsReady)
            {
                LisContext.LisDOM.TcpIpCommand.ConnectToTCPIP();
                if (!LisContext.LisDOM.TcpIpCommand.IsReady)
                {
                    Logger.LogInstance.LogInfo(LisContext.LisDOM.TcpIpCommand.FullMessage);
                    MessageBox.Show(this, LisContext.LisDOM.TcpIpCommand.FullMessage, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address connected.");
                }
            }
            else
            {
                LisContext.LisDOM.TcpIpCommand.DisconnectToTCPIP();
                Logger.LogInstance.LogInfo($"{Settings.Default.IP_ADDRESS} IP Address disconnected.");
            }

            this.InitLIS();
            Logger.LogInstance.LogDebug("LisConsole ConnectTCPIP completed.");
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            Application.Exit();
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

        private void addUserToolStripMenuItem_Click(object sender, EventArgs e)
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
            LisContext.LisDOM.TcpIpCommand.DisconnectToTCPIP();
            Application.Exit();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            Logger.LogInstance.LogInfo(selectedEquipment + " Started.");
        }

        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            LisContext.LisDOM.TcpIpCommand.DisconnectToTCPIP();
            Application.Exit();
        }
    }
}