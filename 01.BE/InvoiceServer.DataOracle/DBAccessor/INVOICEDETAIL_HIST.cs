//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InvoiceServer.Data.DBAccessor
{
    using System;
    using System.Collections.Generic;
    
    public partial class INVOICEDETAIL_HIST
    {
        public long ID { get; set; }
        public Nullable<long> INVOICEID { get; set; }
        public Nullable<long> PRODUCTID { get; set; }
        public Nullable<decimal> QUANTITY { get; set; }
        public Nullable<decimal> PRICE { get; set; }
        public Nullable<long> TAXID { get; set; }
        public Nullable<decimal> TOTAL { get; set; }
        public Nullable<decimal> AMOUNTTAX { get; set; }
        public Nullable<bool> DISCOUNT { get; set; }
        public Nullable<decimal> AMOUNTDISCOUNT { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public Nullable<int> ADJUSTMENTTYPE { get; set; }
        public string DISCOUNTDESCRIPTION { get; set; }
        public Nullable<int> DISCOUNTRATIO { get; set; }
        public string PRODUCTNAME { get; set; }
        public string UNITNAME { get; set; }
        public Nullable<decimal> SUM { get; set; }
        public Nullable<System.DateTime> HIST_DATE { get; set; }
    }
}
