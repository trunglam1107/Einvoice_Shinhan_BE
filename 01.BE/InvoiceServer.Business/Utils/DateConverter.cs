using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Utils
{
    public static class DateConverter
    {
        public static DateTime? ConvertDateTimeUTC(string timeStamp)
        {
            DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            double numberTimeStamp = 0;
            bool canConvert = Double.TryParse(timeStamp, out numberTimeStamp);
            if (!canConvert)
            {
                return null;
            }

            var timeComvert = Convert.ToDouble(timeStamp);

            return BaseTime.AddMilliseconds(timeComvert);
        }

        public static DateTime? ConvertDateTime(string timeStamp)
        {
            try
            {
                if (timeStamp.IsNullOrEmpty())
                {
                    return null;
                }

                return Convert.ToDateTime(timeStamp);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
