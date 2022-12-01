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

namespace InvoiceServer.GateWay.Services.GateWayLog
{
    public class GateWayLogService : IGateWayLogService
    {
        private static readonly Logger logger = new Logger();
        protected readonly IDbContext _context;
        protected readonly IDbSet<GATEWAY_LOG> dbSet;
        public GateWayLogService(IDbContext context)
        {
            this._context = context;
            this.dbSet = context.Set<GATEWAY_LOG>();
        }

        public bool Insert(GATEWAY_LOG entity)
        {
            int result = 0;
            try
            {
                dbSet.Add(entity);
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("InsertGateWayLogService", dbEx);
            }

            return result > 0;
        }

        public bool Update(GATEWAY_LOG entity)
        {
            int result = 0;
            try
            {
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("UpdateGateWayLogService", dbEx);
            }

            return result > 0;
        }

        public bool Delete(GATEWAY_LOG entity)
        {
            dbSet.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}
