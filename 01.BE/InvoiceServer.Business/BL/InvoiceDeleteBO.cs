using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Configuration;

namespace InvoiceServer.Business.BL
{
    public class InvoiceDeleteBO : IInvoiceDeleteBO
    {
        #region Fields, Properties
        private readonly IInvoiceRepository invoiceRepository;
        private readonly UserSessionInfo currentUser;
        private readonly IDeletingInvoiceRepository deletingInvoiceRepository;
        private readonly ICancellingInvoiceRepository cancellingInvoiceRepository;
        private readonly IInvoiceDetailRepository invoiceDetailRepository;
        private readonly IClientRepository clientRepository;
        private readonly ITaxRepository taxRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        private readonly IAutoNumberBO autoNumberBO;
        private readonly IAnnouncementBO announcementBO;
        private readonly PrintConfig config;
        private readonly IReleaseAnnouncementBO releaseAnnouncementBO;
        #endregion

        #region Contructor
        public InvoiceDeleteBO(IRepositoryFactory repoFactory, UserSessionInfo userSessionInfo)
        {
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.currentUser = userSessionInfo;
            this.deletingInvoiceRepository = repoFactory.GetRepository<IDeletingInvoiceRepository>();
            this.cancellingInvoiceRepository = repoFactory.GetRepository<ICancellingInvoiceRepository>();
            this.invoiceDetailRepository = repoFactory.GetRepository<IInvoiceDetailRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.autoNumberBO = new AutoNumberBO(repoFactory);
            this.announcementBO = new AnnouncementBO(repoFactory,null, userSessionInfo);
            this.releaseAnnouncementBO = new ReleaseAnnouncementBO(repoFactory, null, userSessionInfo);
        }
        public void InvoiceDelete(CancellingInvoiceMaster deleteInfo)
        {
            if (invoiceRepository.CheckSendMailStatus(deleteInfo.InvoiceId))
            {
                InsertDeleteInvoice(deleteInfo);
                INVOICE invoice = this.invoiceRepository.GetById(deleteInfo.InvoiceId);
                invoice.INVOICESTATUS = (int)InvoiceStatus.Delete;
                invoice.UPDATEDDATE = deleteInfo.CancellingDate;
                invoice.MESSAGECODE = null;
                this.invoiceRepository.Update(invoice);
            }
            else
            {
                throw new BusinessLogicException(ResultCode.InvoiceSentmail, MsgApiResponse.DataInvalid);
            }
        }
        #endregion
        #region method
        public void MultipleInvoiceDelete(CancellingInvoiceMaster deleteInfo)
        {
            foreach (var p in deleteInfo.ReleaseInvoiceInfos)
            {
                if (invoiceRepository.CheckSendMailStatus(p.InvoiceId))
                {
                    p.CancellingDate = deleteInfo.CancellingDate;
                    p.Description = deleteInfo.Description;
                    InsertDeleteMultipleInvoice(p);
                    INVOICE invoice = this.invoiceRepository.GetById(p.InvoiceId);
                    invoice.INVOICESTATUS = (int)InvoiceStatus.Delete;
                    invoice.UPDATEDDATE = deleteInfo.CancellingDate;
                    this.invoiceRepository.Update(invoice);
                }
                else
                {
                    throw new BusinessLogicException(ResultCode.InvoiceSentmail, MsgApiResponse.DataInvalid);
                }
            }
        }

        private void InsertDeleteMultipleInvoice(ReleaseInvoiceInfo deleteInfo)
        {
            DELETINGINVOICE deletingInvoice = new DELETINGINVOICE();
            deletingInvoice.COMPANYID = this.currentUser.Company.Id;
            deletingInvoice.INVOICEID = deleteInfo.InvoiceId;
            deletingInvoice.DELETEDBY = this.currentUser.Id;
            deletingInvoice.DELETEDDATE = deleteInfo.CancellingDate;
            deletingInvoice.DESCRIPTION = deleteInfo.Description;
            this.deletingInvoiceRepository.Insert(deletingInvoice);
        }
        private void InsertDeleteInvoice(CancellingInvoiceMaster deleteInfo)
        {
            DELETINGINVOICE deletingInvoice = new DELETINGINVOICE();
            deletingInvoice.COMPANYID = this.currentUser.Company.Id;
            deletingInvoice.INVOICEID = deleteInfo.InvoiceId;
            deletingInvoice.DELETEDBY = this.currentUser.Id;
            deletingInvoice.DELETEDDATE = deleteInfo.CancellingDate;
            deletingInvoice.DESCRIPTION = deleteInfo.Description;
            this.deletingInvoiceRepository.Insert(deletingInvoice);
        }

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
                    releaseAnnouncementMaster.UserAction = this.currentUser.Id;
                    releaseAnnouncementMaster.UserActionId = this.currentUser.Id;
                    var resultSign = this.releaseAnnouncementBO.SignAnnounFileMultipleCancel(releaseAnnouncementMaster, this.currentUser);//ký biên bản

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
                    }
                }

                
            }
        }
        private void InsertCancelMultipleInvoice(ReleaseInvoiceInfo deleteInfo)
        {
            CANCELLINGINVOICE cancellingInvoice = new CANCELLINGINVOICE();
            cancellingInvoice.COMPANYID = this.currentUser.Company.Id;
            cancellingInvoice.INVOICEID = deleteInfo.InvoiceId;
            cancellingInvoice.CANCELBY = this.currentUser.Id;
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
        private AnnouncementMD MapAnnouncement(InvoiceInfo invoiceInfo,string reasion)
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
            announcementInfo.TotalAmount = invoiceInfo.Total;
            announcementInfo.ServiceName = invoiceInfo.InvoiceDetail.FirstOrDefault().ProductName;
            announcementInfo.Reasion = reasion;
            //announcementInfo.BeforeContain = invoiceInfo.Sum.ToString();
            //announcementInfo.ChangeContain = "-" + invoiceInfo.Sum.ToString();
            announcementInfo.AnnouncementType = 3;
            announcementInfo.AnnouncementStatus = (int)AnnouncementStatus.Approved;
            announcementInfo.CreatedBy = this.currentUser.Id;
            announcementInfo.ApprovedBy = this.currentUser.Id;
            announcementInfo.ProcessBy = this.currentUser.Id;
            announcementInfo.UpdateStatus = AnnouncementStatus.New;

            string announcementId = "BBHHD";
            //if (announcementInfo.AnnouncementType == 1)
            //    announcementId = "BBDCHD";
            //if (announcementInfo.AnnouncementType == 2)
            //    announcementId = "BBTTHD";
            announcementInfo.MinutesNo = this.autoNumberBO.CreateAutoNumber((long)invoiceInfo.CompanyId, announcementId, invoiceInfo.BranchId, true);

            return announcementInfo;
        }
        #endregion
    }
}
