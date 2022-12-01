using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReleaseInvoiceMaster
    {
        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime ReleasedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        public string Description { get; set; }

        [StringTrimAttribute]
        [JsonProperty("fileReceipt")]
        public string FileReceipt { get; set; }

        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long? CompanyId { get; set; }

        [JsonIgnore()]
        public long? UserActionId { get; set; }

        [JsonProperty("items")]
        public List<ReleaseInvoiceInfo> ReleaseInvoiceInfos { get; set; }

        public ReleaseInvoiceMaster()
        {

        }
        public ReleaseInvoiceMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseInvoiceMaster>(srcObject, this);
            }
        }

        public void SetDataForReleaseInvoice(INVOICE invoice, long? companyId, long userAction, string description)
        {
            if (invoice == null)
            {
                return;
            }
            this.UserActionId = userAction;
            this.Description = description;
            this.CompanyId = companyId;
            this.ReleaseInvoiceInfos = new List<ReleaseInvoiceInfo>();
            this.ReleaseInvoiceInfos.Add(new ReleaseInvoiceInfo() { InvoiceId = invoice.ID, InvoiceNo = invoice.NO });
            this.ReleasedDate = DateTime.Now;
        }

        public void SetDataForReleaseInvoice(INVOICE invoice, INVOICE invoiceReplaced, long? companyId, long userAction, string description)
        {
            SetDataForReleaseInvoice(invoice, companyId, userAction, description);
            if (invoiceReplaced == null)
            {
                return;
            }

            this.ReleaseInvoiceInfos.Add(new ReleaseInvoiceInfo() { InvoiceId = invoiceReplaced.ID, InvoiceNo = invoiceReplaced.NO });
        }

        public void SetDataForReleaseInvoice(List<ReleaseInvoiceInfo> releaseInvoices, long? companyId, long userAction, string description, DateTime dateRelease)
        {
            this.UserActionId = userAction;
            this.Description = description;
            this.CompanyId = companyId;
            this.ReleaseInvoiceInfos = releaseInvoices;
            this.ReleasedDate = dateRelease;
        }

    }
}
