using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.GateWay.Models.MVan;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace InvoiceServer.GateWay.Services.NotificationMinvoice
{
    public class NotificationMinvoiceService : INotificationMinvoiceService
    {
        private static readonly Logger logger = new Logger();
        protected readonly IDbContext _context;
        protected readonly IDbSet<NOTIFICATIONMINVOICE> dbSet;
        public NotificationMinvoiceService(IDbContext context)
        {
            this._context = context;
            this.dbSet = context.Set<NOTIFICATIONMINVOICE>();
        }

        public bool Insert(NOTIFICATIONMINVOICE entity)
        {
            int result = 0;
            try
            {
                dbSet.Add(entity);
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("InsertNotificationMinvoiceService", dbEx);
            }

            return result > 0;
        }

        public bool Update(NOTIFICATIONMINVOICE entity)
        {
            int result = 0;
            try
            {
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("UpdateNotificationMinvoiceService", dbEx);
            }

            return result > 0;
        }
    }
}
