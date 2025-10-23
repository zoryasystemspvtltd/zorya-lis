using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Lis.Api.Controllers.Api
{
    public class PatientsController : ApiController
    {
        private ITestRequestDetailsManager testmanager;
        private IPatientDetailsManager manager;
        private IResponseManager responseMgr;
        private ILogger logger;
        public PatientsController(IPatientDetailsManager equipmentManager,
             IResponseManager responseManager
            , ILogger Logger
            , ITestRequestDetailsManager testmanager)
        {
            manager = equipmentManager;
            responseMgr = responseManager;
            logger = Logger;
            this.testmanager = testmanager;
        }

        private ListOptions ApiOption
        {
            get
            {
                var apiOption = System.Web.HttpContext.Current.Request.Headers.GetValues("ApiOption");
                if (apiOption == null || apiOption.Count() == 0)
                {
                    throw new KeyNotFoundException("Invalid Option specified");
                }

                var option = JsonConvert.DeserializeObject<ListOptions>(apiOption.FirstOrDefault(),
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });
                return option;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ItemList<TestRequestDetail> Get()
        {
            try
            {
                var patients = manager.Get(ApiOption);

                return patients;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public PatientDetail Get(long Id)
        {
            try
            {
                var patient = manager.Get(Id);

                return patient;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost]
       // [ActionName("patient-test-order")]
        public HttpResponseMessage Post([FromBody] PatientOrder newOrder)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.PreconditionFailed, ModelState.Keys);
                }

                long testRequestDetailsId = manager.CreateNewOrderFromAPI(newOrder);
                ExternalAPIResponse aPIResponse = responseMgr.CreateExternalAPIResponse("success", "Order received successfully",newOrder.PatientInfo.PatientId, testRequestDetailsId);

                return Request.CreateResponse<ExternalAPIResponse>(HttpStatusCode.Created, aPIResponse);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                ExternalAPIResponse aPIResponse = responseMgr.CreateExternalAPIResponse("error", e.Message, newOrder.PatientInfo.PatientId, 0);

                return Request.CreateResponse<ExternalAPIResponse>(HttpStatusCode.BadRequest, aPIResponse);
            }
        }

        [AllowAnonymous]
        [HttpPut]
        public HttpResponseMessage Put(List<AuthorizeRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.PreconditionFailed, ModelState.Keys);
                }
                foreach (var sample in request)
                {
                    if (sample.Status == ReportStatusType.DoctorApproved
                        || sample.Status == ReportStatusType.DoctorRejected)
                    {
                        testmanager.DoctorReview(sample.Id, sample.Status, sample.Note, sample.RunIndex);
                    }
                    if (sample.Status == ReportStatusType.TechnicianApproved
                        || sample.Status == ReportStatusType.TechnicianRejected
                        || sample.Status == ReportStatusType.New)
                    {
                        testmanager.TechnicianReview(sample.Id, sample.Status, sample.Note, sample.RunIndex);
                    }

                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }
    }
}
