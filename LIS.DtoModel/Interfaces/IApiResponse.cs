using Newtonsoft.Json;

namespace LIS.DtoModel
{
    [JsonObject]
    public interface IAPIResponse
    {
         int StatusCode { get; set; }
         string Message { get; set; }
         IApiError ResponseException { get; set; }
         object Result { get; set; }
    }

    public interface IExternalAPIResponse
    {
        string Status { get; set; }
        string PatientID { get; set; }
        string ZoryaOrderRef { get; set; }
        string Message { get; set; }
    }
}
