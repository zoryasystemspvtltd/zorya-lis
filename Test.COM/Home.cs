using LIS.Logger;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.COM
{
    public partial class Home : Form
    {
        private SerialPort port;
        private TcpClient client;
        private Thread clientThread;
        private readonly Random _random = new Random(1024);
        public Home()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (rdbSerialPort.Checked)
                {
                    port = new SerialPort(comboBox1.Text)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        StopBits = (StopBits)1,
                        Parity = (Parity)0
                    };
                    port.Open();
                    port.DataReceived += Port_DataReceived;
                    port.DiscardOutBuffer();
                    port.DiscardInBuffer();
                    this.Invoke((MethodInvoker)delegate
                    {
                        Logger.LogInstance.LogInfo("PORT Connected.");
                    });
                }
                else
                {
                    client = new TcpClient();
                    client.ConnectAsync(txtIPAddress.Text, Convert.ToInt32(txtPortNo.Text));
                    Logger.LogInstance.LogInfo("TCP/IP Connected.");
                }
                btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                client.Close();
                throw ex;
            }
        }
        private void TCP_HL7SendData()
        {
            if (!client.Connected)
                client = new TcpClient(txtIPAddress.Text, Convert.ToInt32(txtPortNo.Text));

            Stream sm = client.GetStream();
            try
            {
                StreamReader sr = new StreamReader(sm);
                StreamWriter sw = new StreamWriter(sm);
                sw.AutoFlush = true;
                string specialchar = @"^~\&";
                //string msg = $"MSH|{specialchar}|LabXpert|Mindray|||20230505175421||ORM^O01|8901|P|2.3.1||||||UNICODE{Constants.vbCr}ORC|RF||1636843E|BL||1^1|1|||||||||NW{Constants.vbCr}{Strings.ChrW(0x1C)}{Constants.vbCr}";
                string msg = $"MSH|{specialchar}|||||20251004165702||QRY^Q02|4|P|2.3.1||||||ASCII|||{Constants.vbCr}QRD|20120508104700|R|D|1|||RD|300000|OTH|||T|{Constants.vbCr}QRF||||||RCT|COR|ALL||{Constants.vbCr}{Strings.ChrW(0x1C)}{Constants.vbCr}";
                //string msg = $"MSH|{specialchar}|LabXpert|Mindray|||20230506013045||ORU^R01|9048|P|2.3.1||||||UNICODE{Constants.vbCr}PID|1||^^^^MR{Constants.vbCr}PV1|1{Constants.vbCr}OBR|1||1770190e|00001^Automated Count^99MRC|||20230506013024|||||||||||||||||HM|NotValidated|||||||Administrator{Constants.vbCr}OBX|1|IS|08001^Take Mode^99MRC||A||||||F{Constants.vbCr}OBX|2|IS|08002^Blood Mode^99MRC||W||||||F{Constants.vbCr}OBX|3|IS|08003^Test Mode^99MRC||CBC+DIFF||||||F{Constants.vbCr}OBX|4|IS|01002^Ref Group^99MRC||General||||||F{Constants.vbCr}OBX|5|IS|05007^Project Type^99MRC||BL||||||F{Constants.vbCr}OBX|6|ST|01012^Shelf No^99MRC||1||||||F{Constants.vbCr}OBX|7|ST|01013^Tube No^99MRC||1||||||F{Constants.vbCr}OBX|8|ST|09001^Analyzer^99MRC||BC-6200_1||||||F{Constants.vbCr}OBX|9|ST|09003^SN^99MRC||TW-02000906||||||F{Constants.vbCr}OBX|10|ST|09999^AuditResult^99MRC||Review||||||F{Constants.vbCr}OBX|11|NM|6690-2^WBC^LN||10.42|10*3/uL|4.00-10.00|H~N|||F{Constants.vbCr}OBX|12|NM|751-8^NEU#^LN||5.49|10*3/uL|2.00-7.00|N|||F{Constants.vbCr}OBX|13|NM|731-0^LYM#^LN||3.80|10*3/uL|0.80-4.00|N|||F{Constants.vbCr}OBX|14|NM|742-7^MON#^LN||0.81|10*3/uL|0.12-1.20|N|||F{Constants.vbCr}OBX|15|NM|711-2^EOS#^LN||0.29|10*3/uL|0.02-0.50|N|||F{Constants.vbCr}OBX|16|NM|704-7^BAS#^LN||0.03|10*3/uL|0.00-0.10|N|||F{Constants.vbCr}OBX|17|NM|51584-1^IMG#^LN||0.03|10*3/uL|0.00-999.99|N|||F{Constants.vbCr}OBX|18|NM|770-8^NEU%^LN||52.6|%|50.0-70.0|N|||F{Constants.vbCr}OBX|19|NM|736-9^LYM%^LN||36.5|%|20.0-40.0|N|||F{Constants.vbCr}OBX|20|NM|5905-5^MON%^LN||7.8|%|3.0-12.0|N|||F{Constants.vbCr}OBX|21|NM|713-8^EOS%^LN||2.8|%|0.5-5.0|N|||F{Constants.vbCr}OBX|22|NM|706-2^BAS%^LN||0.3|%|0.0-1.0|N|||F{Constants.vbCr}OBX|23|NM|38518-7^IMG%^LN||0.3|%|0.0-100.0|N|||F{Constants.vbCr}OBX|24|NM|789-8^RBC^LN||4.55|10*6/uL|3.50-5.50|N|||F{Constants.vbCr}OBX|25|NM|718-7^HGB^LN||12.4|g/dL|11.0-16.0|N|||F{Constants.vbCr}OBX|26|NM|4544-3^HCT^LN||37.5|%|37.0-54.0|N|||F{Constants.vbCr}OBX|27|NM|787-2^MCV^LN||82.4|fL|80.0-100.0|N|||F{Constants.vbCr}OBX|28|NM|785-6^MCH^LN||27.3|pg|27.0-34.0|N|||F{Constants.vbCr}OBX|29|NM|786-4^MCHC^LN||33.1|g/dL|32.0-36.0|N|||F{Constants.vbCr}OBX|30|NM|788-0^RDW-CV^LN||14.4|%|11.0-16.0|N|||F{Constants.vbCr}OBX|31|NM|21000-5^RDW-SD^LN||44.2|fL|35.0-56.0|N|||F{Constants.vbCr}OBX|32|NM|777-3^PLT^LN||232|10*3/uL|100-300|N|||F{Constants.vbCr}OBX|33|NM|32623-1^MPV^LN||12.6|fL|6.5-12.0|H~N|||F{Constants.vbCr}OBX|34|NM|32207-3^PDW^LN||16.5||15.0-17.0|N|||F{Constants.vbCr}OBX|35|NM|10002^PCT^99MRC||0.293|%|0.108-0.282|H~N|||F{Constants.vbCr}OBX|36|NM|10013^PLCC^99MRC||101|10*9/L|30-90|H~N|||F{Constants.vbCr}OBX|37|NM|10014^PLCR^99MRC||43.3|%|11.0-45.0|N|||F{Constants.vbCr}OBX|38|NM|30392-5^NRBC#^LN||0.000|10*3/uL|0.000-9999.999|N|||F{Constants.vbCr}OBX|39|NM|26461-4^NRBC%^LN||0.00|/100WBC|0.00-9999.99|N|||F{Constants.vbCr}OBX|40|NM|10022^PLT-I^99MRC||232|10*9/L||N|||F{Constants.vbCr}OBX|41|NM|10024^WBC-D^99MRC||10.22|10*9/L||N|||F{Constants.vbCr}OBX|42|NM|10052^TNC-D^99MRC||10.22|10*9/L||N|||F{Constants.vbCr}OBX|43|NM|10026^WBC-N^99MRC||10.42|10*9/L||N|||F{Constants.vbCr}OBX|44|NM|10059^TNC-N^99MRC||10.42|10*9/L||N|||F{Constants.vbCr}OBX|45|NM|10020^HFC#^99MRC||0.03|10*9/L||N|||F{Constants.vbCr}OBX|46|NM|10021^HFC%^99MRC||0.3|%||N|||F{Constants.vbCr}OBX|47|NM|10054^IME%^99MRC||0.0|%||N|||F{Constants.vbCr}OBX|48|NM|10053^IME#^99MRC||0.00|10*9/L||N|||F{Constants.vbCr}OBX|49|NM|10055^H-NR%^99MRC||****|%||N|||F{Constants.vbCr}OBX|50|NM|10056^L-NR%^99MRC||****|%||N|||F{Constants.vbCr}OBX|51|NM|10057^NLR^99MRC||1.44|||N|||F{Constants.vbCr}OBX|52|NM|10058^PLR^99MRC||61.12|||N|||F{Constants.vbCr}OBX|53|NM|10032^InR#^99MRC||0.00|10*9/L||N|||F{Constants.vbCr}OBX|54|NM|10033^InR‰^99MRC||0.00|‰||N|||F{Constants.vbCr}OBX|55|NM|15199-3^Micro#^LN||0.20|10*12/L||N|||F{Constants.vbCr}OBX|56|NM|10042^Micro%^99MRC||4.5|%||N|||F{Constants.vbCr}OBX|57|NM|15198-5^Macro#^LN||0.08|10*12/L||N|||F{Constants.vbCr}OBX|58|NM|10040^Macro%^99MRC||1.7|%||N|||F{Constants.vbCr}OBX|59|NM|10031^PDW-SD^99MRC||17.3|fL||N|||F{Constants.vbCr}OBX|60|NM|12227-5^CORRECTED WBC^LN||10.42|10*3/uL|4.00-10.00|H~N|||F{Constants.vbCr}{Strings.ChrW(0x1C)}{Constants.vbCr}";
                string writemsg = ReplaceSpecialCharecter(msg, false);
                //string writemsg = ReplaceSpecialCharecter(textBox1.Text, false);
                //char EndFormat2 = Strings.ChrW(0xD);
                //var writemessages = writemsg.Split(EndFormat2);
                //foreach (var item in writemessages)
                //{
                sw.Write(writemsg);
                string rawmsg = ReplaceSpecialCharecter(writemsg, true);
                Logger.LogInstance.LogInfo("Write :" + rawmsg);
                //}


                StringBuilder messages = new StringBuilder();
                char[] charArray = new char[1024];
                while (true)
                {
                    var readByteCount = sr.Read(charArray, 0, charArray.Length);
                    if (readByteCount > 0)
                    {
                        var rawmsg1 = new string(charArray, 0, readByteCount);
                        Logger.LogInstance.LogInfo("Read :" + ReplaceSpecialCharecter(rawmsg1, false));
                    }
                }
            }

            catch (ArgumentNullException ane)
            {
                Logger.LogInstance.LogError("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Logger.LogInstance.LogError("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Logger.LogInstance.LogError("Unexpected exception : {0}", e.ToString());
            }
            finally
            {
                clientThread.Abort();
                sm.Close();
            }

        }

        private async Task TCP_ASTMSendData()
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    client = new TcpClient();
                    await client.ConnectAsync(txtIPAddress.Text, Convert.ToInt32(txtPortNo.Text));
                }

                NetworkStream stream = client.GetStream();
                string msg = textBox1.Text;
                string writemsg = ReplaceSpecialCharecter(msg, false);
                byte[] writeData = Encoding.ASCII.GetBytes(writemsg);
                await stream.WriteAsync(writeData, 0, writeData.Length);
                //string rawmsg = ReplaceSpecialCharecter(writemsg, true);
                Logger.LogInstance.LogInfo("Write :" + msg);
                while (true)
                {
                    var data = new byte[1024];
                    int bytesRead = await stream.ReadAsync(data, 0, data.Length);

                    if (bytesRead > 0)
                    {
                        string response = Encoding.ASCII.GetString(data, 0, bytesRead);
                        Logger.LogInstance.LogInfo("Read :" + response);
                    }
                }
            }

            catch (ArgumentNullException ane)
            {
                Logger.LogInstance.LogError("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Logger.LogInstance.LogError("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Logger.LogInstance.LogError("Unexpected exception : {0}", e.ToString());
            }
            finally
            {
                if (client?.Connected == true)
                {
                    client.Close();
                }
            }

        }
        /// <summary>
        /// ENQ or (char)5 -enquiry
        /// ACK or (char)6 -acknowledge
        /// STX or (char)2- start of text
        /// ETX or (char)3- end of text
        /// ETB or (char)17- end of text
        /// EOT or (char)4 - end of transmission
        /// NAK or (char)21 - negative acknowledge
        /// DLE or (char)10 - data link escape 
        /// CR	carriage return
        /// </summary>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var text = port.ReadExisting().ToString();

            if (text == "" + (char)4)
            {
                text = "<EOT>";
            }
            else if (text == "" + (char)5)
            {
                text = "<ENQ>";
                if (rdbTCPIP.Checked)
                {
                    port.Write("" + (char)6);

                    this.Invoke((MethodInvoker)delegate
                    {
                        Logger.LogInstance.LogInfo("Write :<ACK>");
                    });
                }
            }
            else if (text == "" + (char)6)
            {
                text = "<ACK>";
            }
            else if (text == "" + (char)21)
            {
                text = "<NAK>";
            }
            else
            {
                if (rdbTCPIP.Checked)
                {
                    port.Write("" + (char)6);

                    this.Invoke((MethodInvoker)delegate
                    {
                        Logger.LogInstance.LogInfo("Write :<ACK>");
                    });
                }
            }

            this.Invoke((MethodInvoker)delegate
            {
                Logger.LogInstance.LogInfo("Read :" + text);
            });

        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (rdbSerialPort.Checked)
            {
                var response = ReplaceSpecialCharecter(textBox1.Text, false);
                port.Write(response);
                var text = ReplaceSpecialCharecter(response, false);
                Logger.LogInstance.LogInfo("Write :" + text);
            }
            else
            {
                //test test HL7 uncomment the below method
                //clientThread = new Thread(new ThreadStart(TCP_HL7SendData));
                clientThread = new Thread(new ThreadStart(RunTCPAsyncMethod));
                clientThread.Start();
            }
        }

        private void RunTCPAsyncMethod()
        {
            TCP_ASTMSendData().GetAwaiter().GetResult();  // Blocking wait, not recommended
        }
        private string ReplaceSpecialCharecter(string text, bool reverse)
        {
            /*
                <ACK> acknowledge (ASCII Decimal 6).
                {Constants.vbCr} carriage return (ASCII decimal 13).
                <ENQ> enquiry (ASCII Decimal 5).
                <EOT> end of transmission (ASCII decimal 4).
                <ETB> end of transmission block (ASCII Decimal 23). For use only
                <ETX> end of text (ASCII Decimal 3). Required at the end of eachrecord.
                <LF> line feed (ASCII Decimal 10).
                <NAK> negative acknowledge (ASCII Decimal 21).
                <STX> start of text (ASCII Decimal 2).
             */

            Dictionary<string, char> specialCharecters = new Dictionary<string, char>();
            specialCharecters.Add("<ACK>", (char)6);
            specialCharecters.Add("<VT>", (char)11);
            specialCharecters.Add("<FS>", (char)28);
            specialCharecters.Add("<CR>", (char)13);
            specialCharecters.Add("<ENQ>", (char)5);
            specialCharecters.Add("<EOT>", (char)4);
            specialCharecters.Add("<ETB>", (char)23);
            specialCharecters.Add("<ETX>", (char)3);
            specialCharecters.Add("<LF>", (char)10);
            specialCharecters.Add("<NAK>", (char)21);
            specialCharecters.Add("<STX>", (char)2);

            if (!reverse)
            {
                foreach (var key in specialCharecters.Keys)
                {
                    string oldValue = key;
                    string newValue = "" + specialCharecters[key];

                    text = text.Replace(oldValue, newValue);
                }
            }
            else
            {
                foreach (var key in specialCharecters.Keys)
                {
                    string oldValue = "" + specialCharecters[key];
                    string newValue = key;

                    text = text.Replace(oldValue, newValue);
                }
            }
            return text;
        }

        private void Home_Load(object sender, EventArgs e)
        {
            Logger.LogInstance.LogInfo("COM Simulator Started.");
        }

        private void rdbSerialPort_CheckedChanged(object sender, EventArgs e)
        {
            lblIPAddress.Visible = false;
            lblPortNo.Visible = false;
            txtIPAddress.Visible = false;
            txtPortNo.Visible = false;
            label1.Visible = true;
            comboBox1.Visible = true;
        }

        private void rdbTCPIP_CheckedChanged(object sender, EventArgs e)
        {
            lblIPAddress.Visible = true;
            lblPortNo.Visible = true;
            txtIPAddress.Visible = true;
            txtPortNo.Visible = true;
            label1.Visible = false;
            comboBox1.Visible = false;
        }

        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            clientThread.Abort();
            client.Close();
        }
    }
}
