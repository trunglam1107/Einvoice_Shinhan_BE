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
    
    public partial class INVOICEDECLARATIONRELEASE
    {
        public long ID { get; set; }
        public long DECLARATIONID { get; set; }
        public string RELEASECOMPANYNAME { get; set; }
        public string SERI { get; set; }
        public Nullable<System.DateTime> FROMDATE { get; set; }
        public Nullable<System.DateTime> TODATE { get; set; }
        public Nullable<int> RELEASETYPE { get; set; }
        public Nullable<bool> ISDELETED { get; set; }
        public Nullable<long> REGISTERTYPEID { get; set; }
    
        public virtual INVOICEDECLARATION INVOICEDECLARATION { get; set; }
        public virtual INVOICEDECLARATIONRELEASEREGISTERTYPE INVOICEDECLARATIONRELEASEREGISTERTYPE { get; set; }
    }
}
