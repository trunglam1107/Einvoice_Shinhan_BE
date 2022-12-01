using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    partial class InvoiceBO
    {
        private bool isDiscountTemplate = false;
        private bool isMultiTaxTemplate = false;

        public ResultImportSheet ImportData(UploadInvoice dataUpload, string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            //ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoice.SheetNames);
            //DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoice.SheetName, ImportInvoice.ColumnImport, ref rowError);
            logger.Error("Start read file", new Exception("ImportData"));
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoiceNew.SheetNames);
            DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoiceNew.SheetName, ImportInvoiceNew.ColumnImport, ref rowError);
            logger.Error("Done read file", new Exception(dtInvoice.Rows.Count.ToString()));
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtInvoice == null || dtInvoice.Rows.Count == 0)
            {
                return null;
            }

            //var dtDistintc = dtInvoice.AsEnumerable()
            //       .GroupBy(r => new { IDDocument = r.Field<Object>(ImportInvoice.InvoiceIndentity).ConvertToString() })
            //       .Select(g => g.First()).CopyToDataTable();

            //var registerTemplate = this.registerTemplates.GetById(dataUpload.RegisterTemplateId);
            //var invSample = registerTemplate?.INVOICESAMPLE;
            //this.isDiscountTemplate = invSample?.ISDISCOUNT ?? false;
            //this.isMultiTaxTemplate = invSample?.ISMULTITAX ?? false;

            //try
            //{
            //    transaction.BeginTransaction();
            //    List<long> listInvoiceImportsuccess;
            //    listResultImport = ProcessData(dataUpload, companyId, dtDistintc, dtInvoice, out listInvoiceImportsuccess);
            //    if (listResultImport.ErrorCode == ResultCode.NoError)
            //    {
            //        InsertQueueCreateFile(listInvoiceImportsuccess);
            //        transaction.Commit();
            //    }
            //    else
            //    {
            //        transaction.Rollback();
            //    }
            //}
            //catch
            //{
            //    transaction.Rollback();
            //    throw;
            //}

            try
            {
                //transaction.BeginTransaction();
                List<long> listInvoiceImportsuccess;
                listResultImport = ProcessData_NEW(dataUpload, dtInvoice, out listInvoiceImportsuccess);
                //if (listResultImport.ErrorCode == ResultCode.NoError)
                //{

                //}
                //else
                //{
                //    transaction.Rollback();
                //}

                InsertQueueCreateFile(listInvoiceImportsuccess);
                //transaction.Commit();

            }
            catch
            {
                //transaction.Rollback();
                throw;
            }

            return listResultImport;

        }

        public ResultImportSheet ImportDataInvoiceReplace(UploadInvoice dataUpload, string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            logger.Error("Start read file", new Exception("ImportData"));
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoiceReplace.SheetNames);
            DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoiceReplace.SheetName, ImportInvoiceReplace.ColumnImport, ref rowError);
            logger.Error("Done read file", new Exception(dtInvoice.Rows.Count.ToString()));
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtInvoice == null || dtInvoice.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                //transaction.BeginTransaction();
                List<long> listInvoiceImportsuccess;
                listResultImport = ProcessData_InvoiceReplace(dataUpload, dtInvoice, out listInvoiceImportsuccess);

                InsertQueueCreateFile(listInvoiceImportsuccess);

            }
            catch
            {
                //transaction.Rollback();
                throw;
            }

            return listResultImport;
        }

        public ResultImportSheet ImportDataInvoiceAdjusted(UploadInvoice dataUpload, string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            logger.Error("Start read file", new Exception("ImportData"));
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoiceAdjustForInvAdjusted.SheetNames);
            DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoiceAdjustForInvAdjusted.SheetName, ImportInvoiceReplace.ColumnImport, ref rowError);
            logger.Error("Done read file", new Exception(dtInvoice.Rows.Count.ToString()));
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtInvoice == null || dtInvoice.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                //transaction.BeginTransaction();
                List<long> listInvoiceImportsuccess;
                listResultImport = ProcessData_InvoiceAdjustForInvAdjusted(dataUpload, dtInvoice, out listInvoiceImportsuccess);

                InsertQueueCreateFile(listInvoiceImportsuccess);

            }
            catch
            {
                //transaction.Rollback();
                throw;
            }

            return listResultImport;
        }


        private ResultImportSheet ProcessData_NEW(UploadInvoice dataUpload, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;
            logger.Error("Start insert " + dtInvoice.Rows.Count, new Exception(DateTime.Now.ToString()));
            transaction.BeginTransaction();
            foreach (DataRow row in dtInvoice.Rows)
            {
                //logger.Error("Start insert " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
                rowIndex++;
                if (rowIndex > 1)
                {
                    TAX tax = this.taxRepository.GetTaxBYCODE(row[ImportInvoiceNew.TaxRate].Trim());

                    if (tax == null)
                    {
                        continue;
                    }

                    string invoicetype = row[ImportInvoiceNew.InvoiceType].Trim();

                    string feeamount = row[ImportInvoiceNew.Feeamount].Trim();

                    if (invoicetype.Equals("1") && Convert.ToDecimal(feeamount) < 0)
                    {
                        continue;
                    }

                    listResultImport.RowError.AddRange(ValidateHeaderInvoiceNew(row, rowIndex));
                    listResultImport.RowError.AddRange(ExcecuteData_New(dataUpload, row, rowIndex, dtInvoice, out invoiceId));
                    listInvoiceImportsuccess.Add(invoiceId);
                }
                //logger.Error("End insert: " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
            }
            logger.Error("Insert success: " + listInvoiceImportsuccess.Count, new Exception(DateTime.Now.ToString()));
            return listResultImport;
        }

        private ResultImportSheet ProcessData_InvoiceReplace(UploadInvoice dataUpload, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;
            logger.Error("Start insert " + dtInvoice.Rows.Count, new Exception(DateTime.Now.ToString()));
            transaction.BeginTransaction();
            foreach (DataRow row in dtInvoice.Rows)
            {
                //logger.Error("Start insert " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
                rowIndex++;
                if (rowIndex > 1)
                {
                    TAX tax = this.taxRepository.GetTax(row[ImportInvoiceReplace.Taxrate].Trim());

                    if (tax == null)
                    {
                        continue;
                    }

                    string invoicetype = row[ImportInvoiceReplace.LoaiHoaDon].Trim();

                    string feeamount = row[ImportInvoiceReplace.Feeamount].Trim();

                    if (invoicetype.Equals("1") && Convert.ToDecimal(feeamount) < 0)
                    {
                        continue;
                    }

                    listResultImport.RowError.AddRange(ValidateHeaderInvoiceReplace(row, rowIndex));
                    listResultImport.RowError.AddRange(ExcecuteData_InvoiceReplace(dataUpload, row, rowIndex, dtInvoice, out invoiceId));
                    listInvoiceImportsuccess.Add(invoiceId);
                }
                //logger.Error("End insert: " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
            }
            logger.Error("Insert success: " + listInvoiceImportsuccess.Count, new Exception(DateTime.Now.ToString()));
            return listResultImport;
        }
        private ResultImportSheet ProcessData_InvoiceAdjustForInvAdjusted(UploadInvoice dataUpload, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;
            logger.Error("Start insert " + dtInvoice.Rows.Count, new Exception(DateTime.Now.ToString()));
            transaction.BeginTransaction();
            foreach (DataRow row in dtInvoice.Rows)
            {
                //logger.Error("Start insert " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
                rowIndex++;
                if (rowIndex > 1)
                {
                    TAX tax = this.taxRepository.GetTax(row[ImportInvoiceAdjustForInvAdjusted.Taxrate].Trim());

                    if (tax == null)
                    {
                        continue;
                    }

                    string invoicetype = row[ImportInvoiceAdjustForInvAdjusted.LoaiHoaDon].Trim();

                    string feeamount = row[ImportInvoiceAdjustForInvAdjusted.Feeamount].Trim();

                    if (invoicetype.Equals("1") && Convert.ToDecimal(feeamount) < 0)
                    {
                        continue;
                    }

                    listResultImport.RowError.AddRange(ValidateHeaderInvoiceAdjustForInvAdjusted(row, rowIndex));
                    listResultImport.RowError.AddRange(ExcecuteData_InvoiceAdjustForInvAdjusted(dataUpload, row, rowIndex, dtInvoice, out invoiceId));
                    listInvoiceImportsuccess.Add(invoiceId);
                }
                //logger.Error("End insert: " + DateTime.Now.ToString("yyyyMMdd HHmm:ss:fff"), new Exception("ExcecuteData_New"));
            }
            logger.Error("Insert success: " + listInvoiceImportsuccess.Count, new Exception(DateTime.Now.ToString()));
            return listResultImport;
        }
        private ResultImportSheet ProcessData(UploadInvoice dataUpload, long companyId, DataTable dtDistintc, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;

            // rowIndex lúc trước lấy theo dtDistinct nên sai, sửa lại rowIndex là index của dtInvoice
            var listInvoiceIdentityCheckPassing = new Dictionary<string, bool>();
            foreach (DataRow row in dtDistintc.Rows)
            {
                if (row[ImportInvoice.InvoiceIndentity] == null || row[ImportInvoice.InvoiceIndentity].IsNullOrEmpty())
                {
                    continue;
                }
                listInvoiceIdentityCheckPassing.Add(row[ImportInvoice.InvoiceIndentity].Trim(), false);
            }

            foreach (DataRow row in dtInvoice.Rows)
            {
                rowIndex++;
                if (row[ImportInvoice.InvoiceIndentity] == null || row[ImportInvoice.InvoiceIndentity].IsNullOrEmpty())
                {
                    continue;
                }
                // sửa rowIndex: dòng if này để duyệt qua mỗi InvoiceIndentity chỉ 1 lần, tương đương duyệt bằng dtDistintc
                if (listInvoiceIdentityCheckPassing[row[ImportInvoice.InvoiceIndentity].Trim()])
                {
                    continue;
                }
                listInvoiceIdentityCheckPassing[row[ImportInvoice.InvoiceIndentity].Trim()] = true;

                listResultImport.RowError.AddRange(ValidateHeaderInvoice(row, rowIndex));
                listResultImport.RowError.AddRange(ExcecuteData(dataUpload, row, rowIndex, companyId, dtInvoice, out invoiceId));
                listInvoiceImportsuccess.Add(invoiceId);
            }

            if (listResultImport.RowError.Count == 0)
            {
                listResultImport.RowSuccess = (rowIndex - 1);
                listResultImport.ErrorCode = ResultCode.NoError;
                listResultImport.Message = MsgApiResponse.ExecuteSeccessful;
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportDataNotSuccess;
                listResultImport.Message = MsgApiResponse.DataInvalid;
            }

            return listResultImport;
        }

        private void ValidClient(DataTable dtInvoice, object invoiceIndentity)
        {
            var dtInvoiceList = dtInvoice.Select("" + ImportInvoice.InvoiceIndentity.ToString() + " = '" + invoiceIndentity.ConvertToString() + "'").ToList();
            var clientInRow = dtInvoiceList[0][ImportInvoice.Client].ToString();
            var companyInRow = dtInvoiceList[0][ImportInvoice.CompanyName].ToString();
            var taxCodeInRow = dtInvoiceList[0][ImportInvoice.ClientTaxCode].ToString();
            var addressInRow = dtInvoiceList[0][ImportInvoice.ClientAddress].ToString();
            foreach (var row in dtInvoiceList)
            {
                if (row[ImportInvoice.Client].ToString() != "" && row[ImportInvoice.Client].ToString() != clientInRow)
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                    //Lỗi client
                }
                if (row[ImportInvoice.CompanyName].ToString() != "" && row[ImportInvoice.CompanyName].ToString() != companyInRow)
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                    //Lỗi client
                }
                if (row[ImportInvoice.ClientTaxCode].ToString() != "" && row[ImportInvoice.ClientTaxCode].ToString() != taxCodeInRow)
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                    //Lỗi client
                }
                if (row[ImportInvoice.ClientAddress].ToString() != "" && row[ImportInvoice.ClientAddress].ToString() != addressInRow)
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
                    //Lỗi client
                }
            }
        }

        private ListError<ImportRowError> ListErrorAdd(CURRENCY currentCurrency, DataRow row, ListError<ImportRowError> listError, int rowIndex)
        {
            if (currentCurrency == null)
            {
                if (row[ImportInvoice.CurrencyName].IsNullOrWhitespace())
                {
                    listError.Add(new ImportRowError(ResultCode.ImportCurrencyNameNotExists, ImportInvoice.CurrencyName, rowIndex));
                }
                if (row[ImportInvoice.CurrencyExchangeRate].IsNullOrWhitespace())
                {
                    listError.Add(new ImportRowError(ResultCode.ImportCurrencyExchangeRateNotExists, ImportInvoice.CurrencyExchangeRate, rowIndex));
                }
            }
            return listError;
        }

        private ListError<ImportRowError> ValidateCurrency(DataTable dtInvoice, DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            var invoiceIndentity = row[ImportInvoice.InvoiceIndentity];
            var dtInvoiceList = dtInvoice.Select("" + ImportInvoice.InvoiceIndentity.ToString() + " = '" + invoiceIndentity.ConvertToString() + "'").ToList();
            // Lấy ra các giá trị của currency của dòng đầu tiên
            var currencyCode = dtInvoiceList[0][ImportInvoice.CurrencyCode].ToString();
            var currencyName = dtInvoiceList[0][ImportInvoice.CurrencyName].ToString();
            var currencyExchangeRate = dtInvoiceList[0][ImportInvoice.CurrencyExchangeRate].ToString();

            var currentCurrency = this.currencyRepository.GetByCode(currencyCode);
            // Nếu là thêm mới currency thì kiểm tra xem Name và ExchangeRate có bị null không
            listError = ListErrorAdd(currentCurrency, row, listError, rowIndex);

            // Lặp kiểm tra nếu cũng một sản phẩm thì các giá trị Code, Name, ExchangeRate phải giống nhau
            foreach (var dtRow in dtInvoiceList)
            {
                if (dtRow[ImportInvoice.CurrencyCode].ToString() != "" && dtRow[ImportInvoice.CurrencyCode].ToString() != currencyCode)
                {
                    listError.Add(new ImportRowError(ResultCode.ImportInvoiceCurrencyCodeNotEqual, ImportInvoice.CurrencyCode, rowIndex, dtRow[ImportInvoice.CurrencyCode].ToString()));
                }
                if (currentCurrency != null)
                {
                    if (dtRow[ImportInvoice.CurrencyName].ToString() != "" && dtRow[ImportInvoice.CurrencyName].ToString() != currencyName)
                    {
                        listError.Add(new ImportRowError(ResultCode.ImportInvoiceCurrencyNameNotEqual, ImportInvoice.CurrencyName, rowIndex, dtRow[ImportInvoice.CurrencyName].ToString()));
                    }
                    if (dtRow[ImportInvoice.CurrencyExchangeRate].ToString() != "" && dtRow[ImportInvoice.CurrencyExchangeRate].ToString() != currencyExchangeRate)
                    {
                        listError.Add(new ImportRowError(ResultCode.ImportInvoiceCurrencyExchangeRateNotEqual, ImportInvoice.CurrencyExchangeRate, rowIndex, dtRow[ImportInvoice.CurrencyExchangeRate].ToString()));
                    }
                }
            }

            return listError;
        }
        private ListError<ImportRowError> ExcecuteData_New(UploadInvoice dataUpload, DataRow row, int rowIndex, DataTable dtInvoice, out long invoiceIdImportSucess)
        {

            var listError = new ListError<ImportRowError>();
            long invoiceId = 0;
            try
            {
                listError.AddRange(InsertInvoice_New(dataUpload, row, rowIndex, out invoiceId));
                invoiceIdImportSucess = invoiceId;
                logger.Error("Import error for invoiceId 1: " + invoiceId, new Exception("Import error for invoiceId 1"));
                if (invoiceId == 0)
                {
                    return listError;
                }
                listError.AddRange(InsertDetailInvoice_New(row, rowIndex, invoiceId));
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error("Import error for invoiceId: " + invoiceId, new Exception(ex.ToString()));
                transaction.Rollback();
                invoiceIdImportSucess = 0;
            }
            return listError;
        }

        private ListError<ImportRowError> ExcecuteData_InvoiceReplace(UploadInvoice dataUpload, DataRow row, int rowIndex, DataTable dtInvoice, out long invoiceIdImportSucess)
        {

            var listError = new ListError<ImportRowError>();
            long invoiceId = 0;
            try
            {
                listError.AddRange(InsertInvoiceReplace_New(dataUpload, row, rowIndex, out invoiceId));
                invoiceIdImportSucess = invoiceId;
                logger.Error("Import error for invoiceId 1: " + invoiceId, new Exception("Import error for invoiceId 1"));
                if (invoiceId == 0)
                {
                    return listError;
                }
                listError.AddRange(InsertDetailInvoiceReplace_New(row, rowIndex, invoiceId));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error("Import error for invoiceId: " + invoiceId, new Exception(ex.ToString()));
                transaction.Rollback();
                invoiceIdImportSucess = 0;
            }
            return listError;
        }

        private ListError<ImportRowError> ExcecuteData_InvoiceAdjustForInvAdjusted(UploadInvoice dataUpload, DataRow row, int rowIndex, DataTable dtInvoice, out long invoiceIdImportSucess)
        {

            var listError = new ListError<ImportRowError>();
            long invoiceId = 0;
            try
            {
                listError.AddRange(InsertInvoiceAdjustForInvAdjusted_New(dataUpload, row, rowIndex, out invoiceId));
                invoiceIdImportSucess = invoiceId;
                logger.Error("Import error for invoiceId 1: " + invoiceId, new Exception("Import error for invoiceId 1"));
                if (invoiceId == 0)
                {
                    return listError;
                }
                listError.AddRange(InsertDetailInvoiceAdjustForInvAdjusted_New(row, rowIndex, invoiceId));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error("Import error for invoiceId: " + invoiceId, new Exception(ex.ToString()));
                transaction.Rollback();
                invoiceIdImportSucess = 0;
            }
            return listError;
        }
        private ListError<ImportRowError> ExcecuteData(UploadInvoice dataUpload, DataRow row, int rowIndex, long companyId, DataTable dtInvoice, out long invoiceIdImportSucess)
        {
            var listError = new ListError<ImportRowError>();
            long invoiceId = 0;
            ValidClient(dtInvoice, row[ImportInvoice.InvoiceIndentity]);
            listError.AddRange(ValidateCurrency(dtInvoice, row, rowIndex));
            InvoiceInfo listAmount = GetTotalAmountInvoice(dtInvoice, row[ImportInvoice.InvoiceIndentity], listError, rowIndex);
            listError.AddRange(InsertInvoice(dataUpload, row, companyId, rowIndex, listAmount, out invoiceId));
            invoiceIdImportSucess = invoiceId;

            if (invoiceId == 0)
            {
                return listError;
            }

            int rowIndexDetail = 0;
            foreach (DataRow dtrow in dtInvoice.Rows)
            {
                rowIndexDetail++;
                if (!dtrow[ImportInvoice.InvoiceIndentity].IsEquals(row[ImportInvoice.InvoiceIndentity]))
                {
                    continue;
                }

                listError.AddRange(InsertDetailInvoice(dtrow, companyId, rowIndexDetail, invoiceId));
            }

            return listError;
        }

        private ListError<ImportRowError> InsertInvoice_New(UploadInvoice dataUpload, DataRow row, int rowIndex, out long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            try
            {
                
                CLIENT client;
                invoiceId = 0;
                listError.AddRange(ExcecuteClientData_New(row, rowIndex, out client));
                if (client == null)
                {
                    logger.Error("No client error", new Exception("No client error"));
                    return listError;
                }

                MYCOMPANY getCompany = this.myCompanyRepository.GetByBranchId(row[ImportInvoiceNew.MSCN].Trim());

                if (getCompany == null)
                {
                    logger.Error("No company error " + row[ImportInvoiceNew.MSCN].Trim(), new Exception("No company error"));
                    return listError;
                }

                DeclarationMaster getSymbol = this.declarationRepository.GetListSymboy(getCompany.COMPANYSID).FirstOrDefault();

                if (getSymbol == null)
                {
                    logger.Error("No symbol : " + getCompany.COMPANYSID, new Exception("No symbol"));
                    return listError;
                }

                CURRENCY cURRENCY = this.currencyRepository.GetByCode(row[ImportInvoiceNew.CurrencyCode].Trim());

                if (cURRENCY == null)
                {
                    logger.Error("No cURRENCY ", new Exception("No cURRENCY"));
                    return listError;
                }
                var date = row[ImportInvoiceNew.ReleasedDate].Trim();

                //var checkInvoice = this.invoiceRepository.GetAll().Where(x=> x.COMPANYID == getCompany.COMPANYSID && 
                //(x.RELEASEDDATE == DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture) || x.RELEASEDDATE < DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture) || x.RELEASEDDATE > DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture))
                //&& (x.INVOICESTATUS == (int)InvoiceStatus.Approved || x.INVOICESTATUS == (int)InvoiceStatus.Released)).ToList();

                //if (checkInvoice.Count() < 0)
                //{

                //}
                //else
                //{
                //    invoiceId = 0;
                //}
                INVOICE invoice = new INVOICE();
                invoice.CLIENTID = client.ID;
                invoice.REGISTERTEMPLATEID = getSymbol.RegisterTemplateId;
                invoice.SYMBOL = getSymbol.symboyFinal;
                invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                invoice.INVOICENO = invoice.NO.ToDecimal(0);
                invoice.COMPANYID = getCompany.COMPANYSID;
                invoice.TOTAL = row[ImportInvoiceNew.Feeamount].ToDecimal();
                invoice.CURRENCYID = cURRENCY.ID;
                invoice.TOTALTAX = row[ImportInvoiceNew.VATAmount].ToDecimal();
                invoice.CURRENCYEXCHANGERATE = row[ImportInvoiceNew.TyGia].ToDecimal();
                invoice.SUM = (invoice.TOTAL.ToDecimal() + invoice.TOTALTAX.ToDecimal());
                invoice.INVOICESTATUS = (int)InvoiceStatus.New;
                invoice.TYPEPAYMENT = 2;
                invoice.REPORT_CLASS = row[ImportInvoiceNew.ReportClass].Trim();
                invoice.VAT_INVOICE_TYPE = row[ImportInvoiceNew.InvoiceType].Trim();

                invoice.RELEASEDDATE = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                invoice.CREATEDDATE = DateTime.Now;
                invoice.CREATEDBY = this.currentUser.Id;//người tạo là người nào
                invoice.DECLAREID = getSymbol.Id;
                invoice.HTHDON = getSymbol.HTHDon;
                invoice.CUSTOMERNAME = client.CUSTOMERNAME;
                invoice.PERSONCONTACT = client.PERSONCONTACT;
                invoice.CUSTOMERTAXCODE = client.TAXCODE;
                invoice.CUSTOMERADDRESS = client.ADDRESS;
                invoice.CUSTOMERBANKACC = client.BANKACCOUNT;

                if (getCompany.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    var parrentCompany = this.myCompanyRepository.GetById((long)getCompany.COMPANYID);
                    invoice.COMPANYNAME = parrentCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = parrentCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = parrentCompany.TAXCODE;
                }
                else
                {
                    //var mycompany = myCompanyRepository.GetById(currentInvoice.COMPANYID);
                    invoice.COMPANYNAME = getCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = getCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = getCompany.TAXCODE;
                }

                invoice.IMPORTTYPE = 3;//Cột nguồn tạo hóa đơn : 3: Upload bằng file

                this.invoiceRepository.Insert(invoice);
                invoiceId = invoice.ID;
            }
            catch (Exception ex)
            {
                logger.Error("InsertInvoice_New error", ex);
                invoiceId = 0;
                throw;
            }
            return listError;
        }
        private ListError<ImportRowError> InsertDetailInvoice_New(DataRow row, int rowIndex, long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
            TAX tax = this.taxRepository.GetTaxBYCODE(row[ImportInvoiceNew.TaxRate].Trim());
            invoiceDetail.TAXID = tax?.ID;
            invoiceDetail.PRODUCTID = null;
            invoiceDetail.PRODUCTNAME = row[ImportInvoiceNew.ProductName].Trim();
            invoiceDetail.TOTAL = row[ImportInvoiceNew.Feeamount].ToDecimal();
            invoiceDetail.AMOUNTTAX = row[ImportInvoiceNew.VATAmount].ToDecimal();
            invoiceDetail.SUM = (invoiceDetail.TOTAL.ToDecimal() + invoiceDetail.AMOUNTTAX.ToDecimal());
            invoiceDetail.INVOICEID = invoiceId;
            this.invoiceDetailRepository.Insert(invoiceDetail);
            return listError;
        }

        private ListError<ImportRowError> InsertInvoiceReplace_New(UploadInvoice dataUpload, DataRow row, int rowIndex, out long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            try
            {

                CLIENT client;
                invoiceId = 0;
                listError.AddRange(ExcecuteClientDataInvRep_New(row, rowIndex, out client));
                if (client == null)
                {
                    logger.Error("No client error", new Exception("No client error"));
                    return listError;
                }

                MYCOMPANY getCompany = this.myCompanyRepository.GetByBranchId(row[ImportInvoiceReplace.MaCN].Trim());

                if (getCompany == null)
                {
                    logger.Error("No company error " + row[ImportInvoiceReplace.MaCN].Trim(), new Exception("No company error"));
                    return listError;
                }

                DeclarationMaster getSymbol = this.declarationRepository.GetListSymboy(getCompany.COMPANYSID).FirstOrDefault();

                if (getSymbol == null)
                {
                    logger.Error("No symbol : " + getCompany.COMPANYSID, new Exception("No symbol"));
                    return listError;
                }

                CURRENCY cURRENCY = this.currencyRepository.GetByCode(row[ImportInvoiceReplace.LoaiTien].Trim());

                if (cURRENCY == null)
                {
                    logger.Error("No cURRENCY ", new Exception("No cURRENCY"));
                    return listError;
                }
                var date = row[ImportInvoiceReplace.NgayHoaDon].Trim();

                var shhddh = row[ImportInvoiceReplace.SoHDDH].Trim();

                var nghddh = row[ImportInvoiceReplace.NgayHDDH].Trim();

                INVOICE getInvoice = this.invoiceRepository.GetInvoiceBySymbolReleaseddateInvoiceno(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture), Convert.ToDecimal(shhddh), getSymbol.Symbol,client.ID);

                //var checkInvoice = this.invoiceRepository.GetAll().Where(x=> x.COMPANYID == getCompany.COMPANYSID && 
                //(x.RELEASEDDATE == DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture) || x.RELEASEDDATE < DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture) || x.RELEASEDDATE > DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture))
                //&& (x.INVOICESTATUS == (int)InvoiceStatus.Approved || x.INVOICESTATUS == (int)InvoiceStatus.Released)).ToList();

                //if (checkInvoice.Count() < 0)
                //{

                //}
                //else
                //{
                //    invoiceId = 0;
                //}
                INVOICE invoice = new INVOICE();
                invoice.CLIENTID = client.ID;
                invoice.REGISTERTEMPLATEID = getSymbol.RegisterTemplateId;
                invoice.SYMBOL = getSymbol.symboyFinal;
                invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                invoice.INVOICENO = invoice.NO.ToDecimal(0);
                invoice.COMPANYID = getCompany.COMPANYSID;
                invoice.TOTAL = row[ImportInvoiceReplace.Feeamount].ToDecimal();
                invoice.CURRENCYID = cURRENCY.ID;
                invoice.TOTALTAX = row[ImportInvoiceReplace.VATAmount].ToDecimal();
                invoice.CURRENCYEXCHANGERATE = row[ImportInvoiceReplace.TyGia].ToDecimal();
                invoice.SUM = (invoice.TOTAL.ToDecimal() + invoice.TOTALTAX.ToDecimal());
                invoice.INVOICESTATUS = (int)InvoiceStatus.New;
                invoice.TYPEPAYMENT = 2;
                invoice.REPORT_CLASS = row[ImportInvoiceReplace.ReportClass].Trim();
                invoice.VAT_INVOICE_TYPE = row[ImportInvoiceReplace.LoaiHoaDon].Trim();

                invoice.RELEASEDDATE = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                invoice.CREATEDDATE = DateTime.Now;
                invoice.CREATEDBY = this.currentUser.Id;//người tạo là người nào
                invoice.DECLAREID = getSymbol.Id;
                invoice.HTHDON = getSymbol.HTHDon;
                invoice.CUSTOMERNAME = client.CUSTOMERNAME;
                invoice.PERSONCONTACT = client.PERSONCONTACT;
                invoice.CUSTOMERTAXCODE = client.TAXCODE;
                invoice.CUSTOMERADDRESS = client.ADDRESS;
                invoice.CUSTOMERBANKACC = client.BANKACCOUNT;
                invoice.CIF_NO = row[ImportInvoiceReplace.CIFKhachHang].Trim();

                if (getCompany.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    var parrentCompany = this.myCompanyRepository.GetById((long)getCompany.COMPANYID);
                    invoice.COMPANYNAME = parrentCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = parrentCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = parrentCompany.TAXCODE;
                }
                else
                {
                    //var mycompany = myCompanyRepository.GetById(currentInvoice.COMPANYID);
                    invoice.COMPANYNAME = getCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = getCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = getCompany.TAXCODE;
                }
                invoice.INVOICETYPE = 1;//Hoá đơn thay thế
                invoice.IMPORTTYPE = 3;//Cột nguồn tạo hóa đơn : 3: Upload bằng file
                invoice.ORIGINNO = getInvoice.NO;
                invoice.PARENTID = getInvoice.ID;
                invoice.PARENTCODE = getInvoice.CODE;
                invoice.PARENTSYMBOL = getInvoice.SYMBOL;

                this.invoiceRepository.Insert(invoice);
                invoiceId = invoice.ID;
            }
            catch (Exception ex)
            {
                logger.Error("InsertInvoice_New error", ex);
                invoiceId = 0;
                throw;
            }
            return listError;
        }


        private ListError<ImportRowError> InsertInvoiceAdjustForInvAdjusted_New(UploadInvoice dataUpload, DataRow row, int rowIndex, out long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            try
            {

                CLIENT client;
                invoiceId = 0;
                listError.AddRange(ExcecuteClientDataInvAdjustForInvAdjusted_New(row, rowIndex, out client));
                if (client == null)
                {
                    logger.Error("No client error", new Exception("No client error"));
                    return listError;
                }

                MYCOMPANY getCompany = this.myCompanyRepository.GetByBranchId(row[ImportInvoiceAdjustForInvAdjusted.MaCN].Trim());

                if (getCompany == null)
                {
                    logger.Error("No company error " + row[ImportInvoiceAdjustForInvAdjusted.MaCN].Trim(), new Exception("No company error"));
                    return listError;
                }

                DeclarationMaster getSymbol = this.declarationRepository.GetListSymboy(getCompany.COMPANYSID).FirstOrDefault();

                if (getSymbol == null)
                {
                    logger.Error("No symbol : " + getCompany.COMPANYSID, new Exception("No symbol"));
                    return listError;
                }

                CURRENCY cURRENCY = this.currencyRepository.GetByCode(row[ImportInvoiceAdjustForInvAdjusted.LoaiTien].Trim());

                if (cURRENCY == null)
                {
                    logger.Error("No cURRENCY ", new Exception("No cURRENCY"));
                    return listError;
                }
                var date = row[ImportInvoiceAdjustForInvAdjusted.NgayHoaDon].Trim();

                var shhddh = row[ImportInvoiceAdjustForInvAdjusted.SoHDBDC].Trim();

                var nghddh = row[ImportInvoiceAdjustForInvAdjusted.NgayHDBDC].Trim();

                INVOICE getInvoice = this.invoiceRepository.GetInvoiceBySymbolReleaseddateInvoiceno(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture), Convert.ToDecimal(shhddh), getSymbol.Symbol, client.ID);

                INVOICE invoice = new INVOICE();
                invoice.CLIENTID = client.ID;
                invoice.REGISTERTEMPLATEID = getSymbol.RegisterTemplateId;
                invoice.SYMBOL = getSymbol.symboyFinal;
                invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                invoice.INVOICENO = invoice.NO.ToDecimal(0);
                invoice.COMPANYID = getCompany.COMPANYSID;
                invoice.TOTAL = row[ImportInvoiceAdjustForInvAdjusted.Feeamount].ToDecimal();
                invoice.CURRENCYID = cURRENCY.ID;
                invoice.TOTALTAX = row[ImportInvoiceAdjustForInvAdjusted.VATAmount].ToDecimal();
                invoice.CURRENCYEXCHANGERATE = row[ImportInvoiceAdjustForInvAdjusted.TyGia].ToDecimal();
                invoice.SUM = (invoice.TOTAL.ToDecimal() + invoice.TOTALTAX.ToDecimal());
                invoice.INVOICESTATUS = (int)InvoiceStatus.New;
                invoice.TYPEPAYMENT = 2;
                invoice.REPORT_CLASS = row[ImportInvoiceAdjustForInvAdjusted.ReportClass].Trim();
                invoice.VAT_INVOICE_TYPE = row[ImportInvoiceAdjustForInvAdjusted.LoaiHoaDon].Trim();

                invoice.RELEASEDDATE = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                invoice.CREATEDDATE = DateTime.Now;
                invoice.CREATEDBY = this.currentUser.Id;//người tạo là người nào
                invoice.DECLAREID = getSymbol.Id;
                invoice.HTHDON = getSymbol.HTHDon;
                invoice.CUSTOMERNAME = client.CUSTOMERNAME;
                invoice.PERSONCONTACT = client.PERSONCONTACT;
                invoice.CUSTOMERTAXCODE = client.TAXCODE;
                invoice.CUSTOMERADDRESS = client.ADDRESS;
                invoice.CUSTOMERBANKACC = client.BANKACCOUNT;
                invoice.CIF_NO = row[ImportInvoiceReplace.CIFKhachHang].Trim();

                if (getCompany.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
                {
                    var parrentCompany = this.myCompanyRepository.GetById((long)getCompany.COMPANYID);
                    invoice.COMPANYNAME = parrentCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = parrentCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = parrentCompany.TAXCODE;
                }
                else
                {
                    //var mycompany = myCompanyRepository.GetById(currentInvoice.COMPANYID);
                    invoice.COMPANYNAME = getCompany.COMPANYNAME;
                    invoice.COMPANYADDRESS = getCompany.ADDRESS;
                    invoice.COMPANYTAXCODE = getCompany.TAXCODE;
                }
                invoice.INVOICETYPE = 2;//Hoá đơn điều chỉnh cho hoá đơn 
                invoice.IMPORTTYPE = 3;//Cột nguồn tạo hóa đơn : 3: Upload bằng file
                invoice.ORIGINNO = getInvoice.NO;
                invoice.PARENTID = getInvoice.ID;
                invoice.PARENTCODE = getInvoice.CODE;
                invoice.PARENTSYMBOL = getInvoice.SYMBOL;

                this.invoiceRepository.Insert(invoice);
                invoiceId = invoice.ID;

                this.UpdateAnnouncementStatus(getInvoice.ID, getInvoice.COMPANYID, 1);

            }
            catch (Exception ex)
            {
                logger.Error("InsertInvoice_New error", ex);
                invoiceId = 0;
                throw;
            }
            return listError;
        }

        private ListError<ImportRowError> InsertDetailInvoiceAdjustForInvAdjusted_New(DataRow row, int rowIndex, long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
            TAX tax = this.taxRepository.GetTax(row[ImportInvoiceAdjustForInvAdjusted.Taxrate].Trim());
            invoiceDetail.TAXID = tax?.ID;
            invoiceDetail.PRODUCTID = null;
            invoiceDetail.PRODUCTNAME = row[ImportInvoiceAdjustForInvAdjusted.Noidunghoadon].Trim();
            invoiceDetail.TOTAL = row[ImportInvoiceAdjustForInvAdjusted.Feeamount].ToDecimal();
            invoiceDetail.AMOUNTTAX = row[ImportInvoiceAdjustForInvAdjusted.VATAmount].ToDecimal();
            invoiceDetail.SUM = (invoiceDetail.TOTAL.ToDecimal() + invoiceDetail.AMOUNTTAX.ToDecimal());
            invoiceDetail.INVOICEID = invoiceId;
            this.invoiceDetailRepository.Insert(invoiceDetail);
            return listError;
        }
        private ListError<ImportRowError> InsertDetailInvoiceReplace_New(DataRow row, int rowIndex, long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
            TAX tax = this.taxRepository.GetTax(row[ImportInvoiceReplace.Taxrate].Trim());
            invoiceDetail.TAXID = tax?.ID;
            invoiceDetail.PRODUCTID = null;
            invoiceDetail.PRODUCTNAME = row[ImportInvoiceReplace.Noidunghoadon].Trim();
            invoiceDetail.TOTAL = row[ImportInvoiceReplace.Feeamount].ToDecimal();
            invoiceDetail.AMOUNTTAX = row[ImportInvoiceReplace.VATAmount].ToDecimal();
            invoiceDetail.SUM = (invoiceDetail.TOTAL.ToDecimal() + invoiceDetail.AMOUNTTAX.ToDecimal());
            invoiceDetail.INVOICEID = invoiceId;
            this.invoiceDetailRepository.Insert(invoiceDetail);
            return listError;
        }
        private ListError<ImportRowError> InsertInvoice(UploadInvoice dataUpload, DataRow row, long companyId, int rowIndex, InvoiceInfo listAmount, out long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            CLIENT client;
            invoiceId = 0;
            listError.AddRange(ExcecuteClientData(row, rowIndex, companyId, out client));
            if (client == null)
            {
                return listError;
            }

            TYPEPAYMENT typePayment = GetTypePayment(row[ImportInvoice.TypePayment].Trim());
            if (typePayment == null)
            {
                listError.Add(new ImportRowError(ResultCode.ImportColumnIsNotExist, ImportInvoice.TypePayment, rowIndex, row[ImportInvoice.TypePayment].ToString()));
            }

            if (listError.Count > 0)
            {
                return listError;
            }

            // Xử lý currency
            var currency = InsertCurrency(row);

            INVOICE invoice = new INVOICE();
            invoice.CLIENTID = client.ID;
            invoice.CUSTOMERNAME = row[ImportInvoice.Client].Trim();
            invoice.REGISTERTEMPLATEID = dataUpload.RegisterTemplateId;
            invoice.SYMBOL = dataUpload.Symbo;
            invoice.NO = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            invoice.INVOICENO = invoice.NO.ToDecimal(0);
            invoice.COMPANYID = companyId;
            invoice.TOTAL = listAmount.Total;
            invoice.TOTALDISCOUNT = listAmount.TotalDiscount;
            invoice.TOTALDISCOUNTTAX = listAmount.TotalDiscountTax;
            invoice.TOTALTAX = listAmount.TotalTax;
            invoice.CURRENCYID = currency.ID;
            invoice.CURRENCYEXCHANGERATE = row[ImportInvoice.CurrencyExchangeRate].ToDecimal();

            if (this.isDiscountTemplate)
            {
                invoice.ISDISCOUNT = 1;
            }
            invoice.SUM = (invoice.TOTAL.ToDecimal() + invoice.TOTALTAX.ToDecimal());
            if (this.isDiscountTemplate)
            {
                invoice.SUM -= invoice.TOTALDISCOUNT;
                if (invoice.SUM < 0)
                {
                    listError.Add(new ImportRowError(ResultCode.ImportSumLessThanZero, ImportInvoice.InvoiceIndentity, rowIndex));
                }
            }

            invoice.INVOICESTATUS = (int)InvoiceStatus.New;
            invoice.CREATEDDATE = row[ImportInvoice.InvoiceDate].ConvertDateTime();
            invoice.NOTE = row[ImportInvoice.Decription].ConvertToString();
            if (typePayment != null)
                invoice.TYPEPAYMENT = typePayment.ID;
            this.invoiceRepository.Insert(invoice);
            invoiceId = invoice.ID;
            return listError;
        }
        private ListError<ImportRowError> InsertDetailInvoice(DataRow row, long companyId, int rowIndex, long invoiceId)
        {
            var listError = new ListError<ImportRowError>();
            listError.AddRange(ValidateDetailInvoice(row, rowIndex));
            PRODUCT product;
            INVOICEDETAIL invoiceDetail = new INVOICEDETAIL();
            invoiceDetail.CopyDataOfDataRow(row, this.isDiscountTemplate);
            TAX tax = this.taxRepository.GetTax(row[ImportInvoice.InvoiceTax].Trim());

            if (invoiceDetail.DISCOUNT != true || this.isDiscountTemplate)
            {
                listError.AddRange(ExcecuteProductData(row, companyId, out product, tax));
                if (product == null)
                {
                    return listError;
                }
                invoiceDetail.PRODUCTID = product.ID;
                if (invoiceId == 0 || listError.Count > 0)
                {
                    return listError;
                }
                invoiceDetail.TOTAL = invoiceDetail.QUANTITY * (invoiceDetail.PRICE ?? 0);

                if (tax != null)
                {
                    invoiceDetail.AMOUNTTAX = (invoiceDetail.TOTAL * (tax.TAX1 ?? 0)) / 100;
                }
                else
                {
                    invoiceDetail.AMOUNTTAX = 0M;
                }

                if (this.isDiscountTemplate && invoiceDetail.DISCOUNTRATIO.HasValue)
                {
                    invoiceDetail.AMOUNTDISCOUNT = invoiceDetail.TOTAL * ((decimal)invoiceDetail.DISCOUNTRATIO / 100M);
                }
            }
            else
            {
                invoiceDetail.PRODUCTID = null;
                invoiceDetail.TOTAL = row[ImportInvoice.InvoiceDetailTotal].ToDecimal();
                invoiceDetail.AMOUNTTAX = row[ImportInvoice.InvoiceDetailAmountTax].ToDecimal();
                invoiceDetail.DISCOUNTDESCRIPTION = row[ImportInvoice.InvoiceDetailProductName].Trim();
            }
            invoiceDetail.INVOICEID = invoiceId;
            invoiceDetail.TAXID = tax?.ID;
            this.invoiceDetailRepository.Insert(invoiceDetail);
            return listError;
        }

        public IEnumerable<InvoiceCheckDaily> FilterInvoiceCheckDaily(ConditionSearchCheckNumberInvoice condition)
        {
            var query = invoiceRepository.FilterInvoiceCheckDaily(condition);
            condition.TotalRecords = query.Count();            
            return query.Skip(condition.Skip).Take(condition.Take).ToList();            
        }
      

        #region ImportClient

        private ListError<ImportRowError> ExcecuteClientData_New(DataRow row, int rowIndex, out CLIENT clientOut)
        {
            var listError = new ListError<ImportRowError>();
            int isOrg = 0;
            string companyName = string.Empty;
            //if (row[ImportInvoice.InvoiceisOrg].ConvertToString() == "x")
            //{
            //    isOrg = 1;
            //    companyName = row[ImportInvoice.CompanyName].Trim().ConvertToString();
            //}
            //else
            //{
            //    isOrg = 0;
            //    companyName = row[ImportInvoice.Client].Trim().ConvertToString();
            //}
            clientOut = FilterClient_New(row[ImportInvoiceNew.CIFKH].Trim(), null, null, isOrg);
            if (clientOut == null)
            {
                //listError.AddRange(InsertOrUpdateClient_New(row, rowIndex, out clientOut));
            }
            return listError;
        }

        private ListError<ImportRowError> ExcecuteClientDataInvRep_New(DataRow row, int rowIndex, out CLIENT clientOut)
        {
            var listError = new ListError<ImportRowError>();
            int isOrg = 0;
            string companyName = string.Empty;
            //if (row[ImportInvoice.InvoiceisOrg].ConvertToString() == "x")
            //{
            //    isOrg = 1;
            //    companyName = row[ImportInvoice.CompanyName].Trim().ConvertToString();
            //}
            //else
            //{
            //    isOrg = 0;
            //    companyName = row[ImportInvoice.Client].Trim().ConvertToString();
            //}
            clientOut = FilterClient_New(row[ImportInvoiceReplace.CIFKhachHang].Trim(), null, null, null);
            if (clientOut == null)
            {
                //listError.AddRange(InsertOrUpdateClient_New(row, rowIndex, out clientOut));
            }
            return listError;
        }

        private ListError<ImportRowError> ExcecuteClientDataInvAdjustForInvAdjusted_New(DataRow row, int rowIndex, out CLIENT clientOut)
        {
            var listError = new ListError<ImportRowError>();
            int isOrg = 0;
            string companyName = string.Empty;
            //if (row[ImportInvoice.InvoiceisOrg].ConvertToString() == "x")
            //{
            //    isOrg = 1;
            //    companyName = row[ImportInvoice.CompanyName].Trim().ConvertToString();
            //}
            //else
            //{
            //    isOrg = 0;
            //    companyName = row[ImportInvoice.Client].Trim().ConvertToString();
            //}
            clientOut = FilterClient_New(row[ImportInvoiceAdjustForInvAdjusted.CIFKhachHang].Trim(), null, null, null);
            if (clientOut == null)
            {
                //listError.AddRange(InsertOrUpdateClient_New(row, rowIndex, out clientOut));
            }
            return listError;
        }

        private ListError<ImportRowError> ExcecuteClientData(DataRow row, int rowIndex, long companyId, out CLIENT clientOut)
        {
            var listError = new ListError<ImportRowError>();
            int isOrg = 0;
            string companyName = string.Empty;
            if (row[ImportInvoice.InvoiceisOrg].ConvertToString() == "x")
            {
                isOrg = 1;
                companyName = row[ImportInvoice.CompanyName].Trim().ConvertToString();
            }
            else
            {
                isOrg = 0;
                companyName = row[ImportInvoice.Client].Trim().ConvertToString();
            }
            clientOut = FilterClient(companyId, row[ImportInvoice.ClientTaxCode].Trim(), companyName, row[ImportInvoice.ClientAddress].Trim(), isOrg);
            if (clientOut == null)
            {
                listError.AddRange(InsertOrUpdateClient(row, rowIndex, out clientOut));
            }
            return listError;

        }

        private CURRENCY InsertCurrency(DataRow row)
        {
            var currencyCode = row[ImportInvoice.CurrencyCode].ToString();
            var currency = this.currencyRepository.GetByCode(currencyCode);

            if (currency == null)
            {
                var currencyName = row[ImportInvoice.CurrencyName].ToString();
                var currencyExchangeRate = row[ImportInvoice.CurrencyExchangeRate].ToString();
                currency = new CURRENCY()
                {
                    CODE = currencyCode,
                    NAME = currencyName,
                    EXCHANGERATE = currencyExchangeRate.ToDecimal(),
                };
                this.currencyRepository.Insert(currency);
            }

            return currency;
        }
        private void ErrorTax(List<DataRow> dtInvoiceList, string taxInRow)
        {
            foreach (var row in dtInvoiceList)
            {
                if (!this.isMultiTaxTemplate)
                {
                    if (row[ImportInvoice.InvoiceTax].ToString() != taxInRow && row[ImportInvoice.InvoiceIsDiscount].Trim() == "")
                    {
                        throw new BusinessLogicException(ResultCode.ImportInvoiceTaxNotIsTheSame, MsgApiResponse.DataInvalid);
                        //Lỗi thuế suất
                    }
                }
                else
                {
                    if (row[ImportInvoice.InvoiceTax].ToString() == "" && row[ImportInvoice.InvoiceIsDiscount].Trim() == "")
                    {
                        throw new BusinessLogicException(ResultCode.ImportInvoiceTaxEmpty, MsgApiResponse.DataInvalid);
                        //Lỗi thuế suất
                    }
                }

            }
        }

        private InvoiceInfo GetTotalAmountInvoice(DataTable dtInvoice, object invoiceIndentity, ListError<ImportRowError> listError, int rowIndex)
        {
            InvoiceInfo listAmount = new InvoiceInfo();
            listAmount.Total = 0; listAmount.TotalDiscount = 0; listAmount.TotalDiscountTax = 0; listAmount.TotalTax = 0;

            var dtInvoiceList = dtInvoice.Select("" + ImportInvoice.InvoiceIndentity.ToString() + " = '" + invoiceIndentity.ConvertToString() + "'").ToList();

            var taxInRow = dtInvoiceList[0][ImportInvoice.InvoiceTax].ToString();
            ErrorTax(dtInvoiceList, taxInRow);

            TAX tax = new TAX();
            foreach (DataRow dtrow in dtInvoiceList)
            {
                if (dtrow[ImportInvoice.InvoiceNotMoney].Trim().ToLower() != "x")
                {
                    this.GetTotalAmountInvoiceItem(dtrow, listAmount, listError, rowIndex);
                }
            }

            if (listAmount.TotalDiscount > listAmount.Total)
            {
                listError.Add(new ImportRowError(ResultCode.ImportDiscountGreaterTotal, ImportInvoice.InvoiceIndentity, rowIndex));
            }

            if (this.isDiscountTemplate || !this.isMultiTaxTemplate)
            {
                var totalTax = (listAmount.Total - listAmount.TotalDiscount) * (tax?.TAX1 ?? 0) / 100;
                if (totalTax >= 0)
                {
                    listAmount.TotalTax = totalTax;
                }
            }

            return listAmount;
        }

        private void GetTotalAmountInvoiceItem(DataRow dtrow, InvoiceInfo listAmount, ListError<ImportRowError> listError, int rowIndex)
        {
            listAmount.Total += dtrow[ImportInvoice.InvoiceDetailQuantity].ToInt().Value * dtrow[ImportInvoice.InvoiceDetailPrice].ToDecimal().Value;

            // Nếu là chiết khấu theo từng mặt hàng thì tính tiền chiết khấu ở dòng nào có CK%
            if (this.isDiscountTemplate)
            {
                var discount = dtrow[ImportInvoice.DiscountPercent].Trim().ToLower();
                if (discount != "")
                {
                    var discountPercentText = discount.TrimEnd('%');
                    int discountPercent = 0;
                    if (!int.TryParse(discountPercentText, out discountPercent))
                    {
                        listError.Add(new ImportRowError(ResultCode.ImportDiscountPercentNotValid, ImportInvoice.DiscountPercent, rowIndex));
                    }
                    else
                    {
                        listAmount.TotalDiscount += (dtrow[ImportInvoice.InvoiceDetailTotal].ToDecimal() ?? 0M) * ((decimal)discountPercent / 100);
                    }
                }
            }
            else if (dtrow[ImportInvoice.InvoiceIsDiscount].Trim().ToLower() == "x")
            {
                listAmount.TotalDiscount += dtrow[ImportInvoice.InvoiceDetailTotal].ToDecimal() ?? 0M;
                listAmount.TotalDiscountTax += dtrow[ImportInvoice.InvoiceDetailAmountTax].ToDecimal() ?? 0M;
            }
        }

        private CLIENT FilterClient_New(string taxCode, string customerName, string address, int? isOrg)
        {
            return this.clientRepository.FilterClient(taxCode, customerName);
        }

        private CLIENT FilterClient(long companyId, string taxCode, string customerName, string address, int? isOrg)
        {
            return this.clientRepository.FilterClient(companyId, taxCode, customerName, address, isOrg);
        }

        private ListError<ImportRowError> InsertOrUpdateClient_New(DataRow row, int rowIndex, out CLIENT clientOut)
        {
            clientOut = null;
            var listError = new ListError<ImportRowError>();
            //var isOrg = row[ImportInvoice.InvoiceisOrg].ToString().Trim() == "x";
            //if (isOrg)
            //{
            //    bool isExitedTax = this.clientRepository.ContainTax(row[ImportInvoice.ClientTaxCode].ToString());
            //    if (isExitedTax)
            //    {
            //        listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportInvoice.ClientTaxCode, rowIndex, row[ImportInvoice.ClientTaxCode].ToString()));
            //    }
            //}

            if (listError.Count > 0)
            {
                return listError;
            }

            CLIENT client = ProcessInsertOrUpdateClient(row);

            this.account.CreateAccountClient(client);
            clientOut = client;
            return listError;
        }
        private ListError<ImportRowError> InsertOrUpdateClient(DataRow row, int rowIndex, out CLIENT clientOut)
        {
            clientOut = null;
            var listError = new ListError<ImportRowError>();
            var isOrg = row[ImportInvoice.InvoiceisOrg].ToString().Trim() == "x";
            if (isOrg)
            {
                bool isExitedTax = this.clientRepository.ContainTax(row[ImportInvoice.ClientTaxCode].ToString());
                if (isExitedTax)
                {
                    listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportInvoice.ClientTaxCode, rowIndex, row[ImportInvoice.ClientTaxCode].ToString()));
                }
            }

            if (listError.Count > 0)
            {
                return listError;
            }

            CLIENT client = ProcessInsertOrUpdateClient(row);

            this.account.CreateAccountClient(client);
            clientOut = client;
            return listError;
        }

        private CLIENT ProcessInsertOrUpdateClient(DataRow row)
        {
            CLIENT client = null;
            var isClientExistsInDB = false;
            var taxCode = row[ImportInvoice.ClientTaxCode].ToString().Trim();
            var personContact = row[ImportInvoice.Client].ToString().Trim();
            var address = row[ImportInvoice.ClientAddress].ToString().Trim();
            var isOrg = row[ImportInvoice.InvoiceisOrg].ToString().Trim() == "x";
            var customerName = row[ImportInvoice.CompanyName].ToString().Trim();
            var mobile = row[ImportInvoice.Phone].ToString().Trim();
            var myEmail = row[ImportInvoice.ClientEmail].ToString().Trim();
            var bankAccount = row[ImportInvoice.ClientAccount].ToString().Trim();
            var bankName = row[ImportInvoice.ClientBankName].ToString().Trim();
            // Là Tổ chức: check TaxCode 
            if (isOrg)
            {
                client = this.clientRepository.GetByTaxCode(taxCode);
                isClientExistsInDB = client != null;
            }
            // Là Cá nhân: check Tên và Địa chỉ
            if (!isOrg)
            {
                client = this.clientRepository.GetByPersonalContactAndAddress(personContact, address);
                isClientExistsInDB = client != null;
            }

            if (!isClientExistsInDB) // Create
            {
                client = new CLIENT();
                client.CopyDataOfDataRow(row);
                client.TYPESENDINVOICE = (int)TypeSendInvoice.Email;
                client.CREATEDDATE = DateTime.Now;
                client.CREATEDBY = this.currentUser.Id;

                this.clientRepository.Insert(client);

                // Trong trường hợp customerCode được insert bằng màn hình Invoice hoặc import invoice
                // customerCode sẽ được gán bằng "_1" nếu đã tồn tại customerCode với giá trị đó
                var customerCode = client.ID.ToString();
                var maxCustomerCode = this.clientRepository.GetMaxCustomerCode(client.ID);
                if (maxCustomerCode != 0)
                {
                    customerCode += $"_{maxCustomerCode.ToString()}";
                }

                client.CUSTOMERCODE = customerCode;
                this.clientRepository.Update(client);
            }
            else // Update
            {
                client.UPDATEDDATE = DateTime.Now;
                client.UPDATEDBY = this.currentUser.Id;
                client.CUSTOMERNAME = customerName;
                client.PERSONCONTACT = personContact;
                client.ADDRESS = address;
                client.MOBILE = mobile;
                client.EMAIL = myEmail;
                client.RECEIVEDINVOICEEMAIL = myEmail;
                client.BANKACCOUNT = bankAccount;
                client.BANKNAME = bankName;
                client.TAXCODE = taxCode;
                this.clientRepository.Update(client);
            }

            return client;
        }
        #endregion

        #region Import Product
        private ListError<ImportRowError> ExcecuteProductData(DataRow row, long companyId, out PRODUCT productOut, TAX tax)
        {
            var listError = new ListError<ImportRowError>();
            productOut = FilterProduct(companyId, row[ImportInvoice.InvoiceDetailProductName].Trim(), row[ImportInvoice.InvoiceDetailUnit].Trim());
            if (productOut == null)
            {
                listError.AddRange(InsertProduct(row, companyId, out productOut, tax));
            }

            return listError;
        }

        private PRODUCT FilterProduct(long companyId, string name, string unitName)
        {
            return this.productRepository.FilterProduct(companyId, name, unitName);
        }

        private ListError<ImportRowError> InsertProduct(DataRow row, long companyId, out PRODUCT productOut, TAX tax)
        {
            productOut = null;
            UNITLIST unitList = FilterUnit(row[ImportInvoice.InvoiceDetailUnitCode].Trim(), row[ImportInvoice.InvoiceDetailUnit].Trim(), companyId);
            if (unitList == null)
            {
                UnitListViewModel unitInfo = new UnitListViewModel();
                unitInfo.Code = row[ImportInvoice.InvoiceDetailUnitCode].Trim().ToString().Trim();
                unitInfo.Name = row[ImportInvoice.InvoiceDetailUnit].Trim().ToString().Trim();
                unitInfo.CompanyId = companyId;
                unitList = InsertUnit(unitInfo);
            }

            PRODUCT product = this.productRepository.FilterProductNameUnitCode(row[ImportInvoice.InvoiceDetailProductName].Trim(), row[ImportInvoice.InvoiceDetailUnitCode].Trim());
            if (product == null)
            {
                product = new PRODUCT();
                product.CopyDataOfDataRow(row);
                product.UNITID = unitList.ID;
                product.UNIT = unitList.NAME;
                product.TAXID = tax?.ID;
                product.CREATEDDATE = DateTime.Now;
                product.CREATEDBY = this.currentUser.Id;
                this.productRepository.Insert(product);
            }
            productOut = product;
            productOut.PRODUCTCODE = product.ID.ToString();
            return new ListError<ImportRowError>();
        }
        private UNITLIST FilterUnit(string code, string name, long companyId)
        {
            return this.unitListRepository.GetByNameOrCode(code, name, companyId).FirstOrDefault();
        }
        #endregion

        #region Validation
        private ListError<ImportRowError> ValidateDetailInvoice(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDetailProductName, rowIndex));

            if (row[ImportInvoice.InvoiceIsDiscount].IsNullOrWhitespace())
            {
                listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDetailUnit, rowIndex));
                listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDetailQuantity, rowIndex));
                listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDetailPrice, rowIndex));
            }

            listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDetailTotal, rowIndex));


            listError.Add(ValidDataMaxLength(row, ImportProduct.MaxLength50, ImportProduct.Unit, rowIndex));
            return listError;
        }
        private ListError<ImportRowError> ValidateHeaderInvoiceNew(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.MSCN, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.ReportClass, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.InvoiceType, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.CIFKH, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.ReleasedDate, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.CurrencyCode, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.TyGia, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.Feeamount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.TaxRate, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.VATAmount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceNew.ProductName, rowIndex));

            return listError;
        }

        private ListError<ImportRowError> ValidateHeaderInvoiceReplace(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.MaCN, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.ReportClass, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.LoaiHoaDon, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.CIFKhachHang, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.NgayHoaDon, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.LoaiTien, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.TyGia, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.Feeamount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.Taxrate, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.VATAmount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceReplace.Noidunghoadon, rowIndex));

            return listError;
        }

        private ListError<ImportRowError> ValidateHeaderInvoiceAdjustForInvAdjusted(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.MaCN, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.ReportClass, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.LoaiHoaDon, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.CIFKhachHang, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.NgayHoaDon, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.LoaiTien, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.TyGia, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.Feeamount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.Taxrate, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.VATAmount, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustForInvAdjusted.Noidunghoadon, rowIndex));

            return listError;
        }

        private ListError<ImportRowError> ValidateHeaderInvoice(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceDate, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoice.ClientEmail, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoice.ClientAddress, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoice.CurrencyCode, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoice.CurrencyExchangeRate, rowIndex));
            if (row[ImportInvoice.InvoiceisOrg].ConvertToString().Trim().ToLower() == "x")
            {
                listError.Add(ValidDataIsNull(row, ImportInvoice.CompanyName, rowIndex));
                listError.Add(ValidDataIsNull(row, ImportInvoice.ClientTaxCode, rowIndex));
            }
            else
            {
                listError.Add(ValidDataIsNull(row, ImportInvoice.Client, rowIndex));
            }
            if (!this.isMultiTaxTemplate)
            {
                listError.Add(ValidDataIsNull(row, ImportInvoice.InvoiceTax, rowIndex));
            }

            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportInvoice.ClientTaxCode, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportInvoice.ClientEmail, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength50, ImportInvoice.Client, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportClient.MaxLength25, ImportInvoice.ClientAccount, rowIndex));

            // valid is numberic
            listError.Add(ValidNumber(row, ImportInvoice.CurrencyExchangeRate, rowIndex));

            return listError;
        }
        private ImportRowError ValidDataIsNull(DataRow row, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (!row[colunName].IsNullOrEmpty())
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataIsEmpty, colunName, rowIndex);
            }
            catch (Exception)
            {
                return errorColumn;
            }
        }

        private ImportRowError ValidDataMaxLength(DataRow row, int maxLength, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (row[colunName].IsNullOrEmpty() || row[colunName].ToString().Length < maxLength)
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataExceedMaxLength, colunName, rowIndex);
            }
            catch (Exception)
            {

                return errorColumn;
            }
        }

        private ImportRowError ValidNumber(DataRow row, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (row[colunName].IsNullOrEmpty() && row[colunName].ToString().IsNumeric())
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataIsNotNumberic, colunName, rowIndex);
            }
            catch (Exception)
            {

                return errorColumn;
            }
        }
        #endregion

        #region Process Client
        private CLIENT ProcessClient(InvoiceInfo info)
        {
            CLIENT client = null;
            var isClientExistsInDB = false;
            // Chọn từ màn hình gợi ý
            if (info.ClientId.HasValue)
            {
                client = GetClient(info.ClientId.Value);
                isClientExistsInDB = true;
            }
            // Nếu là nhập thủ công thì:
            // Là Tổ chức: check TaxCode 
            if (info.IsOrg && !isClientExistsInDB)
            {
                client = this.clientRepository.GetByTaxCode(info.TaxCode);
                isClientExistsInDB = client != null;
            }
            // Là Cá nhân: check Tên và Địa chỉ
            if (!info.IsOrg && !isClientExistsInDB)
            {
                client = this.clientRepository.GetByPersonalContactAndAddress(info.PersonContact, info.Address);
                isClientExistsInDB = client != null;
            }
            // Là khách hàng vãng lai:
            if (OtherExtensions.IsCurrentClient(info.CustomerCode) && !isClientExistsInDB)
            {
                client = this.clientRepository.GetByCustomerCode(info.CustomerCode);
                isClientExistsInDB = client != null;
            }

            if (!isClientExistsInDB)
            {
                client = InsertClient(info);

                // Trong trường hợp customerCode được insert bằng màn hình Invoice hoặc import invoice
                // customerCode sẽ được gán bằng "_1" nếu đã tồn tại customerCode với giá trị đó
                var customerCode = client.ID.ToString();
                var maxCustomerCode = this.clientRepository.GetMaxCustomerCode(client.ID);
                if (maxCustomerCode != 0)
                {
                    customerCode += $"_{maxCustomerCode.ToString()}";
                }

                client.CUSTOMERCODE = customerCode;
                this.clientRepository.Update(client);
                this.account.CreateAccountClient(client);
            }
            else
            {
                info.ClientId = client.ID;
            }

            return client;
        }
        private CLIENT InsertClient(InvoiceInfo info)
        {
            var clientNew = new CLIENT();
            clientNew.CopyData(info);
            clientNew.CREATEDDATE = DateTime.Now;
            clientNew.CREATEDBY = info.UserAction;
            clientNew.CUSTOMERTYPE = (int)CustomerType.NoneAccounting;
            clientNew.TYPESENDINVOICE = (int)TypeSendInvoice.Email;
            clientNew.BANKACCOUNT = info.BankAccount;
            clientNew.BANKNAME = info.BankName;
            this.clientRepository.Insert(clientNew);
            return clientNew;
        }
        private CLIENT GetClient(long id)
        {
            var currentClient = this.clientRepository.GetById(id);
            if (currentClient == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This clientId [{0}] not found in data of client", id));
            }

            return currentClient;
        }
        private UNITLIST InsertUnit(UnitListViewModel unitListInfo)
        {
            var unit = new UNITLIST();
            unit.CODE = unitListInfo.Code;
            unit.NAME = unitListInfo.Name;
            this.unitListRepository.Insert(unit);
            return unit;
        }
        #endregion

        #region Process Product
        private PRODUCT ProcessProduct(InvoiceInfo invoiceInfo, InvoiceDetailInfo invoiceDetail)
        {
            PRODUCT product = new PRODUCT();
            if (invoiceDetail.ProductId.HasValue)
            {
                product = GetProduct(invoiceDetail.ProductId.Value, invoiceInfo.CompanyId.Value);
            }

            return product;
        }

        private PRODUCT GetProduct(long id, long companyId)
        {
            var currentProduct = this.productRepository.GetById(id, companyId);
            if (currentProduct == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This Product [{0}] not found in data of client", id));
            }

            return currentProduct;
        }

        private TYPEPAYMENT GetTypePayment(string codePayment)
        {
            return this.typePaymentRepository.GetPaymentByCode(codePayment);
        }
        #endregion
    }
}
