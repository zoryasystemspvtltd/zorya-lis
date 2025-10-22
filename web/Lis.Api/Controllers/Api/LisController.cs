
using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.Logger;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace Lis.Api.Controllers.Api
{
    public class LisController : ApiController
    {
        //TODO HMAC Authentication
        private ITestRequestDetailsManager testRequestManager;
        private IResultManager resultManager;
        private ILogger logger;
        private IResponseManager responseMgr;

        public LisController(
            ITestRequestDetailsManager testRequestDetailsManager,
            IResultManager resultHisManager,
            IResponseManager responseManager,
            ILogger Logger)
        {
            testRequestManager = testRequestDetailsManager;
            resultManager = resultHisManager;
            responseMgr = responseManager;
            logger = Logger;
        }

        /// <summary>
        /// Save Lis result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Post(Result result)
        {
            APIResponse aPIResponse = null;
            try
            {
                var requestStrign = JsonConvert.SerializeObject(result);
                logger.LogInfo($"Post result Request: {requestStrign}");




                var responsemessage = resultManager.Add(result);
                // TODO After result saved to local - need to send to HIS

                // Order
                //"IPOPFLAG": null,
                //"PINNO": null,
                //"REF_VISITNO": "40000165181025",
                //"ADMISSIONNO": "TB/KSB/251018/0001",
                //"REQDATETIME": "2025-10-18T20:32:59.407",
                //"TESTPROF_CODE": "COMPLETE HAEMOGRAM",
                //"PROCESSED": null,
                //"PATFNAME": "DEMO",
                //"PATMNAME": null,
                //"PATLNAME": null,
                //"PAT_DOB": null,
                //"GENDER": null,
                //"PATAGE": null,
                //"AGEUNIT": null,
                //"DOC_NAME": null,
                //"PATIENTTYPECLASS": null,
                //"SEQNO": null,
                //"ADDDATE": null,
                //"ADDTIME": null,
                //"TITLE": null,
                //"LABNO": null,
                //"DATESTAMP": null,
                //"PARAMCODE": "Hematocrit (PCV) ",
                //"PARAMNAME": "Hematocrit (PCV) ",
                //"MRESULT": null,
                //"BC_PRINTED": null,
                //"ROW_ID": "67bd9a1d-9d97-458e-88a7-dbb769227360",
                //"isSynced": false,
                //"branch_ID": "ee09d44e-757d-4fd2-b171-1f59224390ea"

                // HIS Api Field - Result
                //SRNO = row["SRNO"]?.ToString() ?? string.Empty,
                //SDATE = row.Field<DateTime?>("SDATE") ?? DateTime.MinValue,
                //SAMPLEID = row["SAMPLEID"]?.ToString() ?? string.Empty,
                //TESTID = row["TESTID"]?.ToString() ?? string.Empty,
                //MACHINEID = row["MACHINEID"]?.ToString() ?? string.Empty,
                //SUFFIX = row["SUFFIX"]?.ToString() ?? string.Empty,
                //TRANSFERFLAG = row["TRANSFERFLAG"]?.ToString() ?? string.Empty,
                //TMPVALUE = row["TMPVALUE"]?.ToString() ?? string.Empty,
                //DESCRIPTION = row["DESCRIPTION"]?.ToString() ?? string.Empty,
                //RUNDATE = row.Field<DateTime?>("RUNDATE") ?? DateTime.MinValue,
                //ROW_ID = rowId,
                //isSynced = true // set to true for sending; DB update will handle marking as synced

                var apiRequest = new PostTestResultsRuquest();

                // TODO Question ?
                //apiRequest.ClientId = result.TestResult.ClientId;

                var values = new List<TestValuesData>();
                foreach (var r in result.ResultDetails)
                {
                    var value = new TestValuesData();

                    // Convert Zorya format to Accuhealth Format before saving to the DB
                    value.SRNO = $"{result.TestResult.TestRequestDetail.SEQNO}";
                    value.SDATE = result.TestResult.TestRequestDetail.DATESTAMP;
                    value.SAMPLEID = $"{result.TestResult.TestRequestDetail.Id}";
                    value.TESTID = $"{result.TestResult.TestRequestId}";
                    value.MACHINEID = $"{result.TestResult.EquipmentId}";
                    value.SUFFIX = string.Empty;
                    value.TRANSFERFLAG = string.Empty;
                    value.TMPVALUE = r.LISParamValue;
                    value.DESCRIPTION = r.LISParamCode;
                    value.RUNDATE = result.TestResult.ResultDate;
                    value.ROW_ID = result.TestResult.TestRequestDetail.ROW_ID;
                    value.isSynced = true; // set to true for sending; DB update will handle marking as synced


                    values.Add(value);
                }

                apiRequest.TestValues = values.ToArray();

                // TODO Post apiRequest to /api/LIS/PostTestResults

                aPIResponse = responseMgr.CreateResponse(HttpStatusCode.OK, "Result added successfully", null, responsemessage);
                logger.LogInfo($"Post result response{responsemessage}");
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                aPIResponse = responseMgr.CreateResponse(HttpStatusCode.OK, ex.Message, null, 0);
                return Request.CreateResponse<APIResponse>(HttpStatusCode.InternalServerError, aPIResponse);
            }
            return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
        }


        /// <summary>
        /// Acknowledge receive of sample from LIS Console.
        /// </summary>
        /// <param name="Id">Sample Code</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut]
        public HttpResponseMessage Put(long Id)
        {
            APIResponse aPIResponse = null;
            try
            {

                logger.LogInfo($"Acknowledge Sample Request: {Id}");

                var responsemessage = testRequestManager.UpdateStatus(Id, ReportStatusType.SentToEquipment);

                aPIResponse = responseMgr.CreateResponse(HttpStatusCode.OK, "Result added successfully", null, responsemessage);

                logger.LogInfo($"Acknowledge Sample Response: {responsemessage}");
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                aPIResponse = responseMgr.CreateResponse(HttpStatusCode.OK, ex.Message, null, false);
                return Request.CreateResponse<APIResponse>(HttpStatusCode.InternalServerError, aPIResponse);
            }

            return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
        }

        /// <summary>
        /// This methods is used to ping the API from LIS Console
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public bool Get()
        {
            try
            {
                logger.LogInfo($"Ping API Request");
                var testRequestDetails = testRequestManager.Ping();
                logger.LogInfo($"Ping API Response");
                return testRequestDetails;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return false;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<TestRequestDetail> Get(string Id)
        {
            try
            {
                logger.LogInfo($"Get Sample Request: {Id}");

                var testRequestDetails = testRequestManager.GetBySampleNo(Id, ReportStatusType.New);

                var responseStrign = JsonConvert.SerializeObject(testRequestDetails);
                logger.LogInfo($"Get Sample Response: {responseStrign}");
                return testRequestDetails;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public bool? Get(string SampleNo, string LisHostCode)
        {
            try
            {
                logger.LogInfo($"Get Sample Request: {SampleNo}");

                var isPanel = testRequestManager.IsPanelTest(SampleNo, LisHostCode);

                var responseStrign = JsonConvert.SerializeObject(isPanel);
                logger.LogInfo($"Get Sample Response: {responseStrign}");
                return isPanel;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }
    }
}
