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
    
    public partial class GATEWAY_LOG
    {
        public long ID { get; set; }
        public string NAME { get; set; }
        public string BODY { get; set; }
        public string OBJECTNAME { get; set; }
        public string IP { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public Nullable<System.DateTime> CREATEDBY { get; set; }
        public Nullable<long> COMPANYID { get; set; }
    }
}
