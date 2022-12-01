using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InvoiceServer.Business.BL
{
    public class SignatureBO : ISignatureBO
    {
        #region Fields, Properties

        private readonly ISignatureRepository signatureRepository;
        private readonly Logger logger = new Logger();
        private readonly EmailConfig emailConfig;
        private readonly PrintConfig config;
        #endregion

        #region Contructor

        public SignatureBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.signatureRepository = repoFactory.GetRepository<ISignatureRepository>();
            this.config = config;
            this.emailConfig = emailConfig;
        }
        public SignatureBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.signatureRepository = repoFactory.GetRepository<ISignatureRepository>();
        }
        #endregion

        #region Methods
        public IEnumerable<SignatureInfo> FilterCA(ConditionSearchCA condition, int skip = int.MinValue, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            ProcessCertInfo();
            var signutares = this.signatureRepository.FilterCA(condition).Skip(skip).Take(take).ToList();
            
            return signutares.Select(p => new SignatureInfo(p));
        }
        public SIGNATURE GetSignature(SignatureInfo signatureInfo)
        {
            SIGNATURE signature = this.signatureRepository.GetSignatureByCompany(signatureInfo);
            if (signature == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, string.Format("This Signature [{0}] not found in data of client", signatureInfo.SerialNumber));
            }

            return signature;
        }

        public ResultCode CreateOrUpdate(SignatureInfo signatureInfo)
        {
            if (signatureInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            SIGNATURE signature = this.signatureRepository.GetSignatureByCompany(signatureInfo);
            if (signature == null)
            {
                return Create(signatureInfo);
            }

            return Update(signatureInfo);
        }
        public SignatureInfo GetSignatureInfo(long id)
        {
            SIGNATURE signature = this.signatureRepository.GetSignatureInfo(id);
            return new SignatureInfo(signature);
        }
        public SIGNATURE GetByCompany(long companyId)
        {
            return this.signatureRepository.GetByCompany(companyId);
        }

        /// <summary>
        /// Lấy danh sách chữ ký của các branchs
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public List<SIGNATURE> GetByCompanies(List<long> companyIds)
        {
            var obj = this.signatureRepository.GetByCompanies(companyIds);
            return obj;
        }
        public bool GetBySlot(int slot, string cert)
        {
            var result = this.signatureRepository.GetBySlot(slot, cert);
            if (result != null)
                return false;
            return true;

        }
        public bool GetByCompanyId(long companyId)
        {
            return this.signatureRepository.Contains(s => s.COMPANYID == companyId && !(s.DELETED ?? false));
        }
        public ResultCode Delete(long id)
        {
            SIGNATURE signature = this.signatureRepository.GetSignatureInfo(id);
            signature.DELETED = true;
            return this.signatureRepository.Update(signature) ? ResultCode.NoError : ResultCode.UnknownError;
        }
        public SignatureInfo SaveFileCA(SignatureInfo fileInfo)
        {
            string folder = Path.Combine(HttpContext.Current.Server.MapPath("\\Data\\Asset\\FileCA\\" + fileInfo.CompanyId + "_CA.pfx"));
            if (File.Exists(folder))
            {
                File.Delete(folder);
            }
            return addCertificate(folder, fileInfo);
        }
        public ExportFileInfo DownloadDataSignature(ConditionSearchCA condition)
        {
            var dataRport = new SignatureExport(condition.CurrentUser?.Company);
            var signatures = this.signatureRepository.FilterCA(condition);
            dataRport.Items = signatures.Select(p => new SignatureInfo(p)).ToList();
            ExportSignature export = new ExportSignature(dataRport, config, emailConfig);
            return export.ExportFile();
        }
        public void ProcessCertInfo()
        {
            try
            {
                var listSign = new List<SIGNATURE>();
                var lstbranch = this.signatureRepository.GetSignatureActive().ToList();

                foreach (var cert in lstbranch)
                {
                    var certInfo = GetCertInfo(cert.SLOTS ?? 0, cert.PASSWORD, cert.SERIALNUMBER);
                    var dateEffectiveString = certInfo.EffectiveDate.Split('/').ToList();
                    int yearEffective = int.Parse(dateEffectiveString[2].Substring(0, 4));
                    int monthEffective = int.Parse(dateEffectiveString[0]);
                    int dayEffective = int.Parse(dateEffectiveString[1]);
                    var effectiveDate = new DateTime(yearEffective, monthEffective, dayEffective);
                    cert.FROMDATE = effectiveDate;
                    var dateExpirationString = certInfo.ExpirationDate.Split('/').ToList();
                    int yearExpiration = int.Parse(dateExpirationString[2].Substring(0, 4));
                    int monthExpiration = int.Parse(dateExpirationString[0]);
                    int dayExpiration = int.Parse(dateExpirationString[1]);
                    var expirationDate = new DateTime(yearExpiration, monthExpiration, dayExpiration);
                    cert.TODATE = expirationDate;
                    cert.RELEASENAME = certInfo.ReleaseName;
                    cert.CERTSERIALNUMBER = certInfo.SerialNumber;
                    listSign.Add(cert);
                }
                this.signatureRepository.Update(listSign[0]);
            }
            catch (Exception ex)
            {
                logger.Error("GetCertInfo", ex);
            }
            
        }
        private CertInfo GetCertInfo(int slotId, string password, string serialNumber)
        {
            string response = String.Empty;
            string UrlGetSlots = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionGetCertInfo);//ApiSignInvoice.ActionGetCertInfo
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = password,
                SerialNumber = serialNumber,
                Slot = slotId
            });
            var result = new CertInfo();
            string DATA = json.ToString();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlGetSlots);
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
                request.ContentLength = byteArray.Length;
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
                    response = responseReader.ReadToEnd();
                }
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<CertInfo>(response);
            }
            catch (Exception ex)
            {
                logger.Error("GetCertInfo", ex);
                //throw new BusinessLogicException(ResultCode.NotSlotHSM, "Không thể load cổng Server HSM", ex);
            }

            return result;
        }
        public string GetSerialNumberByPassword(SignatureInfo fileInfo)
        {
            var listSignature = this.signatureRepository.GetListByPassword(fileInfo.Password);
            List<string> listSerial = new List<string>();
            foreach (var item in listSignature)
            {
                listSerial.Add(item.SERIALNUMBER);
            }
            UsbToken.Password = fileInfo.Password;
            UsbToken.lst = listSerial;
            return UsbToken.Instance.DectectUSBToken();
        }
        public string GetSerialBySlot(long slotId, string password)
        {
            string response = String.Empty;
            string UrlGetSlots = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionGetSerialBySlot);
            string json = new JavaScriptSerializer().Serialize(new
            {
                Slot = slotId,
                Password = password,
            });
            string DATA = json.ToString();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlGetSlots);
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
                request.ContentLength = byteArray.Length;
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
                    response = responseReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.NotSlotHSM, "Không thể load cổng Server HSM", ex);
            }

            return response;
        }

        public string GetSlots(string password)
        {
            string response = String.Empty;
            string UrlGetSlots = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionGetSlot);
            string json = new JavaScriptSerializer().Serialize(new
            {
                Password = password,
            });
            string DATA = json.ToString();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlGetSlots);
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(DATA);
                request.ContentLength = byteArray.Length;
                request.Timeout = 300000;
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
                    response = responseReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.NotSlotHSM, ex.Message);
            }

            return response;
        }
        public bool GetTypeSign()
        {
            return bool.Parse(WebConfigurationManager.AppSettings["SignHSM"]);
        }

        public SignatureInfo addCertificate(string location, SignatureInfo signatureInfo)
        {
            try
            {
                X509Store store = new X509Store("ShinhanBank", StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2 myCertificate = new X509Certificate2(location, signatureInfo.Password);
                store.Add(myCertificate);
                signatureInfo.FromDate = DateTime.Parse(myCertificate.GetEffectiveDateString());
                signatureInfo.ToDate = DateTime.Parse(myCertificate.GetExpirationDateString());
                signatureInfo.SerialNumber = myCertificate.SerialNumber;
            }
            catch (Exception ex)
            {
                string mess = ex.Message.ToString();
                throw new BusinessLogicException(ResultCode.PasswordCaInvalid, mess);
            }
            return signatureInfo;
        }
        public ResultCode Create(SignatureInfo signatureInfo)
        {
            SIGNATURE signature = new SIGNATURE()
            {
                COMPANYID = signatureInfo.CompanyId,
                SERIALNUMBER = signatureInfo.SerialNumber,
                COMPANYNAME = signatureInfo.CompanyName,
                FROMDATE = signatureInfo.FromDate ?? DateTime.Now,
                TODATE = signatureInfo.ToDate ?? DateTime.Now,
                PASSWORD = signatureInfo.Password,
                TYPESIGN = signatureInfo.TypeSign,
                SLOTS = signatureInfo.Slot
            };
            bool result = this.signatureRepository.Insert(signature);
            return result ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode Update(SignatureInfo signatureInfo)
        {
            SIGNATURE signature = GetSignature(signatureInfo);
            if (signature.COMPANYID == signatureInfo.CompanyId
                && signature.COMPANYNAME == signatureInfo.CompanyName
                && signature.SERIALNUMBER == signatureInfo.SerialNumber
                && signature.FROMDATE == signatureInfo.FromDate
                && signature.TODATE == signatureInfo.ToDate
                && signature.PASSWORD == signatureInfo.Password
                && signature.TYPESIGN == signatureInfo.TypeSign
                && signature.SLOTS == signatureInfo.Slot)
            {
                return ResultCode.NoError;
            }
            signature.SERIALNUMBER = signatureInfo.SerialNumber;
            signature.COMPANYNAME = signatureInfo.CompanyName;
            signature.FROMDATE = signatureInfo.FromDate ?? DateTime.Now;
            signature.TODATE = signatureInfo.ToDate ?? DateTime.Now;
            signature.PASSWORD = signatureInfo.Password;
            signature.TYPESIGN = signatureInfo.TypeSign;
            signature.SLOTS = signatureInfo.Slot;
            signature.DELETED = false;
            bool result = this.signatureRepository.Update(signature);
            return result ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public IEnumerable<SignatureInfo> GetList(long companyId)
        {
            var signatures = this.signatureRepository.GetList(companyId);
            return signatures.Select(p => new SignatureInfo(p));
        }
        public string GetSerialnumber(long compnyID)
        {
            try
            {
                var signatures = this.signatureRepository.GetList(compnyID);
                return signatures.FirstOrDefault(p => p.COMPANYID == compnyID).SERIALNUMBER;
            }
            catch
            {
                return null;
            }
        }

        public long CountFillterCA(ConditionSearchCA condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.signatureRepository.FilterCA(condition).Count();
        }

        #endregion
    }
}