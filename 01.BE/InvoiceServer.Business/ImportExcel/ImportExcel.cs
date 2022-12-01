using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace InvoiceServer.Business
{
    public class ImportExcel
    {
        private const string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES;'";
        private readonly OleDbConnection oleDbConnection = null;
        private static readonly Logger logger = new Logger();
        private readonly Dictionary<string, DataTable> dicMasterData = null;

        public ImportExcel(string fullPathToExcel, IEnumerable<string> sheetNames)
        {
            oleDbConnection = new OleDbConnection(string.Format(ConnectionString, fullPathToExcel));
            dicMasterData = LoadData(sheetNames);
        }

        public DataTable GetBySheetName(string sheetName)
        {
            if (!dicMasterData.ContainsKey(sheetName))
            {
                return null;
            }

            return dicMasterData[sheetName];
        }

        public DataTable GetBySheetName(string sheetName, List<string> columnImport, ref ListError<ImportRowError> listRowError)
        {
            DataTable dataBysheetName = GetBySheetName(sheetName);
            if (dataBysheetName == null)
            {
                return null;
            }

            dataBysheetName.TableName = sheetName;
            foreach (var item in columnImport)
            {
                logger.Error(item.ToString(), new Exception("GetBySheetName"));
                if (!dataBysheetName.Columns.Contains(item))
                {
                    listRowError.Add(new ImportRowError(ResultCode.ImportColumnIsNotExist, item, null));
                }
            }

            return dataBysheetName.RemoveRowSpace();

        }

        private Dictionary<string, DataTable> LoadData(IEnumerable<string> sheetNames)
        {
            Dictionary<string, DataTable> masterData = new Dictionary<string, DataTable>();
            try
            {
                using (oleDbConnection)
                {
                    oleDbConnection.Open();
                    DataTable dtSheet = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    foreach (DataRow item in dtSheet.Rows)
                    {
                        string sheetName = item["TABLE_NAME"].ToString().Replace("$", "");
                        if (!sheetNames.Contains(sheetName))
                        {
                            continue;
                        }

                        DataTable dataOfSheet = GetDataBySheetName(sheetName, oleDbConnection);
                        if (masterData.ContainsKey(sheetName))
                        {
                            masterData[sheetName] = dataOfSheet;
                        }
                        else
                        {
                            masterData.Add(sheetName, dataOfSheet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("File import format invalid", ex);
            }

            return masterData;
        }

        private DataTable GetDataBySheetName(string sheetName, OleDbConnection oleDbConnection)
        {
            string queryData = string.Format("SELECT * from [{0}$]", sheetName);
            DataTable data = new DataTable();
            using (OleDbCommand oleDbCommand = new OleDbCommand(queryData, oleDbConnection))
            {
                using (OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader())
                {
                    data.Load(oleDbDataReader);
                    return data;
                }
            }
        }
    }
}
