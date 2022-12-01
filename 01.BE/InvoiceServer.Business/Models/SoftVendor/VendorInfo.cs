using InvoiceServer.Data.Utils;
using System;

namespace InvoiceServer.Business.Models
{
    public class VendorInfo
    {
        [DataConvert("Id")]
        public long Id { get; set; }// 

        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }// 

        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }// 

        [DataConvert("Address")]
        public string Address { get; set; }// 

        public string ContractNo { get; set; }

        public string ContractDate { get; set; }

        public VendorInfo()
        {
            this.ContractNo = "01";
            this.ContractDate = DateTime.Now.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public VendorInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, VendorInfo>(srcObject, this);
            }
        }
    }
}
