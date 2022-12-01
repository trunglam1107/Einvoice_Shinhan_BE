using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IRegisterTemplatesBO
    {
        IEnumerable<RegisterTemplateMaster> GetList(long CompanyId);

        IEnumerable<TemplateAccepted> GetInvoiceApproved(long CompanyId);

        IEnumerable<RegisterTemplateMaster> Filter(ConditionSearchRegisterTemplate condition);

        RegisterTemplateInfo GetById(long id);

        long Count(ConditionSearchRegisterTemplate condition);

        long Create(RegisterTemplateInfo info);

        ResultCode Update(long id, RegisterTemplateInfo info);

        ResultCode Delete(long id);

        ExportFileInfo PrintView(long companyId, string parttern, string symbol, string fromNumber);

        IEnumerable<RegisterTemplateDenominator> GetListDenominator(long companyId, long? branch);
        IEnumerable<RegisterTemplateSample> GetRegisterTemplateSampleOfCompany(long companyID);
    }
}


