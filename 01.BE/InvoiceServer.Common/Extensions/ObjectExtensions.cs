using System;

namespace InvoiceServer.Common.Extensions
{
    public static partial class ObjectExtensions
    {
        public static decimal? ToDecimal(this object value)
        {
            if (value == null)
            {
                return null;
            }

            decimal outValue = 0M;
            decimal.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static decimal ToDecimal(this object value, int defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            decimal outValue = 0;
            decimal.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static bool IsEquals(this object value, object valueCompare)
        {
            if (value == null || valueCompare == null)
            {
                return false;
            }

            return value.ToString().Equals(valueCompare.ToString());
        }

        public static bool IsEquals(this object value, string valueCompare)
        {
            if (value == null || valueCompare == null)
            {
                return false;
            }

            return value.ToString().Equals(valueCompare);
        }

        public static int? ToInt(this object value)
        {
            if (value == null)
            {
                return null;
            }

            int outValue = 0;
            int.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static int? ToInt(this object value, int valueDefault)
        {
            if (value == null)
            {
                return valueDefault;
            }

            int outValue = 0;
            int.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static double? ToDouble(this object value)
        {
            if (value == null)
            {
                return null;
            }

            double outValue = 0;
            double.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static double ToDouble(this object value, int defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            double outValue = 0;
            double.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static double? ToDouble(this object value, double valueDefault)
        {
            if (value == null)
            {
                return valueDefault;
            }

            double outValue = 0;
            double.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static bool? ToBoolean(this object value)
        {
            if (value == null)
            {
                return null;
            }

            switch (value.ConvertToString().ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                case "":
                    return false;
                default:
                    return false;
            }
        }

        public static bool IsNullOrEmpty(this object value)
        {
            return string.IsNullOrEmpty(value.ConvertToString());
        }

        public static bool IsNullOrWhitespace(this object value)
        {
            return string.IsNullOrWhiteSpace(value.ConvertToString());
        }

        public static string Trim(this object value)
        {
            return value.ToString().Trim();
        }

        public static string ToUpperAndTrim(this object value)
        {
            return value.ToString().ToUpper().Trim();
        }

        public static DateTime? ConvertDateTime(this object value)
        {
            try
            {
                if (value.IsNullOrEmpty())
                {
                    return DateTime.Now;
                }

                return Convert.ToDateTime(value);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }
        public static string ConvertToString(this object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }

        public static string ToDecimalFomat(this object value, string fomatPatten)
        {
            if (value == null)
            {
                return null;
            }
            decimal outValue = 0;
            decimal.TryParse(value.ToString(), out outValue);
            if (outValue == 0)
            {
                return String.Format(fomatPatten, outValue.ToDecimal());
            }

            return String.Format(fomatPatten, value.ToDecimal());
        }
    }
}
