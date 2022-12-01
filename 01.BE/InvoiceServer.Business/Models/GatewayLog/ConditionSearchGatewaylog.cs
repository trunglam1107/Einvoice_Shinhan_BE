using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.GatewayLog
{
    public class ConditionSearchGatewaylog : BaseConditionSearchModel
    {
        public string GatewaylogName { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public long? companyId { get; set; }

        public string language { get; set; }
        public ConditionSearchGatewaylog(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            GatewaylogManagementInfo.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? GatewaylogManagementInfo.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }

        public ConditionSearchGatewaylog()
        {

        }

    }
}
