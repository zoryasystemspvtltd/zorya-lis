using LIS.BusinessLogic.Helper;
using LIS.DtoModel;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lis.Api
{
    public static class GlobalScheduler
    {
        private static ILogger _logger;
        private static System.Timers.Timer _timer;
        private static readonly int SchedulerIntervalMinute = Convert.ToInt32(ConfigurationManager.AppSettings["SchedulerIntervalMinute"]);
        private static readonly string HospitalApiUrl = ConfigurationManager.AppSettings["HospitalApiUrl"];
        private static readonly string ExternalAPIBaseUri = ConfigurationManager.AppSettings["ExternalAPIBaseUri"];
        private static readonly string AccuHealthClientId = ConfigurationManager.AppSettings["AccuHealthClientId"];
        private static readonly string AccuHealthBranchId = ConfigurationManager.AppSettings["AccuHealthBranchId"];
        private static bool isRunning = false;

        public static void StartScheduler(ILogger logger)
        {
            _logger = logger;

            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 1000 * 60 * SchedulerIntervalMinute; // One Hour
            _timer.Enabled = true;
            isRunning = false;
            _logger.LogDebug("Synchronization Scheduler Enabled.");
        }

        private static async void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_logger.LogInfo("Ping His Adapter Elapsed Started.");
            //await PingHisAdapterAsync(); // 1 Hour
            //_logger.LogInfo("Ping His Adapter Elapsed End.");

            // Schedular is replaced by background Strored Procedure
            /*  */
            _logger.LogInfo("Synchronization Scheduler Elapsed Started.");
            if (!isRunning)
            {
                isRunning = true;
                if (await GetPendingOrderCount() > 0)
                {
                    var order = await GetTestOrders();

                    await ProcessTestOrder(order);
                }
                isRunning = false;
            }
            _logger.LogInfo("Synchronization Scheduler Elapsed End.");

        }

        private static async Task<int> GetPendingOrderCount()
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

                    var jsonString = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GetPendingOrderCountResponse>(jsonString);

                    var pendingCount = response.PendingCount;
                    _logger.LogInfo($"GetPendingOrderCount - {pendingCount}.");
                    return pendingCount;
                }


            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return 0;
            }
        }

        private static async Task<GetTestOrdersResponse> GetTestOrders()
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

        private static async Task ProcessTestOrder(GetTestOrdersResponse order)
        {
            foreach (var orderItem in order.Data)
            {
                // Map Accuhealth data to LIs Data

                var orderId = await SaveLisOrderAsync(orderItem);

                // Acknowledge
                if (orderId != null)
                {
                    // TODO Uncomment
                    var isSyncd = await UpdateOrderStatus(orderItem);
                }
            }
        }

        private static async Task<Guid?> SaveLisOrderAsync(AccuHealthTestOrder newOrder)
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

        private static async Task<bool> UpdateOrderStatus(AccuHealthTestOrder orderItem)
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

    }
}