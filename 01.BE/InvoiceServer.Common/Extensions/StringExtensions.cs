using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using InvoiceServer.Common.Constants;

namespace InvoiceServer.Common.Extensions
{
    /// <summary>
    /// Extensions for <see cref="System.String"/>
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Logger logger = new Logger();
        /// <summary>
        /// A nicer way of calling <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// A nicer way of calling the inverse of <see cref="System.String.IsNullOrEmpty(string)"/>
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is not null or an empty string (""); otherwise, false.</returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !value.IsNullOrEmpty();
        }

        /// <summary>
        /// A nicer way of calling <see cref="System.String.Format(string, object[])"/>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// Returns a string array containing the trimmed substrings in this <paramref name="value"/>
        /// that are delimited by the provided <paramref name="separators"/>.
        /// </summary>
        public static IEnumerable<string> SplitAndTrim(this string value, params char[] separators)
        {
            Ensure.Argument.ArgumentNotNull(value, "source");

            return value.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        }

        /// <summary>
        /// Checks if the <paramref name="source"/> contains the <paramref name="input"/> based on the provided <paramref name="comparison"/> rules.
        /// </summary>
        public static bool Contains(this string source, string input, StringComparison comparison)
        {
            return source.IndexOf(input, comparison) >= 0;
        }

        /// <summary>
        /// Limits the length of the <paramref name="source"/> to the specified <paramref name="maxLength"/>.
        /// </summary>
        public static string Limit(this string source, int maxLength)
        {
            if (source.IsNullOrEmpty() || source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength).Trim();
        }

        public static bool IsOverLength(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            else
            {
                return str.Length > maxLength;
            }
        }

        public static bool IsEmail(this string str)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(str);
                return mailAddress.Address == str;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNumeric(this string str)
        {
            double valueConvert = 0;
            return double.TryParse(str, out valueConvert);
        }


        public static string EmptyNull(this string str)
        {
            return str ?? "";
        }

        public static bool IsEquals(this string str, string value)
        {
            return str.EmptyNull().Equals(value);
        }

        public static bool IsEqualUpper(this string str, string value)
        {
            return str.EmptyNull().ToUpper().Equals(value.EmptyNull().ToUpper());
        }

        public static T ParseEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str, true);
        }

        public static bool IsEmumDefined<T>(this string str)
        {
            return Enum.IsDefined(typeof(T), str);
        }

        public static bool IsEnumType<T>(this string str)
        {
            bool isEnumType = true;
            try
            {
                ParseEnum<T>(str);
            }
            catch
            {
                isEnumType = false;
            }

            return isEnumType;
        }

        public static bool IsPhoneNumber(this string str)
        {
            try
            {
                return Regex.Match(str, @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$").Success;
            }
            catch
            {
                return false;
            }
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static bool ToBoolean(this string value)
        {
            switch (value.EmptyNull().ToLower())
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
                    throw new InvalidCastException("You can't cast a weird value to a bool!");
            }
        }

        public static decimal? ToDecimal(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            decimal outValue = 0;
            decimal.TryParse(value, out outValue);
            return outValue;
        }


        public static string ToUpperAndStrim(this string value)
        {
            return value.EmptyNull().Trim().ToUpper();
        }

        public static string[] SplitLineFeed(this string value)
        {
            return value.Split(new string[] { "\n" }, StringSplitOptions.None);
        }

        public static int NumberLine(this string value, int maxlength, double ratioConvertUpperCase)
        {
            int numberLine = 0;
            if (value.IsNullOrEmpty())
            {
                return 1;
            }

            int maxlenghtCharacter = maxlength;
            string[] array = SplitLineFeed(value);
            foreach (var item in array)
            {
                if (item.IsNullOrEmpty())
                {
                    continue;
                }

                if (item.NumberCharactorUpdateCase() > (maxlength / 2))
                {
                    maxlenghtCharacter = (int)(maxlength / ratioConvertUpperCase);
                }

                numberLine = numberLine + RoundNumber(item.GetLength(), maxlenghtCharacter);
            }

            return numberLine;
        }

        public static int NumberLineNoteItem(this string value, int maxlength, double ratioConvertUpperCase)
        {
            int numberLine = 0;
            int maxlenghtCharacter = maxlength;
            string[] array = SplitLineFeed(value);
            foreach (var item in array)
            {
                if (item.NumberCharactorUpdateCase() > (maxlength / 2))
                {
                    maxlenghtCharacter = (int)(maxlength / ratioConvertUpperCase);
                }

                numberLine = numberLine + RoundNumber(item.GetLength(), maxlenghtCharacter);
            }

            return numberLine;
        }

        private static int RoundNumber(int divisor, int divided)
        {
            if (divisor == 0)
            {
                return 1;
            }

            int result = (divisor / divided);
            int overbalance = divisor % divided;
            if (overbalance > 0)
            {
                result = result + 1;
            }

            return result;
        }

        public static string BreackLine(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return str.Replace("\n", "<br>");
        }

        private static int NumberCharactorUpdateCase(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return 0;
            }

            return str.ToCharArray().Count(c => char.IsUpper(c));
        }

        public static string ConvertString(this decimal? val)
        {
            return String.Format("{0:0.00}", val).Replace(".00", "");
        }

        private static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        public static bool IsChinese(this char c)
        {
            return cjkCharRegex.IsMatch(c.ToString());
        }

        public static int GetHeight(this string str, int lineHeightDefault, int bonusHeight)
        {
            foreach (var item in str)//foreach (var item in str.ToCharArray())
            {
                if (item.IsChinese())
                {
                    return lineHeightDefault + bonusHeight;
                }
            }
            return lineHeightDefault;
        }

        public static int GetLength(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return 0;
            }
            int halfSize = 0;
            int numberFullSize = 0;
            foreach (var item in str)/* foreach (var item in str.ToCharArray())*/
            {
                if (item.IsChinese())
                {
                    halfSize++;
                }
                else
                {
                    numberFullSize++;
                }
            }

            return numberFullSize + (halfSize * 2);
        }

        public static double ConvertToDouble(this string value)
        {
            double result = 0;
            if (value.IsNumeric())
            {
                result = double.Parse(value);
            }

            return result;
        }
        public static DateTime? ConvertDateTime(this string timeStamp)
        {
            try
            {
                if (timeStamp.IsNullOrEmpty())
                {
                    return null;
                }

                return DateTime.ParseExact(timeStamp, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DateTime? ConvertDateTimeUTC(this string timeStamp)
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

        public static string DecodeUrl(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            return HttpUtility.UrlDecode(value, Encoding.UTF8);
        }

        public static int ToInt(this string value, int valueDefault)
        {
            if (value == null)
            {
                return valueDefault;
            }

            int outValue = 0;
            int.TryParse(value.ToString(), out outValue);
            return outValue;
        }

        public static int LengthOfText(this string value)
        {
            if (value == null)
            {
                return 0;
            }

            return value.Length;

        }

        public static string ConvertToString(this string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.ToString();

        }

        public static List<string> ConvertToList(this string value, char character)
        {
            if (value.IsNullOrEmpty())
            {
                return new List<string>();
            }

            List<string> listData = value.Split(character).ToList();
            return listData;
        }


        public static string GetAccount(this string value)
        {
            if (value.IsNullOrEmpty())
            {
                return string.Empty;
            }
            string[] arrayCharacter = value.Split(' ');
            if (arrayCharacter.Length == 0)
            {
                return null;
            }
            StringBuilder bld = new StringBuilder();
            bld = bld.Append(arrayCharacter[arrayCharacter.Length - 1]);
            for (int i = 0; i < arrayCharacter.Length - 1; i++)
            {
                string item = arrayCharacter[i];
                if (item.IsNullOrEmpty())
                {
                    continue;
                }

                bld = bld.Append(arrayCharacter[i].Substring(0, 1).ToLower());
            }
            string accountId = bld.ToString();
            return accountId.ToAscii();
        }

        public static string ToAscii(this string unicode)
        {
            if (string.IsNullOrEmpty(unicode)) return "";
            string result = unicode.ToLower().Trim();
            string[] arrSrc = new string[] { " ", "&", "'", ">","<","!",":","#",".","+",
                "~","@","$","%","^","*","(",")",",","}","{","]","[",";","?","/","\\","\"","“","”",
                "Đ","đ", "ê", "â", "ư", "ơ", "ă","ô",
                    "ế", "ấ", "ứ", "ớ", "ắ","á","ú","ó","ố","í","ý","é",
                    "ề", "ầ", "ừ", "ờ", "ằ","à","ù","ò","ồ","ì","ỳ","è",
                    "ể", "ẩ", "ử", "ở", "ẳ","ả","ủ","ỏ","ổ","ỉ","ỷ","ẻ",
                    "ễ", "ẫ", "ữ", "ỡ", "ẵ","ã","ũ","õ","ỗ","ĩ","ỹ","ẽ",
                    "ệ", "ậ", "ự", "ợ", "ặ","ạ","ụ","ọ","ộ","ị","ỵ","ẹ"};
            string[] arrDest = new string[] { "-", "", "", "", "", "","","","","","","",
                "","","","","","","","","","","","","","","","","","",
                "D","d", "e", "a", "u", "o", "a","o",
                    "e", "a", "u", "o", "a","a","u","o","o","i","y","e",
                    "e", "a", "u", "o", "a","a","u","o","o","i","y","e",
                    "e", "a", "u", "o", "a","a","u","o","o","i","y","e",
                    "e", "a", "u", "o", "a","a","u","o","o","i","y","e",
                    "e", "a", "u", "o", "a","a","u","o","o","i","y","e"};
            for (int ct = 0; ct < arrSrc.Length; ct++)
            {
                result = result.Replace(arrSrc[ct].ToString(), arrDest[ct].ToString());
            }
            return result;
        }

        public static int FindNumber(this string str)
        {
            if (str == null) return 0;
            String number = new String(str.Where(Char.IsDigit).ToArray());
            return ToInt(number, 0);
        }

        public static string EscapeXMLValue(this string xmlString)
        {

            if (xmlString.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return xmlString.Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("&", "&amp;");
        }

        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }

        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        public static string MappingSymbol(int? lHDSDung, int? hTHDon, string date, int? type, string symbol)
        {

            var dateNow = DateTime.Now;
            var yearDate = "";
            string dateDeclaration = "";

            if (dateNow != null && dateNow.Year > 0)
            {
                yearDate = dateNow.Year.ToString().Substring(2);
            }
            if (date == yearDate)
            {
                dateDeclaration = date;

            }
            else
            {
                dateDeclaration = yearDate.ToString();
            }
            HTHDon.ListHTHDon.TryGetValue(hTHDon, out string typehTHDon);
            TypeINVOICEDECLARATION.ListTypeInvoice.TryGetValue(type, out string typeInvoice);
            string mappingSymbol = lHDSDung.ToString() + typehTHDon + dateDeclaration + typeInvoice + symbol;

            return mappingSymbol;
        }

        public static string EncryptAesManaged(string raw)
        {
            try
            {
                string encryptedText = null;
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = Encoding.ASCII.GetBytes(AES.Key);
                    aes.IV = Encoding.ASCII.GetBytes(AES.IV);
                    byte[] encrypted = Encrypt(raw, aes.Key, aes.IV);
                    encryptedText = Convert.ToBase64String(encrypted);
                    return encryptedText;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return null;
        }

        static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            return encrypted;
        }
        public static string Decrypt(string base64cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = base64cipherText;
            try
            {

                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64cipherText)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                                plaintext = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                logger.Error("password not encrypt", new Exception($"Password not encrypt: {base64cipherText}"));
            }
            return plaintext;
        }
    }
}