using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Helper;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InvoiceServer.Business.BL
{
    public class ReleaseAnnouncementBO : IReleaseAnnouncementBO
    {
        #region Fields, Properties
        private readonly IReleaseAnnouncementRepository releaseAnnouncementRepository;
        private readonly IReleaseAnnouncementDetaiRepository releaseAnnouncementDetailRepository;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IDbTransactionManager transaction;
        private readonly PrintConfig config;
        private readonly IReportCancellingDetailRepository reportCancellingDetailRepository;
        private readonly AnnouncementBO announcementBO;
        private static readonly Logger logger = new Logger();
        private readonly List<int> listAnnouncementExceed = new List<int>();
        private readonly ISignatureRepository signatureRepository;
        private readonly ISignDetailRepository signDetailRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        #endregion

        #region Contructor

        public ReleaseAnnouncementBO(IRepositoryFactory repoFactory, PrintConfig config, UserSessionInfo userSessionInfo)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.releaseAnnouncementRepository = repoFactory.GetRepository<IReleaseAnnouncementRepository>();
            this.releaseAnnouncementDetailRepository = repoFactory.GetRepository<IReleaseAnnouncementDetaiRepository>();
            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.reportCancellingDetailRepository = repoFactory.GetRepository<IReportCancellingDetailRepository>();
            this.config = config;
            this.announcementBO = new AnnouncementBO(repoFactory, config, userSessionInfo);
            this.signatureRepository = repoFactory.GetRepository<ISignatureRepository>();
            this.signDetailRepository = repoFactory.GetRepository<ISignDetailRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
        }
        #endregion

        #region Methods

        #endregion

        #region Private Methods

        public IEnumerable<AnnouncementsRelease> FilterReleaseAnnouncement(long releaseId, long companyId, List<int> releaseStatus)
        {
            var releaseAnnouncementDetails = this.releaseAnnouncementDetailRepository.FilteAnnouncementRelease(releaseId, companyId, releaseStatus).ToList();
            return releaseAnnouncementDetails;
        }

        public ServerSignAnnounResult SignAnnounFile(ReleaseAnnouncementMaster releaseAnnouncement, UserSessionInfo currentUser)
        {
            string result = "Can't connect to server";
            ServerSignAnnounResult serverSignResult = new ServerSignAnnounResult();
            try
            {
                releaseAnnouncement.CompanyId = releaseAnnouncement.ReleaseAnnouncementInfos.FirstOrDefault().CompanyId == 0 ? releaseAnnouncement.CompanyId : releaseAnnouncement.ReleaseAnnouncementInfos.FirstOrDefault().CompanyId;
                logger.Error("CompanyId: " + releaseAnnouncement.CompanyId, new Exception("SignAnnounFile"));
                //create invoice realease
                Result release = Create(releaseAnnouncement);
                //get invoice realease
                List<int> releaseStatus = new List<int>() { (int)ReleaseAnnouncementStatus.New, (int)ReleaseAnnouncementStatus.SignError };
                IEnumerable<AnnouncementsRelease> announcementsRelease = this.releaseAnnouncementDetailRepository.FilteAnnouncementRelease(release.ReleaseId, releaseAnnouncement.CompanyId ?? 0, releaseStatus);
                string pathFilePdf = String.Empty;
                string pathFile = String.Empty;

                transaction.BeginTransaction();
                if (releaseAnnouncement.ReleaseAnnouncementInfos.Count > 0)
                {
                    UpdateStatustAfterSignAnnouncement(announcementsRelease, release, currentUser);

                    foreach (var item in releaseAnnouncement.ReleaseAnnouncementInfos)
                    {
                        var companyInfo = this.myCompanyRepository.GetById(releaseAnnouncement.CompanyId ?? 0);
                        // truong hop la PGD thi get lai companyName hien thi tai phan chu ky
                        var companyNameSign = companyInfo.COMPANYNAME;
                        // truong hop la PGD thi get thong tin cua chi nhanh
                        //companyInfo = GetCompanyInfo(companyInfo);

                        AnnouncementDownload announcementPrint = this.announcementRepository.GetAnnouncementPrintInfo(item.AnnouncementId, companyInfo);
                        if (announcementPrint.AnnouncementStatus == (int)AnnouncementStatus.Released)
                        {
                            //get filepath
                            pathFilePdf = getPathFilePdfSign(item.AnnouncementId, announcementPrint.CompanyId);
                            string folderTree = PathUtil.FolderTree(announcementPrint.AnnouncementDate);
                            string signedPdfPath = pathFilePdf.Replace("\\Announcements\\", "\\Releases\\SignAnnouncement\\" + folderTree + "\\").Replace(".pdf", "_sign.pdf");
                            string onlyPath = Directory.GetParent(signedPdfPath).FullName;
                           
                            CreateDirectory(onlyPath);

                            //SIGNATURE signature = new SIGNATURE();

                            if (companyInfo.LEVELCUSTOMER == "PGD")
                            {
                                companyInfo = GetCompanyInfo(companyInfo);
                                companyNameSign = companyInfo.COMPANYNAME;
                                //signature = this.signatureRepository.GetByCompany((long)companyInfo.COMPANYID);
                            }
                            //else
                            //{
                            //    signature = this.signatureRepository.GetByCompany((long)companyInfo.COMPANYSID);
                            //}

                            logger.Error("file path " + pathFilePdf, new Exception("file path"));
                            //call api sign
                            //result = callApiServerSign(pathFilePdf, false, companyNameSign, item.AnnouncementId, signature, folderTree+"\\");
                            var signresult = SignAnnouncement(pathFilePdf, companyNameSign, signedPdfPath);
                            result = signresult ? "True": "False";
                            if (File.Exists(pathFilePdf))
                            {
                                File.Delete(pathFilePdf);
                            }
                            // Add SignDetail
                            //item.CompanyName = companyNameSign;
                            //item.SerialNumber = signature.SERIALNUMBER;
                        }
                    }

                    if (result == "True")
                    {
                        transaction.Commit();
                        //update status
                        //AddSignDetail(releaseAnnouncement);
                    }
                    else
                    {
                        transaction.Rollback();
                        DeletePathFile(pathFile);
                    }
                }
                else
                {
                    transaction.Rollback();
                }
                serverSignResult.AnnouncementSuccess = this.listAnnouncementExceed.Count;
                serverSignResult.AnnouncementFail = 0;
                serverSignResult.Message = result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.Error(currentUser.UserId, ex);
            }
            return serverSignResult;
        }


        public ServerSignAnnounResult SignAnnounFileMultipleCancel(ReleaseAnnouncementMaster releaseAnnouncement, UserSessionInfo currentUser)
        {
            string result = "Can't connect to server";
            ServerSignAnnounResult serverSignResult = new ServerSignAnnounResult();
            try
            {
                releaseAnnouncement.CompanyId = releaseAnnouncement.ReleaseAnnouncementInfos.FirstOrDefault().CompanyId == 0 ? releaseAnnouncement.CompanyId : releaseAnnouncement.ReleaseAnnouncementInfos.FirstOrDefault().CompanyId;
                logger.Error("CompanyId: " + releaseAnnouncement.CompanyId, new Exception("SignAnnounFile"));
                //create invoice realease
                Result release = Create(releaseAnnouncement);
                //get invoice realease
                List<int> releaseStatus = new List<int>() { (int)ReleaseAnnouncementStatus.New, (int)ReleaseAnnouncementStatus.SignError };
                IEnumerable<AnnouncementsRelease> announcementsRelease = this.releaseAnnouncementDetailRepository.FilteAnnouncementRelease(release.ReleaseId, releaseAnnouncement.CompanyId ?? 0, releaseStatus);
                string pathFilePdf = String.Empty;
                string pathFile = String.Empty;

                transaction.BeginTransaction();
                if (releaseAnnouncement.ReleaseAnnouncementInfos.Count > 0)
                {
                    UpdateStatustAfterSignAnnouncement(announcementsRelease, release, currentUser);

                    foreach (var item in releaseAnnouncement.ReleaseAnnouncementInfos)
                    {
                        var companyInfo = this.myCompanyRepository.GetById(releaseAnnouncement.CompanyId ?? 0);
                        // truong hop la PGD thi get lai companyName hien thi tai phan chu ky
                        var companyNameSign = companyInfo.COMPANYNAME;
                        // truong hop la PGD thi get thong tin cua chi nhanh
                        //companyInfo = GetCompanyInfo(companyInfo);

                        AnnouncementDownload announcementPrint = this.announcementRepository.GetAnnouncementPrintInfo(item.AnnouncementId, companyInfo);
                        if (announcementPrint.AnnouncementStatus == (int)AnnouncementStatus.Released)
                        {
                            //get filepath
                            pathFilePdf = getPathFilePdfSign(item.AnnouncementId, announcementPrint.CompanyId);
                            string folderTree = PathUtil.FolderTree(announcementPrint.AnnouncementDate);
                            string signedPdfPath = pathFilePdf.Replace("\\Announcements\\", "\\Releases\\SignAnnouncement\\" + folderTree + "\\").Replace(".pdf", "_sign.pdf");
                            string onlyPath = Directory.GetParent(signedPdfPath).FullName;

                            CreateDirectory(onlyPath);

                            //SIGNATURE signature = new SIGNATURE();

                            if (companyInfo.LEVELCUSTOMER == "PGD")
                            {
                                companyInfo = GetCompanyInfo(companyInfo);
                                companyNameSign = companyInfo.COMPANYNAME;
                                //signature = this.signatureRepository.GetByCompany((long)companyInfo.COMPANYID);
                            }
                            //else
                            //{
                            //    signature = this.signatureRepository.GetByCompany((long)companyInfo.COMPANYSID);
                            //}

                            logger.Error("file path " + pathFilePdf, new Exception("file path"));
                            //call api sign
                            //result = callApiServerSign(pathFilePdf, false, companyNameSign, item.AnnouncementId, signature, folderTree+"\\");
                            var signresult = SignAnnouncement(pathFilePdf, companyNameSign, signedPdfPath);
                            result = signresult ? "True" : "False";
                            if (File.Exists(pathFilePdf))
                            {
                                File.Delete(pathFilePdf);
                            }
                            // Add SignDetail
                            //item.CompanyName = companyNameSign;
                            //item.SerialNumber = signature.SERIALNUMBER;
                        }
                    }

                    if (result == "True")
                    {
                        transaction.Commit();
                        //update status
                        //AddSignDetail(releaseAnnouncement);
                    }
                    else
                    {
                        transaction.Rollback();
                        DeletePathFile(pathFile);
                    }
                }
                else
                {
                    transaction.Rollback();
                }
                serverSignResult.AnnouncementSuccess = this.listAnnouncementExceed.Count;
                serverSignResult.AnnouncementFail = 0;
                serverSignResult.Message = result;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.Error(currentUser.UserId, ex);
            }
            return serverSignResult;
        }
        private void DeletePathFile(string pathFile)
        {
            if (File.Exists(pathFile))
            {
                File.Delete(pathFile);
            }
        }

        private MYCOMPANY GetCompanyInfo(MYCOMPANY companyInfo)
        {
            if (companyInfo.LEVELCUSTOMER == LevelCustomerInfo.TransactionOffice)
            {
                companyInfo = this.myCompanyRepository.GetById((long)companyInfo.COMPANYID);
            }
            return companyInfo;
        }
        private void CreateDirectory(string onlyPath)
        {
            if (!Directory.Exists(onlyPath))
            {
                Directory.CreateDirectory(onlyPath);
            }
        }

        private void AddSignDetail(ReleaseAnnouncementMaster releaseAnnouncement)
        {
            foreach (var item in releaseAnnouncement.ReleaseAnnouncementInfos)
            {
                this.AddSignDetail(item.AnnouncementId, item.SerialNumber, item.CompanyName);
            }
        }

        public Result Create(ReleaseAnnouncementMaster info, ReleaseAnnouncementStatus status = ReleaseAnnouncementStatus.New)
        {
            Result resultRelease = new Result();
            try
            {
                transaction.BeginTransaction();
                RELEASEANNOUNCEMENT announcementConclude = InsertReleaseAnnouncement(info, status);
                InsertReleaseAnnouncementDetail(announcementConclude.ID, announcementConclude.COMPANYID.Value, info.ReleaseAnnouncementInfos);
                resultRelease.Count = info.ReleaseAnnouncementInfos.Count;
                resultRelease.ReleaseId = announcementConclude.ID;
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return resultRelease;
        }

        private RELEASEANNOUNCEMENT InsertReleaseAnnouncement(ReleaseAnnouncementMaster info, ReleaseAnnouncementStatus status)
        {
            if (info == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!info.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            RELEASEANNOUNCEMENT releaseAnnouncement = new RELEASEANNOUNCEMENT();
            releaseAnnouncement.CopyData(info);
            releaseAnnouncement.STATUS = (int)status;
            releaseAnnouncement.RELEASEDDATE = DateTime.Now;
            releaseAnnouncement.RELEASEBY = info.UserActionId;
            this.releaseAnnouncementRepository.Insert(releaseAnnouncement);
            return releaseAnnouncement;
        }

        private void InsertReleaseAnnouncementDetail(long releaseAnnouncementId, long companyId, List<ReleaseAnnouncementInfo> releaseAnnouncementDetails)
        {
            releaseAnnouncementDetails.ForEach(p =>
            {
                ANNOUNCEMENT currentAnnouncement = GetAnnouncement(p.AnnouncementId);
                var currentInvoice = GetInvoice(currentAnnouncement.INVOICEID);
                if (currentInvoice.INVOICESTATUS == (int)InvoiceStatus.Cancel)
                {
                    throw new BusinessLogicException(ResultCode.AnnouncementInvoiceCancel, "Hóa đơn của biên bản này đã bị hủy, bạn không thể ký hóa đơn này.");
                }
                List<string> symbols = GetSymbolsInReportCancellingInvoice(companyId, currentAnnouncement.REGISTERTEMPLATEID);
                if (symbols.Count > 0)
                {
                    foreach (var item in symbols)
                    {
                        if (currentAnnouncement.SYMBOL == item)
                        {
                            throw new BusinessLogicException(ResultCode.InvoiceNotApproveSymbol, "Dãy hóa đơn đã bị hủy,không thể thực hiện được chức năng phê duyệt/ký hóa đơn.");
                        }
                    }
                }
                RELEASEANNOUNCEMENTDETAIL releaseAnnouncementDetail = new RELEASEANNOUNCEMENTDETAIL();
                releaseAnnouncementDetail.ANNOUNCEMENTID = currentAnnouncement.ID;
                releaseAnnouncementDetail.RELEASEANNOUNCEMENTID = releaseAnnouncementId;
                releaseAnnouncementDetail.MESSAGES = "Hệ thống đang ký biên bản";
                this.releaseAnnouncementDetailRepository.Insert(releaseAnnouncementDetail);
                UpdateAnnouncement(currentAnnouncement);
            });
        }

        private ANNOUNCEMENT GetAnnouncement(long id)
        {
            ANNOUNCEMENT announcement = this.announcementRepository.GetByOnlyId(id);
            if (announcement == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This announcement id [{0}] not found in data of client", id));
            }
            return announcement;
        }

        private INVOICE GetInvoice(long id)
        {
            INVOICE invoice = this.invoiceRepository.GetById(Convert.ToInt32(id));
            if (invoice == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This invoice id [{0}] not found in data of client", id));
            }
            return invoice;
        }

        private List<string> GetSymbolsInReportCancellingInvoice(long companyId, long invoiceTemplateId)
        {
            IEnumerable<REPORTCANCELLINGDETAIL> reportCancelling = this.reportCancellingDetailRepository.Filter(companyId, invoiceTemplateId);
            return reportCancelling.Select(p => p.SYMBOL).ToList();

        }

        private void UpdateAnnouncement(ANNOUNCEMENT currentAnnouncement)
        {
            this.announcementRepository.Update(currentAnnouncement);
        }

        private string getPathFilePdfSign(long announcementId, long companyId)
        {
            //get path file
            ExportFileInfo file = this.announcementBO.PrintView(announcementId, companyId, false);

            ExportFileInfo fileInfoPdf = new ExportFileInfo();
            string fullPathFileContainContract = CreateFolderContainReport(companyId);
            fileInfoPdf.FileName = string.Format("{0}.pdf", announcementId);
            fileInfoPdf.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfoPdf.FileName);
            Spire.Doc.Document document = new Spire.Doc.Document();
            document.LoadFromFile(file.FullPathFileName);
            document.SaveToFile(fileInfoPdf.FullPathFileName, FileFormat.PDF);
          
            return Path.Combine(fileInfoPdf.FullPathFileName);
        }

        private string getPathFilePdfSignMultipleCancel(long announcementId, long companyId)
        {
            //get path file
            ExportFileInfo file = this.announcementBO.PrintView(announcementId, companyId, false);

            ExportFileInfo fileInfoPdf = new ExportFileInfo();
            string fullPathFileContainContract = CreateFolderContainReport(companyId);
            fileInfoPdf.FileName = string.Format("{0}.pdf", announcementId);
            fileInfoPdf.FullPathFileName = Path.Combine(fullPathFileContainContract, fileInfoPdf.FileName);
            Spire.Doc.Document document = new Spire.Doc.Document();
            document.LoadFromFile(file.FullPathFileName);
            document.SaveToFile(fileInfoPdf.FullPathFileName, FileFormat.PDF);

            return Path.Combine(fileInfoPdf.FullPathFileName);
        }
        private bool SignAnnouncement(string pathFile, string branchName, string pathSign)
        {
            var listTextDetect1 = new List<string>()
                {
                    "ĐẠI DIỆN BÊN A",
                    "ĐẠI DIỆN BÊN B",
                };
            var locationFound = PdfGetTextData.FindLastLocationOfListTextReverse(pathFile, listTextDetect1, 10);
            //string fileName = fileInfo.FullPathFileName.Replace("signSwitchTemp.pdf", "signSwitch.pdf");
            using (Stream inputPdfStream = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(pathSign, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                var pdfContentByte = stamper.GetOverContent(locationFound.Page);
                var image = iTextSharp.text.Image.GetInstance(Path.Combine(this.config.FullPathFileAsset, "rectangle.png"));
                //float signPositionX = 275;// pdfContentByte.PdfDocument.PageSize.Width - 30;
                image.SetAbsolutePosition(locationFound.X - 60f, locationFound.Y - 80f);
                image.ScalePercent(70);
                pdfContentByte.AddImage(image);

                var imageCheck = iTextSharp.text.Image.GetInstance(Path.Combine(this.config.FullPathFileAsset, "check-valid.png"));
                //float signPositionX = 275;// pdfContentByte.PdfDocument.PageSize.Width - 30;
                imageCheck.SetAbsolutePosition(locationFound.X + 16f, locationFound.Y - 60f);
                imageCheck.ScalePercent(100);
                pdfContentByte.AddImage(imageCheck);

                string fontPath = Path.Combine(this.config.FullPathFileAsset, "timesbd.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                // Thêm mục chữ ký người chuyển đổi vào page cuối

                string textConverter1 = "Signature valid";
                var fontConverter1 = new Font(bf, 8.25f, Font.NORMAL, new BaseColor(0, 0, 255));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter1, fontConverter1), locationFound.X + 50f, locationFound.Y - 40f, 0);
                string textConverter2 = "Đã được ký điện tử bởi";
                var fontConverter2 = new Font(bf, 8.25f, Font.NORMAL, new BaseColor(0, 0, 255));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverter2, fontConverter2), locationFound.X + 46f, locationFound.Y - 50f, 0);

                string textConverterName = branchName;
                var fontConverterName = new Font(bf, 8.25f, Font.NORMAL, new BaseColor(0, 0, 255));
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textConverterName, fontConverterName), locationFound.X + 44f, locationFound.Y - 61f, 0);
                string textDateConvert = "Ngày ký: " + DateTime.Today.ToString("dd/MM/yyyy");
                ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(textDateConvert, fontConverterName), locationFound.X + 48f, locationFound.Y - 71, 0);

                //pdfContentByte.SetLineWidth(0.8d);
                //pdfContentByte.MoveTo(300 - 70, locationFound.Y - 17.9);
                //pdfContentByte.LineTo(300 + 70, locationFound.Y - 17.9);
                pdfContentByte.ClosePathStroke();

                stamper.Close();
                reader.Close();
            }
            return File.Exists(pathSign);
        }
        private string CreateFolderContainReport(long companyId)
        {
            string fullPathForlder = Path.Combine(WebConfigurationManager.AppSettings["FolderInvoiceFile"].ToString(), companyId.ToString(), "Announcements");
            if (!Directory.Exists(fullPathForlder))
            {
                Directory.CreateDirectory(fullPathForlder);
            }

            return fullPathForlder;
        }

        private string callApiServerSign(string pathFilePdf, bool clientSign, string companyName, long announcementId, SIGNATURE signature, string dateFolder)
        {
            bool SignHsm = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                FilePathPdf = pathFilePdf,
                ClientSign = clientSign,
                CompanyName = companyName,
                AnnouncementId = announcementId,
                SerialNumber = signature.SERIALNUMBER,
                DateSign = DateTime.Now.ToString("dd/MM/yyyy"),
                SignHSM = SignHsm,
                Slot = signature.SLOTS,
                DateFolder = dateFolder
            });
            string response = String.Empty;
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSignAnnouncement"], ApiSignAnnouncement.ActionServerSign, ApiSignAnnouncement.ActionSignAnnoun);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            //request.Timeout = 
            using (Stream webStream = request.GetRequestStream())
            //using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                webStream.Write(byteArray, 0, byteArray.Length);
                webStream.Close();
            }
            WebResponse webResponse = request.GetResponse();
            using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                StreamReader responseReader = new StreamReader(webStream);
                response = responseReader.ReadToEnd().ToString();
                response = JsonConvert.DeserializeObject(response).ToString();
            }
            return response.ToString();
        }

        private void UpdateStatustAfterSignAnnouncement(IEnumerable<AnnouncementsRelease> announcementsRelease, Result release, UserSessionInfo currentUser)
        {
            //update status

            StatusReleaseAnnouncement statusReleaseAnnouncement = new StatusReleaseAnnouncement();
            statusReleaseAnnouncement.AnnouncementsRelease = announcementsRelease.ToList();
            statusReleaseAnnouncement.AnnouncementsRelease.ForEach(p =>
            {
                p.Signed = true;
                p.VerificationCode = GetVerificationCode(p.AnnouncementId);
            });
            UpdateStatusReleaseAnnouncementDetail(release.ReleaseId, statusReleaseAnnouncement, currentUser);
        }

        public ResultCode UpdateStatusReleaseAnnouncementDetail(long releaseId, StatusReleaseAnnouncement statusReleaseAnnouncement, UserSessionInfo currentUser)
        {
            RELEASEANNOUNCEMENT releaseAnnouncement = GetReleaseAnnouncementById(releaseId);
            if (releaseAnnouncement == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return UpdateStatusReleaseAnnouncementDetail(releaseId, statusReleaseAnnouncement.AnnouncementsRelease);
        }

        public RELEASEANNOUNCEMENT GetReleaseAnnouncementById(long id)
        {
            return this.releaseAnnouncementRepository.GetById(id);
        }
        public RELEASEANNOUNCEMENTDETAIL GetReleaseAnnouncementDetail(long AnnouId)
        {
            return this.releaseAnnouncementDetailRepository.GetReleaseAnnouncementDetail(AnnouId);
        }
        private ResultCode UpdateStatusReleaseAnnouncementDetail(long releaseId, List<AnnouncementsRelease> announcementsRelease)
        {
            announcementsRelease.ForEach(p =>
            {
                RELEASEANNOUNCEMENTDETAIL currentReleaseAnnouncementDetail = GetReleaseAnnouncementDetail(releaseId, p.AnnouncementId);
                currentReleaseAnnouncementDetail.SIGNED = p.Signed;
                currentReleaseAnnouncementDetail.VERIFICATIONCODE = p.VerificationCode;
                this.releaseAnnouncementDetailRepository.Update(currentReleaseAnnouncementDetail);
                if (p.Signed)
                {
                    UpdateStatusAnnouncement(p.AnnouncementId, (int)AnnouncementStatus.Released);
                }
            });

            return ResultCode.NoError;
        }

        private RELEASEANNOUNCEMENTDETAIL GetReleaseAnnouncementDetail(long releaseId, long announcementId)
        {
            RELEASEANNOUNCEMENTDETAIL releaseAnnouncementDetail = this.releaseAnnouncementDetailRepository.FilteReleaseAnnouncementDetail(releaseId, announcementId);
            if (releaseAnnouncementDetail == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return releaseAnnouncementDetail;
        }

        private void UpdateStatusAnnouncement(long announcementId, int announcementStatus)
        {
            var currentAnnouncement = GetAnnouncement(announcementId);
            if (currentAnnouncement.COMPANYSIGN != true)
            {
                this.listAnnouncementExceed.Add(Convert.ToInt32(announcementId));
            }
            if (currentAnnouncement.ANNOUNCEMENTSTATUS != (int)AnnouncementStatus.Successfull)
            {
                currentAnnouncement.ANNOUNCEMENTSTATUS = announcementStatus;
                currentAnnouncement.COMPANYSIGNDATE = DateTime.Now;
                currentAnnouncement.COMPANYSIGN = true;
                this.announcementRepository.Update(currentAnnouncement);
            }
        }

        private string GetVerificationCode(long announcementId)
        {
            string verificationCode = string.Format("{0}{1}", DateTime.Now.ToString(), announcementId).GetHashCode().ToString("x");
            return verificationCode.ToUpper();
        }

        public ResultCode Update(long id, long companyId, StatusRelease statusRelease)
        {
            var currentReleaseAnnouncement = GetReleaseAnnouncementById(id);
            if (currentReleaseAnnouncement == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            currentReleaseAnnouncement.STATUS = statusRelease.Status;
            currentReleaseAnnouncement.RELEASESTATUS = statusRelease.Message;
            currentReleaseAnnouncement.RELEASEBY = statusRelease.LoginId;
            this.releaseAnnouncementRepository.Update(currentReleaseAnnouncement);
            return ResultCode.NoError;
        }

        public RELEASEANNOUNCEMENT GetReleaseAnnouncementByAnnouncementId(long announcementId)
        {
            return this.releaseAnnouncementRepository.GetByAnnouncementId(announcementId);

        }

        private void AddSignDetail(long announcementId, string serialNumber, string companyName)
        {
            var signDetail = new SIGNDETAIL()
            {
                ANNOUNCEMENTID = announcementId,
                SERIALNUMBER = serialNumber,
                NAME = companyName,
                ISCLIENTSIGN = false,
                CREATEDDATE = DateTime.Now,
                TYPESIGN = (int)SignDetailTypeSign.Announcement,
            };
            this.signDetailRepository.Insert(signDetail);
        }

        #endregion
    }
}