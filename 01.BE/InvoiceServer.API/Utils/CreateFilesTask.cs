using InvoiceServer.Business.BL;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Common.TaskScheduler;
using InvoiceServer.Business.Cache;
#pragma warning disable CS0105 // The using directive for 'InvoiceServer.Business.Models' appeared previously in this namespace
using InvoiceServer.Business.Models;
#pragma warning restore CS0105 // The using directive for 'InvoiceServer.Business.Models' appeared previously in this namespace
#pragma warning disable CS0105 // The using directive for 'InvoiceServer.Common' appeared previously in this namespace
using InvoiceServer.Common;
#pragma warning restore CS0105 // The using directive for 'InvoiceServer.Common' appeared previously in this namespace
#pragma warning disable CS0105 // The using directive for 'InvoiceServer.Common.Constants' appeared previously in this namespace
using InvoiceServer.Common.Constants;
#pragma warning restore CS0105 // The using directive for 'InvoiceServer.Common.Constants' appeared previously in this namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using InvoiceServer.API.Config;
using InvoiceServer.API.Business;
using InvoiceServer.Common.Tasks;

namespace InvoiceServer.API.Utils
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask'
	public class CreateFilesTask : BackgroundQueueBase, IBackgroundQueue
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Company'
		public MYCOMPANY Company { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Company'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.InvoiceSamples'
        public List<INVOICESAMPLE> InvoiceSamples { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.InvoiceSamples'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Templates'
        public List<INVOICETEMPLATE> Templates { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Templates'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Invoices'
        public List<INVOICE> Invoices { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Invoices'
        private readonly IInvoiceBO invoiceBO;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.CreateFilesTask(IBOFactory, UserSessionInfo, MYCOMPANY, List<INVOICESAMPLE>, List<INVOICETEMPLATE>, List<INVOICE>)'
        public CreateFilesTask(IBOFactory boFactory, UserSessionInfo CurrentUser, MYCOMPANY company, List<INVOICESAMPLE> invoiceSamples, List<INVOICETEMPLATE> templates, List<INVOICE> invoices)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.CreateFilesTask(IBOFactory, UserSessionInfo, MYCOMPANY, List<INVOICESAMPLE>, List<INVOICETEMPLATE>, List<INVOICE>)'
        {
           
            //var config = new SessionConfig()
            //{
            //    LoginSecretKey = Config.ApplicationSetting.Instance.LoginSecretKey,
            //    SessionTimeout = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.SessionTimeout),
            //    CreateTokenRandom = Config.ApplicationSetting.Instance.EnableCreateTokenRandom,
            //    SessionResetPasswordExpire = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.ResetPasswordTimeOut),
            //};
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = "D:\\store\\shinhan";
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice, fullPathFileExportData);

            try
            {
                if (CurrentUser.Company != null)
                {
                    config.BuildAssetByCompany(CurrentUser.Company);
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //logger.Error(this.CurrentUser.UserId, ex);
            }
            config.FullPathFolderTemplateInvoice = Config.ApplicationSetting.Instance.TemplateInvoiceFolder;
            
            string location = HttpContext.Current == null ? null : HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath);
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = location,
            };
           
            if (HttpContext.Current == null)
            {
                this.invoiceBO = boFactory.GetBO<IInvoiceBO>(config, emailConfig);
            }
            else
            {
                this.invoiceBO = boFactory.GetBO<IInvoiceBO>(config, emailConfig, CurrentUser);
            }

            this.Company = company;
            this.InvoiceSamples = invoiceSamples;
            this.Templates = templates;
            this.Invoices = invoices;

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.SetParams(MYCOMPANY, List<INVOICESAMPLE>, List<INVOICETEMPLATE>, List<INVOICE>)'
        public void SetParams(MYCOMPANY company, List<INVOICESAMPLE> invoiceSamples, List<INVOICETEMPLATE> templates, List<INVOICE> invoices)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.SetParams(MYCOMPANY, List<INVOICESAMPLE>, List<INVOICETEMPLATE>, List<INVOICE>)'
        {
            this.Company = company;
            this.InvoiceSamples = invoiceSamples;
            this.Templates = templates;
            this.Invoices = invoices;
           

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Execute()'
        public override void Execute()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CreateFilesTask.Execute()'
        {

            if (this.IsWaiting(true)) return;
            try
            {
                //this.invoiceBO.JobCreateFile(this.Invoices, this.InvoiceSamples, this.Templates, this.Company);

                return; // success
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
               // WebLog.Log.Error("PaidQueue.Execute: " + this.ToJson(), ex.Message, this.FileLog());
            }

            
        }
    }
}
