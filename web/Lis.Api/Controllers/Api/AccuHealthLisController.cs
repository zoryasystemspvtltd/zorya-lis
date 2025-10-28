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
using System.Threading.Tasks;
using System.Web.Http;

namespace Lis.Api.Controllers.Api
{
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
        public AccuHealthLisController(ILogger logger, IResponseManager responseManager, ApplicationDBContext dBContext, IModuleIdentity identity)
        {
            this.dBContext = dBContext;
            this.logger = logger;
            this.responseManager = responseManager;
            this.identity = identity;
        }

        [AllowAnonymous]
        [HttpGet]
        // GET api/<controller>
        // Sync Test Param - api/LIS/GetParams?ClientId=67DB18E8-2988-4F53-A252-B6CB0CB8873F&Branch_Id=EE09D44E-757D-4FD2-B171-1F59224390EA
        public bool Get()
        {
            try
            {
                logger.LogInfo($"Ping API Request");
                bool isValid = false;
                var equipment = dBContext.EquipmentMaster.Where(p => p.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                isValid = (equipment != null);
                logger.LogInfo($"Ping API Response");
                return isValid;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return false;
            }
        }

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
        /// Saving new order from Accuhealth
        /// </summary>
        /// <param name="listresult"></param>
        /// <returns></returns>`
        [AllowAnonymous]
        public HttpResponseMessage Post(IEnumerable<LisTestValue> listresult)
        {
            try
            {
                HttpResponseMessage response = null;
                foreach (var testValue in listresult)
                {
                    var order = dBContext.AccuHealthTestOrders
                                .Join(dBContext.AccuHealthParamMappings,
                                    t => t.PARAMCODE,
                                    pm => pm.HIS_PARAMCODE,
                                    (t, pm) => new { t, pm })
                                .Join(dBContext.EquipmentMaster,
                                    temp => temp.pm.EquipmentId,
                                    e => e.Id,
                                    (temp, e) => new { temp.t, temp.pm, e })
                                .Where(x => x.e.AccessKey.Equals(identity.AccessKey, StringComparison.OrdinalIgnoreCase)
                                         && x.t.REF_VISITNO == testValue.REF_VISITNO
                                         && x.pm.LIS_PARAMCODE == testValue.PARAMCODE)
                                .Select(x => x.t)
                                .FirstOrDefault();

                    if (order != null)
                    {
                        order.Value = testValue.Value;
                        dBContext.SaveChanges();

                        APIResponse aPIResponse = responseManager.CreateResponse(HttpStatusCode.OK, "New Sample added successfully", null, order.ROW_ID);

                        response = Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);

                    }
                    else
                    {
                        response = null;
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
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