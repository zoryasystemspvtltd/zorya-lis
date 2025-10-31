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
    public class AccuHealthController : ApiController
    {
        private static readonly string HospitalApiUrl = ConfigurationManager.AppSettings["HospitalApiUrl"];
        private static readonly string ExternalAPIBaseUri = ConfigurationManager.AppSettings["ExternalAPIBaseUri"];
        private static readonly string AccuHealthClientId = ConfigurationManager.AppSettings["AccuHealthClientId"];
        private static readonly string AccuHealthBranchId = ConfigurationManager.AppSettings["AccuHealthBranchId"];

        private IResponseManager responseManager;
        private ApplicationDBContext dBContext;
        private ILogger logger;
        public AccuHealthController(ILogger logger, IResponseManager responseManager, ApplicationDBContext dBContext)
        {
            this.dBContext = dBContext;
            this.logger = logger;
            this.responseManager = responseManager;
        }


        /// <summary>
        /// Saving new order from Accuhealth
        /// </summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>`
        [AllowAnonymous]
        public HttpResponseMessage Post(AccuHealthTestOrder newOrder)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.PreconditionFailed, ModelState.Keys);
                }
                var isExists = dBContext.AccuHealthTestOrders.Any(o => o.ROW_ID == newOrder.ROW_ID);
                if (!isExists)
                {
                    dBContext.AccuHealthTestOrders.Add(newOrder);
                    dBContext.SaveChanges();

                    APIResponse aPIResponse = responseManager.CreateResponse(HttpStatusCode.OK, "New Sample added successfully", null, newOrder.ROW_ID);

                    return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
                
                }
                else
                {
                    logger.LogError($"Duplicate Test Order {newOrder.ROW_ID}");
                    return null;
                }
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