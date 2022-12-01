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
    public class InvoiceSampleBO : IInvoiceSampleBO
    {
        #region Fields, Properties

        private readonly IInvoiceSampleRepository invoiceSampleRepository;
        private readonly IInvoiceTemplateSampleRepository invoiceTemplateSampleRepository;
        private readonly IInvoiceTypeRepository invoiceTypeRepository;

        private readonly UserSessionInfo currentUser;

        #endregion

        #region Contructor

        public InvoiceSampleBO(IRepositoryFactory repoFactory, UserSessionInfo _currentUser)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.invoiceSampleRepository = repoFactory.GetRepository<IInvoiceSampleRepository>();
            this.invoiceTemplateSampleRepository = repoFactory.GetRepository<IInvoiceTemplateSampleRepository>();
            this.invoiceTypeRepository = repoFactory.GetRepository<IInvoiceTypeRepository>();

            this.currentUser = _currentUser;
        }

        #endregion

        #region Methods

        #endregion

        public IEnumerable<InvoiceSampleInfo> GetList()
        {
            var invoiceSamples = this.invoiceSampleRepository.GetList(currentUser);

            var invoiceSampleList = new List<InvoiceSampleInfo>();
            if (invoiceSamples.Any())
            {
                invoiceSamples.ForEach(x => invoiceSampleList.Add(
                    new InvoiceSampleInfo()
                    {
                        InvoiceTypeId = x.INVOICETYPEID ?? 0,
                        Code = x.CODE,
                        Denominator = x.DENOMINATOR,
                        FileName = x.FILENAME,
                        Name = x.NAME,
                        Note = x.NOTE,
                        InvoiceTypeName = invoiceTypeRepository.GetById(x.INVOICETYPEID ?? 0)?.NAME
                    }
                ));
            }

            return invoiceSampleList;
        }

        public InvoiceSampleInfo GetInvoiceSampleById(long invoiceSampleId)
        {
            var currentInvoiceSample = invoiceSampleRepository.GetInvoiceSampleById(invoiceSampleId);
            if (currentInvoiceSample != null)
            {
                return new InvoiceSampleInfo()
                {
                    Name = currentInvoiceSample.NAME,
                    Id = currentInvoiceSample.ID,
                    Code = currentInvoiceSample.CODE,
                    InvoiceTypeId = currentInvoiceSample.INVOICETYPEID ?? 0,
                    IsDiscount = currentInvoiceSample.ISDISCOUNT,
                    IsMultiTax = currentInvoiceSample.ISMULTITAX,
                    InvoiceTemplateSampleId = currentInvoiceSample.INVOICETEMPLATESAMPLEID,
                };
            }
            return null;
        }

        public InvoiceSampleInfo CreateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo)
        {
            bool create = true;
            var invoiceTemplateSample = this.invoiceTemplateSampleRepository.GetById(invoiceSampleInfo.InvoiceTemplateSampleId ?? 0);
            if (invoiceTemplateSample == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, "InvoiceTemplateSample not exists!");
            }

            if (IsExisted(invoiceSampleInfo.Code, invoiceSampleInfo.Name, create, this.currentUser))
            {
                throw new BusinessLogicException(ResultCode.ExistedInvoiceSample, string.Format("Template [{0}/{1}] is existed", invoiceSampleInfo.Code, invoiceSampleInfo.Name));
            }
            var invoiceSample = new INVOICESAMPLE()
            {
                NAME = invoiceSampleInfo.Name,
                CODE = invoiceSampleInfo.Code,
                NOTE = invoiceSampleInfo.Note,
                DENOMINATOR = invoiceSampleInfo.Denominator,
                INVOICETYPEID = invoiceSampleInfo.InvoiceTypeId,
                FILENAME = invoiceSampleInfo.FileName,
                ISMULTITAX = invoiceSampleInfo.IsMultiTax,
                ISDISCOUNT = invoiceSampleInfo.IsDiscount,
                TEMPLATEURL = invoiceSampleInfo.TemplateURL,
                DELETED = false,
                NAMEREPORT = invoiceSampleInfo.Name,
                CREATEDDATE = DateTime.Now,
                INVOICETEMPLATESAMPLEID = invoiceSampleInfo.InvoiceTemplateSampleId,
            };
            var currentInvoiceSample = invoiceSampleRepository.CreateInvoiceSample(this.currentUser, invoiceSample);
            return new InvoiceSampleInfo()
            {
                Id = currentInvoiceSample.ID,
                Name = currentInvoiceSample.NAME,
                Code = currentInvoiceSample.CODE,
                InvoiceTypeId = currentInvoiceSample.INVOICETYPEID ?? 0,
                IsMultiTax = currentInvoiceSample.ISMULTITAX,
                IsDiscount = currentInvoiceSample.ISDISCOUNT,
                InvoiceTemplateSampleId = currentInvoiceSample.INVOICETEMPLATESAMPLEID,
            };
        }

        public InvoiceSampleInfo UpdateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo)
        {
            bool create = false;
            if (IsExisted(invoiceSampleInfo.Code, invoiceSampleInfo.Name, create, this.currentUser))
            {
                throw new BusinessLogicException(ResultCode.ExistedInvoiceSample, string.Format("Template [{0}/{1}] is existed", invoiceSampleInfo.Code, invoiceSampleInfo.Name));
            }
            var invoiceSample = new INVOICESAMPLE()
            {
                NAME = invoiceSampleInfo.Name,
                CODE = invoiceSampleInfo.Code,
                NOTE = invoiceSampleInfo.Note,
                DENOMINATOR = invoiceSampleInfo.Denominator,
                INVOICETYPEID = invoiceSampleInfo.InvoiceTypeId,
                FILENAME = invoiceSampleInfo.FileName,
                ISMULTITAX = invoiceSampleInfo.IsMultiTax,
                ISDISCOUNT = invoiceSampleInfo.IsDiscount,
                TEMPLATEURL = invoiceSampleInfo.TemplateURL,
                DELETED = false,
                NAMEREPORT = invoiceSampleInfo.Name,
                INVOICETEMPLATESAMPLEID = invoiceSampleInfo.InvoiceTemplateSampleId,
            };
            var currentInvoiceSample = invoiceSampleRepository.CreateInvoiceSample(this.currentUser, invoiceSample);
            return new InvoiceSampleInfo()
            {
                Id = currentInvoiceSample.ID,
                Name = currentInvoiceSample.NAME,
                Code = currentInvoiceSample.CODE,
                InvoiceTypeId = currentInvoiceSample.INVOICETYPEID ?? 0,
                IsMultiTax = currentInvoiceSample.ISMULTITAX,
                IsDiscount = currentInvoiceSample.ISDISCOUNT,
            };
        }

        private bool IsExisted(string code, string name, bool create, UserSessionInfo currentUser)
        {
            return this.invoiceSampleRepository.ContainCode(code, name, create, currentUser);
        }

        public InvoiceSampleInfo Update(InvoiceSampleInfo info)
        {
            if (info.Id != null)
            {
                var updateInvoiceSampleModel = new INVOICESAMPLE()
                {
                    ID = (long)info.Id,
                    CODE = info.Code,
                    INVOICETYPEID = info.InvoiceTypeId,
                    ISMULTITAX = info.IsMultiTax,
                    ISDISCOUNT = info.IsDiscount
                };
                var currentInvoiceSample = invoiceSampleRepository.Update(this.currentUser, updateInvoiceSampleModel);
                return new InvoiceSampleInfo()
                {
                    Id = currentInvoiceSample.ID,
                    Name = currentInvoiceSample.NAME,
                    Code = currentInvoiceSample.CODE,
                    InvoiceTypeId = currentInvoiceSample.INVOICETYPEID ?? 0,
                    IsMultiTax = currentInvoiceSample.ISMULTITAX,
                    IsDiscount = currentInvoiceSample.ISDISCOUNT,
                };
            }
            return new InvoiceSampleInfo();
        }

        public ResultCode Update(long id, InvoiceSampleInfo invoiceSampleInfo)
        {
            var currentInvoiceSample = new INVOICESAMPLE()
            {
                NAME = invoiceSampleInfo.Name,
                CODE = invoiceSampleInfo.Code,
                NOTE = invoiceSampleInfo.Note,
                DENOMINATOR = invoiceSampleInfo.Denominator,
                INVOICETYPEID = invoiceSampleInfo.InvoiceTypeId,
                FILENAME = invoiceSampleInfo.FileName,
                ISDISCOUNT = invoiceSampleInfo.IsDiscount,
                ISMULTITAX = invoiceSampleInfo.IsMultiTax,
                TEMPLATEURL = invoiceSampleInfo.TemplateURL,
                DELETED = false,
                NAMEREPORT = invoiceSampleInfo.Name,
                CREATEDDATE = DateTime.Now,
                INVOICETEMPLATESAMPLEID = invoiceSampleInfo.InvoiceTemplateSampleId,
            };
            var result = invoiceSampleRepository.Update(this.currentUser, id, currentInvoiceSample);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.InvoiceIssuedNotUpdate;
        }

        public InvoiceSampleInfo GetByCode(string code)
        {
            var currentInvoiceSample = invoiceSampleRepository.GetByCode(code, currentUser);
            if (currentInvoiceSample != null)
            {
                return new InvoiceSampleInfo()
                {
                    Id = currentInvoiceSample.ID,
                    Name = currentInvoiceSample.NAME,
                    Code = currentInvoiceSample.CODE,
                    InvoiceTypeId = currentInvoiceSample.INVOICETYPEID ?? 0,
                    Note = currentInvoiceSample.NOTE,
                    Denominator = currentInvoiceSample.DENOMINATOR,
                    FileName = currentInvoiceSample.FILENAME,
                    IsDiscount = currentInvoiceSample.ISDISCOUNT,
                    IsMultiTax = currentInvoiceSample.ISMULTITAX,
                    TemplateURL = currentInvoiceSample.TEMPLATEURL,

                };
            }

            return new InvoiceSampleInfo();
        }

        public List<Data.DBAccessor.INVOICESAMPLE> GetAll() 
        {
            return  invoiceSampleRepository.GetAll().ToList();
        }
        public InvoiceSampleInfo GetById(long id)
        {
            var invoiceSample = invoiceSampleRepository.GetInvoiceSampleById(id);
            if (invoiceSample != null)
            {
                return new InvoiceSampleInfo()
                {
                    Id = invoiceSample.ID,
                    Name = invoiceSample.NAME,
                    Code = invoiceSample.CODE,
                    InvoiceTypeId = invoiceSample.INVOICETYPEID ?? 0,
                    Note = invoiceSample.NOTE,
                    Denominator = invoiceSample.DENOMINATOR,
                    FileName = invoiceSample.FILENAME,
                    IsDiscount = invoiceSample.ISDISCOUNT,
                    IsMultiTax = invoiceSample.ISMULTITAX,
                    TemplateURL = invoiceSample.TEMPLATEURL,
                    InvoiceTemplateSampleId = invoiceSample.INVOICETEMPLATESAMPLEID,
                    RegisterTemplateCount = invoiceSample.REGISTERTEMPLATES.Where(p => !(p.DELETED ?? false)).ToList().Count,
                };
            }

            return new InvoiceSampleInfo();
        }

        public ResultCode Delete(string code)
        {
            var result = invoiceSampleRepository.DeleteInvoiceSample(code, currentUser);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.InvoiceIssuedNotUpdate;
        }

        public ResultCode DeleteById(long id)
        {
            var result = invoiceSampleRepository.DeleteInvoiceSampleById(id);
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.InvoiceIssuedNotUpdate;
        }

        public IEnumerable<InvoiceSampleInfo> Filter(ConditionSearchInvoiceSample condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var listInvoiceSample = this.invoiceSampleRepository.Filter(currentUser, condition).AsQueryable()
                .OrderBy(condition.ColumnOrder1, condition.OrderType.Equals(OrderTypeConst.Desc)).ThenBy(condition.ColumnOrder2, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take)
                .ToList();
            var invoiceSampleViewModel = new List<InvoiceSampleInfo>();
            if (listInvoiceSample.Any())
            {
                listInvoiceSample.ForEach(x => invoiceSampleViewModel.Add(
                    new InvoiceSampleInfo()
                    {
                        Id = x.ID,
                        InvoiceTypeId = x.INVOICETYPEID ?? 0,
                        Code = x.CODE,
                        Denominator = x.DENOMINATOR,
                        FileName = x.FILENAME,
                        Name = x.NAME,
                        Note = x.NOTE,
                        InvoiceTypeName = invoiceTypeRepository.GetById(x.INVOICETYPEID ?? 0)?.NAME,
                        InvoiceTemplateSampleId = x.INVOICETEMPLATESAMPLEID,
                        RegisterTemplateCount = x.REGISTERTEMPLATES.Where(p => !(p.DELETED ?? false)).ToList().Count
                    }
                ));
            }

            return invoiceSampleViewModel;
        }

        public long Count(ConditionSearchInvoiceSample condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.invoiceSampleRepository.Filter(currentUser, condition).Count();
        }

        public IEnumerable<InvoiceSampleInfo> GetByInvoiceTypeId(long invoiceTypeId)
        {
            var listReturn = new List<InvoiceSampleInfo>();
            var listGetByInvoiceTypeId =
                invoiceSampleRepository.GetByInvoiceTypeId(this.currentUser, invoiceTypeId);
            foreach (var invoiceSample in listGetByInvoiceTypeId)
            {
                listReturn.Add(new InvoiceSampleInfo()
                {
                    Id = invoiceSample.ID,
                    Name = invoiceSample.NAME,
                    Code = invoiceSample.CODE,
                    InvoiceTypeId = invoiceSample.INVOICETYPEID ?? 0,
                    Note = invoiceSample.NOTE,
                    Denominator = invoiceSample.DENOMINATOR,
                    FileName = invoiceSample.FILENAME,
                    IsDiscount = invoiceSample.ISDISCOUNT,
                    IsMultiTax = invoiceSample.ISMULTITAX,
                    TemplateURL = invoiceSample.TEMPLATEURL,
                    InvoiceTemplateSampleId = invoiceSample.INVOICETEMPLATESAMPLEID
                });
            }
            return listReturn;
        }

        public bool MyInvoiceSampleUsing(long invoiceID)
        {
            return invoiceSampleRepository.MyInvoiceSampleUsing(invoiceID);
        }
    }
}