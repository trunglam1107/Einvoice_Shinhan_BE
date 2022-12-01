using InvoiceServer.Data.Utils;

namespace InvoiceServer.Business.Models.InvoiceTemplate
{
    public class InvoiceTemplateViewModel
    {
        [DataConvert("Id")]
        public long Id { get; set; }

        [DataConvert("Name")]
        public string Name { get; set; }

        [DataConvert("UrlFile")]
        public string UrlFile { get; set; }

        [DataConvert("ImageTemplate")]
        public string ImageTemplate { get; set; }

        [DataConvert("DetailInvoice")]
        public string DetailInvoice { get; set; }

        [DataConvert("NumberRecord")]
        public int? NumberRecord { get; set; }

        [DataConvert("IsShowOrder")]
        public bool IsShowOrder { get; set; }

        [DataConvert("Orders")]
        public int? Orders { get; set; }

        //[DataConvert("Deleted")]
        public bool? Deleted { get; set; }

        [DataConvert("Width")]
        public int Width { get; set; }

        [DataConvert("Height")]
        public int Height { get; set; }

        [DataConvert("PageSize")]
        public string PageSize { get; set; }

        [DataConvert("PageOrientation")]
        public string PageOrientation { get; set; }

        [DataConvert("LocationSignLeft")]
        public decimal LocationSignLeft { get; set; }

        [DataConvert("LocationSignButton")]
        public decimal LocationSignButton { get; set; }

        [DataConvert("NumberCharacterInLine")]
        public int? NumberCharacterInLine { get; set; }

        [DataConvert("LocationSignRight")]
        public decimal? LocationSignRight { get; set; }

        [DataConvert("FooterInvoice")]
        public string FooterInvoice { get; set; }

        [DataConvert("HeaderInvoice")]
        public string HeaderInvoice { get; set; }

        [DataConvert("IsMultiTax")]
        public bool? IsMultiTax { get; set; }

        [DataConvert("IsDiscount")]
        public bool? IsDiscount { get; set; }

        [DataConvert("NumberLineOfPage")]
        public int NumberLineOfPage { get; set; }

        public InvoiceTemplateViewModel()
        {

        }

        public InvoiceTemplateViewModel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                var propInfoSrcObj = srcObject.GetType().GetProperty("Deleted");
                if (propInfoSrcObj != null)
                {
                    this.Deleted = (bool)srcObject.GetType().GetProperty("Deleted").GetValue(srcObject);
                }
                else
                {
                    this.Deleted = false;
                }
                DataObjectConverter.Convert<object, InvoiceTemplateViewModel>(srcObject, this);
            }
        }
    }
}
