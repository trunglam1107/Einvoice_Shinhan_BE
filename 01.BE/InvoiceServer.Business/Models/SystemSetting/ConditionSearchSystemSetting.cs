namespace InvoiceServer.Business.Models
{
    public class ConditionSearchSystemSetting
    {
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public long? CompanyId { get; private set; }

        public ConditionSearchSystemSetting(UserSessionInfo currentUser, string orderBy, string orderType)
        {
            this.CompanyId = currentUser.Company.Id;

            string macthOrderBy = "Type";
            string macthOrderType = "ASC";

            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
