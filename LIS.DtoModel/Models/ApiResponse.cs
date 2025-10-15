using Newtonsoft.Json;

namespace LIS.DtoModel
{
    [JsonObject]
    public class APIResponse : IAPIResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public IApiError ResponseException { get; set; }
        public object Result { get; set; }
    }
    public class ExternalAPIResponse : IExternalAPIResponse
    {
        public string Status { get; set; }
        public string PatientID { get; set; }
        public string ZoryaOrderRef { get; set; }
        public string Message { get; set; }
    }
}
