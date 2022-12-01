using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class RegisterType
    {
        [DataConvert("Id")]
        [JsonProperty("id")]
        public long Id { get; set; }


        [DataConvert("Name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        public RegisterType()
        {
        }
      
        public RegisterType(object srcObject)
           : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RegisterType>(srcObject, this);
            }
        }
    }
}



