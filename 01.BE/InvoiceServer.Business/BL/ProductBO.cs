using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class ProductBO : IProductBO
    {
        #region Fields, Properties
        private readonly IProductRepository productRepository;
        private readonly ITaxRepository taxRepository;
        private readonly IDbTransactionManager transaction;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly PrintConfig config;
        private readonly IUnitListRepository unitListRepository;
        private readonly List<UNITLIST> unitListsImport;
        private readonly IInvoiceRepository invoiceRepository;

        #endregion

        #region Contructor

        public ProductBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.productRepository = repoFactory.GetRepository<IProductRepository>();
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.config = config;
            this.unitListRepository = repoFactory.GetRepository<IUnitListRepository>();
            this.unitListsImport = new List<UNITLIST>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
        }

        #endregion

        #region Methods

        public ResultCode Create(ProductInfo productInfo)
        {
            if (productInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!productInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            if (IsExistedCode(productInfo.CompanyId, productInfo.ProductCode, true, 0))
            {
                throw new BusinessLogicException(ResultCode.ExistedProductCode, string.Format("Product code  [{0}] is existed", productInfo.ProductCode));
            }
            if (IsExistsNameUnit(productInfo.ProductName, productInfo.UnitId, true, 0))
            {
                throw new BusinessLogicException(ResultCode.ExistedNameUnit, string.Format("Product name  [{0}] Unit [{1}] is existed", productInfo.ProductName, productInfo.Unit));
            }
            if (IsExistsDifferTax(productInfo.CompanyId, productInfo.ProductName, productInfo.TaxId, true, 0))
            {
                throw new BusinessLogicException(ResultCode.ExistedDifferTax, string.Format("Tax group of products [{0}] is heterogeneous", productInfo.ProductName));
            }

            var product = new PRODUCT();
            product.CopyData(productInfo);
            product.UNIT = GetUnit(productInfo.UnitId);
            product.CREATEDDATE = DateTime.Now;
            product.CREATEDBY = productInfo.UserAction;
            return this.productRepository.Insert(product) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode Update(long id, ProductInfo productInfo)
        {
            if (productInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!productInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            if (IsExistedCode(productInfo.CompanyId, productInfo.ProductCode, false, productInfo.Id))
            {
                throw new BusinessLogicException(ResultCode.ExistedProductCode, string.Format("Product code  [{0}] is existed", productInfo.ProductCode));
            }
            if (IsExistsNameUnit(productInfo.ProductName, productInfo.UnitId, false, productInfo.Id))
            {
                throw new BusinessLogicException(ResultCode.ExistedNameUnit, string.Format("Product name  [{0}] Unit [{1}] is existed", productInfo.ProductName, productInfo.Unit));
            }
            if (IsExistsDifferTax(productInfo.CompanyId, productInfo.ProductName, productInfo.TaxId, false, productInfo.Id))
            {
                throw new BusinessLogicException(ResultCode.ExistedDifferTax, string.Format("Tax group of products [{0}] is heterogeneous", productInfo.ProductName));
            }

            PRODUCT currentProduct = GetProduct(id, productInfo.CompanyId);
            currentProduct.CopyData(productInfo);
            currentProduct.UNIT = GetUnit(currentProduct.UNITID);
            currentProduct.UPDATEDDATE = DateTime.Now;
            currentProduct.UPDATEDBY = productInfo.UserAction;
            return this.productRepository.Update(currentProduct) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private bool IsExistedCode(long companyId, string Code, bool isCreate, long id)
        {
            return this.productRepository.ContainCode(companyId, Code, isCreate, id);
        }

        private bool IsExistsNameUnit(string name, long unitId, bool isCreate, long id)
        {
            return this.productRepository.ContainNameUnitId(name, unitId, isCreate, id);
        }

        private bool IsExistsDifferTax(long companyId, string name, long taxId, bool isCreate, long id)
        {
            return this.productRepository.ExistsDifferTax(companyId, name, taxId, isCreate, id);
        }

        private string GetUnit(long? id)
        {
            var unit = this.unitListRepository.GetById(id ?? 0);

            return unit?.NAME;
        }

        public ResultCode Delete(long id, long? companyId)
        {
            PRODUCT currentProduct = GetProduct(id, companyId);
            bool productIsUsed = ProductIsUsed(currentProduct.ID);
            if (productIsUsed)
            {
                throw new BusinessLogicException(ResultCode.DataIsUsed, "Data is used on the invoice can not deleted");
            }

            currentProduct.DELETED = true;
            return this.productRepository.Update(currentProduct) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ProductInfo GetProductInfo(long id)
        {
            PRODUCT product = GetProduct(id);
            return new ProductInfo(product);
        }

        public IEnumerable<ProductInfo> Filter(ConditionSearchProduct condition, int skip = 0, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var projects = this.productRepository.FilterProducts(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();
            return projects.Select(p => new ProductInfo(p));
        }

        public long Count(ConditionSearchProduct condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.productRepository.FilterProducts(condition).Count();
        }

        public bool MyProductUsing(long id)
        {
            return productRepository.MyProductUsing(id);
        }

        #endregion

        #region Private Methods

        private PRODUCT GetProduct(long id, long? companyId)
        {
            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            PRODUCT product = GetProduct(id);

            return product;
        }

        private PRODUCT GetProduct(long id)
        {
            PRODUCT product = this.productRepository.GetById(id);
            if (product == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return product;
        }

        #endregion

        public ResultImportSheet ImportData(string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportProduct.SheetNames);
            DataTable dtProducts = importExcel.GetBySheetName(ImportProduct.SheetName, ImportProduct.ColumnImport, ref rowError);
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtProducts == null || dtProducts.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                transaction.BeginTransaction();
                listResultImport = ProcessData(companyId, dtProducts);
                if (listResultImport.ErrorCode == ResultCode.NoError)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return listResultImport;
        }

        private ResultImportSheet ProcessData(long companyId, DataTable dtDistintc)
        {
            var listResultImport = new ResultImportSheet();
            int rowIndex = 1;
            foreach (DataRow row in dtDistintc.Rows)
            {
                listResultImport.RowError.AddRange(Validate(row, rowIndex, companyId));
                listResultImport.RowError.AddRange(ExcecuteData(row, rowIndex, companyId));
                rowIndex++;
            }

            this.unitListRepository.AddList(this.unitListsImport);

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

            return listResultImport;
        }

        private ListError<ImportRowError> Validate(DataRow row, int rowIndex, long companyId)
        {
            var listError = new ListError<ImportRowError>();
            listError.Add(ValidDataIsNull(row, ImportProduct.ProductCode, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportProduct.ProductName, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportProduct.UnitCode, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportProduct.Unit, rowIndex));

            listError.Add(ValidDataMaxLength(row, ImportProduct.MaxLength15, ImportProduct.ProductCode, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportProduct.MaxLength10, ImportProduct.UnitCode, rowIndex));
            listError.Add(ValidDataMaxLength(row, ImportProduct.MaxLength50, ImportProduct.Unit, rowIndex));

            listError.Add(ValidateDuplicateUnitName(row, rowIndex, companyId));
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

        private ImportRowError ValidateDuplicateUnitName(DataRow row, int rowIndex, long companyId)
        {
            var unitName = row[ImportProduct.Unit].Trim();
            var unitCode = row[ImportProduct.UnitCode].Trim();
            var listUnitList = this.unitListRepository.GetByName(unitName, companyId);
            if (listUnitList.Any(u => u.CODE != unitCode && u.NAME == unitName))
            {
                return new ImportRowError(ResultCode.ImportDataUnitNameDuplicate, "Unit", rowIndex);
            }

            if (this.unitListsImport.Any(u => u.CODE != unitCode && u.NAME == unitName))
            {
                return new ImportRowError(ResultCode.ImportDataUnitNameDuplicateInFile, "Unit", rowIndex);
            }

            if (this.unitListsImport.Any(u => u.CODE == unitCode && u.NAME != unitName))
            {
                return new ImportRowError(ResultCode.ImportDataOneCodeWithManyNameInFile, "Unit", rowIndex);
            }
            return null;
        }

        private ListError<ImportRowError> ExcecuteData(DataRow row, int rowIndex, long companyId)
        {
            var listError = new ListError<ImportRowError>();
            var product = FilterProduct(row[ImportProduct.ProductCode].Trim());
            var taxName = row[ImportProduct.Tax].Trim();
            var tax = this.taxRepository.GetTax(taxName);
            long taxId = 1;
            if (!tax.IsNullOrEmpty())
            {
                taxId = tax.ID;
            }
            row[ImportProduct.TaxId] = taxId;

            if (product == null)
            {
                listError.AddRange(InsertProduct(row, companyId, rowIndex));
            }
            else
            {
                listError.AddRange(UpdateProduct(product, rowIndex, row, companyId));
            }

            return listError;

        }

        private PRODUCT FilterProduct(string code)
        {
            return this.productRepository.FilterProductCode(code);
        }

        private ListError<ImportRowError> InsertProduct(DataRow row, long companyId, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            var unitList = this.unitListRepository.GetByCode(row[ImportProduct.UnitCode].ToString(), companyId);
            var unitId = unitList == null ? 0 : unitList.ID;
            bool isError = this.productRepository.ContainNameUnitId(row[ImportProduct.ProductName].ToString(), unitId, true, 0);
            if (isError)
            {
                listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportProduct.ProductName, rowIndex, row[ImportProduct.ProductName].ToString()));
            }

            if (listError.Count > 0)
            {
                return listError;
            }

            PRODUCT product = new PRODUCT();

            product.CopyData(row);
            product.UNITID = unitId;

            product.CREATEDDATE = DateTime.Now;
            // Update [UnitList]
            if (unitList != null)
            {
                if (unitList.NAME != product.UNIT)
                {
                    unitList.NAME = product.UNIT;
                }
            }
            else
            {
                unitList = new UNITLIST()
                {
                    CODE = row[ImportProduct.UnitCode].ToString(),
                    NAME = product.UNIT,
                };
            }
            // Add product to list unitlist
            AddProductToUnit(unitList, product);

            return listError;
        }

        private ListError<ImportRowError> UpdateProduct(PRODUCT currentProduct, int rowIndex, DataRow row, long companyId)
        {
            var listError = new ListError<ImportRowError>();
            var unitList = this.unitListRepository.GetByCode(row[ImportProduct.UnitCode].ToString(), companyId);
            var unitId = unitList == null ? 0 : unitList.ID;
            bool isError = this.productRepository.ContainNameUnitId(row[ImportProduct.ProductName].ToString(), unitId, false, currentProduct.ID);
            if (isError)
            {
                listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportProduct.ProductName, rowIndex, row[ImportProduct.ProductName].ToString()));
            }
            if (listError.Count > 0)
            {
                return listError;
            }

            currentProduct.CopyData(row);
            currentProduct.UNITID = unitId;

            // Update [UnitList]
            if (unitList != null)
            {
                if (unitList.NAME != currentProduct.UNIT)
                {
                    unitList.NAME = currentProduct.UNIT;
                }
            }
            else
            {
                unitList = new UNITLIST()
                {
                    CODE = row[ImportProduct.UnitCode].ToString(),
                    NAME = currentProduct.UNIT,
                };
            }
            AddProductToUnit(unitList, currentProduct);

            return listError;
        }

        private void AddProductToUnit(UNITLIST unitList, PRODUCT product)
        {
            var unitQuery = this.unitListsImport.Where(u => u.CODE == unitList.CODE);
            if (unitQuery.Any())
            {
                var unit = unitQuery.FirstOrDefault();
                RemoveDuplicateProduct(unitList, product);
                unit.PRODUCTs.Add(product);
            }
            else
            {
                unitList.PRODUCTs.Add(product);
                RemoveDuplicateProduct(unitList, product);
                this.unitListsImport.Add(unitList);
            }
        }

        private void RemoveDuplicateProduct(UNITLIST unitList, PRODUCT product)
        {
            var productQuery = unitList.PRODUCTs.Where(p => p.PRODUCTCODE == product.PRODUCTCODE);
            if (productQuery.Any())
            {
                var prod = productQuery.FirstOrDefault();
                prod.CopyData(product);
            }
        }

        private bool ProductIsUsed(long id)
        {
            return this.invoiceRepository.ContainProduct(id);
        }

        public ExportFileInfo DownloadDataProduct(ConditionSearchProduct codition, CompanyInfo company)
        {
            var dataRport = new ProductExport(company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(codition.CompanyId ?? 0);
            var products = this.productRepository.FilterProducts(codition);

            dataRport.Items = products.Select(p => new ProductInfo(p)).ToList();

            ExportProductView export = new ExportProductView(dataRport, config, systemSettingInfo);
            return export.ExportFile();
        }
    }
}