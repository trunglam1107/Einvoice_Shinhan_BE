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
    
    public partial class REGISTERTEMPLATE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public REGISTERTEMPLATE()
        {
            this.HOLDINVOICEs = new HashSet<HOLDINVOICE>();
            this.INVOICEs = new HashSet<INVOICE>();
            this.INVOICEDECLARATIONs = new HashSet<INVOICEDECLARATION>();
            this.INVOICERELEASESDETAILs = new HashSet<INVOICERELEASESDETAIL>();
            this.NOTIFICATIONUSEINVOICEDETAILs = new HashSet<NOTIFICATIONUSEINVOICEDETAIL>();
            this.REPORTCANCELLINGDETAILs = new HashSet<REPORTCANCELLINGDETAIL>();
        }
    
        public long ID { get; set; }
        public Nullable<long> INVOICESAMPLEID { get; set; }
        public Nullable<long> COMPANYID { get; set; }
        public string CODE { get; set; }
        public string PREFIX { get; set; }
        public string SUFFIX { get; set; }
        public Nullable<long> INVOICETEMPLATEID { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<int> STATUS { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HOLDINVOICE> HOLDINVOICEs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INVOICE> INVOICEs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INVOICEDECLARATION> INVOICEDECLARATIONs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INVOICERELEASESDETAIL> INVOICERELEASESDETAILs { get; set; }
        public virtual INVOICESAMPLE INVOICESAMPLE { get; set; }
        public virtual INVOICETEMPLATE INVOICETEMPLATE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICATIONUSEINVOICEDETAIL> NOTIFICATIONUSEINVOICEDETAILs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REPORTCANCELLINGDETAIL> REPORTCANCELLINGDETAILs { get; set; }
    }
}
