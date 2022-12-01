using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class MyCompanyRepository : GenericRepository<MYCOMPANY>, IMyCompanyRepository
    {
        public MyCompanyRepository(IDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get an IEnumerable MyCompany 
        /// </summary>
        /// <returns><c>IEnumerable MyCompany</c> if MyCompany not Empty, <c>null</c> otherwise</returns>
        public IEnumerable<MYCOMPANY> GetList()
        {
            return GetMyCompanyActive();
        }

        /// <summary>
        /// Get an MyCompany by CompanySID.
        /// </summary>
        /// <param name="id">The condition get MyCompany.</param>
        /// <returns><c>Operator</c> if CompanySID on database, <c>null</c> otherwise</returns>
        public MYCOMPANY GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.COMPANYSID == id && !(p.DELETED ?? false));
        }
        /// <summary>
        ///  Lấy danh sách companies 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<MYCOMPANY> GetByIds(List<long> ids)
        {
            return this.dbSet.Where(p => ids.Contains(p.COMPANYSID) && (p.DELETED == null || !p.DELETED.Value)).ToList();
        }


        private IQueryable<MYCOMPANY> GetMyCompanyActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public IEnumerable<MYCOMPANY> FilterCompany(ConditionSearchCompany condition)
        {
            var companies = GetMyCompanyActive().Where(p => p.LEVELCUSTOMER.Equals(CustomerLevel.Sellers));
            companies = companies.Where(p => (p.COMPANYID ?? 0) == condition.CompanyId);

            companies = companies.Where(p => (p.ACTIVE ?? false));
            if (!condition.Keyword.IsNullOrEmpty())
            {
                companies = companies.Where(p => (p.COMPANYNAME.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.EMAIL.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.TAXCODE.ToUpper().Contains(condition.Keyword.ToUpper())));
            }

            return companies;
        }

        public IEnumerable<MYCOMPANY> FilterCustomer(ConditionSearchCustomer condition)
        {
            var companies = GetMyCompanyActive();
            if (!condition.Branch.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.COMPANYSID == condition.Branch);
            }

            if (!condition.CustomerName.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.COMPANYNAME.ToUpper().Contains(condition.CustomerName.ToUpper()));
            }

            if (!condition.TaxCode.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.TAXCODE.ToUpper().Contains(condition.TaxCode.ToUpper()));
            }


            if (!condition.Delegate.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.DELEGATE.ToUpper().Contains(condition.Delegate.ToUpper()));
            }

            if (!condition.BranchId.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.BRANCHID.Contains(condition.BranchId));
            }

            if (!condition.Tel.IsNullOrEmpty())
            {
                companies = companies.Where(p => p.TEL1.ToUpper().Contains(condition.Tel.ToUpper()));
            }
            if (condition.Status.HasValue)
            {
                companies = companies.Where(p => p.ACTIVE == (condition.Status.Value != 0));
            }
            return companies;
        }

        public IEnumerable<MYCOMPANY> FilterCustomer(ConditionSearchCustomer condition, string customerType)
        {
            var companies = GetMyCompanyActive();
            companies = companies.Where(p => p.COMPANYID == condition.CompanyId && p.LEVELCUSTOMER.Equals(customerType));
            if (!condition.CustomerName.IsNullOrEmpty())
            {
                companies = companies.Where(p => (p.COMPANYNAME.ToUpper().Contains(condition.CustomerName.ToUpper())
                    || p.DELEGATE.ToUpper().Contains(condition.CustomerName.ToUpper())
                    || p.TEL1.ToUpper().Contains(condition.CustomerName.ToUpper())
                    || p.TAXCODE.ToUpper().Contains(condition.CustomerName.ToUpper())));
            }
            return companies;
        }

        public bool ContainEmail(string email)
        {
            bool result = false;
            if (!email.IsNullOrEmpty())
            {
                result = Contains(p => ((p.EMAIL ?? string.Empty).ToUpper()).Equals(email.ToUpper()) && !(p.DELETED ?? false));
            }
            return result;
        }

        public bool ContainTaxCode(string taxCode)
        {
            bool result = false;
            if (!taxCode.IsNullOrEmpty())
            {
                result = Contains(p => ((p.TAXCODE ?? string.Empty).Equals(taxCode) && !(p.DELETED ?? false)));
            }

            return result;
        }

        public bool ContainEmail(long companyId, string email, string levelCustomer)
        {
            bool result = false;
            if (!email.IsNullOrEmpty())
            {
                result = Contains(p => ((p.EMAIL ?? string.Empty).ToUpper()).Equals(email.ToUpper()) && !(p.DELETED ?? false) && p.COMPANYID == companyId && p.LEVELCUSTOMER.Equals(levelCustomer));
            }
            return result;
        }

        public bool ContainTaxCode(long companyId, string taxCode, string levelCustomer)
        {
            bool result = false;
            if (!taxCode.IsNullOrEmpty())
            {
                result = Contains(p => ((p.TAXCODE ?? string.Empty).Equals(taxCode) && !(p.DELETED ?? false)) && p.COMPANYID == companyId && p.LEVELCUSTOMER.Equals(levelCustomer));
            }

            return result;
        }

        public IEnumerable<MYCOMPANY> GetCompanyChill(long companyIDRoot)
        {
            var myCompanyUpgradeChill = from myCompanyUpgrade in this.context.Set<MYCOMPANYUPGRADE>().Where(p => p.COMPANYID == companyIDRoot)
                                        join myCompany in this.context.Set<MYCOMPANY>().Where(p => !(p.DELETED ?? false))
                                        on myCompanyUpgrade.COMPANYSID equals myCompany.COMPANYSID
                                        where (myCompanyUpgrade.TYPE == 0) && (myCompanyUpgrade.APPLYDATE > DateTime.Now)
                                        select myCompany;
            var myCompanyChill = GetMyCompanyActive().Where(p => p.COMPANYID == companyIDRoot && p.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice).ToList();
            return myCompanyChill.Union(myCompanyUpgradeChill.ToList());
        }

        private IQueryable<CustomerOfCompany> GetCustomerOfCompany(long companyId)
        {
            var customerOfCompany = (from leve1 in dbSet
                                     where leve1.COMPANYID == companyId
                                     && !(leve1.DELETED ?? false)
                                     && (leve1.ACTIVE ?? false)
                                     select new CustomerOfCompany
                                     {
                                         CompanySID = leve1.COMPANYSID,
                                         CompanyName = leve1.COMPANYNAME,
                                         CustomerName = leve1.COMPANYNAME,
                                         TaxCode = leve1.TAXCODE,
                                         Email = (leve1.EMAILOFCONTRACT ?? leve1.EMAIL),
                                         PersonContact = leve1.DELEGATE,
                                         Tel = leve1.TEL1,
                                         LevelCustomer = leve1.LEVELCUSTOMER,
                                         CustomerIdOfCompany = leve1.COMPANYSID,

                                     }).Union
                                    (from leve1 in dbSet
                                     join leve2 in this.context.Set<MYCOMPANY>()
                                     on leve1.COMPANYSID equals leve2.COMPANYID
                                     where leve1.COMPANYID == companyId
                                     && !(leve2.DELETED ?? false)
                                     && (leve2.ACTIVE ?? false)
                                     select new CustomerOfCompany
                                     {
                                         CompanySID = leve2.COMPANYSID,
                                         CompanyName = leve1.COMPANYNAME,
                                         CustomerName = leve2.COMPANYNAME,
                                         TaxCode = leve2.TAXCODE,
                                         Email = (leve2.EMAILOFCONTRACT ?? leve2.EMAIL),
                                         PersonContact = leve2.DELEGATE,
                                         Tel = leve2.TEL1,
                                         LevelCustomer = leve2.LEVELCUSTOMER,
                                         CustomerIdOfCompany = leve1.COMPANYSID,
                                     }).Union
                                    (from leve1 in dbSet
                                     join leve2 in this.context.Set<MYCOMPANY>()
                                     on leve1.COMPANYSID equals leve2.COMPANYID
                                     join leve3 in this.context.Set<MYCOMPANY>()
                                     on leve2.COMPANYSID equals leve3.COMPANYID
                                     where leve1.COMPANYID == companyId
                                     && !(leve3.DELETED ?? false)
                                     && (leve3.ACTIVE ?? false)
                                     select new CustomerOfCompany
                                     {
                                         CompanySID = leve3.COMPANYSID,
                                         CompanyName = leve1.COMPANYNAME,
                                         CustomerName = leve3.COMPANYNAME,
                                         TaxCode = leve3.TAXCODE,
                                         Email = (leve3.EMAILOFCONTRACT ?? leve3.EMAIL),
                                         PersonContact = leve3.DELEGATE,
                                         Tel = leve3.TEL1,
                                         LevelCustomer = leve3.LEVELCUSTOMER,
                                         CustomerIdOfCompany = leve1.COMPANYSID,
                                     }).Union
                                    (from leve1 in dbSet
                                     join leve2 in this.context.Set<MYCOMPANY>()
                                     on leve1.COMPANYSID equals leve2.COMPANYID
                                     join leve3 in this.context.Set<MYCOMPANY>()
                                     on leve2.COMPANYSID equals leve3.COMPANYID
                                     join leve4 in this.context.Set<MYCOMPANY>()
                                     on leve3.COMPANYSID equals leve4.COMPANYID
                                     where leve1.COMPANYID == companyId
                                     && !(leve4.DELETED ?? false)
                                     && (leve4.ACTIVE ?? false)
                                     select new CustomerOfCompany
                                     {
                                         CompanySID = leve4.COMPANYSID,
                                         CompanyName = leve1.COMPANYNAME,
                                         CustomerName = leve3.COMPANYNAME,
                                         TaxCode = leve4.TAXCODE,
                                         Email = (leve4.EMAILOFCONTRACT ?? leve4.EMAIL),
                                         PersonContact = leve4.DELEGATE,
                                         Tel = leve4.TEL1,
                                         LevelCustomer = leve4.LEVELCUSTOMER,
                                         CustomerIdOfCompany = leve1.COMPANYSID,
                                     });


            return customerOfCompany.AsQueryable();
        }

        public IEnumerable<ReportCustomer> GetReportReportCustomer(ConditionReportSummary condition)
        {
            var customerOfCompany = GetCustomerOfCompany(condition.CompanyId.Value);
            var userNumberInvoicebyContract = GetNumberInvoiceUsed(condition.DateFrom.Value, condition.DateFrom.Value);
            //
            var numberRegistByCompany = from notification in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                        join notificationDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>()
                                            on notification.ID equals notificationDetail.NOTIFICATIONUSEINVOICEID
                                        where (notification.CREATEDDATE >= condition.DateFrom.Value)
                                            && !(notification.CREATEDDATE <= condition.DateFrom.Value)
                                        group notificationDetail by new { notification.COMPANYID } into temp
                                        select new NumberInvoiceRegist()
                                        {
                                            CompanyId = temp.Key.COMPANYID,
                                            NumberRegist = temp.Sum(i => i.NUMBERUSE),
                                        };

            if (condition.CustomerId.HasValue)
            {
                customerOfCompany = customerOfCompany.Where(p => p.CustomerIdOfCompany == condition.CustomerId.Value);
            }
            else if (condition.CustomerName != null)
            {
                customerOfCompany = customerOfCompany.Where(p => p.CustomerName.Contains(condition.CustomerName));
            }

            var reportCustomer = from cust in customerOfCompany
                                 join numberUse in userNumberInvoicebyContract
                                 on cust.CompanySID equals numberUse.CompanyId into numberUseTemp
                                 from numberUse in numberUseTemp.DefaultIfEmpty()
                                     //
                                 join numberRegist in numberRegistByCompany
                                 on cust.CompanySID equals numberRegist.CompanyId into numberRegistTemp
                                 from numberRegist in numberRegistTemp.DefaultIfEmpty()
                                 select new ReportCustomer()
                                 {
                                     CustomerName = cust.CustomerName,
                                     PersonContact = cust.PersonContact,
                                     Email = cust.Email,
                                     Tel = cust.Tel,
                                     TaxCode = cust.TaxCode,
                                     RumberRegister = numberRegist.NumberRegist,
                                     NumberUse = numberUse.NumberUse,
                                 };

            return reportCustomer.OrderBy(p => p.CustomerName);
        }
        public IEnumerable<ReportCustomer> GetReportReportCompany(ConditionReportSummary condition)
        {
            var customerOfCompany = GetMyCompanyActive();
            var userNumberInvoicebyContract = GetNumberInvoiceUsed2(condition.DateFrom.Value, condition.DateFrom.Value);
            //
            var numberRegistByCompany = from notification in this.context.Set<NOTIFICATIONUSEINVOICE>()
                                        join notificationDetail in this.context.Set<NOTIFICATIONUSEINVOICEDETAIL>()
                                            on notification.ID equals notificationDetail.NOTIFICATIONUSEINVOICEID
                                        where (notification.CREATEDDATE >= condition.DateFrom.Value)
                                            && !(notification.CREATEDDATE <= condition.DateFrom.Value)
                                        group notificationDetail by new { notification.COMPANYID } into temp
                                        select new NumberInvoiceRegist()
                                        {
                                            CompanyId = temp.Key.COMPANYID,
                                            NumberRegist = temp.Sum(i => i.NUMBERUSE),
                                        };
            if (condition.CustomerId.HasValue)
            {
                customerOfCompany = customerOfCompany.Where(p => p.COMPANYSID == condition.CustomerId.Value).AsQueryable();
            }

            var reportCustomer = from cust in customerOfCompany
                                 join numberUse in userNumberInvoicebyContract
                                 on cust.COMPANYSID equals numberUse.CompanyId into numberUseTemp
                                 from numberUse in numberUseTemp.DefaultIfEmpty()
                                     //
                                 join numberRegist in numberRegistByCompany
                                 on cust.COMPANYSID equals numberRegist.CompanyId into numberRegistTemp
                                 from numberRegist in numberRegistTemp.DefaultIfEmpty()
                                 select new ReportCustomer()
                                 {
                                     CustomerName = cust.COMPANYNAME,
                                     PersonContact = cust.PERSONCONTACT,
                                     Email = cust.EMAIL,
                                     Tel = cust.TEL1,
                                     TaxCode = cust.TAXCODE,
                                     RumberRegister = numberRegist.NumberRegist,
                                     NumberUse = numberUse.NumberUse,
                                 };

            return reportCustomer.OrderBy(p => p.CustomerName);
        }

        public long getIdHo()
        {
            List<MYCOMPANY> listMyCompany;
            listMyCompany = (from myCompany in this.dbSet
                             where myCompany.ACTIVE == true && myCompany.LEVELCUSTOMER == "HO"
                             select myCompany).ToList();
            return listMyCompany[0].COMPANYSID;
            //return listMyCompany[0]
        }

        public IEnumerable<BranchLevel> GetListBranchs(long companyId, int isGetAll)
        {
            List<MYCOMPANY> listMyCompany;
            if (isGetAll == 1)
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where !(myCompany.DELETED ?? false)
                                 select myCompany).ToList();
            }
            else
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where (myCompany.COMPANYSID == companyId)
                                         && !(myCompany.DELETED ?? false)
                                 select myCompany).ToList();
            }
            return GetResBranchLevel(listMyCompany);
        }

        public IEnumerable<BranchLevel> GetListBranchs_V2(long companyId, int isGetAll)
        {
            List<MYCOMPANY> listMyCompany;
            if (isGetAll == 1)
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where !(myCompany.DELETED ?? false) && (myCompany.LEVELCUSTOMER == "HO") || (myCompany.LEVELCUSTOMER == "CN")
                                 select myCompany).ToList();
            }
            else
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where (myCompany.COMPANYSID == companyId)
                                         && !(myCompany.DELETED ?? false)
                                 select myCompany).ToList();
            }
            return GetResBranchLevelNoPGD(listMyCompany);

        }
        public IEnumerable<BranchLevel> GetListBranchs_2(long companyId)
        {
            List<MYCOMPANY> listMyCompany;
            listMyCompany = (from myCompany in this.dbSet
                             where (myCompany.COMPANYSID == companyId) || (myCompany.COMPANYID == companyId)
                                     && !(myCompany.DELETED ?? false)
                             select myCompany).ToList();
            return GetResBranchLevel(listMyCompany);

        }
        public IEnumerable<BranchLevel> GetListAllBranchs()
        {
            List<MYCOMPANY> listMyCompany;
            listMyCompany = (from myCompany in this.dbSet
                             where !(myCompany.DELETED ?? false) && (myCompany.LEVELCUSTOMER == "HO") || (myCompany.LEVELCUSTOMER == "CN")
                             select myCompany).ToList();
            return GetResBranchLevelNoPGD(listMyCompany);
        }
        private IEnumerable<BranchLevel> GetResBranchLevelNoPGD(List<MYCOMPANY> listMyCompany)
        {
            var IdHo = getIdHo().ToString();
            var res = from myCompany in listMyCompany
                      where myCompany.ACTIVE ?? false
                      select new BranchLevel()
                      {
                          Id = myCompany.COMPANYSID,
                          CompanyId = myCompany.COMPANYSID,
                          Name = myCompany.LEVELCUSTOMER == "HO" ? myCompany.COMPANYNAME : "--" + myCompany.COMPANYNAME,
                          Clever = myCompany.LEVELCUSTOMER == "HO" ? IdHo :
                                           myCompany.LEVELCUSTOMER == "PGD" && myCompany.COMPANYID == 1 ? IdHo + "2" :
                                           myCompany.LEVELCUSTOMER == "CN" ? IdHo + "3" + myCompany.COMPANYSID.ToString().PadLeft(5, char.Parse("0")) :
                                           myCompany.LEVELCUSTOMER == "PGD" && myCompany.COMPANYID != 1 ? IdHo + "3" + myCompany.COMPANYID.ToString().PadLeft(5, char.Parse("0")) + myCompany.COMPANYSID :
                                                                                                                                                       "",
                          TaxCode = myCompany.TAXCODE,
                          Address = myCompany.ADDRESS,
                          Tell = myCompany.TEL1,
                          Fax = myCompany.FAX,
                          Email = myCompany.EMAIL,
                          PersonContact = myCompany.PERSONCONTACT,
                          Delegate = myCompany.DELEGATE,
                          BankAccount = myCompany.BANKACCOUNT,
                          AccountHolder = myCompany.ACCOUNTHOLDER,
                          BankName = myCompany.BANKNAME,
                          CityId = myCompany.CITYID,
                          TaxDepartmentId = myCompany.TAXDEPARTMENTID,
                          Description = myCompany.DESCRIPTION,
                          Delete = myCompany.DELETED,
                          EmailOfContract = myCompany.EMAILOFCONTRACT,
                      };
            return res;
        }
        private IEnumerable<BranchLevel> GetResBranchLevel(List<MYCOMPANY> listMyCompany)
        {
            var IdHo = getIdHo().ToString();
            var res = from myCompany in listMyCompany
                      where myCompany.ACTIVE ?? false
                      select new BranchLevel()
                      {
                          Id = myCompany.COMPANYSID,
                          CompanyId = myCompany.COMPANYSID,
                          Name = myCompany.LEVELCUSTOMER == "HO" ? myCompany.COMPANYNAME :
                                           myCompany.LEVELCUSTOMER == "CN" ? "--" + myCompany.COMPANYNAME :
                                                                           "----" + myCompany.COMPANYNAME,
                          Clever = myCompany.LEVELCUSTOMER == "HO" ? IdHo :
                                           myCompany.LEVELCUSTOMER == "PGD" && myCompany.COMPANYID == 1 ? IdHo + "2" :
                                           myCompany.LEVELCUSTOMER == "CN" ? IdHo + "3" + myCompany.COMPANYSID.ToString().PadLeft(5, char.Parse("0")) :
                                           myCompany.LEVELCUSTOMER == "PGD" && myCompany.COMPANYID != 1 ? IdHo + "3" + myCompany.COMPANYID.ToString().PadLeft(5, char.Parse("0")) + myCompany.COMPANYSID :
                                                                                                                                                       "",
                          TaxCode = myCompany.TAXCODE,
                          Address = myCompany.ADDRESS,
                          Tell = myCompany.TEL1,
                          Fax = myCompany.FAX,
                          Email = myCompany.EMAIL,
                          PersonContact = myCompany.PERSONCONTACT,
                          Delegate = myCompany.DELEGATE,
                          BankAccount = myCompany.BANKACCOUNT,
                          AccountHolder = myCompany.ACCOUNTHOLDER,
                          BankName = myCompany.BANKNAME,
                          CityId = myCompany.CITYID,
                          TaxDepartmentId = myCompany.TAXDEPARTMENTID,
                          Description = myCompany.DESCRIPTION,
                          Delete = myCompany.DELETED,
                          EmailOfContract = myCompany.EMAILOFCONTRACT,
                      };
            return res;
        }
        public IEnumerable<BranchLevel> GetListBranchsOnly(long companyId, int isGetAll)
        {
            List<MYCOMPANY> listMyCompany;
            if (isGetAll == 1)
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where !(myCompany.DELETED ?? false)
                                    && (myCompany.LEVELCUSTOMER != LevelCustomerInfo.TransactionOffice)
                                 select myCompany).ToList();
            }
            else
            {
                listMyCompany = (from myCompany in this.dbSet
                                 where (myCompany.COMPANYSID == companyId)
                                        && !(myCompany.DELETED ?? false)
                                        && (myCompany.LEVELCUSTOMER != LevelCustomerInfo.TransactionOffice)
                                 select myCompany).ToList();
            }
            var res = from myCompany in listMyCompany
                      where myCompany.ACTIVE ?? false
                      select new BranchLevel()
                      {
                          Id = myCompany.COMPANYSID,
                          Name = myCompany.LEVELCUSTOMER == "HO" ? myCompany.COMPANYNAME : "--" + myCompany.COMPANYNAME,
                          Clever = myCompany.LEVELCUSTOMER == "HO" ? "1" :
                                           myCompany.COMPANYID + "2" + myCompany.COMPANYSID.ToString().PadLeft(5, char.Parse("0")),
                          TaxCode = myCompany.TAXCODE,
                          Address = myCompany.ADDRESS,
                          Tell = myCompany.TEL1,
                          Fax = myCompany.FAX,
                          Email = myCompany.EMAIL,
                          PersonContact = myCompany.PERSONCONTACT,
                          Delegate = myCompany.DELEGATE,
                          BankAccount = myCompany.BANKACCOUNT,
                          AccountHolder = myCompany.ACCOUNTHOLDER,
                          BankName = myCompany.BANKNAME,
                          CityId = myCompany.CITYID,
                          TaxDepartmentId = myCompany.TAXDEPARTMENTID,
                          Description = myCompany.DESCRIPTION,
                          Delete = myCompany.DELETED,
                          EmailOfContract = myCompany.EMAILOFCONTRACT,
                      };
            var res2 = GetListBranchsUpgrade(companyId, isGetAll).AsEnumerable();
            var resFinal = res.AsEnumerable().Union(res2);
            return resFinal;
        }
        private IEnumerable<BranchLevel> GetListBranchsUpgrade(long companyId, int isGetAll)
        {
            List<MYCOMPANYUPGRADE> listMyCompanyUpgrade;
            if (isGetAll == 1)
            {
                listMyCompanyUpgrade = this.context.Set<MYCOMPANYUPGRADE>().ToList();
            }
            else
            {
                listMyCompanyUpgrade = this.context.Set<MYCOMPANYUPGRADE>().Where(p => p.COMPANYSID == companyId).ToList();
            }
            var myCompanys = GetMyCompanyActive();
            var res = from myCompanyUpgrade in listMyCompanyUpgrade
                      join myCompany in myCompanys
                      on myCompanyUpgrade.COMPANYSID equals myCompany.COMPANYSID
                      where (myCompanyUpgrade.TYPE == 1)
                           && (myCompanyUpgrade.APPLYDATE > DateTime.Now)
                      select new BranchLevel()
                      {
                          Id = myCompanyUpgrade.COMPANYSID,
                          Name = "--" + myCompanyUpgrade.COMPANYNAME,
                          Clever = "12" + myCompanyUpgrade.COMPANYSID,
                          TaxCode = myCompanyUpgrade.TAXCODE,
                          Address = myCompanyUpgrade.ADDRESS,
                          Delegate = myCompanyUpgrade.DELEGATE,
                          Tell = myCompanyUpgrade.TEL,
                          Fax = myCompany.FAX,
                          Email = myCompany.EMAIL,
                          PersonContact = myCompany.PERSONCONTACT,
                          BankAccount = myCompany.BANKACCOUNT,
                          AccountHolder = myCompany.ACCOUNTHOLDER,
                          BankName = myCompany.BANKNAME,
                          CityId = myCompany.CITYID,
                          TaxDepartmentId = myCompany.TAXDEPARTMENTID,
                          Description = myCompany.DESCRIPTION,
                          Delete = myCompany.DELETED,
                          EmailOfContract = myCompany.EMAILOFCONTRACT,
                      };
            return res;
        }

        public MYCOMPANY GetHO()
        {
            return this.GetMyCompanyActive().Where(m => m.LEVELCUSTOMER == LevelCustomerInfo.HO).FirstOrDefault();
        }

        public MYCOMPANY GetHODiff(long companyIdDiff)
        {
            return this.GetMyCompanyActive().Where(m => m.LEVELCUSTOMER == LevelCustomerInfo.HO && m.COMPANYSID != companyIdDiff).FirstOrDefault();
        }

        public IEnumerable<MYCOMPANY> GetBranchs()
        {
            return this.GetMyCompanyActive().Where(m => m.LEVELCUSTOMER == LevelCustomerInfo.Branch || m.LEVELCUSTOMER == LevelCustomerInfo.HO);
        }

        public decimal MyCompanyUsing(long companyID)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("U_RESULTOUTPUT", typeof(decimal));
            db.SP_COMPANY_USING(companyID, outputparam);
            var result = outputparam.Value.ToDecimal() ?? 0;
            return result;
        }
        //Get InvoiceUse include REPORTCANCELLING
        private IEnumerable<NumberInvoiceUse> GetNumberInvoiceUsed(DateTime datefrom, DateTime dateto)
        {
            var numberUsedByContract = from numberUsed in (from invoice in this.context.Set<INVOICE>()
                                                           where (invoice.CREATEDDATE >= datefrom)
                                                             && !(invoice.CREATEDDATE <= dateto)
                                                             && !(invoice.INVOICENO > 0)
                                                           group invoice by new { invoice.COMPANYID } into g
                                                           select new NumberInvoiceUse()
                                                           {
                                                               CompanyId = g.Key.COMPANYID,
                                                               NumberUse = g.Count(),
                                                           }).Union(
                                       from cancelling in this.context.Set<REPORTCANCELLING>()
                                       join cancellingDetail in this.context.Set<REPORTCANCELLINGDETAIL>()
                                       on cancelling.ID equals cancellingDetail.REPORTCANCELLINGID
                                       where (cancelling.APPROVEDDATE >= datefrom) && !(cancelling.APPROVEDDATE <= dateto)
                                         && cancelling.STATUS == (int)ReportCancellingStatus.Approved
                                       group cancellingDetail by new { cancelling.COMPANYID } into g
                                       select new NumberInvoiceUse()
                                       {
                                           CompanyId = g.Key.COMPANYID,
                                           NumberUse = g.Sum(i => i.NUMBER),
                                       })
                                       group numberUsed by new { numberUsed.CompanyId } into g
                                       select new NumberInvoiceUse()
                                       {
                                           CompanyId = g.Key.CompanyId,
                                           NumberUse = g.Sum(i => i.NumberUse),
                                       };
            return numberUsedByContract;
        }
        //Get InvoiceUse of Invoice
        private IEnumerable<NumberInvoiceUse> GetNumberInvoiceUsed2(DateTime datefrom, DateTime dateto)
        {
            var numberUsedByContract = from numberUsed in (from invoice in this.context.Set<INVOICE>()
                                                           where (invoice.CREATEDDATE >= datefrom)
                                                             && !(invoice.CREATEDDATE <= dateto)
                                                             && !(invoice.INVOICENO > 0)
                                                           group invoice by new { invoice.COMPANYID })
                                       select new NumberInvoiceUse()
                                       {
                                           CompanyId = numberUsed.Key.COMPANYID,
                                           NumberUse = numberUsed.Count(),
                                       };
            return numberUsedByContract;
        }

        public MYCOMPANY GetLevelCustomer(long currentCompanyId)
        {
            return this.GetMyCompanyActive().Where(p => p.COMPANYSID == currentCompanyId).FirstOrDefault();
        }
        public MYCOMPANY GetByTaxCode(string taxCode)
        {
            return GetMyCompanyActive().Where(p => p.TAXCODE == taxCode).FirstOrDefault();
        }
        public MYCOMPANY GetByNameTaxCode(string taxCode, string customerName)
        {
            var companies = GetMyCompanyActive().Where(p => p.BRANCHID == taxCode);
            if (customerName != null)
            {
                companies = companies.Where(p => p.COMPANYNAME.ToUpper() == customerName.ToUpper());
            }
            return companies.FirstOrDefault();
        }
        public MYCOMPANY GetByName(string customerName)
        {
            long? coreBranchId = long.Parse(customerName);
            return GetMyCompanyActive().Where(p => p.COREBRID == coreBranchId || p.BRANCHID == customerName).FirstOrDefault();
        }

        public MYCOMPANY GetByBranchId(string branchId)
        {
            //var listCompany = GetMyCompanyActive().ToList();
            //MYCOMPANY getData = listCompany.Where(p => p.BRANCHID == branchId.Trim()).FirstOrDefault();
            //return getData;
            var Company = GetMyCompanyActive().FirstOrDefault(p => p.BRANCHID == branchId.Trim());
            return Company;
        }

        public IEnumerable<MYCOMPANY> GetListByCompanyId(long? companyId)
        {
            return dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
        }
    }


}
