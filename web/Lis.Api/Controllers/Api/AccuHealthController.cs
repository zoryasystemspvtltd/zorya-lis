using LIS.BusinessLogic.Helper;
using LIS.DataAccess;
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

        private ApplicationDBContext dBContext;
        private ILogger logger;
        public AccuHealthController(ILogger logger, ApplicationDBContext dBContext)
        {
            this.dBContext = dBContext;
            this.logger = logger;
        }

        [AllowAnonymous]
        // GET api/<controller>
        // Sync Test Param - api/LIS/GetParams?ClientId=67DB18E8-2988-4F53-A252-B6CB0CB8873F&Branch_Id=EE09D44E-757D-4FD2-B171-1F59224390EA
        public async Task<IEnumerable<string>> Get()
        {
            var response = await GetTestParams();

            foreach (var data in response.Data)
            {
                var existing = dBContext.HisTestMaster.FirstOrDefault(p => p.HISTestCode.Equals(data.PARAMCODE));

                if (existing == null)
                {
                    dBContext.HisTestMaster.Add(new HisTestMaster()
                    {
                        HISTestCode = data.PARAMCODE,
                        HISTestCodeDescription = data.PARAMNAME,
                        HISSpecimenCode = data.PARAMCODE,
                        HISSpecimenName = data.PARAMNAME,
                        CreatedOn = DateTime.UtcNow,
                        DepartmentCode = "NA",
                        IsActive = true
                    });
                }
            }

            await dBContext.SaveChangesAsync();
            return new List<string>();
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
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