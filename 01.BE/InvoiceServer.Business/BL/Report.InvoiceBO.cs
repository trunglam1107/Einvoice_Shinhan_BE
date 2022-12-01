using DQSServer.Business.ExportXml;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public partial class InvoiceBO : IInvoiceBO
    {
        string folderStore = System.Configuration.ConfigurationManager.AppSettings["FolderInvoiceFile"];
        public ExportFileInfo PrintViewSituationInvoiceUsage(ConditionReportUse condition, CompanyInfo company)
        {
            ReportInvoiceUsing dataExport = DataSituationInvoiceUsage(condition, company);
            SituationInvoiceUsageView export = new SituationInvoiceUsageView(dataExport, config, condition.Language);
            return export.ExportFile();
        }

        public ReportInvoiceUsing ViewSituationInvoiceUsage(ConditionReportUse codition, CompanyInfo company)
        {
            return DataSituationInvoiceUsage(codition, company);
        }
        public ReportSituationInvoiceUsage ViewSituationInvoiceUsageSummary(ConditionReportUse codition, CompanyInfo company)
        {
            return DataSituationInvoiceUsageSummary(codition, company);
        }
        public ReportSituationInvoiceUsage ViewSituationInvoiceUsageSixMonth(ConditionReportUse codition, CompanyInfo company)
        {
            return DataSituationInvoiceUsageSixMonth(codition, company);
        }

        public int GetInvoiceUsed(ConditionReportUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceRepository.GetNumberInvoiceUsed(condition);
        }

        private ReportSituationInvoiceUsage DataSituationInvoiceUsageSixMonth(ConditionReportUse codition, CompanyInfo company)
        {
            if (codition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportSituationInvoiceUsage report = new ReportSituationInvoiceUsage(company);
            report.items = GetSituationInvoiceUsageSixMonth(codition);
            report.Year = codition.Year;
            report.Precious = codition.Precious;
            report.IsViewByMonth = codition.IsMonth > 0;
            report.Branch = new MyCompanyInfo(this.myCompanyRepository.GetById(codition.Branch ?? 0));
            return report;
        }

        private ReportInvoiceUsing DataSituationInvoiceUsage(ConditionReportUse codition, CompanyInfo company)
        {
            if (codition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportInvoiceUsing report = new ReportInvoiceUsing(company);
            report.items = GetSituationInvoiceUsage(codition);
            report.Year = codition.Year;
            report.Precious = codition.Precious;
            report.IsViewByMonth = codition.IsMonth > 0;
            report.Branch = new MyCompanyInfo(this.myCompanyRepository.GetById(codition.Branch ?? 0));
            return report;
        }
        private ReportSituationInvoiceUsage DataSituationInvoiceUsageSummary(ConditionReportUse codition, CompanyInfo company)
        {
            if (codition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportSituationInvoiceUsage report = new ReportSituationInvoiceUsage(company);
            report.items = GetSituationInvoiceUsageSummary(codition);
            report.Year = codition.Year;
            report.Precious = codition.Precious;
            report.IsViewByMonth = codition.IsMonth > 0;
            report.Branch = new MyCompanyInfo(this.myCompanyRepository.GetById(codition.Branch ?? 0));
            return report;
        }
        public ReportGeneralInvoice ViewGeneralInvoiceUsage(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportGeneralInvoice report = new ReportGeneralInvoice(condition.CurrentUser?.Company);
            report.items = FillterListInvoiceGroupBySummaryViewReport(condition).ToList();
            report.Year = condition.Year;
            report.Precious = condition.Month;
            report.IsViewByMonth = condition.IsMonth > 0;
            report.Branch = new MyCompanyInfo(this.myCompanyRepository.GetById(condition.Branch ?? 0));
            report.TotalRecords = CountListInvoiceGroupBySummaryViewReport(condition);
            return report;
        }
        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBySummaryViewReport(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            //x.TotalAmount *= exChangeRate;
            //x.AmountTax *= exChangeRate;
            //x.Sum *= exChangeRate;

            var listInvoiceReport = this.invoiceRepository.FillterListInvoiceSummary(condition).Select(x =>
                                        {
                                            var exChangeRate = x.Currency == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                                            x.Total *= exChangeRate;
                                            x.TotalTax *= exChangeRate;
                                            x.SumAmountInvoice *= exChangeRate;
                                            return x;
                                        }).ToList();
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);

            foreach (var item in listInvoiceReport)
            {
                FormatBySystemSettingBTH(item, systemSetting);
            }
            return listInvoiceReport;
        }
        public long CountListInvoiceGroupBySummaryViewReport(ConditionReportDetailUse condition)
        {
            return this.invoiceRepository.CountFillterListInvoiceSummaryViewReport(condition);
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoice(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var listInvoiceReport = this.invoiceRepository.FillterListInvoice(condition);
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var item in listInvoiceReport)
            {
                FormatBySystemSetting(item, systemSetting);
            }
            return listInvoiceReport;
        }

        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBy(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var listInvoiceReport = this.invoiceRepository.FillterListInvoiceGroupBy(condition);
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var item in listInvoiceReport)
            {
                FormatBySystemSetting(item, systemSetting);
            }
            return listInvoiceReport;
        }
        public IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBySummary(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var listInvoiceReport = this.invoiceRepository.FillterListInvoiceSummary(condition);
            var systemSetting = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            foreach (var item in listInvoiceReport)
            {
                FormatBySystemSetting(item, systemSetting);
            }
            return listInvoiceReport;
        }
        public long CountFillterListInvoice(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceRepository.CountList(condition);
        }

        public long CountFillterListInvoiceGroupBy(ConditionReportDetailUse condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceRepository.CountList(condition, 1);
        }

        private List<ReportMonth> GetSituationInvoiceUsage(ConditionReportUse condition)
        {
            var reportUses = new List<ReportMonth>();
            var listRegisterTemplate = this.noticeInvoiceDetailRepository.FilterNotificationUseInvoiceDetailsUseReport(condition.CompanyId, condition.Branch ?? 0, condition.DateFrom.Value, condition.DateTo.Value).ToList();

            listRegisterTemplate.ForEach(p =>
            {
                ReportUse item = new ReportUse();
                condition.Symbol = p.NotificationUseCode;
                condition.RegisterTemplateId = p.RegisterTemplatesId;
                var numberInvoiceRealse = this.invoiceRepository.GetNumberInvoiceReleased(condition);
                if (numberInvoiceRealse >= 0)
                {
                    //this.GetSituationInvoiceUsageItem(p, condition, reportUses, listInvoiceRealse, true);
                    this.GetSituationInvoiceUsageItemReport(p, condition, reportUses, numberInvoiceRealse);
                }
            });

            return reportUses;
        }
        private List<ReportUse> GetSituationInvoiceUsageSummary(ConditionReportUse condition)
        {
            var reportUses = new List<ReportUse>();
            var listRegisterTemplate = this.noticeInvoiceDetailRepository.FilterNotificationUseInvoiceDetailsUseReport(condition.CompanyId, condition.Branch ?? 0, condition.DateFrom.Value, condition.DateTo.Value).ToList();

            listRegisterTemplate.ForEach(p =>
            {
                ReportUse item = new ReportUse();
                condition.Symbol = p.NotificationUseCode;
                condition.RegisterTemplateId = p.RegisterTemplatesId;
                var listInvoiceRealse = this.invoiceRepository.GetNumberInvoiceReleasedSummary(condition);
                if (listInvoiceRealse >= 0)
                {
                    this.GetSituationInvoiceUsageItem(p, condition, reportUses, listInvoiceRealse);
                }
            });

            return reportUses;
        }
        private void GetSituationInvoiceUsageItem(RegisterTemplateReport registerTemplateReport, ConditionReportUse condition, List<ReportUse> reportUses, long listInvoiceRealse, bool isReport = false)
        {
            ReportUse item = new ReportUse();
            condition.isReport = item.isReport = isReport;
            item.InvoiceSample = registerTemplateReport.InvoiceSampleName;
            item.InvoiceSampleCode = registerTemplateReport.RegisterTemplatesCode;
            item.InvoiceSymbol = registerTemplateReport.NotificationUseCode;
            item.InvoiceCode = registerTemplateReport.TypeCodeInvoice;
            var dateToPrivious = DateTime.Parse(condition.DateTo.Value.ToString()).AddDays(-1);
            List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegister = new List<NOTIFICATIONUSEINVOICEDETAIL>();
            if (isReport)
            {
                numberInvoiceRegister = GetNumberInvoiceRegisterRpUs(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, dateToPrivious).ToList();
                numberInvoiceRegister = numberInvoiceRegister.Where(p => p.USEDDATE < condition.DateFrom).ToList();
                if (numberInvoiceRegister.Count > 0)
                {
                    item.RegistNumberFrom = numberInvoiceRegister.OrderBy(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                    item.RegistNumberTo = numberInvoiceRegister.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
                }
                List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegisterBetweenDate = GetNumberInvoiceRegisterBetweenDate(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, condition.DateTo).ToList();
                numberInvoiceRegisterBetweenDate = numberInvoiceRegisterBetweenDate.Where(p => p.USEDDATE >= condition.DateFrom && p.USEDDATE <= dateToPrivious).ToList();
                if (numberInvoiceRegisterBetweenDate.Count > 0)
                {
                    item.SoMua_TuSo = numberInvoiceRegisterBetweenDate.OrderByDescending(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                    item.SoMua_DenSo = numberInvoiceRegisterBetweenDate.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
                }
            }
            else
            {
                numberInvoiceRegister = GetNumberInvoiceRegister(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, dateToPrivious).ToList();
                if (numberInvoiceRegister.Count > 0)
                {
                    item.RegistNumberFrom = numberInvoiceRegister.OrderBy(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                    item.RegistNumberTo = numberInvoiceRegister.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
                }
            }

            item.SoDuHoaDonDauKy = GetMaxInvoiceNo(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom);
            item.TongSuDung_TuSo = this.invoiceRepository.GetMinNo(condition);
            item.TongSuDung_DenSo = this.invoiceRepository.GetMaxNo(condition);
            var NumberInvoiceCanceled = this.invoiceRepository.GetNumberInvoiceCanceled(condition);
            var listInvoiceCanceled = this.invoiceRepository.FilterInvoiceCanceled(condition);
            var listInvoiceNoteRelease = this.invoiceRepository.FilterInvoiceNotRelease(condition);
            item.HuyBoSoLuong = NumberInvoiceCanceled;
            item.ChuaPhatHanh = listInvoiceNoteRelease;
            item.DieuChinh = this.invoiceRepository.FilterInvoiceAdjustment(condition);
            item.ThayThe = this.invoiceRepository.FilterInvoiceSubstitute(condition);
            if (listInvoiceCanceled.Count() > 0)
            {
                item.HuyBoSo = GetStringFromListInvoice(listInvoiceCanceled);
            }
            else
            {
                item.HuyBoSo = "";
            }
            var listInvoiceDeleted = this.invoiceRepository.FilterInvoiceDeleted(condition);
            item.XoaBoSoLuong = listInvoiceDeleted.Count();
            if (item.XoaBoSoLuong > 0)
            {
                item.XoaBoSo = GetStringFromListInvoice(listInvoiceDeleted);
            }
            if (isReport)
            {
                item.SoLuong_DaSuDung = listInvoiceRealse - (item.XoaBoSoLuong + item.HuyBoSoLuong);
            }
            else
            {
                item.SoLuong_DaSuDung = listInvoiceRealse;
            }
            REPORTCANCELLINGDETAIL cancellingDetail = GetInvoiceCancelling(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, condition.DateTo);
            if (cancellingDetail != null)
            {
                item.HuySoLuong = isReport ? 0 : cancellingDetail.NUMBER.ToInt(0).Value;
                item.HuySo = GetListNumberBetween(cancellingDetail.NUMBERFROM, cancellingDetail.NUMBERTO);
            }
            if (item.DauKy_Tong > 0)
            {
                reportUses.Add(item);
            }
        }

        private void GetSituationInvoiceUsageItemReport(RegisterTemplateReport registerTemplateReport, ConditionReportUse condition, List<ReportMonth> reportUses, long listInvoiceRealse)
        {
            ReportMonth item = new ReportMonth();
            item.InvoiceSample = registerTemplateReport.InvoiceSampleName;
            item.InvoiceSampleCode = registerTemplateReport.RegisterTemplatesCode;
            item.InvoiceSymbol = registerTemplateReport.NotificationUseCode;
            item.InvoiceCode = registerTemplateReport.TypeCodeInvoice;

            List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegister = new List<NOTIFICATIONUSEINVOICEDETAIL>();

            numberInvoiceRegister = GetNumberInvoiceRegisterRpUs(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, condition.DateTo).ToList();
            numberInvoiceRegister = numberInvoiceRegister.Where(p => p.USEDDATE < condition.DateFrom).ToList();
            if (numberInvoiceRegister.Count > 0)
            {
                item.RegistNumberFrom = numberInvoiceRegister.OrderBy(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                item.RegistNumberTo = numberInvoiceRegister.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
            }
            List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegisterBetweenDate = GetNumberInvoiceRegisterBetweenDateReport(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, condition.DateTo).ToList();
            numberInvoiceRegisterBetweenDate = numberInvoiceRegisterBetweenDate.Where(p => p.USEDDATE >= condition.DateFrom && p.USEDDATE < condition.DateTo).ToList();
            if (numberInvoiceRegisterBetweenDate.Count > 0)
            {
                item.SoMua_TuSo = numberInvoiceRegisterBetweenDate.OrderByDescending(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                item.SoMua_DenSo = numberInvoiceRegisterBetweenDate.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
            }
            item.SoDuHoaDonDauKy = GetMaxInvoiceNo(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom);
            item.TongSuDung_TuSo = this.invoiceRepository.GetMinNo(condition);
            item.TongSuDung_DenSo = this.invoiceRepository.GetMaxNo(condition);
            //var NumberInvoiceCanceled = this.invoiceRepository.GetNumberInvoiceCanceled(condition);
            //var listInvoiceCanceled = this.invoiceRepository.FilterInvoiceCanceled(condition);
            var listInvoiceNoteRelease = this.invoiceRepository.FilterInvoiceNotRelease(condition);
            //item.HuyBoSoLuong = NumberInvoiceCanceled;
            item.ChuaPhatHanh = listInvoiceNoteRelease;
            item.DieuChinh = this.invoiceRepository.FilterInvoiceAdjustment(condition);
            item.ThayThe = this.invoiceRepository.FilterInvoiceSubstitute(condition);
            var listInvoiceDeleted = this.invoiceRepository.FilterInvoiceDeleted(condition);
            item.XoaBoSoLuong = listInvoiceDeleted.Count();
            if (item.XoaBoSoLuong > 0)
            {
                item.XoaBoSo = GetStringInvoiceNo(listInvoiceDeleted);
            }
            item.SoLuong_DaSuDung = listInvoiceRealse - (item.XoaBoSoLuong + item.HuyBoSoLuong);
            REPORTCANCELLINGDETAIL cancellingDetail = GetInvoiceCancelling(registerTemplateReport.CompanyId.Value, registerTemplateReport.RegisterTemplatesId, registerTemplateReport.NotificationUseCode, condition.DateFrom, condition.DateTo);
            if (cancellingDetail != null)
            {
                item.HuySoLuong = (long.Parse(cancellingDetail.NUMBERTO) - long.Parse(cancellingDetail.NUMBERFROM)) + 1;
                item.HuySo = GetListNumberBetween(cancellingDetail.NUMBERFROM, cancellingDetail.NUMBERTO);
                item.TongSuDung_DenSo = cancellingDetail.NUMBERTO;
                item.TongSuDung_TuSo = item.TongSuDung_TuSo.IsNullOrEmpty() ? cancellingDetail.NUMBERFROM : item.TongSuDung_TuSo;
            }
            if (item.DauKy_Tong > 0)
            {
                reportUses.Add(item);
            }
        }

        private List<ReportUse> GetSituationInvoiceUsageSixMonth(ConditionReportUse condition)
        {
            var reportUses = new List<ReportUse>();
            var listRegisterTemplate = this.noticeInvoiceDetailRepository.FilterNotificationUseInvoiceDetailsUseReport(condition.CompanyId, condition.Branch ?? 0, condition.DateFrom.Value, condition.DateTo.Value).ToList();

            listRegisterTemplate.ForEach(p =>
            {
                ReportUse item = new ReportUse();
                condition.Symbol = p.NotificationUseCode;
                condition.RegisterTemplateId = p.RegisterTemplatesId;
                var listInvoiceRealse = this.invoiceRepository.GetNumberInvoiceReleased(condition);
                if (listInvoiceRealse >= 0)
                {
                    item.InvoiceSample = p.InvoiceSampleName;
                    item.InvoiceSampleCode = p.RegisterTemplatesCode;
                    item.InvoiceSymbol = p.NotificationUseCode;
                    item.InvoiceCode = p.TypeCodeInvoice;
                    var dateToPrivious = DateTime.Parse(condition.DateFrom.Value.ToString()).AddDays(-1);
                    List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegister = GetNumberInvoiceRegister(p.CompanyId.Value, p.RegisterTemplatesId, p.NotificationUseCode, dateToPrivious).ToList();
                    if (numberInvoiceRegister.Count > 0)
                    {
                        item.RegistNumberFrom = numberInvoiceRegister.OrderBy(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                        item.RegistNumberTo = numberInvoiceRegister.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
                    }

                    List<NOTIFICATIONUSEINVOICEDETAIL> numberInvoiceRegisterBetweenDate = GetNumberInvoiceRegisterBetweenDate(p.CompanyId.Value, p.RegisterTemplatesId, p.NotificationUseCode, condition.DateFrom, condition.DateTo).ToList();
                    if (numberInvoiceRegisterBetweenDate.Count > 0)
                    {
                        item.SoMua_TuSo = numberInvoiceRegisterBetweenDate.OrderBy(i => i.NUMBERFROM).FirstOrDefault().NUMBERFROM;
                        item.SoMua_DenSo = numberInvoiceRegisterBetweenDate.OrderByDescending(i => i.NUMBERTO).FirstOrDefault().NUMBERTO;
                    }

                    item.SoDuHoaDonDauKy = GetMaxInvoiceNo(p.CompanyId.Value, p.RegisterTemplatesId, p.NotificationUseCode, condition.DateFrom);

                    item.TongSuDung_TuSo = this.invoiceRepository.GetMinNo(condition);
                    item.TongSuDung_DenSo = this.invoiceRepository.GetMaxNo(condition);
                    var NumberInvoiceDelete = this.invoiceRepository.GetNumberInvoiceCanceled(condition);

                    item.HuyBoSoLuong = NumberInvoiceDelete;

                    item.SoLuong_DaSuDung = (listInvoiceRealse - item.HuyBoSoLuong);

                    REPORTCANCELLINGDETAIL cancellingDetail = GetInvoiceCancelling(p.CompanyId.Value, p.RegisterTemplatesId, p.NotificationUseCode, condition.DateFrom, condition.DateTo);
                    if (cancellingDetail != null)
                    {
                        item.HuySoLuong = cancellingDetail.NUMBER.ToInt(0).Value;
                    }
                    if (item.DauKy_Tong > 0)
                    {
                        reportUses.Add(item);
                    }
                }
            });

            return reportUses;
        }

        private string GetStringFromListInvoice(IEnumerable<INVOICE> invoices)
        {
            string arrayInvoiceNo = string.Empty;
            invoices.ForEach(p =>
            {
                arrayInvoiceNo = arrayInvoiceNo + p.NO + "; ";
            });
            arrayInvoiceNo = arrayInvoiceNo.Remove(arrayInvoiceNo.Length - 2);
            return arrayInvoiceNo;
        }
        private string GetStringInvoiceNo(IEnumerable<INVOICE> invoices)
        {
            var lst = invoices.OrderBy(p => p.INVOICENO);
            var arr = GroupConsecutive(lst).ToArray();
            string lstInvoiceNo = string.Empty;
            foreach (var item in arr)
            {
                if (item.Count() == 1)
                {
                    lstInvoiceNo = lstInvoiceNo + item.ToArray()[0].NO + "; ";
                }
                else
                {
                    var itemCount = item.Count();
                    lstInvoiceNo = lstInvoiceNo + item.ToArray()[0].NO + " - " + item.ToArray()[itemCount - 1].NO + "; ";
                }
            }
            return lstInvoiceNo.Remove(lstInvoiceNo.Length - 2);
        }

        public IEnumerable<IEnumerable<INVOICE>> GroupConsecutive(IEnumerable<INVOICE> invoices)
        {
            var group = new List<INVOICE>();
            foreach (var i in invoices)
            {
                if (group.Count == 0 || i.INVOICENO - group[group.Count - 1].INVOICENO <= 1)
                    group.Add(i);
                else
                {
                    yield return group;
                    group = new List<INVOICE> { i };
                }
            }
            yield return group;
        }
        private string GetListNumberBetween(string numberFrom, string numberTo)
        {

            if (numberFrom.IsNullOrEmpty() || numberTo.IsNullOrEmpty())
            {
                return string.Empty;
            }
            if ((long.Parse(numberTo) - long.Parse(numberFrom)) == 1)
            {
                return string.Format("{0}; {1}", numberFrom, numberTo);
            }
            return string.Format("{0} - {1}", numberFrom, numberTo);
        }

        public ExportFileInfo PrintViewSituationInvoiceUsageXML(ConditionReportUse codition, CompanyInfo company)
        {
            if (codition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ReportInvoiceUsing report = new ReportInvoiceUsing(company);
            report.items = GetSituationInvoiceUsage(codition);
            report.Year = codition.Year;
            report.Precious = codition.Precious;
            report.IsViewByMonth = codition.IsMonth == 0;
            report.DateFrom = codition.DateFrom ?? DateTime.Now;
            report.DateTo = codition.DateTo ?? DateTime.Now;
            report.Branch = new MyCompanyInfo(this.myCompanyRepository.GetById(codition.Branch ?? 0));
            SituationInvoiceUsageXml export = new SituationInvoiceUsageXml(report, config);
            return export.WriteFileXML();
        }

        public ExportFileInfo DownloadInvoiceXML(long id)
        {
            InvoicePrintInfo invoicePrint = new InvoicePrintInfo();
            if (this.invoiceInfo.Id != 0 && this.invoiceInfo.InvoiceNo != null)
            {
                invoicePrint = this.invoiceInfo;
            }
            else
            {
                invoicePrint = this.invoiceRepository.GetInvoicePrintInfo(id);
                //invoicePrint = this.invoiceRepository.GetInvoiceInfo(id);
            }
            //var statistical = this.invoiceStatisticalRepository.FilterInvoiceStatistical(id);
            //var invoiceGift = this.invoiceGiftRepository.FilterInvoiceGift(id);
            //if (statistical.ToList().Count > 0)
            //{
            //    invoicePrint.StatisticalSumAmount = 0;
            //    invoicePrint.StatisticalSumTaxAmount = 0;
            //    statistical.ForEach(p =>
            //    {
            //        invoicePrint.StatisticalSumAmount += p.AMOUNT;
            //        invoicePrint.StatisticalSumTaxAmount += p.AMOUNTTAX;
            //    });
            //}
            //if (invoiceGift.ToList().Count > 0)
            //{
            //    invoicePrint.GiftSumAmount = 0;
            //    invoicePrint.GiftSumTaxAmount = 0;
            //    invoiceGift.ForEach(p =>
            //    {
            //        invoicePrint.GiftSumAmount += p.PRETAXAMOUNT;
            //        invoicePrint.GiftSumTaxAmount += p.AMOUNTTAX;
            //    });
            //}
            //Statisticals = statistical.ToList(),
            //Invoicegifts = invoiceGift.ToList(),

            invoicePrint.DateSign = invoicePrint.DateRelease.HasValue ? invoicePrint.DateRelease.Value.ToString("dd/MM/yyy") : String.Empty;
            invoicePrint.ClientSignDate = invoicePrint.DateClientSign.HasValue ? invoicePrint.DateClientSign.Value.ToString("dd/MM/yyy") : String.Empty;
            invoicePrint.InvoiceItems = GetInvoiceDetail(invoicePrint.Id).Select(p => new InvoiceItem(p)).ToList();

            var systemSetting = this.systemSettingRepository.FilterSystemSetting(invoicePrint.CompanyId);
            FormatBySystemSetting(invoicePrint, systemSetting);

            MYCOMPANY myconpany = GetCompany(invoicePrint.CompanyId);
            invoicePrint.Signature = invoicePrint.Signature ?? myconpany.COMPANYNAME;
            this.config.BuildAssetByCompany(new CompanyInfo(myconpany));

            InvoiceExportXmlModel invoiceExportXmlModel = new InvoiceExportXmlModel()
            {
                InvoicePrintInfo = invoicePrint,
                PrintConfig = this.config,
                SystemSettingInfo = systemSetting,
            };
            InvoiceXml invoiceXml = new InvoiceXml(invoiceExportXmlModel);
            return invoiceXml.ExportReport();
        }

        public string ExportXmlsJob(InvoiceExportXmlModel invoiceExportXml)
        {
            InvoiceXmlJob invoiceXml = new InvoiceXmlJob(invoiceExportXml);
            return invoiceXml.ExportXmlJob();
        }
        public InvoiceReleaseInfo GetInvoiceReleaseInfo(long id)
        {
            return this.invoiceRepository.GetInvoiceReleaseInfo(id);
        }


        private IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegister(long companyId, long registerTemplatesId, string symbol, DateTime? dateTo)
        {
            return this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetails(companyId, registerTemplatesId, symbol.Replace("/", ""), dateTo);
        }
        private IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterRpUs(long companyId, long registerTemplatesId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            return this.noticeInvoiceDetailRepository.GetNotificationUseInvoiceDetailsRpUs(companyId, registerTemplatesId, symbol.Replace("/", ""), dateFrom, dateTo);
        }
        private IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDate(long companyId, long registerTemplatesId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            return this.noticeInvoiceDetailRepository.GetNumberInvoiceRegisterBetweenDate(companyId, registerTemplatesId, symbol, dateFrom, dateTo);
        }
        private IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDateReport(long companyId, long registerTemplatesId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            return this.noticeInvoiceDetailRepository.GetNumberInvoiceRegisterBetweenDateReport(companyId, registerTemplatesId, symbol, dateFrom, dateTo);
        }
        private string GetMaxInvoiceNo(long companyId, long registerId, string symbol, DateTime? dateFrom)
        {
            string maxInvoiceNo = string.Empty;
            INVOICE maxInvoice = this.invoiceRepository.GetMaxInvoice(companyId, registerId, symbol, dateFrom);
            if (maxInvoice == null)
            {
                return maxInvoiceNo;
            }

            maxInvoiceNo = maxInvoice.NO;
            return maxInvoiceNo;
        }

        private REPORTCANCELLINGDETAIL GetInvoiceCancelling(long companyId, long registerId, string symbol, DateTime? dateFrom, DateTime? dateTo)
        {
            REPORTCANCELLINGDETAIL cancellingDetail = this.reportCancellingDetailRepository.Filter(companyId, registerId, symbol, dateFrom, dateTo);
            return cancellingDetail;
        }

        public ExportFileInfo DownloadReportCombine(ConditionReportDetailUse condition)
        {
            var CurrentCompany = this.myCompanyRepository.GetById(condition.Branch ?? 0);
            var company = condition.CurrentUser.Company.DeepCopy();
            company.CompanyName = CurrentCompany.COMPANYNAME;
            company.TaxCode = CurrentCompany.TAXCODE;
            var dataRport = new ReportCombineExport(company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            dataRport.Items = this.invoiceRepository.FillterListInvoice(condition).ToList();
            ReportCombineView export = new ReportCombineView(dataRport, config, systemSettingInfo, condition.Language);
            return export.ExportFile();
        }

        public ExportFileInfo DownloadReportInvoiceDetail(ConditionReportDetailUse condition)
        {
            var dataRport = new ReportCombineExport(condition.CurrentUser?.Company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            dataRport.Items = this.invoiceRepository.FillterListInvoice(condition).AsQueryable().OrderByDescending(x => x.InvoiceCode).ThenByDescending(x => x.InvoiceSymbol).ThenByDescending(x => x.InvoiceNo).ThenByDescending(x => x.Created).ToList();
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }

            ReportInvoiceDetailStatisticsView export = new ReportInvoiceDetailStatisticsView(dataRport, systemSettingInfo, config, condition.Language);
            return export.ExportFile(condition.Language);
        }

        public ExportFileInfo DownloadReportInvoiceDetailGroupBy(ConditionReportDetailUse condition)
        {
            var dataRport = new ReportCombineExport(condition.CurrentUser?.Company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            dataRport.Items = this.invoiceRepository.FillterListInvoiceGroupBy(condition).AsQueryable().OrderByDescending(x => x.InvoiceCode).ThenByDescending(x => x.InvoiceSymbol).ThenByDescending(x => x.InvoiceNo).ThenByDescending(x => x.Created).ToList();
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }

            ReportInvoiceDetailStatisticsViewGroupBy export = new ReportInvoiceDetailStatisticsViewGroupBy(dataRport, systemSettingInfo, config, condition.Language);
            return export.ExportFile(condition.Language);
        }
        public ReportInvoiceDetail ViewGeneralInvoiceUsage(ConditionReportUse codition, CompanyInfo company)
        {
            return null;
        }
        public ExportFileInfo DownloadReportInvoiceDataSummary(ConditionReportDetailUse condition)
        {
            var companyInfo = this.myCompanyRepository.GetById((long)condition.Branch);
            var myCompanyInfo = new CompanyInfo(companyInfo);
            var dataRport = new ReportCombineExport(myCompanyInfo);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.Branch ?? 0);
            dataRport.Items = this.invoiceRepository.FillterListInvoiceSummary(condition).Select(x =>
                                        {
                                            var exChangeRate = x.Currency == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                                            x.Total *= exChangeRate;
                                            x.TotalTax *= exChangeRate;
                                            x.SumAmountInvoice *= exChangeRate;
                                            return x;
                                        }).ToList();
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSettingBTH(item, systemSettingInfo);
            }

            dataRport.Month = condition.Month;
            dataRport.Year = condition.Year;
            ReportInvoiceDataSummaryView export = new ReportInvoiceDataSummaryView(dataRport, systemSettingInfo, config, condition);
            return export.ExportFile();
        }
        private void FormatBySystemSettingBTH(ReportInvoiceDetail reportInvoiceDetail, SystemSettingInfo SSInfo)
        {
            var systemSetting = SSInfo.DeepCopy();
            StandardSystemSettingIfVNDBTH(systemSetting, reportInvoiceDetail.Currency);
            reportInvoiceDetail.Quantity = FormatNumber.SetFractionDigit(reportInvoiceDetail.Quantity, systemSetting.Quantity);
            reportInvoiceDetail.Price = FormatNumber.SetFractionDigit(reportInvoiceDetail.Price, systemSetting.Price);
            reportInvoiceDetail.TotalAmount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalAmount, systemSetting.Amount);
            reportInvoiceDetail.Sum = FormatNumber.SetFractionDigit(reportInvoiceDetail.Sum, systemSetting.Amount);
            reportInvoiceDetail.SumAmountInvoice = FormatNumber.SetFractionDigit(reportInvoiceDetail.SumAmountInvoice, systemSetting.Amount);
            reportInvoiceDetail.TotalDiscount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalDiscount, systemSetting.Amount);
            reportInvoiceDetail.TotalDiscountTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalDiscountTax, systemSetting.Amount);
            reportInvoiceDetail.AmountTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.AmountTax, systemSetting.Amount);
            reportInvoiceDetail.TotalTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalTax, systemSetting.Amount);
            reportInvoiceDetail.CurrencyExchangeRate = FormatNumber.SetFractionDigit(reportInvoiceDetail.CurrencyExchangeRate, systemSetting.ExchangeRate);

            reportInvoiceDetail.TaxAmount = (reportInvoiceDetail.TotalAmount * reportInvoiceDetail.Tax) / 100;
            reportInvoiceDetail.TaxAmount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TaxAmount, systemSetting.Amount);

            //reportInvoiceDetail.Total = (reportInvoiceDetail.TotalAmount + reportInvoiceDetail.TaxAmount);
            reportInvoiceDetail.Total = FormatNumber.SetFractionDigit(reportInvoiceDetail.Total, systemSetting.Amount);

            reportInvoiceDetail.UnitPriceAfterTax = reportInvoiceDetail.Quantity == 0 ? reportInvoiceDetail.Price : (reportInvoiceDetail.Total / reportInvoiceDetail.Quantity);
            reportInvoiceDetail.UnitPriceAfterTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.UnitPriceAfterTax, systemSetting.Price);
        }
        private void FormatBySystemSetting(ReportInvoiceDetail reportInvoiceDetail, SystemSettingInfo SSInfo)
        {
            var systemSetting = SSInfo.DeepCopy();
            StandardSystemSettingIfVND(systemSetting, reportInvoiceDetail.Currency);
            reportInvoiceDetail.Quantity = FormatNumber.SetFractionDigit(reportInvoiceDetail.Quantity, systemSetting.Quantity);
            reportInvoiceDetail.Price = FormatNumber.SetFractionDigit(reportInvoiceDetail.Price, systemSetting.Price);
            reportInvoiceDetail.TotalAmount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalAmount, systemSetting.Amount);
            reportInvoiceDetail.Sum = FormatNumber.SetFractionDigit(reportInvoiceDetail.Sum, systemSetting.Amount);
            reportInvoiceDetail.SumAmountInvoice = FormatNumber.SetFractionDigit(reportInvoiceDetail.SumAmountInvoice, systemSetting.Amount);
            reportInvoiceDetail.TotalDiscount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalDiscount, systemSetting.Amount);
            reportInvoiceDetail.TotalDiscountTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.TotalDiscountTax, systemSetting.Amount);
            reportInvoiceDetail.AmountTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.AmountTax, systemSetting.Amount);
            reportInvoiceDetail.CurrencyExchangeRate = FormatNumber.SetFractionDigit(reportInvoiceDetail.CurrencyExchangeRate, systemSetting.ExchangeRate);

            reportInvoiceDetail.TaxAmount = (reportInvoiceDetail.TotalAmount * reportInvoiceDetail.Tax) / 100;
            reportInvoiceDetail.TaxAmount = FormatNumber.SetFractionDigit(reportInvoiceDetail.TaxAmount, systemSetting.Amount);

            //reportInvoiceDetail.Total = (reportInvoiceDetail.TotalAmount + reportInvoiceDetail.TaxAmount);
            reportInvoiceDetail.Total = FormatNumber.SetFractionDigit(reportInvoiceDetail.Total, systemSetting.Amount);

            reportInvoiceDetail.UnitPriceAfterTax = reportInvoiceDetail.Quantity == 0 ? reportInvoiceDetail.Price : (reportInvoiceDetail.Total / reportInvoiceDetail.Quantity);
            reportInvoiceDetail.UnitPriceAfterTax = FormatNumber.SetFractionDigit(reportInvoiceDetail.UnitPriceAfterTax, systemSetting.Price);
        }
        public void UpdataInvoiceStatus(InvoiceInfo invoice, int status)
        {
            var invocurrent = this.invoiceRepository.GetById(invoice.Id);
            invocurrent.INVOICESTATUS = status;
            invocurrent.UPDATEDDATE = DateTime.Now;
            this.invoiceRepository.Update(invocurrent);

        }

        public ExportFileInfo ReportInvoiceDataSummaryXML(ConditionReportDetailUse condition)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            //var dataRport = new ReportCombineExport(condition.CurrentUser?.Company);
            //var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            var companyInfo = this.myCompanyRepository.GetById((long)condition.Branch);
            var myCompanyInfo = new CompanyInfo(companyInfo);
            var dataRport = new ReportCombineExport(myCompanyInfo);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(1);
            dataRport.Items = this.invoiceRepository.FillterListInvoiceSummary(condition).Select(x =>
            {
                var exChangeRate = x.Currency == DefaultFields.CURRENCY_VND ? 1 : x.CurrencyExchangeRate;
                x.Total *= exChangeRate;
                x.TotalTax *= exChangeRate;
                x.SumAmountInvoice *= exChangeRate;
                return x;
            }).ToList();
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSettingBTH(item, systemSettingInfo);
            }
            dataRport.Month = condition.Month;
            dataRport.Year = condition.Year;
            //dataRport.Time = condition.Time;
            dataRport.Time = condition.Time == 0 ? 1 : 0;
            dataRport.TimeNo = condition.TimeNo;
            LKDLieu.ListLKDLieu.TryGetValue(condition.IsMonth, out string periodsGeneral);
            dataRport.periods = periodsGeneral;
            ReportGeneralMonthlyXml export = new ReportGeneralMonthlyXml(dataRport, config);
            var XMLContextob = export.WriteFileXML();
            //return export.WriteFileXML();
            string XMLContext = "";
            if (XMLContextob != null)
            {
                XMLContext = XMLContextob.ToString();
            }
            fileInfo.FileName = string.Format("bao-cao-tong-hop-du-lieu-hoa-don-{0}.xml",DateTime.Now.ToString("yyyyMMdd"));
            fileInfo.FullPathFileName = Path.Combine(this.folderStore, condition.CompanyId.ToString(), "Report", fileInfo.FileName);
            string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            if (!File.Exists(fileInfo.FullPathFileName))
            {
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }
            else
            {
                File.Delete(fileInfo.FullPathFileName);
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }

            //string path = CallApiSignReport(fileInfo.FullPathFileName, signature);
            ////string pathFolder = Path.Combine(@"C:\store\unit", condition.CompanyId.ToString(), @"Report\Sign", fileInfo.FileName);
            //if (File.Exists(path))
            //    fileInfo.FullPathFileName = path;

            return fileInfo;

        }
        public decimal CountInvoiceNotRelease(long companyId)
        {
            return this.invoiceRepository.CountInvoiceNotRelease(companyId);
        }

        public decimal CountInvoiceAdjustment(long companyId)
        {
            return this.invoiceRepository.CountInvoiceAdjustment(companyId);
        }

        public decimal CountInvoiceCancel(long companyId)
        {
            return this.invoiceRepository.CountInvoiceCancel(companyId);
        }

        public decimal CountInvoiceSubstitute(long companyId)
        {
            return this.invoiceRepository.CountInvoiceSubstitute(companyId);
        }

        public decimal CountInvoiceDeleted(long companyId)
        {
            return this.invoiceRepository.CountInvoiceDeleted(companyId);
        }

        public decimal CountInvoiceRelease(long companyId)
        {
            return this.invoiceRepository.CountInvoiceRelease(companyId);
        }

        public int GetExpirceDateToken(long companyId)
        {
            var dateNumber = this.declarationRepository.GetExpireDateToken(companyId);

            return dateNumber;
        }

    }
}
