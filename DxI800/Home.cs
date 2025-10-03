using LisConsole.Properties;
using LIS.Com.Businesslogic;
using LIS.DtoModel;
using LIS.Logger;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LisConsole
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

            var settings = new PortSettings();

            settings.AutoConnect = Settings.Default.AUTO_CONNECT;
            settings.BaudRate = Settings.Default.BAUD_RATE;
            settings.DataBits = Settings.Default.DATA_BITS;
            settings.PortName = Settings.Default.PORT_NAME;
            settings.Parity = Settings.Default.PARITY;
            settings.StopBits = Settings.Default.STOP_BITS;
            selectedEquipment = (EquipmentType)Enum.Parse(typeof(EquipmentType), Settings.Default.EQUIPMENT_TYPE);
            LisContext.LisDOM.InitSerialCommand(settings, selectedEquipment);
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
            if (LisContext.LisDOM.IsCommandReady)
            {
                if (LisContext.LisDOM.Command.IsReady)
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

        private void CenterControlInParent(Control ctrlToCenter)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;
            ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ConnectCOM();
        }

        private void ConnectCOM()
        {
            Logger.LogInstance.LogDebug("LisConsole ConnectCOM started.");
            Logger.LogInstance.LogDebug($"ConnectCOM IsCommandReady : {LisContext.LisDOM.IsCommandReady}");
            if (LisContext.LisDOM.IsCommandReady)
            {
                Logger.LogInstance.LogDebug($"ConnectCOM IsReady : {LisContext.LisDOM.Command.IsReady}");
                if (!LisContext.LisDOM.Command.IsReady)
                {
                    LisContext.LisDOM.Command.ConnectToCOMPort();
                    if (!LisContext.LisDOM.Command.IsReady)
                    {
                        Logger.LogInstance.LogInfo(LisContext.LisDOM.Command.Message);
                        MessageBox.Show(this, LisContext.LisDOM.Command.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Logger.LogInstance.LogInfo($"{Settings.Default.PORT_NAME} port connected.");
                    }
                }
                else
                {
                    LisContext.LisDOM.Command.DisconnectToCOMPort();
                    Logger.LogInstance.LogInfo($"{Settings.Default.PORT_NAME} port disconnected.");
                }
            }

            this.InitLIS();
            Logger.LogInstance.LogDebug("LisConsole ConnectCOM completed.");
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
            this.ConnectCOM();
        }

        void MenuQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            Logger.LogInstance.LogInfo(selectedEquipment + " Started.");
        }
    }
}
