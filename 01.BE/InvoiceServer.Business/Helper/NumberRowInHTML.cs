using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InvoiceServer.Business.Helper
{
    // Kiểm tra 1 chuỗi cần bao nhiêu hàng trong template
    class NumberRowInHtml
    {
        public float NameFieldLengthByInch { get; set; }  // chiều dài field Diễn giải là 11cm -> 4.33 inchs

        public string Name { get; set; }

        public int NumberOfRow { get; set; }

        public List<string> NameCuttedByLength { get; set; }

        public float FontSize { get; set; }

        private string NameUnSign { get; set; }


        public NumberRowInHtml()
        {
            this.NameFieldLengthByInch = 1; // Đơn vị inch
            this.NameCuttedByLength = new List<string>();
            this.FontSize = 16;
        }

        public NumberRowInHtml(string name, float fieldLength, float fontSize)
            : this()
        {
            this.NameFieldLengthByInch = fieldLength; // Đơn vị inch
            name = name.Trim();
            this.Name = name;
            this.NameUnSign = RemoveUnicode(name);
            this.FontSize = fontSize;

            this.NumberOfRow = this.CountRow();
        }

        // itextSharp chạy sai với chữ tiếng việt => chuyển có dấu thành không dấu
        public static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }

        // Trả về chiều dài của text theo 0.01 inch
        private float MearsureText(string textUnSign)
        {
            ///
            string pathFont = Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, "Data\\Asset\\font-times-new-roman.ttf");
            BaseFont bf = BaseFont.CreateFont(pathFont, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font font = new Font(bf, this.FontSize, Font.NORMAL, new BaseColor(0, 0, 255));
            Phrase phrase = new Phrase(textUnSign, font);
            var lengthInch = ColumnText.GetWidth(phrase);
            return lengthInch;
        }

        private int CountRow()
        {
            var itemNameByLines = new List<string>();   // List chứa name được tách ra thành nhiều dòng nếu quá dài
            var itemNameUnSign = this.NameUnSign;
            var line = "";
            int countWord = 0;
            while (itemNameUnSign.Length > 0)
            {
                countWord++;
                var lineSubOneWord = "";
                var lineAddOneWord = MoveWord(line, itemNameUnSign, out lineSubOneWord);

                if (MearsureText(lineAddOneWord) / 100f > this.NameFieldLengthByInch)   // line đã dài hơn 1 dòng
                {
                    if (countWord == 1) // trường hợp có 1 từ mà đã dài hơn 1 dòng trong template: cắt từ đó ra thành 2
                    {
                        var wordBefor = lineAddOneWord[0].ToString();
                        var wordAfter = lineAddOneWord.Substring(1);
                        // cắt và kiểm tra từng ký tự một cho đến khi length dài hơn 1 dòng thì dừng lại
                        while (MearsureText(wordBefor) / 100f < this.NameFieldLengthByInch)
                        {
                            wordBefor = wordBefor + wordAfter[0];
                            wordAfter = wordAfter.Substring(1);
                        }
                        itemNameByLines.Add(wordBefor.Substring(0, wordBefor.Length - 1));
                        var lastChar = wordBefor.Substring(wordBefor.Length - 1);
                        itemNameUnSign = lastChar + wordAfter + lineSubOneWord;  // còn dư 1 số ký tự
                        countWord = 0;
                        line = "";
                        continue;
                    }
                    else    // Trường hợp có nhiều từ mới dài hơn 1 dòng
                    {
                        itemNameByLines.Add(line);
                        line = "";
                        countWord = 0;
                        itemNameUnSign = itemNameUnSign.Trim();
                        continue;
                    }
                }

                line = lineAddOneWord;
                itemNameUnSign = lineSubOneWord;
                if (itemNameUnSign.Length == 0)
                {
                    itemNameByLines.Add(line);
                    line = "";
                    countWord = 0;
                }
            }

            this.CutName(itemNameByLines);

            return itemNameByLines.Count == 0 ? 1 : itemNameByLines.Count;
        }

        // Chuyển 1 từ đầu tiên từ s1 sang s2
        private string MoveWord(string s1, string s2, out string s2SubOneWord)
        {
            StringBuilder res = new StringBuilder(s1);
            while (s2.Length > 0 && char.IsWhiteSpace(s2[0]))
            {
                res.Append(s2[0]);
                s2 = s2.Substring(1);
            }
            while (s2.Length > 0 && !char.IsWhiteSpace(s2[0]))
            {
                res.Append(s2[0]);
                s2 = s2.Substring(1);
            }
            s2SubOneWord = s2;
            return res.ToString();
        }

        // Cắt chuỗi lớn ra thành từng hàng để fit template
        private void CutName(List<string> itemNameByLines)
        {
            var name = this.Name;
            var line = "";
            var listCutted = new List<string>();
            foreach (var rowName in itemNameByLines)
            {
                line = name.Substring(0, rowName.Length);
                name = name.Substring(rowName.Length);
                while (name.Length > 0 && char.IsWhiteSpace(name[0]))
                {
                    name = name.Substring(1);
                }
                listCutted.Add(line);
            }

            this.NameCuttedByLength = listCutted;
        }
    }
}
