using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public class Email
    {
        private readonly IEmailActiveRepository emailRepository;
        private readonly IEmailActiveReferenceRepository emailReferenceRepository;
        private readonly Logger logger = new Logger();

        public Email(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.emailRepository = repoFactory.GetRepository<IEmailActiveRepository>();
            this.emailReferenceRepository = repoFactory.GetRepository<IEmailActiveReferenceRepository>();
        }

        public long CreateEmailActive(EmailInfo emailInfo, int typeEmail = 1, long? companyId = 0)
        {
            try
            {
                
                var emailActive = new EMAILACTIVE();
                emailInfo.CompanyId = companyId;
                emailActive.CopyData(emailInfo);
                emailActive.SENDSTATUS = (int)StatusSendEmail.New;
                emailActive.TYPEEMAIL = typeEmail;
                emailActive.CREATEDDATE = DateTime.Now;
                this.emailRepository.Insert(emailActive);
                this.CreateEmailActiveReference(emailActive);
                return emailActive.ID;
            }
            catch(Exception ex)
            {
                logger.Error("Create email active failed , error : " + ex.ToString(), new Exception(ex.ToString()));
                return 0;
            }
            
        }

        public bool UpdateEmailActive(EmailInfo emailInfo, long emailActiveId)
        {
            var currentEmail = this.emailRepository.GetById(emailActiveId);
            currentEmail.EMAILTO = emailInfo.EmailTo;
            currentEmail.CONTENTEMAIL = emailInfo.Content;
            return this.emailRepository.Update(currentEmail);
        }

        public Dictionary<string, EMAILACTIVE> CreateEmailActive(List<EmailInfo> emailInfos, int typeEmail = 1)
        {
            Dictionary<string, EMAILACTIVE> res = new Dictionary<string, EMAILACTIVE>();
            List<EMAILACTIVE> emailActives = new List<EMAILACTIVE>();
            foreach (var emailInfo in emailInfos)
            {
                var emailActive = new EMAILACTIVE();
                emailActive.CopyData(emailInfo);
                emailActive.SENDSTATUS = (int)StatusSendEmail.New;
                emailActive.TYPEEMAIL = typeEmail;
                emailActive.CREATEDDATE = DateTime.Now;
                emailActives.Add(emailActive);
                res.Add(emailInfo.Content, emailActive);
            }
            this.emailRepository.AddRange(emailActives);
            this.CreateEmailActiveReference(emailActives);
            return res;
        }

        public void UpdateStatusEmail(long id)
        {
            var currentEmail = this.emailRepository.GetById(id);
            currentEmail.SENDSTATUS = (int)StatusSendEmail.Successfull;
            this.emailRepository.Update(currentEmail);
        }

        #region private methos
        private void CreateEmailActiveReference(EMAILACTIVE emailActive)
        {
            var emailActiveReferences = new List<EMAILACTIVEREFERENCE>();
            foreach (var refId in emailActive.RefIds)
            {
                var emailActiveReference = new EMAILACTIVEREFERENCE()
                {
                    EMAILACTIVEID = emailActive.ID,
                    REFID = refId,
                };
                emailActiveReferences.Add(emailActiveReference);
            }
            this.emailReferenceRepository.AddRange(emailActiveReferences);
        }

        private void CreateEmailActiveReference(List<EMAILACTIVE> emailActives)
        {
            var emailActiveReferences = new List<EMAILACTIVEREFERENCE>();
            foreach (var emailActive in emailActives)
            {
                foreach (var refId in emailActive.RefIds)
                {
                    var emailActiveReference = new EMAILACTIVEREFERENCE()
                    {
                        EMAILACTIVEID = emailActive.ID,
                        REFID = refId,
                    };
                    emailActiveReferences.Add(emailActiveReference);
                }
            }
            this.emailReferenceRepository.AddRange(emailActiveReferences);
        }
        #endregion private methos
    }
}