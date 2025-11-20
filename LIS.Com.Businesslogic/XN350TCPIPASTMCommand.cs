using LIS.DtoModel;
using LIS.DtoModel.Models;
using LIS.DtoModel.Models.ExternalApi;
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
        public override async Task SendOrderData(string sampleStr)
        {
            try
            {
                output = new string[7];// This need to be 7 for equipment XN350
                string[] sampleField = sampleStr.Split('^');
                string sampleId = sampleField[2].Trim();
                Logger.Logger.LogInstance.LogDebug("XN350 SendOrderData method started for SampleNo: " + sampleId);

                string datetime = DateTime.Now.AddMinutes(-30).ToString("yyyyMMddhhmmss");
                var specialchar = @"\^&";
                var headerSegment = $"1H|{specialchar}|||||||||||E1394-97{Strings.Chr(13)}{Strings.Chr(3)}";
                string patientSegment = "";
                var trailerSegment = "";
                var orderSegment1 = "";
                var orderSegment2 = "";
                var temporderSegment = $"O|1|{sampleStr}||";
                IEnumerable<AccuHealthSample> testlist = await LisContext.LisDOM.GetTestRequestDetails(sampleId);

                if (testlist != null && testlist.Count() > 0)
                {
                    string patientFirstName = "";
                    string patientLastName = "";
                    trailerSegment = $"5L|1|N{Strings.Chr(13)}{Strings.Chr(3)}";

                    var firstTest = testlist.First();
                    var testname = "";
                    //var patientGender = string.Empty;
                    //var patientId = string.Empty;
                    //var dob = string.Empty;
                    //string patientBedNo = "";

                    //patientId = firstTest.Patient?.HisPatientId.ToString();
                    //patientGender = firstTest.Patient?.Gender?.Substring(0, 1);
                    //patientBedNo = firstTest.BedNo;

                    //if (firstTest.Patient?.DateOfBirth != null)
                    //{
                    //    dob = firstTest.Patient?.DateOfBirth.ToString("yyyyMMdd");
                    //}

                    patientFirstName = firstTest.PATFNAME;
                    patientLastName = firstTest.PATMNAME;


                    for (int i = 0; i < testlist.Count();)
                    {
                        var test = testlist.ElementAt(i);
                        //var ackSent = await LisContext.LisDOM.AcknowledgeSample(test.Id);
                        testname += "^^^^" + test.LisParamCode;

                        i++;
                        if (testlist.Count() == i)
                            break;
                        else
                            testname += @"\";
                    }
                    //patientSegment = $"2P|1|||{patientId}|^{patientFirstName}^{patientLastName}||{dob}|{patientGender}|||||^||||||||||||^^^{patientBedNo}{Strings.Chr(13)}{Strings.Chr(3)}";
                    patientSegment = $"2P|1||||^{patientFirstName}^{patientLastName}||||||||^||||||||||||^^^{Strings.Chr(13)}{Strings.Chr(3)}";
                    temporderSegment += $"{testname}||{datetime}|||||N||||||||||||||Q";

                    if (temporderSegment.Length > 230)
                    {
                        var order1 = temporderSegment.Substring(0, 230);
                        int orderCharCount = temporderSegment.Length - 230;
                        var order2 = temporderSegment.Substring(230, orderCharCount);
                        orderSegment1 = $"3{order1}{Strings.Chr(23)}"; //23 means ETB
                        orderSegment2 = $"4{order2}{Strings.Chr(13)}{Strings.Chr(3)}";
                        output[0] = Strings.Chr(5).ToString();
                        output[1] = headerSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Header Segment {0}", headerSegment);
                        output[2] = patientSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Patient Segment {0}", patientSegment);
                        output[3] = orderSegment1;
                        Logger.Logger.LogInstance.LogDebug("XN350 Order1 Segment {0}", orderSegment1);
                        output[4] = orderSegment2;
                        Logger.Logger.LogInstance.LogDebug("XN350 Order2 Segment {0}", orderSegment2);
                        output[5] = trailerSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Trailer Segment {0}", trailerSegment);
                        index = 0;
                    }
                    else
                    {
                        orderSegment2 = $"3{temporderSegment}{Strings.Chr(13)}{Strings.Chr(3)}";
                        output[1] = Strings.Chr(5).ToString();
                        output[2] = headerSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Header Segment {0}", headerSegment);
                        output[3] = patientSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Patient Segment {0}", patientSegment);
                        output[4] = orderSegment2;
                        Logger.Logger.LogInstance.LogDebug("XN350 Order2 Segment {0}", orderSegment2);
                        output[5] = trailerSegment;
                        Logger.Logger.LogInstance.LogDebug("XN350 Trailer Segment {0}", trailerSegment);
                        index = 1;
                    }
                }
                else//no test order
                {
                    headerSegment = $"1H|{specialchar}|||||||||||E1394-97{Strings.Chr(13)}{Strings.Chr(3)}";
                    patientSegment = $"2P|1{Strings.Chr(13)}{Strings.Chr(3)}";
                    orderSegment1 = $"3O|{sampleStr}||||{datetime}||||||||||||||||||Y{Strings.Chr(13)}{Strings.Chr(3)}";
                    trailerSegment = $"4L|1|N{Strings.Chr(13)}{Strings.Chr(3)}";
                    output[1] = Strings.Chr(5).ToString();
                    output[2] = headerSegment;
                    Logger.Logger.LogInstance.LogDebug("XN350 Header Segment {0}", headerSegment);
                    output[3] = patientSegment;
                    Logger.Logger.LogInstance.LogDebug("XN350 Patient Segment {0}", patientSegment);
                    output[4] = orderSegment1;
                    Logger.Logger.LogInstance.LogDebug("XN350 Order Segment {0}", orderSegment1);
                    output[5] = trailerSegment;
                    Logger.Logger.LogInstance.LogDebug("XN350 Trailer Segment {0}", trailerSegment);
                    index = 1;
                }

                WriteToPort("" + (char)5);

                Logger.Logger.LogInstance.LogDebug("XN350 SendOrderData method completed for SampleNo " + sampleId);
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("XN350 SendOrderData method exception:", ex);
            }
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
                    if (segments[1].Substring(0, 1).ToUpper() == "Q")
                    {
                        string[] queryFields = segments[1].Split('|');
                        await SendOrderData(queryFields[2]);
                    }

                    else if (segments[1].Substring(0, 1).ToUpper() == "P")
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