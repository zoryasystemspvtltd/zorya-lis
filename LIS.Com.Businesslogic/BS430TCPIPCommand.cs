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
            string ORMMessage = string.Empty;
            string specialchar = @"^~\&";
            string message_MSH = $"MSH|{specialchar}|||||{datetime}||QRY^Q02|{messageControlId}|P|2.3.1||||||ASCII|||{(char)13}";
            string message_MSA = $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
            string message_err = $"ERR|0|{(char)13}";
            string message_qak = string.Empty;
            bool flag = IsValidSampleNo(sampleNo);
            if (flag)
            {
                IEnumerable<TestRequestDetail> testlist = await LisContext.LisDOM.GetTestRequestDetails(sampleNo);
                if (testlist != null && testlist.Count() > 0)
                {
                    string patientFirstName = "";
                    string patientLastName = "";
                    string testname = "";
                    string patientClass = "MedicalInsurance";
                    var firstTest = testlist.First();
                    string patientLocation = "Pathology^^" + firstTest.BedNo;
                    string patientId = firstTest.Patient.HisPatientId + "^^^^MR";
                    string gender = "";
                    switch (firstTest.Patient.Gender)
                    {
                        case "MALE":
                            gender = "Male";
                            break;
                        case "FEMALE":
                            gender = "Female";
                            break;
                    }

                    string DOB = firstTest.Patient.DateOfBirth.ToString("yyyyMMddhhmmss");
                    var name = firstTest.Patient?.Name.Split(' ');
                    if (name.Count() > 1)
                    {
                        if (name.Count() > 1)
                        {
                            patientFirstName = name[0];
                        }
                        if (name.Count() == 3)
                        {
                            patientLastName = name[2];
                        }
                        else if (name.Count() == 2)
                        {
                            patientLastName = name[1];
                        }
                        var fullname = patientFirstName + patientLastName;
                        if (fullname.Length > 48)
                        {
                            patientFirstName = patientFirstName.Substring(0, 1);
                        }
                    }
                    else
                    {
                        patientFirstName = firstTest.Patient?.Name;
                        if (patientFirstName.Length > 48)
                        {
                            patientFirstName = patientFirstName.Substring(0, 47);
                        }
                    }

                    for (int i = 0; i < testlist.Count(); i++)
                    {
                        var test = testlist.ElementAt(i);
                        await LisContext.LisDOM.AcknowledgeSample(test.Id);
                        testname = test.LISTestCode;
                    }
                    message_qak = $"QAK|SR|OK|{(char)13}";
                    string message_PID = $"PID|1||{patientId}||{patientLastName}^{patientFirstName}||{DOB}|{gender}{(char)13}";
                    string message_PV1 = $"PV1|1|{patientClass}|{patientLocation}|||||||||||||||||{(char)13}";
                    string message_ORC = $"ORC|AF|{sampleNo}||{(char)13}";
                    string message_OBR = $"OBR|1|{sampleNo}|||||{datetime}|{datetime}|||||||{datetime}|||||||||HM|||||||{(char)13}";
                    string message_OBX1 = $"OBX|1|IS|08001^Take Mode^99MRC||A||||||F{(char)13}";
                    string message_OBX2 = $"OBX|2|IS|08002^Blood Mode^99MRC||W||||||F{(char)13}";
                    string message_OBX3 = $"OBX|3|IS|08003^Test Mode^99MRC||{testname}||||||F{(char)13}";
                    string message_OBX4 = $"OBX|4|IS|01002^Ref Group^99MRC||XXXX||||||F{(char)13}";
                    string message_OBX5 = $"OBX|5|NM|30525-0^Age^LN||1|hr|||||F{(char)13}";
                    string message_OBX6 = $"OBX|6|ST|01001^Remark^99MRC||20170809SYS0066||||||F{(char)13}";
                    ORMMessage = message_MSH + message_MSA + message_PID + message_PV1 + message_ORC + message_OBR +
                        message_OBX1 + message_OBX2 + message_OBX3 + message_OBX4 + message_OBX5 + message_OBX6;
                }
                else
                {
                    ORMMessage = message_MSH + message_MSA;
                }
            }
            else
            {
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