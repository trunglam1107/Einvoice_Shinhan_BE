using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace InvoiceServer.Business.BL
{
    public class InvoiceReplace
    {
        public long ID { get; set; }
        public decimal? TOTALTAX { get; set; }
        public decimal? TOTAL { get; set; }
        public decimal? SUM { get; set; }
        public string REFNUMBER { get; set; }
        public string PRODUCTNAME { get; set; }

        public string YEARMONTH { get; set; }

        public long? CURRENCYID { get; set; }

        public long? COMPANYID { get; set; }

        public string VAT_RATE { get; set; }

        public string REPORT_CLASS { get; set; }

        public string VAT_INVOICE_TYPE { get; set; }

        public string CIF_NO { get; set; }

        public DateTime? RELEASEDDATE { get; set; }

        public string FEE_BEN_OUR { get; set; }
    }

    public class JobReplacedInvoiceBO : IJobReplacedInvoice
    {
        private readonly IAnnouncementBO announcementBO;
        private readonly IReleaseAnnouncementBO releaseAnnouncementBO;
        private readonly IInvoiceBO invoiceBO;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IRegisterTemplatesRepository registerTemplatesRepository;
        private readonly IAutoNumberBO autoNumberBO;
        private readonly IInvoiceReplaceRepository invoiceReplaceRepository;
        private readonly INotificationUseInvoiceRepository notificationUseRepository;
        readonly UserSessionInfo user;
        readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        private readonly PrintConfig config;
        private readonly Logger logger = new Logger();
        private readonly IClientRepository clientRepository;
        private readonly ITaxRepository taxRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly ICancellingInvoiceRepository cancellingInvoiceRepository;
        public JobReplacedInvoiceBO(IRepositoryFactory repoFactory, PrintConfig printConfig, UserSessionInfo userSessionInfo)
        {
            this.user = userSessionInfo;
            this.announcementBO = new AnnouncementBO(repoFactory, printConfig, userSessionInfo);
            this.invoiceBO = new InvoiceBO(repoFactory, printConfig);
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.releaseAnnouncementBO = new ReleaseAnnouncementBO(repoFactory, printConfig, userSessionInfo);
            this.autoNumberBO = new AutoNumberBO(repoFactory);
            this.registerTemplatesRepository = repoFactory.GetRepository<IRegisterTemplatesRepository>();
            this.config = printConfig;
            this.invoiceReplaceRepository = repoFactory.GetRepository<IInvoiceReplaceRepository>();
            this.notificationUseRepository = repoFactory.GetRepository<INotificationUseInvoiceRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.cancellingInvoiceRepository = repoFactory.GetRepository<ICancellingInvoiceRepository>();
        }


        public void ReplaceInvoice()
        {
            var lstInvoiceInfo = this.invoiceReplaceRepository.GetAllInvoiceReplace();//lấy hóa đơn từ bảng invoice
            if (lstInvoiceInfo.Count() > 0)
            {
                try
                {
                    var lstAdjDaily = lstInvoiceInfo.Where(x => x.VAT_INVOICE_TYPE != null && (x.VAT_INVOICE_TYPE.Contains("1") || x.VAT_INVOICE_TYPE.Contains("2"))).ToList();//lấy hóa đơn điều chỉnh daily
                    logger.Error("lstAdjDaily + COMPANYSID : " + lstAdjDaily.FirstOrDefault().COMPANYID, new Exception(lstAdjDaily.Count().ToString()));
                    if (lstAdjDaily.Count() > 0)
                    {
                        CreateAjusment(lstAdjDaily);//Tạo biên bản và hóa đơn điều chỉnh daily
                    }

                    var lstMonthly = lstInvoiceInfo.Where(x => x.VAT_INVOICE_TYPE != null && x.VAT_INVOICE_TYPE.Contains("3")).ToList();//lấy hóa đơn điều chỉnh monthly
                    logger.Error("lstMonthly + COMPANYSID : " + lstAdjDaily.FirstOrDefault().COMPANYID, new Exception(lstMonthly.Count().ToString()));
                    if (lstMonthly.Count() > 0)
                    {
                        CreateAjusmentMonthly(lstMonthly);//Tạo biên bản và hóa đơn điều chỉnh monthly
                    }

                    DeleteAllInvoiceReplace();//Xóa tất cả dữ liệu trong bảng invoicereplace
                }
                catch (Exception ex)
                {
                    logger.Error("Error", ex);
                }
            }
        }
        public bool ReplaceInvoice_new()
        {
            var lstInvoiceInfo = this.invoiceReplaceRepository.GetAllInvoiceReplace();//lấy hóa đơn từ bảng invoice
            logger.Error("List all invoice replace : " + lstInvoiceInfo.Count().ToString(), new Exception("List all invoice replace"));
            if (lstInvoiceInfo.Count() > 0)
            {
                try
                {
                    var lstAdjDaily = lstInvoiceInfo.Where(x => x.VAT_INVOICE_TYPE != null && (x.VAT_INVOICE_TYPE.Contains("1") || x.VAT_INVOICE_TYPE.Contains("2"))).ToList();//lấy hóa đơn điều chỉnh daily
                    logger.Error("lstAdjDaily + COMPANYSID : " + lstAdjDaily.FirstOrDefault().COMPANYID, new Exception(lstAdjDaily.Count().ToString()));
                    if (lstAdjDaily.Count() > 0)
                    {
                        CreateAjusment(lstAdjDaily);//Tạo biên bản và hóa đơn điều chỉnh daily
                    }

                    var lstMonthly = lstInvoiceInfo.Where(x => x.VAT_INVOICE_TYPE != null && x.VAT_INVOICE_TYPE.Contains("3")).ToList();//lấy hóa đơn điều chỉnh monthly
                    logger.Error("lstMonthly + COMPANYSID : " + lstAdjDaily.FirstOrDefault().COMPANYID, new Exception(lstMonthly.Count().ToString()));
                    if (lstMonthly.Count() > 0)
                    {
                        CreateAjusmentMonthly(lstMonthly);//Tạo biên bản và hóa đơn điều chỉnh monthly
                    }

                    DeleteAllInvoiceReplace();//Xóa tất cả dữ liệu trong bảng invoicereplace

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error", ex);
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        //public InvoiceInfo GetInvoiceInfoByRef(string refnum)
        //{
        //    var currentInvoice = this.invoiceRepository.GetInvoiceByRefNumber(refnum);

        //    // Set parent symbol 
        //    currentInvoice.PARENTSYMBOL = currentInvoice.SYMBOL;
        //    long? oldTemplateId = currentInvoice.REGISTERTEMPLATEID;
        //    var currentDetails = currentInvoice.INVOICEDETAILs.Where(p => !(p.DELETED ?? false)).Select(p => new InvoiceDetailInfo(p)).ToList();
        //    List<InvoiceDetailInfo> invoiceDetails = new List<InvoiceDetailInfo>();

        //    if (currentInvoice.REPORT_CLASS != null)
        //    {
        //        InvoiceDetailInfo invoiceDetailInfo = new InvoiceDetailInfo();
        //        invoiceDetailInfo.Price = currentInvoice.TOTAL;
        //        invoiceDetailInfo.Total = currentInvoice.TOTAL;
        //        invoiceDetailInfo.AmountTax = currentInvoice.TOTALTAX;
        //        invoiceDetailInfo.Sum = (currentInvoice.TOTAL + currentInvoice.TOTALTAX);
        //        invoiceDetailInfo.ProductName = currentInvoice.REPORT_CLASS != null ? currentInvoice.REPORT_CLASS.Contains("6") ? "Tiền lãi ngân hàng ( interest collection)" : "Phí dịch vụ Ngân hàng ( Fee commission)" : "";
        //        var taxInfo = this.taxRepository.GetTax(Convert.ToDecimal(currentInvoice.VAT_RATE));
        //        if (taxInfo != null)
        //        {
        //            invoiceDetailInfo.TaxId = taxInfo.ID;
        //        }
        //        else
        //        {
        //            invoiceDetailInfo.TaxId = null;
        //        }

        //        invoiceDetails.Add(invoiceDetailInfo);
        //    }
        //    else
        //    {

        //        invoiceDetails = currentDetails;
        //    }

        //    invoiceDetails = invoiceDetails.OrderBy(p => p.Id).ToList();
        //    InvoiceInfo invoiceInfo_ = new InvoiceInfo(currentInvoice, invoiceDetails, null, null);
        //    var ProductFirst = currentInvoice?.INVOICEDETAILs.OrderByDescending(p => p.ID).FirstOrDefault().TAXID;
        //    var client = this.clientRepository.GetById((long)invoiceInfo_.ClientId);
        //    invoiceInfo_.Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
        //    invoiceInfo_.TaxId = ProductFirst;
        //    var company = this.myCompanyRepository.GetById(companyId ?? 0);
        //    invoiceInfo_.Delegate = company.DELEGATE;

        //    invoiceInfo_.FileReceipt = Path.GetFileName(invoiceInfo_.FileReceipt);
        //    if (invoiceInfo_.Sum.HasValue)
        //    {
        //        invoiceInfo_.AmountInWords = ReadNuberToText.ConvertNumberToString(invoiceInfo_.Sum.Value.ToString());
        //    }
        //    if (!isInvoice || currentInvoice.INVOICETYPE != null)
        //    {
        //        invoiceInfo_.CompanyName = currentInvoice.CUSTOMERNAME;
        //        invoiceInfo_.PersonContact = currentInvoice.PERSONCONTACT;
        //        invoiceInfo_.CustomerName = currentInvoice.PERSONCONTACT;
        //        invoiceInfo_.Address = currentInvoice.CUSTOMERADDRESS;
        //        invoiceInfo_.BankAccount = currentInvoice.CUSTOMERBANKACC;
        //        invoiceInfo_.TaxCode = currentInvoice.CUSTOMERTAXCODE;
        //        invoiceInfo_.ParentReleaseDate = currentInvoice?.INVOICE2?.RELEASEDDATE;
        //        invoiceInfo_.OldTemplateId = oldTemplateId;
        //    }
        //    invoiceInfo_.CustomerCode = currentInvoice?.CLIENT.CUSTOMERCODE;
        //    return invoiceInfo_;
        //}
        private void CreateAjusment(List<InvoiceReplace> infoInvoices)
        {
            try
            {
                if (infoInvoices.Count > 0)
                {
                    foreach (var item in infoInvoices)
                    {
                        try
                        {
                            var curentInvoice = this.invoiceRepository.GetInvoiceByRefNumber(item.REFNUMBER);//lấy thông tin hóa đơn gốc đã ký by số ref

                            if (curentInvoice != null)
                            {
                                InvoiceInfo invoiceInfo = this.invoiceBO.GetInvoiceInfo(curentInvoice.ID, curentInvoice.COMPANYID);
                                //invoiceInfo.InvoiceTemplateCode = this.registerTemplatesRepository.GetById(invoiceInfo.RegisterTemplateId).CODE;
                                //tạo biên bản
                                AnnouncementMD announcementInfo = MapAnnouncement(invoiceInfo);

                                logger.Error("invoiceInfo + no current  " + invoiceInfo.No.ToString() + " companyid current : " + invoiceInfo.CompanyId.ToString(), new Exception("invoiceInfo"));

                                ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo);

                                logger.Error("ANNOUNCEMENT NewAnnoucement  " + newAnnoucement.INVOICENO.ToString(), new Exception("ANNOUNCEMENT NewAnnoucement"));

                                if (newAnnoucement != null)
                                {
                                    ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                                    releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                                    List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                                    ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                                    {
                                        AnnouncementId = newAnnoucement.ID,
                                        InvoiceId = newAnnoucement.INVOICEID
                                    });
                                    releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                                    releaseAnnouncementMaster.UserAction = this.user.Id;
                                    releaseAnnouncementMaster.UserActionId = this.user.Id;
                                    this.releaseAnnouncementBO.SignAnnounFile(releaseAnnouncementMaster, this.user);//ký biên bản
                                    CreateInvoiceAjustment(newAnnoucement, item, invoiceInfo);//tạo hóa đơn điều chỉnh
                                    DeleteAfterCreateAjust(item.ID);//Xóa hóa đơn cũ sau khi tạo
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(this.user.UserId, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error", ex);
            }
        }

        private void CreateInvoiceAjustment(ANNOUNCEMENT Annoucement, InvoiceReplace invoiceAjusmentInfo, InvoiceInfo invoiceInfo)
        {
            try
            {
                string folderTree = PathUtil.FolderTree(Annoucement.ANNOUNCEMENTDATE);
                string fileAnnoucement = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}\\{3}_sign.pdf", this.config.PathInvoiceFile, Annoucement.COMPANYID, folderTree, Annoucement.ID);
                invoiceInfo.ReleasedDate = invoiceAjusmentInfo.RELEASEDDATE;
                Byte[] bytes = File.ReadAllBytes(fileAnnoucement);
                invoiceInfo.FileReceipt = Convert.ToBase64String(bytes);
                invoiceInfo.Total = -invoiceAjusmentInfo.TOTAL;
                invoiceInfo.TotalTax = -invoiceAjusmentInfo.TOTALTAX;
                invoiceInfo.Sum = -invoiceAjusmentInfo.SUM;
                invoiceInfo.InvoiceDetail.FirstOrDefault().Total = -invoiceAjusmentInfo.TOTAL;
                invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName = invoiceAjusmentInfo.PRODUCTNAME ?? invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
                invoiceInfo.InvoiceDetail.FirstOrDefault().AdjustmentType = (int)AdjustmentType.Down;
                invoiceInfo.InvoiceType = (int)InvoiceType.AdjustmentUpDown;
                invoiceInfo.OriginNo = invoiceInfo.No;
                //invoiceInfo.ParentCode = invoiceInfo.InvoiceTemplateCode;
                invoiceInfo.ParentSymbol = invoiceInfo.Symbol;
                invoiceInfo.FolderPath = this.config.PathInvoiceFile + "\\FileRecipt";
                invoiceInfo.Report_Class = invoiceAjusmentInfo.REPORT_CLASS;
                invoiceInfo.Vat_invoice_type = invoiceAjusmentInfo.VAT_INVOICE_TYPE;
                invoiceInfo.Fee_ben_our = invoiceAjusmentInfo.FEE_BEN_OUR;
                invoiceInfo.Cif_no = invoiceAjusmentInfo.CIF_NO;
                this.invoiceBO.AdjustJob(invoiceInfo, invoiceAjusmentInfo.RELEASEDDATE);
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }

        }

        private void CreateAjusmentMonthly(List<InvoiceReplace> infoInvoices)
        {
            if (infoInvoices.Count > 0)
            {
                foreach (var item in infoInvoices)
                {
                    try
                    {
                        var curentInvoice = this.invoiceRepository.GetInvoiceForAdjustmentMonthly(item.CIF_NO, item.YEARMONTH, item.CURRENCYID, item.COMPANYID, item.VAT_RATE, item.REPORT_CLASS);//lấy thông tin hóa đơn gốc đã ký by số ref

                        if (curentInvoice != null)
                        {
                            InvoiceInfo invoiceInfo = this.invoiceBO.GetInvoiceInfo(curentInvoice.ID, curentInvoice.COMPANYID);
                            //invoiceInfo.InvoiceTemplateCode = this.registerTemplatesRepository.GetById(invoiceInfo.RegisterTemplateId).CODE;
                            //tạo biên bản
                            AnnouncementMD announcementInfo = MapAnnouncement(invoiceInfo);
                            ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo);
                            if (newAnnoucement != null)
                            {
                                ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                                releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                                List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                                ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                                {
                                    AnnouncementId = newAnnoucement.ID,
                                    InvoiceId = newAnnoucement.INVOICEID
                                });
                                releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                                releaseAnnouncementMaster.UserAction = this.user.Id;
                                releaseAnnouncementMaster.UserActionId = this.user.Id;
                                this.releaseAnnouncementBO.SignAnnounFile(releaseAnnouncementMaster, this.user);//ký biên bản
                                CreateInvoiceAjustmentMonthly(newAnnoucement, item, invoiceInfo);//tạo hóa đơn điều chỉnh
                                DeleteAfterCreateAjust(item.ID);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(this.user.UserId, ex);
                    }
                }
            }

        }


        private void CreateInvoiceAjustmentMonthly(ANNOUNCEMENT Annoucement, InvoiceReplace invoiceAjusmentInfo, InvoiceInfo invoiceInfo)
        {
            invoiceInfo.ReleasedDate = invoiceAjusmentInfo.RELEASEDDATE;
            string folderTree = PathUtil.FolderTree(Annoucement.ANNOUNCEMENTDATE);
            string fileAnnoucement = string.Format("{0}\\{1}\\Releases\\SignAnnouncement\\{2}\\{3}_sign.pdf", this.config.PathInvoiceFile, Annoucement.COMPANYID, folderTree, Annoucement.ID);
            Byte[] bytes = File.ReadAllBytes(fileAnnoucement);
            invoiceInfo.FileReceipt = Convert.ToBase64String(bytes);
            invoiceInfo.Total = invoiceAjusmentInfo.TOTAL;
            invoiceInfo.TotalTax = invoiceAjusmentInfo.TOTALTAX;
            invoiceInfo.Sum = invoiceAjusmentInfo.SUM;
            invoiceInfo.InvoiceDetail.FirstOrDefault().Total = invoiceAjusmentInfo.TOTAL;
            invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName = invoiceAjusmentInfo.PRODUCTNAME ?? invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
            invoiceInfo.InvoiceDetail.FirstOrDefault().AdjustmentType = (int)AdjustmentType.Down;
            invoiceInfo.InvoiceType = (int)InvoiceType.AdjustmentUpDown;
            invoiceInfo.OriginNo = invoiceInfo.No;
            invoiceInfo.ParentCode = invoiceInfo.InvoiceTemplateCode;
            invoiceInfo.ParentSymbol = invoiceInfo.Symbol;
            invoiceInfo.FolderPath = this.config.PathInvoiceFile + "\\FileRecipt";
            this.invoiceBO.Adjust(invoiceInfo);
        }




        private AnnouncementMD MapAnnouncement(InvoiceInfo invoiceInfo)
        {
            MYCOMPANY myCompany = new MYCOMPANY();
            AnnouncementMD announcementInfo = new AnnouncementMD();
            announcementInfo.LegalGrounds = this.GetConfig("Circulars");
            announcementInfo.Title = "Biên bản điều chỉnh hóa đơn";
            announcementInfo.AnnouncementDate = DateTime.Now;
            announcementInfo.CompanyId = invoiceInfo.CompanyId ?? 0;
            announcementInfo.CompanyName = invoiceInfo.CompanyNameInvoice;
            announcementInfo.CompanyAddress = invoiceInfo.CompanyAddress;
            announcementInfo.CompanyTel = myCompany.TEL1;
            announcementInfo.CompanyTaxCode = invoiceInfo.CompanyTaxCode;
            announcementInfo.CompanyRepresentative = invoiceInfo.Delegate;
            announcementInfo.CompanyPosition = myCompany.POSITION;
            announcementInfo.ClientName = invoiceInfo.CustomerName ?? invoiceInfo.CompanyName;
            announcementInfo.ClientAddress = invoiceInfo.Address;
            announcementInfo.ClientTel = invoiceInfo.Mobile;
            announcementInfo.ClientTaxCode = invoiceInfo.TaxCode;
            announcementInfo.ClientRepresentative = invoiceInfo.CustomerName;
            announcementInfo.ClientPosition = null;
            announcementInfo.InvoiceId = invoiceInfo.Id;
            announcementInfo.RegisterTemplateId = invoiceInfo.RegisterTemplateId;
            //announcementInfo.Denominator = invoiceInfo.InvoiceTemplateCode;//Mẫu số
            announcementInfo.Symbol = invoiceInfo.Symbol;
            announcementInfo.InvoiceNo = invoiceInfo.No;
            announcementInfo.InvoiceDate = invoiceInfo.ReleasedDate ?? DateTime.Now;
            announcementInfo.TotalAmount = invoiceInfo.Total;
            announcementInfo.ServiceName = invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
            announcementInfo.Reasion = "Điều chỉnh giảm do sai số tiền";
            announcementInfo.BeforeContain = invoiceInfo.Sum.ToString();
            announcementInfo.ChangeContain = "-" + invoiceInfo.Sum.ToString();
            announcementInfo.AnnouncementType = 1;
            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Approved;
            announcementInfo.CreatedBy = this.user.Id;
            announcementInfo.ApprovedBy = this.user.Id;
            announcementInfo.ProcessBy = this.user.Id;
            announcementInfo.UpdateStatus = AnnouncementStatus.New;

            string announcementId = "BBHHD";
            if (announcementInfo.AnnouncementType == 1)
                announcementId = "BBDCHD";
            if (announcementInfo.AnnouncementType == 2)
                announcementId = "BBTTHD";
            announcementInfo.MinutesNo = this.autoNumberBO.CreateAutoNumber((long)invoiceInfo.CompanyId, announcementId, invoiceInfo.BranchId, true);

            return announcementInfo;
        }

        private void DeleteAfterCreateAjust(long Id)
        {
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string cmdText = "DELETE FROM INVOICEREPLACE WHERE ID = " + Id;
            using (OracleConnection conn = new OracleConnection(connectionString))
            using (OracleCommand cmd = new OracleCommand(cmdText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAllInvoiceReplace()
        {
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string cmdText = "DELETE FROM INVOICEREPLACE";
            using (OracleConnection conn = new OracleConnection(connectionString))
            using (OracleCommand cmd = new OracleCommand(cmdText, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #region Multiple Invoice Cancel
        public void MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo)
        {
            foreach (var p in deleteInfo.ReleaseInvoiceInfos)
            {

                InvoiceInfo invoiceInfo = GetInvoiceInfo(p.InvoiceId); //lấy thông tin hóa đơn

                AnnouncementMD announcementInfo = MapAnnouncement(invoiceInfo, deleteInfo.Description); //map biên bản hủy

                ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo); //tạo biên bản

                if (newAnnoucement != null)
                {
                    ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                    releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                    List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                    ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                    {
                        AnnouncementId = newAnnoucement.ID,
                        InvoiceId = newAnnoucement.INVOICEID
                    });
                    releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                    releaseAnnouncementMaster.UserAction = this.user.Id;
                    releaseAnnouncementMaster.UserActionId = this.user.Id;
                    var resultSign = this.releaseAnnouncementBO.SignAnnounFileMultipleCancel(releaseAnnouncementMaster, this.user);//ký biên bản

                    if (resultSign.Message == "True")
                    {
                        //Hủy hóa đơn đó
                        p.CancellingDate = deleteInfo.CancellingDate;
                        p.Description = deleteInfo.Description;
                        InsertCancelMultipleInvoice(p);
                        INVOICE invoice = this.invoiceRepository.GetById(p.InvoiceId);
                        invoice.INVOICESTATUS = (int)InvoiceStatus.Cancel;
                        invoice.UPDATEDDATE = deleteInfo.CancellingDate;
                        this.invoiceRepository.Update(invoice);

                        announcementInfo.id = newAnnoucement.ID;
                        announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Successfull;
                        this.announcementBO.UpdateAnnounceAfterInvoiceCancel(announcementInfo);
                    }
                }


            }
        }
        private void InsertCancelMultipleInvoice(ReleaseInvoiceInfo deleteInfo)
        {
            CANCELLINGINVOICE cancellingInvoice = new CANCELLINGINVOICE();
            cancellingInvoice.COMPANYID = this.user.Company.Id;
            cancellingInvoice.INVOICEID = deleteInfo.InvoiceId;
            cancellingInvoice.CANCELBY = this.user.Id;
            cancellingInvoice.CANCELLEDDATE = deleteInfo.CancellingDate;
            cancellingInvoice.DESCRIPTION = deleteInfo.Description;
            this.cancellingInvoiceRepository.Insert(cancellingInvoice);
        }
        public InvoiceInfo GetInvoiceInfo(long id, bool isInvoice = true)
        {
            INVOICE currentInvoice = GetInvoice(id);

            // Set parent symbol 
            currentInvoice.PARENTSYMBOL = currentInvoice.SYMBOL;
            long? oldTemplateId = currentInvoice.REGISTERTEMPLATEID;
            var currentDetails = currentInvoice.INVOICEDETAILs.Where(p => !(p.DELETED ?? false)).Select(p => new InvoiceDetailInfo(p)).ToList();
            List<InvoiceDetailInfo> invoiceDetails = new List<InvoiceDetailInfo>();
            if (currentInvoice.IMPORTTYPE != 2)
            {
                if (currentInvoice.REPORT_CLASS != null)
                {
                    InvoiceDetailInfo invoiceDetailInfo = new InvoiceDetailInfo();
                    invoiceDetailInfo.Price = currentInvoice.TOTAL;
                    invoiceDetailInfo.Total = currentInvoice.TOTAL;
                    invoiceDetailInfo.AmountTax = currentInvoice.TOTALTAX;
                    invoiceDetailInfo.Sum = (currentInvoice.TOTAL + currentInvoice.TOTALTAX);
                    if (currentInvoice.REF_CTT != null)
                    {
                        invoiceDetailInfo.ProductName = currentInvoice.REF_CTT;
                    }
                    else
                    {
                        invoiceDetailInfo.ProductName = currentInvoice.REPORT_CLASS != null ?
                            currentInvoice.REPORT_CLASS.Contains("6") ? "Tiền lãi ngân hàng ( interest collection)" + " Ref:" + currentInvoice.REFNUMBER :
                            "Phí dịch vụ Ngân hàng ( Fee commission)" + " Ref:" + currentInvoice.REFNUMBER : "";
                    }

                    var taxInfo = this.taxRepository.GetTax(Convert.ToDecimal(currentInvoice.VAT_RATE));
                    if (taxInfo != null)
                    {
                        invoiceDetailInfo.TaxId = taxInfo.ID;
                    }
                    else
                    {
                        invoiceDetailInfo.TaxId = null;
                    }

                    invoiceDetails.Add(invoiceDetailInfo);
                }
                else
                {
                    invoiceDetails = currentDetails;
                }
            }
            else
            {

                invoiceDetails = currentDetails;
            }

            invoiceDetails = invoiceDetails.OrderBy(p => p.Id).ToList();
            InvoiceInfo invoiceInfo_ = new InvoiceInfo(currentInvoice, invoiceDetails, null, null);
            var ProductFirst = currentInvoice?.INVOICEDETAILs.OrderByDescending(p => p.ID).FirstOrDefault().TAXID;
            var client = this.clientRepository.GetById((long)invoiceInfo_.ClientId);
            invoiceInfo_.Email = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
            invoiceInfo_.TaxId = ProductFirst;
            var company = this.myCompanyRepository.GetById(currentInvoice.COMPANYID);
            invoiceInfo_.Delegate = company.DELEGATE;

            invoiceInfo_.FileReceipt = Path.GetFileName(invoiceInfo_.FileReceipt);
            if (invoiceInfo_.Sum.HasValue)
            {
                invoiceInfo_.AmountInWords = ReadNuberToText.ConvertNumberToString(invoiceInfo_.Sum.Value.ToString());
            }
            if (!isInvoice || currentInvoice.INVOICETYPE != null)
            {
                invoiceInfo_.CompanyName = currentInvoice.CUSTOMERNAME;
                invoiceInfo_.PersonContact = currentInvoice.PERSONCONTACT;
                invoiceInfo_.CustomerName = currentInvoice.PERSONCONTACT;
                invoiceInfo_.Address = currentInvoice.CUSTOMERADDRESS;
                invoiceInfo_.BankAccount = currentInvoice.CUSTOMERBANKACC;
                invoiceInfo_.TaxCode = currentInvoice.CUSTOMERTAXCODE;
                invoiceInfo_.ParentReleaseDate = currentInvoice?.INVOICE2?.RELEASEDDATE;
                invoiceInfo_.OldTemplateId = oldTemplateId;
            }
            invoiceInfo_.CustomerCode = currentInvoice?.CLIENT.CUSTOMERCODE;
            invoiceInfo_.CompanyId = currentInvoice.COMPANYID;
            invoiceInfo_.BranchId = company.BRANCHID;
            invoiceInfo_.VatInvoiceType = currentInvoice.VAT_INVOICE_TYPE;
            return invoiceInfo_;
        }

        public INVOICE GetInvoice(long id)
        {
            INVOICE invoices = this.invoiceRepository.GetById(id);
            if (invoices == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This InvoiceId [{0}] not found in data of client", id));
            }

            return invoices;
        }

        private AnnouncementMD MapAnnouncement(InvoiceInfo invoiceInfo, string reasion)
        {
            MYCOMPANY myCompany = new MYCOMPANY();
            AnnouncementMD announcementInfo = new AnnouncementMD();
            announcementInfo.LegalGrounds = this.GetConfig("CircularsTT78");
            //announcementInfo.LegalGrounds = "- Căn cứ Nghị định 51/2010/NĐ-CP ngày 14/05/2010 của Chính phủ quy định về hoá đơn bán hàng hoá, cung ứng dịch vụ.\n- Căn cứ Nghị định 04/2014/NĐ-CP ngày 17/01/2014 sửa đổi, bổ sung một số điều của Nghị định 51/2010/NĐ-CP ngày 14/05/2010 của Chính phủ quy định về hoá đơn bán hàng hoá, cung ứng dịch vụ.\n- Căn cứ Thông tư số 39/2014/TT BTC ngày 31/03/2014 hướng dẫn thi hành Nghị định số 51/2010/NĐ-CP và Nghị định 04/2014/NĐ-CP.\n- Căn cứ Thông tư 32/2011/TT-BTC ngày 14/03/2011 của Bộ Tài chính Hướng dẫn về khởi tạo, phát hành và sử dụng hoá đơn điện tử bán hàng hóa, cung ứng dịch vụ.";
            announcementInfo.Title = "Biên bản hủy hóa đơn";
            announcementInfo.AnnouncementDate = DateTime.Now;
            announcementInfo.CompanyId = invoiceInfo.CompanyId ?? 0;
            announcementInfo.CompanyName = invoiceInfo.CompanyNameInvoice;
            announcementInfo.CompanyAddress = invoiceInfo.CompanyAddress;
            announcementInfo.CompanyTel = myCompany.TEL1;
            announcementInfo.CompanyTaxCode = invoiceInfo.CompanyTaxCode;
            announcementInfo.CompanyRepresentative = invoiceInfo.Delegate;
            announcementInfo.CompanyPosition = myCompany.POSITION;
            announcementInfo.ClientName = invoiceInfo.CustomerName ?? invoiceInfo.CompanyName;
            announcementInfo.ClientAddress = invoiceInfo.Address;
            announcementInfo.ClientTel = invoiceInfo.Mobile;
            announcementInfo.ClientTaxCode = invoiceInfo.TaxCode;
            announcementInfo.ClientRepresentative = invoiceInfo.CustomerName;
            announcementInfo.ClientPosition = null;
            announcementInfo.InvoiceId = invoiceInfo.Id;
            announcementInfo.RegisterTemplateId = invoiceInfo.RegisterTemplateId;
            //announcementInfo.Denominator = invoiceInfo.InvoiceTemplateCode;//Mẫu số
            announcementInfo.Symbol = invoiceInfo.Symbol;
            announcementInfo.InvoiceNo = invoiceInfo.No;
            announcementInfo.InvoiceDate = invoiceInfo.ReleasedDate ?? DateTime.Now;
            announcementInfo.TotalAmount = invoiceInfo.Sum;
            announcementInfo.ServiceName = invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
            announcementInfo.Reasion = reasion;
            //announcementInfo.BeforeContain = invoiceInfo.Sum.ToString();
            //announcementInfo.ChangeContain = "-" + invoiceInfo.Sum.ToString();
            announcementInfo.AnnouncementType = 3;
            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Approved;
            announcementInfo.CreatedBy = this.user.Id;
            announcementInfo.ApprovedBy = this.user.Id;
            announcementInfo.ProcessBy = this.user.Id;
            announcementInfo.UpdateStatus = AnnouncementStatus.New;

            string announcementId = "BBHHD";
            //if (announcementInfo.AnnouncementType == 1)
            //    announcementId = "BBDCHD";
            //if (announcementInfo.AnnouncementType == 2)
            //    announcementId = "BBTTHD";
            announcementInfo.MinutesNo = this.autoNumberBO.CreateAutoNumber((long)invoiceInfo.CompanyId, announcementId, invoiceInfo.BranchId, true);

            return announcementInfo;
        }
        #endregion Multiple Invoice Cancel

        #region Multiple Invoice Adjusted
        public void MultipleInvoiceAdjusted(CancellingInvoiceMaster deleteInfo)
        {
            foreach (var p in deleteInfo.ReleaseInvoiceInfos)
            {

                InvoiceInfo invoiceInfo = GetInvoiceInfo(p.InvoiceId); //lấy thông tin hóa đơn

                AnnouncementMD announcementInfo = MapAnnouncementAdjustment(invoiceInfo, deleteInfo.Description); //map biên bản hủy

                ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo); //tạo biên bản

                if (newAnnoucement != null)
                {
                    ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                    releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                    List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                    ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                    {
                        AnnouncementId = newAnnoucement.ID,
                        InvoiceId = newAnnoucement.INVOICEID
                    });
                    releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                    releaseAnnouncementMaster.UserAction = this.user.Id;
                    releaseAnnouncementMaster.UserActionId = this.user.Id;
                    var resultSign = this.releaseAnnouncementBO.SignAnnounFileMultipleCancel(releaseAnnouncementMaster, this.user);//ký biên bản

                    if (resultSign.Message == "True")
                    {
                        INVOICE invoice = this.invoiceRepository.GetById(invoiceInfo.Id);
                        //invoice.INVOICESTATUS = (int)InvoiceStatus.Cancel;
                        invoice.INVOICETYPE = 3;
                        invoice.PARENTID = invoiceInfo.Id;
                        invoice.PARENTSYMBOL = invoiceInfo.Symbol;
                        invoice.UPDATEDDATE = DateTime.Now;
                        this.invoiceRepository.Update(invoice);

                        announcementInfo.id = newAnnoucement.ID;
                        announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Successfull;
                        this.announcementBO.UpdateAnnounceAfterInvoiceCancel(announcementInfo);
                    }
                }
            }
        }

        private AnnouncementMD MapAnnouncementAdjustment(InvoiceInfo invoiceInfo, string reasion)
        {
            MYCOMPANY myCompany = new MYCOMPANY();
            AnnouncementMD announcementInfo = new AnnouncementMD();
            announcementInfo.LegalGrounds = this.GetConfig("CircularsTT78");
            //announcementInfo.LegalGrounds = "- Căn cứ Nghị định 51/2010/NĐ-CP ngày 14/05/2010 của Chính phủ quy định về hoá đơn bán hàng hoá, cung ứng dịch vụ.\n- Căn cứ Nghị định 04/2014/NĐ-CP ngày 17/01/2014 sửa đổi, bổ sung một số điều của Nghị định 51/2010/NĐ-CP ngày 14/05/2010 của Chính phủ quy định về hoá đơn bán hàng hoá, cung ứng dịch vụ.\n- Căn cứ Thông tư số 39/2014/TT BTC ngày 31/03/2014 hướng dẫn thi hành Nghị định số 51/2010/NĐ-CP và Nghị định 04/2014/NĐ-CP.\n- Căn cứ Thông tư 32/2011/TT-BTC ngày 14/03/2011 của Bộ Tài chính Hướng dẫn về khởi tạo, phát hành và sử dụng hoá đơn điện tử bán hàng hóa, cung ứng dịch vụ.";
            announcementInfo.Title = "Biên bản điều chỉnh hoá đơn";
            announcementInfo.AnnouncementDate = DateTime.Now;
            announcementInfo.CompanyId = invoiceInfo.CompanyId ?? 0;
            announcementInfo.CompanyName = invoiceInfo.CompanyNameInvoice;
            announcementInfo.CompanyAddress = invoiceInfo.CompanyAddress;
            announcementInfo.CompanyTel = myCompany.TEL1;
            announcementInfo.CompanyTaxCode = invoiceInfo.CompanyTaxCode;
            announcementInfo.CompanyRepresentative = invoiceInfo.Delegate;
            announcementInfo.CompanyPosition = myCompany.POSITION;
            announcementInfo.ClientName = invoiceInfo.CustomerName ?? invoiceInfo.CompanyName;
            announcementInfo.ClientAddress = invoiceInfo.Address;
            announcementInfo.ClientTel = invoiceInfo.Mobile;
            announcementInfo.ClientTaxCode = invoiceInfo.TaxCode;
            announcementInfo.ClientRepresentative = invoiceInfo.CustomerName;
            announcementInfo.ClientPosition = null;
            announcementInfo.InvoiceId = invoiceInfo.Id;
            announcementInfo.RegisterTemplateId = invoiceInfo.RegisterTemplateId;
            //announcementInfo.Denominator = invoiceInfo.InvoiceTemplateCode;//Mẫu số
            announcementInfo.Symbol = invoiceInfo.Symbol;
            announcementInfo.InvoiceNo = invoiceInfo.No;
            announcementInfo.InvoiceDate = invoiceInfo.ReleasedDate ?? DateTime.Now;
            announcementInfo.TotalAmount = invoiceInfo.Sum;//Giá trị hoá đơn
            announcementInfo.ServiceName = invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
            announcementInfo.Reasion = reasion;
            //announcementInfo.BeforeContain = invoiceInfo.Sum.ToString();
            //announcementInfo.ChangeContain = "-" + invoiceInfo.Sum.ToString();
            announcementInfo.AnnouncementType = 1;
            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Approved;
            announcementInfo.CreatedBy = this.user.Id;
            announcementInfo.ApprovedBy = this.user.Id;
            announcementInfo.ProcessBy = this.user.Id;
            announcementInfo.UpdateStatus = AnnouncementStatus.New;

            string announcementId = "BBDCHD";
            //if (announcementInfo.AnnouncementType == 1)
            //    announcementId = "BBDCHD";
            //if (announcementInfo.AnnouncementType == 2)
            //    announcementId = "BBTTHD";
            announcementInfo.MinutesNo = this.autoNumberBO.CreateAutoNumber((long)invoiceInfo.CompanyId, announcementId, invoiceInfo.BranchId, true);

            return announcementInfo;
        }
        #endregion

        #region Upload Invoice Cancel
        public ResultImportSheet ImportData(UploadInvoice dataUpload, string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            //ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoice.SheetNames);
            //DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoice.SheetName, ImportInvoice.ColumnImport, ref rowError);
            logger.Error("Start read file invoice cancel", new Exception("ImportData"));
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoiceCancel.SheetNames);
            DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoiceCancel.SheetName, ImportInvoiceCancel.ColumnImport, ref rowError);
            logger.Error("Done read file invoice cancel", new Exception(dtInvoice.Rows.Count.ToString()));
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtInvoice == null || dtInvoice.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                List<long> listInvoiceImportsuccess;
                listResultImport = ProcessData_NEW(dataUpload, dtInvoice, out listInvoiceImportsuccess);
            }
            catch (Exception ex)
            {
                logger.Error("Error import invoice cancel", new Exception(ex.ToString()));
            }
            return listResultImport;
        }

        private ResultImportSheet ProcessData_NEW(UploadInvoice dataUpload, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;
            logger.Error("Start insert " + dtInvoice.Rows.Count, new Exception(DateTime.Now.ToString()));
            foreach (DataRow row in dtInvoice.Rows)
            {
                rowIndex++;

                listResultImport.RowError.AddRange(ValidateHeaderInvoiceCancel(row, rowIndex));
                MYCOMPANY getCompany = this.myCompanyRepository.GetByBranchId(row[ImportInvoiceCancel.MaCN].Trim());
                var date = row[ImportInvoiceCancel.NgayHoaDon].Trim();
                var symbol = row[ImportInvoiceCancel.KyHieu].Trim();
                var invoiceno = row[ImportInvoiceCancel.SoHD].Trim();

                CLIENT cLIENT = this.clientRepository.GetByCustomerCode(row[ImportInvoiceCancel.CifKH].Trim());

                INVOICE getInvoice = this.invoiceRepository.GetInvoiceBySymbolReleaseddateInvoiceno(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture), Convert.ToDecimal(invoiceno), symbol, cLIENT.ID);

                if (getInvoice == null)
                {
                    continue;
                }
                else
                {
                    InvoiceInfo invoiceInfo = GetInvoiceInfo(getInvoice.ID); //lấy thông tin hóa đơn

                    AnnouncementMD announcementInfo = MapAnnouncement(invoiceInfo, row[ImportInvoiceCancel.NoiDungHuy].Trim()); //map biên bản hủy

                    ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo); //tạo biên bản

                    if (newAnnoucement != null)
                    {
                        ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                        releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                        List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                        ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                        {
                            AnnouncementId = newAnnoucement.ID,
                            InvoiceId = newAnnoucement.INVOICEID
                        });
                        releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                        releaseAnnouncementMaster.UserAction = this.user.Id;
                        releaseAnnouncementMaster.UserActionId = this.user.Id;
                        var resultSign = this.releaseAnnouncementBO.SignAnnounFileMultipleCancel(releaseAnnouncementMaster, this.user);//ký biên bản

                        if (resultSign.Message == "True")
                        {
                            ReleaseInvoiceInfo releaseInvoiceInfo = new ReleaseInvoiceInfo();

                            //Hủy hóa đơn đó
                            releaseInvoiceInfo.CancellingDate = DateTime.Now;
                            releaseInvoiceInfo.Description = row[ImportInvoiceCancel.NoiDungHuy].Trim();
                            releaseInvoiceInfo.InvoiceId = invoiceInfo.Id;
                            InsertCancelMultipleInvoice(releaseInvoiceInfo);
                            INVOICE invoice = this.invoiceRepository.GetById(invoiceInfo.Id);
                            invoice.INVOICESTATUS = (int)InvoiceStatus.Cancel;
                            invoice.UPDATEDDATE = releaseInvoiceInfo.CancellingDate;
                            this.invoiceRepository.Update(invoice);

                            announcementInfo.id = newAnnoucement.ID;
                            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Successfull;
                            this.announcementBO.UpdateAnnounceAfterInvoiceCancel(announcementInfo);
                        }
                    }

                }
            }
            //logger.Error("Insert success: " + listInvoiceImportsuccess.Count, new Exception(DateTime.Now.ToString()));
            return listResultImport;
        }

        private ListError<ImportRowError> ValidateHeaderInvoiceCancel(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();

            listError.Add(ValidDataIsNull(row, ImportInvoiceCancel.MaCN, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceCancel.KyHieu, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceCancel.SoHD, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceCancel.NgayHoaDon, rowIndex));

            return listError;
        }

        private ListError<ImportRowError> ValidateHeaderInvoiceAdjusted(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();

            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustedImp.MaCN, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustedImp.KyHieu, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustedImp.SoHD, rowIndex));
            listError.Add(ValidDataIsNull(row, ImportInvoiceAdjustedImp.NgayHoaDon, rowIndex));

            return listError;
        }
        private ImportRowError ValidDataIsNull(DataRow row, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (!row[colunName].IsNullOrEmpty())
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataIsEmpty, colunName, rowIndex);
            }
            catch (Exception)
            {
                return errorColumn;
            }
        }
        #endregion Upload Invoice Cancel

        #region Import Invoice Adjusted
        public ResultImportSheet ImportDataInvoiceAdjusted(UploadInvoice dataUpload, string fullPathFile, long companyId)
        {
            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            logger.Error("Start read file invoice cancel", new Exception("ImportData"));
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportInvoiceAdjustedImp.SheetNames);
            DataTable dtInvoice = importExcel.GetBySheetName(ImportInvoiceAdjustedImp.SheetName, ImportInvoiceAdjustedImp.ColumnImport, ref rowError);
            logger.Error("Done read file invoice cancel", new Exception(dtInvoice.Rows.Count.ToString()));
            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtInvoice == null || dtInvoice.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                List<long> listInvoiceImportsuccess;
                listResultImport = ProcessData_NEW_AdjustedInvoice(dataUpload, dtInvoice, out listInvoiceImportsuccess);
            }
            catch (Exception ex)
            {
                logger.Error("Error import invoice cancel", new Exception(ex.ToString()));
            }
            return listResultImport;
        }

        private ResultImportSheet ProcessData_NEW_AdjustedInvoice(UploadInvoice dataUpload, DataTable dtInvoice, out List<long> listInvoiceImportsuccess)
        {
            ResultImportSheet listResultImport = new ResultImportSheet();
            listInvoiceImportsuccess = new List<long>();
            long invoiceId = 0;
            int rowIndex = 0;
            logger.Error("Start insert " + dtInvoice.Rows.Count, new Exception(DateTime.Now.ToString()));
            foreach (DataRow row in dtInvoice.Rows)
            {
                rowIndex++;

                listResultImport.RowError.AddRange(ValidateHeaderInvoiceAdjusted(row, rowIndex));
                MYCOMPANY getCompany = this.myCompanyRepository.GetByBranchId(row[ImportInvoiceAdjustedImp.MaCN].Trim());
                var date = row[ImportInvoiceAdjustedImp.NgayHoaDon].Trim();
                var symbol = row[ImportInvoiceAdjustedImp.KyHieu].Trim();
                var invoiceno = row[ImportInvoiceAdjustedImp.SoHD].Trim();

                CLIENT cLIENT = this.clientRepository.GetByCustomerCode(row[ImportInvoiceAdjustedImp.CifKH].Trim());

                INVOICE getInvoice = this.invoiceRepository.GetInvoiceBySymbolReleaseddateInvoiceno(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture), Convert.ToDecimal(invoiceno), symbol, cLIENT.ID);

                if (getInvoice == null)
                {
                    continue;
                }
                else
                {
                    InvoiceInfo invoiceInfo = GetInvoiceInfo(getInvoice.ID); //lấy thông tin hóa đơn

                    AnnouncementMD announcementInfo = MapAnnouncementAdjustment(invoiceInfo, row[ImportInvoiceAdjustedImp.NoiDungDieuChinh].Trim()); //map biên bản điều chỉnh

                    ANNOUNCEMENT newAnnoucement = this.announcementBO.SaveMD(announcementInfo); //tạo biên bản

                    if (newAnnoucement != null)
                    {
                        ReleaseAnnouncementMaster releaseAnnouncementMaster = new ReleaseAnnouncementMaster();
                        releaseAnnouncementMaster.CompanyId = newAnnoucement.COMPANYID;
                        List<ReleaseAnnouncementInfo> ListReleaseAnnouncementInfo = new List<ReleaseAnnouncementInfo>();
                        ListReleaseAnnouncementInfo.Add(new ReleaseAnnouncementInfo
                        {
                            AnnouncementId = newAnnoucement.ID,
                            InvoiceId = newAnnoucement.INVOICEID
                        });
                        releaseAnnouncementMaster.ReleaseAnnouncementInfos = ListReleaseAnnouncementInfo;
                        releaseAnnouncementMaster.UserAction = this.user.Id;
                        releaseAnnouncementMaster.UserActionId = this.user.Id;
                        var resultSign = this.releaseAnnouncementBO.SignAnnounFileMultipleCancel(releaseAnnouncementMaster, this.user);//ký biên bản

                        if (resultSign.Message == "True")
                        {
                            INVOICE invoice = this.invoiceRepository.GetById(invoiceInfo.Id);
                            //invoice.INVOICESTATUS = (int)InvoiceStatus.Cancel;
                            invoice.INVOICETYPE = 3;
                            invoice.PARENTID = invoiceInfo.Id;
                            invoice.PARENTSYMBOL = invoiceInfo.Symbol;
                            invoice.UPDATEDDATE = DateTime.Now;
                            this.invoiceRepository.Update(invoice);

                            announcementInfo.id = newAnnoucement.ID;
                            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Successfull;
                            this.announcementBO.UpdateAnnounceAfterInvoiceCancel(announcementInfo);
                        }


                    }

                }
            }
            //logger.Error("Insert success: " + listInvoiceImportsuccess.Count, new Exception(DateTime.Now.ToString()));
            return listResultImport;
        }
        #endregion



    }
}
