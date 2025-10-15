using System.Net;

namespace LIS.DtoModel.Interfaces
{
    public interface IResponseManager
    {
        APIResponse CreateResponse(HttpStatusCode httpStatusCode, string message, IApiError apiError, object result);
        ExternalAPIResponse CreateExternalAPIResponse(string status, string message, string patientid,long orderRef);
    }
}
