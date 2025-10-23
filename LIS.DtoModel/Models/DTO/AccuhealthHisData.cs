using System;

namespace LIS.DtoModel.Models
{
    public class GetParamsResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }
        public ParamData[] Data { get; set; }
    }

    public class ParamData
    {
        public string PARAM_ID { get; set; }
        public string PARAM_NAME { get; set; }
        public string VALUE { get; set; }
    }

    public class GetPendingOrderCountResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }
        public int PendingCount { get; set; }
    }

    public class GetTestOrdersResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }

        public TestOrdersData[] Data { get; set; }
    }

    public class TestOrdersData
    {
        public string IPOPFLAG { get; set; }
        public string PINNO { get; set; }
        public string REF_VISITNO { get; set; }
        public string ADMISSIONNO { get; set; }
        public string REQDATETIME { get; set; }
        public string TESTPROF_CODE { get; set; }
        public string PROCESSED { get; set; }
        public string PATFNAME { get; set; }
        public string PATMNAME { get; set; }
        public string PATLNAME { get; set; }
        public string PAT_DOB { get; set; }
        public string GENDER { get; set; }
        public string PATAGE { get; set; }
        public string AGEUNIT { get; set; }
        public string DOC_NAME { get; set; }
        public string PATIENTTYPECLASS { get; set; }
        public string SEQNO { get; set; }
        public string ADDDATE { get; set; }
        public string ADDTIME { get; set; }
        public string TITLE { get; set; }
        public string LABNO { get; set; }
        public DateTime? DATESTAMP { get; set; }
        public string PARAMCODE { get; set; }
        public string PARAMNAME { get; set; }
        public string MRESULT { get; set; }
        public string BC_PRINTED { get; set; }
        public Guid ROW_ID { get; set; }
        public bool isSynced { get; set; }
        public Guid branch_ID { get; set; }
    }


    public class UpdateOrderStatusRuquest
    {
        public Guid ClientId { get; set; }
        public Guid ROW_ID { get; set; }
        public bool isSynced { get; set; }
    }
    public class UpdateOrderStatusResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }
    }

    public class PostTestResultsResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }
    }

    public class PostTestResultsRuquest
    {
        public Guid ClientId { get; set; }
        public TestValuesData[] TestValues { get; set; }
    }

    public class TestValuesData
    {
        public string SRNO { get; set; }
        public DateTime? SDATE { get; set; }
        public string SAMPLEID { get; set; }
        public string TESTID { get; set; }
        public string MACHINEID { get; set; }
        public string SUFFIX { get; set; }
        public string TRANSFERFLAG { get; set; }
        public string TMPVALUE { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime RUNDATE { get; set; }
        public Guid ROW_ID { get; set; }
        public bool isSynced { get; set; }
    }
}