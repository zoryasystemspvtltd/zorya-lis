namespace AccuHealthSimulator;

public class GetParamsResponse
{
    public GetParamsResponse()
    {
        Data[0] = new ParamData()
        {
            PARAM_ID = "DUMMY ID",
            PARAM_NAME = "DUMMY NAME",
            VALUE = "DUMMY VALUE"
        };
    }
    public string ResponseType { get; set; } = "Success";
    public string Message { get; set; } = "Test orders retrieved successfully.";
    public ParamData[] Data { get; set; } = new ParamData[1];
}

public class ParamData
{
    public string PARAM_ID { get; set; }
    public string PARAM_NAME { get; set; }
    public string VALUE { get; set; }
}

public class GetPendingOrderCountResponse()
{
    public string ResponseType { get; set; } = "Success";
    public string Message { get; set; } = "Pending order count retrieved successfully.";

    public int PendingCount { get; set; } = 1;
}

public class GetTestOrdersResponse()
{
    public string ResponseType { get; set; } = "Success";
    public string Message { get; set; } = "Test orders retrieved successfully.";

    public TestOrdersData[] Data { get; set; } = new TestOrdersData[1];
}

public class TestOrdersData
{
    public string REF_VISITNO { get; set; }
    public string ADMISSIONNO { get; set; }
    public string REQDATETIME { get; set; }
    public string TESTPROF_CODE { get; set; }
    public string PROCESSED { get; set; }
    public string PARAMCODE { get; set; }
    public string PARAMNAME { get; set; }
    public Guid ROW_ID { get; set; }
    public bool isSynced { get; set; }
    public Guid branch_ID { get; set; }
}


public class UpdateOrderStatusRuquest()
{
    public Guid ClientId { get; set; }
    public Guid ROW_ID { get; set; }
    public bool isSynced { get; set; }
}
public class UpdateOrderStatusResponse()
{
    public string ResponseType { get; set; } = "Success";
    public string Message { get; set; } = "Order status synced successfully.";
}

public class PostTestResultsResponse()
{
    public string ResponseType { get; set; } = "Success";
    public string Message { get; set; } = "Test results synced successfully.";
}

public class PostTestResultsRuquest()
{
    public Guid ClientId { get; set; }
    public TestValuesData[] TestValues { get; set; } = new TestValuesData[1];
}

public class TestValuesData
{
    public string SRNO { get; set; }
    public DateTime SDATE { get; set; }
    public string SAMPLEID { get; set; }
    public string TESTID { get; set; }
    public string MACHINEID { get; set; }
    public string SUFFIX { get; set; }
    public string TRANSFERFLAG { get; set; }
    public string TMPVALUE { get; set; }
    public string DESCRIPTION { get; set; }
    public DateTime RUNDATE { get; set; }
    public Guid ROW_ID { get; set; }
}