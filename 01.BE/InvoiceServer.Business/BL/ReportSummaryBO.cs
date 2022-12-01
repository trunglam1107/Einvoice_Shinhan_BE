using InvoiceServer.Business.DAO;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class ReportSummaryBO : IReportSummaryBO
    {

        private readonly IInvoiceRepository invoiceRepository;
        private readonly ITaxRepository taxRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly PrintConfig config;
        private readonly ISystemSettingRepository systemSettingRepository;
        #region Contructor

        public ReportSummaryBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.config = config;
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
        }

        #endregion
        public IEnumerable<ReportCustomer> ViewReportCustomers(ConditionReportSummary condition, CompanyInfo company)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.myCompanyRepository.GetReportReportCompany(condition);
        }

        public ExportFileInfo ExportReportCustomers(ConditionReportSummary condition, CompanyInfo company)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportCustomerExport dataRport = new ReportCustomerExport(condition);
            var reportCustomerData = this.myCompanyRepository.GetReportReportCompany(condition);
            dataRport.Items = reportCustomerData.ToList();
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            ReportCustomerView export = new ReportCustomerView(dataRport, config, systemSettingInfo);
            return export.ExportFile();
        }

        public ExportFileInfo PrintViewMonthlyInvoiceBoard(ConditionInvoiceBoard condition, CompanyInfo companyInfo)
        {
            var CurrentCompany = this.myCompanyRepository.GetById(condition.Branch ?? 0);
            var company = companyInfo.DeepCopy();
            company.CompanyName = CurrentCompany.COMPANYNAME;
            company.TaxCode = CurrentCompany.TAXCODE;
            ReporMonthlyBoard report = new ReporMonthlyBoard(company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId);
            report.items = GetMothlyInvoiceBoardExport(condition);
            report.Month = condition.Month;
            report.Year = condition.DateFrom.Year;
            ReportMothlyBoardView export = new ReportMothlyBoardView(report, systemSettingInfo, config, condition.IsMonth > 0, condition.Language);
            return export.ExportFile();
        }

        public ReporMonthlyBoard PrintViewMonthlyInvoiceBoardView(ConditionInvoiceBoard condition, CompanyInfo companyInfo, int skip = 0, int take = 0)
        {
            var CurrentCompany = this.myCompanyRepository.GetById(condition.Branch ?? 0);
            var company = companyInfo.DeepCopy();
            company.CompanyName = CurrentCompany.COMPANYNAME;
            company.TaxCode = CurrentCompany.TAXCODE;
            ReporMonthlyBoard report = new ReporMonthlyBoard(company);
            report.items = GetMothlyInvoiceBoard(condition, skip, take);
            report.Month = condition.Month;
            report.Year = condition.DateFrom.Year;
            return report;
        }
        private List<ReportListInvoices> GetMothlyInvoiceBoardExport(ConditionInvoiceBoard codition)
        {
            var totalGot = 0;
            var addHeaderRow = false;
            var numberInvoiceTaken = 0;
            List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Released };
            List<ReportListInvoices> listInvoice = GetListInvoiceReport(codition, invoiceStatus);
            var taxs = this.taxRepository.GetList()
                                .OrderBy(p => p.ORDERREPORT)
                                .Select(p => { p.DESCRIPTION = $"{p.ORDERREPORT}. {p.DESCRIPTION}"; return p; })
                                .ToList();
            List<ReportListInvoices> invoiceMonthlyBoard = new List<ReportListInvoices>();

            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(codition.CompanyId);
            listInvoice.ForEach(i =>
            {
                FormatBySystemSetting(i, systemSettingInfo);
            });
            var tuple = Tuple.Create(totalGot, addHeaderRow, numberInvoiceTaken, systemSettingInfo);
            foreach (var tax in taxs)
            {
                if (!this.GetMothlyInvoiceBoardItemExport(tax, codition, listInvoice, invoiceMonthlyBoard, tuple))
                {
                    continue;
                }
            }

            return invoiceMonthlyBoard;
        }

        private List<ReportListInvoices> GetMothlyInvoiceBoard(ConditionInvoiceBoard codition, int skip = 0, int take = 0)
        {
            var maxValue = skip + take;
            var totalGot = 0;
            var addHeaderRow = false;
            var numberInvoiceTaken = 0;
            var indexGet = skip;
            List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Released };
            var listInvoice = GetListInvoiceReport(codition, invoiceStatus);
            var taxs = this.taxRepository.GetList()
                                .OrderBy(p => p.ORDERREPORT)
                                .Select(p => { p.DESCRIPTION = $"{p.ORDERREPORT}. {p.DESCRIPTION}"; return p; })
                                .ToList();
            List<ReportListInvoices> invoiceMonthlyBoard = new List<ReportListInvoices>();

            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(codition.CompanyId);
            listInvoice.ForEach(i =>
            {
                FormatBySystemSetting(i, systemSettingInfo);
            });
            var tuple = Tuple.Create(maxValue, totalGot, addHeaderRow, numberInvoiceTaken, indexGet, skip, take, systemSettingInfo);
            foreach (var tax in taxs)
            {
                if (totalGot < take)
                {
                    if (!this.GetMothlyInvoiceBoardItem(tax, codition, listInvoice, invoiceMonthlyBoard, tuple))
                    {
                        continue;
                    }
                }
            }

            return invoiceMonthlyBoard;
        }

        private List<ReportListInvoices> GetListInvoiceReport(ConditionInvoiceBoard codition, List<int> invoiceStatus)
        {
            return this.invoiceRepository.FilterInvoiceGroupByTaxId(codition, invoiceStatus)
                                .Select(x =>
                                {
                                    var exChangeRate = x.CurrencyCode == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                                    //if (x.AdjustmentType == (int)AdjustmentType.Down)
                                    //{
                                    //    x.Total = (x.Total ?? 0);
                                    //    x.TotalTax = (x.TotalTax ?? 0);
                                    //}
                                    x.InvoiceNote = x.RefNumber;  // Task #29631: [Bank][Bảng kê tháng/quý] Hiển thị nội dung Mã giao dịch trong cột Ghi chú
                                    x.Total *= exChangeRate;
                                    x.TotalTax *= exChangeRate;

                                    return x;
                                })
                                .ToList();
        }

        private bool GetMothlyInvoiceBoardItemExport(TAX tax
            , ConditionInvoiceBoard codition
            , List<ReportListInvoices> listInvoice
            , List<ReportListInvoices> invoiceMonthlyBoard
            , Tuple<int, bool, int, SystemSettingInfo> tuple)
        {
            SystemSettingInfo systemSettingInfo = tuple.Item4;
            codition.TaxID = tax.ID;
            var listInvoiceByTax = listInvoice.Where(p => (p.TaxId == tax.ID));
            var numberInvoiceCurrentTax = listInvoiceByTax.Count();
            if (numberInvoiceCurrentTax == 0)
            {
                var rowTaxDescription = new ReportListInvoices(tax.DESCRIPTION);
                var rowTaxCode = new ReportListInvoices(0, 0, tax.CODE);
                FormatBySystemSetting(rowTaxDescription, systemSettingInfo);
                FormatBySystemSetting(rowTaxCode, systemSettingInfo);
                invoiceMonthlyBoard.Add(rowTaxDescription);
                invoiceMonthlyBoard.Add(rowTaxCode);
                return false;
            }

            var rowTaxDescription2 = new ReportListInvoices(tax.DESCRIPTION);
            FormatBySystemSetting(rowTaxDescription2, systemSettingInfo);
            invoiceMonthlyBoard.Add(rowTaxDescription2);
            var listAdd = listInvoiceByTax.OrderBy(p => p.InvoiceNo2).ToList();
            invoiceMonthlyBoard.AddRange(listAdd);
            // truong hop lay toi record > so record cua tax hien tai thi moi add row total cua tax hien tai

            decimal totalAmountByTax = listInvoiceByTax.Sum(p => (p.Total ?? 0));
            decimal totalAmountTaxtByTax = listInvoiceByTax.Sum(p => (p.TotalTax ?? 0));
            var rowTotalByTax = new ReportListInvoices(totalAmountByTax, totalAmountTaxtByTax, tax.CODE);
            FormatBySystemSetting(rowTotalByTax, systemSettingInfo);
            invoiceMonthlyBoard.Add(rowTotalByTax);
            return true;
        }
        private bool GetMothlyInvoiceBoardItem(TAX tax
            , ConditionInvoiceBoard codition
            , List<ReportListInvoices> listInvoice
            , List<ReportListInvoices> invoiceMonthlyBoard 
            , Tuple<int, int, bool, int, int, int, int, Tuple<SystemSettingInfo>> tuple)
        {
            var maxValue = tuple.Item1;
            var totalGot = tuple.Item2;
            var addHeaderRow = tuple.Item3;
            var numberInvoiceTaken = tuple.Item4;
            var indexGet = tuple.Item5;
            var skip = tuple.Item6;
            var take = tuple.Item7;
            SystemSettingInfo systemSettingInfo = tuple.Rest.Item1;
            if (indexGet <= 0)
            {
                addHeaderRow = true;
            }
            codition.TaxID = tax.ID;
            var listInvoiceByTax = listInvoice.OrderBy(x=> x.InvoiceNo2).Where(p => (p.TaxId == tax.ID));
            var numberInvoiceCurrentTax = listInvoiceByTax.Count();
            numberInvoiceTaken += numberInvoiceCurrentTax + 2;
            if (skip > numberInvoiceTaken)
            {
                indexGet -= numberInvoiceCurrentTax - 2;
                return false;
            }
            if (numberInvoiceCurrentTax == 0)
            {
                var rowTaxDescription = new ReportListInvoices(tax.DESCRIPTION);
                var rowTaxCode = new ReportListInvoices(0, 0, tax.CODE);
                FormatBySystemSetting(rowTaxDescription, systemSettingInfo);
                FormatBySystemSetting(rowTaxCode, systemSettingInfo);
                invoiceMonthlyBoard.Add(rowTaxDescription);
                invoiceMonthlyBoard.Add(rowTaxCode);
                totalGot += 2;
                numberInvoiceTaken += 2;
                indexGet -= 2;
                return false;
            }
            if (skip == 0 || addHeaderRow)
            {
                var rowTaxDescription2 = new ReportListInvoices(tax.DESCRIPTION);
                FormatBySystemSetting(rowTaxDescription2, systemSettingInfo);
                invoiceMonthlyBoard.Add(rowTaxDescription2);
                totalGot += 1;
            }
            indexGet -= 1;
            if (indexGet < 0)
            {
                indexGet = 0;
            }
            var listAdd = listInvoiceByTax.OrderBy(p => p.InvoiceNo2).Skip(indexGet).Take(take - totalGot).ToList();
            invoiceMonthlyBoard.AddRange(listAdd);
            totalGot += listAdd.Count;
            // truong hop lay toi record > so record cua tax hien tai thi moi add row total cua tax hien tai
            if (maxValue > numberInvoiceTaken && totalGot <= take)
            {
                decimal totalAmountByTax = listInvoiceByTax.Sum(p => (p.Total ?? 0));
                decimal totalAmountTaxtByTax = listInvoiceByTax.Sum(p => (p.TotalTax ?? 0));
                var rowTotalByTax = new ReportListInvoices(totalAmountByTax, totalAmountTaxtByTax, tax.CODE);
                FormatBySystemSetting(rowTotalByTax, systemSettingInfo);
                invoiceMonthlyBoard.Add(rowTotalByTax);
                totalGot += 1;
            }
            indexGet = skip - numberInvoiceTaken;
            return true;
        }

        private void FormatBySystemSetting(ReportListInvoices reportListInvoices, SystemSettingInfo systemSettingInfo)
        {
            StandardSystemSettingIfVNDBTH(systemSettingInfo);
            reportListInvoices.TotalByTax = FormatNumber.SetFractionDigit(reportListInvoices.TotalByTax, systemSettingInfo.ConvertedAmount);
            reportListInvoices.TotalTaxByTax = FormatNumber.SetFractionDigit(reportListInvoices.TotalTaxByTax, systemSettingInfo.ConvertedAmount);
            reportListInvoices.Total = FormatNumber.SetFractionDigit(reportListInvoices.Total, systemSettingInfo.Amount);
            reportListInvoices.TotalTax = FormatNumber.SetFractionDigit(reportListInvoices.TotalTax, systemSettingInfo.Amount);
        }

        private void StandardSystemSettingIfVNDBTH(SystemSettingInfo systemSetting)
        {
            systemSetting.Amount = 0;
            systemSetting.ConvertedAmount = 0;
        }

        public ExportFileInfo PrintViewMonthlyInvoiceBoardXML(ConditionInvoiceBoard condition, CompanyInfo companyInfo)
        {
            var CurrentCompany = this.myCompanyRepository.GetById(condition.Branch ?? 0);
            var company = companyInfo.DeepCopy();
            company.CompanyName = CurrentCompany.COMPANYNAME;
            company.TaxCode = CurrentCompany.TAXCODE;
            ReporMonthlyBoard report = new ReporMonthlyBoard(company);
            report.items = GetMothlyInvoiceBoardExport(condition);
            report.Month = condition.DateFrom.Month;
            report.Year = condition.DateFrom.Year;
            ReportMothlyBoardXml export = new ReportMothlyBoardXml(report, config);
            return export.WriteFileXML();
        }
    }
}
