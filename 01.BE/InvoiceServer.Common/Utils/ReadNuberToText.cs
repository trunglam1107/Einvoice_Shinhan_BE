using System;
using System.Text;

namespace InvoiceServer.Common
{
    public static class ReadNuberToText
    {
        private static string join_unit(string n)
        {
            int numberChars = n.Length;
            int numberUnit = (numberChars % 3 > 0) ? (numberChars / 3 + 1) : (numberChars / 3);
            n = n.PadLeft(numberUnit * 3, '0');
            numberChars = n.Length;
            string _string = "";
            int i = 1;
            while (i <= numberUnit)
            {
                if (i == numberUnit) _string = join_number((int.Parse(n.Substring(numberChars - (i * 3), 3))).ToString()) + unit(i) + _string;
                else _string = join_number(n.Substring(numberChars - (i * 3), 3)) + unit(i) + _string;
                i += 1;
            }
            return _string;
        }

        private static string unit(int n)
        {
            string chuoi = "";
            if (n == 1) chuoi = " đồng ";
            else if (n == 2) chuoi = " nghìn ";
            else if (n == 3) chuoi = " triệu ";
            else if (n == 4) chuoi = " tỷ ";
            else if (n == 5) chuoi = " nghìn tỷ ";
            else if (n == 6) chuoi = " triệu tỷ ";
            else if (n == 7) chuoi = " tỷ tỷ ";
            return chuoi;
        }

        private static string convert_number(string n)
        {
            string _chars = "";
            if (n == "0") _chars = "không";
            else if (n == "1") _chars = "một";
            else if (n == "2") _chars = "hai";
            else if (n == "3") _chars = "ba";
            else if (n == "4") _chars = "bốn";
            else if (n == "5") _chars = "năm";
            else if (n == "6") _chars = "sáu";
            else if (n == "7") _chars = "bảy";
            else if (n == "8") _chars = "tám";
            else if (n == "9") _chars = "chín";
            return _chars;
        }

        private static string join_number(string n)
        {
            string _chars = "";
            int i = 1, j = n.Length;
            while (i <= j)
            {
                if (i == 1) _chars = convert_number(n.Substring(j - i, 1)) + _chars;
                else if (i == 2) _chars = convert_number(n.Substring(j - i, 1)) + " mươi " + _chars;
                else if (i == 3) _chars = convert_number(n.Substring(j - i, 1)) + " trăm " + _chars;
                i += 1;
            }
            return _chars;
        }

        public static string replace_special_word(string _string_in)
        {
            string _string = join_unit(_string_in);
            _string = _string.Replace("không mươi không ", "");
            _string = _string.Replace("không mươi", "linh");
            _string = _string.Replace("i không", "i");
            _string = _string.Replace("i năm", "i lăm");
            _string = _string.Replace("một mươi", "mười");
            _string = _string.Replace("mươi một", "mươi mốt");
            return _string;
        }

