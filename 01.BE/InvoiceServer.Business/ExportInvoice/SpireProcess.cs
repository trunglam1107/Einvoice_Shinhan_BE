using Spire.Xls;

namespace InvoiceServer.Business.ExportInvoice
{
    public class SpireProcess
    {
        public Workbook Workbook { get; set; }
        public Worksheet Worksheet { get; set; }

        public SpireProcess(string fullPathFile)
        {
            Active(fullPathFile);
        }


        private void Active(string fullPathFile)
        {
            Workbook = new Workbook();
            Workbook.LoadFromFile(fullPathFile);
            Worksheet = Workbook.Worksheets[0];
        }
    }
}
