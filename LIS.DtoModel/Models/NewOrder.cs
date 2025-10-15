using LIS.DtoModel.Interfaces;
using System.Collections.Generic;

namespace LIS.DtoModel.Models
{
    public class NewOrder : INewOrder
    {
        public PatientDetail PatientDetail { get; set; }
        public IEnumerable<TestRequestDetail> TestRequestDetails { get; set; }
    }
    public class PatientOrder : IPatientOrder
    {
        public PatientInfo PatientInfo { get; set; }       
    }
}
