using LIS.DtoModel;
using LIS.DtoModel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class ZYBIOZ5TCPIPHL7Command : TCPIPHL7Command
    {
        public ZYBIOZ5TCPIPHL7Command(TCPIPSettings _settings) : base(_settings)
        { }

        public override async Task ResultProcess(string message, string messageControlId)
        {
            Logger.Logger.LogInstance.LogDebug("Result process method excuted.");
           
            string[] resultMesgSegments = message.TrimEnd((char)13).Split((char)13); // vbCr<CR>
            if (resultMesgSegments.Length > 1)
            {
                string[] field = resultMesgSegments[1].Split('|');
                if (field[0].Trim() == "OBR")
                {
                    string sampleNo = field[3];
                    if (resultMesgSegments.Length > 2)
                    {
                        await SaveResult(sampleNo, resultMesgSegments);
                    }
                }
            }
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

            Logger.Logger.LogInstance.LogDebug("ZYBIOZ5 Result posted to API for SampleNo: " + lsResult[0].REF_VISITNO);
            await LisContext.LisDOM.SaveTestResult(lsResult);
        }
    }
}