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
    
    public partial class PRODUCT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PRODUCT()
        {
            this.INVOICEDETAILs = new HashSet<INVOICEDETAIL>();
        }
    
        public long ID { get; set; }
        public string PRODUCTCODE { get; set; }
        public string PRODUCTNAME { get; set; }
        public Nullable<decimal> PRICE { get; set; }
        public string UNIT { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<long> CREATEDBY { get; set; }
        public Nullable<long> TAXID { get; set; }
        public Nullable<System.DateTime> UPDATEDDATE { get; set; }
        public Nullable<long> UPDATEDBY { get; set; }
        public Nullable<bool> DELETED { get; set; }
        public Nullable<System.DateTime> CREATEDDATE { get; set; }
        public Nullable<bool> NOTDISPLAY { get; set; }
        public Nullable<long> UNITID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INVOICEDETAIL> INVOICEDETAILs { get; set; }
        public virtual TAX TAX { get; set; }
        public virtual UNITLIST UNITLIST { get; set; }
    }
}