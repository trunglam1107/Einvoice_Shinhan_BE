using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
    public class QuarztJobBussiness
    {
        private readonly IQuarztJobBO quarztJobBO;
        public QuarztJobBussiness(IBOFactory boFactory)
        {
            this.quarztJobBO = boFactory.GetBO<IQuarztJobBO>();
        }

        public IEnumerable<QuarztJob> GetAll(ConditionSearchQuartzJob condition)
        {
            List<QuarztJob> result = new List<QuarztJob>();
            var list = this.quarztJobBO.getallJob(condition).ToList();
            foreach (var loop in list)
            {
                loop.NEXTTIMERUN = QuarztSingleton.Instance.next(loop.JOBNAME).ToString("yyyy-MM-dd HH:mm:ss");
                loop.LASTTIMERUN = QuarztSingleton.Instance.prev(loop.JOBNAME).ToString("yyyy-MM-dd HH:mm:ss");
                result.Add(loop);
            }
            result = result.AsQueryable()
                            .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take)
                            .ToList();
            return result;

        }
        public QuarztJob getById(long id)
        {
            var condition = new ConditionSearchQuartzJob(null, null);
            var list = GetAll(condition);
            return list.FirstOrDefault(p => p.ID == id);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.UpdateProcessByName(string, bool)'
        public void UpdateProcessByName(string name, bool processing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.UpdateProcessByName(string, bool)'
        {
            this.quarztJobBO.UpdateProcessByName(name, processing);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.findJobName(long)'
        public string findJobName(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.findJobName(long)'
        {
            var condition = new ConditionSearchQuartzJob(null, null);
            var list = GetAll(condition);
            return list.FirstOrDefault(p => p.ID.Equals(id)).JOBNAME;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.updateStatus(long, bool)'
        public bool updateStatus(long id, bool status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.updateStatus(long, bool)'
        {
            return this.quarztJobBO.updateStatus(id, status);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.UpdateCron(QuarztJob)'
        public ResultCode UpdateCron(QuarztJob model)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.UpdateCron(QuarztJob)'
        {
            if (model == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            excuteJob(false, model);
            return this.quarztJobBO.UpdateCron(model);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.excuteJob(bool, QuarztJob)'
        public void excuteJob(bool isCreate, QuarztJob loop)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'QuarztJobBussiness.excuteJob(bool, QuarztJob)'
        {
            if (isCreate)
            {
                switch (loop.JOBNAME)
                {
                    case "PrintLog":
                        QuarztSingleton.Instance.createJobbyCron<TestQuartDo>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendEmailAccount":
                        QuarztSingleton.Instance.createJobbyCron<QuarztActionJobSendEmail>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ApproveInvoice":
                        QuarztSingleton.Instance.createJobbyCron<ApproveInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ReleaseInvoice":
                        QuarztSingleton.Instance.createJobbyCron<ReleaseInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendWarningQuantity":
                        QuarztSingleton.Instance.createJobbyCron<SendMailWarning>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ImportFile":
                        QuarztSingleton.Instance.createJobbyCron<ImportClients>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ImportInvoices":
                        QuarztSingleton.Instance.createJobbyCron<ImportInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "InvoiceAdjustment":
                        QuarztSingleton.Instance.createJobbyCron<JobReplacedInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendEmails":
                        QuarztSingleton.Instance.createJobbyCron<SendMail>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "KeepSession":
                        QuarztSingleton.Instance.createJobbyCron<KeepSession>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendInvoiceGDT":
                        QuarztSingleton.Instance.createJobbyCron<SendInvoiceGDT>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SignPersonal":
                        QuarztSingleton.Instance.createJobbyCron<SignPersonal>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (loop.JOBNAME)
                {
                    case "PrintLog":
                        QuarztSingleton.Instance.onUpdateTrigger<TestQuartDo>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendEmailAccount":
                        QuarztSingleton.Instance.onUpdateTrigger<QuarztActionJobSendEmail>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ApproveInvoice":
                        QuarztSingleton.Instance.onUpdateTrigger<ApproveInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ReleaseInvoice":
                        QuarztSingleton.Instance.onUpdateTrigger<ReleaseInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendWarningQuantity":
                        QuarztSingleton.Instance.onUpdateTrigger<SendMailWarning>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ImportFile":
                        QuarztSingleton.Instance.onUpdateTrigger<ImportClients>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "ImportInvoices":
                        QuarztSingleton.Instance.onUpdateTrigger<ImportInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "InvoiceAdjustment":
                        QuarztSingleton.Instance.onUpdateTrigger<JobReplacedInvoice>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendEmails":
                        QuarztSingleton.Instance.onUpdateTrigger<SendMail>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "KeepSession":
                        QuarztSingleton.Instance.onUpdateTrigger<KeepSession>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SendInvoiceGDT":
                        QuarztSingleton.Instance.onUpdateTrigger<SendInvoiceGDT>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    case "SignPersonal":
                        QuarztSingleton.Instance.onUpdateTrigger<SignPersonal>(loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.JOBNAME, loop.SCRONEXPRESSION);
                        break;
                    default:
                        break;
                }

            }

        }

    }
}