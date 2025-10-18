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
            return new GetPendingOrderCountResponse();
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
                ROW_ID = "b7d93c65-1d23-42f1-8b43-7a812f6b1e0b",
                isSynced = false,
                branch_ID = "9e89e6de-62f7-4ad1-a4c0-f43a51979f3b"
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
