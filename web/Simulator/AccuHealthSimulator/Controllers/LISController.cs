using LIS.DtoModel.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AccuHealthSimulator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LISController : ControllerBase
    {

        [HttpGet("GetParams")]
        public GetParamsResponse GetParams([FromQuery] Guid ClientId, [FromQuery] Guid BranchId)
        {
            GetParamsResponse response = null;

            using (StreamReader r = new StreamReader("GetParams.json"))
            {
                string json = r.ReadToEnd();
                response = JsonConvert.DeserializeObject<GetParamsResponse>(json);

            }

            return response;
        }

        [HttpGet("GetPendingOrderCount")]
        public GetPendingOrderCountResponse GetPendingOrderCount([FromQuery] Guid ClientId, [FromQuery] Guid BranchId)
        {
            return new GetPendingOrderCountResponse();
        }

        [HttpGet("GetTestOrders")]
        public GetTestOrdersResponse GetTestOrders([FromQuery] Guid ClientId, [FromQuery] Guid BranchId)
        {
            GetTestOrdersResponse response = null;
           
            using (StreamReader r = new StreamReader("GetTestOrders.json"))
            {
                string json = r.ReadToEnd();
                response = JsonConvert.DeserializeObject<GetTestOrdersResponse>(json);
                
            }

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
