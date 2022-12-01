using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class ReleaseAnnouncementInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        //[JsonProperty("no")]
        //public string InvoiceNo { get; set; }

        public ReleaseAnnouncementInfo() 
        { 

        }
        public ReleaseAnnouncementInfo(object srcObject) 
            :this() 
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseAnnouncementInfo>(srcObject, this);
            }
        }
    }
}
