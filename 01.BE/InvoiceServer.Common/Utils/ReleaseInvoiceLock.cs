namespace InvoiceServer.Common
{
    public static class ReleaseInvoiceLock
    {
        private readonly static object _lockObject = new object();

        public static object LockObject
        {
            get => _lockObject;
        }

    }
}
