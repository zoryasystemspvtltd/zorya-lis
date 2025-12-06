using LIS.DtoModel;
using LIS.DtoModel.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class XN350TCPIPASTMCommand : TCPIPASTMCommand
    {
        private readonly JArray validCodes;
        public XN350TCPIPASTMCommand(TCPIPSettings _settings) : base(_settings)
        {
            try
            {
                var path = $"{Environment.CurrentDirectory}\\Data\\XN350.json";
                var jsonData = File.ReadAllText(path);
                validCodes = JArray.Parse(jsonData);
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("Create method exception:", ex);
                Logger.Logger.LogInstance.LogError("Please add XN350.JSON file under bin/Data/");
            }
        }

        public override async Task CreateMessage(string message)
        {
            Logger.Logger.LogInstance.LogDebug("XN350 CreateMessage method started '{0}'", message);
            sInputMsg = "";
            string formattedmessage = "";
            string[] segments;
            try
            {
                segments = message.Split(Strings.Chr(10));  // Chr(10) <LF>
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
                Logger.Logger.LogInstance.LogException("Create method exception:", ex);
            }

            await Identify(formattedmessage);
            Logger.Logger.LogInstance.LogDebug("XN350 CreateMessage method completed");
        }

        public override async Task Identify(string message)
        {
            Logger.Logger.LogInstance.LogDebug("XN350 Identify method started");
            Logger.Logger.LogInstance.LogDebug("XN350 Identify method Data: " + message);
            List<string> sampleList = new List<string>();
            ArrayList uniqueSampleList;
            string[] segments = message.Split(Strings.Chr(13)); // Chr(13) <CR>
            try
            {
                if (segments.Length > 1)
                {
                    if (segments[1].Substring(0, 1).ToUpper() == "P")
                    {
                        for (int i = 0; i <= segments.Length - 2; i++)
                        {
                            if (segments[i].Substring(0, 1).ToUpper() == "O")
                            {
                                string sSpecimenId = segments[i].Split('^')[2].Trim();
                                sampleList.Add(sSpecimenId);
                            }
                        }

                        Hashtable ht = new Hashtable();
                        foreach (string str in sampleList)
                            ht[str] = DBNull.Value;

                        uniqueSampleList = new ArrayList(ht.Keys);
                        await ParseMessage(message, uniqueSampleList);
                    }
                }
                Logger.Logger.LogInstance.LogDebug("XN350 Identify method completed");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("XN350 Identify method exception:", ex);
            }
        }

        public override async Task ParseMessage(string message, ArrayList sampleIdLst)
        {
            try
            {
                Logger.Logger.LogInstance.LogDebug("XN350 ParseMessage method started");

                string[] record = message.Split(Strings.Chr(13)); // Chr(13)
                for (int j = 0; j <= sampleIdLst.Count - 1; j++)
                {
                    var lsResult = new List<LisTestValue>();
                    string sampleNo = "";
                    for (int index = 0; index <= record.Length - 1; index++)
                    {
                        string[] field = record[index].Split('|');
                        switch (field[0])
                        {
                            case "O":
                                {
                                    string[] sampleField = field[3].Split('^');
                                    sampleNo = sampleField[2].Trim();
                                    break;
                                }

                            case "R":
                                {
                                    if (sampleNo == sampleIdLst[j].ToString())
                                    {
                                        string[] parameter = field[2].Split('^');
                                        string paramCode = parameter[4];
                                        bool isValid = validCodes.Any(item => (string)item["Code"] == paramCode);
                                        if (paramCode != "" && isValid)
                                        {
                                            LisTestValue resultDetails = new LisTestValue();
                                            resultDetails.REF_VISITNO = sampleNo;
                                            resultDetails.PARAMCODE = paramCode;
                                            resultDetails.Value = field[3];
                                            Logger.Logger.LogInstance.LogDebug("XN350 Result processed for SampleNo " + sampleNo + " and Parameter " + paramCode);
                                            lsResult.Add(resultDetails);
                                        }
                                        else
                                            continue;
                                    }
                                    break;
                                }
                        }
                    }

                    Logger.Logger.LogInstance.LogDebug("XN350 Result posted to API for SampleNo: " + lsResult[0].REF_VISITNO);
                    await LisContext.LisDOM.SaveTestResult(lsResult);

                }
                Logger.Logger.LogInstance.LogDebug("XN350 ParseMessage method completed");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("XN350 ParseMessage method exception:", ex);
            }
        }

    }
}