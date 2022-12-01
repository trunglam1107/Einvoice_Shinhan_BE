using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InvoiceServer.Business.ExportXml
{
    public class ReportGeneralMonthlyXml
    {
        public readonly ReportCombineExport reportInvoiceDetailIvoice;
        public readonly PrintConfig Config;
        public readonly SystemSettingInfo systemSetting;



        public ReportGeneralMonthlyXml(ReportCombineExport reportInvoiceDetai, PrintConfig config)
        {
            Config = config;
            reportInvoiceDetailIvoice = reportInvoiceDetai;
        }



        public XElement WriteFileXML()
        {


            #region Common
            #region ttchung ttchungPeriod
            XElement ttchungPeriod = new XElement("TTChung",
                                                       new XElement("PBan", "2.0.0"), //lên phiên bản từ 2.0.0 => 2.0.1
                                                       new XElement("MSo", "01/TH-HĐĐT"),
                                                       new XElement("Ten", DefaultFields.NAME_FILE_REPORT_MONTHLY),
                                                       new XElement("SBTHDLieu", reportInvoiceDetailIvoice.SBTHDulieu),
                                                       new XElement("LKDLieu", reportInvoiceDetailIvoice.periods),
                                                       new XElement("KDLieu", CheckMonth(reportInvoiceDetailIvoice.Month.ToString()) + '/' + reportInvoiceDetailIvoice.Year.ToString()),
                                                       //new XElement("KDLieu", reportInvoiceDetailIvoice.Month.ToString() + '/' + reportInvoiceDetailIvoice.Year.ToString()),
                                                       new XElement("LDau", reportInvoiceDetailIvoice.Time),
                                                       new XElement("NLap", DateTime.Now.ToString("yyyy-MM-dd")),
                                                       new XElement("TNNT", reportInvoiceDetailIvoice.Company.CompanyName),
                                                       new XElement("MST", reportInvoiceDetailIvoice.Company.TaxCode),
                                                       new XElement("DVTTe","VND"),
                                                       new XElement("HDDIn", 0),
                                                       new XElement("LHHoa", 9)
                                                       );
            #endregion

            #region ttchung ttchung
            XElement ttchung = new XElement("TTChung",
                                                       new XElement("PBan", "2.0.0"),//lên phiên bản từ 2.0.0 => 2.0.1
                                                       new XElement("MSo", "01/TH-HĐĐT"),
                                                       new XElement("Ten", DefaultFields.NAME_FILE_REPORT_MONTHLY),
                                                       new XElement("SBTHDLieu", reportInvoiceDetailIvoice.SBTHDulieu),
                                                       new XElement("LKDLieu", reportInvoiceDetailIvoice.periods),
                                                       new XElement("KDLieu", CheckMonth(reportInvoiceDetailIvoice.Month.ToString()) + '/' + reportInvoiceDetailIvoice.Year.ToString()),
                                                       //new XElement("KDLieu", reportInvoiceDetailIvoice.Month.ToString() + '/' + reportInvoiceDetailIvoice.Year.ToString()),
                                                       new XElement("LDau", reportInvoiceDetailIvoice.Time),
                                                       new XElement("BSLThu", reportInvoiceDetailIvoice.TimeNo),
                                                       new XElement("NLap", DateTime.Now.ToString("yyyy-MM-dd")),
                                                       new XElement("TNNT", reportInvoiceDetailIvoice.Company.CompanyName),
                                                       new XElement("MST", reportInvoiceDetailIvoice.Company.TaxCode),
                                                       new XElement("DVTTe", "VND"),
                                                       new XElement("HDDIn", 0),
                                                       new XElement("LHHoa", 9)
                                                       );


            #region NDBTHDLIEU
            XElement NDBTHDLieu = new XElement("NDBTHDLieu");
            NDBTHDLieu.Add(ReportItemDetail());
            #endregion


            #region Signature 
            /* XElement signature = new XElement("Signature ",
                                                  new XElement("SignedInfo",
                                                                            new XElement("CanonicalizationMethod", "Algorithm = http://www.w3.org/TR/2001/REC-xml-c14n-20010315"),
                                                                            new XElement("SignatureMethod", "Algorithm=http://www.w3.org/2000/09/xmldsig#rsa-sha1"),
                                                                            new XElement("Reference ", "URI =#7025a5d5-637d-4260-bbb6-4d598f26acf1",
                                                                                             new XElement("Transforms ",
                                                                                                              new XElement("Transforms ", "Algorithm = http://www.w3.org/2000/09/xmldsig#enveloped-signature"),
                                                                                                              new XElement("Transforms ", "Algorithm = http://www.w3.org/TR/2001/REC-xml-c14n-20010315")),
                                                                                              new XElement("DigestMethod", "Algorithm = http://www.w3.org/2000/09/xmldsig#sha1"),
                                                                                              new XElement("DigestValue", "2ZEDtYQaZLj18rCP5jGYqS/8OrI")),
                                                                             new XElement("Reference ", "URI=#signtime",
                                                                                             new XElement("Transforms ",
                                                                                                              new XElement("Transforms ", "Algorithm = http://www.w3.org/2000/09/xmldsig#enveloped-signature"),
                                                                                                              new XElement("Transforms ", "Algorithm = http://www.w3.org/TR/2001/REC-xml-c14n-20010315")),
                                                                                              new XElement("DigestMethod", "Algorithm = http://www.w3.org/2000/09/xmldsig#sha1"),
                                                                                              new XElement("DigestValue", "2ZEDtYQaZLj18rCP5jGYqS/8OrI"))),

                                                  new XElement("SignatureValue", "",
                                                  new XElement("KeyInfo",
                                                                         new XElement("X509Data",
                                                                                                 new XElement("X509SubjectName"),
                                                                                                  new XElement("X509Certificate"))),

                                                  new XElement("Object", new XAttribute("Id", "signtime"),
                                                                                        new XElement("SignatureProperties",
                                                                                                                           new XElement("SignatureProperty ", new XAttribute("Target", "#signtime"),
                                                                                                                                                                                                   new XElement("SigningTime", new XAttribute("xmlns", "https://hoadondientu.gdt.gov.vn/#signatureproperties"), "2021-11-10T11:39:38"))))));

 */
            #endregion

            #region DLDTHop
            XElement DLBTHop = null;
            string id = "Report_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (reportInvoiceDetailIvoice.Time == 1)
            {
                DLBTHop = new XElement("DLBTHop", ttchungPeriod, NDBTHDLieu, new XAttribute("Id", id));
            }
            else
            {
                DLBTHop = new XElement("DLBTHop", ttchung, NDBTHDLieu, new XAttribute("Id", id));
            }
            #endregion

            #region NNT
            /* XElement NNT = new XElement("DLBTHop", signature);*/
            #endregion
            #region DSCKS
            XElement DSCKS = new XElement("DSCKS", new XElement("NNT"));
            #endregion

            #region DSCKS
            XElement BTHDLieu = new XElement("BTHDLieu", DLBTHop, DSCKS);
            #endregion
            #endregion
            #endregion


            return BTHDLieu;
        }

        private XElement ReportItemDetail()
        {

            XElement DSDLieu = new XElement("DSDLieu");
            XElement DLieu = new XElement("DLieu");
            if (this.reportInvoiceDetailIvoice.Items.Count() == 0)
            {
                DSDLieu.Add(DLieu);
                return DSDLieu;
            }
            var detail = this.reportInvoiceDetailIvoice.Items;
            
            for (int i = 0; i < detail.Count(); i++)
            {

                if (!String.IsNullOrEmpty(detail[i].TaxCode))
                {
                    if (detail[i].InvoiceType > 1)
                    {
                        LKDLieu.ListLKDLieu.TryGetValue(reportInvoiceDetailIvoice.IsMonth, out string periodsGeneral);
                        if(!detail[i].Currency.Equals("VND"))
                        {
                            DLieu = new XElement("DLieu",
                                                              new XElement("STT", i + 1),
                                                              new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                              new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                              new XElement("SHDon", detail[i].InvoiceNo),
                                                              new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                              new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                              new XElement("MSTNMua", detail[i].TaxCode),
                                                              new XElement("MKHang", detail[i].CustomerCode),
                                                              new XElement("MHHDVu", detail[i].ProductCode),
                                                              new XElement("THHDVu", detail[i].ProductName),
                                                              new XElement("DVTinh", detail[i].Unit),
                                                              new XElement("SLuong", detail[i].Quantity),
                                                              new XElement("TTCThue", detail[i].Total),
                                                              new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                              new XElement("TgTThue", detail[i].TotalTax),
                                                              new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                              new XElement("TGia", detail[i].CurrencyExchangeRate),
                                                              new XElement("TThai", detail[i].InvoiceType),
                                                              new XElement("LHDCLQuan", "1"),
                                                               new XElement("KHMSHDCLQuan", (detail[i].ParentSymbol).Length > 0 ? detail[i].ParentSymbol.Substring(0, 1) : detail[i].ParentSymbol),
                                                               new XElement("KHHDCLQuan", detail[i].ParentSymbol.Length > 6 ? detail[i].ParentSymbol.Substring(1, 6) : detail[i].ParentSymbol),
                                                               new XElement("SHDCLQuan", detail[i].ParentNo),
                                                              new XElement("GChu", detail[i].Note)
                                                        );
                        }
                        else
                        {
                            DLieu = new XElement("DLieu",
                                                           new XElement("STT", i + 1),
                                                           new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                           new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                           new XElement("SHDon", detail[i].InvoiceNo),
                                                           new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                           new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                           new XElement("MSTNMua", detail[i].TaxCode),
                                                           new XElement("MKHang", detail[i].CustomerCode),
                                                           new XElement("MHHDVu", detail[i].ProductCode),
                                                           new XElement("THHDVu", detail[i].ProductName),
                                                           new XElement("DVTinh", detail[i].Unit),
                                                           new XElement("SLuong", detail[i].Quantity),
                                                           new XElement("TTCThue", detail[i].Total),
                                                           new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                           new XElement("TgTThue", detail[i].TotalTax),
                                                           new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                           new XElement("TThai", detail[i].InvoiceType),
                                                           new XElement("LHDCLQuan", "1"),
                                                            new XElement("KHMSHDCLQuan", (detail[i].ParentSymbol).Length > 0 ? detail[i].ParentSymbol.Substring(0, 1) : detail[i].ParentSymbol),
                                                            new XElement("KHHDCLQuan", detail[i].ParentSymbol.Length > 6 ? detail[i].ParentSymbol.Substring(1, 6) : detail[i].ParentSymbol),
                                                            new XElement("SHDCLQuan", detail[i].ParentNo),
                                                           new XElement("GChu", detail[i].Note)
                                                     );
                        }
                        
                    }
                    else
                    {
                        if(!detail[i].Currency.Equals("VND"))
                        {
                            DLieu = new XElement("DLieu",
                                                           new XElement("STT", i + 1),
                                                           new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                           new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                           new XElement("SHDon", detail[i].InvoiceNo),
                                                           new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                           new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                           new XElement("MSTNMua", detail[i].TaxCode),
                                                           new XElement("MKHang", detail[i].CustomerCode),
                                                           new XElement("MHHDVu", detail[i].ProductCode),
                                                           new XElement("THHDVu", detail[i].ProductName),
                                                           new XElement("DVTinh", detail[i].Unit),
                                                           new XElement("SLuong", detail[i].Quantity),
                                                           new XElement("TTCThue", detail[i].Total),
                                                            //new XElement("TSuat", detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName),
                                                            new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                           new XElement("TgTThue", detail[i].TotalTax),
                                                           new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                           new XElement("TGia", detail[i].CurrencyExchangeRate),
                                                           new XElement("TThai", detail[i].InvoiceType),
                                                           new XElement("GChu", detail[i].Note)
                                                           );
                        }
                        else
                        {
                            DLieu = new XElement("DLieu",
                                                           new XElement("STT", i + 1),
                                                           new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                           new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                           new XElement("SHDon", detail[i].InvoiceNo),
                                                           new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                           new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                           new XElement("MSTNMua", detail[i].TaxCode),
                                                           new XElement("MKHang", detail[i].CustomerCode),
                                                           new XElement("MHHDVu", detail[i].ProductCode),
                                                           new XElement("THHDVu", detail[i].ProductName),
                                                           new XElement("DVTinh", detail[i].Unit),
                                                           new XElement("SLuong", detail[i].Quantity),
                                                           new XElement("TTCThue", detail[i].Total),
                                                            //new XElement("TSuat", detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName),
                                                            new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                           new XElement("TgTThue", detail[i].TotalTax),
                                                           new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                           new XElement("TThai", detail[i].InvoiceType),
                                                           new XElement("GChu", detail[i].Note)
                                                           );
                        }
                        
                    }
                }
                else
                {
                    if (detail[i].InvoiceType > 1)
                    {
                        LKDLieu.ListLKDLieu.TryGetValue(reportInvoiceDetailIvoice.IsMonth, out string periodsGeneral);
                        if(!detail[i].Currency.Equals("VND"))
                        {
                            DLieu = new XElement("DLieu",
                                                       new XElement("STT", i + 1),
                                                       new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                       new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                       new XElement("SHDon", detail[i].InvoiceNo),
                                                       new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                       new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                       new XElement("MSTNMua", detail[i].TaxCode),
                                                       new XElement("MKHang", detail[i].CustomerCode),
                                                       new XElement("MHHDVu", detail[i].ProductCode),
                                                       new XElement("THHDVu", detail[i].ProductName),
                                                       new XElement("DVTinh", detail[i].Unit),
                                                       new XElement("SLuong", detail[i].Quantity),
                                                       new XElement("TTCThue", detail[i].Total),
                                                       new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                       new XElement("TgTThue", detail[i].TotalTax),
                                                       new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                       new XElement("TGia", detail[i].CurrencyExchangeRate),
                                                       new XElement("TThai", detail[i].InvoiceType),
                                                       new XElement("LHDCLQuan", "1"),
                                                       new XElement("KHMSHDCLQuan", (detail[i].ParentSymbol).Length > 0 ? detail[i].ParentSymbol.Substring(0, 1) : detail[i].ParentSymbol),
                                                       new XElement("KHHDCLQuan", detail[i].ParentSymbol.Length > 6 ? detail[i].ParentSymbol.Substring(1, 6) : detail[i].ParentSymbol),
                                                       new XElement("SHDCLQuan", detail[i].ParentNo),
                                                       new XElement("GChu", detail[i].Note)
                                                       );
                        }
                        else
                        {
                            DLieu = new XElement("DLieu",
                                                       new XElement("STT", i + 1),
                                                       new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                       new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                       new XElement("SHDon", detail[i].InvoiceNo),
                                                       new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                       new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                       new XElement("MSTNMua", detail[i].TaxCode),
                                                       new XElement("MKHang", detail[i].CustomerCode),
                                                       new XElement("MHHDVu", detail[i].ProductCode),
                                                       new XElement("THHDVu", detail[i].ProductName),
                                                       new XElement("DVTinh", detail[i].Unit),
                                                       new XElement("SLuong", detail[i].Quantity),
                                                       new XElement("TTCThue", detail[i].Total),
                                                       new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                       new XElement("TgTThue", detail[i].TotalTax),
                                                       new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                       new XElement("TThai", detail[i].InvoiceType),
                                                       new XElement("LHDCLQuan", "1"),
                                                       new XElement("KHMSHDCLQuan", (detail[i].ParentSymbol).Length > 0 ? detail[i].ParentSymbol.Substring(0, 1) : detail[i].ParentSymbol),
                                                       new XElement("KHHDCLQuan", detail[i].ParentSymbol.Length > 6 ? detail[i].ParentSymbol.Substring(1, 6) : detail[i].ParentSymbol),
                                                       new XElement("SHDCLQuan", detail[i].ParentNo),
                                                       new XElement("GChu", detail[i].Note)
                                                       );
                        }
                       
                    }
                    else
                    {
                        if (!detail[i].Currency.Equals("VND"))
                        {
                            DLieu = new XElement("DLieu",
                                                        new XElement("STT", i + 1),
                                                        new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                        new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                        new XElement("SHDon", detail[i].InvoiceNo),
                                                        new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                        new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                        new XElement("MSTNMua", detail[i].TaxCode),
                                                        new XElement("MKHang", detail[i].CustomerCode),
                                                        new XElement("MHHDVu", detail[i].ProductCode),
                                                        new XElement("THHDVu", detail[i].ProductName),
                                                        new XElement("DVTinh", detail[i].Unit),
                                                        new XElement("SLuong", detail[i].Quantity),
                                                        new XElement("TTCThue", detail[i].Total),
                                                        new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                        new XElement("TgTThue", detail[i].TotalTax),
                                                        new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                        new XElement("TGia", detail[i].CurrencyExchangeRate),
                                                        new XElement("TThai", detail[i].InvoiceType),
                                                        new XElement("GChu", detail[i].Note)
                                                        );
                        }
                        else
                        {
                            DLieu = new XElement("DLieu",
                                                        new XElement("STT", i + 1),
                                                        new XElement("KHMSHDon", (detail[i].InvoiceSymbol).Length > 0 ? detail[i].InvoiceSymbol.Substring(0, 1) : detail[i].InvoiceSymbol),
                                                        new XElement("KHHDon", detail[i].InvoiceSymbol.Length > 6 ? detail[i].InvoiceSymbol.Substring(1, 6) : detail[i].InvoiceSymbol),
                                                        new XElement("SHDon", detail[i].InvoiceNo),
                                                        new XElement("NLap", detail[i].ReleasedDate.Value.ToString("yyyy-MM-dd")),
                                                        new XElement("TNMua", detail[i].IsOrg == true ? detail[i].CustomerName : detail[i].PersonContact),
                                                        new XElement("MSTNMua", detail[i].TaxCode),
                                                        new XElement("MKHang", detail[i].CustomerCode),
                                                        new XElement("MHHDVu", detail[i].ProductCode),
                                                        new XElement("THHDVu", detail[i].ProductName),
                                                        new XElement("DVTinh", detail[i].Unit),
                                                        new XElement("SLuong", detail[i].Quantity),
                                                        new XElement("TTCThue", detail[i].Total),
                                                        new XElement("TSuat", detail[i].TaxName != null ? detail[i].TaxName.Equals("Không chịu thuế") ? "KCT" : detail[i].TaxName : "KCT"),
                                                        new XElement("TgTThue", detail[i].TotalTax),
                                                        new XElement("TgTTToan", detail[i].SumAmountInvoice),
                                                        new XElement("TThai", detail[i].InvoiceType),
                                                        new XElement("GChu", detail[i].Note)
                                                        );
                        }
                            
                    }

                }

                DSDLieu.Add(DLieu);
            }

            return DSDLieu;
        }

        public string CheckMonth(string month)
        {
            string rMonth = "";
            if (month.Length == 1)
            {
                rMonth = "0" + month;
            }
            else
            {
                rMonth = month;
            }

            return rMonth;
        }

    }
}
