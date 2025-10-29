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
        private static readonly string HospitalApiUrl = ConfigurationManager.AppSettings["HospitalApiUrl"];
        private static readonly string ExternalAPIBaseUri = ConfigurationManager.AppSettings["ExternalAPIBaseUri"];
        private static readonly string AccuHealthClientId = ConfigurationManager.AppSettings["AccuHealthClientId"];
        private static readonly string AccuHealthBranchId = ConfigurationManager.AppSettings["AccuHealthBranchId"];

        private IResponseManager responseManager;
        private ApplicationDBContext dBContext;
        private ILogger logger;
        private IModuleIdentity identity;
        public AccuHealthLisController(ILogger logger, IResponseManager responseManager, ApplicationDBContext dBContext, IModuleIdentity identity )
        {
            this.dBContext = dBContext;
            this.logger = logger;
            this.responseManager = responseManager;
            this.identity = identity;
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


                var testOrderList = dBContext.AccuHealthTestOrders
                            .Where(p => p.TESTPROF_CODE.Equals(Id, StringComparison.OrdinalIgnoreCase) && 
                                            p.Status == ReportStatusType.New)
                            .Join(dBContext.AccuHealthParamMappings,
                                t => t.PARAMCODE,
                                pm => pm.HIS_PARAMCODE,
                                (t, pm) => new { t, pm })
                            .Join(dBContext.EquipmentMaster,
                                tmp => tmp.pm.EquipmentId,
                                e => e.Id,
                                (tmp, e) => new { tmp.t, tmp.pm, e })
                            .Where(x => x.e.IsActive &&
                                    x.e.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase))
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

                var responseStrign = JsonConvert.SerializeObject(testOrderList);
                logger.LogInfo($"Get Sample Response: {responseStrign}");
                return testOrderList;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }


        /// <summary>
        /// Save the list of result received from LIS Console
        /// </summary>
        /// <param name="testValue"></param>
        /// <returns></returns>`
        [AllowAnonymous]
        public HttpResponseMessage Post(LisTestValue[] values)
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
                }

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

        

        private async Task<GetParamsResponse> GetTestParams()
        {
            HttpResponseMessage responseMessage = null;

            GetPendingOrderCountResponse responseCount = new GetPendingOrderCountResponse();
            try
            {
                logger.LogInfo("GetTestParams Started.");

                string apiUrl = $"{HospitalApiUrl}lis/GetParams?ClientId={AccuHealthClientId}";
                if (!AccuHealthBranchId.Equals(string.Empty))
                {
                    apiUrl += $"&BranchId={AccuHealthBranchId}";
                }

                using (var client = new ApiClient().GetHttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    responseMessage = await client.GetAsync(apiUrl);

                    var jsonString = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GetParamsResponse>(jsonString);
                    logger.LogInfo("GetTestParams Completed.");
                    return response;
                }


            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                return null;
            }
        }
    }

    
}