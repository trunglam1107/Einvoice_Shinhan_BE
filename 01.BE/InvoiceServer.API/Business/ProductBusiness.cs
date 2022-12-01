using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness'
    public class ProductBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness'
    {
        #region Fields, Properties

        private readonly IProductBO productBO;

        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.ProductBusiness(IBOFactory)'
        public ProductBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.ProductBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.productBO = boFactory.GetBO<IProductBO>(printConfig);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.FillterProduct(out long, string, string, string, string, int, int)'
        public List<ProductInfo> FillterProduct(out long totalRecords, string productCode = null, string productName = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.FillterProduct(out long, string, string, string, string, int, int)'
        {
            var condition = new ConditionSearchProduct(this.CurrentUser, productCode, productName, orderby, orderType);
            totalRecords = this.productBO.Count(condition);
            return this.productBO.Filter(condition, skip, take).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.GetProductDetail(long)'
        public ProductInfo GetProductDetail(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.GetProductDetail(long)'
        {
            var product = this.productBO.GetProductInfo(id);
            return product;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Create(ProductInfo)'
        public ResultCode Create(ProductInfo productInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Create(ProductInfo)'
        {
            if (productInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long? companyId = this.CurrentUser.Company.Id;
            if (!companyId.HasValue || !IsCompanyOfUser(companyId.Value))
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData,
                    string.Format("Create product faild because parameter companyId [{0}] is not of current user", companyId));
            }

            productInfo.CompanyId = companyId.Value;
            productInfo.UserAction = this.CurrentUser.Id;
            return this.productBO.Create(productInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Update(long, ProductInfo)'
        public ResultCode Update(long id, ProductInfo produtInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Update(long, ProductInfo)'
        {
            if (produtInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long? companyId = this.CurrentUser.Company.Id;
            if (!companyId.HasValue || !IsCompanyOfUser(companyId.Value))
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData,
                    string.Format("Create product faild because parameter companyId [{0}] is not of current user", companyId));
            }

            produtInfo.CompanyId = companyId.Value;
            produtInfo.UserAction = this.CurrentUser.Id;
            return this.productBO.Update(id, produtInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.Delete(long)'
        {
            long? companyId = this.CurrentUser.Company.Id;
            return this.productBO.Delete(id, companyId);
        }

        private bool IsCompanyOfUser(long companyId)
        {
            bool result = true;
            long? idCompanyOfUser = this.CurrentUser.Company.Id;
            if (!idCompanyOfUser.HasValue ||
                idCompanyOfUser.Value != companyId)
            {
                result = false;
            }

            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.ImportData(FileImport)'
        public ResultImportSheet ImportData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.ImportData(FileImport)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }

            string fullPathfile = SaveFileImport(fileImport);
            long companyId = GetCompanyIdOfUser();
            return this.productBO.ImportData(fullPathfile, companyId);
        }

        private static string SaveFileImport(FileImport fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileName = string.Format(@"{0}\{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport.Data));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.DownloadDataProduct(string, string)'
        public ExportFileInfo DownloadDataProduct(string code, string name)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.DownloadDataProduct(string, string)'
        {
            var condition = new ConditionSearchProduct(this.CurrentUser, code, name, name, "ASC");
            ExportFileInfo fileInfo = this.productBO.DownloadDataProduct(condition, this.CurrentUser.Company);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.MyProductUsing(long)'
        public MyUsingModel MyProductUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductBusiness.MyProductUsing(long)'
        {
            var myusing = new MyUsingModel();
            myusing.isUsing = productBO.MyProductUsing(id);
            return myusing;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        #endregion
    }
}