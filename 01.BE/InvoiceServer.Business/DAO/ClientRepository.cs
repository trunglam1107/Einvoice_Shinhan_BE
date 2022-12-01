using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ClientRepository : GenericRepository<CLIENT>, IClientRepository
    {
        private static readonly object lockSeedCurrentClient = new object();
        public ClientRepository(IDbContext context)
            : base(context)
        {
            this.SeedClient();
        }

        private void SeedClient()   // Seed khách hàng vãng lai cho BIDC
        {
            if (!OtherExtensions.IsCurrentClient())
                return;
            // Thêm khách hàng vãng lai cho BIDC
            lock (lockSeedCurrentClient)
            {
                var isCurrentClientExists = dbSet.Count(e => e.CUSTOMERCODE == BIDCDefaultFields.CURRENT_CLIENT);
                if (isCurrentClientExists == 0)
                {
                    var client = new CLIENT() { CUSTOMERCODE = BIDCDefaultFields.CURRENT_CLIENT, CUSTOMERNAME = BIDCDefaultFields.CURRENT_CLIENT, ISORG = false, };
                    this.Insert(client);
                }
            }
        }

        public override bool Insert(CLIENT entity)
        {
            if (OtherExtensions.IsCurrentClient(entity.CUSTOMERCODE))   // Nếu là khách hàng vãng lai thì tạo mới không cần thông tin 
            {
                CLIENT newClient = new CLIENT() { CUSTOMERCODE = BIDCDefaultFields.CURRENT_CLIENT, CUSTOMERNAME = BIDCDefaultFields.CURRENT_CLIENT, ISORG = false, };
                var propInfo = entity.GetType().GetProperties();
                foreach (var item in propInfo)
                {
                    if (item.PropertyType.IsPrimitive())
                        entity.GetType().GetProperty(item.Name).SetValue(entity, item.GetValue(newClient, null), null);
                }
            }
            return base.Insert(entity);
        }

        public override bool Update(CLIENT entity)
        {
            if (OtherExtensions.IsCurrentClient(entity.CUSTOMERCODE))   // Nếu là khách hàng vãng lai thì update không cần thông tin
            {
                CLIENT newClient = new CLIENT() { ID = entity.ID, CUSTOMERCODE = BIDCDefaultFields.CURRENT_CLIENT, CUSTOMERNAME = BIDCDefaultFields.CURRENT_CLIENT, ISORG = false, };
                var propInfo = newClient.GetType().GetProperties();
                foreach (var item in propInfo)
                {
                    if (item.PropertyType.IsPrimitive())
                        entity.GetType().GetProperty(item.Name).SetValue(entity, item.GetValue(newClient, null), null);
                }
            }
            return base.Update(entity);
        }

        public IEnumerable<CLIENT> GetList()
        {
            return GetClientActive();
        }
        public IQueryable<CLIENT> GetListSendByMonth()
        {
            return GetClientActive().AsNoTracking().Where(p => p.SENDINVOICEBYMONTH == true && p.ISORG == true);

        }
        public CLIENT GetById(long id)
        {
            return GetClientActive().FirstOrDefault(p => p.ID == id);
        }

        /// <summary>
        /// Lấy danh sách clients
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<CLIENT> GetByIds(List<long?> ids) 
        {
            return GetClientActive().Where(p => ids.Contains(p.ID)).ToList();
        }
        public CLIENT GetByIdForSendMail(long id)
        {
            return GetClientActive().AsNoTracking().FirstOrDefault(p => p.ID == id);
        }

        public CLIENT GetByTaxCode(string taxCode)
        {
            return GetClientActive().FirstOrDefault(p => p.TAXCODE == taxCode);
        }

        public CLIENT GetByPersonalContactAndAddress(string personalContact, string address)
        {
            return GetClientActive().FirstOrDefault(p => (p.PERSONCONTACT ?? string.Empty).ToUpper() == personalContact.ToUpper()
                && p.ADDRESS.ToUpper() == address.ToUpper()
                );
        }

        public IQueryable<ClientDetail> Filter(ConditionSearchClient condition)
        {
            IQueryable<CLIENT> clients = FilterByCondition(condition);
            var clientInfos = SelectClientDetail(clients);
            return clientInfos;
        }

        private IQueryable<CLIENT> FilterByCondition(ConditionSearchClient condition)
        {
            var clients = GetClientActive();

            if (!condition.TaxCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.ToUpper().Contains(condition.TaxCode.ToUpper()));
            }

            if (!condition.ClientName.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.ClientName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.ClientName.ToUpper()));
            }

            if (!condition.CustomerCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (!condition.UseTotalInvoice.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.USETOTALINVOICE ?? false) == condition.UseTotalInvoice);
            }

            if (!condition.TaxIncentives.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.TAXINCENTIVES ?? false) == condition.TaxIncentives);
            }

            if (!condition.SendInvoiceByMonth.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.SENDINVOICEBYMONTH ?? false) == condition.SendInvoiceByMonth);
            }

            if (!condition.IsPersonal.IsNullOrEmpty())
            {
                if (condition.IsPersonal == "false")
                {
                    clients = clients.Where(p => p.ISORG == true);
                }
                else
                {
                    clients = clients.Where(p => p.ISORG == false);
                }
            }

            return clients;
        }

        public List<ClientDetail> FilterByCIF(ConditionSearchClient condition)
        {
            IQueryable<CLIENT> clients = FilterByCIFByCondition(condition);
            var res = new List<ClientDetail>();
            var countClientExactly = 0;

            if (!condition.CustomerCode.IsNullOrEmpty())
            {
                var clientExactlyQuery = clients.Where(p => p.CUSTOMERCODE.ToUpper() == condition.CustomerCode.ToUpper());
                var clientDetailExactlyQuery = SelectClientDetailByCIF(clientExactlyQuery);
                res.AddRange(clientDetailExactlyQuery.Skip(condition.Skip).Take(condition.Take).ToList());
                countClientExactly = res.Count;
            }

            var numberTake = condition.Take > countClientExactly ? condition.Take - countClientExactly : 0;
            for (int i = 0; i < countClientExactly; i++)
            {
                var cId = res[i].Id;
                clients = clients.Where(e => e.ID != cId);
            }
            if (numberTake > 0)
            {
                var clientInfosQuery = SelectClientDetailByCIF(clients);
                res.AddRange(clientInfosQuery.Skip(condition.Skip).Take(numberTake).ToList());
            }

            return res;
        }

        private IQueryable<CLIENT> FilterByCIFByCondition(ConditionSearchClient condition)
        {
            var clients = GetClientActive();

            if (!condition.CustomerCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (!condition.IsPersonal.IsNullOrEmpty())
            {
                if (condition.IsPersonal == "false")
                {
                    clients = clients.Where(p => p.ISORG == true);
                }
                else
                {
                    clients = clients.Where(p => p.ISORG == false);
                }
            }

            return clients;
        }

        private static IQueryable<ClientDetail> SelectClientDetailByCIF(IQueryable<CLIENT> clients)
        {
            return from client in clients
                   orderby client.ID descending
                   select new ClientDetail
                   {
                       Id = client.ID,
                       CustomerCode = client.CUSTOMERCODE,
                       TaxCode = client.TAXCODE,
                       CustomerName = (client.ISORG.Value) ? client.CUSTOMERNAME : client.PERSONCONTACT,
                       PersonContact = client.PERSONCONTACT,
                       CompanyName = (client.ISORG.Value) ? client.CUSTOMERNAME : client.PERSONCONTACT,
                       Address = client.ADDRESS,
                       Mobile = client.MOBILE,
                       Email = client.EMAIL,
                       CustomerId = client.CUSTOMERID,
                       //CompanyId = client.COMPANYID,
                       IsOrg = client.ISORG,
                       BankAccount = client.BANKACCOUNT,
                       BankName = client.BANKNAME,
                       ReceivedInvoiceEmail = client.RECEIVEDINVOICEEMAIL,
                       UseTotalInvoice = client.USETOTALINVOICE,
                       TaxIncentives = client.TAXINCENTIVES,
                       SendInvoiceByMonth = client.SENDINVOICEBYMONTH,
                       UseRegisterEmail = client.USEREGISTEREMAIL,
                   };
        }

        public long Count(ConditionSearchClient filterCondition)
        {
            var clients = GetClientActive();

            if (!filterCondition.TaxCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.ToUpper().Contains(filterCondition.TaxCode.ToUpper()));
            }

            if (!filterCondition.ClientName.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(filterCondition.ClientName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(filterCondition.ClientName.ToUpper()));
            }

            if (!filterCondition.CustomerCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(filterCondition.CustomerCode.ToUpper()));
            }

            if (!filterCondition.UseTotalInvoice.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.USETOTALINVOICE ?? false) == filterCondition.UseTotalInvoice);
            }

            if (!filterCondition.TaxIncentives.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.TAXINCENTIVES ?? false) == filterCondition.TaxIncentives);
            }

            if (!filterCondition.SendInvoiceByMonth.IsNullOrEmpty())
            {
                clients = clients.Where(p => (p.SENDINVOICEBYMONTH ?? false) == filterCondition.SendInvoiceByMonth);
            }

            return clients.Count();
        }

        public long CountByCIF(ConditionSearchClient condition)
        {
            var clients = FilterByCIFByCondition(condition);
            return clients.Count();
        }

        private IQueryable<CLIENT> GetClientActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public CLIENT FilterClient(long companyId, string taxCode, string email, string customerName)
        {
            return GetClientActive().FirstOrDefault(p => p.TAXCODE.Equals(taxCode) && p.EMAIL.Equals(email) && p.CUSTOMERNAME.Equals(customerName));
        }

        public CLIENT FilterClient(long companyId, string taxCode, string customerName)
        {
            return GetClientActive().FirstOrDefault(p => p.TAXCODE.Equals(taxCode) && p.CUSTOMERNAME.Equals(customerName));
        }
        public CLIENT FilterClient(string taxCode, string customerName)
        {
            return GetClientActive().FirstOrDefault(p => p.CUSTOMERCODE.Equals(taxCode));
        }
        public CLIENT FilterClient(long companyId, string taxCode, string customerName, string address, int? isOrg)
        {
            if (isOrg != 1)
                return GetClientActive().FirstOrDefault(p => p.PERSONCONTACT.Equals(customerName) && p.ADDRESS.Equals(address));
            return GetClientActive().FirstOrDefault(p => p.TAXCODE.Equals(taxCode));
        }
        public bool ContainEmail(string email, long companyId)
        {
            bool result = false;
            if (!email.IsNullOrEmpty())
            {
                result = Contains(p => ((p.EMAIL ?? string.Empty).ToUpper()).Equals(email.ToUpper()) && !(p.DELETED ?? false));
            }
            return result;
        }


        public bool ContainTax(string taxCode)
        {
            bool result = false;
            if (!taxCode.IsNullOrEmpty())
            {
                result = Contains(p => ((p.TAXCODE ?? string.Empty).ToUpper()).Equals(taxCode.ToUpper()) && !(p.DELETED ?? false));
            }

            return result;
        }

        public bool ContainCustomerCode(string customerCode)
        {
            bool result = false;
            if (!customerCode.IsNullOrEmpty())
            {
                result = Contains(p => ((p.CUSTOMERCODE ?? string.Empty).ToUpper()).Equals(customerCode.ToUpper()) && !(p.DELETED ?? false));
            }

            return result;
        }

        public bool MyClientUsing(long clientID)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_CLIENT_USING(clientID, outputparam);
            var result = outputparam.Value.ToDecimal();
            if (result == 1)
                return true;
            return false;
        }

        // Trong trường hợp customerCode được insert bằng màn hình Invoice hoặc import invoice
        // customerCode sẽ được gán bằng "_1" nếu đã tồn tại customerCode với giá trị đó
        public int GetMaxCustomerCode(long clientId)
        {
            var maxCustomerCodeSuffix = 0;
            // Nếu chưa có customerCode nào trùng
            if (!this.ContainCustomerCode(clientId.ToString()))
            {
                return maxCustomerCodeSuffix;
            }

            var customerCodePrefix = clientId.ToString() + "_";
            var listSameCustomerCode = GetClientActive().Where(p => p.CUSTOMERCODE.StartsWith(customerCodePrefix)).ToList();

            // Tìm ra customerCode có suffix "_1" "_2",... lớn nhất
            listSameCustomerCode.ForEach(c =>
            {
                var customerCodeSplit = c.CUSTOMERCODE.Split('_');
                if (customerCodeSplit.Count() == 2 && customerCodeSplit[1].IsNumeric())
                {
                    var suffix = customerCodeSplit[1].ToInt() ?? 0;
                    if (suffix > maxCustomerCodeSuffix)
                    {
                        maxCustomerCodeSuffix = suffix;
                    }

                }
            });

            return maxCustomerCodeSuffix + 1;
        }
        public CLIENT GetByCustomerCode(string customerCode)
        {
            return GetClientActive().FirstOrDefault(p => p.CUSTOMERCODE == customerCode);
        }

        public ReceiverAccountActiveInfo GetClientAccountInfo(long clientId)
        {
            //var clients = GetClientActive().Where(p => p.ID == clientId);

            var clientInfo = (from client in this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false))
                              join loginUser in this.context.Set<LOGINUSER>() on client.TAXCODE equals loginUser.USERID
                              where client.ID == clientId
                              select new ReceiverAccountActiveInfo
                              {
                                  Email = client.RECEIVEDINVOICEEMAIL,
                                  CompanyName = client.CUSTOMERNAME ?? client.PERSONCONTACT,
                                  CustomerCode = client.CUSTOMERCODE,
                                  CustomerName = loginUser.USERNAME,
                                  UserName = loginUser.USERNAME,
                                  UserId = loginUser.USERID,
                                  Password = loginUser.PASSWORD,
                                  IsOrg = client.ISORG ?? false
                              }).FirstOrDefault();
            return clientInfo;
        }

        public IEnumerable<CLIENT> FilterClients(ConditionSearchClient condition)
        {
            var clients = GetClientActive();

            if (!condition.TaxCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXCODE.ToUpper().Contains(condition.TaxCode.ToUpper()));
            }

            if (!condition.ClientName.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERNAME.ToUpper().Contains(condition.ClientName.ToUpper()) || p.PERSONCONTACT.ToUpper().Contains(condition.ClientName.ToUpper()));
            }

            if (!condition.CustomerCode.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }

            if (!condition.UseTotalInvoice.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.USETOTALINVOICE == condition.UseTotalInvoice);
            }

            if (!condition.TaxIncentives.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.TAXINCENTIVES == condition.TaxIncentives);
            }

            if (!condition.SendInvoiceByMonth.IsNullOrEmpty())
            {
                clients = clients.Where(p => p.SENDINVOICEBYMONTH == condition.SendInvoiceByMonth);
            }

            return clients;
        }

        public List<ClientDetail> ClientListSuggestion(ConditionSearchClient condition)
        {
            var res = new List<ClientDetail>();
            var clients = FilterByCondition(condition);
            var countClientExactly = 0;

            if (!condition.ClientName.IsNullOrEmpty())
            {
                var clientExactlyQuery = clients.Where(p => p.CUSTOMERNAME.ToUpper() == condition.ClientName.ToUpper() || p.PERSONCONTACT.ToUpper() == condition.ClientName.ToUpper());
                var clientDetailExactlyQuery = SelectClientDetail(clientExactlyQuery);
                res.AddRange(clientDetailExactlyQuery.Skip(condition.Skip).Take(condition.Take).ToList());
                countClientExactly = res.Count;
            }

            var numberTake = condition.Take > countClientExactly ? condition.Take - countClientExactly : 0;
            for (int i = 0; i < countClientExactly; i++)
            {
                var cId = res[i].Id;
                clients = clients.Where(e => e.ID != cId);
            }
            if (numberTake > 0)
            {
                var clientInfosQuery = SelectClientDetail(clients);
                res.AddRange(clientInfosQuery.Skip(condition.Skip).Take(numberTake).ToList());
            }

            return res;
        }
        public CLIENT GetByEmail(string email)
        {
            return GetClientActive().FirstOrDefault(p => p.EMAIL.Trim().ToUpper() == email.Trim().ToUpper());
        }
        private static IQueryable<ClientDetail> SelectClientDetail(IQueryable<CLIENT> clients)
        {
            return from client in clients
                   orderby client.ID descending
                   select new ClientDetail
                   {
                       Id = client.ID,
                       CustomerCode = client.CUSTOMERCODE,
                       TaxCode = client.TAXCODE,
                       CustomerName = client.CUSTOMERNAME,
                       PersonContact = client.PERSONCONTACT,
                       CompanyName = (client.ISORG.Value) ? client.CUSTOMERNAME : client.PERSONCONTACT,
                       Address = client.ADDRESS,
                       Mobile = client.MOBILE,
                       Email = client.EMAIL,
                       CustomerId = client.CUSTOMERID,
                       //CompanyId = client.COMPANYID,
                       IsOrg = client.ISORG,
                       BankAccount = client.BANKACCOUNT,
                       BankName = client.BANKNAME,
                       ReceivedInvoiceEmail = client.RECEIVEDINVOICEEMAIL,
                       UseTotalInvoice = client.USETOTALINVOICE,
                       TaxIncentives = client.TAXINCENTIVES,
                       SendInvoiceByMonth = client.SENDINVOICEBYMONTH,
                       UseRegisterEmail = client.USEREGISTEREMAIL,
                   };
        }

        public PTClientAccountInfo GetClient(long? clientId)
        {
            var Clients = from CLIENT in GetClientActive()
                          join MYCOMPANY in this.context.Set<MYCOMPANY>()
                          on CLIENT.TRX_BRANCH_NO equals MYCOMPANY.BRANCHID
                          where CLIENT.ID == clientId
                          select new PTClientAccountInfo
                          {
                              ClientID = CLIENT.ID,
                              Email = CLIENT.EMAIL,
                              UseRegisterEmail = CLIENT.USEREGISTEREMAIL,
                              ReceivedInvoiceEmail = CLIENT.RECEIVEDINVOICEEMAIL,
                              CompanyId = MYCOMPANY.COMPANYSID
                          };
            return Clients.FirstOrDefault();
        }
    }
}

