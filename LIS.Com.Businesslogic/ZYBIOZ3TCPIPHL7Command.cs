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

        public override async Task<string> ProccessMessage(string sampleNo, string rawMessage, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("ZYBIOZ3 ProccessMessage method started");
            string[] resultMesgSegments = rawMessage.TrimEnd((char)13).Split((char)13); // <CR>

            await SaveResult(sampleNo, resultMesgSegments);
            
            string response = @"MSH|^~&|Z3|Zybio|||" + DateTime.Now.ToString("yyyyMMddhhmmss") +
                "||ACK^R01|" + messageControlId + "|P|2.3.1||||||||" + (char)13 +
                $"MSA|AA|{messageControlId}|||||{(char)13}";
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