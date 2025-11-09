using LIS.BusinessLogic.Helper;
using LIS.DataAccess;
using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Lis.Api.Controllers.Api
{
    /// <summary>
    /// This API is used to comminicate between Zorya LIS server and indivedual LIS terminal
    /// Used for Accu Health
    /// </summary>
    public class AccuHealthLisController : ApiController
    {
        private IResponseManager responseManager;
        private ApplicationDBContext dBContext;
        private ILogger logger;
        private IModuleIdentity identity;
        private AccuHealthDataSynchronizer accuHealthDataSynchronizer;
        public AccuHealthLisController(ILogger logger,
            IResponseManager responseManager,
            ApplicationDBContext dBContext,
            IModuleIdentity identity,
            AccuHealthDataSynchronizer accuHealthDataSynchronizer)
        {
            this.dBContext = dBContext;
            this.logger = logger;
            this.responseManager = responseManager;
            this.identity = identity;
            this.accuHealthDataSynchronizer = accuHealthDataSynchronizer;
        }   

        [AllowAnonymous]
        [HttpGet]
        public bool Get()
        {
            try
            {
                logger.LogInfo($"Ping API Request");
                var testRequestDetails = true; // Todo 
                logger.LogInfo($"Ping API Response");
                return testRequestDetails;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return false;
            }
        }
        /// <summary>
        /// Get list of Sample for a specific sampleno coming through barcode
        /// </summary>
        /// <param name="Id">Barcode</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<AccuHealthSample> Get(string Id)
        {
            try
            {
                logger.LogInfo($"Get Sample Request: {Id}");
                logger.LogInfo($"Get Sample Request: {identity.AccessKey}");

                var testOrders = dBContext.AccuHealthTestOrders
                            .Where(p => p.REF_VISITNO.Equals(Id, StringComparison.OrdinalIgnoreCase))
                            .Join(dBContext.AccuHealthParamMappings,
                                t => t.PARAMCODE,
                                pm => pm.HIS_PARAMCODE,
                                (t, pm) => new { t, pm })
                            .Join(dBContext.EquipmentMaster,
                                tmp => tmp.pm.EquipmentId,
                                e => e.Id,
                                (tmp, e) => new { tmp.t, tmp.pm, e })
                            .Where(x => x.e.IsActive &&
                                    x.e.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase));


                foreach (var item in testOrders)
                {
                    item.t.Status = ReportStatusType.SentToEquipment;
                }

                var orders = testOrders
                    .Select(x => new AccuHealthSample()
                    {
                        PATFNAME = x.t.PATFNAME,
                        PATMNAME = x.t.PATMNAME,
                        PATLNAME = x.t.PATLNAME,
                        PAT_DOB = x.t.PAT_DOB,
                        GENDER = x.t.GENDER,
                        PATAGE = x.t.PATAGE,
                        AGEUNIT = x.t.AGEUNIT,
                        SampleNo = x.t.REF_VISITNO,
                        LisParamCode = x.pm.LIS_PARAMCODE,
                        HIS_PARAMCODE = x.pm.HIS_PARAMCODE,
                        SPECIMEN = x.pm.SPECIMEN
                    })
                    .ToList();

                var responseStrign = JsonConvert.SerializeObject(orders);
                logger.LogInfo($"Get Sample Response: {orders}");
                logger.LogInfo($"Get Sample Response: {responseStrign}");

                dBContext.SaveChanges();

                return orders;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }


        /// <summary>
        /// Save the list of result received from LIS Console
        /// REF_VISITNO means SampleNo
        /// </summary>
        /// <param name="testValue"></param>
        /// <returns></returns>`
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Post(LisTestValue[] values)
        {
            try
            {
                foreach (var item in values)
                {
                    item.Equipment = identity.AccessKey;
                }

                dBContext.LisTestValues.AddRange(values);

                var accuHealthResults = new List<AccuHealthTestValue>();

                foreach (var item in values)
                {
                    item.Equipment = identity.AccessKey;

                    var recordsToUpdate = dBContext.AccuHealthTestOrders
                    .Join(dBContext.AccuHealthParamMappings,
                        o => o.PARAMCODE,
                        pm => pm.HIS_PARAMCODE,
                        (o, pm) => new { o, pm })
                    .Join(dBContext.EquipmentMaster,
                        x => x.pm.EquipmentId,
                        e => e.Id,
                        (x, e) => new { x.o, x.pm, e })
                    .Where(x => x.e.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase)
                    && x.pm.LIS_PARAMCODE == item.PARAMCODE && x.o.REF_VISITNO == item.REF_VISITNO).FirstOrDefault();

                    if (recordsToUpdate != null)
                    {
                        recordsToUpdate.o.Value = item.Value;
                        recordsToUpdate.o.Status = ReportStatusType.ReportGenerated;

                        var accuHealth = new AccuHealthTestValue()
                        {
                            ROW_ID = recordsToUpdate.o.ROW_ID,
                            isSynced = true,
                            SRNO = recordsToUpdate.o.REF_VISITNO,
                            SDATE = recordsToUpdate.o.REQDATETIME,
                            SAMPLEID = recordsToUpdate.o.REF_VISITNO,
                            TESTID = recordsToUpdate.o.PARAMCODE,
                            MACHINEID = recordsToUpdate.e.Name,
                            SUFFIX = "",
                            TRANSFERFLAG = "",
                            TMPVALUE = recordsToUpdate.o.Value,
                            DESCRIPTION = recordsToUpdate.o.PARAMCODE,
                            RUNDATE = DateTime.Now,
                        };
                        accuHealthResults.Add(accuHealth);
                    }
                }
                dBContext.SaveChanges();

                // TODO Send to AccuHealth
                if (accuHealthResults != null)
                {
                    await accuHealthDataSynchronizer.PostTestResults(accuHealthResults);

                    dBContext.AccuHealthTestValues.AddRange(accuHealthResults);
                    dBContext.SaveChanges();
                }
                APIResponse aPIResponse = responseManager.CreateResponse(HttpStatusCode.OK, "Value saved successfully.", null, null);

                return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

    }
}