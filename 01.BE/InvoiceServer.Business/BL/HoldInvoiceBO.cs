using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class HoldInvoiceBO : IHoldInvoiceBO
    {
        private readonly IHoldInvoiceRepository holdInvoiceRepository;
        private readonly IHoldInvoiceDetailRepository holdInvoiceDetailRepository;
        private readonly UserSessionInfo currentUser;
        private readonly IDbTransactionManager transaction;

        public HoldInvoiceBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.holdInvoiceRepository = repoFactory.GetRepository<IHoldInvoiceRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.holdInvoiceDetailRepository = repoFactory.GetRepository<IHoldInvoiceDetailRepository>();
        }

        public HoldInvoiceBO(IRepositoryFactory repoFactory, UserSessionInfo userSessionInfo)
            : this(repoFactory)
        {
            this.currentUser = userSessionInfo;
        }

        public IEnumerable<HoldInvoiceViewModel> Filter(ConditionSearchHoldInvoice condition, int skip = 0, int take = Int32.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var holdInvoices = this.holdInvoiceRepository.Filter(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();
            var res = holdInvoices.Select(h => new HoldInvoiceViewModel(h)).ToList();

            return res;
        }

        public long Count(ConditionSearchHoldInvoice condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.holdInvoiceRepository.Filter(condition).Count();
        }

        public ResultCode Create(HoldInvoiceViewModel holdInvoiceViewModel)
        {
            var holdIvoice = new HOLDINVOICE()
            {
                COMPANYID = holdInvoiceViewModel.CompanyId,
                CODE = holdInvoiceViewModel.Code,
                REGISTERTEMPLATEID = holdInvoiceViewModel.RegisterTemplateId,
                SYMBOL = holdInvoiceViewModel.Symbol,
                INVOICEDATE = holdInvoiceViewModel.InvoiceDate,
                QUANTITY = holdInvoiceViewModel.Quantity,
                NUMBERFROM = holdInvoiceViewModel.NumberFrom,
                NUMBERTO = holdInvoiceViewModel.NumberTo,
                CREATEDBY = currentUser.Id,
                CREATEDDATE = DateTime.Now,
                HOLDINVOICEDETAILs = holdInvoiceViewModel.HoldInvoiceDetails.Select(x => new HOLDINVOICEDETAIL()
                {
                    INVOICENO = x.InvoiceNo,
                    INVOICESTATUS = x.InvoiceStatus
                }).ToList(),
            };
            var isCreateSuccess = holdInvoiceRepository.Insert(holdIvoice);
            if (isCreateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.UnknownError;
        }

        public HoldInvoiceViewModel GetById(long id)
        {
            var res = new HoldInvoiceViewModel();
            var holdInvoice = holdInvoiceRepository.GetById(id, currentUser.Company.Id ?? 0);
            if (holdInvoice != null)
            {
                res = new HoldInvoiceViewModel(holdInvoice);
                foreach (var holdInvoiceDetail in holdInvoice.HOLDINVOICEDETAILs)
                {
                    res.HoldInvoiceDetails.Add(new HoldInvoiceDetailViewModel(holdInvoiceDetail));
                }
            }
            return res;
        }

        public ResultCode Update(HoldInvoiceViewModel holdInvoiceViewModel)
        {
            var holdInvoice = this.holdInvoiceRepository.GetById(holdInvoiceViewModel.Id, this.currentUser.Company.Id ?? 0);

            holdInvoice.CODE = holdInvoiceViewModel.Code;
            holdInvoice.REGISTERTEMPLATEID = holdInvoiceViewModel.RegisterTemplateId;
            holdInvoice.SYMBOL = holdInvoiceViewModel.Symbol;
            holdInvoice.INVOICEDATE = holdInvoiceViewModel.InvoiceDate;
            holdInvoice.QUANTITY = holdInvoiceViewModel.Quantity;
            holdInvoice.NUMBERFROM = holdInvoiceViewModel.NumberFrom;
            holdInvoice.NUMBERTO = holdInvoiceViewModel.NumberTo;
            holdInvoice.UPDATEDBY = this.currentUser.Id;
            holdInvoice.UPDATEDDATE = DateTime.Now;

            try
            {
                transaction.BeginTransaction();
                var holdInvoiceDetails = holdInvoice.HOLDINVOICEDETAILs.ToList();
                foreach (var holdInvoiceDetail in holdInvoiceDetails)
                {
                    this.holdInvoiceDetailRepository.Delete(holdInvoiceDetail);
                }
                holdInvoice.HOLDINVOICEDETAILs = null;
                this.holdInvoiceRepository.Update(holdInvoice);
                foreach (var holdInvoiceDetailVM in holdInvoiceViewModel.HoldInvoiceDetails)
                {
                    this.holdInvoiceDetailRepository.Insert(new HOLDINVOICEDETAIL()
                    {
                        HOLDINVOICEID = holdInvoice.ID,
                        INVOICENO = holdInvoiceDetailVM.InvoiceNo,
                        INVOICESTATUS = holdInvoiceDetailVM.InvoiceStatus,
                    });
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }

            return ResultCode.NoError;
        }

        public ResultCode Delete(long id)
        {
            var holdInvoice = this.holdInvoiceRepository.GetById(id, this.currentUser.Company.Id ?? 0);

            try
            {
                transaction.BeginTransaction();
                var holdInvoiceDetails = holdInvoice.HOLDINVOICEDETAILs.ToList();
                foreach (var holdInvoiceDetail in holdInvoiceDetails)
                {
                    this.holdInvoiceDetailRepository.Delete(holdInvoiceDetail);
                }

                this.holdInvoiceRepository.Delete(holdInvoice);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }

            return ResultCode.NoError;
        }
    }
}