
using InvoiceServer.API.Business.Portal;
using InvoiceServer.API.Controllers;
using InvoiceServer.Common;
using Quartz;
using System;
using System.Threading.Tasks;
using System.Web;

namespace InvoiceServer.API.Business
{
    public static class LockJob
    {
        public static readonly object lockJobSign = new object();

        public static readonly object lockJobApproved = new object();

        public static readonly object lockJobSendMail = new object();

        public static readonly object lockJobSendAccount = new object();

        public static readonly object lockJobReplace = new object();

        public static readonly object lockJobSendInvoiceGDT = new object();
        public static readonly object lockJobSignPer = new object();

    }

    #region Execute job

    public class SendInvoiceGDT : IJob
    {
        private static readonly Logger logger = new Logger();

        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"SendInvoiceGDT registered {timeId}");
            return RunJob(timeId);
        }

        private Task RunJob(long timeId)
        {
            //lock (LockJob.lockJobSendInvoiceGDT)
            //{

            //}

            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            ReportController reportController = new ReportController();
            InvoiceController invoiceController = new InvoiceController();
            var getJobName = invoiceController.FindByJobName("SendInvoiceGDT");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        logger.QuarztJob(false, $"SendInvoiceGDT start {timeId}");

                        reportController.UpdateProcessJob("SendInvoiceGDT", true);
                        reportController.JobAutoSendInvoiceGDT();
                        reportController.UpdateProcessJob("SendInvoiceGDT", false);
                        logger.QuarztJob(false, $"SendInvoiceGDT done {timeId}");
                        return Task.FromResult($"SendInvoiceGDT done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Quarzt SendInvoiceGDT {timeId}", ex);
                        logger.QuarztJob(true, $"SendInvoiceGDT error {timeId}");
                        return Task.FromResult($"SendInvoiceGDT failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"SendInvoiceGDT processing {timeId}");
                    return Task.FromResult($"SendInvoiceGDT processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt SendInvoiceGDT {timeId}", ex);
                logger.QuarztJob(true, $"SendInvoiceGDT error {timeId}");
                return Task.FromResult($"SendInvoiceGDT failed {timeId}");
            }

        }
    }
    public class KeepSession : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"KeepSession registered {timeId}");
            logger.QuarztJob(false, $"KeepSession start {timeId}");
            ValidateSession valid = new ValidateSession();
            valid.check();
            logger.QuarztJob(false, $"KeepSession done {timeId}");
            return Task.FromResult($"KeepSession done {timeId}");
        }
    }
    public class QuarztActionJobSendEmail : IJob
    {
        private static readonly Logger logger = new Logger();

        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"SendEmailAccount registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            InvoiceController controller = new InvoiceController();
            var getJobName = controller.FindByJobName("SendEmailAccount");

            try
            {
                if (getJobName.PROCESSING == false)
                {
                    logger.QuarztJob(false, $"SendEmailAccount start {timeId}");
                    controller.UpdateProcessJob("SendEmailAccount", true);
                    valid.JobSendEmailForCustomer();
                    controller.UpdateProcessJob("SendEmailAccount", false);
                    logger.QuarztJob(false, $"SendEmailAccount done {timeId}");
                    return Task.FromResult($"SendEmailAccount done {timeId}");
                }
                else
                {
                    logger.QuarztJob(true, $"SendEmailAccount processing {timeId}");
                    return Task.FromResult($"SendEmailAccount processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt SendEmailAccount {timeId}", ex);
                logger.QuarztJob(true, $"SendEmailAccount error {timeId}");
                return Task.FromResult($"SendEmailAccount failed {timeId}");
            }
        }
        //public Task Execute(IJobExecutionContext context)
        //{
        //    var timeId = DateTime.Now.Ticks;
        //    logger.QuarztJob(false, $"SendEmailAccount registered {timeId}");
        //    //lock (LockJob.lockJobSendAccount)
        //    //{

        //    //}

        //    try
        //    {
        //        if (getJobName.PROCESSING == false)
        //        {
        //            try
        //            {
  
        //                Httpcontext.Instance.getHttpcontext();
        //                ValidateSession valid = new ValidateSession();
        //                valid.check();
        //                valid.JobSendEmailForCustomer();
        //                logger.QuarztJob(false, $"SendEmailAccount done {timeId}");
        //                return Task.FromResult($"SendEmailAccount done {timeId}");
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Error($"Quarzt SendEmailAccount {timeId}", ex);
        //                logger.QuarztJob(true, $"SendEmailAccount error {timeId}");
        //                return Task.FromResult($"SendEmailAccount failed {timeId}");
        //            }
        //        }
        //        else
        //        {
        //            logger.QuarztJob(true, $"SendEmailAccount processing {timeId}");
        //            return Task.FromResult($"SendEmailAccount processing {timeId}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Quarzt SendEmailAccount {timeId}", ex);
        //        logger.QuarztJob(true, $"SendEmailAccount error {timeId}");
        //        return Task.FromResult($"SendEmailAccount failed {timeId}");
        //    }

        //}
    }
    public class ApproveInvoice : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"ApproveInvoice registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            InvoiceController controller = new InvoiceController();
            var getJobName = controller.FindByJobName("ApproveInvoice");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        logger.QuarztJob(false, $"ApproveInvoice start {timeId}");
                        controller.UpdateProcessJob("ApproveInvoice", true);
                        controller.JobApprove();
                        logger.QuarztJob(false, $"ApproveInvoice done {timeId}");
                        controller.UpdateProcessJob("ApproveInvoice", false);
                        return Task.FromResult($"ApproveInvoice done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        controller.UpdateProcessJob("ApproveInvoice", false);
                        logger.Error($"Quarzt ApproveInvoice {timeId}", ex);
                        logger.QuarztJob(true, $"ApproveInvoice error {timeId}");
                        return Task.FromResult($"ApproveInvoice failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"ApproveInvoice processing {timeId}");
                    return Task.FromResult($"ApproveInvoice processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                controller.UpdateProcessJob("ApproveInvoice", false);
                logger.Error($"Quarzt ApproveInvoice {timeId}", ex);
                logger.QuarztJob(true, $"ApproveInvoice error {timeId}");
                return Task.FromResult($"ApproveInvoice failed {timeId}");
            }
            //lock (LockJob.lockJobApproved)
            //{

            //}

        }
    }

    public class JobReplacedInvoice : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"InvoiceAdjusment registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            JobReplaceInvoiceController controller = new JobReplaceInvoiceController();
            InvoiceController invcontroller = new InvoiceController();
            var getJobName = invcontroller.FindByJobName("InvoiceAdjustment");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        logger.QuarztJob(false, $"InvoiceAdjusment start {timeId}");
                        invcontroller.UpdateProcessJob("InvoiceAdjustment", true);
                        var checkReplaceInvoice = controller.ReplaceInvoiceNew();
                        if (checkReplaceInvoice == true)
                        {
                            invcontroller.JobApprove();
                        }
                        invcontroller.UpdateProcessJob("InvoiceAdjustment", false);
                        logger.QuarztJob(false, $"InvoiceAdjustment done {timeId}");
                        return Task.FromResult($"InvoiceAdjustment done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Quarzt InvoiceAdjustment {timeId}", ex);
                        logger.QuarztJob(true, $"InvoiceAdjustment error {timeId}");
                        return Task.FromResult($"InvoiceAdjustment failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"InvoiceAdjustment processing {timeId}");
                    return Task.FromResult($"InvoiceAdjustment processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt InvoiceAdjustment {timeId}", ex);
                logger.QuarztJob(true, $"InvoiceAdjustment error {timeId}");
                return Task.FromResult($"InvoiceAdjustment failed {timeId}");
            }
        }

    }

    public class ReleaseInvoice : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"ReleaseInvoice registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            //lock (LockJob.lockJobSign)
            //{
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            InvoiceController controller = new InvoiceController();
            var getJobName = controller.FindByJobName("ReleaseInvoice");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        logger.QuarztJob(false, $"ReleaseInvoice start {timeId}");
                        controller.UpdateProcessJob("ReleaseInvoice", true);
                        controller.JobServerSign();
                        logger.QuarztJob(false, $"ReleaseInvoice done {timeId}");
                        controller.UpdateProcessJob("ReleaseInvoice", false);
                        return Task.FromResult($"ReleaseInvoice done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        controller.UpdateProcessJob("ReleaseInvoice", false);
                        logger.Error($"Quarzt ReleaseInvoice {timeId}", ex);
                        logger.QuarztJob(true, $"ReleaseInvoice error {timeId}");
                        return Task.FromResult($"ReleaseInvoice failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"ReleaseInvoice processing {timeId}");
                    return Task.FromResult($"ReleaseInvoice processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                controller.UpdateProcessJob("ReleaseInvoice", false);
                logger.Error($"Quarzt ReleaseInvoice {timeId}", ex);
                logger.QuarztJob(true, $"ReleaseInvoice error {timeId}");
                return Task.FromResult($"ReleaseInvoice failed {timeId}");
            }

            //}
        }
    }
    public class SignPersonal : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"SignPersonal registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            //lock (LockJob.lockJobSignPer)
            //{

            //}

            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            InvoiceController controller = new InvoiceController();
            var getJobName = controller.FindByJobName("SignPersonal");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        logger.QuarztJob(false, $"SignPersonal start {timeId}");
                        controller.UpdateProcessJob("SignPersonal", true);
                        controller.JobSignPersonal();
                        logger.QuarztJob(false, $"SignPersonal done {timeId}");
                        controller.UpdateProcessJob("SignPersonal", false);
                        return Task.FromResult($"SignPersonal done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        controller.UpdateProcessJob("SignPersonal", false);
                        logger.Error($"Quarzt SignPersonal {timeId}", ex);
                        logger.QuarztJob(true, $"SignPersonal error {timeId}");
                        return Task.FromResult($"SignPersonal failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"SignPersonal processing {timeId}");
                    return Task.FromResult($"SignPersonal processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                controller.UpdateProcessJob("SignPersonal", false);
                logger.Error($"Quarzt SignPersonal {timeId}", ex);
                logger.QuarztJob(true, $"SignPersonal error {timeId}");
                return Task.FromResult($"SignPersonal failed {timeId}");
            }

        }
    }
    public class SendMail : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"SendMails registered {timeId}");
            return RunJob(timeId);
        }
        private Task RunJob(long timeId)
        {
            //lock (LockJob.lockJobSendMail)
            //{

            //}
            logger.QuarztJob(false, $"SendMails start {timeId}");
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            InvoiceController controller = new InvoiceController();
            var getJobName = controller.FindByJobName("SendEmails");
            try
            {
                if (getJobName.PROCESSING == false)
                {
                    try
                    {
                        controller.JobSendMails();
                        logger.QuarztJob(false, $"SendMails done {timeId}");
                        return Task.FromResult($"SendMails done {timeId}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Quarzt SendMails {timeId}", ex);
                        logger.QuarztJob(true, $"SendMails error {timeId}");
                        return Task.FromResult($"SendMails failed {timeId}");
                    }
                }
                else
                {
                    logger.QuarztJob(true, $"SendMails processing {timeId}");
                    return Task.FromResult($"SendMails processing {timeId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt SendMails {timeId}", ex);
                logger.QuarztJob(true, $"SendMails error {timeId}");
                return Task.FromResult($"SendMails failed {timeId}");
            }

        }
    }
    public class SendMailWarning : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"SendWarningQuantity registered {timeId}");
            //lock (LockJob.lockJob)
            //{
            try
            {
                logger.QuarztJob(false, $"SendWarningQuantity start {timeId}");
                Httpcontext.Instance.getHttpcontext();
                ValidateSession valid = new ValidateSession();
                valid.check();
                SendMailwarning warningControl = new SendMailwarning();
                warningControl.SendMailWarning();
                logger.QuarztJob(false, $"SendWarningQuantity done {timeId}");
                return Task.FromResult($"SendWarningQuantity done {timeId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt SendWarningQuantity {timeId}", ex);
                logger.QuarztJob(true, $"SendWarningQuantity error {timeId}");
                return Task.FromResult($"SendWarningQuantity failed {timeId}");
            }
            //}
        }
    }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ImportClients'
    public class ImportClients : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"ImportFile registered  {timeId}");
            //lock (LockJob.lockJob)
            //{
            try
            {
                logger.QuarztJob(false, $"ImportFile start {timeId}");
                Httpcontext.Instance.getHttpcontext();
                ValidateSession valid = new ValidateSession();
                valid.check();
                InvoiceController invoice = new InvoiceController();
                invoice.autoImport(true);// isJob is true
                logger.QuarztJob(false, $"ImportFile done {timeId}");
                return Task.FromResult($"ImportFile done {timeId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt ImportFile {timeId}", ex);
                logger.QuarztJob(true, $"ImportFile error {timeId}");
                return Task.FromResult($"ImportFile failed {timeId}");
            }
            //}
        }
    }
    public class ImportInvoice : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            var timeId = DateTime.Now.Ticks;
            logger.QuarztJob(false, $"ImportInvoices registered {timeId}");
            //lock (LockJob.lockJob)
            //{
            try
            {
                logger.QuarztJob(false, $"ImportInvoices start {timeId}");
                Httpcontext.Instance.getHttpcontext();
                ValidateSession valid = new ValidateSession();
                valid.check();
                logger.QuarztJob(false, $"ImportInvoices done {timeId}");
                return Task.FromResult($"ImportInvoices done {timeId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Quarzt ImportInvoices {timeId}", ex);
                logger.QuarztJob(true, $"ImportInvoices error {timeId}");
                return Task.FromResult($"ImportInvoices failed {timeId}");
            }
            //}
        }
    }



    public class TestQuartDo : IJob
    {
        private static readonly Logger logger = new Logger();
        public Task Execute(IJobExecutionContext context)
        {
            //lock (LockJob.lockJob)
            //{
            Httpcontext.Instance.getHttpcontext();
            ValidateSession valid = new ValidateSession();
            valid.check();
            logger.QuarztJob(false, "NamHT");
            return Task.FromResult("done");
            //}
        }
    }
    #endregion

    #region Process 
    public class SendMailwarning : BaseController
    {
        private readonly InvoicePortalBusiness invoicePortalbusiness;
        private readonly static Logger logger = new Logger();
        public SendMailwarning()
        {
            invoicePortalbusiness = new InvoicePortalBusiness(GetBOFactory());
        }

        public bool SendMailWarning()
        {
            bool result = false;
            try
            {
                var kq = this.invoicePortalbusiness.SendMailWarning();
                logger.QuarztJob(false, "Kq SendMailWarning: " + kq);
                result = true;
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            return result;
        }
    }
    public class SendMailmonthly : BaseController
    {
        private readonly ReleaseInvoiceBusiness releaseInvoiceBusiness;
        private readonly static Logger logger = new Logger();
        public SendMailmonthly()
        {
            releaseInvoiceBusiness = new ReleaseInvoiceBusiness(GetBOFactory());
        }

        public bool SendMailMonthly()
        {
            bool result = false;
            try
            {
                var kq = this.releaseInvoiceBusiness.ServiceSendMailByMonth();
                logger.QuarztJob(false, "Kq SendMailMonthly: " + kq);
                result = true;
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            return result;
        }
    }
    public class ValidateSession : BaseController
    {
        private readonly SessionBusiness session;
        private readonly InvoicePortalBusiness invoicebusiness;
        private readonly static Logger logger = new Logger();
        public ValidateSession()
        {
            session = new SessionBusiness(GetBOFactory());
            invoicebusiness = new InvoicePortalBusiness(GetBOFactory());
        }
        public bool JobSendEmailForCustomer()
        {
            bool result = false;
            try
            {
                this.invoicebusiness.SendEmailClientAccountInfo(1);
                result = true;
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                result = false;
            }
            return result;
        }

        public void check()
        {
            try
            {
                //|| this.CurrentUser.Company == null
                if (this.CurrentUser == null || this.CurrentUser.Company == null)
                {
                    session.QuarzLogin();
                }

                //logger.QuarztJob(false, $"check CurrentUser 1 is {CurrentUser.UserId} + {CurrentUser.Email} + {CurrentUser.RoleUser}");
            }
            catch
            {
                session.QuarzLogin();
                //logger.QuarztJob(false, $"Check CurrentUser 2 is {CurrentUser.UserId} + {CurrentUser.Email} + {CurrentUser.RoleUser}");
            }
        }
    }

    public sealed class Httpcontext
    {
        private Httpcontext() { }
        private static readonly Lazy<Httpcontext> lazy = new Lazy<Httpcontext>(() => new Httpcontext());
        public static Httpcontext Instance
        {
            get { return lazy.Value; }
        }
        public HttpContext ulti { get; set; }

        public void getHttpcontext()
        {
            HttpContext.Current = ulti;
        }

    }
    #endregion
}