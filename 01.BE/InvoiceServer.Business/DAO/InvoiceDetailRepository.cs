using Dapper;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceDetailRepository : GenericRepository<INVOICEDETAIL>, IInvoiceDetailRepository
    {
        public InvoiceDetailRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICEDETAIL> FilterInvoiceDetail(long invoiceId)
        {
            return dbSet.Where(p => !(p.DELETED ?? false) && (p.INVOICEID == invoiceId)).OrderBy(x => x.PRODUCTNAME);
        }
        public IQueryable<INVOICEDETAIL> FilterInvoiceDetailJob(long invoiceId)
        {
            return dbSet.Where(p => !(p.DELETED ?? false) && (p.INVOICEID == invoiceId)).OrderBy(x => x.PRODUCTNAME);
        }
        public List<InvoiceDetail> GetDetail(List<InvoicePrintModel> invoicePrints)
        {
            var result = new List<InvoiceDetail>();
            var skip = 0;
            var take = (invoicePrints.Count() / 6) + 1;
            for (var i = 1; i <= 6; i++)
            {
                try
                {
                    var listResult = new List<InvoiceDetail>();
                    var listProcess = new List<decimal>(invoicePrints.Skip(skip).Take(take).Select(p => p.ID));
                    if(listProcess.Count > 0)
                    {
                        string invoiceids = string.Join(",", listProcess);
                        string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                        string sqlResult = @"SELECT DT.ID AS Id, DT.INVOICEID AS InvoiceId, DT.QUANTITY AS Quantity, DT.PRICE AS Price, DT.TAXID AS TaxId, DT.TOTAL AS Total,
                                      DT.AMOUNTTAX AS AmountTax, DT.PRODUCTNAME AS ProductName, DT.SUM AS Sum, T.CODE AS TaxCode,
                                      T.NAME AS TaxName, T.DISPLAYINVOICE AS DisplayInvoice  
                                    FROM INVOICEDETAIL DT 
                                    INNER JOIN TAX T ON T.ID = DT.TAXID
                                    WHERE DT.INVOICEID IN ({INVOICEIDS})
                                    ORDER BY DT.PRODUCTNAME";
                        sqlResult = sqlResult.Replace("{INVOICEIDS}", invoiceids);
                        //logger.Error("GetDetail: " + i + " script: " + sqlResult, new Exception(take.ToString()));
                        using (var connection = new OracleConnection(connectionString))
                        {
                            connection.Open();
                            listResult = connection.Query<InvoiceDetail>(sqlResult).ToList();
                            connection.Close();
                        }
                        result.AddRange(listResult);
                        skip = take * i;
                    }
                    
                }
                catch (Exception ex)
                {
                    throw;
                }
            }


            return result;
        }
        //đợi confirm BIDC
        public IEnumerable<InvoiceItem> GetInvoiceItems(long invoiceId)
        {
            var invoiceDetails = FilterInvoiceDetail(invoiceId);
            var invoiceItems =
                from invoiceDetail in invoiceDetails
                    //join unit in this.context.Set<UNITLIST>() on invoiceDetail.UNITID equals unit.ID
                select new InvoiceItem(invoiceDetail)
                {
                    //Unit = unit.NAME,
                };
            return invoiceItems;
        }

        public INVOICEDETAIL GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        private IQueryable<INVOICEDETAIL> GetInvoiceDetailActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public bool ContainProduct(long productId)
        {
            return Contains(p => p.PRODUCTID == productId);
        }
    }
}