        public static string ConvertNumberToString(string _string_in) //đọc đc 18 số vd: 999999999999999999
        {
            try
            {
                decimal total = 0;
                decimal.TryParse(_string_in, out total);
                StringBuilder bldRs = new StringBuilder();
                total = Math.Round(total, 0);
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "linh", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "nghìn", "", "", "triệu", "", "", "tỷ", "", "", "nghìn", "", "", "triệu" };
                string nstr = total.ToString();
                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }
                for (int i = len - 1; i >= 0; i--)
                {
                    if (SubConvertNumberToString(n, i, ch, rch, u, bldRs))
                        continue;

                    bldRs.Append(" ");
                    bldRs.Append(ch[n[i]]);

                    bldRs.Append(" ");
                    bldRs.Append(i % 3 == 0 ? u[i] : u[i % 3]);
                }
                string rs = bldRs.ToString();
                if (rs[rs.Length - 1] != ' ')
                    rs += " đồng.";
                else
                    rs += "đồng.";
                if (rs.Length > 2)
                {
                    string rs1 = rs.Substring(0, 2);
                    rs1 = rs1.ToUpper();
                    rs = rs.Substring(2);
                    rs = rs1 + rs;
                }
                return rs.Trim().Replace("linh,", "linh").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,",
               "mười");
            }
            catch
            {
                return "Số bạn nhập vào quá lớn";
            }
        }

        private static bool SubConvertNumberToString(int[] n, int i, string[] ch, string[] rch, string[] u, StringBuilder bldRs)
        {
            int len = n.Length;
            if (i % 3 == 2)// số 0 ở hàng trăm
            {
                if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) return true;//nếu cả 3 số là 0 thì bỏ qua không đọc
            }
            else if (i % 3 == 1) // số ở hàng chục
            {
                return SubConvertNumberToString2(n, i, rch, bldRs);
            }
            else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
            {
                return SubConvertNumberToString3(n, i, ch, rch, u, bldRs);
            }
            return false;
        }

        private static bool SubConvertNumberToString2(int[] n, int i, string[] rch, StringBuilder bldRs)
        {
            if (n[i] == 0)
            {
                if (n[i - 1] == 0) { return true; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                else
                {
                    bldRs.Append(" ");
                    bldRs.Append(rch[n[i]]);
                    return true;// hàng chục là 0 thì bỏ qua, đọc số hàng đơn vị
                }
            }
            if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
            {
                bldRs.Append(" mười");
                return true;
            }
            return false;
        }

        private static bool SubConvertNumberToString3(int[] n, int i, string[] ch, string[] rch, string[] u, StringBuilder bldRs)
        {
            Func<int, string, string, string> textNumber = (index, value1, value2) =>
            {
                if (index % 3 == 0)
                    return value1;
                return value2;
            };
            int len = n.Length;
            if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
            {
                if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) return true;
                bldRs.Append(" ");
                bldRs.Append(textNumber(i, u[i], u[i % 3]));
                return true;
            }
            if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
            {
                bldRs.Append(" ");
                bldRs.Append((n[i + 1] == 1 || n[i + 1] == 0) ? ch[n[i]] : rch[n[i]]);
                bldRs.Append(" ");
                bldRs.Append(textNumber(i, u[i], u[i % 3]));
                return true;
            }
            if (n[i] == 5 && n[i + 1] != 0) // cách đọc số 5, nếu số hàng chục khác 0 thì đọc số 5 là lăm
            {
                bldRs.Append(" ");
                bldRs.Append(rch[n[i]]);
                bldRs.Append(" ");
                bldRs.Append(textNumber(i, u[i], u[i % 3]));
                return true;
            }
            return false;
        }

        public static string ConvertNumberToStringInvoice(string _string_in) //đọc đc 18 số vd: 999999999999999999
        {
            try
            {
                decimal total = 0;
                decimal.TryParse(_string_in, out total);
                StringBuilder bldRs = new StringBuilder();
                total = Math.Round(total, 0);
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "linh", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "nghìn", "", "", "triệu", "", "", "tỷ", "", "", "nghìn", "", "", "triệu" };
                string dau = "";
                if (total < 0)
                {
                    dau = "Âm ";
                    total = total * -1;
                }
                string nstr = total.ToString();
                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }
                for (int i = len - 1; i >= 0; i--)
                {
                    if (SubConvertNumberToString(n, i, ch, rch, u, bldRs))
                        continue;

                    //rs += " " + ch[n[i]];// đọc số
                    bldRs.Append(" ");
                    bldRs.Append(ch[n[i]]);
                    //rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                    bldRs.Append(" ");
                    bldRs.Append(i % 3 == 0 ? u[i] : u[i % 3]);
                }
                string rs = bldRs.ToString();
                if (rs.Length > 2)
                {
                    //string rs1 = rs.Substring(0, 2);
                    //rs1 = rs1.ToUpper();
                    //rs = rs.Substring(2);
                    //rs = rs1 + rs;
                    if (dau != "")
                    {
                        string rs1 = rs.Substring(1);
                        rs = dau + rs1;
                    }
                    else
                    {

                        string rs1 = rs.Substring(0, 2);
                        rs1 = rs1.ToUpper();
                        rs = rs.Substring(2);
                        rs = rs1 + rs;
                    }
                }
                return rs.Trim().Replace("linh,", "linh").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,",
               "mười");
            }
            catch
            {
                return "Số bạn nhập vào quá lớn";
            }
        }
    }
}
