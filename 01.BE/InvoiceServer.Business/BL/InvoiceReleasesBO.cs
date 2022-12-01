using InvoiceServer.Business.DAO;
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
    public class InvoiceReleasesBO : IInvoiceReleasesBO
    {
        #region Fields, Properties

        private readonly IInvoiceConcludeRepository invoiceConcludeRepository;

        private readonly IInvoiceConcludeDetailRepository invoiceReleaseDetailRepository;
        private readonly IRegisterTemplatesRepository registerTemplatesRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly IInvoiceReleasesTemplateRepository releaseTemplateRepository;
        private readonly INotificationUseInvoiceDetailRepository notificationUseInvoiceDetailRepository;
        private readonly INotificationUseInvoiceRepository notificationUseInvoiceRepository;
        private readonly UserSessionInfo currentUser;
        private readonly ICityRepository cityRepository;
        #endregion

        #region Contructor

        public InvoiceReleasesBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.invoiceConcludeRepository = repoFactory.GetRepository<IInvoiceConcludeRepository>();
            this.invoiceReleaseDetailRepository = repoFactory.GetRepository<IInvoiceConcludeDetailRepository>();
            this.registerTemplatesRepository = repoFactory.GetRepository<IRegisterTemplatesRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.releaseTemplateRepository = repoFactory.GetRepository<IInvoiceReleasesTemplateRepository>();
            this.notificationUseInvoiceDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.notificationUseInvoiceRepository = repoFactory.GetRepository<INotificationUseInvoiceRepository>();
            this.cityRepository = repoFactory.GetRepository<ICityRepository>();
        }
        public InvoiceReleasesBO(IRepositoryFactory repoFactory, UserSessionInfo userSessionInfo)
            : this(repoFactory)
        {
            this.currentUser = userSessionInfo;
        }
        #endregion

        #region Methods

        public IEnumerable<InvoiceReleasesMaster> Filter(ConditionSearchInvoiceReleases condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoiceReleases = this.invoiceConcludeRepository.FilterInvoiceConclude(condition).ToList();
            return invoiceReleases;
        }

        public long Count(ConditionSearchInvoiceReleases condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.invoiceConcludeRepository.CountInvoiceConclude(condition);
        }

        public ResultCode Create(InvoiceReleasesInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                INVOICERELEAS invoiceRelease = InsertInvoiRelease(info);
                InsertInvoiceReleaseDetail(invoiceRelease.ID, info.InvoiceReleasesDetailInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public ResultCode Update(long id, long? companyId, InvoiceReleasesInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                UpdateInvoiRelease(id, companyId, info);
                DeleteInvoiceReleasesDetail(id);
                InsertInvoiceReleaseDetail(id, info.InvoiceReleasesDetailInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public InvoiceReleasesInfo GetInvoiceReleasesInfo(long id, long? companyId, bool isView = false)
        {
            INVOICERELEAS currentInvoiceRelease = GetInvoiceRelease(id, companyId, isView);
            if (currentInvoiceRelease.STATUS == (int)RecordStatus.Approved)
            {
                currentInvoiceRelease.MYCOMPANY.COMPANYNAME = currentInvoiceRelease.COMPANYNAME;
            }
            if (currentInvoiceRelease.STATUS == (int)RecordStatus.Created)
            {
                var city = this.cityRepository.GetById(currentInvoiceRelease.CITYID ?? 0);
                var company = this.myCompanyRepository.GetById(currentInvoiceRelease.COMPANYID ?? 0);
                currentInvoiceRelease.CITYNAME = city.NAME;
                currentInvoiceRelease.COMPANYNAME = company.COMPANYNAME;
                currentInvoiceRelease.TAXCODE = company.TAXCODE;
            }
            var invoiceReleasesDetails = currentInvoiceRelease.INVOICERELEASESDETAILs.Where(p => !(p.DELETED ?? false));
            List<InvoiceReleasesDetailInfo> invoiceConcludeDetails = invoiceReleasesDetails.Select(p => new InvoiceReleasesDetailInfo(p)
            {
                InvoiceTypeName = currentInvoiceRelease.STATUS == (int)RecordStatus.Created ? p.REGISTERTEMPLATE.INVOICESAMPLE.INVOICETYPE.NAME : p.INVOICETYPENAME,
                Code = currentInvoiceRelease.STATUS == (int)RecordStatus.Created ? p.REGISTERTEMPLATE.CODE : p.REGISTERTEMPLATECODE,
                InvoiceSampleName = currentInvoiceRelease.STATUS == (int)RecordStatus.Created ? p.REGISTERTEMPLATE.INVOICESAMPLE.NAME : p.INVOICESAMPLENAME,
            }).ToList();
            return new InvoiceReleasesInfo(currentInvoiceRelease, invoiceConcludeDetails);
        }

        public ResultCode Delete(long id, long? companyId)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteInvoiceReleases(id, companyId);
                DeleteInvoiceReleasesDetail(id);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public InvoiceReleasesTemplateInfo GetTemplateReleaseTemplate()
        {
            var releaseTemplate = this.releaseTemplateRepository.GetTemplateReleaseInvoice();
            return new InvoiceReleasesTemplateInfo(releaseTemplate);
        }

        #endregion

        #region Private Methods
        private INVOICERELEAS InsertInvoiRelease(InvoiceReleasesInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var city = this.cityRepository.GetById(info.CityId ?? 0);
            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);

            INVOICERELEAS invoiceRelease = new INVOICERELEAS();
            invoiceRelease.CopyData(info);
            invoiceRelease.CITYNAME = city?.NAME;
            invoiceRelease.STATUS = (int)RecordStatus.Created;
            invoiceRelease.CREATEDDATE = DateTime.Now;
            invoiceRelease.CREATEDBY = this.currentUser.Id;
            invoiceRelease.COMPANYNAME = company.COMPANYNAME;
            invoiceRelease.TAXCODE = company.TAXCODE;
            this.invoiceConcludeRepository.Insert(invoiceRelease);
            return invoiceRelease;
        }

        private void UpdateInvoiRelease(long id, long? companyId, InvoiceReleasesInfo info)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }
            var city = this.cityRepository.GetById(info.CityId ?? 0);
            var company = this.myCompanyRepository.GetById(info.CompanyId ?? 0);

            INVOICERELEAS invoiceRelease = GetInvoiceRelease(id, companyId);

            invoiceRelease.CopyData(info);
            invoiceRelease.UPDATEDDATE = DateTime.Now;
            invoiceRelease.UPDATEDBY = info.UserActionId;
            invoiceRelease.CITYNAME = city?.NAME;
            invoiceRelease.COMPANYNAME = company.COMPANYNAME;
            invoiceRelease.TAXCODE = company.TAXCODE;
            this.invoiceConcludeRepository.Update(invoiceRelease);
        }

        private void InsertInvoiceReleaseDetail(long invoiceReleaseId, List<InvoiceReleasesDetailInfo> invoiceReleaseDetailInfo)
        {
            ResultCode errorCode;
            string errorMessage;
            invoiceReleaseDetailInfo.ForEach(p =>
            {
                if (!p.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }

                if (!p.RegisterTemplatesId.HasValue)
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, "Column [invoiceTemplateId] Is not null");
                }

                var registerTemplate = this.registerTemplatesRepository.GetById(p.RegisterTemplatesId ?? 0);
                var invoiceSample = registerTemplate.INVOICESAMPLE;
                var invoiceType = registerTemplate.INVOICESAMPLE.INVOICETYPE;

                REGISTERTEMPLATE templateUse = GetTemplateCompanyUse(p.RegisterTemplatesId.Value);
                INVOICERELEASESDETAIL invoiceReleaseDetail = new INVOICERELEASESDETAIL();
                invoiceReleaseDetail.CopyData(p);
                invoiceReleaseDetail.REGISTERTEMPLATESID = templateUse.ID;
                invoiceReleaseDetail.INVOICECONCLUDEID = invoiceReleaseId;
                invoiceReleaseDetail.INVOICESAMPLEID = templateUse.INVOICESAMPLE.ID;
                invoiceReleaseDetail.INVOICETYPENAME = invoiceType.NAME;
                invoiceReleaseDetail.REGISTERTEMPLATECODE = registerTemplate.CODE;
                invoiceReleaseDetail.INVOICESAMPLENAME = invoiceSample.NAME;

                this.invoiceReleaseDetailRepository.Insert(invoiceReleaseDetail);
            });
        }

        private IEnumerable<INVOICERELEASESDETAIL> GetListInvoiceConcludeDetail(long invoiceConcludeId)
        {
            return this.invoiceReleaseDetailRepository.FilterInvoiceConcludeDetail(invoiceConcludeId);
        }

        private INVOICERELEAS GetInvoiceRelease(long id, long? companyId, bool isview = false)
        {
            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            INVOICERELEAS invoiceConclude = GetInvoiceRelease(id);
            var tamp = myCompanyRepository.GetHO();
            if (invoiceConclude.COMPANYID != companyId && tamp.COMPANYSID != companyId && !isview)
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData, ClientManagementInfo.NotPermissionData);
            }

            return invoiceConclude;
        }

        private INVOICERELEAS GetInvoiceRelease(long id)
        {
            INVOICERELEAS invoiceConclude = this.invoiceConcludeRepository.GetById(id);
            if (invoiceConclude == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }
            return invoiceConclude;
        }

        private void DeleteInvoiceReleases(long id, long? companyId)
        {
            var invoiceRelease = GetInvoiceRelease(id, companyId);
            if (invoiceRelease.STATUS.HasValue && invoiceRelease.STATUS.Value == (int)RecordStatus.Approved)
            {
                throw new BusinessLogicException(ResultCode.ReleaseInvoiceApprovedNotUpdate, "Notify the approved bill is not delete");
            }
            invoiceRelease.DELETED = true;
            invoiceRelease.UPDATEDDATE = DateTime.Now;
            this.invoiceConcludeRepository.Update(invoiceRelease);
        }

        private void DeleteInvoiceReleasesDetail(long invoiceConclude)
        {
            var invoiceConcludeDetails = this.GetListInvoiceConcludeDetail(invoiceConclude);
            invoiceConcludeDetails.ForEach(p =>
            {
                p.DELETED = true;
                this.invoiceReleaseDetailRepository.Update(p);
            });
        }

        public ResultCode UpdateStatus(long id, long? companyId, InvoiceReleasesStatus status, bool isUnApproved = false)
        {
            INVOICERELEAS currentInvoiceRelease = this.GetInvoiceRelease(id, companyId);
            if (isUnApproved)
            {
                var notificationDetail = this.notificationUseInvoiceDetailRepository.GetByRelease(currentInvoiceRelease.ID);
                foreach (var item in notificationDetail)
                {
                    var notification = this.notificationUseInvoiceRepository.GetById(item.NotificationId ?? 0);
                    if (notification.STATUS == (int)RecordStatus.Approved)
                    {
                        throw new BusinessLogicException(ResultCode.ReleaseInvoiceApprovedNotUnUpdate, "Notify the approved bill is not editable");
                    }
                }
            }
            else
            {
                currentInvoiceRelease.COMPANYNAME = this.currentUser.Company.CompanyName;
            }
            if (currentInvoiceRelease.STATUS.HasValue && currentInvoiceRelease.STATUS.Value == (int)RecordStatus.Approved && !isUnApproved)
            {
                throw new BusinessLogicException(ResultCode.ReleaseInvoiceApprovedNotUpdate, "Notify the approved bill is not editable");
            }

            var city = this.cityRepository.GetById(currentInvoiceRelease.CITYID ?? 0);
            var company = this.myCompanyRepository.GetById(currentInvoiceRelease.COMPANYID ?? 0);

            currentInvoiceRelease.STATUS = status.Status;
            currentInvoiceRelease.UPDATEDBY = status.ActionUserId;
            currentInvoiceRelease.UPDATEDDATE = DateTime.Now;

            if (status.Status == (int)RecordStatus.Approved)
            {
                currentInvoiceRelease.CITYNAME = city.NAME;
                currentInvoiceRelease.COMPANYNAME = company.COMPANYNAME;
                currentInvoiceRelease.TAXCODE = company.TAXCODE;

                foreach (var invoiceReleaseDetail in currentInvoiceRelease.INVOICERELEASESDETAILs)
                {
                    var registerTemplate = invoiceReleaseDetail.REGISTERTEMPLATE;
                    var invoiceSample = registerTemplate.INVOICESAMPLE;
                    var invoiceType = registerTemplate.INVOICESAMPLE.INVOICETYPE;

                    invoiceReleaseDetail.INVOICETYPENAME = invoiceType.NAME;
                    invoiceReleaseDetail.REGISTERTEMPLATECODE = registerTemplate.CODE;
                    invoiceReleaseDetail.INVOICESAMPLENAME = invoiceSample.NAME;
                }
            }

            return this.invoiceConcludeRepository.Update(currentInvoiceRelease) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private REGISTERTEMPLATE GetTemplateCompanyUse(long id)
        {
            REGISTERTEMPLATE templateCompanyUse = this.registerTemplatesRepository.GetById(id);
            if (templateCompanyUse == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This invoiceSampleId [{0}] not found in data of client", id));
            }

            return templateCompanyUse;
        }

        #endregion     
    }
}