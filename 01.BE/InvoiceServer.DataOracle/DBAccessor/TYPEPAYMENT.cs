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
    
    public partial class TYPEPAYMENT
    {
        public long ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<int> ORDERS { get; set; }
    }
}