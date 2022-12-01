using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchClient : BaseConditionSearchModel
    {
        public string TaxCode { get; set; }
        public string ClientName { get; set; }
        public string CustomerCode { get; set; }
        public bool? UseTotalInvoice { get; set; }
        public bool? TaxIncentives { get; set; }
        public bool? SendInvoiceByMonth { get; set; }
        public string IsPersonal { get; set; }

        public ConditionSearchClient(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            ClientManagementInfo.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ClientManagementInfo.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
