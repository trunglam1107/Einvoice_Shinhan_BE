using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.ExportInvoice;
using InvoiceServer.Business.ExportXml;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InvoiceServer.Business.BL
{
    public class DeclarationBO : IDeclarationBO
    {
        #region Fields, Properties

        private readonly IDeclarationRepository DeclarationRepository;
        private readonly IDeclarationReleaseRepository DeclarationReleaseRepository;
        private readonly ICityRepository CityRepository;
        private readonly ITaxDepartmentRepository TaxDepartmentRepository;
        private readonly IInvoiceDeclarationReleaseRegisterTypeReposity InvoiceDeclarationReleaseRegisterTypeReposity;
        private readonly IDbTransactionManager transaction;
        private readonly UserSessionInfo currentUser;
        string folderStore = ConfigurationManager.AppSettings["FolderInvoiceFile"];
        private readonly ISignatureRepository signatureRepository;
        private static readonly Logger logger = new Logger();
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly ICompanySymbolRepository compSymbolRepository;
        #endregion

        #region Contructor
        public DeclarationBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");

            this.DeclarationRepository = repoFactory.GetRepository<IDeclarationRepository>();
            this.DeclarationReleaseRepository = repoFactory.GetRepository<IDeclarationReleaseRepository>();
            this.CityRepository = repoFactory.GetRepository<ICityRepository>();
            this.TaxDepartmentRepository = repoFactory.GetRepository<ITaxDepartmentRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.InvoiceDeclarationReleaseRegisterTypeReposity = repoFactory.GetRepository<IInvoiceDeclarationReleaseRegisterTypeReposity>();
            this.signatureRepository = repoFactory.GetRepository<ISignatureRepository>();
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.compSymbolRepository = repoFactory.GetRepository<ICompanySymbolRepository>();
        }
        public DeclarationBO(IRepositoryFactory repoFactory, UserSessionInfo userSessionInfo)
            : this(repoFactory)
        {
            this.currentUser = userSessionInfo;
        }
        #endregion
        #region Method
        public IEnumerable<DeclarationInfo> Filter(ConditionSearchDeclaration condition, int skip = 0, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var Declaration = this.DeclarationRepository.FilterDeclaration(condition, skip, take).ToList();
            return Declaration;
        }
        public List<CompanySymbolInfo> ListSymbol(long companyId)
        {
            return this.DeclarationRepository.ListSymbol(companyId);
        }
        public DeclarationInfo GetDeclarationInfo(long id, long? companyId)
        {
            INVOICEDECLARATION currentDeclaration = GetDeclaration(id);
            IQueryable<SYMBOL> currentSymbol = this.DeclarationRepository.GetbyRefId(id);
            var INVOICEDECLARATIONRELEASEs = currentDeclaration.INVOICEDECLARATIONRELEASEs.Where(p => p.ISDELETED == null || p.ISDELETED == false);
            var COMPANYSYMBOLINFOs = currentDeclaration.SYMBOLs;

            DeclarationInfo declarationInfo = new DeclarationInfo(currentDeclaration);
            var city = this.CityRepository.GetById(currentDeclaration.CITYID);
            var tax = this.TaxDepartmentRepository.GetById(currentDeclaration.TAXDEPARTMENTID);
            var listCompany = this.myCompanyRepository.GetListByCompanyId(currentDeclaration.COMPANYID);
            List<DeclarationReleaseInfo> DeclarationReleaseInfos = INVOICEDECLARATIONRELEASEs.Select(p => new DeclarationReleaseInfo(p)).ToList();
            //List<CompanySymbolInfo> CompanySymbolInfos = (from companySymbol in COMPANYSYMBOLINFOs
            //                                              join company in listCompany on companySymbol.COMPANYID equals company.COMPANYSID
            //                                              select new CompanySymbolInfo
            //                                              { 
            //                                                  Id = companySymbol.ID,
            //                                                  CompanyId=companySymbol.COMPANYID,
            //                                                  RefId = companySymbol.REFID,
            //                                                  CompanyName = company.COMPANYNAME,
            //                                                  Symbol = companySymbol.SYMBOL1
            //                                              }).ToList();

            List<CompanySymbolInfo> CompanySymbolInfos = (from company in listCompany
                                                          join cSymbol in currentSymbol on company.COMPANYSID equals cSymbol.COMPANYID
                                                          into cSymbols from cSymbol in cSymbols.DefaultIfEmpty()
                                                          select new CompanySymbolInfo
                                                          {
                                                              Id = cSymbol?.ID ,
                                                              CompanyId = company.COMPANYSID,
                                                              RefId = id,
                                                              CompanyName = (company.LEVELCUSTOMER == "HO" || company.LEVELCUSTOMER == "CN") ? company.COMPANYNAME : "----" + company.COMPANYNAME,
                                                              Symbol = cSymbol?.SYMBOL1 
                                                          }).ToList();




            declarationInfo = new DeclarationInfo(declarationInfo, DeclarationReleaseInfos, CompanySymbolInfos);
            if (city != null)
            {
                declarationInfo.ProvinceCity = city.NAME;
            }
            if (tax != null)
            {
                declarationInfo.TaxCompanyName = tax.NAME;
                declarationInfo.TaxCompanyCode = tax.CODE;
            }

            return declarationInfo;
        }

        public DeclarationInfo GetDeclarationLast(long? companyId)
        {
            INVOICEDECLARATION currentDeclaration = this.DeclarationRepository.GetLastInfo(companyId);
            DeclarationInfo declarationInfo = null;
            if (currentDeclaration == null)
            {
                declarationInfo = new DeclarationInfo(currentDeclaration);
            }
            else
            {
                var INVOICEDECLARATIONRELEASEs = currentDeclaration.INVOICEDECLARATIONRELEASEs.Where(p => p.ISDELETED == null || p.ISDELETED == false);

                declarationInfo = new DeclarationInfo(currentDeclaration);
                var city = this.CityRepository.GetById(currentDeclaration.CITYID);
                var tax = this.TaxDepartmentRepository.GetById(currentDeclaration.TAXDEPARTMENTID);

                List<DeclarationReleaseInfo> DeclarationReleaseInfos = INVOICEDECLARATIONRELEASEs.Select(p => new DeclarationReleaseInfo(p)).ToList();
                declarationInfo = new DeclarationInfo(declarationInfo, DeclarationReleaseInfos,null);
                if (city != null)
                {
                    declarationInfo.ProvinceCity = city.NAME;
                }
                if (tax != null)
                {
                    declarationInfo.TaxCompanyName = tax.NAME;
                    declarationInfo.TaxCompanyCode = tax.CODE;
                }

            }

            return declarationInfo;
        }
        private INVOICEDECLARATION GetDeclaration(long id)
        {
            INVOICEDECLARATION declaration = this.DeclarationRepository.GetById(id);
            if (declaration == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            return declaration;
        }
        private IEnumerable<INVOICEDECLARATIONRELEASE> GetListDeclarationReleasse(long id)
        {
            return this.DeclarationReleaseRepository.GetList(id);
        }

        private IEnumerable<SYMBOL> GetListCompanySymbol(long id)
        {
            return this.compSymbolRepository.GetList(id);
        }

        public IEnumerable<RegisterType> GetRegisterTypes()
        {
            return this.InvoiceDeclarationReleaseRegisterTypeReposity.GetAll()
                .ToList()
                .Select(x => new RegisterType(x));
        }

        public string Approve(long INVOICEDECLARATIONID)
        {
            var INVOICEDECLARATION = this.DeclarationRepository.GetById(INVOICEDECLARATIONID);
            if (INVOICEDECLARATION == null)
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataInvalid);

            if (INVOICEDECLARATION.STATUS != DeclarationStatus.New)
                throw new BusinessLogicException(ResultCode.InvoiceDeclarationStatusIsNotAddNew, MsgApiResponse.DataInvalid);

            INVOICEDECLARATION.STATUS = DeclarationStatus.Approve;
            this.DeclarationRepository.Update(INVOICEDECLARATION);
            return "";
        }

        public string RevertApprove(long INVOICEDECLARATIONID)
        {
            var INVOICEDECLARATION = this.DeclarationRepository.GetById(INVOICEDECLARATIONID);
            if (INVOICEDECLARATION == null)
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataInvalid);

            if (INVOICEDECLARATION.STATUS != DeclarationStatus.Approve)
                throw new BusinessLogicException(ResultCode.InvoiceDeclarationStatusIsNotApprove, MsgApiResponse.DataInvalid);

            INVOICEDECLARATION.STATUS = DeclarationStatus.New;
            this.DeclarationRepository.Update(INVOICEDECLARATION);
            return "";
        }


        public long Count(ConditionSearchDeclaration condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.DeclarationRepository.CountDeclaration(condition);
        }
        public ResultCode Create(DeclarationInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                INVOICEDECLARATION Declaration = InsertDeclaration(info);
                InsertDeclarationReleaseList(Declaration.ID, info.releaseInfoes);
                InsertComSymbol(Declaration.ID, info.companySymbolInfos);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error(this.currentUser.UserId, ex);
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;


        }
        public ResultCode Update(long id, DeclarationInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                UpdateDeclaration(id, info);
                DeleteDeclarationReleaseInfo(id);
                InsertDeclarationReleaseList(id, info.releaseInfoes);
                DeleteCompanySymbol(id);
                InsertComSymbol(id , info.companySymbolInfos);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;

        }

        public ResultCode UpdateSymbol(long id, DeclarationInfo info)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteCompanySymbol(id);
                InsertComSymbol(id, info.companySymbolInfos);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public ResultCode UpdateStatus(long id, int? status)
        {
            try
            {
                transaction.BeginTransaction();
                INVOICEDECLARATION Declaration = DeclarationRepository.GetById(id);
                Declaration.STATUS = status;
                Declaration.UPDATEDATE = DateTime.Now;
                this.DeclarationRepository.Update(Declaration);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;

        }
        public ResultCode Delete(long id, long? companyId)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteDeclarationInfo(id, companyId);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }
        private INVOICEDECLARATION InsertDeclaration(DeclarationInfo info)
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

            INVOICEDECLARATION declaration = new INVOICEDECLARATION();
            declaration.CopyData(info);
            declaration.STATUS = (int)DeclarationStatus.New;
            declaration.DECLARATIONDATE = DateTime.Now;
            declaration.UPDATEBY = currentUser.UserName;
            this.DeclarationRepository.Insert(declaration);
            return declaration;
        }
        private void InsertDeclarationReleaseList(long DeclarationId, List<DeclarationReleaseInfo> DeclarationReleaseInfoes)
        {
            ResultCode errorCode;
            string errorMessage;
            DeclarationReleaseInfoes.ForEach(info =>
            {
                if (!info.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }
                INVOICEDECLARATIONRELEASE declaration = new INVOICEDECLARATIONRELEASE();
                declaration.CopyData(info);
                declaration.DECLARATIONID = DeclarationId;
                this.DeclarationReleaseRepository.Insert(declaration);
            });
        }

        public void InsertComSymbol(long DeclarationId, List<CompanySymbolInfo> companySymbolInfos)
        {
            ResultCode errorCode;
            string errorMessage;
            companySymbolInfos.ForEach(info =>
            {
                if (!info.IsValid(out errorCode, out errorMessage))
                {
                    throw new BusinessLogicException(errorCode, errorMessage);
                }

                SYMBOL comsym = new SYMBOL();
                if (info.Symbol != null)
                {
                    comsym.CopyData(info, DeclarationId);
                    //comsym.REFID = DeclarationId;
                    this.compSymbolRepository.Insert(comsym);
                }
               
            });
        }
        private void UpdateDeclaration(long id, DeclarationInfo info)
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
            INVOICEDECLARATION declaration = GetDeclaration(id);
            declaration.CopyData(info);
            declaration.UPDATEDATE = DateTime.Now;
            declaration.UPDATEBY = currentUser.UserName;
            this.DeclarationRepository.Update(declaration);
        }
        private void DeleteDeclarationInfo(long id, long? companyId)
        {
            var declaration = GetDeclaration(id);

            if (declaration.STATUS.HasValue && declaration.STATUS.Value == (int)DeclarationStatus.Completed)
            {
                throw new BusinessLogicException(ResultCode.ReleaseInvoiceApprovedNotUpdate, "The sent declaration is not delete");
            }

            declaration.ISDELETED = true;
            declaration.UPDATEDATE = DateTime.Now;
            declaration.UPDATEBY = currentUser.UserName;
            this.DeclarationRepository.Update(declaration);
            DeleteDeclarationReleaseInfo(id);
        }
        private void DeleteDeclarationReleaseInfo(long id)
        {
            var declarationDetails = this.GetListDeclarationReleasse(id);

            declarationDetails.ForEach(p =>
            {
                p.ISDELETED = true;
                this.DeclarationReleaseRepository.Update(p);
            });

        }

        private void DeleteCompanySymbol(long id)
        {
            var lstCompanySymbols = this.GetListCompanySymbol(id);
            lstCompanySymbols.ForEach(p =>
            {
                this.compSymbolRepository.DeleteCompanySymbol((long)p.COMPANYID);
            });
        }

        public DeclarationMaster GetDeclarationSymbol(long? companyId)
        {
            var declare = this.DeclarationRepository.GetListSymboy(companyId).OrderByDescending(x => x.Id).FirstOrDefault();

            var releaseddate = this.DeclarationRepository.GetMaxDateInvoice(companyId, declare.symboyFinal);

            if (releaseddate == null)
            {
                //throw new BusinessLogicException(ResultCode.ReleaseDateInvalid, "Ngày hóa đơn không hợp lệ");
                releaseddate = DateTime.Now;
            }

            declare.Releaseddate = releaseddate;
            
            return declare;

        }
        public ExportFileInfo GetXmlFile(long id)
        {
            ExportFileInfo fileInfo;
            fileInfo = GetDelarationXmlFile(id);
            return fileInfo;
        }
        public ExportFileInfo GetXmlNotiFile()
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            fileInfo.FileName = "Cancel_Notification.xml";
            fileInfo.FullPathFileName = Path.Combine(DefaultFields.ASSET_FOLDER, fileInfo.FileName);
            return fileInfo;
        }
        public string GetXmlNotifullPathFile()
        {
            return Path.Combine(this.folderStore, this.currentUser.Company.Id.ToString(), "Notification");
        }
        public ExportFileInfo GetXmlNotiFileById(long id, long companyId)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            var folder = ConfigurationManager.AppSettings["pathServer"];
            fileInfo.FileName = id + "_sign.xml";
            fileInfo.FullPathFileName = Path.Combine(folder, "Data", companyId.ToString(), "Releases", "Sign", fileInfo.FileName);
            return fileInfo;
        }

        private ExportFileInfo GetDelarationXmlFile(long id)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();
            DeclarationInfo declaration = GetDeclarationInfo(id, null);
            var XMLContextob = ParseXML.RegisterInvoice(declaration);
            string XMLContext = "";
            if (XMLContextob != null)
            {
                XMLContext = XMLContextob.ToString();
            }
            fileInfo.FileName = string.Format("Declaration_{0}.xml", id);
            fileInfo.FullPathFileName = Path.Combine(this.folderStore, this.currentUser.Company.Id.ToString(), "Declaration", fileInfo.FileName);
            //if (File.Exists(fileInfo.FullPathFileName))
            //{
            //    return fileInfo;
            //}
            string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            if (!File.Exists(fileInfo.FullPathFileName))
            {
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }
            else
            {
                File.Delete(fileInfo.FullPathFileName);
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }

            return fileInfo;

        }
        public ResultCode UpdateAfterSign(DeclarationInfo declarationInfo)
        {
            var res = ResultCode.NoError;
            //if (string.IsNullOrEmpty(declarationInfo.XmlString))
            //{
            string folderSign = Path.Combine(this.folderStore, this.currentUser.Company.Id.ToString(), "Declaration", "Sign");
            if (!Directory.Exists(folderSign))
            {
                Directory.CreateDirectory(folderSign);
            }
            string filePath = Path.Combine(folderSign, string.Format("Declaration_{0}.xml", declarationInfo.ID));
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(declarationInfo.XmlString));
                res = UpdateStatus(declarationInfo.ID, DeclarationStatus.Signed);
            }

            //}
            return res;
        }

        public ResultCode SignDeclaration(long Id)
        {
            var res = ResultCode.NoError;
            ExportFileInfo fileInfo = new ExportFileInfo();
            DeclarationInfo declaration = GetDeclarationInfo(Id, null);
            var XMLContextob = ParseXML.RegisterInvoice(declaration);
            string XMLContext = "";
            if (XMLContextob != null)
            {
                XMLContext = XMLContextob.ToString();
            }
            fileInfo.FileName = declaration.Status == 6 ? string.Format("Declaration_{0}_sign.xml", Id) : string.Format("Declaration_{0}.xml", Id);
            fileInfo.FullPathFileName = Path.Combine(this.folderStore, this.currentUser.Company.Id.ToString(), "Declaration", fileInfo.FileName);

            string tempFolder = Path.GetDirectoryName(fileInfo.FullPathFileName);
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            if (!File.Exists(fileInfo.FullPathFileName))
            {
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }
            else
            {
                File.Delete(fileInfo.FullPathFileName);
                File.WriteAllText(fileInfo.FullPathFileName, XMLContext);
            }
            SIGNATURE signature = this.signatureRepository.GetByCompany((long)declaration.CompanyID);
            if (signature == null)
            {
                throw new BusinessLogicException(ResultCode.CaNotExisted, "Vui lòng upload file CA cho chi nhánh đang sử dụng");
            }


            string path = CallApiSignDecalaration(fileInfo.FullPathFileName, signature);
            if (File.Exists(path))
            {
                res = UpdateStatus(Id, DeclarationStatus.Signed);
            }
            else
            {
                res = UpdateStatus(Id, DeclarationStatus.Waiting);
            }
            return res;

        }
        public SIGNATURE GetSignature(long companyId)
        {
            return this.signatureRepository.GetByCompany(companyId);
        }

        public string CallApiSignDecalaration(string pathXml, SIGNATURE signature, string tagKey = null)
        {

            bool SignHsm = bool.Parse(WebConfigurationManager.AppSettings["SignHSM"] ?? "false");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                TagKey = tagKey == null ? "DLTKhai" : tagKey,
                TagSign = "NNT",
                SerialNumber = signature.SERIALNUMBER,
                PathFileXml = pathXml,
                Slot = signature.SLOTS,
                SignHSM = SignHsm
                //Certificatepath= certificatepath,
            });
            string response = String.Empty;
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionServerSignXml);
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

        public ResultCode CheckSendTwan(long id)
        {
            return this.DeclarationRepository.CheckSendTwan(id);
        }

        public ExportFileInfo getFileSignXML(long id)
        {
            ExportFileInfo fileInfo = new ExportFileInfo();

            fileInfo.FileName = string.Format("Declaration_{0}.xml", id);
            fileInfo.FullPathFileName = Path.Combine(this.folderStore, this.currentUser.Company.Id.ToString(), "Declaration", "Sign", fileInfo.FileName);
            if (!File.Exists(fileInfo.FullPathFileName))
            {
                return fileInfo;
            }

            return fileInfo;
        }


        public ExportFileInfo ExportPdf(long INVOICEDECLARATIONID, PrintConfig config)
        {
            var fullPathFile = Path.Combine(config.FullFolderAssetOfCompany, "dang-ky-su-dung-hoa-don-" + INVOICEDECLARATIONID.ToString() + ".pdf");
            if (File.Exists(fullPathFile))
            {
                File.Delete(fullPathFile);
            }

            ExportFileInfo fileInfo = BuildPdfFile(INVOICEDECLARATIONID, config);

            return fileInfo;
        }

        private ExportFileInfo BuildPdfFile(long INVOICEDECLARATIONID, PrintConfig config)
        {
            var INVOICEDECLARATION = GetDeclarationInfo(INVOICEDECLARATIONID, null);
            if (INVOICEDECLARATION == null)
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataInvalid);

            var exportInfo = new DeclarationExportPdfInfo
            {
                DeclarationTypeID = (INVOICEDECLARATION.DeclarationType ?? 0).ToString(),
                TaxPayerName = INVOICEDECLARATION.CompanyName,
                TaxCode = INVOICEDECLARATION.CompanyTaxCode,
                TaxDepartmentName = INVOICEDECLARATION.TaxCompanyName,
                PeronalContact = INVOICEDECLARATION.CustName,
                Tel1 = INVOICEDECLARATION.CustPhone,
                Address = INVOICEDECLARATION.CustAddress,
                Email = INVOICEDECLARATION.CustEmail,
                HTHDon = (INVOICEDECLARATION.HTHDon ?? 0).ToString(),
                HTGDLHDDT = (INVOICEDECLARATION.HTGDLHDDT ?? 0).ToString(),
                PThuc = (INVOICEDECLARATION.PThuc ?? 0).ToString(),
                LHDSDung = (INVOICEDECLARATION.LHDSDung ?? 0).ToString(),
                Details = new List<DeclarationExportPdfDetail>()
            };
            if (INVOICEDECLARATION.releaseInfoes != null
                && INVOICEDECLARATION.releaseInfoes.Count > 0)
            {
                var stt = 1;
                foreach (var releaseInfo in INVOICEDECLARATION.releaseInfoes)
                {
                    var detail = new DeclarationExportPdfDetail
                    {
                        STT = stt.ToString(),
                        ReleaseConpanyName = releaseInfo.ReleaseCompanyName,
                        Seri = releaseInfo.Seri,
                        FromDate = releaseInfo.fromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        ToDate = releaseInfo.toDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        RegisterTypeID = releaseInfo.RegisterTypeID.ToString()
                    };

                    exportInfo.Details.Add(detail);
                }
                stt++;
            }
            else
            {
                exportInfo.Details.Add(new DeclarationExportPdfDetail
                {
                    STT = "&nbsp;"
                });
            }




            var fullPathFile = Path.Combine(config.FullFolderAssetOfCompany, "dang-ky-su-dung-hoa-don-" + INVOICEDECLARATIONID.ToString() + ".pdf");
            var exportModel = new DeclarationExportModel
            {
                DataExport = exportInfo,
                TemplateFile = Path.Combine(config.FullPathFolderTemplateDeclaration, "invoice-declaration-template.html"),
                DetailTemplateFile = Path.Combine(config.FullPathFolderTemplateDeclaration, "invoice-declaration-detail-templates.html"),
                FilePath = fullPathFile
            };


            HQExportFile export = new HQExportFile(exportModel);
            var res = export.ExportDeclarationFile(exportModel);

            return res;
        }

        public DeclarationReleaseInfo UploadFile(long companyId)
        {
            var cert = loadcertificateFile(companyId);
            var releaseInfos = new DeclarationReleaseInfo()
            {
                Seri = cert.SerialNumber,
                ReleaseCompanyName = cert.issuer,
                fromDate = cert.FromDate,
                toDate = cert.ToDate,
                RegisterTypeID = 1
            };
            return releaseInfos;
        }



        public SignCert loadcertificateFile(long companyId)
        {
            SIGNATURE signature = GetSignature(companyId);
            if (signature == null)
            {
                throw new BusinessLogicException(ResultCode.CaNotExisted, "Vui lòng upload file CA cho chi nhánh đang sử dụng");
            }

            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = signature.PASSWORD,
                SerialNumber = signature.SERIALNUMBER,
                Slot = signature.SLOTS
            });
            SignCert respone = new SignCert();
            string DATA = json.ToString();
            string UrlServerSign = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionGetCertBySlot);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlServerSign);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
            request.ContentLength = byteArray.Length;
            request.Timeout = 18000000;//tăng timeout gọi api
            using (Stream webStream = request.GetRequestStream())
            //using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                webStream.Write(byteArray, 0, byteArray.Length);
                webStream.Close();
            }
            WebResponse webResponse = request.GetResponse();
            using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                StreamReader responseReader = new StreamReader(webStream);
                var responea = responseReader.ReadToEnd();
                respone = (SignCert)js.Deserialize(responea, typeof(SignCert));
            }
            return respone;
        }

        public ExportFileInfo DownloadExcelDeclare(ConditionSearchDeclaration condition)
        {
            var dataRport = new ReportExcelDeclare();
            dataRport.Items = this.DeclarationRepository.FilterDeclarationForExcel(condition).ToList();
            ReportExportAllDeclareView reportExportAllDeclareView = new ReportExportAllDeclareView(dataRport);
            return reportExportAllDeclareView.ExportFile();
        }
        public ExportFileInfo DownloadExcelSymbol(long companyId)
        {
            var data = this.DeclarationRepository.ListSymbol(companyId);
            ExportSymbol exportSymbol = new ExportSymbol(data);
            return exportSymbol.ExportFile();
        }
        public int GetExpireDateToken(long? companyId)
        {
            var date = this.DeclarationRepository.ExpireTokenDate(companyId ?? 0);
            var countDate = (date - DateTime.Now).TotalDays;

            var result = (countDate > 0 && countDate < 1)
                ? 1 : Convert.ToInt32(countDate);

            return result;
        }

        public DateTime ExpireDateToken(long? companyId)
        {
            var date = this.DeclarationRepository.ExpireTokenDate(companyId ?? 0);

            return date;
        }

        #endregion
    }
}
