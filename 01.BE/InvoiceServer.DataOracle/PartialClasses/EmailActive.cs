using System.Collections.Generic;

namespace InvoiceServer.Data.DBAccessor
{
    public partial class EMAILACTIVE
    {
        /// <summary>
        /// Use only for insert, update
        /// </summary>
        IList<long> refIds;
        public IList<long> RefIds
        {
            get { return refIds ?? (refIds = new List<long>()); }
            set { refIds = value; }
        }
    }
}
