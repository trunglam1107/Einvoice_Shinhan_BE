using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common;

namespace InvoiceServer.Business.DAO
{
    public class DeclarationRepository : GenericRepository<INVOICEDECLARATION>, IDeclarationRepository
    {
        private readonly DataClassesDataContext _dbSet = new DataClassesDataContext();

        public DeclarationRepository(IDbContext context)
            : base(context)
        {
        }
        public IEnumerable<INVOICEDECLARATION> GetList()
        {
            return GetDeclarationList();
        }
        public INVOICEDECLARATION GetById(long id)
        {
            return dbSet.FirstOrDefault(p => p.ID == id && (p.ISDELETED == null || p.ISDELETED == false));
        }
        public IQueryable<SYMBOL> GetbyRefId(long id)
        {
            return this.context.Set<SYMBOL>().Where(p => p.REFID == id);
        }
        public List<CompanySymbolInfo> ListSymbol(long companyId)
        {
            var symbolList = (from symbol in this.context.Set<SYMBOL>()
                             join company in this.context.Set<MYCOMPANY>() on symbol.COMPANYID equals company.COMPANYSID
                             select new CompanySymbolInfo()
                             {
                                 CompanyId = company.COMPANYSID,
                                 CompanyIdParent = company.COMPANYID,
                                 Symbol = symbol.SYMBOL1,
                                 CompanyName = company.COMPANYNAME, 
                                 Taxcode = company.TAXCODE,
                                 BranchCode = company.BRANCHID
                             }).ToList();
            if (companyId > 0)
            {
                symbolList = symbolList.Where(x => x.CompanyIdParent == companyId).ToList();
            }
            else
            {
                symbolList = symbolList.ToList();
            }
            return symbolList;
        }

