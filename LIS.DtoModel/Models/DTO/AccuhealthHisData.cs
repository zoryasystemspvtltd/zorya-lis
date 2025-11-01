using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LIS.DtoModel.Models
{
    public class GetParamsResponse
    {
        public string ResponseType { get; set; }
        public string Message { get; set; }
        public AccuHealthParamMapping[] Data { get; set; }
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

        public AccuHealthTestOrder[] Data { get; set; }
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
        public AccuHealthTestValue[] TestValues { get; set; }
    }

    [Table("AccuHealthTestValues")]
    public class AccuHealthTestValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Guid ROW_ID { get; set; }
        public bool isSynced { get; set; }
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

    }

    [Table("LisTestValues")]
    public class LisTestValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string REF_VISITNO { get; set; }
        public string PARAMCODE { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Equipment { get; set; }
    }

    [Table("AccuHealthTestOrders")]
    public class AccuHealthTestOrder
    {
        [Key]
        public Guid ROW_ID { get; set; }
        public bool isSynced { get; set; }
        public Guid branch_ID { get; set; }
        public string IPOPFLAG { get; set; }
        public string PINNO { get; set; }
        public string REF_VISITNO { get; set; }
        public string ADMISSIONNO { get; set; }
        public DateTime? REQDATETIME { get; set; }
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
        public string Value { get; set; }
        public ReportStatusType Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }

    [Table("AccuHealthParamMappings")]
    public class AccuHealthParamMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string HIS_PARAMCODE { get; set; }
        public string HIS_PARAMNAME { get; set; }
        public string LIS_PARAMCODE { get; set; }
        public string SPECIMEN { get; set; }
        public string UNIT { get; set; }

        [ForeignKey("Equipment")]
        public int EquipmentId { get; set; }

        [JsonIgnore]
        public virtual EquipmentMaster Equipment { get; set; }
    }

    public class AccuHealthSample
    {
        public string PATFNAME { get; set; }
        public string PATMNAME { get; set; }
        public string PATLNAME { get; set; }
        public string PAT_DOB { get; set; }
        public string GENDER { get; set; }
        public string PATAGE { get; set; }
        public string AGEUNIT { get; set; }
        public string SampleNo { get; set; }
        public string LisParamCode { get; set; }
        public string HIS_PARAMCODE { get; set; }
        public string SPECIMEN { get; set; }
    }
}