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
    
    public partial class KEYSTORE
    {
        public long ID { get; set; }
        public Nullable<long> COMPANYID { get; set; }
        public string TYPE { get; set; }
        public string SERIALNUMBER { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public Nullable<System.DateTime> UPDATEDDATE { get; set; }
    
        public virtual MYCOMPANY MYCOMPANY { get; set; }
    }
}
