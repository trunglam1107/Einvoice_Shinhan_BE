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
    
    public partial class FUNCTION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FUNCTION()
        {
            this.ROLEFUNCTIONs = new HashSet<ROLEFUNCTION>();
        }
    
        public long ID { get; set; }
        public Nullable<long> USERLEVELSID { get; set; }
        public string FUNCTIONNAME { get; set; }
        public string ACTION { get; set; }
        public string DETAILFUNCTION { get; set; }
        public string DETAILFUNCTIONEN { get; set; }
        public Nullable<int> ORDERS { get; set; }
        public Nullable<bool> DELETED { get; set; }
    
        public virtual USERLEVEL USERLEVEL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ROLEFUNCTION> ROLEFUNCTIONs { get; set; }
    }
}
