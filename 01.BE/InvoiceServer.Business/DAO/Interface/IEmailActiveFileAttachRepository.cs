using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IEmailActiveFileAttachRepository : IRepository<EMAILACTIVEFILEATTACH>
    {
        /// <summary>
        /// Get an IEnumerable EmailActive 
        /// </summary>
        /// <returns><c>IEnumerable EmailActive</c> if City not Empty, <c>null</c> otherwise</returns>
        IEnumerable<EMAILACTIVEFILEATTACH> Filter(int emailActiveId);
    }
}
