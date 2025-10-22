using Microsoft.AspNetCore.Mvc;

namespace AccuHealthSimulator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LISController : ControllerBase
    {

        [HttpGet("GetParams")]
        public GetParamsResponse GetParams()
        {
            return new GetParamsResponse();
        }

        [HttpGet("GetPendingOrderCount")]
        public GetPendingOrderCountResponse GetPendingOrderCount()
        {
            // return new GetPendingOrderCountResponse();
            var response = new GetPendingOrderCountResponse()
            {
                PendingCount = 2
            };

            return response;
        }

        [HttpGet("GetTestOrders")]
        public GetTestOrdersResponse GetTestOrders()
        {
            var response = new  GetTestOrdersResponse();
            response.Data[0] = new TestOrdersData()
            {
                REF_VISITNO = "RV10023",
                ADMISSIONNO = "ADM987654",
                REQDATETIME = "2025-10-15T09:42:00",
                TESTPROF_CODE = "CBC001",
                PROCESSED = "N",
                PARAMCODE = "HB",
                PARAMNAME = "Hemoglobin",
                ROW_ID = Guid.NewGuid(),
                isSynced = false,
                branch_ID = Guid.NewGuid()
            };
            return response;
        }

        [HttpPost("UpdateOrderStatus")]
        public UpdateOrderStatusResponse UpdateOrderStatus(UpdateOrderStatusRuquest ruquest)
        {
            return new UpdateOrderStatusResponse();
        }

        [HttpPost("PostTestResults")]
        public PostTestResultsResponse PostTestResults(PostTestResultsRuquest ruquest)
        {
            return new PostTestResultsResponse();
        }
    }
}
