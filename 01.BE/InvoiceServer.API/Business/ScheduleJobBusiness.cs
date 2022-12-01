using InvoiceServer.API.Helper;
using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
    public class ScheduleJobBusiness : BaseBusiness
    {
        private readonly IReleaseInvoiceBO releaseInvoiceBO;
        private readonly IInvoiceBO invoiceBO;
        private readonly IQueueCreateFileInvoiceBO queueCreateBO;
        public static readonly ScheduleJobBusiness Instance = new ScheduleJobBusiness();

        public ScheduleJobBusiness()
        {
            var boFactory = new BOFactory(DbContextManager.GetContext());

            EmailConfig emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = System.Web.Hosting.HostingEnvironment.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath)
            };
            var printConfig = GetPrintConfig();
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
            this.releaseInvoiceBO = boFactory.GetBO<IReleaseInvoiceBO>(printConfig);
            this.queueCreateBO = boFactory.GetBO<IQueueCreateFileInvoiceBO>();

            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = System.Web.Hosting.HostingEnvironment.MapPath(Config.ApplicationSetting.Instance.FolderAssetOfCompany)
            };

            boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
        }

        public void ClearRecuringJob()
        {
            BackgroundJobHelper.ClearRecuringJob(ScheduleJobInfo.DailyCheckUserLoginJobId);
        }

        public void SendEmailNoticeReleaseInvoice()
        {
            var listEmail = this.releaseInvoiceBO.SendNotificeReleaseInvoice(new List<int> { (int)ReleaseInvoiceStatus.ProcessSendEmail, (int)ReleaseInvoiceStatus.SendEmailError });
            List<ResultRelease> result = new List<ResultRelease>();
            foreach (var item in listEmail)
            {
                result.Add(ProcessSendEmail(item));
            }

            releaseInvoiceBO.UpdateProcessSendEmail(result);
        }

        private ResultRelease ProcessSendEmail(SendEmailRelease item)
        {
            ResultRelease result = new ResultRelease(item.ReleaseId.Value, item.ReleaseDetailId);
            return result;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathFileAsset = System.Web.Hosting.HostingEnvironment.MapPath(Config.ApplicationSetting.Instance.FolderAssetOfCompany);
            PrintConfig config = new PrintConfig(fullPathFileAsset);
            config.FullPathFolderTemplateInvoice = System.Web.Hosting.HostingEnvironment.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

        public void CreateFileInvoicePdf()
        {
            var queueCreateFiles = GetQueueCreateFile();
            List<long> invoicesId = queueCreateFiles.Select(p => p.InvoiceId).ToList();
            UpdateQueueCreateFile(invoicesId, QueueCreateFileStatus.Creating);
            foreach (var item in queueCreateFiles)
            {
                this.invoiceBO.ExportInvoiceToPdf(item.InvoiceId);
                UpdateQueueCreateFile(new List<long>() { item.InvoiceId }, QueueCreateFileStatus.Successful);
            }
        }

        private List<QueueCreateFile> GetQueueCreateFile()
        {
            List<long> statusQueue = new List<long>() { (int)QueueCreateFileStatus.New, (int)QueueCreateFileStatus.Error };
            return this.queueCreateBO.FilterByStatus(statusQueue).ToList();
        }

        private void UpdateQueueCreateFile(List<long> invoicesId, QueueCreateFileStatus queueStatus)
        {
            this.queueCreateBO.Update(invoicesId, queueStatus);
        }
    }
}