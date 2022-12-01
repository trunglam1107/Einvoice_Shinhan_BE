using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportAgenciesExport
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        //public string ContractType
        //{
        //    get
        //    {
        //        if (this.ContractTypeId.HasValue)
        //        {
        //            return this.ContractTypeId.Value == 1 ? "Khách hàng" : "Đại lý";
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}

        public int? ContractTypeId { get; set; }

        public string CustomerName { get; set; }

        public List<ReportAgencies> Items { get; set; }

        public ReportAgenciesExport(ConditionReportSummary condition)
        {
            this.DateTo = condition.DateTo;
            this.DateFrom = condition.DateFrom;
            this.CustomerName = condition.CustomerName;
            //this.ContractTypeId = condition.ContactType;
        }

    }
}
