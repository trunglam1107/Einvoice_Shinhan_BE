using System;
using System.Text;

namespace InvoiceServer.Common
{
    public static class FormatNumber
    {
        public static decimal? SetFractionDigit(decimal? value, int fractionDigit = 0)
        {
            if (value == null)
            {
                return null;
            }

            return Math.Round((value ?? 0.0m), fractionDigit, MidpointRounding.AwayFromZero);
        }

        public static decimal SetFractionDigit(decimal value, int fractionDigit = 0)
        {
            return Math.Round(value, fractionDigit, MidpointRounding.AwayFromZero);
        }

        public static string GetStringFormat(int? precition)
        {
            var res = "{0:#,##0";
            if (precition <= 0 || precition == null)
                return res + "}";

            StringBuilder bldRes = new StringBuilder(res);
            bldRes = bldRes.Append(".");
            for (int i = 0; i < precition; i++)
            {
                bldRes = bldRes.Append("0");
            }
            bldRes.Append("}");
            return bldRes.ToString();
        }

        public static string GetStrFormat(int? precition,decimal? value)
        {
            var res = "{0:###0";

            if (value == 0)
            {
                return res + "}";
            }

            if (precition <= 0 || precition == null)
            {
                return res + "}";
            }
            

            StringBuilder bldRes = new StringBuilder(res);
            bldRes = bldRes.Append(".");
            for (int i = 0; i < precition; i++)
            {
                bldRes = bldRes.Append("0");
            }
            bldRes.Append("}");
            return bldRes.ToString();
        }
    }
}
