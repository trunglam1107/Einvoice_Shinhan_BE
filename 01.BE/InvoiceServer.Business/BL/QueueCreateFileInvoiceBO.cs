using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class QueueCreateFileInvoiceBO : IQueueCreateFileInvoiceBO
    {
        #region Fields, Properties
        private readonly IQueueCreateFileInvoiceRepository queueRepository;

        #endregion

        #region Contructor

        public QueueCreateFileInvoiceBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.queueRepository = repoFactory.GetRepository<IQueueCreateFileInvoiceRepository>();
        }

        #endregion

        #region Methods
        public ResultCode Update(List<long> invoiceIds, QueueCreateFileStatus status)
        {
            var queueCreateFiles = this.queueRepository.FilterByInvoicesId(invoiceIds).ToList();
            queueCreateFiles.ForEach(p =>
            {
                p.PROCESSSTATUS = (int)status;
                this.queueRepository.Update(p);
            });

            return ResultCode.NoError;
        }

        public ResultCode Create(List<long> invoiceIds)
        {
            foreach (var item in invoiceIds)
            {
                QUEUECREATEFILEINVOICE queue = new QUEUECREATEFILEINVOICE();
                queue.INVOICEID = item;
                queue.PROCESSSTATUS = (int)QueueCreateFileStatus.New;
                queue.CREATEDFILE = false;
                this.queueRepository.Insert(queue);
            }

            return ResultCode.NoError;
        }
        public IEnumerable<QueueCreateFile> FilterByStatus(List<long> status)
        {
            var queueCreateFiles = this.queueRepository.FilterByStatus(status);
            return queueCreateFiles.Select(p => new QueueCreateFile(p.INVOICEID));
        }

        #endregion  
    }
}