        public IEnumerable<DeclarationInfo> FilterDeclaration(ConditionSearchDeclaration condition, int skip = 0, int take = int.MaxValue)
        {
            var query = this.FilterDeclarationQuery(condition);
            var Declarations = query.AsQueryable().OrderBy(condition.ColumnOrder, condition.Order_Type.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();

            var DeclarationsMaster =
                from Declaration in Declarations
                join city in this._dbSet.CITies on Declaration.CITYID equals city.ID
                join tax in this._dbSet.TAXDEPARTMENTs on Declaration.TAXDEPARTMENTID equals tax.ID
                join mid in this._dbSet.MINVOICE_DATA on Declaration.MESSAGECODE equals mid.MESSAGECODE into m
                where (Declaration.ISDELETED == null || Declaration.ISDELETED == false)
                select new DeclarationInfo()
                {
                    ID = Declaration.ID,
                    ProvinceCity = city.NAME,
                    TaxCompanyName = tax.NAME,
                    TaxCompanyCode = tax.CODE,
                    DeclarationType = Declaration.DECLARATIONTYPE,
                    CompanyName = Declaration.COMPANYNAME,
                    CompanyTaxCode = Declaration.COMPANYTAXCODE,
                    CustName = Declaration.CUSTNAME,
                    CustAddress = Declaration.CUSTADDRESS,
                    CustEmail = Declaration.CUSTEMAIL,
                    CustPhone = Declaration.CUSTPHONE,
                    DeclarationDate = Declaration.DECLARATIONDATE,
                    HTHDon = Declaration.HTHDON,
                    HTGDLHDDT = Declaration.HTGDLHDDT,
                    PThuc = Declaration.PTHUC,
                    LHDSDung = Declaration.LHDSDUNG,
                    Type = Declaration.TYPE,
                    Symbol = Declaration.SYMBOL,
                    CityId = Declaration.CITYID,
                    TaxDepartmentId = Declaration.TAXDEPARTMENTID,
                    UpdateDate = Declaration.UPDATEDATE,
                    UpdateBy = Declaration.UPDATEBY,
                    Status = Declaration.STATUS,
                    MessageCode = Declaration.MESSAGECODE,
                    mINVOICE_DATA = Declaration.MESSAGECODE!= null ?  m.FirstOrDefault() : null,
                    ApprovedDate = Declaration.MESSAGECODE != null ? m.FirstOrDefault().APPROVEDDATE : null
                };

            var res = DeclarationsMaster.ToList();

            foreach (var declarationInfo in res)
            {
                ProcessSendStatus(declarationInfo);
            }

            return res;

        }

        public IEnumerable<DeclarationInfo> FilterDeclarationForExcel(ConditionSearchDeclaration condition)
        {
            var query = this.FilterDeclarationQuery(condition);
            var Declarations = query.AsQueryable().OrderByDescending(x => x.DECLARATIONDATE).ToList();

            var DeclarationsMaster =
                from Declaration in Declarations
                join city in this._dbSet.CITies on Declaration.CITYID equals city.ID
                join tax in this._dbSet.TAXDEPARTMENTs on Declaration.TAXDEPARTMENTID equals tax.ID
                join mid in this._dbSet.MINVOICE_DATA on Declaration.MESSAGECODE equals mid.MESSAGECODE into m
                where (Declaration.ISDELETED == null || Declaration.ISDELETED == false)
                select new DeclarationInfo()
                {
                    ID = Declaration.ID,
                    ProvinceCity = city.NAME,
                    TaxCompanyName = tax.NAME,
                    TaxCompanyCode = tax.CODE,
                    DeclarationType = Declaration.DECLARATIONTYPE,
                    CompanyName = Declaration.COMPANYNAME,
                    CompanyTaxCode = Declaration.COMPANYTAXCODE,
                    CustName = Declaration.CUSTNAME,
                    CustAddress = Declaration.CUSTADDRESS,
                    CustEmail = Declaration.CUSTEMAIL,
                    CustPhone = Declaration.CUSTPHONE,
                    DeclarationDate = Declaration.DECLARATIONDATE,
                    HTHDon = Declaration.HTHDON,
                    HTGDLHDDT = Declaration.HTGDLHDDT,
                    PThuc = Declaration.PTHUC,
                    LHDSDung = Declaration.LHDSDUNG,
                    Type = Declaration.TYPE,
                    Symbol = Declaration.SYMBOL,
                    CityId = Declaration.CITYID,
                    TaxDepartmentId = Declaration.TAXDEPARTMENTID,
                    UpdateDate = Declaration.UPDATEDATE,
                    UpdateBy = Declaration.UPDATEBY,
                    Status = Declaration.STATUS,
                    MessageCode = Declaration.MESSAGECODE,
                    //mINVOICE_DATA = m.OrderByDescending(x => x.CREATEDDATE).FirstOrDefault(),
                    //ApprovedDate = m.OrderByDescending(x => x.CREATEDDATE).FirstOrDefault().APPROVEDDATE
                    mINVOICE_DATA = Declaration.MESSAGECODE != null ? m.FirstOrDefault() : null,
                    ApprovedDate = Declaration.MESSAGECODE != null ? m.FirstOrDefault().APPROVEDDATE : null
                };

            var res = DeclarationsMaster.ToList();

            foreach (var declarationInfo in res)
            {
                ProcessSendStatus(declarationInfo);
            }

            return res;
        }

        private void ProcessSendStatus(DeclarationInfo declarationInfo)
        {
            if (declarationInfo == null)
                return;

            var mINVOICE_DATA = declarationInfo.mINVOICE_DATA;

            if (mINVOICE_DATA == null)
            {
                declarationInfo.SendStatusMessage = "";
                declarationInfo.SendStatusCode = 0;
            }
            else
            {
                if (mINVOICE_DATA.THOP == "1" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationRecieved && mINVOICE_DATA.STATUS == 1)
                {
                    declarationInfo.SendStatusMessage = "Đã tiếp nhận Tờ khai đăng ký sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 1;
                }
                else if (mINVOICE_DATA.THOP == "2" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationRecieved)
                {
                    declarationInfo.SendStatusMessage = "Không tiếp nhận Tờ khai đăng ký sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 2;
                }
                else if (mINVOICE_DATA.THOP == "3" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationRecieved && mINVOICE_DATA.STATUS == 1)
                {
                    declarationInfo.SendStatusMessage = "Đã tiếp nhận Tờ khai đăng ký thay đổi thông tin sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 3;
                }
                else if (mINVOICE_DATA.THOP == "4" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationRecieved)
                {
                    declarationInfo.SendStatusMessage = "Không tiếp nhận Tờ khai đăng ký thay đổi thông tin sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 4;
                }
                else if (mINVOICE_DATA.TTXNCQT == "1" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationApproved && mINVOICE_DATA.STATUS == 1)
                {
                    declarationInfo.SendStatusMessage = "Đã chấp nhận đăng ký/thay đổi thông tin sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 5;
                }
                else if (mINVOICE_DATA.TTXNCQT == "2" && mINVOICE_DATA.MLTDIEP == MinvoiceStatus.DeclarationApproved)
                {
                    declarationInfo.SendStatusMessage = "Không chấp nhận đăng ký/thay đổi thông tin sử dụng hóa đơn điện tử";
                    declarationInfo.SendStatusCode = 7;
                }
                else
                {
                    declarationInfo.SendStatusMessage = mINVOICE_DATA.ERROR;
                    declarationInfo.SendStatusCode = 8;
                }
            }
        }

        public int CountDeclaration(ConditionSearchDeclaration condition)
        {
            var query = this.FilterDeclarationQuery(condition);
            return query.Count();
        }

        private IEnumerable<INVOICEDECLARATION> FilterDeclarationQuery(ConditionSearchDeclaration condition)
        {
            var invoiceConcludes = this.GetDeclarationList();

            if (!string.IsNullOrEmpty(condition.CompanyName))
            {
                invoiceConcludes = invoiceConcludes.Where(p => p.COMPANYNAME == condition.CompanyName);
            }
            if (condition.Status.HasValue)
            {
                invoiceConcludes = invoiceConcludes.Where(p => p.STATUS == condition.Status);
            }
            if (condition.CompanyID.HasValue && condition.CompanyID != 0)
            {
                invoiceConcludes = invoiceConcludes.Where(p => p.COMPANYID == condition.CompanyID);
            }

            return invoiceConcludes;
        }

        private IQueryable<INVOICEDECLARATION> GetDeclarationList()
        {
            var listDecla = dbSet;

            return listDecla;
        }

        public ResultCode CheckSendTwan(long id)
        {
            var declaration = (from declara in this.context.Set<INVOICEDECLARATION>()
                               join mInvoice in this.context.Set<MINVOICE_DATA>()
                               on declara.MESSAGECODE equals mInvoice.MESSAGECODE
                               where declara.ID == id
                               select new DeclarationInfo
                               {
                                   SendStatus = mInvoice.STATUS
                               }).FirstOrDefault();

            if (declaration != null)
            {
                if (declaration.SendStatus != 1)
                {
                    throw new BusinessLogicException(ResultCode.SendMailCQT, MsgApiResponse.DataNotFound);
                }
                else
                {
                    var INVOICEDECLARATION = GetById(id);
                    if (INVOICEDECLARATION != null)
                    {
                        INVOICEDECLARATION.STATUS = DeclarationStatus.Completed;
                        _dbSet.INVOICEDECLARATIONs.Add(INVOICEDECLARATION);
                        _dbSet.SaveChanges();
                    }


                }

            }
            else
            {
                throw new BusinessLogicException(ResultCode.SendMailCQT, MsgApiResponse.DataNotFound);
            }
            return ResultCode.NoError;
        }

        public IQueryable<DeclarationMaster> GetListSymboy(long? companyId)
        {
            var mycompany = this.context.Set<MYCOMPANY>().FirstOrDefault(x => x.COMPANYSID == companyId);

            var symbols = this.context.Set<SYMBOL>().Where(x => x.COMPANYID == mycompany.COMPANYSID);

            return (from de in dbSet
                    join symbol in symbols
                    on de.ID equals symbol.REFID
                    where (de.STATUS != null && de.STATUS == DeclarationStatus.Completed)
                    select new DeclarationMaster
                    {
                        Id = de.ID,
                        Symbol = symbol.SYMBOL1,
                        HTHDon = de.HTHDON,
                        LHDSDung = de.LHDSDUNG,
                        DeclarationDate = de.DECLARATIONDATE,
                        Type = de.TYPE,
                        RegisterTemplateId = de.REGISTERTEMPLATEID,

                    });
        }

        public INVOICEDECLARATION GetLastInfo(long? companyID)
        {
            var result = from invdec in this.context.Set<INVOICEDECLARATION>()
                         join minvoice in this.context.Set<MINVOICE_DATA>() on invdec.MESSAGECODE equals minvoice.MESSAGECODE
                         where (invdec.STATUS != null && invdec.STATUS == (int)DeclarationStatus.Completed) &&
                         (minvoice.STATUS != null && minvoice.STATUS == StatusCQT.success) &&
                         (minvoice.MLTDIEP != null && minvoice.MLTDIEP.Equals(MinvoiceStatus.DeclarationApproved)) &&
                         (minvoice.TTXNCQT != null && minvoice.TTXNCQT.Equals(TTXNCQT.CQTCNTK)) &&
                         (companyID == null || invdec.COMPANYID == companyID)
                         orderby invdec.DECLARATIONDATE descending
                         select invdec;
            return result.FirstOrDefault();
        }

        public int GetDeclarationUseRegisterTemplate(long id)
        {
            var invoiceConcludes = this.GetDeclarationList();
            invoiceConcludes = invoiceConcludes.Where(p => p.REGISTERTEMPLATEID == id);
            return invoiceConcludes.Count();
        }
        public DateTime? GetMaxDateInvoice(long? companyId, string symbol)
        {
            //var mycompany = this.context.Set<MYCOMPANY>().FirstOrDefault(x => x.COMPANYSID == companyId); p.COMPANYID == mycompany.COMPANYID &&

            var releaseddate = this.context.Set<INVOICE>().
                Where(p => p.SYMBOL.Equals(symbol) && (p.INVOICESTATUS == (int)InvoiceStatus.Approved || p.INVOICESTATUS == (int)InvoiceStatus.Released))
                .OrderByDescending(p => p.INVOICENO).FirstOrDefault()?.RELEASEDDATE;


            return releaseddate;
        }

        public DateTime ExpireTokenDate(long? companyId)
        {
            var date = DateTime.Now;
            var mycompany = this.context.Set<MYCOMPANY>().FirstOrDefault(x => x.COMPANYSID == companyId);

            var result = (from declaration in this.dbSet
                          join declarationRelease in this.context.Set<INVOICEDECLARATIONRELEASE>()
                          on declaration.ID equals declarationRelease.DECLARATIONID
                          where declaration.STATUS == 5
                             && declaration.COMPANYID == mycompany.COMPANYID
                             && declaration.ISDELETED != true
                          select declarationRelease.TODATE).FirstOrDefault();

            return result ?? DateTime.Now;
        }
        public int GetExpireDateToken(long? companyId)
        {
            var date = this.ExpireTokenDate(companyId ?? 0);
            var result = (date - DateTime.Now).Days;

            return result;
        }
    }
}
