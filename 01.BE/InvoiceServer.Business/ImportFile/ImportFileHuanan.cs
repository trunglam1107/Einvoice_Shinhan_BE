using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.Business.DAO
{
    partial class ImportFileDAO
    {
        readonly List<ClientAddInfo> listSendMailHuanan = new List<ClientAddInfo>();
        public void SendMailHuanan()
        {
            var listClientCreateAccountAndSendMail = GetListNewUserCreateAccount();
            Account account = new Account(this.repoFactory);
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(GetConfig("EmailTemplateFilePath")),
            };
            ClientBO clientBO = new ClientBO(this.repoFactory, emailConfig);

            try
            {
                transaction.BeginTransaction();
                account.CreateAccountListClient(listClientCreateAccountAndSendMail);
                AddUserSendMail(listClientCreateAccountAndSendMail, clientBO, this.listSendMailHuanan);
                UpdateCustomerTypeHuanan(listClientCreateAccountAndSendMail);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.Error("SendMailHuanan: import done, create users, emails error ", ex);
                throw new BusinessLogicException(ResultCode.ImportInvoiceFileSuccess, "Import file error", ex);
            }

            this.SendMailFromList(this.listSendMailHuanan);
            this.UpdateEmailLoginOfCustomerHuanan();
        }

        private List<CLIENT> GetListNewUserCreateAccount()
        {
            var query =
                from client in this.dbContext.CLIENTs
                join login in this.dbContext.LOGINUSERs on client.ID equals login.CLIENTID into loginUserTemp
                from loginUser in loginUserTemp.DefaultIfEmpty()
                where loginUser == null
                    && (client.EMAIL != null && client.EMAIL.Trim() != "")
                //&& (client.USEREGISTEREMAIL == true ? (client.EMAIL != null && client.EMAIL.Trim() != "") : (client.RECEIVEDINVOICEEMAIL != null && client.RECEIVEDINVOICEEMAIL.Trim() != ""))
                select client;
            return query.ToList();
        }

        private void UpdateCustomerTypeHuanan(List<CLIENT> clients)
        {
            foreach (var client in clients)
            {
                if (client.CUSTOMERTYPE != (int)CustomerType.IsAccounting)
                {
                    client.CUSTOMERTYPE = (int)CustomerType.IsAccounting;
                }
            }
            this.dbContext.BulkSaveChanges(bulk =>
            {
                bulk.BatchSize = 1000;
                bulk.BatchTimeout = 3600;   // 1 hour timeout
            });
        }

        /// <summary>
        /// Kiểm tra toàn bộ client, nếu email trong client khác email trong loginUser thì update
        /// </summary>
        private void UpdateEmailLoginOfCustomerHuanan()
        {
            var updateEmailInfoQuery = from client in this.dbContext.CLIENTs
                                       join loginuser in this.dbContext.LOGINUSERs on client.ID equals loginuser.CLIENTID
                                       where !(client.DELETED ?? false)
                                           && !(loginuser.DELETED ?? false)
                                           && (client.EMAIL ?? "").ToUpper() != (loginuser.EMAIL ?? "").ToUpper()
                                       select new
                                       {
                                           UserId = loginuser.USERSID,
                                           UserName = loginuser.USERNAME,
                                           ClientId = client.ID,
                                           ClientEmail = client.EMAIL,
                                       };

            var updateEmailInfos = updateEmailInfoQuery.ToList();
            var idsLoginUser = updateEmailInfos.Select(x => x.UserId);
            var loginUsers = this.dbContext.LOGINUSERs.Where(e => idsLoginUser.Contains(e.USERSID)).ToList();
            foreach (var updateEmailInfo in updateEmailInfos)
            {
                var loginUser = loginUsers.Where(e => e.USERSID == updateEmailInfo.UserId).FirstOrDefault();
                loginUser.EMAIL = updateEmailInfo.ClientEmail;
            }
            this.dbContext.BulkSaveChanges(bulk =>
            {
                bulk.BatchSize = 1000;
                bulk.BatchTimeout = 3600;   // 1 hour timeout
            });
        }
    }
}