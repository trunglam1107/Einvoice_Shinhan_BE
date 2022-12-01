using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class NoticeUseInvoiceDetailBO : INoticeUseInvoiceDetailBO
    {
        #region Fields, Properties

        private readonly INotificationUseInvoiceDetailRepository invoiceUseDetailRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IReportCancellingDetailRepository reportCancellingDetailRepository;
        protected readonly static object lockObject = new object();

        #endregion

        #region Contructor

        public NoticeUseInvoiceDetailBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.invoiceUseDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.reportCancellingDetailRepository = repoFactory.GetRepository<IReportCancellingDetailRepository>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
        }

        #endregion

        #region Methods

        public UseInvoiceDetailInfo GetNoticeUseInvoiceDetail(long id)
        {
            NOTIFICATIONUSEINVOICEDETAIL currentNoticeUseDetailInvoice = GetInvoiceUseDetail(id);
            return new UseInvoiceDetailInfo(currentNoticeUseDetailInvoice);
        }

        #endregion
        public IEnumerable<string> FilterInvoiceSymbol(long companyId, long invoiceTemplateId, bool? isCreate)
        {
            IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> notificationUseInvoiceDetails = this.invoiceUseDetailRepository.FilterInvoiceSymbol(companyId, invoiceTemplateId);
            List<string> symbols = GetSymbolsInReportCancellingInvoice(companyId, invoiceTemplateId);
            if ((bool)isCreate)
                return notificationUseInvoiceDetails.Where(p => !symbols.Contains(p.CODE)).Select(p => p.CODE).Distinct().ToList();
            return notificationUseInvoiceDetails.Select(p => p.CODE).Distinct().ToList();
        }

        public decimal GetNumberInvoiceRegisterOfAll()
        {
            decimal numberInvoiceRegister = 0M;
            var noticeUseInvoiceDetail = this.invoiceUseDetailRepository.GetAllNoticeUseInvoiceDetail();
            noticeUseInvoiceDetail.ForEach(p =>
            {
                numberInvoiceRegister = numberInvoiceRegister + (p.NUMBERUSE ?? 0);
            });

            return numberInvoiceRegister;
        }

        public decimal GetNumberInvoiceRegisterOfCompany(long companyId)
        {
            decimal numberInvoiceRegister = 0M;
            var noticeUseInvoiceDetail = this.invoiceUseDetailRepository.GetNoticeUseInvoiceDetailOfCompany(companyId);
            noticeUseInvoiceDetail.ForEach(p =>
            {
                numberInvoiceRegister = numberInvoiceRegister + (p.NUMBERUSE ?? 0);
            });

            return numberInvoiceRegister;
        }

        public bool UsedByInvoices(long NotiId)
        {
            return this.invoiceUseDetailRepository.UsedByInvoices(NotiId);
        }

        public IEnumerable<RegisterTemplateCancelling> FilterInvoiceSymbolUse(long companyId, long invoiceSampleId, long reportCancellingId)
        {
            List<RegisterTemplateCancelling> resgiterTemplaterReturn = new List<RegisterTemplateCancelling>();
            var registerTemplatesCancelling = this.invoiceUseDetailRepository.FilterNotificationUseInvoiceDetailsUse(companyId, invoiceSampleId).OrderByDescending(p => p.NumberTo).ToList();
            List<RegisterTempleteUse> templateUseDistinct = registerTemplatesCancelling.Select(p => new RegisterTempleteUse(p.RegisterTemplatesId, p.Symbol))
                                                          .GroupBy(p => new { p.Id, p.Code })
                                                          .Select(g => g.First())
                                                          .ToList();
            foreach (var item in templateUseDistinct)
            {
                bool isExistedReportCancelling = this.reportCancellingDetailRepository.ContainRegisterTemplate(item.Id, item.Code, reportCancellingId);
                if (isExistedReportCancelling)
                {
                    continue;
                }

                RegisterTemplateCancelling register = registerTemplatesCancelling.FirstOrDefault(p => p.RegisterTemplatesId == item.Id && p.Symbol.Equals(item.Code));
                string numberFrom = GetNextInvoice(companyId, item.Id, item.Code.Replace("/", ""));
                if (numberFrom.IsNullOrEmpty())
                {
                    register.NumberFrom = registerTemplatesCancelling.Where(p => p.RegisterTemplatesId == item.Id && p.Symbol.Equals(item.Code)).Min(p => p.NumberFrom);
                }
                else
                {
                    register.NumberFrom = numberFrom;
                }

                resgiterTemplaterReturn.Add(register);
            }

            return resgiterTemplaterReturn;
        }

        #region Private Methods

        private NOTIFICATIONUSEINVOICEDETAIL GetInvoiceUseDetail(long id)
        {
            NOTIFICATIONUSEINVOICEDETAIL invoiceUse = this.invoiceUseDetailRepository.GetById(id);
            if (invoiceUse == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return invoiceUse;
        }
        private string GetNextInvoice(long companyId, long templateid, string symbol)
        {
            lock (lockObject)
            {
                double maxInvoiceNo = 0;
                var maxInvoice = GetMaxInvoiceNo(companyId, templateid, symbol);
                if (maxInvoice.IsNullOrEmpty())
                {
                    return string.Empty;
                }

                double.TryParse(maxInvoice, out maxInvoiceNo);
                maxInvoiceNo = maxInvoiceNo + 1;
                return maxInvoiceNo.ToString().PadLeft(maxInvoice.Length, '0');
            }
        }

        private string GetMaxInvoiceNo(long companyId, long templateid, string symbol)
        {
            var invoicesNo = this.invoiceRepository.GetMaxInvoiceNo(companyId, templateid, symbol);
            string maxInvoice = string.Empty;
            if (invoicesNo != null && invoicesNo.IsNotNullOrEmpty())
            {
                maxInvoice = invoicesNo;
            }

            return maxInvoice;
        }

        private List<string> GetSymbolsInReportCancellingInvoice(long companyId, long invoiceTemplateId)
        {
            IEnumerable<REPORTCANCELLINGDETAIL> reportCancelling = this.reportCancellingDetailRepository.Filter(companyId, invoiceTemplateId);
            return reportCancelling.Select(p => p.SYMBOL).ToList();

        }
        #endregion

    }
}