using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.BL
{
    public class ConfigEmailServerBO : IConfigEmailServerBO
    {
        #region Fields, Properties
        private readonly IConfigEmailServerRepository emailServerRepository;

        #endregion

        #region Contructor

        public ConfigEmailServerBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.emailServerRepository = repoFactory.GetRepository<IConfigEmailServerRepository>();
        }

        #endregion

        #region Methods

        public EmailServerInfo GetByCompanyId(long companyId)
        {
            return GetEmailServer(companyId);
        }

        public ResultCode Create(EmailServerInfo emailActiveInfo)
        {
            if (emailActiveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!emailActiveInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var currentEmailServer = GetCurrentEmailServer(emailActiveInfo.CompanyId);
            ResultCode result;
            if (currentEmailServer == null)
            {
                result = CreateEmailServer(emailActiveInfo);
            }
            else
            {
                result = UpdateEmailServer(emailActiveInfo);
            }

            return result;
        }

        #endregion

        #region Private Method
        private EmailServerInfo GetEmailServer(long companyId)
        {
            var company = GetCurrentEmailServer(companyId);
            if (company == null)
            {
                return new EmailServerInfo();

            }

            return new EmailServerInfo(company);
        }

        private CONFIGEMAILSERVER GetCurrentEmailServer(long companyId)
        {
            var emailServer = this.emailServerRepository.GetByCompany(companyId);
            return emailServer;
        }

        private ResultCode CreateEmailServer(EmailServerInfo emailActiveInfo)
        {
            var configEmailServer = new CONFIGEMAILSERVER();
            configEmailServer.CopyData(emailActiveInfo);
            return this.emailServerRepository.Insert(configEmailServer) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        private ResultCode UpdateEmailServer(EmailServerInfo emailActiveInfo)
        {
            var configEmailServer = GetCurrentEmailServer(emailActiveInfo.CompanyId);
            configEmailServer.CopyData(emailActiveInfo);
            this.emailServerRepository.Update(configEmailServer);
            return ResultCode.NoError;

        }
        #endregion
    }
}