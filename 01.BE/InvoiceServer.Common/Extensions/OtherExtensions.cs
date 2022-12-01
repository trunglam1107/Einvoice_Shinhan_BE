using InvoiceServer.Common.Constants;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace InvoiceServer.Common.Extensions
{
    /// <summary>
    /// Những extension lẻ, không tạo một class riêng được thì đưa vào đây
    /// </summary>
    public static class OtherExtensions
    {
        /// <summary>
        /// Xử lý thông tin từ table invoice nếu là khách hàng vãng lai (không có Customer Code), không cần table client
        /// </summary>
        /// <returns>True: nếu ngân hàng này cần xử lý thông tin cho khách hàng vãng lai và không có CustomerCode, ngược lại return Fasle </returns>
        public static bool IsCurrentClient(string CustomerCode)
        {
            return IsCurrentClient()
                && (CustomerCode == BIDCDefaultFields.CURRENT_CLIENT);
        }

        /// <summary>
        /// Xử lý thông tin từ table invoice nếu là khách hàng vãng lai (không có Customer Code), không cần table client
        /// </summary>
        /// <returns>True: nếu ngân hàng này cần xử lý thông tin cho khách hàng vãng lai, ngược lại return Fasle </returns>
        public static bool IsCurrentClient()
        {
            return CustomerIndex.IsBIDC;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}