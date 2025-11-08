using LIS.DtoModel;
using LIS.DtoModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class CL1200iTCPIPHL7Command : TCPIPHL7Command
    {
        public CL1200iTCPIPHL7Command(TCPIPSettings _settings) : base(_settings)
        { }

        public override async Task<string> ProccessMessage(string sampleNo, string rawMessage, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("CL1200i ProccessMessage method started");
            string[] resultMesgSegments = rawMessage.TrimEnd((char)13).Split((char)13); // <CR>

            await SaveResult(sampleNo, resultMesgSegments);
            Logger.Logger.LogInstance.LogDebug("All the mandatory tags are present in the ORU message");
            string response = @"MSH|^~\&|||||" + DateTime.Now.ToString("yyyyMMddhhmmss") +
                "||ACK^R01|" + messageControlId + "|P|2.3.1||||0||ASCII|||" + (char)13 +
                $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            return response;
        }

        private async Task SaveResult(string sampleNo, string[] resultMesgSegments)
        {
            List<LisTestValue> lsResult = new List<LisTestValue>();
            for (int i = 0; i < resultMesgSegments.Length; i++)
            {
                string[] field = resultMesgSegments[i].Split('|');

                if (field[0].Trim() == "OBX" && field[2] == "NM")
                {
                    var resultDetails = new LisTestValue();
                    var paramCode = field[4].ToString();
                    var paramValue = field[5].ToString();
                    resultDetails.PARAMCODE = paramCode;
                    resultDetails.Value = paramValue;
                    resultDetails.REF_VISITNO = sampleNo;
                    lsResult.Add(resultDetails);
                }
            }

            Logger.Logger.LogInstance.LogDebug("CL1200i Result posted to API for SampleNo: " + lsResult[0].REF_VISITNO);
            await LisContext.LisDOM.SaveTestResult(lsResult);
        }

        public override async Task<OrderHL7Response> SendOrderData(string sampleNo, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("CL1200i generateORMField method started for SampleNo: " + sampleNo);
            string datetime = DateTime.Now.ToString("yyyyMMddhhmmss");
            string specialchar = @"^~\&";
            string message_MSH = $"MSH|{specialchar}|||||{datetime}||DSR^Q03|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
            string message_MSA = $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            string message_err = $"ERR|0|{(char)13}";
            string message_qak = string.Empty;
            string message_DSP = string.Empty;
            string DSRMessage, QRYMessage;
            var response = new OrderHL7Response();

            IEnumerable<AccuHealthSample> testlist = await LisContext.LisDOM.GetTestRequestDetails(sampleNo);
            if (testlist != null && testlist.Count() > 0)
            {

                var firstTest = testlist.First();
                var specimen = firstTest.SPECIMEN.ToLower();
                var name = firstTest.PATFNAME;
                var gender = firstTest.GENDER;
                var dob = firstTest.PAT_DOB;
                if (dob != null)
                {
                    dob = Convert.ToDateTime(dob).ToString("yyyyMMddhhmmss");
                }
                else
                {
                    dob = "";
                }
                if (name.Length > 40)
                {
                    name = name.Substring(0, 39);
                }
                for (int i = 1; i <= 28; i++)
                {

                    switch (i)
                    {
                        case 3:
                            message_DSP += $"DSP|{i}||{name}|||{(char)13}";
                            break;
                        case 4:
                            message_DSP += $"DSP|{i}||{dob}|||{(char)13}";
                            break;
                        case 5:
                            message_DSP += $"DSP|{i}||{gender}|||{(char)13}";
                            break;
                        case 21:
                            message_DSP += $"DSP|{i}||{sampleNo}|||{(char)13}";
                            break;
                        case 24:
                            message_DSP += $"DSP|{i}||N|||{(char)13}";
                            break;
                        case 26:
                            message_DSP += $"DSP|{i}||{specimen}|||{(char)13}";
                            break;
                        default:
                            message_DSP += $"DSC||{(char)13}";
                            break;
                    }
                }
                for (int i = 0; i < testlist.Count(); i++)
                {
                    int j = 29 + i;
                    var test = testlist.ElementAt(i);
                    var testname = test.LisParamCode + "^^^";
                    message_DSP += $"DSP|{j}||{testname}|||{(char)13}";
                }
                message_qak = $"QAK|SR|OK|{(char)13}";
                string message_QRD = $"QRD|{datetime}|R|D|54|||RD|{sampleNo}|OTH|||T|{(char)13}";
                string message_QRF = $"QRF||{datetime}|{datetime}|||RCT|COR|ALL||{(char)13}";
                string message_DSC = $"DSC||{(char)13}";

                DSRMessage = message_MSH + message_MSA + message_err + message_qak + message_QRD + message_QRF + message_DSP + message_DSC;
                DSRMessage = AddHeaderAndFooterToHL7Msg(DSRMessage);

                QRYMessage = SendResponse("OK", messageControlId);
                response.QRYResponse = QRYMessage;
                response.DSRResponse = DSRMessage;
                return response;
            }
            else
            {
                QRYMessage = SendResponse("NF", messageControlId);
                response.QRYResponse = QRYMessage;
                response.DSRResponse = null;
                return response;
            }
        }

        public override string SendResponse(string qak, string messageControlId)
        {
            string datetime = DateTime.Now.ToString("yyyyMMddhhmmss");
            string specialchar = @"^~\&";
            string message_MSH = $"MSH|{specialchar}|||||{datetime}||QCK^Q02|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
            string message_MSA = $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            string message_err = $"ERR|0|{(char)13}";
            string message_qak = $"QAK|SR|{qak}|{(char)13}";
            
            var response = message_MSH + message_MSA + message_err + message_qak;
            return AddHeaderAndFooterToHL7Msg(response);
        }

        public string AddHeaderAndFooterToHL7Msg(string RawMessage)
        {
            char BeginFormat = (char)11; //Strings.ChrW(0xB); //VT
            char EndFormat1 = (char)28;//Strings.ChrW(0x1C); //FS
            char EndFormat2 = (char)13; //CR

            string NwkMessage = RawMessage.PadLeft(RawMessage.Length + 1, BeginFormat);
            string NwkMessage1 = NwkMessage.PadRight(NwkMessage.Length + 1, EndFormat1);
            string NwkMessage2 = NwkMessage1.PadRight(NwkMessage1.Length + 1, EndFormat2);

            return NwkMessage2;
        }
    }
}