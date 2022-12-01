namespace InvoiceServer.Business.Models
{
    public class BaseConditionSearchModel
    {
        public UserSessionInfo CurrentUser { get; set; }
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public long TotalRecords { get; set; }
        public long? CompanyId
        {
            get
            {
                if (CurrentUser != null)
                {
                    return CurrentUser.Company.Id;
                }
                else
                {
                    return 0;
                }
            }
        }

        public BaseConditionSearchModel()
        {
            Skip = 0;
            Take = int.MaxValue;
            ColumnOrder = "Id";
            OrderType = "ASC";
            TotalRecords = 0;
        }
    }
}
