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
    
    public partial class CURRENCY
    {
        public long ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public Nullable<decimal> EXCHANGERATE { get; set; }
        public string DECIMALSEPARATOR { get; set; }
        public string DECIMALUNIT { get; set; }
    }
}
