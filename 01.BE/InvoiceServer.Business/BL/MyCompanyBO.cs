using InvoiceServer.Business.Cache;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class MyCompanyBO : IMyCompanyBO
    {
        #region Fields, Properties
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IUserLevelRepository levelRepository;
        private readonly IDbTransactionManager transaction;
        private readonly UpdateloadImageConfig uploadConfig;
        private readonly Permission permission;
        private readonly ISignatureRepository signature;
        #endregion

        #region Contructor

        public MyCompanyBO(IRepositoryFactory repoFactory, UpdateloadImageConfig uploadImageConfig)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.signature = repoFactory.GetRepository<ISignatureRepository>();
            this.levelRepository = repoFactory.GetRepository<IUserLevelRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.uploadConfig = uploadImageConfig;
            this.permission = new Permission(repoFactory);
        }

        #endregion

        #region Methods

        public CompanyInfo GetCompanyInfo(long companyId, string userLevel)
        {
            var companyInfo = GetCompany(companyId);
            var adminOfCompany = GetAdminOfCompany(companyInfo.COMPANYSID, userLevel);
            var myCompanyInfo = new CompanyInfo(companyInfo, adminOfCompany);
            return myCompanyInfo;
        }

        public CompanyInfo GetCompanyInfo(long id)
        {
            MYCOMPANY mycompany = this.myCompanyRepository.GetById(id);
            if (mycompany == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, "Get company fail");
            }

            return new CompanyInfo(mycompany);
        }
        public MYCOMPANY GetCompanyById(long id)
        {
            return this.myCompanyRepository.GetById(id);
        }
        public IEnumerable<MyCompanyInfo> GetList()
        {
            var companyList = this.myCompanyRepository.GetList();
            return companyList.Select(p => new MyCompanyInfo(p));
        }
        public SmallSignatureInfor getByID(long id)
        {
            var companyList = this.myCompanyRepository.GetList();
            var sig = this.signature.GetList(id);
            SmallSignatureInfor small = new SmallSignatureInfor()
            {
                CompanyId = id,
                CompanyName = companyList.Where(p => p.COMPANYSID == id).Select(p => p.COMPANYNAME).FirstOrDefault(),
                SerialNumber = sig.Where(p => (p.DELETED != true)).Select(p => p.SERIALNUMBER).FirstOrDefault()
            };
            return small;
        }

        #endregion

        public IEnumerable<MasterCompanyInfo> FilterCompany(ConditionSearchCompany condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var companies = this.myCompanyRepository.FilterCompany(condition).AsQueryable()
                .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            return companies.Select(p => new MasterCompanyInfo(p));
        }

        public long CountFillterCompany(ConditionSearchCompany condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.myCompanyRepository.FilterCompany(condition).Count();
        }

        public ResultCode Create(CompanyInfo companyInfo, UserSessionInfo currentUser)
        {
            try
            {
                transaction.BeginTransaction();
                MYCOMPANY currentCompany = CreateCompany(companyInfo);
                CreateUserOfCompany(companyInfo.Account, currentCompany, currentUser);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        private void CreateUserOfCompany(CompanyAccount accountInfo, MYCOMPANY currentCompany, UserSessionInfo currentUser)
        {
            var loginUser = CreateUser(accountInfo, currentCompany);
            this.permission.CreatePermission(loginUser, currentUser);
        }

        public ResultCode Update(long companyId, CompanyInfo companyInfo)
        {
            try
            {
                transaction.BeginTransaction();
                MYCOMPANY currentCompany = CompanyUpdate(companyId, companyInfo);
                UpdateUser(companyInfo.Account, currentCompany, companyInfo.Id);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        public ResultCode Delete(long id, string userLevel)
        {
            try
            {
                transaction.BeginTransaction();
                DeleteCompany(id);
                DeleteAllUserOfCompany(id);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        private void DeleteCompany(long id)
        {
            MYCOMPANY currentCompany = GetCompany(id);

            currentCompany.DELETED = true;
            currentCompany.ACTIVE = false;
            myCompanyRepository.Update(currentCompany);
        }

        public MyCompanyInfo GetCompanyOfUser(long id, long? companyId)
        {
            var myCompany = GetCompany(id, companyId);
            MyCompanyInfo mycompanyInfo = new MyCompanyInfo(myCompany);
            mycompanyInfo.Logo = GetBase64StringImage(companyId.Value, mycompanyInfo.Logo);
            mycompanyInfo.SignaturePicture = myCompany.SIGNATUREFILENAME;
            return mycompanyInfo;
        }

        public ResultCode UpdateMyCompany(long id, long? companyId, MyCompanyInfo companyInfo, string userLevel)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!companyInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }


            MYCOMPANY currentCompny = GetCompany(id, companyId);

            if (!currentCompny.TAXCODE.IsEquals(companyInfo.TaxCode)
                 && this.myCompanyRepository.ContainTaxCode(companyInfo.TaxCode))
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                    string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", companyInfo.TaxCode));
            }

            if (companyInfo.Email.IsNotNullOrEmpty() && !currentCompny.EMAIL.IsEquals(companyInfo.Email)        // Email is changed
               && this.myCompanyRepository.ContainEmail(companyInfo.Email)) // New Email is existed
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsEmail,
                                  string.Format("Create Company failed because company with Email = [{0}] is exist.", companyInfo.Email));
            }
            currentCompny.LOGOFILENAME = SaveImage(companyInfo, FileTypeUpload.Logo);
            currentCompny.SIGNATUREFILENAME = SaveImage(companyInfo, FileTypeUpload.SignaturePicture);
            currentCompny.CopyData(companyInfo);
            this.myCompanyRepository.Update(currentCompny);
            UserSessionCache.Instance.UpdateCompanyInfo(id, new CompanyInfo(currentCompny));
            return ResultCode.NoError;
        }

        #region Private Method

        private MYCOMPANY GetCompany(long id)
        {
            var company = this.myCompanyRepository.GetById(id);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }

            return company;
        }

        private MYCOMPANY GetCompany(long id, long? companyId)
        {
            if (!companyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            MYCOMPANY company = this.myCompanyRepository.GetById(id);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.ResouceIdNotFound);
            }

            if (company.COMPANYSID != companyId.Value)
            {
                throw new BusinessLogicException(ResultCode.NotPermisionData, ClientManagementInfo.NotPermissionData);
            }

            return company;
        }

        /// <summary>
        /// Get the dealer admin of dealer company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private LOGINUSER GetAdminOfCompany(long companyId, string level)
        {

            var adminOfCompany = this.loginUserRepository.GetUsersByRoleName(level).FirstOrDefault(p => p.COMPANYID == companyId);
            if (adminOfCompany == null)
            {
                return new LOGINUSER();
            }
            return adminOfCompany;
        }

        private MYCOMPANY CreateCompany(CompanyInfo companyInfo)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!companyInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            bool isExitTaxCode = this.myCompanyRepository.ContainTaxCode(companyInfo.TaxCode);
            if (isExitTaxCode)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsTaxCode,
                                  string.Format("Create Company failed because company with TaxCode = [{0}] is exist.", companyInfo.TaxCode));
            }

            bool isExitEmail = this.myCompanyRepository.ContainEmail(companyInfo.Email);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.CompanyNameIsExitsEmail,
                                  string.Format("Create Company failed because company with Email = [{0}] is exist.", companyInfo.Email));
            }

            var myCompany = new MYCOMPANY();
            myCompany.CopyData(companyInfo);
            myCompany.LEVELCUSTOMER = CustomerLevel.Sellers;
            myCompany.LEVELAGENCIES = 1;
            myCompany.ACTIVE = true;
            this.myCompanyRepository.Insert(myCompany);
            return myCompany;
        }

        private LOGINUSER CreateUser(CompanyAccount accoutInfo, MYCOMPANY myCompany)
        {
            if (accoutInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!accoutInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            bool isExitUserId = this.loginUserRepository.ContainUserId(accoutInfo.UserId);
            if (isExitUserId)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", accoutInfo.UserId));
            }

            bool isExitEmail = this.loginUserRepository.ContainEmail(accoutInfo.Email);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                  string.Format("Create User failed because user with Email = [{0}] is exist.", accoutInfo.Email));
            }

            USERLEVEL userLevel = this.levelRepository.FillterByLevel(RoleInfo.BRANCH);
            LOGINUSER loginUser = new LOGINUSER();
            loginUser.USERID = accoutInfo.UserId;
            loginUser.PASSWORD = accoutInfo.Password;
            loginUser.USERLEVELSID = userLevel.USERLEVELSID;
            loginUser.COMPANYID = myCompany.COMPANYSID;
            loginUser.ISACTIVE = accoutInfo.IsActive;
            loginUser.EMAIL = accoutInfo.Email;
            loginUserRepository.Insert(loginUser);
            loginUser.USERLEVEL = userLevel;
            return loginUser;
        }

        private void UpdateUser(CompanyAccount accountInfo, MYCOMPANY currentCompany, long? companyId)
        {
            if (accountInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (accountInfo.id == 0)
            {
                CreateUser(accountInfo, currentCompany);
                return;
            }

            this.loginUserRepository.GetByEmail(accountInfo.Email);
            ResultCode errorCode;
            string errorMessage;
            if (!accountInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            LOGINUSER currentUser = this.loginUserRepository.GetByUserId(accountInfo.UserId, companyId.Value);
            if (!currentUser.EMAIL.IsEquals(accountInfo.Email)        // Email is changed
               && this.loginUserRepository.ContainEmail(accountInfo.Email)) // New Email is existed
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                string.Format("Update UserLogin with Email [{0}] exist", accountInfo.Email));
            }

            if (accountInfo.Password.IsNotNullOrEmpty())
            {
                currentUser.PASSWORD = accountInfo.Password;
            }

            currentUser.EMAIL = accountInfo.Email;
            currentUser.ISACTIVE = accountInfo.IsActive;
            currentUser.UPDATEDDATE = DateTime.Now;
            loginUserRepository.Update(currentUser);

        }

        private MYCOMPANY CompanyUpdate(long id, CompanyInfo companyInfo)
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!companyInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            MYCOMPANY currentCompny = GetCompany(id);
            currentCompny.CopyData(companyInfo);
            this.myCompanyRepository.Update(currentCompny);
            UserSessionCache.Instance.UpdateCompanyInfo(id, new CompanyInfo(currentCompny));
            return currentCompny;
        }

        private void DeleteAllUserOfCompany(long companyId)
        {
            var userOfCompany = this.loginUserRepository.GetByIdCompany(companyId).ToList();
            userOfCompany.ForEach(p =>
            {
                p.DELETED = true;
                this.loginUserRepository.Update(p);
            });
        }

        private string SaveImage(MyCompanyInfo companyInfo, FileTypeUpload fileType)
        {
            string imageData = GetImageData(companyInfo, fileType);
            if (imageData.IsNullOrEmpty()) return string.Empty;

            // Init file name, folder upload
            var imageName = fileType + ".png";
            var folder = GetFolderUpload(companyInfo.Id);
            var filePath = string.Format("{0}\\{1}", folder, imageName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Validate and save image
            var bytes = Convert.FromBase64String(imageData);
            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                var sizeOfImage = ms.Length / 1024;
                image = Image.FromStream(ms);

                if (sizeOfImage > uploadConfig.MaxSizeImage)
                {
                    throw new BusinessLogicException(ResultCode.FileLarge, string.Format("Image {0} size too large", fileType.ToString()));
                }

                image.Save(filePath, ImageFormat.Png);
                image.Dispose();
            }

            return imageName;
        }

        private string GetImageData(MyCompanyInfo companyInfo, FileTypeUpload fileType)
        {
            string imageData;
            switch (fileType)
            {
                case FileTypeUpload.Logo:
                    imageData = companyInfo.Logo;
                    break;
                case FileTypeUpload.SignaturePicture:
                    imageData = companyInfo.SignaturePicture;
                    break;
                default:
                    imageData = companyInfo.Logo;
                    break;
            }
            return imageData;
        }

        private string GetBase64StringImage(long companyId, string fileName)
        {
            var folder = GetFolderUpload(companyId);
            var filePath = string.Format("{0}\\{1}", folder, fileName);

            if (!File.Exists(filePath)) return string.Empty;

            var bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        }

        private string GetFolderUpload(long companyId)
        {
            return string.Format("{0}\\{1}", uploadConfig.RootFolderUpload, companyId);
        }
        #endregion

        public MYCOMPANY GetCompanyByName(string name)
        {
            MYCOMPANY mycompany = this.myCompanyRepository.GetByName(name);
            if (mycompany == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, "Get company fail");
            }

            return mycompany;
        }

        public IEnumerable<CompanyInfo> GetCompanyChill(long companyIDRoot)
        {
            var result = myCompanyRepository.GetCompanyChill(companyIDRoot);
            return result.Select(item => new CompanyInfo(item));
        }

        public decimal MyCompanyUsing(long companyID)
        {
            return myCompanyRepository.MyCompanyUsing(companyID);
        }
    }
}