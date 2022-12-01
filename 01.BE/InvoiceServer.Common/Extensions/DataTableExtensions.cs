using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
namespace InvoiceServer.Common.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, row[prop.Name], null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return new List<T>();
            }
        }

        public static bool CheckColumnIsExisted(this DataTable table, IEnumerable<string> columnName, out string errorMessage)
        {
            string msg = string.Empty;
            bool isExisted = true;
            foreach (var item in columnName)
            {
                if (!table.Columns.Contains(item))
                {
                    msg = string.Format("Column name [{0}] in sheet name[{1}] is not exist]", item, table.TableName);
                    isExisted = false;
                    break;
                }
            }

            errorMessage = msg;
            return isExisted;
        }

        public static DataTable RemoveRowSpace(this DataTable table)
        {
            if (table.Rows.Count == 0)
            {
                return table;
            }

            var dataOfDataTable = table.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is System.DBNull || string.Compare((field.ConvertToString()).Trim(), string.Empty) == 0));
            if (!dataOfDataTable.Any())
            {
                return new DataTable();
            }

            return dataOfDataTable.CopyToDataTable();
        }
    }
}
