using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using System.Net;

namespace LIS.Businesslogic
{
    public class ResponseManager : IResponseManager
    {
        public APIResponse CreateResponse(HttpStatusCode httpStatusCode, string message, IApiError apiError, object result)
        {
            return new APIResponse()
            {
                StatusCode = (int)httpStatusCode,
                Message = message,
                Result = result,
                ResponseException = apiError
            };
        }
        public ExternalAPIResponse CreateExternalAPIResponse(string status, string message, string patienId, long orderRef)
        {
            return new ExternalAPIResponse()
            {
                Status = status,
                Message = message,
                PatientID = patienId,
                ZoryaOrderRef = orderRef == 0 ? null : "ZOR-" + orderRef
            };
        }
    }
}
