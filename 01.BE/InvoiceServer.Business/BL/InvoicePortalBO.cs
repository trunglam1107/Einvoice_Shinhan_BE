using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvoiceServer.Business.BL
{
    public class InvoicePortalBO : IInvoicePortalBO
    {
        private readonly IInvoiceRepository invoiceRepository;
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IReleaseInvoiceRepository releaseInvoiceRepository;
        private readonly IReleaseAnnouncementRepository releaseAnnouncementRepository;
        private readonly IClientRepository clientRepository;
        private readonly INotificationUseInvoiceDetailRepository notificationUseInvoiceDetailRepository;
        private readonly IDbTransactionManager transaction;
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly PrintConfig config;
        private readonly Account account;

        public InvoicePortalBO(IRepositoryFactory repoFactory)
        {
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.releaseInvoiceRepository = repoFactory.GetRepository<IReleaseInvoiceRepository>();
            this.releaseAnnouncementRepository = repoFactory.GetRepository<IReleaseAnnouncementRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.notificationUseInvoiceDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.account = new Account(repoFactory);
        }
        public InvoicePortalBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.releaseInvoiceRepository = repoFactory.GetRepository<IReleaseInvoiceRepository>();
            this.releaseAnnouncementRepository = repoFactory.GetRepository<IReleaseAnnouncementRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.notificationUseInvoiceDetailRepository = repoFactory.GetRepository<INotificationUseInvoiceDetailRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.account = new Account(repoFactory);
            this.config = config;
        }

        public IEnumerable GetInvoiceByVeriCode(string VerificationCode)
        {
            if (VerificationCode.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoice = SearchInvoiceByVeriCode(VerificationCode);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by verification code {0}", VerificationCode));
            }
            return invoice;
        }

        IEnumerable SearchInvoiceByVeriCode(string verificationCode)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var researchInvoice = from INVOICE in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on INVOICE.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                  join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                    on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on REGISTERTEMPLATE.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  //join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join CURRENCY in this._dbSet.CURRENCies on INVOICE.CURRENCYID equals CURRENCY.ID
                                  where INVOICE.VERIFICATIONCODE.Equals(verificationCode)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = INVOICE.ID,
                                      CompanyID = INVOICE.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                      CustomerName = INVOICE.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = INVOICE.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = INVOICE.DELETED,
                                      Created = INVOICE.CREATEDDATE,
                                      CreatedBy = INVOICE.CREATEDBY,
                                      Updated = INVOICE.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = REGISTERTEMPLATE.CODE,
                                      Symbol = INVOICE.SYMBOL,
                                      InvoiceNo = INVOICE.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = INVOICE.CLIENTID,
                                      VerificationCode = INVOICE.VERIFICATIONCODE,
                                      ReleasedDate = INVOICE.RELEASEDDATE,
                                      Sum = INVOICE.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = INVOICE.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = INVOICE.COMPANYTAXCODE, //Cột MST người bán,
                                      //UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi),
                                      CurrencyCode = CURRENCY.CODE,
                                      InvoiceType = (INVOICE.INVOICESTATUS == 4 && INVOICE.PARENTID == null) ? 0 : (INVOICE.INVOICESTATUS == 4 && INVOICE.PARENTID != null) ? 1 : 2, //0 : Đã ký, 1: Điều chỉnh, 2: Hủy
                                  };
            return researchInvoice.Distinct();
        }

        public IEnumerable GetInvoiceByVeriCodeAndClientId(string VerificationCode, long ClientId, string DateFrom, string DateTo)
        {
            if (VerificationCode.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var invoice = SearchInvoiceByVeriCodeAndClientId(VerificationCode, ClientId, DateFrom, DateTo);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by verification code {0}", VerificationCode));
            }
            return invoice;
        }

        IEnumerable SearchInvoiceByVeriCodeAndClientId(string verificationCode, long ClientId, string DateFrom, string DateTo)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            var researchInvoice = from INVOICE in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on INVOICE.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                  join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                    on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on REGISTERTEMPLATE.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join CURRENCY in this._dbSet.CURRENCies on INVOICE.CURRENCYID equals CURRENCY.ID
                                  where (!String.IsNullOrEmpty(verificationCode) && INVOICE.VERIFICATIONCODE.Equals(verificationCode)) && client.ID == ClientId
                                        && INVOICE.RELEASEDDATE >= dateFrom && INVOICE.RELEASEDDATE <= dateTo
                                        && INVOICE.INVOICESTATUS == (int)InvoiceStatus.Released
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = INVOICE.ID,
                                      CompanyID = INVOICE.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                      CustomerName = INVOICE.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = INVOICE.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = INVOICE.DELETED,
                                      Created = INVOICE.CREATEDDATE,
                                      CreatedBy = INVOICE.CREATEDBY,
                                      Updated = INVOICE.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = REGISTERTEMPLATE.CODE,
                                      Symbol = INVOICE.SYMBOL,
                                      InvoiceNo = INVOICE.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = INVOICE.CLIENTID,
                                      VerificationCode = INVOICE.VERIFICATIONCODE,
                                      ReleasedDate = INVOICE.RELEASEDDATE,
                                      Sum = INVOICE.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = INVOICE.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = INVOICE.COMPANYTAXCODE, //Cột MST người bán,
                                      UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi)
                                      CurrencyCode = CURRENCY.CODE,
                                      InvoiceType = (INVOICE.INVOICESTATUS == 4 && INVOICE.PARENTID == null) ? 0 : (INVOICE.INVOICESTATUS == 4 && INVOICE.PARENTID != null) ? 1 : 2, //0 : Đã ký, 1: Điều chỉnh, 2: Hủy
                                      InvoiceNo2 = INVOICE.INVOICENO
                                  };
            return researchInvoice.Distinct().OrderByDescending(x => x.InvoiceNo2);
        }
        public IQueryable<PTInvoice> GetBYInvoiceByVeriCodeAndClientId(string verificationCode, long ClientId, string DateFrom, string DateTo)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            var researchInvoice = from INVOICE in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on INVOICE.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                  join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                    on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on REGISTERTEMPLATE.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join CURRENCY in this._dbSet.CURRENCies on INVOICE.CURRENCYID equals CURRENCY.ID
                                  where (!String.IsNullOrEmpty(verificationCode) && INVOICE.VERIFICATIONCODE.Equals(verificationCode)) && client.ID == ClientId
                                        && INVOICE.RELEASEDDATE >= dateFrom && INVOICE.RELEASEDDATE <= dateTo
                                        && (INVOICE.INVOICESTATUS == (int)InvoiceStatus.Released ||
                                        INVOICE.INVOICESTATUS == (int)InvoiceStatus.Cancel ||
                                        INVOICE.INVOICESTATUS == (int)InvoiceStatus.Delete)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = INVOICE.ID,
                                      CompanyID = INVOICE.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                      CustomerName = INVOICE.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = INVOICE.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = INVOICE.DELETED,
                                      Created = INVOICE.CREATEDDATE,
                                      CreatedBy = INVOICE.CREATEDBY,
                                      Updated = INVOICE.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = REGISTERTEMPLATE.CODE,
                                      Symbol = INVOICE.SYMBOL,
                                      InvoiceNo = INVOICE.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = INVOICE.CLIENTID,
                                      VerificationCode = INVOICE.VERIFICATIONCODE,
                                      ReleasedDate = INVOICE.RELEASEDDATE,
                                      Sum = INVOICE.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = INVOICE.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = INVOICE.COMPANYTAXCODE, //Cột MST người bán,
                                      UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi)
                                      CurrencyCode = CURRENCY.CODE,
                                      InvoiceType = INVOICE.INVOICESTATUS == 4 ? 0 : INVOICE.PARENTID != null ? 1 : 2, //Trạng thái hóa đơn : 0 : Đã ký, 1: Điều chỉnh, 2: Hủy
                                  };
            return researchInvoice.Distinct();

        }
        public string GetToken(string VerificationCode, long companyID)
        {
            PTUser ptus = GetUserClient(VerificationCode, companyID);
            var us = GetUser(ptus.UserName, ptus.Pass);
            return us == null ? "" : us.USERID;
        }

        private LOGINUSER GetUser(string userId, string Pass)
        {
            var loginUser = this.loginUserRepository.ClientLogin(userId, Pass);
            if (loginUser == null)
            {
                throw new BusinessLogicException(ResultCode.LoginUserIdNotExist,
                                   string.Format("User id does not exist: {0}", userId));
            }
            return loginUser;
        }

        private PTUser GetUserClient(string VerificationCode, long companyID)
        {
            var qry = from invoice in _dbSet.INVOICEs
                          //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                      join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                      join company in this._dbSet.MYCOMPANies on invoice.COMPANYID equals company.COMPANYSID
                      join template in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals template.ID into templateTemp
                      join login in this._dbSet.LOGINUSERs on client.ID equals login.CLIENTID into loginTemp
                      //from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                      from template in templateTemp.DefaultIfEmpty()
                      from login in loginTemp.DefaultIfEmpty()
                      where invoice.VERIFICATIONCODE.Equals(VerificationCode) && invoice.COMPANYID == companyID
                      select new PTUser
                      {
                          UserName = login.USERID,
                          Pass = login.PASSWORD,
                      };
            return qry.FirstOrDefault();
        }

        public IEnumerable SearchInvoiceByInvoiceNo(string InvoiceNo, string taxcode, string DateFrom, string DateTo)
        {
            if (InvoiceNo.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            //var invoice = SearchInvoiceByInvoiceNoClientID(InvoiceNo, clientId, DateFrom, DateTo);
            var invoice = SearchInvoiceByInvoiceNoByTaxCode(InvoiceNo, taxcode, DateFrom, DateTo);
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by InvoiceCode code {0}", InvoiceNo));
            }
            return invoice;
        }

        public IEnumerable SearchInvoiceByInvoiceNoByTaxCode(string InvoiceCode, string taxcode, string DateFrom, string DateTo)
        {
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var researchInvoice = from invoice in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on invoice.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                                  join registerTemplate in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on registerTemplate.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join currency in this._dbSet.CURRENCies on invoice.CURRENCYID equals currency.ID
                                  where invoice.CUSTOMERTAXCODE == taxcode && invoice.RELEASEDDATE != null
                                        && invoice.RELEASEDDATE >= dateFrom && invoice.RELEASEDDATE <= dateTo
                                        && (invoice.INVOICESTATUS == (int)InvoiceStatus.Released ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = invoice.ID,
                                      CompanyID = invoice.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                      CustomerName = invoice.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = invoice.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = invoice.DELETED,
                                      Created = invoice.RELEASEDDATE,
                                      CreatedBy = invoice.CREATEDBY,
                                      Updated = invoice.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = registerTemplate.CODE,
                                      Symbol = invoice.SYMBOL,
                                      InvoiceNo = invoice.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = invoice.CLIENTID,
                                      InvoiceStatus = invoice.INVOICESTATUS,
                                      ClientSign = invoice.CLIENTSIGN ?? false,
                                      VerificationCode = string.Empty,
                                      ReleasedDate = System.Data.Entity.DbFunctions.TruncateTime(invoice.RELEASEDDATE),
                                      Sum = invoice.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = invoice.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = invoice.COMPANYTAXCODE, //Cột MST người bán
                                      UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi)
                                      InvoiceNo2 = invoice.INVOICENO,
                                      CurrencyCode = currency.CODE,
                                      InvoiceType = (invoice.INVOICESTATUS == 4 && invoice.PARENTID == null) ? 0 : (invoice.INVOICESTATUS == 4 && invoice.PARENTID != null) ? 1 : 2, //0 : Đã ký, 1: Điều chỉnh, 2: Hủy
                                  };
            //var kq = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().ToList();

            //kq = kq.AsQueryable().OrderByDescending(x => x.ReleasedDate).ThenByDescending(x => x.InvoiceNo).ToList();
            //var kq = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().ToList();

            //var result = researchInvoice.OrderByDescending(x => x.ReleasedDate).ThenByDescending(x => x.InvoiceNo).ToList();
            //return result;
            //return kq;

            var result = researchInvoice;

            if (InvoiceCode != "0")
            {
                result = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }
            else
            {
                result = researchInvoice.OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }

            return result;
        }

        IEnumerable SearchInvoiceByInvoiceNoClientID(string InvoiceCode, long ClientID, string DateFrom, string DateTo)
        {
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var researchInvoice = from invoice in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on invoice.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                                  join registerTemplate in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on registerTemplate.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join currency in this._dbSet.CURRENCies on invoice.CURRENCYID equals currency.ID
                                  where invoice.CLIENTID == ClientID && invoice.RELEASEDDATE != null
                                        && invoice.RELEASEDDATE >= dateFrom && invoice.RELEASEDDATE <= dateTo
                                        && (invoice.INVOICESTATUS == (int)InvoiceStatus.Released ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = invoice.ID,
                                      CompanyID = invoice.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                      CustomerName = invoice.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = invoice.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = invoice.DELETED,
                                      Created = invoice.RELEASEDDATE,
                                      CreatedBy = invoice.CREATEDBY,
                                      Updated = invoice.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = registerTemplate.CODE,
                                      Symbol = invoice.SYMBOL,
                                      InvoiceNo = invoice.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = invoice.CLIENTID,
                                      InvoiceStatus = invoice.INVOICESTATUS,
                                      ClientSign = invoice.CLIENTSIGN ?? false,
                                      VerificationCode = string.Empty,
                                      ReleasedDate = System.Data.Entity.DbFunctions.TruncateTime(invoice.RELEASEDDATE),
                                      Sum = invoice.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = invoice.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = invoice.COMPANYTAXCODE, //Cột MST người bán
                                      UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi)
                                      InvoiceNo2 = invoice.INVOICENO,
                                      CurrencyCode = currency.CODE,
                                      InvoiceType = invoice.INVOICESTATUS == 4 ? 0 : invoice.PARENTID != null ? 1 : 2, //0 : Đã ký,1: Điều chỉnh, 2: Hủy
                                  };
            //var kq = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().ToList();

            //kq = kq.AsQueryable().OrderByDescending(x => x.ReleasedDate).ThenByDescending(x => x.InvoiceNo).ToList();
            //var kq = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().ToList();

            //var result = researchInvoice.OrderByDescending(x => x.ReleasedDate).ThenByDescending(x => x.InvoiceNo).ToList();
            //return result;
            //return kq;

            var result = researchInvoice;

            if (InvoiceCode != "0")
            {
                result = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }
            else
            {
                result = researchInvoice.OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }

            return result;
        }


        public IQueryable<PTInvoice> GetByInvoiceNoClientID(string InvoiceCode, long ClientID, string DateFrom, string DateTo)
        {
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var researchInvoice = from invoice in this._dbSet.INVOICEs
                                      //join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on invoice.ID equals releaseDetail.INVOICEID
                                  join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                                  join registerTemplate in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals registerTemplate.ID
                                  join ivtype in this._dbSet.INVOICESAMPLEs on registerTemplate.INVOICESAMPLEID equals ivtype.ID
                                  join invoiceType in this._dbSet.INVOICETYPEs on ivtype.INVOICETYPEID equals invoiceType.ID
                                  join loginuser in this._dbSet.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                  join currency in this._dbSet.CURRENCies on invoice.CURRENCYID equals currency.ID
                                  where invoice.CLIENTID == ClientID && invoice.RELEASEDDATE != null
                                        && invoice.RELEASEDDATE >= dateFrom && invoice.RELEASEDDATE <= dateTo
                                        && (invoice.INVOICESTATUS == (int)InvoiceStatus.Released ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Cancel ||
                                        invoice.INVOICESTATUS == (int)InvoiceStatus.Delete)
                                  let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                  select new PTInvoice()
                                  {
                                      ID = invoice.ID,
                                      CompanyID = invoice.COMPANYID,
                                      CustomerID = null,
                                      CustomerCode = null,
                                      TaxCode = notCurrentClient ? client.TAXCODE : invoice.CUSTOMERTAXCODE,
                                      CustomerName = invoice.CUSTOMERNAME,
                                      Address = null,
                                      TypeSendInvoice = null,
                                      Mobile = null,
                                      Fax = null,
                                      Email = null,
                                      Delegate = null,
                                      PersonContact = invoice.CUSTOMERNAME,
                                      BankAccount = null,
                                      AccountHolder = null,
                                      BankName = null,
                                      Description = null,
                                      CustomerType = null,
                                      Password = null,
                                      Deleted = invoice.DELETED,
                                      Created = invoice.RELEASEDDATE,
                                      CreatedBy = invoice.CREATEDBY,
                                      Updated = invoice.UPDATEDDATE,
                                      UpdateBy = null,
                                      InvoiceCode = registerTemplate.CODE,
                                      Symbol = invoice.SYMBOL,
                                      InvoiceNo = invoice.NO,
                                      InvoiceType_Name = invoiceType.NAME,
                                      InvoiceSample_Name = invoiceType.NAME,
                                      ClientID = invoice.CLIENTID,
                                      InvoiceStatus = invoice.INVOICESTATUS,
                                      ClientSign = invoice.CLIENTSIGN ?? false,
                                      VerificationCode = string.Empty,
                                      ReleasedDate = System.Data.Entity.DbFunctions.TruncateTime(invoice.RELEASEDDATE),
                                      Sum = invoice.SUM, //Cột số tiền hóa đơn ( thể hiện số tiền sau thuế ),
                                      CurrencyExchangeRate = invoice.CURRENCYEXCHANGERATE,//Currency,
                                      TaxCodeBuyer = invoice.COMPANYTAXCODE, //Cột MST người bán
                                      UserId = loginuser.USERID, //Cột tài khoản ( cột này tạm thời chưa dùng, nên em tạo nhưng hide đi)
                                      InvoiceNo2 = invoice.INVOICENO,
                                      CurrencyCode = currency.CODE,
                                      InvoiceType = (invoice.INVOICESTATUS == 4 && invoice.PARENTID == null) ? 0 : (invoice.INVOICESTATUS == 4 && invoice.PARENTID != null) ? 1 : 2, //0 : Đã ký, 1: Điều chỉnh, 2: Hủy
                                  };
            var result = researchInvoice;

            if (InvoiceCode != "0")
            {
                result = researchInvoice.Where(x => x.InvoiceNo.Contains(InvoiceCode)).Distinct().OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }
            else
            {
                result = researchInvoice.OrderByDescending(x => x.InvoiceNo2).ThenByDescending(x => x.ReleasedDate);
            }

            return result;
        }

        public IEnumerable GetAnnounByVeriCode(string VerificationCode)
        {
            if (VerificationCode.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var Announcements = SearchAnnounByVeriCode(VerificationCode);
            if (Announcements == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found announcement by verification code {0}", VerificationCode));
            }
            return Announcements;
        }

        IEnumerable SearchAnnounByVeriCode(string verificationCode)
        {
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var Announcements = from ANNOUNCEMENT in this._dbSet.ANNOUNCEMENTs
                                join RELEASEANNOUNCEMENTDETAIL in this._dbSet.RELEASEANNOUNCEMENTDETAILs
                                      on ANNOUNCEMENT.ID equals RELEASEANNOUNCEMENTDETAIL.ANNOUNCEMENTID
                                join INVOICE in this._dbSet.INVOICEs on ANNOUNCEMENT.INVOICEID equals INVOICE.ID
                                join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                  on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                where RELEASEANNOUNCEMENTDETAIL.VERIFICATIONCODE.Equals(verificationCode)
                                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                select new PTAnnouncement()
                                {
                                    ID = ANNOUNCEMENT.ID,
                                    CompanyID = INVOICE.COMPANYID,
                                    CustomerID = null,
                                    CustomerCode = null,
                                    TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                    CustomerName = INVOICE.CUSTOMERNAME,
                                    Address = null,
                                    TypeSendInvoice = null,
                                    Mobile = null,
                                    Fax = null,
                                    Email = null,
                                    Delegate = null,
                                    PersonContact = INVOICE.CUSTOMERNAME,
                                    BankAccount = null,
                                    AccountHolder = null,
                                    BankName = null,
                                    Description = null,
                                    CustomerType = null,
                                    Password = null,
                                    Created = ANNOUNCEMENT.CREATEDDATE,
                                    CreatedBy = ANNOUNCEMENT.CREATEDBY,
                                    Updated = ANNOUNCEMENT.UPDATEDDATE,
                                    UpdateBy = null,
                                    InvoiceID = INVOICE.ID,
                                    InvoiceCode = REGISTERTEMPLATE.CODE,
                                    Symbol = INVOICE.SYMBOL,
                                    InvoiceNo = INVOICE.NO,
                                    AnnouncementType = ANNOUNCEMENT.ANNOUNCEMENTTYPE,
                                    AnnouncementTypeName = ANNOUNCEMENT.TITLE,
                                    ClientID = INVOICE.CLIENTID,
                                    InvoiceStatus = INVOICE.INVOICESTATUS,
                                    ClientSign = ANNOUNCEMENT.CLIENTSIGN ?? false,
                                    VerificationCode = RELEASEANNOUNCEMENTDETAIL.VERIFICATIONCODE,
                                    MinutesNo = ANNOUNCEMENT.MINUTESNO,
                                    ReleasedDate = ANNOUNCEMENT.ANNOUNCEMENTDATE,
                                };
            return Announcements.Distinct();
        }

        public IEnumerable GetAnnounByVeriCodeAndClientId(string VerificationCode, long ClientId, string DateFrom, string DateTo)
        {
            if (VerificationCode.IsNullOrEmpty())
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var Announcements = SearchAnnounByVeriCodeAndClientId(VerificationCode, ClientId, DateFrom, DateTo);
            if (Announcements == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found announcement by verification code {0}", VerificationCode));
            }
            return Announcements;
        }

        IEnumerable SearchAnnounByVeriCodeAndClientId(string verificationCode, long clientId, string DateFrom, string DateTo)
        {
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var Announcements = from ANNOUNCEMENT in this._dbSet.ANNOUNCEMENTs
                                join RELEASEANNOUNCEMENTDETAIL in this._dbSet.RELEASEANNOUNCEMENTDETAILs
                                      on ANNOUNCEMENT.ID equals RELEASEANNOUNCEMENTDETAIL.ANNOUNCEMENTID
                                join INVOICE in this._dbSet.INVOICEs on ANNOUNCEMENT.INVOICEID equals INVOICE.ID
                                join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                  on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                where (!String.IsNullOrEmpty(verificationCode) && RELEASEANNOUNCEMENTDETAIL.VERIFICATIONCODE.Equals(verificationCode)) && client.ID == clientId
                                      && ANNOUNCEMENT.ANNOUNCEMENTDATE >= dateFrom && ANNOUNCEMENT.ANNOUNCEMENTDATE <= dateTo
                                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                orderby ANNOUNCEMENT.UPDATEDDATE descending
                                select new PTAnnouncement()
                                {
                                    ID = ANNOUNCEMENT.ID,
                                    CompanyID = INVOICE.COMPANYID,
                                    CustomerID = null,
                                    CustomerCode = null,
                                    TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                    CustomerName = INVOICE.CUSTOMERNAME,
                                    Address = null,
                                    TypeSendInvoice = null,
                                    Mobile = null,
                                    Fax = null,
                                    Email = null,
                                    Delegate = null,
                                    PersonContact = INVOICE.CUSTOMERNAME,
                                    BankAccount = null,
                                    AccountHolder = null,
                                    BankName = null,
                                    Description = null,
                                    CustomerType = null,
                                    Password = null,
                                    Created = ANNOUNCEMENT.CREATEDDATE,
                                    CreatedBy = ANNOUNCEMENT.CREATEDBY,
                                    Updated = ANNOUNCEMENT.UPDATEDDATE,
                                    UpdateBy = null,
                                    InvoiceID = INVOICE.ID,
                                    InvoiceCode = REGISTERTEMPLATE.CODE,
                                    Symbol = INVOICE.SYMBOL,
                                    InvoiceNo = INVOICE.NO,
                                    AnnouncementType = ANNOUNCEMENT.ANNOUNCEMENTTYPE,
                                    AnnouncementTypeName = ANNOUNCEMENT.TITLE,
                                    ClientID = INVOICE.CLIENTID,
                                    InvoiceStatus = INVOICE.INVOICESTATUS,
                                    ClientSign = ANNOUNCEMENT.CLIENTSIGN ?? false,
                                    VerificationCode = RELEASEANNOUNCEMENTDETAIL.VERIFICATIONCODE,
                                    MinutesNo = ANNOUNCEMENT.MINUTESNO,
                                    ReleasedDate = ANNOUNCEMENT.ANNOUNCEMENTDATE,
                                };
            return Announcements.Distinct().OrderByDescending(x => x.ReleasedDate);
        }

        public IEnumerable GethAnnounByInvoiceNo(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
        {
            //if (InvoiceNo.IsNullOrEmpty())
            //{
            //    throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            //}
            var Announcements = SearchAnnounByInvoiceNoClientID(InvoiceNo, ClientID, DateFrom, DateTo);
            if (Announcements == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, string.Format("Not found invoice by InvoiceCode code {0}", InvoiceNo));
            }
            return Announcements;
        }

        IEnumerable SearchAnnounByInvoiceNoClientID(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
        {
            //&& ANNOUNCEMENT.COMPANYSIGNDATE != null
            var dateFrom = DateFrom.ConvertDateTime();
            var dateTo = ((DateTime)DateTo.ConvertDateTime()).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var Announcements = from ANNOUNCEMENT in this._dbSet.ANNOUNCEMENTs
                                join RELEASEANNOUNCEMENTDETAIL in this._dbSet.RELEASEANNOUNCEMENTDETAILs
                                    on ANNOUNCEMENT.ID equals RELEASEANNOUNCEMENTDETAIL.ANNOUNCEMENTID into lstRELEASEANNOUNCEMENTDETAILs
                                from RELEASEANNOUNCEMENTDETAIL in lstRELEASEANNOUNCEMENTDETAILs.DefaultIfEmpty()
                                join INVOICE in this._dbSet.INVOICEs on ANNOUNCEMENT.INVOICEID equals INVOICE.ID
                                join client in this._dbSet.CLIENTs on INVOICE.CLIENTID equals client.ID
                                join REGISTERTEMPLATE in this._dbSet.REGISTERTEMPLATES
                                     on INVOICE.REGISTERTEMPLATEID equals REGISTERTEMPLATE.ID
                                orderby ANNOUNCEMENT.UPDATEDDATE descending
                                where INVOICE.CLIENTID == ClientID
                                      && ANNOUNCEMENT.ANNOUNCEMENTDATE >= dateFrom && ANNOUNCEMENT.ANNOUNCEMENTDATE <= dateTo
                                let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                                select new PTAnnouncement()
                                {
                                    ID = ANNOUNCEMENT.ID,
                                    CompanyID = INVOICE.COMPANYID,
                                    CustomerID = null,
                                    CustomerCode = null,
                                    TaxCode = notCurrentClient ? client.TAXCODE : INVOICE.CUSTOMERTAXCODE,
                                    CustomerName = INVOICE.CUSTOMERNAME,
                                    Address = null,
                                    TypeSendInvoice = null,
                                    Mobile = null,
                                    Fax = null,
                                    Email = null,
                                    Delegate = null,
                                    PersonContact = INVOICE.CUSTOMERNAME,
                                    BankAccount = null,
                                    AccountHolder = null,
                                    BankName = null,
                                    Description = null,
                                    CustomerType = null,
                                    Password = null,
                                    Created = ANNOUNCEMENT.CREATEDDATE,
                                    CreatedBy = ANNOUNCEMENT.CREATEDBY,
                                    Updated = ANNOUNCEMENT.UPDATEDDATE,
                                    UpdateBy = null,
                                    InvoiceCode = REGISTERTEMPLATE.CODE,
                                    Symbol = INVOICE.SYMBOL,
                                    InvoiceNo = INVOICE.NO,
                                    AnnouncementType = ANNOUNCEMENT.ANNOUNCEMENTTYPE,
                                    AnnouncementTypeName = ANNOUNCEMENT.TITLE,
                                    ClientID = INVOICE.CLIENTID,
                                    InvoiceStatus = INVOICE.INVOICESTATUS,
                                    ClientSign = ANNOUNCEMENT.CLIENTSIGN ?? false,
                                    VerificationCode = "",
                                    MinutesNo = ANNOUNCEMENT.MINUTESNO,
                                    ReleasedDate = ANNOUNCEMENT.ANNOUNCEMENTDATE,
                                };
            if (InvoiceNo != null)
            {
                if (InvoiceNo.Trim() != "0")
                {
                    return Announcements.Where(x => x.InvoiceNo.Contains(InvoiceNo)).Distinct().ToList();
                }
            }

            //var kq = Announcements.Where(x => x.InvoiceNo.Contains(InvoiceNo)).Distinct().ToList();
            return Announcements.OrderByDescending(x => x.ReleasedDate);
        }

        public string GetTokenByInvoiceCode(string InvoiceCode, long companyID, long ClientID)
        {
            PTUser ptus = GetUserClientByInvoiceCode(InvoiceCode, companyID, ClientID);
            var us = GetUser(ptus.UserName, ptus.Pass);
            return us == null ? "" : us.USERID;
        }

        private PTUser GetUserClientByInvoiceCode(string InvoiceCode, long companyID, long ClientID)
        {
            var qry = from invoice in _dbSet.INVOICEs
                      join releaseDetail in this._dbSet.RELEASEINVOICEDETAILs on invoice.ID equals releaseDetail.INVOICEID into releaseDetailTemp
                      join client in this._dbSet.CLIENTs on invoice.CLIENTID equals client.ID
                      join company in this._dbSet.MYCOMPANies on invoice.COMPANYID equals company.COMPANYSID
                      join template in this._dbSet.REGISTERTEMPLATES on invoice.REGISTERTEMPLATEID equals template.ID into templateTemp
                      join login in this._dbSet.LOGINUSERs on client.ID equals login.CLIENTID into loginTemp
                      from releaseDetail in releaseDetailTemp.DefaultIfEmpty()
                      from template in templateTemp.DefaultIfEmpty()
                      from login in loginTemp.DefaultIfEmpty()
                      where invoice.NO.Equals(InvoiceCode) && invoice.COMPANYID == companyID && invoice.CLIENTID == ClientID
                      select new PTUser
                      {
                          UserName = login.USERID,
                          Pass = login.PASSWORD,
                      };
            return qry.FirstOrDefault();
        }

        public long? GetCompanyIdByInvoiceVerifiCode(string VerifiCod)
        {
            return this.releaseInvoiceRepository.GetCompanyIdByVerifiCode(VerifiCod);
        }

        public long? GetCompanyIdByAnnounVerifiCode(string VerifiCod)
        {
            return this.releaseAnnouncementRepository.GetCompanyIdByVerifiCode(VerifiCod);
        }

        public ResultCode UpdataStatusReleaseDetailInvoice(long id)
        {
            var qry = _dbSet.INVOICEs.Where(x => x.ID == id).FirstOrDefault();
            if (qry == null)
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            qry.CLIENTSIGN = true;
            qry.CLIENTSIGNDATE = DateTime.Now;
            _dbSet.SaveChanges();
            return ResultCode.NoError;
        }

        public ResultCode AnnouncementUpdataStatusClientSign(long Id)
        {
            var qry = _dbSet.ANNOUNCEMENTs.Where(x => x.ID == Id).FirstOrDefault();
            if (qry == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            qry.CLIENTSIGN = true;
            qry.CLIENTSIGNDATE = DateTime.Now;
            _dbSet.SaveChanges();
            return ResultCode.NoError;
        }


        public ResultCode UpdateClientInfo(long id, ClientAddInfo client)
        {
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!client.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            CLIENT currentClient = GetClient(id);

            //chi cap nhat thong tin email
            if (currentClient.USEREGISTEREMAIL == true)
                currentClient.EMAIL = client.Email;
            else
                currentClient.RECEIVEDINVOICEEMAIL = client.ReceivedInvoiceEmail;

            currentClient.UPDATEDDATE = DateTime.Now;
            currentClient.UPDATEDBY = client.UserAction;

            ResultCode res;
            try
            {
                this.transaction.BeginTransaction();
                res = this.clientRepository.Update(currentClient) ? ResultCode.NoError : ResultCode.UnknownError;
                this.account.UpdateAccountClient(currentClient); // do chi update thong tin email => k can tao account login
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return res;
        }

        private CLIENT GetClient(long id)
        {
            CLIENT client = this.clientRepository.GetById(id);
            if (client == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return client;
        }

        public IEnumerable<PTClientAccountInfo> GetListClientNotChangePass()
        {
            var Clients = from CLIENT in this._dbSet.CLIENTs
                          join LOGINUSER in this._dbSet.LOGINUSERs
                              on CLIENT.TAXCODE equals LOGINUSER.USERID
                          join MYCOMPANY in this._dbSet.MYCOMPANies
                          on CLIENT.TRX_BRANCH_NO equals MYCOMPANY.BRANCHID
                          where LOGINUSER.PASSWORDSTATUS == (int)PasswordStatus.NotEncript &&
                                  CLIENT.CUSTOMERTYPE == 1 &&
                                  !(CLIENT.DELETED ?? false) &&
                                  CLIENT.ISORG == true
                          select new PTClientAccountInfo
                          {
                              ClientID = CLIENT.ID,
                              Email = CLIENT.EMAIL,
                              UseRegisterEmail = CLIENT.USEREGISTEREMAIL,
                              ReceivedInvoiceEmail = CLIENT.RECEIVEDINVOICEEMAIL,
                              CompanyId = MYCOMPANY.COMPANYSID
                          };
            return Clients.ToList();
        }



        public IEnumerable<NotificationUsedInvoiceDetail> GetNotificationUseInvoiceDetails()
        {
            var rpCancel = from rp in _dbSet.REPORTCANCELLINGDETAILs
                           where rp.REPORTCANCELLING.STATUS == (int)ReportCancellingStatus.Approved
                           select rp.SYMBOL;
            var ListTemplateUsed = from notificationDetail in _dbSet.NOTIFICATIONUSEINVOICEDETAILs
                                   join notification in _dbSet.NOTIFICATIONUSEINVOICEs
                                   on notificationDetail.NOTIFICATIONUSEINVOICEID equals notification.ID
                                   join company in _dbSet.MYCOMPANies
                                   on notification.COMPANYID equals company.COMPANYSID
                                   join registerTemplate in _dbSet.REGISTERTEMPLATES
                                   on notificationDetail.REGISTERTEMPLATESID equals registerTemplate.ID
                                   where (notification.STATUS == (int)RecordStatus.Approved) && !(rpCancel.Contains(notificationDetail.CODE))
                                   group notificationDetail by new { notification.COMPANYID, company.COMPANYNAME, notificationDetail.REGISTERTEMPLATESID, registerTemplate.PREFIX, registerTemplate.SUFFIX, notificationDetail.CODE } into notifications
                                   select new NotificationUsedInvoiceDetail()
                                   {
                                       CompanyId = notifications.Key.COMPANYID,
                                       CompanyName = notifications.Key.COMPANYNAME,
                                       RegisterTemplatesId = notifications.Key.REGISTERTEMPLATESID ?? 0,
                                       Prefix = notifications.Key.PREFIX,
                                       Suffix = notifications.Key.SUFFIX,
                                       NotificationUseCode = notifications.Key.CODE,
                                       NumberRegister = notifications.Sum(x => x.NUMBERUSE),
                                   };
            return ListTemplateUsed.OrderBy(p => p.CompanyName).ThenBy(p => p.RegisterTemplatesId).ThenBy(p => p.NotificationUseCode).ToList();
        }

        public long GetNumberInvoiceReleased(ConditionInvoiceUse condition)
        {
            var numInvoiceRelease = this.invoiceRepository.GetNumberInvoiceReleased(condition);
            return numInvoiceRelease;
        }

        public bool wasSendWarning(ConditionInvoiceUse condition)
        {
            var ListTemplateUsed = from notificationDetail in _dbSet.NOTIFICATIONUSEINVOICEDETAILs
                                   join notification in _dbSet.NOTIFICATIONUSEINVOICEs
                                   on notificationDetail.NOTIFICATIONUSEINVOICEID equals notification.ID
                                   where (notification.STATUS == (int)RecordStatus.Approved
                                        && notification.COMPANYID == condition.CompanyId
                                        && notificationDetail.REGISTERTEMPLATESID == condition.RegisterTemplateId
                                        && notificationDetail.CODE == condition.Symbol
                                        && !(notificationDetail.ISRECEIVEDWARNING ?? false)
                                   )
                                   select notificationDetail;
            if (ListTemplateUsed != null && ListTemplateUsed.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public ResultCode SendEmailWarning(List<NotificationUsedInvoiceDetail> contentMail, string emailCompany, EmailServerInfo emailServerInfo)
        {
            bool result = true;
            StringBuilder content = new StringBuilder();
            string title = "Danh sách chi nhánh sắp hết số hóa đơn ";
            //header
            content.Append("<table class='tableMail' style='border-collapse: collapse;border: 1px solid black; text-align: center; font-size: 13px; '>");
            content.Append("<tr style='font-weight: bold;'><td style='border: 1px solid black; padding: 2px 5px;'>STT</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Tên chi nhánh</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Mẫu số</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Ký hiệu</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Số lượng đăng ký</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Số lượng sử dụng</td>");
            content.Append("<td style='border: 1px solid black; padding: 2px 5px;'>Số lượng còn lại</td>");
            //detail
            var index = 1;
            foreach (var item in contentMail)
            {
                content.AppendFormat("<tr><td style = 'border: 1px solid black;color: #00529b;' >{0}</td>", index);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;padding: 2px 5px;max-width: 400px;text-align: left;' >{0}</td>", item.CompanyName);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;padding: 2px 5px;' >{0}</td>", item.Prefix + "0/" + item.Suffix);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;padding: 2px 5px;'; >{0}</td>", item.NotificationUseCode);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;' >{0}</td>", item.NumberRegister);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;' >{0}</td>", item.NumberUse);
                content.AppendFormat("<td style='border: 1px solid black;color: #00529b;' >{0}</td></tr>", item.NumberRegister - item.NumberUse);
                index++;
            }
            // create info and send mail
            var email = new EmailInfo()
            {
                Subject = title,
                EmailTo = emailCompany,
                Content = content.ToString(),
            };
            ProcessEmail processEmail = new ProcessEmail();
            // return value
            result = processEmail.SendEmail(email, emailServerInfo);
            return result ? ResultCode.NoError : ResultCode.SendEmailFailed;
        }

        public ResultCode UpdateStatusReceiveWarning(List<NotificationUsedInvoiceDetail> listItem)
        {
            bool result = true;
            bool isSuccess = true;
            foreach (var item in listItem)
            {
                var notificationUsedInvoiceDetails = this.notificationUseInvoiceDetailRepository.GetNotificationUseInvoiceDetails((long)item.CompanyId, item.RegisterTemplatesId, item.NotificationUseCode.Replace("/", "")).ToList();
                if (notificationUsedInvoiceDetails != null)
                {
                    foreach (var notificationDetail in notificationUsedInvoiceDetails)
                    {
                        notificationDetail.ISRECEIVEDWARNING = true;
                        isSuccess = this.notificationUseInvoiceDetailRepository.Update(notificationDetail);
                        if (!isSuccess)
                        {
                            result = false;
                        }
                    }
                }
            }
            // return value
            return result ? ResultCode.NoError : ResultCode.DataInvalid;
        }

        public ExportFileInfo ExcelInvoiceNoClient(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
        {
            var dataRport = new InvoiceExportPortal();
            dataRport.Items = GetByInvoiceNoClientID(InvoiceNo, ClientID, DateFrom, DateTo).ToList();
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(1);
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }
            //ReportInvoicesView excel = new ReportInvoicesView(dataRport, config, systemSettingInfo, condition.Language);
            ReportInvoicesPortalView reportInvoicesPortalView = new ReportInvoicesPortalView(dataRport, systemSettingInfo);
            return reportInvoicesPortalView.ExportFile();

        }

        public ExportFileInfo ExcelInvoiceByVeriCodeLogin(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
        {
            var dataRport = new InvoiceExportPortal();
            dataRport.Items = GetBYInvoiceByVeriCodeAndClientId(InvoiceNo, ClientID, DateFrom, DateTo).ToList();
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(1);
            foreach (var item in dataRport.Items)
            {
                FormatBySystemSetting(item, systemSettingInfo);
            }
            //ReportInvoicesView excel = new ReportInvoicesView(dataRport, config, systemSettingInfo, condition.Language);
            ReportInvoicesPortalView reportInvoicesPortalView = new ReportInvoicesPortalView(dataRport, systemSettingInfo);
            return reportInvoicesPortalView.ExportFile();
        }

        private void FormatBySystemSetting(PTInvoice invoiceMaster, SystemSettingInfo SSInfo)
        {
            var systemSetting = SSInfo.DeepCopy();
            StandardSystemSettingIfVND(systemSetting, invoiceMaster.CurrencyCode);

            invoiceMaster.Sum = FormatNumber.SetFractionDigit(invoiceMaster.Sum, systemSetting.Amount);

        }
        private void StandardSystemSettingIfVND(SystemSettingInfo systemSetting, string currencyCode)
        {
            if (currencyCode == DefaultFields.CURRENCY_VND && systemSetting.UseSystemSettingVND == false)
            {
                systemSetting.Amount = 0;
                systemSetting.ConvertedAmount = 0;
            }
        }
    }
}
