using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IRegisterTemplatesRepository : IRepository<REGISTERTEMPLATE>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<REGISTERTEMPLATE> GetList();
        IEnumerable<RegisterTemplateMaster> Filter(ConditionSearchRegisterTemplate condition, int skip = 0, int take = int.MaxValue);

        REGISTERTEMPLATE GetById(long Id);

        IEnumerable<REGISTERTEMPLATE> GetRegisterTemplateCompanyUse(long companyId);

        IEnumerable<TemplateAccepted> GetInvoiceApproved(long companyId);

        IEnumerable<RegisterTemplateDenominator> GetDenominator(long companyId, long? branch);

        IEnumerable<REGISTERTEMPLATE> GetList(long companyId);

        REGISTERTEMPLATE GetByParttern(string parttern, long companyId);

        bool ContainCode(string code, long companyId);
        REGISTERTEMPLATE GetByCompanyId(long companyId, string code);
        IEnumerable<RegisterTemplateSample> GetRegisterTemplateSamplesByCompanyID(long companyId);
        IEnumerable<RegisterTemplateMaster> GetDenominatorTT78(long companyId, long? branch);

    }
}
