using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace InvoiceServer.Business.ExportXml
{
    public class ParseXML
    {
        public readonly InvoicePrintInfo invoice;
        public readonly PrintConfig Config;
        public readonly SystemSettingInfo systemSetting;

        public ParseXML(InvoiceExportXmlModel invoiceExportXmlModel)
        {
            Config = invoiceExportXmlModel.PrintConfig;

            invoice = invoiceExportXmlModel.InvoicePrintInfo;
            InitStringPropertiesToEmpty(this.invoice);

            systemSetting = invoiceExportXmlModel.SystemSettingInfo;
        }

        public void InitStringPropertiesToEmpty(object obj)
        {
            if(obj != null)
            {
                var properties = obj.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        var stringVal = property.GetValue(obj, null);
                        if (stringVal == null)
                            property.SetValue(obj, "");
                    }
                }
            }
        }

        public XElement GenerateXMLWithoutCode()
        {
            #region Common
            #region TTCHUNG
            XElement TTHDLQuan = Create_TTHDLQuan_Element();

            XElement ttchung = new XElement("TTChung",
                                    new XElement("PBan", "2.0.0"),
                                    //new XElement("PBan", "2.0.1"),
                                    new XElement("THDon", DefaultFields.InvoiceName),
                                    //new XElement("KHMSHDon", DefaultFields.INVOICECODE),
                                    new XElement("KHMSHDon", invoice.Symbol.Substring(0,1)), //DefaultFields.INVOICECODE
                                    new XElement("KHHDon", invoice.Symbol.Trim()?.Substring(1, invoice.Symbol.Length - 1)),
                                    new XElement("SHDon", invoice.InvoiceNo?.Trim()),
                                    new XElement("NLap", invoice.DateRelease?.ToString("yyyy-MM-dd")),
                                    new XElement("SBKe"),
                                    new XElement("NBKe"),

                                    new XElement("DVTTe", invoice.CurrencyCode?.Trim()),
                                    new XElement("TGia", invoice.ExchangeRate),
                                    new XElement("HTTToan", invoice.TypePayment?.Trim()),
                                    //new XElement("MSTTCGP", invoice.TaxCode),
                                    new XElement("MSTTCGP", "0309889835"), //Default : 0309889835 : Unit tax code
                                    TTHDLQuan                                  
                                    );



            #endregion

            #region NDHDon
            XElement ndhdon = new XElement("NDHDon", new XElement("NBan",
                                                            new XElement("Ten", invoice.CompanyName?.Trim()),
                                                            new XElement("MST", invoice.TaxCode),
                                                            new XElement("DChi", invoice.Address?.Trim())                                                          
                                                       ),
                                                      new XElement("NMua",
                                                            new XElement("Ten", invoice.IsOrg?invoice.CustomerCompanyNameInvoice:invoice.CustomerNameInvoice?.Trim()),
                                                            new XElement("MST", invoice.CustomerTaxCode?.Trim()),
                                                            new XElement("DChi", invoice.CustomerAddress?.Trim()),
                                                            new XElement("STKNHang", invoice.CustomerBankAccountInvoice.Trim())
                                                       )                                                      
                                                      );
            ndhdon.Add(CreateItemInvoice()); //DSHHDVu
            ndhdon.Add(CreateTToanElement()); //TToan
            #endregion
            #region TTKhac
            XElement ttkhac = new XElement("TTKhac");
            #endregion


            #region DLHDON
            string id = "Invoice_" + invoice.Id;
            XElement dlhdon = new XElement("DLHDon", ttchung, ndhdon, ttkhac, new XAttribute("Id", id));
            #endregion

            #region DSCKS
            XElement dscks = new XElement("DSCKS", new XElement("NBan"),
                                                   new XElement("NMua"));
            #endregion

            #endregion

            XElement xmlTree = new XElement("HDon", dlhdon,
                                                    new XElement("DLQRCode"),
                                                    new XElement("MCCQT", this.invoice?.MCQT),
                                                    dscks);
            return xmlTree;
        }


        public XElement GenerateXMLWithCode()
        {
            #region Common
            #region TTCHUNG
            XElement TTHDLQuan = Create_TTHDLQuan_Element();
            XElement ttchung = new XElement("TTChung",
                                    //new XElement("PBan", "2.0.1"),
                                    new XElement("PBan", "2.0.0"),
                                    new XElement("THDon", DefaultFields.InvoiceName),
                                    new XElement("KHMSHDon", invoice.Symbol.Substring(0,1)), //DefaultFields.INVOICECODE
                                    new XElement("KHHDon", invoice.Symbol.Substring(1, invoice.Symbol.Length - 1)),
                                    new XElement("SHDon", invoice.InvoiceNo),
                                    new XElement("NLap", invoice.DateRelease?.ToString("yyyy-MM-dd")),
                                    new XElement("SBKe"),
                                    new XElement("NBKe"),

                                    new XElement("DVTTe", invoice.CurrencyCode?.Trim()),
                                    new XElement("TGia", invoice.ExchangeRate),
                                    new XElement("HTTToan", invoice.TypePayment?.Trim()),
                                    //new XElement("MSTTCGP", invoice.TaxCode),
                                    new XElement("MSTTCGP", "0309889835"), //Default : 0309889835 : Unit tax code
                                    TTHDLQuan
                                    );
            #endregion

            #region NDHDon
            XElement ndhdon = new XElement("NDHDon", new XElement("NBan",
                                                            new XElement("Ten", invoice.CompanyName?.Trim()),
                                                            new XElement("MST", invoice.TaxCode),
                                                            new XElement("DChi", invoice.Address?.Trim())
                                                       ),

                                                    new XElement("NMua",
                                                            new XElement("Ten", invoice.IsOrg ? invoice.CustomerCompanyNameInvoice : invoice.CustomerNameInvoice?.Trim()),
                                                            new XElement("MST", invoice.CustomerTaxCode?.Trim()),
                                                            new XElement("DChi", invoice.CustomerAddress?.Trim()),
                                                            new XElement("STKNHang", invoice.CustomerBankAccountInvoice.Trim())
                                                       )
                                                      );
            ndhdon.Add(CreateItemInvoice());
            ndhdon.Add(CreateTToanElement()); //TToan
            //XElement xmlPayment2 = new XElement("TToan",
            //                                          new XElement("THTTLTSuat", new XElement("LTSuat",
            //                                                                                    new XElement("TSuat", invoice.Tax),
            //                                                                                    new XElement("ThTien", invoice.Sum),
            //                                                                                    new XElement("TThue", invoice.TaxAmout))),
            //                                          new XElement("TgTCThue", invoice.Total),
            //                                          new XElement("TgTThue", invoice.TaxAmout),
            //                                          new XElement("TTCKTMai", invoice.DiscountAmount),
            //                                          new XElement("TgTTTBSo", invoice.Sum),
            //                                          new XElement("TgTTTBChu", invoice.AmountInwords));

            //ndhdon.Add(xmlPayment2);
            #endregion

            #region TTKhac
            XElement ttkhac = new XElement("TTKhac");
            #endregion


            #region DLHDON
            string id = "Invoice_" + invoice.Id;
            XElement dlhdon = new XElement("DLHDon", ttchung, ndhdon, ttkhac, new XAttribute("Id", id));
            #endregion


            #region DSCKS
            XElement DSCKS = new XElement("DSCKS", new XElement("NBan"),
                                                   new XElement("NMua"));

            #endregion


            #endregion
            XElement xmlTree = new XElement("HDon", dlhdon,
                                                    new XElement("DLQRCode"),
                                                    new XElement("MCCQT", this.invoice?.MCQT),
                                                    DSCKS);

            return xmlTree;
        }


        private XElement CreateItemInvoice()
        {
            XElement xmlTree = new XElement("DSHHDVu");
            if (this.invoice.InvoiceItems.Count == 0)
            {
                return xmlTree;
            }
            var invoiceDetail = invoice.InvoiceItems;
            for (int i = 0; i < invoiceDetail.Count(); i++)
            {
                StringBuilder productName = new StringBuilder();
                if (invoiceDetail[i].Discount == null || invoiceDetail[i].Discount == false)
                {
                    productName.Append(invoiceDetail[i].ProductName);
                }
                else
                {
                    productName = productName.Append(invoiceDetail[i].DiscountDescription);
                }
                if (invoiceDetail[i].TaxId == 4)
                {
                    invoiceDetail[i].AmountTax = null;
                }
                invoiceDetail[i].ProductName = productName.ToString();

                string tax = null;
                if (invoiceDetail[i]?.TaxCode != null)
                {
                    TaxCodes.ListTax.TryGetValue(invoiceDetail[i]?.TaxCode, out tax);
                }

                XElement xmlInvoiceDetail = new XElement("HHDVu",
                                                       new XElement("TChat", "1"),
                                                       new XElement("STT", i + 1),
                                                       new XElement("THHDVu", invoiceDetail[i].ProductName ?? ""),
                                                       new XElement("SLuong", (invoiceDetail[i].Quantity ?? 0)),
                                                       new XElement("DGia", FormatNumber.SetFractionDigit(invoiceDetail[i].Price ?? 0, this.systemSetting.Amount)),
                                                       new XElement("TLCKhau", 0),
                                                       new XElement("STCKhau", 0),
                                                       new XElement("ThTien", FormatNumber.SetFractionDigit(invoiceDetail[i].Total ?? 0, this.systemSetting.Amount)),
                                                       new XElement("TSuat", tax)
                                                       );
                xmlTree.Add(xmlInvoiceDetail);

            }

            return xmlTree;
        }

        
        private XElement CreateTToanElement()
        {
            string tax = null;
            if (invoice.TaxCodeName != null)
            {
                TaxCodes.ListTax.TryGetValue(invoice.TaxCodeName, out tax);
            }
            XElement TToan = new XElement("TToan");
            XElement THTTLTSuat = new XElement("THTTLTSuat", 
                                            new XElement("LTSuat", 
                                                new XElement("TSuat", tax),
                                                new XElement("ThTien", this.invoice.Total),
                                                new XElement("TThue", this.invoice.TaxAmout)
                                            )
                                       );
            TToan.Add(
                            THTTLTSuat,
                            new XElement("TgTCThue", this.invoice.Total),
                            new XElement("TgTThue", this.invoice.TaxAmout),
                            new XElement("DSLPhi"),
                            new XElement("TgTTTBSo", this.invoice.Sum),
                            new XElement("TgTTTBChu", this.invoice.AmountInwords)
                          );        

            return TToan;
        }

        private XElement Create_TTHDLQuan_Element()
        {
            XElement TTHDLQuan = new XElement("TTHDLQuan");
            //if (this.invoice != null && this.invoice.ParentInvoice != null
            //    && this.invoice.Id != this.invoice.ParentInvoice.ID) //Điều chỉnh thông tin sẽ có Id = ParentID
            if(this.invoice != null && this.invoice.ReplacedInvoiceID.HasValue
                && this.invoice.Id != this.invoice.ReplacedInvoiceID)
            {
                //var isAdjustment = this.invoice.InvoiceType != (int)InvoiceType.Substitute;
                //var parentInvoice = this.invoice.ParentInvoice;
                //TTHDLQuan.Add(new XElement("TCHDon", isAdjustment ? 2 : 1));
                //TTHDLQuan.Add(new XElement("LHDCLQuan", 1));
                //TTHDLQuan.Add(new XElement("KHMSHDCLQuan", parentInvoice.SYMBOL.Substring(0, 1)));
                //TTHDLQuan.Add(new XElement("KHHDCLQuan", parentInvoice.SYMBOL.Trim()?.Substring(1, invoice.Symbol.Length - 1)));
                //TTHDLQuan.Add(new XElement("SHDCLQuan", parentInvoice.NO));
                //TTHDLQuan.Add(new XElement("NLHDCLQuan",
                //    parentInvoice.RELEASEDDATE.HasValue ?
                //    parentInvoice.RELEASEDDATE.Value.ToString("yyyy-MM-dd") : ""));
                //TTHDLQuan.Add(new XElement("GChu", this.invoice.InvoiceNote));
                var kyHieu = this.invoice.ReplacedSymbol.Substring(0, 1) + this.invoice.ReplacedSymbol.Trim()?.Substring(1, this.invoice.ReplacedSymbol.Length - 1);
                var isAdjustment = this.invoice.InvoiceType != (int)InvoiceType.Substitute;
                TTHDLQuan.Add(new XElement("TCHDon", isAdjustment ? 2 : 1));
                TTHDLQuan.Add(new XElement("LHDCLQuan", 1));
                TTHDLQuan.Add(new XElement("KHMSHDCLQuan", this.invoice.ReplacedSymbol.Substring(0, 1)));
                TTHDLQuan.Add(new XElement("KHHDCLQuan", this.invoice.ReplacedSymbol.Trim()?.Substring(1, this.invoice.ReplacedSymbol.Length - 1)));
                TTHDLQuan.Add(new XElement("SHDCLQuan", this.invoice.ReplacedInvoiceNo));
                TTHDLQuan.Add(new XElement("NLHDCLQuan", this.invoice.ReplacedReleaseDate.ToString("yyyy-MM-dd")));
                if (this.invoice.InvoiceType == 1)
                {
                    TTHDLQuan.Add(new XElement("GChu", String.Format("Hóa đơn thay thế cho hóa đơn {0} ký hiệu {1}", this.invoice.ReplacedInvoiceNo, kyHieu)));
                }
                else
                {
                    TTHDLQuan.Add(new XElement("GChu", String.Format("Hóa đơn điều chỉnh cho hóa đơn {0} ký hiệu {1}", this.invoice.ReplacedInvoiceNo, kyHieu)));
                }
                
            }

            return TTHDLQuan;
        }

        public static XmlDocument CreateObjectSigningTime()
        {
            XmlDocument doc = new XmlDocument();

            XmlElement element1 = doc.CreateElement("Object");
            doc.AppendChild(element1);

            XmlElement element2 = doc.CreateElement("SignatureProperties");
            element1.AppendChild(element2);

            XmlElement element3 = doc.CreateElement("SignatureProperty");
            element3.SetAttribute("Target", "#signtime");
            element2.AppendChild(element3);

            XmlElement element4 = doc.CreateElement("SigningTime");
            XmlText text2 = doc.CreateTextNode(DateTime.Now.ToString("s"));
            element4.AppendChild(text2);
            element3.AppendChild(element4);
            return doc;
        }
   
        public static XElement RegisterInvoice(DeclarationInfo itemx)
        {
            #region Common
            #region TTCHUNG
            XElement ttchung = new XElement("TTChung",
                               new XElement("PBan", "2.0.0"),
                               //new XElement("PBan", "2.0.1"),
                               new XElement("MSo", "01/ĐKTĐ-HĐĐT"),
                               new XElement("Ten", "Tờ khai đăng ký/thay đổi thông tin sử dụng hóa đơn điện tử"),
                               new XElement("HThuc", itemx.DeclarationType ?? 1),
                               new XElement("TNNT", itemx.CompanyName),
                               new XElement("MST", itemx.CompanyTaxCode),
                               new XElement("CQTQLy", itemx.TaxCompanyName),
                               new XElement("MCQTQLy", itemx.TaxCompanyCode),
                               new XElement("NLHe", itemx.CustName),
                               new XElement("DCLHe", itemx.CustAddress),
                               new XElement("DCTDTu", itemx.CustEmail),

                                new XElement("DTLHe", itemx.CustPhone),
                                new XElement("DDanh", itemx.ProvinceCity),
                                new XElement("NLap", itemx.DeclarationDate.ToString("yyyy-MM-dd"))
                               );
            #endregion

            #region NDHDon
            
            XElement ndtkhai = new XElement("NDTKhai", new XElement("HTHDon",
                                                                             new XElement("CMa", itemx.HTHDon.Equals(HTHDon.CoMa) ? 1 : 0),
                                                                             new XElement("KCMa", itemx.HTHDon.Equals(HTHDon.KhongCoMa) ? 1 : 0)),
                                                                 new XElement("HTGDLHDDT",
                                                                             new XElement("NNTDBKKhan", itemx.HTGDLHDDT.Equals(HTGDLHDDT.NNTDBKKhan) ? 1 : 0),
                                                                             new XElement("NNTKTDNUBND", itemx.HTGDLHDDT.Equals(HTGDLHDDT.NNTKTDNUBND) ? 1 : 0),
                                                                             new XElement("CDLTTDCQT", itemx.HTGDLHDDT.Equals(HTGDLHDDT.CDLTTDCQT) ? 1 : 0),
                                                                             new XElement("CDLQTCTN", itemx.HTGDLHDDT.Equals(HTGDLHDDT.CDLQTCTN) ? 1 : 0)),
                                                                 new XElement("PThuc",
                                                                            new XElement("CDDu", itemx.PThuc.Equals(PThuc.CDDu) ? 1 : 0),
                                                                             new XElement("CBTHop", itemx.PThuc.Equals(PThuc.CBTHop) ? 1 : 0)),

                                                                  new XElement("LHDSDung",
                                                                            new XElement("HDGTGT", itemx.LHDSDung.Equals(LHDSDung.HDGTGT) ? 1 : 0),
                                                                             new XElement("HDBHang", itemx.LHDSDung.Equals(LHDSDung.HDBHang) ? 1 : 0),
                                                                             new XElement("HDBTSCong", itemx.LHDSDung.Equals(LHDSDung.HDBTSCong) ? 1 : 0),
                                                                             new XElement("HDBHDTQGia", itemx.LHDSDung.Equals(LHDSDung.HDBHDTQGia) ? 1 : 0),
                                                                             new XElement("HDKhac", itemx.LHDSDung.Equals(LHDSDung.HDKhac) ? 1 : 0),
                                                                             new XElement("CTu", itemx.LHDSDung.Equals(LHDSDung.CTu) ? 1 : 0)));


            ndtkhai.Add(DeclareRelease(itemx));
            #endregion


            #region DLHDON
            string _code = "declaration_" + itemx.ID;//Guid.NewGuid().ToString();
            XElement dltkhai = new XElement("DLTKhai", ttchung, ndtkhai, new XAttribute("Id", _code));
            XElement dscks = new XElement("DSCKS", new XElement("NNT"));
            #endregion

            #endregion
            XElement xmlTree = new XElement("TKhai", dltkhai, dscks);

            return xmlTree;
        }

        public static XElement DeclareRelease(DeclarationInfo itemx)
        {
            XElement element = new XElement("DSCTSSDung");
           
            if (itemx.releaseInfoes.Count > 0)
            {
                int i = 0;
                foreach (var item in  itemx.releaseInfoes)
                {
                    i++;

                    XElement  CTS = new XElement("CTS",
                  new XElement("STT", i),
                  new XElement("TTChuc", (item.ReleaseCompanyName ?? "").Trim()),
                  new XElement("Seri", (item.Seri ?? "").Trim()),
                  new XElement("TNgay", item.fromDate),
                  new XElement("DNgay", item.toDate),
                  new XElement("HThuc", item.RegisterTypeID == 0 ? 1 : item.RegisterTypeID));
                  element.Add(CTS);

                }
              

            }

            return element;


        }
    }
}
