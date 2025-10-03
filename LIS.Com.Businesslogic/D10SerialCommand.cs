using LIS.DtoModel;
using LIS.DtoModel.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class D10SerialCommand : SerialCommand
    {
        public D10SerialCommand(PortSettings settings)
            : base(settings)
        {

        }
        public override async Task CreateMessage(string message)
        {
            //Remove <CHK1>,<CHK2> character from raw message
            message = message.Replace("<CHK1>", "9");
            message = message.Replace("<CHK2>", "D");
            Logger.Logger.LogInstance.LogDebug("D10 CreateMessage method started. '{0}'", message);
            sInputMsg = "";
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
                Logger.Logger.LogInstance.LogException("D10 CreateMessage method exception:", ex);
            }
            await Identify(formattedmessage);
            Logger.Logger.LogInstance.LogDebug("D10 CreateMessage method completed");
        }

        public override async Task Identify(string message)
        {
            Logger.Logger.LogInstance.LogDebug("D10 Identify method started");
            Logger.Logger.LogInstance.LogDebug("D10 Identify method Data:" + message);
            List<string> sampleList = new List<string>();
            ArrayList uniqueSampleList;
            string[] segments = message.Split(Strings.Chr(13)); // Chr(13)
            try
            {
                if (segments.Length > 1)
                {
                    if (segments[1].Substring(0, 1).ToUpper() == "Q")
                    {
                        string[] queryFields = segments[1].Split('|');
                        string[] sampleField = queryFields[2].Split('^');
                        string sampleID = sampleField[1];
                        await SendOrderData(sampleID);
                    }
                    else if (segments[1].Substring(0, 1).ToUpper() == "P")
                    {
                        Logger.Logger.LogInstance.LogDebug("D10 Patient Info started");
                        for (int i = 0; i <= segments.Length - 2; i++)
                        {
                            if (segments[i].Substring(0, 1).ToUpper() == "O")
                            {
                                string sSpecimenId = segments[i].Split('|')[2];
                                sampleList.Add(sSpecimenId.Split('^')[0]);
                            }
                        }

                        Hashtable ht = new Hashtable();
                        foreach (string str in sampleList)
                            ht[str] = DBNull.Value;

                        uniqueSampleList = new ArrayList(ht.Keys);
                        await ParseMessage(message, uniqueSampleList);
                    }
                }
                Logger.Logger.LogInstance.LogDebug("D10 Identify method completed");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("D10 Identify method exception:", ex);
            }
        }

        public override async Task SendOrderData(string sampleId)
        {
            try
            {
                data = new string[6];
                Logger.Logger.LogInstance.LogDebug("D10 SendOrderData method started for SampleNo: " + sampleId);
                string datetime = DateTime.Now.AddMinutes(-30).ToString("yyyyMMddhhmmss");
                string specialchar = @"\^&";
                string headerSegment = $"1H|{specialchar}|||D10^01^5.00|||||||||{datetime}{Strings.Chr(13)}<CHK1><CHK2>{Strings.Chr(3)}";
                string patientSegment = $"2P|1|{Strings.Chr(13)}<CHK1><CHK2>{Strings.Chr(3)}";
                string orderSegment = "";
                string trailerSegment;
                IEnumerable<TestRequestDetail> testlist = await LisContext.LisDOM.GetTestRequestDetails(sampleId);
                string orderMessage;
                if (testlist.Count() > 0)
                {
                    var testname = "";
                    var firstTest = testlist.First();
                    for (int i = 0; i < testlist.Count();)
                    {
                        var test = testlist.ElementAt(i);
                        var ackSent = await LisContext.LisDOM.AcknowledgeSample(test.Id);
                        testname += "^^^" + test.LISTestCode;
                        i++;
                        if (testlist.Count() == i)
                            break;
                        else
                            testname += @"\";
                    }
                    string injectionDate = DateTime.Now.AddMinutes(-30).ToString("yyyyMMdd");                   
                    orderSegment = $"3O|1|{sampleId}|{sampleId}-01-20-{injectionDate}-01|{testname}|||||||||||||||||||||F{Strings.Chr(13)}<CHK1><CHK2>{Strings.Chr(3)}";
                    trailerSegment = $"4L|1|N{Strings.Chr(13)}<CHK1><CHK2>{Strings.Chr(3)}";
                    data[0] = Strings.Chr(5).ToString();
                    data[1] = headerSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Header Segment {0}", headerSegment);
                    data[2] = patientSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Patient Segment {0}", patientSegment);
                    data[3] = orderSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Order Segment {0}", orderSegment);
                    data[4] = trailerSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Trailer Segment {0}", trailerSegment);

                    index = 0;
                }
                else//no test order
                {
                    data[2] = Strings.Chr(5).ToString();
                    data[3] = headerSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Header Segment {0}", headerSegment);
                    trailerSegment = $"2L|1|I{Strings.Chr(13)}<CHK1><CHK2>{Strings.Chr(3)}";
                    data[4] = trailerSegment;
                    Logger.Logger.LogInstance.LogDebug("D10 Trailer Segment {0}", trailerSegment);

                    index = 2;
                }
                if (!port.IsOpen)
                {
                    port.Open();
                }
                WriteToPort("" + (char)5);
                Logger.Logger.LogInstance.LogDebug("D10 SendOrderData method completed for SampleNo: " + sampleId);
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("D10 SendOrderData method exception:", ex);
            }
        }

        public override async Task ParseMessage(string message, ArrayList sampleIdLst)
        {
            try
            {
                Logger.Logger.LogInstance.LogDebug("D10 ParseMessage method started");
                Logger.Logger.LogInstance.LogDebug("D10 ParseMessage method Data: " + message);
                string[] record = message.Split(Strings.Chr(13)); // Chr(13)
                for (int j = 0; j <= sampleIdLst.Count - 1; j++)
                {
                    Result result = new Result();
                    List<TestResultDetails> lsResult = new List<TestResultDetails>();
                    TestResult testResult = new TestResult();
                    string LisTestCode = "";
                    testResult.ResultDate = DateAndTime.Now;
                    string sampleNo = "";
                    for (int index = 0; index <= record.Length - 1; index++)
                    {
                        string[] field = record[index].Split('|');
                        switch (field[0])
                        {
                            case "O":
                                {
                                    sampleNo = field[2];
                                    testResult.SampleNo = sampleNo;
                                    LisTestCode = field[4].Split('^')[3];
                                    testResult.LISTestCode = LisTestCode;
                                    break;
                                }

                            case "R":
                                {
                                    if (sampleNo == sampleIdLst[j].ToString())
                                    {
                                        TestResultDetails resultDetails = new TestResultDetails();
                                        string[] parameter = field[2].Split('^');
                                        string paramCode = parameter[3];
                                        string paramUnit = parameter[4];
                                        if (paramCode != "")
                                        {
                                            resultDetails.LISParamCode = paramCode;
                                            resultDetails.LISParamValue = field[3];
                                            resultDetails.LISParamUnit = paramUnit;
                                        }
                                        lsResult.Add(resultDetails);
                                    }

                                    break;
                                }
                        }
                    }

                    result.TestResult = testResult;
                    result.ResultDetails = lsResult;
                    Logger.Logger.LogInstance.LogDebug("D10 Result posted to API for SampleNo: " + testResult.SampleNo);
                    await LisContext.LisDOM.SaveTestResult(result);

                }
                Logger.Logger.LogInstance.LogDebug("D10 ParseMessage method completed");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException("D10 ParseMessage method exception:", ex);
            }
        }
    }
}