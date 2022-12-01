using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness'
    public class SignatureBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness'
    {
        #region Fields, Properties

        private readonly ISignatureBO signatureBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.SignatureBusiness(IBOFactory)'
        public SignatureBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.SignatureBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderExportTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.ExportTemplateFilePath),
            };
            this.signatureBO = boFactory.GetBO<ISignatureBO>(printConfig, emailConfig);
        }

        #endregion Contructor

        #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.FillterCA(out long, string, long?, int, int)'
        public IEnumerable<SignatureInfo> FillterCA(out long totalRecords, string taxCode = null, long? branch = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.FillterCA(out long, string, long?, int, int)'
        {
            var condition = new ConditionSearchCA(taxCode, branch);
            totalRecords = this.signatureBO.CountFillterCA(condition);
            var signatures = this.signatureBO.FilterCA(condition, skip, take);
            return signatures;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.CreateOrUpdate(SignatureInfo)'
        public ResultCode CreateOrUpdate(SignatureInfo signatureInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.CreateOrUpdate(SignatureInfo)'
        {
            if (signatureInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode result = this.signatureBO.CreateOrUpdate(signatureInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetList()'
        public IEnumerable<SignatureInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetList()'
        {
            long companyId = CurrentUser.Company.Id ?? 0;
            var signatureInfos = this.signatureBO.GetList(companyId);
            return signatureInfos;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSignatureInfo(long)'
        public SignatureInfo GetSignatureInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSignatureInfo(long)'
        {
            return this.signatureBO.GetSignatureInfo(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.Delete(long)'
        {
            return this.signatureBO.Delete(id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSerial(SignatureInfo)'
        public string GetSerial(SignatureInfo signatureInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSerial(SignatureInfo)'
        {
            return this.signatureBO.GetSerialNumberByPassword(signatureInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetTypeSign()'
        public bool GetTypeSign()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetTypeSign()'
        {
            return this.signatureBO.GetTypeSign();
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSerialBySlot(long, string)'
        public string GetSerialBySlot(long slotId, string password)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSerialBySlot(long, string)'
        {
            return this.signatureBO.GetSerialBySlot(slotId, password);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSlots(string)'
        public string GetSlots(string password)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetSlots(string)'
        {
            return this.signatureBO.GetSlots(password);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetBySlot(int, string)'
        public bool GetBySlot(int slot, string cert)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetBySlot(int, string)'
        {
            return this.signatureBO.GetBySlot(slot, cert);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetByCompanyId(long)'
        public bool GetByCompanyId(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureBusiness.GetByCompanyId(long)'
        {
            return this.signatureBO.GetByCompanyId(companyId);
        }
        public ExportFileInfo DownloadDataSignature(ConditionSearchCA condition)
        {
            return this.signatureBO.DownloadDataSignature(condition);
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