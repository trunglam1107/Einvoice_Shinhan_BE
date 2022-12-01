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

namespace InvoiceServer.GateWay.Services.MinVoice
{
    public class MinVoiceService : IMinvoiceService
    {
        private static readonly Logger logger = new Logger();
        protected readonly IDbContext _context;
        protected readonly IDbSet<MINVOICE_DATA> dbSet;
        public MinVoiceService(IDbContext context)
        {
            this._context = context;
            this.dbSet = context.Set<MINVOICE_DATA>();
        }

        public bool Insert(MINVOICE_DATA entity)
        {
            int result = 0;
            try
            {
                dbSet.Add(entity);
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("InsertMinVoiceService", dbEx);
            }

            return result > 0;
        }

        public bool Update(MINVOICE_DATA entity)
        {
            int result = 0;
            try
            {
                result = _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                logger.Error("UpdateMinVoiceService", dbEx);
            }

            return result > 0;
        }
    }
}
