using LIS.DtoModel;
using LIS.DtoModel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class ZYBIOZ3TCPIPHL7Command : TCPIPHL7Command
    {
        public ZYBIOZ3TCPIPHL7Command(TCPIPSettings _settings) : base(_settings)
        { }

        public override async Task<string> ResultProcess(string message)
        {
            string messageControlId = "";
            Logger.Logger.LogInstance.LogDebug("Result process method excuted.");
            //string message = sInputMsg.ToString();
            sInputMsg.Clear(); //Clear the insput message
            string response = "";
            string[] resultMesgSegments = message.TrimEnd((char)13).Split((char)13); // vbCr<CR>
            if (resultMesgSegments.Length > 1)
            {
                string[] firstrow = resultMesgSegments[0].Split('|');
                if (firstrow[0] == "MSH" || firstrow[0] == "MSH")
                    messageControlId = firstrow[9];

                string[] field = resultMesgSegments[1].Split('|');
                if (field[0].Trim() == "OBR")
                {
                    string sampleNo = field[3];
                    //bool flag = IsValidSampleNo(sampleNo);
                    //For control result
                    response = @"MSH|^~\&|||||" + DateTime.Now.ToString("yyyyMMddhhmmss") +
                                       "||ACK^R01|" + messageControlId + "|P|2.3.1||||2||ASCII||" + (char)13 +
                                       $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";

                    if (resultMesgSegments.Length > 2)
                    {
                        await ProccessMessage(sampleNo, message);
                    }
                }
            }
            return response;
        }

        private async Task ProccessMessage(string sampleNo, string rawMessage)
        {
            Logger.Logger.LogInstance.LogDebug("ZYBIOZ3 ProccessMessage method started");
            string[] resultMesgSegments = rawMessage.TrimEnd((char)13).Split((char)13); // <CR>

            await SaveResult(sampleNo, resultMesgSegments);

            Logger.Logger.LogInstance.LogDebug("ZYBIOZ3 ProccessMessage method completed");
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
                    var code = field[3].Split('^');
                    var paramCode = code[1].ToString();
                    var paramValue = field[5].ToString();
                    resultDetails.PARAMCODE = paramCode;
                    resultDetails.Value = paramValue;
                    resultDetails.REF_VISITNO = sampleNo;
                    lsResult.Add(resultDetails);
                }
            }

            Logger.Logger.LogInstance.LogDebug("ZYBIOZ3 Result posted to API for SampleNo: " + lsResult[0].REF_VISITNO);
            await LisContext.LisDOM.SaveTestResult(lsResult);
        }
    }
}