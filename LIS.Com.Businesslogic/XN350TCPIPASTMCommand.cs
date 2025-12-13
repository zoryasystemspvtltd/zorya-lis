using LIS.DtoModel;
using LIS.DtoModel.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class XN350TCPIPASTMCommand : TCPIPASTMCommand
    {
        public JArray validCodes;
        public XN350TCPIPASTMCommand(TCPIPSettings _settings) : base(_settings)
        {
            try
            {
                CheckCode();

            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("Create method exception:", ex);
                Logger.Logger.LogInstance.LogError("Please add XN350.JSON file under bin/Data/");
            }
        }

        private void CheckCode()
        {
            var path = $"{Environment.CurrentDirectory}\\Data\\XN350.json";
            var jsonData = File.ReadAllText(path);
            validCodes = JArray.Parse(jsonData);
        }

        public override async Task CreateMessageAsync(string message)
        {
            //Remove <CHK1>,<CHK2> character from raw message
            message = message.Replace("<CHK1>", "9");
            message = message.Replace("<CHK2>", "D");
            Logger.Logger.LogInstance.LogDebug("XN350 CreateMessage method started. '{0}'", message);
            string formattedmessage = "";
            string[] segments;
            try
            {
                segments = message.Split(Strings.Chr(10));  // Chr(10)
                for (int i = 0; i <= segments.Length - 1; i++)
                {
                    for (int j = 2; j <= segments[i].Length - 5; j++)
                    {
                        if (j != segments[i].Length - 5 | segments[i].ToString()[j + 1] != Strings.Chr(23))
                            formattedmessage += segments[i][j];
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("XN350 CreateMessage method exception:", ex);
            }
            await ParseMessageAsync(formattedmessage);
            Logger.Logger.LogInstance.LogDebug("XN350 CreateMessage method completed");
        }
        private async Task ParseMessageAsync(string message)
        {
            try
            {
                Logger.Logger.LogInstance.LogDebug("XN350 ParseMessage method started");

                string[] record = message.Split(Strings.Chr(13)); // Chr(13)

                var lsResult = new List<LisTestValue>();
                string sampleNo = "";
                for (int index = 0; index <= record.Length - 1; index++)
                {
                    if (record[index].Length < 5) continue;

                    string[] field = record[index].Split('|');
                    switch (field[0].Trim())
                    {
                        case "O":
                            {
                                string[] sampleField = field[3].Split('^');
                                sampleNo = sampleField[2].Trim();
                                break;
                            }

                        case "R":
                            {
                                string[] parameter = field[2].Split('^');
                                string paramCode = parameter[4];
                                if (validCodes == null)
                                    CheckCode();

                                bool isValid = validCodes.Any(item => (string)item["Code"] == paramCode);
                                if (paramCode != "" && isValid)
                                {
                                    LisTestValue resultDetails = new LisTestValue
                                    {
                                        REF_VISITNO = sampleNo,
                                        PARAMCODE = paramCode,
                                        Value = field[3]
                                    };
                                    Logger.Logger.LogInstance.LogDebug("XN350 Result processed for SampleNo " + sampleNo + " and Parameter " + paramCode);
                                    lsResult.Add(resultDetails);
                                }
                                else
                                    continue;

                                break;
                            }
                    }
                }

                Logger.Logger.LogInstance.LogDebug("XN350 Result posted to API for SampleNo: " + lsResult[0].REF_VISITNO);
                await LisContext.LisDOM.SaveTestResult(lsResult);


                Logger.Logger.LogInstance.LogDebug("XN350 ParseMessage method completed");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("XN350 ParseMessage method exception:", ex);
            }
        }

    }
}