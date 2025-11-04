using Antlr.Runtime.Misc;
using Lis.Api.Providers;
using LIS.Businesslogic;
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
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;

namespace Lis.Api.Controllers.Api
{
    public class PatientsController : ApiController
    {
        private ILogger logger;
        private ApplicationDBContext dBContext;
        private AccuHealthDataSynchronizer accuHealthDataSynchronizer;
        private IResponseManager responseManager;
        public PatientsController(
             ILogger Logger
            , ApplicationDBContext dBContext
            , AccuHealthDataSynchronizer accuHealthDataSynchronizer
            , IResponseManager responseManager)
        {
            logger = Logger;
            this.dBContext = dBContext;
            this.accuHealthDataSynchronizer = accuHealthDataSynchronizer;
            this.responseManager = responseManager;
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

        [AllowAnonymous]
        [HttpPut]
        public async Task<HttpResponseMessage> Put(List<PatientStatus> iDs)
        {
            try
            {
                var accuHealthResults = new List<AccuHealthTestValue>();

                foreach (PatientStatus item in iDs)
                {
                    var recordsToUpdate = dBContext.AccuHealthTestOrders
                    .Join(dBContext.AccuHealthParamMappings,
                        o => o.PARAMCODE,
                        pm => pm.HIS_PARAMCODE,
                        (o, pm) => new { o, pm })
                    .Join(dBContext.EquipmentMaster,
                        x => x.pm.EquipmentId,
                        e => e.Id,
                        (x, e) => new { x.o, x.pm, e })
                    .FirstOrDefault(p => p.o.ROW_ID == item.Id);

                    if (recordsToUpdate != null)
                    {

                        var accuHealth = new AccuHealthTestValue()
                        {
                            ROW_ID = recordsToUpdate.o.ROW_ID,
                            isSynced = true,
                            SRNO = recordsToUpdate.o.REF_VISITNO,
                            SDATE = recordsToUpdate.o.REQDATETIME,
                            SAMPLEID = recordsToUpdate.o.REF_VISITNO,
                            TESTID = recordsToUpdate.o.PARAMCODE,
                            MACHINEID = recordsToUpdate.e.Name,
                            SUFFIX = "",
                            TRANSFERFLAG = "",
                            TMPVALUE = recordsToUpdate.o.Value,
                            DESCRIPTION = recordsToUpdate.o.PARAMCODE,
                            RUNDATE = DateTime.Now,
                        };
                        accuHealthResults.Add(accuHealth);
                    }
                }
                
                if (accuHealthResults != null)
                {
                    await accuHealthDataSynchronizer.PostTestResults(accuHealthResults);

                    dBContext.AccuHealthTestValues.AddRange(accuHealthResults);
                    dBContext.SaveChanges();
                }
                APIResponse aPIResponse = responseManager.CreateResponse(HttpStatusCode.OK, "Result sent to AccuHealth.", null, null);

                return Request.CreateResponse<APIResponse>(HttpStatusCode.OK, aPIResponse);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return null;
            }
        }


        public class PatientStatus
        {
            public Guid Id { get; set; }
            public string note { get; set; }
            public int Status { get; set; }
        }
    }
}
