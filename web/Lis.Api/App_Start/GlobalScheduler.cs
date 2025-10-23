using Lis.Api.App_Start;
using LIS.BusinessLogic.Helper;
using LIS.BusinessLogic.Helper;
using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.DtoModel.Models.ExternalApi;
using LIS.Logger;
using log4net.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        private static async Task ProcessTestOrder(GetTestOrdersResponse order)
        {
            foreach (var orderItem in order.Data)
            {
                // Map Accuhealth data to LIs Data

                var newOrder = new NewOrder()
                {
                    PatientDetail = new PatientDetail()
                    {
                        Name = $"{orderItem.PATFNAME} {orderItem.PARAMNAME} {orderItem.PATLNAME}",
                        DateOfBirth = !string.IsNullOrEmpty(orderItem.PAT_DOB) ? Convert.ToDateTime(orderItem.PAT_DOB) : DateTime.Now,
                        Gender = orderItem.GENDER,
                        HisPatientId = orderItem.REF_VISITNO,
                        ROW_ID = orderItem.ROW_ID,
                    }

                };
                var testRequestDetails = new List<TestRequestDetail>();

                testRequestDetails.Add(new TestRequestDetail()
                {
                    HISTestName = orderItem.TESTPROF_CODE,
                    HISTestCode = orderItem.TESTPROF_CODE,
                    SampleNo = orderItem.ADMISSIONNO, // Sample number must be unique, normally is is barcode number
                    //SampleCollectionDate = orderItem.REQDATETIME,
                    //SampleReceivedDate = orderItem.REQDATETIME,
                    //SpecimenCode = orderItem.PARAMCODE,
                    //SpecimenName = orderItem.PARAMNAME,
                    HISRequestNo = orderItem.REF_VISITNO,
                    HISRequestId = orderItem.REF_VISITNO,



                    IPOPFLAG = orderItem.IPOPFLAG,
                    PINNO = orderItem.PINNO,
                    REF_VISITNO = orderItem.REF_VISITNO,
                    ADMISSIONNO = orderItem.ADMISSIONNO,
                    REQDATETIME = orderItem.REQDATETIME,
                    TESTPROF_CODE = orderItem.TESTPROF_CODE,
                    PROCESSED = orderItem.PROCESSED,
                    PATFNAME = orderItem.PATFNAME,
                    PATMNAME = orderItem.PATMNAME,
                    PATLNAME = orderItem.PATLNAME,
                    PAT_DOB = orderItem.PAT_DOB,
                    GENDER = orderItem.GENDER,
                    PATAGE = orderItem.PATAGE,
                    AGEUNIT = orderItem.AGEUNIT,
                    DOC_NAME = orderItem.DOC_NAME,
                    PATIENTTYPECLASS = orderItem.PATIENTTYPECLASS,
                    SEQNO = orderItem.SEQNO,
                    ADDDATE = orderItem.ADDDATE,
                    ADDTIME = orderItem.ADDTIME,
                    TITLE = orderItem.TITLE,
                    LABNO = orderItem.LABNO,
                    DATESTAMP = orderItem.DATESTAMP,
                    PARAMCODE = orderItem.PARAMCODE,
                    PARAMNAME = orderItem.PARAMNAME,
                    MRESULT = orderItem.MRESULT,
                    BC_PRINTED = orderItem.BC_PRINTED,
                    ROW_ID = orderItem.ROW_ID,
                    isSynced = orderItem.isSynced,
                    branch_ID = orderItem.branch_ID

                });

                newOrder.TestRequestDetails = testRequestDetails;


                var orderId = await SaveLisOrderAsync(newOrder);

                // Acknowledge
                if (orderId > 0)
                {
                    var isSyncd = await UpdateOrderStatus(orderItem);
                }
            }
        }

        private static async Task<long> SaveLisOrderAsync(NewOrder newOrder)
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                _logger.LogInfo("SaveLisOrder Started.");
                var apiUrl = $"{ExternalAPIBaseUri}api/NewSample";
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
                        return Convert.ToInt64(result.Result);
                    }

                   
                }

                _logger.LogInfo("SaveLisOrder End.");

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return 0;
            }
        }

        private static async Task<bool> UpdateOrderStatus(TestOrdersData orderItem)
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

        private static async Task PingHisAdapterAsync()
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                _logger.LogInfo("Ping His Adapter Started.");

                using (var client = new ApiClient().GetHttpClient())
                {
                    responseMessage = await client.GetAsync($"{ExternalAPIBaseUri}api/ping");
                }

                _logger.LogInfo("Ping His Adapter End.");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
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
        private static async Task SyncTestRequisitionAsync()
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                _logger.LogInfo("Synchronization Test Requisition Started.");
                var testDetails = new List<TestDetail>();

                using (var client = new ApiClient().GetHttpClient())
                {
                    isRunning = true;
                    responseMessage = await client.GetAsync(HospitalApiUrl);
                    isRunning = false;
                }

                _logger.LogInfo("Synchronization Test Requisition End.");
            }
            catch (Exception ex)
            {
                isRunning = false;
                _logger.LogException(ex);
            }
        }
    }
}