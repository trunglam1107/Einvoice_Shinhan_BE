using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class CustomerBO : ICustomerBO
    {
        #region Fields, Properties
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly IMyCompanyUpgradeRepository myCompanyUpgradeRepository;
        private readonly ICityRepository cityRepository;
        private readonly ITaxDepartmentRepository taxDepartmentRepository;
        private readonly IDbTransactionManager transaction;
        private readonly EmailConfig emailConfig;
        private readonly PrintConfig config;
        private const int MaxLevelAgencies = 3;

        #endregion

        #region Contructor
        public CustomerBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.myCompanyUpgradeRepository = repoFactory.GetRepository<IMyCompanyUpgradeRepository>();
            this.cityRepository = repoFactory.GetRepository<ICityRepository>();
            this.taxDepartmentRepository = repoFactory.GetRepository<ITaxDepartmentRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }
        public CustomerBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.myCompanyUpgradeRepository = repoFactory.GetRepository<IMyCompanyUpgradeRepository>();
            this.cityRepository = repoFactory.GetRepository<ICityRepository>();
            this.taxDepartmentRepository = repoFactory.GetRepository<ITaxDepartmentRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.emailConfig = emailConfig;
            this.config = config;
        }

        #endregion

        #region Methods


        public IEnumerable<MYCOMPANY> GetList()
        {
            return this.myCompanyRepository.GetList();
        }

        #endregion

        public IEnumerable<CustomerInfo> FilterCustomer(ConditionSearchCustomer condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var customers = this.myCompanyRepository.FilterCustomer(condition).AsQueryable()
                .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            return customers.Select(p => new CustomerInfo(p,null,null,p.MYCOMPANY2));
        }

        public long CountFillterCustomer(ConditionSearchCustomer condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.myCompanyRepository.FilterCustomer(condition).Count();
        }

        public ResultCode Create(CustomerInfo customerInfo)
        {
            //return CreateCustomer(customerInfo);
            try
            {
                transaction.BeginTransaction();
                CreateCustomer(customerInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;

        }

        public ResultCode Update(long id, CustomerInfo customerInfo)
        {
            UpdateCustomer(id, customerInfo);
            return ResultCode.NoError;
        }

        public ResultCode Delete(long id, long companyId, string userLevel)
        {
            var isCompanyUsing = this.myCompanyRepository.MyCompanyUsing(id);
            if (isCompanyUsing > 0)
            {
                throw new BusinessLogicException(ResultCode.CompanyIsUsing, $"Cannot delete record because this customer is using! (Error store procedure code: {isCompanyUsing}");
            }

            MYCOMPANY currentCompany = GetCompany(id);
            return myCompanyRepository.Delete(currentCompany) ? ResultCode.NoError : ResultCode.UnknownError;
        }
        //add import export Excel START
        public ResultImportSheet ImportData(string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportCustomer.SheetNames);
            DataTable dtCustomer = importExcel.GetBySheetName(ImportCustomer.SheetName, ImportCustomer.ColumnImport, ref rowError);
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtCustomer == null || dtCustomer.Rows.Count == 0)
            {
                listResultImport.ErrorCode = ResultCode.FileUploadImportEmpty;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            try
            {
                transaction.BeginTransaction();
                listResultImport = ProcessData(dtCustomer);
                if (listResultImport.ErrorCode == ResultCode.NoError)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw;
            }

            return listResultImport;
        }
        private ResultImportSheet ProcessData(DataTable dtDistintc)
        {
            var listResultImport = new ResultImportSheet();
            int rowIndex = 1;


            foreach (DataRow row in dtDistintc.Rows)
            {
                listResultImport.RowError.AddRange(Validate(row, rowIndex));
                rowIndex++;
                var branchs = this.myCompanyRepository.GetAll().Count(m => m.BRANCHID == row[ImportCustomer.BrandId].ToString());
                if (branchs > 0)
                {
                    continue;
                }

                listResultImport.RowError.AddRange(ExcecuteData(row));


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

            if ((rowIndex - 1) == 0)
            {
                listResultImport.ErrorCode = ResultCode.FileUploadImportEmpty;
                listResultImport.Message = MsgApiResponse.DataInvalid;
            }

            return listResultImport;
        }



        private ListError<ImportRowError> Validate(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportCustomer.CustomerName, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportCustomer.BrandId, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportCustomer.TaxCode, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportCustomer.Address, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportCustomer.LevelCustomer, rowIndex));

            //Truong hop dong rong

            listError.Add(ValidDataMaxLength(row, ImportCustomer.MaxLength50, ImportCustomer.LevelCustomer, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportCustomer.MaxLength50, ImportCustomer.TaxCode, rowIndex));

            listError.Add(ValidDataNotExists(row, ImportCustomer.Company2, rowIndex));
            listError.Add(ValidDataNotExists(row, ImportCustomer.City, rowIndex));
            listError.Add(ValidDataNotExists(row, ImportCustomer.TaxDepartment, rowIndex));

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

        private ImportRowError ValidDataNotExists(DataRow row, string columnName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {

                if (row[columnName].ToString().Trim().IsNotNullOrEmpty())
                {
                    switch (columnName)
                    {
                        case ImportCustomer.Company2:
                            this.ValidDataNotExistsCaseCompany(row, columnName, rowIndex);
                            break;
                        case ImportCustomer.City:
                            this.ValidDataNotExistsCaseCity(row, columnName, rowIndex);
                            break;
                        case ImportCustomer.TaxDepartment:
                            this.ValidDataNotExistsCaseTaxDepartment(row, columnName, rowIndex);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    row[columnName] = null;
                }
                return errorColumn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ImportRowError ValidDataNotExistsCaseCompany(DataRow row, string columnName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            var rslCompanyExists = this.myCompanyRepository.GetByName(row[columnName].ToString().Trim());
            if (rslCompanyExists == null)
            {
                row[columnName] = null;
                errorColumn = new ImportRowError(ResultCode.ImportColumnIsNotExist, columnName, rowIndex);
            }
            else
            {
                row[columnName] = rslCompanyExists.COMPANYSID;
            }
            return errorColumn;
        }

        private ImportRowError ValidDataNotExistsCaseCity(DataRow row, string columnName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            var rslCityExists = this.cityRepository.GetByName(row[columnName].ToString().Trim());
            if (rslCityExists == null)
            {
                row[columnName] = null;
                errorColumn = new ImportRowError(ResultCode.ImportColumnIsNotExist, columnName, rowIndex);
            }
            else
            {
                row[columnName] = rslCityExists.ID;
            }
            return errorColumn;
        }

        private ImportRowError ValidDataNotExistsCaseTaxDepartment(DataRow row, string columnName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            if (row[ImportCustomer.City].ToInt() != null)
            {
                var rslTaxDepartmentExists = this.taxDepartmentRepository.FilterTaxDeparment((int)row[ImportCustomer.City].ToInt(), row[columnName].ToString().Trim());
                if (rslTaxDepartmentExists.IsNullOrEmpty())
                {
                    row[columnName] = null;
                    errorColumn = new ImportRowError(ResultCode.ImportColumnIsNotExist, columnName, rowIndex);
                }
                else
                {
                    row[columnName] = rslTaxDepartmentExists.FirstOrDefault().ID;
                }
            }
            else
            {
                row[columnName] = null;
                errorColumn = new ImportRowError(ResultCode.ImportColumnIsNotExist, columnName, rowIndex);
            }
            return errorColumn;
        }

        private ListError<ImportRowError> ExcecuteData(DataRow row)
        {
            var listError = new ListError<ImportRowError>();
            //check tồn tại chi nhánh với BRANCHID và Tên chi nhánh
            var customer = FilterCustomer(row[ImportCustomer.BrandId].Trim(), row[ImportCustomer.CustomerName].Trim());

            if (customer == null)
            {
                listError.AddRange(InsertCustomer(row));
            }
            else
            {
                listError.AddRange(UpdateCustomer(customer, row));
            }

            return listError;

        }

        private MYCOMPANY FilterCustomer(string taxCode, string customerName)
        {
            return this.myCompanyRepository.GetByNameTaxCode(taxCode, customerName);
        }

        private ListError<ImportRowError> UpdateCustomer(MYCOMPANY currentCustomer, DataRow row)
        {
            MYCOMPANY currentCompany = GetCompany(currentCustomer.COMPANYSID);
            currentCompany.CopyDataOfDataRow(row);
            try
            {
                this.myCompanyRepository.Update(currentCompany);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new ListError<ImportRowError>();
        }

        private void UpdateCustomer(long id, CustomerInfo customerInfo)
        {
            this.ValidateUpdateCustomer(customerInfo);

            var companyHasCurrentBranch = this.myCompanyRepository.FirstOrDefault(p => p.BRANCHID == customerInfo.BranchId);
            if (!companyHasCurrentBranch.IsNullOrEmpty() && !companyHasCurrentBranch.COMPANYSID.Equals(id))
            {
                throw new BusinessLogicException(ResultCode.CompanyBranchExists, "Branch Id exists!");
            }

            MYCOMPANY currentCompany = GetCompany(id);
            if (!currentCompany.TAXCODE.IsEquals(customerInfo.TaxCode)
               && this.myCompanyRepository.ContainTaxCode(customerInfo.CompanyId ?? 0, customerInfo.TaxCode, CustomerLevel.Customer))
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                    string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", customerInfo.TaxCode));
            }

            if (currentCompany.EMAIL.IsNotNullOrEmpty() && !currentCompany.EMAIL.IsEquals(customerInfo.Email)        // Email is changed
               && this.myCompanyRepository.ContainEmail(customerInfo.CompanyId ?? 0, customerInfo.Email, CustomerLevel.Customer)) // New Email is existed
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsEmail,
                                  string.Format("Create Company failed because company with Email = [{0}] is exist.", customerInfo.Email));
            }

            currentCompany.CopyData(customerInfo);
            // Branch thì lấy CompanyId cha là của HO
            //if (customerInfo.LevelCustomer == LevelCustomerInfo.Branch)
            //{
            //    var companyHO = this.myCompanyRepository.GetHO();
            //    if (companyHO == null)
            //    {
            //        throw new BusinessLogicException(ResultCode.CompanyHONotExists, "Head office not exists!");
            //    }
            //    currentCompany.COMPANYID = companyHO.COMPANYSID;
            //}
            try
            {
                this.transaction.BeginTransaction();
                this.myCompanyRepository.Update(currentCompany);
                // Nếu là HO thì thay thế HO
                //if (customerInfo.LevelCustomer == LevelCustomerInfo.HO)
                //{
                //    this.ChangeCompanyHO(currentCompany.COMPANYSID);
                //    var oldCompanyHO = this.myCompanyRepository.GetHODiff(currentCompany.COMPANYSID);
                //    if (oldCompanyHO != null)
                //    {
                //        oldCompanyHO.LEVELCUSTOMER = LevelCustomerInfo.Branch;
                //        oldCompanyHO.COMPANYID = currentCompany.COMPANYSID;
                //        currentCompany.COMPANYID = null;
                //        this.myCompanyRepository.Update(oldCompanyHO);
                //    }
                //}
                this.transaction.Commit();
            }
            catch
            {
                this.transaction.Rollback();
                throw;
            }
        }

        private ListError<ImportRowError> InsertCustomer(DataRow row)
        {
            var myCompany = new MYCOMPANY();

            if (row[ImportCustomer.LevelCustomer].ToString() == LevelCustomerInfo.TransactionOffice)
            {
                if (row[ImportCustomer.Company2].IsNullOrEmpty())
                {
                    throw new BusinessLogicException(ResultCode.ImportPGDNotEmpty, "CompanyId exists!");
                }
            }

            myCompany.CopyDataOfDataRow(row);
            myCompany.LEVELAGENCIES = MaxLevelAgencies;

            try
            {
                this.myCompanyRepository.Insert(myCompany);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new ListError<ImportRowError>();
        }

        public ExportFileInfo DownloadDataCustomer(ConditionSearchCustomer condition)
        {
            var dataRport = new CustomerExport(condition.CurrentUser?.Company);
            var customers = this.myCompanyRepository.FilterCustomer(condition);
            dataRport.Items = customers.Select(p => new CustomerInfo(p, p.CITY, p.TAXDEPARTMENT, p.MYCOMPANY2)).ToList();
            ExportCustomerView export = new ExportCustomerView(dataRport, config, emailConfig);
            return export.ExportFile();
        }
        //add import export Excel END
        public CustomerInfo GetCustomerInfo(long companyId)
        {
            var companyInfo = GetCompany(companyId);
            var companyUpgradeInfo = this.myCompanyUpgradeRepository.GetByCompanySId(companyId);
            var customerInfo = new CustomerInfo(companyInfo, companyInfo.CITY, companyInfo.TAXDEPARTMENT, null, companyUpgradeInfo);
            return customerInfo;
        }

        public CustomerInfo GetCustomerInfoForBB(long companyId)
        {
            var companyInfo = GetCompany(companyId);

            var companyUpgradeInfo = this.myCompanyUpgradeRepository.GetByCompanySId(companyId);

            if (companyInfo.LEVELCUSTOMER == "PGD")
            {
                companyInfo = GetCompany((long)companyInfo.COMPANYID);
            }

            var customerInfo = new CustomerInfo(companyInfo, companyInfo.CITY, companyInfo.TAXDEPARTMENT, null, companyUpgradeInfo);
            return customerInfo;
        }

        private void CreateCustomer(CustomerInfo customerInfo)
        {
            this.ValidateCreateCustomer(customerInfo);
            var branchs = this.myCompanyRepository.GetAll().Count(m => m.BRANCHID == customerInfo.BranchId);
            if (branchs > 0)
            {
                throw new BusinessLogicException(ResultCode.CompanyBranchExists, "Brand Id exists!");
            }
            var myCompany = new MYCOMPANY();
            myCompany.CopyData(customerInfo);
            myCompany.BRANCHID = customerInfo.BranchId;
            myCompany.LEVELAGENCIES = MaxLevelAgencies;
            // Branch thì lấy CompanyId cha là của HO
            if (customerInfo.LevelCustomer == LevelCustomerInfo.Branch)
            {
                var companyHO = this.myCompanyRepository.GetHO();
                if (companyHO == null)
                {
                    throw new BusinessLogicException(ResultCode.CompanyHONotExists, "Head office not exists!");
                }
                myCompany.COMPANYID = companyHO.COMPANYSID;
            }
            //ResultCode res;
            //try
            //{
            //this.transaction.BeginTransaction();
            //res = this.myCompanyRepository.Insert(myCompany) ? ResultCode.NoError : ResultCode.UnknownError;
            // Nếu là HO thì thay thế HO
            this.myCompanyRepository.Insert(myCompany);
            if (customerInfo.LevelCustomer == LevelCustomerInfo.HO)
            {
                this.ChangeCompanyHO(myCompany.COMPANYSID);
                var oldCompanyHO = this.myCompanyRepository.GetHODiff(myCompany.COMPANYSID);
                if (oldCompanyHO != null)
                {
                    oldCompanyHO.LEVELCUSTOMER = LevelCustomerInfo.Branch;
                    oldCompanyHO.COMPANYID = myCompany.COMPANYSID;
                    this.myCompanyRepository.Update(oldCompanyHO);
                }
            }
            //this.transaction.Commit();
            //}
            //catch
            //{
            //    this.transaction.Rollback();
            //    throw;
            //}
            //return res;
        }

        private void ChangeCompanyHO(long newCompanyId)
        {
            var branchs = this.myCompanyRepository.GetAll().Where(m => m.LEVELCUSTOMER == LevelCustomerInfo.Branch);
            foreach (var branch in branchs)
            {
                branch.COMPANYID = newCompanyId;
                this.myCompanyRepository.Update(branch);
            }
        }

        private MYCOMPANY GetCompany(long id)
        {
            var company = this.myCompanyRepository.GetById(id);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }

            return company;
        }

        public IEnumerable<CustomerInfo> FilterCustomerByContructType(ConditionSearchCustomer condition, ContractType contractType, int skip = int.MinValue, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            string customerType = (contractType == ContractType.Agencies) ? CustomerLevel.Sellers : CustomerLevel.Customer;
            var customers = this.myCompanyRepository.FilterCustomer(condition, customerType).AsQueryable()
                .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();

            return customers.Select(p => new CustomerInfo(p));
        }

        public long CountFilterCustomerByContructType(ConditionSearchCustomer condition, ContractType contractType)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            string customerType = (contractType == ContractType.Agencies) ? CustomerLevel.Sellers : CustomerLevel.Customer;
            return this.myCompanyRepository.FilterCustomer(condition, customerType).Count();
        }

        public IEnumerable<Branch> GetBranchs()
        {
            var branchs = this.myCompanyRepository.GetBranchs();
            var res = branchs.Select(b => new Branch
            {
                Id = b.COMPANYSID,
                Name = b.COMPANYNAME,
                TaxCode = b.TAXCODE,
            });
            return res;
        }

        public IEnumerable<BranchLevel> GetListBranchs(long companyId, int isGetAll)
        {
            var branchs = this.myCompanyRepository.GetListBranchs(companyId, isGetAll).OrderBy(p => p.Clever).ToList();
            return branchs;
        }
        public IEnumerable<BranchLevel> GetListBranchs_V2(long companyId, int isGetAll)
        {
            var branchs = this.myCompanyRepository.GetListBranchs_V2(companyId, isGetAll).OrderBy(p => p.Clever).ToList();
            return branchs;
        }
        public IEnumerable<BranchLevel> GetListBranchs_2(long companyId)
        {
            var branchs = this.myCompanyRepository.GetListBranchs_2(companyId).OrderBy(p => p.Clever).ToList();
            return branchs;
        }

        public IEnumerable<BranchLevel> GetListAllBranchs()
        {
            var branchs = this.myCompanyRepository.GetListAllBranchs().OrderBy(p => p.Clever).ToList();
            return branchs;
        }
        public IEnumerable<BranchLevel> GetListBranchsOnly(long companyId, int isGetAll)
        {
            var branchs = this.myCompanyRepository.GetListBranchsOnly(companyId, isGetAll).OrderBy(p => p.Clever).ToList();
            return branchs;
        }

        private void ValidateCreateCustomer(CustomerInfo customerInfo)
        {
            if (customerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (!LevelCustomerInfo.LevelCustomer.Contains(customerInfo.LevelCustomer))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!customerInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            bool isExitTaxCode = this.myCompanyRepository.ContainTaxCode(customerInfo.CompanyId ?? 0, customerInfo.TaxCode, CustomerLevel.Customer);
            if (isExitTaxCode)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                  string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", customerInfo.TaxCode));
            }
        }

        private void ValidateUpdateCustomer(CustomerInfo customerInfo)
        {
            if (customerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (!LevelCustomerInfo.LevelCustomer.Contains(customerInfo.LevelCustomer))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!customerInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }
        }
    }
}