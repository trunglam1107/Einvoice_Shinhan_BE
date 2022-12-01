using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO
{
    public class HistoryReportGeneralRepository : GenericRepository<HISTORYREPORTGENERAL>, IHistoryReportGeneralRepository
    {
        public HistoryReportGeneralRepository(IDbContext context)
           : base(context)
        {
        }

        public ResultSignReportgeneral CheckResultSendTwan(long id)
        {
            var result = (from history in this.context.Set<HISTORYREPORTGENERAL>()
                          join mInvoice in this.context.Set<MINVOICE_DATA>()
                          on history.MESSAGECODE equals mInvoice.MESSAGECODE
                          where history.ID == id
                          select new ResultSignReportgeneral()
                          {
                              Id = history.ID,
                              Message = mInvoice.ERROR,
                              StatusCQT = mInvoice.STATUS,
                              MessageCode = mInvoice.MESSAGECODE,
                              MLDiep = mInvoice.MLTDIEP,
                              LTBAO = mInvoice.LTBAO
                          }).FirstOrDefault();

            return result;


        }

        public IEnumerable<HISTORYREPORTGENERAL> Filter(FilterHistoryReportGeneralCondition condition)
        {
            return dbSet.Where(x =>
                x.ADDITIONALTIMES == condition.AdditionalTimes
                && (
                        (condition.IsMonth == true && x.MONTH == condition.MonthOrQuarterNumber)
                        || (condition.IsMonth == false && x.QUARTER == condition.MonthOrQuarterNumber)
                )
                && x.YEAR == condition.Year)
                .AsEnumerable();
        }

        public IEnumerable<HistoryReportGeneralInfo> GetAll(FilterHistoryReportGeneralCondition condition)
        {
            var data = (from hrg in this.context.Set<HISTORYREPORTGENERAL>()
                        join mycompany in this.context.Set<MYCOMPANY>() on hrg.COMPANYID equals mycompany.COMPANYSID
                        join dataMinvoice in this.context.Set<MINVOICE_DATA>() on hrg.MESSAGECODE equals dataMinvoice.MESSAGECODE
                        into dataMinvoices
                        from dataMinvoice in dataMinvoices.DefaultIfEmpty()
                        select new HistoryReportGeneralInfo()
                        {
                            Id = hrg.ID,
                            CompanyId = hrg.COMPANYID,
                            CompanyName = mycompany.COMPANYNAME,//chi nhánh/PGD
                            PeriodsReport = hrg.PERIODSREPORT,//kỳ tổng hợp(Quý/tháng)
                            AdditionalTimes = hrg.ADDITIONALTIMES,
                            Month = hrg.MONTH,//Tháng
                            Quarter = hrg.QUARTER,//Quý
                            Year = hrg.YEAR,//Năm
                            //LanNop = hrg.ADDITIONALTIMES == null ? "1" : hrg.ADDITIONALTIMES.ToString(),//Lần nộp
                            Status = dataMinvoice.STATUS != null ? dataMinvoice.STATUS == 0 ? 1 : 2 : 3,//Trạng thái
                            FileName = hrg.FILENAME,
                            MessageCode = hrg.MESSAGECODE,
                            Error = dataMinvoice.ERROR,
                            MST = mycompany.TAXCODE,
                            CreateDate = hrg.CREATEDDATE != null ? hrg.CREATEDDATE : DateTime.Now,
                            BranchId = mycompany.BRANCHID
                        }
                        ).OrderByDescending(x => x.Id).ToList();

            var hrgList = data.ToList();

            if (condition.CompanyId > 0)
            {
                hrgList = hrgList.Where(x => x.CompanyId == condition.CompanyId).ToList();
            }

            if (condition.BranchID.IsNotNullOrEmpty())
            {
                hrgList = hrgList.Where(x => x.BranchId == condition.BranchID).ToList();
            }

            if (condition.DateFrom.HasValue)
            {
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate >= condition.DateFrom.Value).ToList();
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate <= dateTo).ToList();
            }

            if(condition.Status > 0)
            {
                //if (condition.Status == 1)
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 1);//trạng thái gửi thành công
                //}
                //else
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 0).ToList();//trạng thái gửi thất bại
                //}
                //if (condition.Status == 1)
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 0).ToList();//trạng thái gửi thất bại
                //}
                //else if (condition.Status == 2)
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 1).ToList();//trạng thái gửi thành công
                //}
                //else
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 3).ToList();//trạng thái đang xử lý
                //}
                hrgList = hrgList.Where(p => p.Status != null && p.Status == condition.Status).ToList();

            }

            if (condition.ReportType > 0)
            {
                if(condition.ReportType == 1) //loại hóa đơn là lần đầu
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes == 0).ToList();
                }
                else
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes > 0).ToList();//loại hóa đơn là bổ sung lần thứ
                }
            }

            hrgList = hrgList.Skip(condition.Skip).Take(condition.Take).ToList();

            return hrgList;
        }

        public IEnumerable<HistoryReportGeneralInfo> GetAllForExcel(FilterHistoryReportGeneralCondition condition)
        {
            var data = (from hrg in this.context.Set<HISTORYREPORTGENERAL>()
                        join mycompany in this.context.Set<MYCOMPANY>() on hrg.COMPANYID equals mycompany.COMPANYSID
                        join dataMinvoice in this.context.Set<MINVOICE_DATA>() on hrg.MESSAGECODE equals dataMinvoice.MESSAGECODE
                        into dataMinvoices
                        from dataMinvoice in dataMinvoices.DefaultIfEmpty()
                        select new HistoryReportGeneralInfo()
                        {
                            Id = hrg.ID,
                            CompanyId = hrg.COMPANYID,
                            CompanyName = mycompany.COMPANYNAME,//chi nhánh/PGD
                            PeriodsReport = hrg.PERIODSREPORT,//kỳ tổng hợp(Quý/tháng)
                            AdditionalTimes = hrg.ADDITIONALTIMES,
                            Month = hrg.MONTH,//Tháng
                            Quarter = hrg.QUARTER,//Quý
                            Year = hrg.YEAR,//Năm
                            //LanNop = hrg.ADDITIONALTIMES == null ? "1" : hrg.ADDITIONALTIMES.ToString(),//Lần nộp
                            Status = dataMinvoice.STATUS != null ? dataMinvoice.STATUS == 0 ? 1 : 2 : 3,//Trạng thái
                            FileName = hrg.FILENAME,
                            MessageCode = hrg.MESSAGECODE,
                            Error = dataMinvoice.ERROR,
                            MST = mycompany.TAXCODE,
                            CreateDate = hrg.CREATEDDATE != null ? hrg.CREATEDDATE : DateTime.Now,
                            BranchId = mycompany.BRANCHID
                        }
                        ).OrderByDescending(x => x.Id).ToList();

            var hrgList = data.ToList();

            if (condition.CompanyId > 0)
            {
                hrgList = hrgList.Where(x => x.CompanyId == condition.CompanyId).ToList();
            }

            if (condition.BranchID.IsNotNullOrEmpty())
            {
                hrgList = hrgList.Where(x => x.BranchId == condition.BranchID).ToList();
            }

            if (condition.DateFrom.HasValue)
            {
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate >= condition.DateFrom.Value).ToList();
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate <= dateTo).ToList();
            }

            if (condition.Status > 0)
            {
                //if (condition.Status == 1)
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 0).ToList();//trạng thái gửi thất bại
                //}
                //else if (condition.Status == 2)
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 1).ToList();//trạng thái gửi thành công
                //}
                //else
                //{
                //    hrgList = hrgList.Where(p => p.Status != null && p.Status == 3).ToList();//trạng thái đang xử lý
                //}
                hrgList = hrgList.Where(p => p.Status != null && p.Status == condition.Status).ToList();
            }

            if (condition.ReportType > 0)
            {
                if (condition.ReportType == 1) //loại hóa đơn là lần đầu
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes == 0).ToList();
                }
                else
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes > 0).ToList();//loại hóa đơn là bổ sung lần thứ
                }
            }

            return hrgList;
        }
        public HISTORYREPORTGENERAL GetById(long id)
        {
            return this.dbSet.FirstOrDefault(h => h.ID == id);
        }

        public long Count(FilterHistoryReportGeneralCondition condition)
        {
            var data = (from hrg in this.context.Set<HISTORYREPORTGENERAL>()
                        join mycompany in this.context.Set<MYCOMPANY>() on hrg.COMPANYID equals mycompany.COMPANYSID
                        join dataMinvoice in this.context.Set<MINVOICE_DATA>() on hrg.MESSAGECODE equals dataMinvoice.MESSAGECODE
                        into dataMinvoices
                        from dataMinvoice in dataMinvoices.DefaultIfEmpty()
                        select new HistoryReportGeneralInfo()
                        {
                            Id = hrg.ID,
                            CompanyId = mycompany.COMPANYSID,
                            CompanyName = mycompany.COMPANYNAME,//chi nhánh/PGD
                            PeriodsReport = hrg.PERIODSREPORT,//kỳ tổng hợp(Quý/tháng)
                            AdditionalTimes = hrg.ADDITIONALTIMES,
                            Month = hrg.MONTH,//Tháng
                            Quarter = hrg.QUARTER,//Quý
                            Year = hrg.YEAR,//Năm
                            Status = dataMinvoice.STATUS != null ? dataMinvoice.STATUS : null,//Trạng thái
                            FileName = hrg.FILENAME,
                            MessageCode = hrg.MESSAGECODE,
                            Error = dataMinvoice.ERROR,
                            MST = mycompany.TAXCODE,
                            CreateDate = hrg.CREATEDDATE != null ? hrg.CREATEDDATE : DateTime.Now,
                            BranchId = mycompany.BRANCHID
                        }
                        ).OrderByDescending(x => x.Id).ToList();

            var hrgList = data.ToList();

            if (condition.CompanyId > 0)
            {
                hrgList = hrgList.Where(x => x.CompanyId == condition.CompanyId).ToList();
            }

            if (condition.BranchID.IsNotNullOrEmpty())
            {
                hrgList = hrgList.Where(x => x.BranchId == condition.BranchID).ToList();
            }

            if (condition.DateFrom.HasValue)
            {
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate >= condition.DateFrom.Value).ToList();
            }

            if (condition.DateTo.HasValue)
            {
                var dateTo = ((DateTime)condition.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                hrgList = hrgList.Where(p => p.CreateDate == null || p.CreateDate <= dateTo).ToList();
            }

            if (condition.Status > 0)
            {
                if (condition.Status == 1)
                {
                    hrgList = hrgList.Where(p => p.Status != null && p.Status == 1).ToList();//trạng thái gửi thành công
                }
                else if (condition.Status == 2)
                {
                    hrgList = hrgList.Where(p => p.Status != null && p.Status == 2).ToList();//trạng thái gửi thành công
                }
                else
                {
                    hrgList = hrgList.Where(p => p.Status != null && p.Status == 0).ToList();//trạng thái gửi thất bại
                }
            }

            if (condition.ReportType > 0)
            {
                if (condition.ReportType == 1) 
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes == 0).ToList();//loại hóa đơn là lần đầu
                }
                else
                {
                    hrgList = hrgList.Where(p => p.AdditionalTimes > 0).ToList();//loại hóa đơn là bổ sung lần thứ
                }
            }

            return hrgList.Count();
        }
        private string convertToString(decimal? number)
        {
            return number.ToString();
        }
        public long GetSBTHDLIEU(long? companyId)
        {
            var getData = this.context.Set<HISTORYREPORTGENERAL>().Where(p => p.COMPANYID == companyId);
            long result = 0;
            if (getData.Count() > 0)
            {
                result = (long)getData.OrderByDescending(p => p.SBTHDLIEU).FirstOrDefault().SBTHDLIEU;
            }
            else
            {
                result = 1;
            }


           //(this.context.Set<HISTORYREPORTGENERAL>().Where(p => p.COMPANYID == companyId)) ?? 1;

            return result;
        }

        private IQueryable<HISTORYREPORTGENERAL> GetFirstTime(decimal? isMonth, int? year, int? month, int? quarter, long? companyId)
        {
            var historys = this.context.Set<HISTORYREPORTGENERAL>().Where(p => p.MESSAGECODE != null);

            historys = historys.Where(p => p.ADDITIONALTIMES == 0 && p.COMPANYID == companyId);

            if (isMonth == 0)
            {
                historys = historys.Where(p => p.QUARTER == month && p.YEAR == year);
            }
            else
            {
                historys = historys.Where(p => p.MONTH == month && p.YEAR == year);
            }

            return historys;
        }

        public IEnumerable<Models.HistoryReportGeneralInfo> GetHistoryReportsList(ConditionReportDetailUse condition)
        {
            var historys = this.context.Set<HISTORYREPORTGENERAL>().Where(p => p.COMPANYID == condition.CompanyId);

            var result = (from history in historys
                          join minvoice in this.context.Set<MINVOICE_DATA>()
                          on history.MESSAGECODE equals minvoice.MESSAGECODE
                          select new HistoryReportGeneralInfo
                          {
                              Status = minvoice.STATUS,
                              LTBAO = minvoice.LTBAO,
                              MLTDIEP = minvoice.MLTDIEP,
                              SBTHDulieu = history.SBTHDLIEU,
                              AdditionalTimes = history.ADDITIONALTIMES,
                              Month = history.MONTH,
                              Quarter = history.QUARTER
                          });

            return result;
        }


    }
}
