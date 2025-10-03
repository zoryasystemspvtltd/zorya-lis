using LisConsole.Properties;
using LIS.Com.Businesslogic;
using LIS.DtoModel;
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace LisConsole
{
    public partial class PortSetting : Form
    {
        // The path to the key where Windows looks for startup applications
        RegistryKey startupApp = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        bool isValidAPI = false;
        public PortSetting()
        {
            InitializeComponent();
            if (startupApp.GetValue("LisConsole") == null)
            {
                // The value doesn't exist, the application is not set to run at startup
                //chkRun.Checked = false;
            }
            else
            {
                // The value exists, the application is set to run at startup
                //chkRun.Checked = true;
            }
        }

        private void LISSettings_Load(object sender, EventArgs e)
        {
            tPort.Text = Settings.Default.PORT_NAME;
            tRate.Text = Settings.Default.BAUD_RATE.ToString();
            tDataBits.Text = Settings.Default.DATA_BITS.ToString();
            tStopBits.Text = Settings.Default.STOP_BITS.ToString();
            tPriority.Text = Settings.Default.PARITY.ToString();
            cbAutoConnect.Checked = Settings.Default.AUTO_CONNECT;
            cbRunOnStartup.Checked = Settings.Default.RUN_ON_STARTUP;
            txtServer.Text = Settings.Default.SERVER_URL;
            txtKey.Text = Settings.Default.API_KEY;

            ddlEquipmentType.DataSource = Enum.GetNames(typeof(EquipmentType));
            var selectedEquipment = (EquipmentType)Enum.Parse(typeof(EquipmentType), Settings.Default.EQUIPMENT_TYPE);
            foreach (string item in ddlEquipmentType.Items)
            {
                var enumItem = (EquipmentType)Enum.Parse(typeof(EquipmentType), item);
                if (enumItem == selectedEquipment)
                {
                    ddlEquipmentType.SelectedItem = item;
                    break;
                }
            }
            //ddlEquipmentType.SelectedValue = selectedEquipment;
        }
        private void bCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void bSave_Click(object sender, EventArgs e)
        {
            if (!isValidAPI)
            {
                LisContext.LisDOM.InitAPI(txtServer.Text, txtKey.Text);
                isValidAPI = await LisContext.LisDOM.PingAPI();
            }

            if (!isValidAPI)
            {
                MessageBox.Show(this, "Invalid API Details", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(tPort.Text))
            {
                MessageBox.Show(this, "Invalid value in PORT NAME", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(tRate.Text))
            {
                MessageBox.Show(this, "Invalid value in BAUD RATE", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(tDataBits.Text))
            {
                MessageBox.Show(this, "Invalid value in DATA BITS", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(tStopBits.Text))
            {
                MessageBox.Show(this, "Invalid value in STOP BITS", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(tPriority.Text))
            {
                MessageBox.Show(this, "Invalid value in PARITY", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show(this, "Invalid value in Server URL", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtKey.Text))
            {
                MessageBox.Show(this, "Invalid value in API Key", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Settings.Default.PORT_NAME = tPort.Text;
            Settings.Default.BAUD_RATE = Convert.ToInt32(tRate.Text);
            Settings.Default.DATA_BITS = Convert.ToInt32(tDataBits.Text);
            Settings.Default.STOP_BITS = Convert.ToInt32(tStopBits.Text);
            Settings.Default.PARITY = Convert.ToInt32(tPriority.Text);
            Settings.Default.AUTO_CONNECT = cbAutoConnect.Checked;
            Settings.Default.SERVER_URL = txtServer.Text;
            Settings.Default.API_KEY = txtKey.Text;
            Settings.Default.EQUIPMENT_TYPE = (string)ddlEquipmentType.SelectedValue;
            if (cbRunOnStartup.Checked)
            {
                // Add the value in the registry so that the application runs at startup
                startupApp.SetValue("LisConsole", Application.ExecutablePath);
                Settings.Default.RUN_ON_STARTUP = true;
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                startupApp.DeleteValue("LisConsole", false);
                Settings.Default.RUN_ON_STARTUP = false;
            }

            var settings = new PortSettings();

            settings.AutoConnect = Settings.Default.AUTO_CONNECT;
            settings.BaudRate = Settings.Default.BAUD_RATE;
            settings.DataBits = Settings.Default.DATA_BITS;
            settings.PortName = Settings.Default.PORT_NAME;
            settings.Parity = Settings.Default.PARITY;
            settings.StopBits = Settings.Default.STOP_BITS;

            Settings.Default.Save();
            this.Close();
        }

        private async void btnValidate_Click(object sender, EventArgs e)
        {
            LisContext.LisDOM.InitAPI(txtServer.Text, txtKey.Text);
            isValidAPI = await LisContext.LisDOM.PingAPI();

            if (!isValidAPI)
            {
                MessageBox.Show(this, "Invalid API Details", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtApiStatus.Text = "Success...";
        }
    }
}
