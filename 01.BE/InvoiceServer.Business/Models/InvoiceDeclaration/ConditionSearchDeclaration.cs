using InvoiceServer.Common.Constants;
using System;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Enums;

namespace InvoiceServer.Business.Models
{ 
    public class ConditionSearchDeclaration
    {
        public int? Status { get; private set; }
        public long? CompanyID { get; private set; }
        public string ColumnOrder { get; set; }
        public string Order_Type { get; set; }
        public string CompanyName { get; private set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }

        public ConditionSearchDeclaration(int? Status ,int companyID)
        {
            this.CompanyID = companyID;
            this.Status = Status;
        }

        public ConditionSearchDeclaration(UserSessionInfo currentUser, string status, string orderBy, string orderType, int companyID)
        {
            if (Enum.IsDefined(typeof(RecordStatus), status.ToInt(0)))
            {
                this.Status = (int)status.ParseEnum<RecordStatus>();
            }
            if (status.IsNotNullOrEmpty())
            {
                this.Status = Convert.ToInt32(status);
            }
            string macthOrderBy;
            string macthOrderType;
            INVOICEDECLARATIONSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? INVOICEDECLARATIONSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.Order_Type = macthOrderType;
            //this.CompanyID = currentUser.Company.Id;      
            this.CompanyID = companyID;
        }
    }
}
