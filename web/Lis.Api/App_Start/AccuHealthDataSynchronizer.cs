using LIS.BusinessLogic.Helper;
using LIS.DataAccess;
using LIS.DtoModel;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Lis.Api
{
    public class AccuHealthDataSynchronizer
    {
        private ILogger _logger;
        private readonly string HospitalApiUrl = ConfigurationManager.AppSettings["HospitalApiUrl"];
        private readonly string ExternalAPIBaseUri = ConfigurationManager.AppSettings["ExternalAPIBaseUri"];
        private readonly string AccuHealthClientId = ConfigurationManager.AppSettings["AccuHealthClientId"];
        private readonly string AccuHealthBranchId = ConfigurationManager.AppSettings["AccuHealthBranchId"];

        public AccuHealthDataSynchronizer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task SynchronizeOrder()
        {
            if (await GetPendingOrderCount() > 0)
            {
                var order = await GetTestOrders();

                await ProcessTestOrder(order);
            }

        }

        public async Task SynchronizeResult()
        {
            //var query = dBContext.AccuHealthTestValues
            //    .Where(p => !p.isSynced)
            //    //.Take(50) // TODO Finetune
            //    ;

            //foreach (var order in query)
            //{
            //    order.isSynced = true;
            //}
            //var result = await query.ToArrayAsync();
            //// TODO Uncomment to send the result to Accuhealth
            //if (await PostTestResults(result))
            //{
            //    dBContext.SaveChanges();
            //}
        }

        private async Task<int> GetPendingOrderCount()
        {
            HttpResponseMessage responseMessage = null;

            GetPendingOrderCountResponse responseCount = new GetPendingOrderCountResponse();
            try
            {
                _logger.LogInfo("GetPendingOrderCount Started.");

                string apiUrl = $"{HospitalApiUrl}lis/GetPendingOrderCount?ClientId={AccuHealthClientId}";
                if (!AccuHealthBranchId.Equals(string.Empty))
                {
                    apiUrl += $"&BranchId={AccuHealthBranchId}";
                }

                using (var client = new ApiClient().GetHttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    responseMessage = await client.GetAsync(apiUrl);

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var jsonString = await responseMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<GetPendingOrderCountResponse>(jsonString);

                        var pendingCount = response.PendingCount;
                        _logger.LogInfo($"GetPendingOrderCount - {pendingCount}.");
                        return pendingCount;
                    }
                    return 0;
                }


            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return 0;
            }
        }

        private async Task<GetTestOrdersResponse> GetTestOrders()
        {
            HttpResponseMessage responseMessage = null;

            GetPendingOrderCountResponse responseCount = new GetPendingOrderCountResponse();
            try
            {
                _logger.LogInfo("GetTestOrders Started.");

                string apiUrl = $"{HospitalApiUrl}lis/GetTestOrders?ClientId={AccuHealthClientId}";
                if (!AccuHealthBranchId.Equals(string.Empty))
                {
                    apiUrl += $"&BranchId={AccuHealthBranchId}";
                }

                using (var client = new ApiClient().GetHttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    responseMessage = await client.GetAsync(apiUrl);

                    var jsonString = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GetTestOrdersResponse>(jsonString);
                    _logger.LogInfo("GetTestOrders Completed.");
                    return response;
                }


            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return null;
            }
        }

        private async Task ProcessTestOrder(GetTestOrdersResponse order)
        {
            foreach (var orderItem in order.Data)
            {
                // Map Accuhealth data to LIs Data

                var orderId = await SaveLisOrderAsync(orderItem);

                // Acknowledge
                if (orderId != orderItem.ROW_ID)
                {
                    // TODO Uncomment
                    var isSyncd = await UpdateOrderStatus(orderItem);
                }
            }
        }

        private async Task<Guid?> SaveLisOrderAsync(AccuHealthTestOrder newOrder)
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                _logger.LogInfo("SaveLisOrder Started.");
                var apiUrl = $"{ExternalAPIBaseUri}api/AccuHealth";
                using (var client = new ApiClient().GetHttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var payload = newOrder;
                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        // Convert JSON string to dynamic object
                        var result = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        return Guid.Parse(result.Result?.ToString());
                    }


                }

                _logger.LogInfo("SaveLisOrder End.");

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return null;
            }
        }

        private async Task<bool> UpdateOrderStatus(AccuHealthTestOrder orderItem)
        {
            string apiUrl = $"{HospitalApiUrl}lis/UpdateOrderStatus";
            using (var client = new ApiClient().GetHttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var payload = new
                {
                    ClientId = AccuHealthClientId,
                    ROW_ID = orderItem.ROW_ID,
                    isSynced = true
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

        private async Task<bool> PostTestResults(AccuHealthTestValue[] results)
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