using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleasesStatus
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonIgnore()]
        public long ActionUserId { get; set; }

        public InvoiceReleasesStatus()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceReleasesStatus(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceReleasesStatus>(srcObject, this);
            }
        }
        public InvoiceReleasesStatus(long id, RecordStatus satus)
        {
            this.Id = id;
            this.Status = (int)satus;
        }
    }
}
