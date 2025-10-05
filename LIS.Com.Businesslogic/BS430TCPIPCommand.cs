using LIS.DtoModel;
using LIS.DtoModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class BS430TCPIPCommand : TCPIPHL7Command
    {
        public BS430TCPIPCommand(TCPIPSettings _settings) : base(_settings)
        { }

        public override async Task<string> ProccessMessage(string sampleNo, string rawMessage, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("BS430 ProccessMessage method started");
            string[] resultMesgSegments = rawMessage.TrimEnd((char)13).Split((char)13); // <CR>

            await SaveResult(sampleNo, resultMesgSegments);
            Logger.Logger.LogInstance.LogDebug("All the mandatory tags are present in the ORU message");
            string response = @"MSH|^~\&|||||" + DateTime.Now.ToString("yyyyMMddhhmmss") +
                "||ACK^R01|1|P|2.3.1||||0||ASCII||" + (char)13 +
                $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            return response;
        }

        private async Task SaveResult(string sampleNo, string[] resultMesgSegments)
        {
            Result result = new Result();
            List<TestResultDetails> lsResult = new List<TestResultDetails>();
            TestResult testResult = new TestResult
            {
                ResultDate = DateTime.Now,
                SampleNo = sampleNo,
            };
            for (int i = 0; i < resultMesgSegments.Length; i++)
            {
                string[] field = resultMesgSegments[i].Split('|');

                if (field[0] == "OBX" && field[2] == "NM")
                {
                    var resultDetails = new TestResultDetails();
                    var paramCode = field[4].ToString();
                    var paramValue = field[5].ToString();
                    resultDetails.LISParamCode = paramCode;
                    resultDetails.LISParamValue = paramValue;
                    resultDetails.LISParamUnit = field[6];
                    lsResult.Add(resultDetails);
                }
            }

            result.TestResult = testResult;
            result.ResultDetails = lsResult;
            Logger.Logger.LogInstance.LogDebug("BS430 Result posted to API for SampleNo: " + testResult.SampleNo);
            await LisContext.LisDOM.SaveTestResult(result);
        }

        public override async Task<string> SendOrderData(string sampleNo, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("BS430 generateORMField method started for SampleNo: " + sampleNo);
            string datetime = DateTime.Now.ToString("yyyyMMddhhmmss");
            string specialchar = @"^~\&";
            string message_MSH = $"MSH|{specialchar}|||||{datetime}||DSR^Q03|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
            string message_MSA = $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            string message_err = $"ERR|0|{(char)13}";
            string message_qak = string.Empty;
            string message_DSP = string.Empty;
            bool flag = IsValidSampleNo(sampleNo);
            string ORMMessage;
            if (flag)
            {
                IEnumerable<TestRequestDetail> testlist = await LisContext.LisDOM.GetTestRequestDetails(sampleNo);
                if (testlist != null && testlist.Count() > 0)
                {
                    var firstTest = testlist.First();
                    var specimen = firstTest.SpecimenName.ToLower();
                    string gender = "";
                    switch (firstTest.Patient.Gender)
                    {
                        case "MALE":
                            gender = "M";
                            break;
                        case "FEMALE":
                            gender = "F";
                            break;
                        default:
                            gender = "O";
                            break;
                    }

                    string DOB = firstTest.Patient.DateOfBirth.ToString("yyyyMMddhhmmss");
                    var name = firstTest.Patient?.Name;
                    if (name.Length > 32)
                    {
                        name = name.Substring(0, 30);
                    }
                    for (int i = 1; i <= 28; i++)
                    {

                        switch (i)
                        {
                            case 3:
                                message_DSP += $"DSP|{i}||{name}|||{(char)13}";
                                break;
                            case 4:
                                message_DSP += $"DSP|{i}||{DOB}|||{(char)13}";
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
                    for (int i = 29; i <= 29 + testlist.Count(); i++)
                    {
                        var test = testlist.ElementAt(i);
                        await LisContext.LisDOM.AcknowledgeSample(test.Id);
                        var testname = test.LISTestCode + "^^^";
                        message_DSP += $"DSP|{i}||{testname}|||{(char)13}";
                    }
                    message_qak = $"QAK|SR|OK|{(char)13}";
                    string message_QRD = $"QRD|{datetime}|R|D|54|||RD|{sampleNo}|OTH|||T|{(char)13}";
                    string message_QRF = $"QRF||{datetime}|{datetime}|||RCT|COR|ALL||{(char)13}";
                    string message_DSC = $"DSC||{(char)13}";

                    ORMMessage = message_MSH + message_MSA + message_err + message_qak + message_QRD + message_QRF + message_DSP + message_DSC;
                }
                else
                {
                    message_MSH = $"MSH|{specialchar}|||||{datetime}||QCK^Q02|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
                    message_qak = $"QAK|SR|NF|{(char)13}";
                    ORMMessage = message_MSH + message_MSA + message_err + message_qak;
                }
            }
            else
            {
                message_MSH = $"MSH|{specialchar}|||||{datetime}||QCK^Q02|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
                message_qak = $"QAK|SR|NF|{(char)13}";
                ORMMessage = message_MSH + message_MSA + message_err + message_qak;
            }

            ORMMessage = AddHeaderAndFooterToHL7Msg(ORMMessage);
            return ORMMessage;
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