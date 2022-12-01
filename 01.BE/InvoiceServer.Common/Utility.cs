using System;

namespace InvoiceServer.Common
{
    /// <summary>
    /// This class provides common utility logics
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Determines whether two specified String objects have the same value.
        /// </summary>
        /// <param name="str1">The first string to compare, or null.</param>
        /// <param name="str2">The second string to compare, or null.</param>
        /// <param name="ignoreCase">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns><c>true</c> if the value of the str1 parameter is equal to the value of the str1 parameter; otherwise, <c>false</c>.</returns>
        public static bool Equals(string str1, string str2, bool ignoreCase)
        {
            if (ignoreCase)
            {
                return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return string.Equals(str1, str2);
            }
        }

        /// <summary>
        /// Determines whether two specified String objects have the same value. Comparity use case-insensitive.
        /// </summary>
        /// <param name="str1">The first string to compare, or null.</param>
        /// <param name="str2">The second string to compare, or null.</param>
        /// <returns><c>true</c> if the value of the str1 parameter is equal to the value of the str1 parameter; otherwise, <c>false</c>.</returns>
        public static bool Equals(string str1, string str2)
        {
            return Equals(str1, str2, true);
        }

        /// <summary>
        /// Check value of a nullable boolean variable is true
        /// </summary>
        /// <param name="nullableBoolValue">Variable to check</param>
        /// <returns><c>true</c> if nullableBoolValue is not null and its value is true, otherwise <c>false</c></returns>
        public static bool IsTrue(bool? nullableBoolValue)
        {
            return nullableBoolValue ?? false;
        }

        /// <summary>
        ///  Validations datetime  
        /// </summary>
        /// <param name="datetime">Data will check</param>
        /// <returns><c>true</c> if datetime between 01/01/1900, 06/06/2079, <c>false</c> otherwise</returns>
        public static bool IsSmallDateTime(DateTime? datetime)
        {
            if (datetime != null)
            {
                DateTime dateFrom = new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
                DateTime dateTo = new DateTime(2079, 06, 06, 23, 59, 59, DateTimeKind.Utc);
                DateTime dateCheck = datetime.Value;

                return !(dateCheck < dateFrom || dateCheck > dateTo);
            }
            else
            {
                return true;
            }
        }

        public static string GetValueByIndex(string[] array, int index)
        {
            if (array == null || index > array.Length - 1)
            {
                return string.Empty;
            }

            return array[index];
        }

    }
}
