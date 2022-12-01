using InvoiceServer.Data.Utils;

namespace InvoiceServer.Business.Models
{
    public class ReceiverInfo
    {
        public string UrlResetPassword { get; set; }

        [DataConvert("UserName")]
        public string UserName { get; set; }

        [DataConvert("UserId")]
        public string UserId { get; set; }


        [DataConvert("Email")]
        public string Email { get; set; }

        public string TypeEmail { get; set; }

        public ReceiverInfo()
        {
        }

        /// <summary>
        /// Constructor current ReceiverInfo object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public ReceiverInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReceiverInfo>(srcObject, this);
            }

        }

        public ReceiverInfo(object srcObject, string typeEmail)
            : this(srcObject)
        {
            TypeEmail = typeEmail;
        }

    }
}
