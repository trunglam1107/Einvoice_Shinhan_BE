using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportCustomerExport
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public long? ContractTypeId { get; set; }

        public string CustomerName { get; set; }

        public List<ReportCustomer> Items { get; set; }

        public ReportCustomerExport(ConditionReportSummary condition)
        {
            this.DateTo = condition.DateTo;
            this.DateFrom = condition.DateFrom;
            this.CustomerName = condition.CustomerName;
        }

    }
}
