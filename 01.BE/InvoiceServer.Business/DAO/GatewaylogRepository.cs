using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models.GatewayLog;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO
{
    public class GatewaylogRepository : GenericRepository<GATEWAY_LOG>, IGatewaylogRepository
    {
        public GatewaylogRepository(IDbContext context)
               : base(context)
        {
        }

        public long Count(ConditionSearchGatewaylog condition)
        {
            var gatewaylogs = GetList();

            if (!condition.GatewaylogName.IsNullOrEmpty())
            {
                gatewaylogs = gatewaylogs.Where(p => p.NAME.ToUpper().Contains(condition.GatewaylogName.ToUpper()));
            }

            if (condition.DateFrom != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateFrom != null && condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date).Where(p => p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            return gatewaylogs.Count();
        }

        public IEnumerable<GatewaylogDetail> Filter(ConditionSearchGatewaylog condition)
        {
            IQueryable<GATEWAY_LOG> gatewaylogs = FilterByCondition(condition);

            gatewaylogs = this.SubFilter(gatewaylogs, condition);

            var gatewaylogInfos = (SelectGatewaylogDetail(gatewaylogs));
            return gatewaylogInfos.ToList();
        }

        public IEnumerable<GATEWAY_LOG> GetList()
        {
            return this.dbSet;
        }

        private IQueryable<GATEWAY_LOG> FilterByCondition(ConditionSearchGatewaylog condition)
        {
            var gatewaylogs = GetList();

            if (!condition.GatewaylogName.IsNullOrEmpty())
            {
                gatewaylogs = gatewaylogs.Where(p => p.NAME.ToUpper().Contains(condition.GatewaylogName.ToUpper()));
            }


            if (condition.DateFrom != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateFrom != null && condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date && p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            return gatewaylogs.AsQueryable();
        }

        private IQueryable<GATEWAY_LOG> SubFilter(IQueryable<GATEWAY_LOG> gatewaylogs, ConditionSearchGatewaylog condition)
        {
            if (condition.ColumnOrder == "NAME")
            {
                if (condition.OrderType.Equals(OrderTypeConst.Desc))
                {
                    gatewaylogs = gatewaylogs.OrderByDescending(p => p.NAME).Skip(condition.Skip).Take(condition.Take);
                }
                else
                {
                    gatewaylogs = gatewaylogs.OrderBy(p => p.NAME).Skip(condition.Skip).Take(condition.Take);
                }
            }
            else
            {
                gatewaylogs = gatewaylogs.OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take);
            }

            if (!condition.GatewaylogName.IsNullOrEmpty())
            {
                gatewaylogs = gatewaylogs.Where(p => p.NAME.ToUpper().Contains(condition.GatewaylogName.ToUpper()));
            }


            if (condition.DateFrom != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE == null || p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            if (condition.DateFrom != null && condition.DateTo != null)
            {
                gatewaylogs = gatewaylogs.Where(p => p.CREATEDDATE.Value.Date >= condition.DateFrom.Value.Date && p.CREATEDDATE.Value.Date <= condition.DateTo.Value.Date).ToList().AsQueryable();
            }

            return gatewaylogs;
        }


        private static IQueryable<GatewaylogDetail> SelectGatewaylogDetail(IQueryable<GATEWAY_LOG> gatewaylogs)
        {
            return from gatewaylog in gatewaylogs
                   select new GatewaylogDetail
                   {
                       Id = gatewaylog.ID,
                       Name = gatewaylog.NAME,
                       Body = gatewaylog.BODY,
                       ObjectName = gatewaylog.OBJECTNAME,
                       IP = gatewaylog.IP,
                       CreatedDate = gatewaylog.CREATEDDATE,
                       CreatedBy = gatewaylog.CREATEDBY
                   };
        }
    }
}
