using LIS.BusinessLogic.Helper;
using LIS.DataAccess;
using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.DtoModel.Models.ExternalApi;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
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
        public AccuHealthLisController(ILogger logger, 
            IResponseManager responseManager, 
            ApplicationDBContext dBContext, 
            IModuleIdentity identity )
        {
            this.dBContext = dBContext;
            this.logger = logger;
            this.responseManager = responseManager;
            this.identity = identity;
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

                var testOrders = dBContext.AccuHealthTestOrders
                            .Where(p => p.TESTPROF_CODE.Equals(Id, StringComparison.OrdinalIgnoreCase))
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
                foreach(var item in values)
                {
                    item.Equipment = identity.AccessKey;
                }

                dBContext.LisTestValues.AddRange(values);

                var recordsToUpdate = dBContext.AccuHealthTestOrders
                .Join(dBContext.AccuHealthParamMappings,
                    o => o.PARAMCODE,
                    pm => pm.HIS_PARAMCODE,
                    (o, pm) => new { o, pm })
                .Join(dBContext.EquipmentMaster,
                    x => x.pm.EquipmentId,
                    e => e.Id,
                    (x, e) => new { x.o, x.pm, e })
                .Join(values,
                    x => new { x.o.REF_VISITNO, LIS_PARAMCODE = x.pm.LIS_PARAMCODE },
                    vItem => new { REF_VISITNO = vItem.REF_VISITNO, LIS_PARAMCODE = vItem.PARAMCODE },
                    (x, vItem) => new { x.o, x.pm, x.e, vItem.Value })
                .Where(x => x.e.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase));

                foreach (var item in recordsToUpdate)
                {
                    item.o.Value = item.Value;
                    item.o.Status = ReportStatusType.ReportGenerated;
                }

                dBContext.SaveChanges();

                var accuHealthResults = recordsToUpdate
                                            .Select(p=> new AccuHealthTestValue()
                                            {
                                                ROW_ID = p.o.ROW_ID,
                                                isSynced = true,
                                                SRNO = p.o.REF_VISITNO,
                                                SDATE = p.o.DATESTAMP,
                                                SAMPLEID = p.o.REF_VISITNO,
                                                TESTID = p.o.PARAMCODE,
                                                MACHINEID = p.e.Name,
                                                SUFFIX = "",
                                                TRANSFERFLAG = "",
                                                TMPVALUE = p.o.Value,
                                                DESCRIPTION = p.o.PARAMCODE,
                                                RUNDATE = DateTime.Now,
                                            })
                                            .ToArray();

                // TODO Send to AccuHealth
                await PostTestResults(accuHealthResults);

                dBContext.AccuHealthTestValues.AddRange(accuHealthResults);
                dBContext.SaveChanges();

                APIResponse aPIResponse = responseManager.CreateResponse(HttpStatusCode.OK, "Value saved successfully.", null,null);

                return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }


        private static readonly string HospitalApiUrl = ConfigurationManager.AppSettings["HospitalApiUrl"];
        private static readonly string AccuHealthClientId = ConfigurationManager.AppSettings["AccuHealthClientId"];
        private static readonly string AccuHealthBranchId = ConfigurationManager.AppSettings["AccuHealthBranchId"];
        private static async Task<bool> PostTestResults(AccuHealthTestValue[] results)
        {
            string apiUrl = $"{HospitalApiUrl}lis/PostTestResults";
            using (var client = new ApiClient().GetHttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var payload = new
                {
                    ClientId = AccuHealthClientId,
                    TestValues = results
                };
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Convert JSON string to dynamic object
                    var result = JsonConvert.DeserializeObject<UpdateOrderStatusResponse>(responseContent);
                    if (result.ResponseType == "Success")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}