using Antlr.Runtime.Misc;
using Lis.Api.Providers;
using LIS.DataAccess;
using LIS.DtoModel;
using LIS.DtoModel.Interfaces;
using LIS.DtoModel.Models;
using LIS.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Lis.Api.Controllers.Api
{
    public class PatientsController : ApiController
    {
         private ILogger logger;
        private ApplicationDBContext dBContext;
        public PatientsController(
             ILogger Logger
            , ApplicationDBContext dBContext)
        {
            logger = Logger;
            this.dBContext = dBContext;
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
        public ItemList<AccuHealthTestOrder> Get()
        {
            try
            {
                ListOptions option = ApiOption;
                return GetTestOrder(option);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        private ItemList<AccuHealthTestOrder> GetTestOrder(ListOptions option)
        {
            var result = new ItemList<AccuHealthTestOrder>();
            var query = dBContext.AccuHealthTestOrders.Where(p => p.Status == option.Status);

            if (!string.IsNullOrEmpty(option.SearchText))
            {
                DateTime searchDate;
                bool isValidSearchDate = DateTime.TryParseExact(option.SearchText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out searchDate);
                if (option.SearchText.Contains("/") && isValidSearchDate)
                {
                    query = query.Where(p => p.CreatedAt.Year == searchDate.Year
                                                && p.CreatedAt.Month == searchDate.Month
                                                && p.CreatedAt.Day == searchDate.Day);
                }
                else
                {
                    query = query.Where(p => p.REF_VISITNO.Contains(option.SearchText)
                                || p.ADMISSIONNO.Contains(option.SearchText)
                                || p.PATFNAME.Contains(option.SearchText)
                                || p.PATMNAME.Contains(option.SearchText)
                                || p.PATLNAME.Contains(option.SearchText));
                }
            }

            result.TotalRecord = query.Count();

            int minRow = (option.CurrentPage - 1) * option.RecordPerPage;
            int maxRow = (option.CurrentPage) * option.RecordPerPage;

            int totalRecordToBeSelected = ((result.TotalRecord - minRow) > option.RecordPerPage)
                ? option.RecordPerPage : (result.TotalRecord - minRow);

            //option.SortColumnName = string.IsNullOrEmpty(option.SortColumnName)
            //    ? "SampleCollectionDate" : option.SortColumnName;

            //TODO SQL View
            //if (!option.SortColumnName.Equals("SampleNo", StringComparison.OrdinalIgnoreCase)
            //    && !option.SortColumnName.Equals("PatientStatus", StringComparison.OrdinalIgnoreCase)
            //    && !option.SortColumnName.Equals("HISTestCode", StringComparison.OrdinalIgnoreCase)
            //    && !option.SortColumnName.Equals("sampleCollectionDate", StringComparison.OrdinalIgnoreCase))
            //{
            //    option.SortColumnName = "CreatedAt";
            //    option.SortDirection = false;
            //}

            option.SortColumnName = "CreatedAt";
            option.SortDirection = false;

            if (option.RecordPerPage == 0)
            {
                totalRecordToBeSelected = result.TotalRecord;
            }

            result.Items = query
                .OrderBy(option.SortColumnName, option.SortDirection)
                .Skip(minRow)
                .Take(totalRecordToBeSelected)
                .ToList();


            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        public PatientDetail Get(long Id)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }

        //[AllowAnonymous]
        //[HttpPut]
        //public HttpResponseMessage Put(List<AuthorizeRequest> request)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.PreconditionFailed, ModelState.Keys);
        //        }
        //        foreach(var sample in request)
        //        {
        //            if (sample.Status == ReportStatusType.DoctorApproved 
        //                || sample.Status == ReportStatusType.DoctorRejected)
        //            {
        //                testmanager.DoctorReview(sample.Id, sample.Status, sample.Note, sample.RunIndex);
        //            }
        //            if (sample.Status == ReportStatusType.TechnicianApproved
        //                || sample.Status == ReportStatusType.TechnicianRejected
        //                || sample.Status == ReportStatusType.New)
        //            {
        //                testmanager.TechnicianReview(sample.Id, sample.Status, sample.Note, sample.RunIndex);
        //            }
                    
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogException(e);
        //        return null;
        //    }
        //}
    }
}
