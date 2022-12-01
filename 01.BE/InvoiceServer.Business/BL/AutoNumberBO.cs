using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvoiceServer.Business.BL
{
    public class AutoNumberBO : IAutoNumberBO
    {
        private readonly IAutoNumberRespository autoNumberRepository;
        private readonly ILastKeyRepository lastkeyRepository;
        public AutoNumberBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.autoNumberRepository = repoFactory.GetRepository<IAutoNumberRespository>();
            this.lastkeyRepository = repoFactory.GetRepository<ILastKeyRepository>();
        }

        public IEnumerable<AutoNumberViewModel> GetAll(ConditionSearchAutoNumber condition)
        {
            var auto = autoNumberRepository.GetList().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            var listAuto = new List<AutoNumberViewModel>();
            if (auto != null)
            {
                auto.ForEach(x => listAuto.Add(
                 new AutoNumberViewModel()
                 {
                     TYPEID = x.TYPEID,
                     TYPENAME = x.TYPENAME,
                     ENABLED1 = x.ENABLED1,
                     ENABLED2 = x.ENABLED2,
                     ENABLED3 = x.ENABLED3,
                     ENABLED4 = x.ENABLED4 ?? false,
                     S1TYPE = x.S1TYPE,
                     S2TYPE = x.S2TYPE,
                     S3TYPE = x.S3TYPE,
                     S4TYPE = x.S4TYPE,
                     S1 = x.S1,
                     S2 = x.S2,
                     S3 = x.S3,
                     S4 = x.S4,
                     SEPARATED = x.SEPARATED,
                     SEPARATORS = x.SEPARATORS,
                     OUTPUTLENGTH = x.OUTPUTLENGTH,
                     OUTPUTORDERS = x.OUTPUTORDERS,
                     ORDERBY = x.ORDERBY ?? 0,
                     ACTIVED = x.ACTIVED,
                     CREATEUSERSID = x.CREATEUSERSID,
                     CREATEDATE = x.CREATEDATE,
                     UPDATEUSERSID = x.UPDATEUSERSID,
                     UPDATEDATE = x.UPDATEDATE
                 }
                  ));
            }
            return listAuto;
        }

        public AutoNumberViewModel GetByID(string id)
        {
            var x = autoNumberRepository.GetById(id);
            var result = new AutoNumberViewModel();
            if (x != null)
            {
                result = new AutoNumberViewModel()
                {
                    TYPEID = x.TYPEID,
                    TYPENAME = x.TYPENAME,
                    ENABLED1 = x.ENABLED1,
                    ENABLED2 = x.ENABLED2,
                    ENABLED3 = x.ENABLED3,
                    ENABLED4 = x.ENABLED4 ?? false,
                    S1TYPE = x.S1TYPE,
                    S2TYPE = x.S2TYPE,
                    S3TYPE = x.S3TYPE,
                    S4TYPE = x.S4TYPE,
                    S1 = x.S1,
                    S2 = x.S2,
                    S3 = x.S3,
                    S4 = x.S4,
                    SEPARATED = x.SEPARATED,
                    SEPARATORS = x.SEPARATORS,
                    OUTPUTLENGTH = x.OUTPUTLENGTH,
                    OUTPUTORDERS = x.OUTPUTORDERS,
                    ORDERBY = x.ORDERBY ?? 0,
                    ACTIVED = x.ACTIVED,
                    CREATEUSERSID = x.CREATEUSERSID,
                    CREATEDATE = x.CREATEDATE,
                    UPDATEUSERSID = x.UPDATEUSERSID,
                    UPDATEDATE = x.UPDATEDATE
                };
            }
            return result;
        }
        public ResultCode Update(string code, AutoNumberViewModel model)
        {
            try
            {
                var result = this.autoNumberRepository.GetById(code);
                if (result != null)
                {
                    result = new AUTONUMBERSETTING()
                    {
                        TYPEID = result.TYPEID,
                        TYPENAME = model.TYPENAME,
                        ENABLED1 = model.ENABLED1,
                        ENABLED2 = model.ENABLED2,
                        ENABLED3 = model.ENABLED3,
                        ENABLED4 = model.ENABLED4,
                        S1TYPE = model.S1TYPE,
                        S2TYPE = model.S2TYPE,
                        S3TYPE = model.S3TYPE,
                        S4TYPE = model.S4TYPE,
                        S1 = model.S1,
                        S2 = model.S2,
                        S3 = model.S3,
                        S4 = model.S4,
                        SEPARATED = model.SEPARATED,
                        SEPARATORS = model.SEPARATORS,
                        OUTPUTLENGTH = model.OUTPUTLENGTH,
                        OUTPUTORDERS = model.OUTPUTORDERS,
                        ORDERBY = model.ORDERBY,
                        ACTIVED = model.ACTIVED,
                        CREATEUSERSID = model.CREATEUSERSID,
                        CREATEDATE = model.CREATEDATE,
                        UPDATEUSERSID = model.UPDATEUSERSID,
                        UPDATEDATE = DateTime.Now,

                    };
                    this.autoNumberRepository.Update(code, result);
                    return ResultCode.NoError;
                }
                else
                {
                    return ResultCode.UnitIssuedNotUpdate;
                }
            }
            catch
            {
                return ResultCode.UnitIssuedNotUpdate;
            }
        }

        public ResultCode Create(AutoNumberViewModel model)
        {
            var result = new AUTONUMBERSETTING()
            {
                TYPEID = model.TYPEID,
                TYPENAME = model.TYPENAME,
                ENABLED1 = model.ENABLED1,
                ENABLED2 = model.ENABLED2,
                ENABLED3 = model.ENABLED3,
                S1TYPE = model.S1TYPE,
                S2TYPE = model.S2TYPE,
                S3TYPE = model.S3TYPE,
                S1 = model.S1,
                S2 = model.S2,
                S3 = model.S3,
                SEPARATED = model.SEPARATED,
                SEPARATORS = model.SEPARATORS,
                OUTPUTLENGTH = model.OUTPUTLENGTH,
                OUTPUTORDERS = model.OUTPUTORDERS,
                ORDERBY = model.ORDERBY,
                ACTIVED = true,
                CREATEUSERSID = model.CREATEUSERSID,
                CREATEDATE = DateTime.Now,
                UPDATEUSERSID = model.CREATEUSERSID,
                UPDATEDATE = DateTime.Now
            };
            var isUpdateSuccess = autoNumberRepository.Insert(result);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.UnitIssuedNotUpdate;
        }
        public ResultCode Delete(string code)
        {
            var result = autoNumberRepository.Delete(autoNumberRepository.GetById(code));
            if (result)
            {
                return ResultCode.NoError;
            }

            return ResultCode.UnitIssuedNotUpdate;
        }


        public String CreateAutoNumber(long companyId, string typeID, string branchId, bool saved = false)
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            //Property of KeyString
            string stringKey1 = "";
            string stringKey2 = "";
            string stringKey3 = "";
            string stringKey4 = "";
            byte? outputLength = 15;
            byte? outputOrders = 3;
            bool? seperated = false;
            string seperators = "-";

            //Declare variable to process
            string keyString;
            Int32 lastKey = 1;
            string lastKeyChar;
            Int32 lastKeyLength;
            Int32 seperatorCount;
            StringBuilder strNumber = new StringBuilder();
            string seperator1 = "";
            string seperator2 = "";
            string seperator3 = "";
            string seperator4 = "";
            Int32 oldLastKey = 0;

            //Process
            AUTONUMBERSETTING rowVoucherType = autoNumberRepository.GetById(typeID);
            if (rowVoucherType != null)
            {
                stringKey1 = FindSxType(rowVoucherType.S1TYPE, rowVoucherType.S1, month, year, rowVoucherType.ENABLED1);
                stringKey2 = FindSxType(rowVoucherType.S2TYPE, rowVoucherType.S2, month, year, rowVoucherType.ENABLED2);
                stringKey3 = FindSxType(rowVoucherType.S3TYPE, rowVoucherType.S3, month, year, rowVoucherType.ENABLED3);
                stringKey4 = FindSxType(rowVoucherType.S4TYPE, branchId, month, year, rowVoucherType.ENABLED4);
                outputLength = rowVoucherType.OUTPUTLENGTH;
                outputOrders = rowVoucherType.OUTPUTORDERS;
                seperated = rowVoucherType.SEPARATED;
                seperators = rowVoucherType.SEPARATORS;
            }
            keyString = stringKey1 + stringKey2 + stringKey3 + stringKey4;
            oldLastKey = lastkeyRepository.GetLastKey(companyId, keyString);
            lastKey = oldLastKey + 1;
            InsertUpdateLastKey(saved, oldLastKey, companyId, keyString, lastKey);
            lastKeyChar = lastKey.ToString().Trim();
            lastKeyLength = lastKeyChar.Length;

            if (seperated == false)
            {
                seperatorCount = 0;
            }
            else
            {
                seperatorCount = 0;
                if (stringKey1.Length > 0)
                {
                    seperatorCount = 1;
                    seperator1 = seperators;
                }
                else
                {
                    seperator1 = "";
                }
                if (stringKey2.Length > 0)
                {
                    seperatorCount = seperatorCount + 1;
                    seperator2 = seperators;
                }
                else
                {
                    seperator2 = "";
                }
                if (stringKey3.Length > 0)
                {
                    seperatorCount = seperatorCount + 1;
                    seperator3 = seperators;
                }
                else
                {
                    seperator3 = "";
                }
                if (stringKey4.Length > 0)
                {
                    seperatorCount = seperatorCount + 1;
                    seperator4 = seperators;
                }
                else
                {
                    seperator4 = "";
                }
            }
            if (outputLength - lastKeyLength - seperatorCount - keyString.Length < 0)
            {
                //Độ dài vượt quá mức quy định. Bạn phải thiết lập lại chiều dài.
                return "";
            }

            for (int i = 0; i < outputLength - lastKeyLength - seperatorCount - keyString.Length; i++)
            {
                strNumber = strNumber.Append("0");
            }
            string stringNumber = strNumber.ToString() + lastKeyChar.ToString();

            stringKey1 = stringKey1.Trim().ToUpper();
            stringKey2 = stringKey2.Trim().ToUpper();
            stringKey3 = stringKey3.Trim().ToUpper();
            stringKey4 = stringKey4.Trim().ToUpper();
            var tempParams = Tuple.Create(seperator1, stringKey1, seperator2, stringKey2, seperator3, stringKey3);
            string newKey = NewKey(outputOrders, stringNumber, stringKey4, seperator4, tempParams);

            return newKey;
        }
        private void InsertUpdateLastKey(bool saved, Int32 oldLastKey, long companyId, string keyString, Int32 lastKey)
        {
            if (saved && oldLastKey == 0)
            {
                lastkeyRepository.InsertLastKey(companyId, keyString, lastKey);
            }
            if (saved)
            {
                //Update LastKey = LastKey + 1 to M_LastKey
                lastkeyRepository.UpdateLastKey(companyId, keyString, lastKey);
            }
        }
        public string NewKey(byte? outputOrders, string stringNumber, string stringKey4, string seperator4, Tuple<string, string, string, string, string, string> tempParams)
        {
            string seperator1 = tempParams.Item1;
            string stringKey1 = tempParams.Item2;
            string seperator2 = tempParams.Item3;
            string stringKey2 = tempParams.Item4;
            string seperator3 = tempParams.Item5;
            string stringKey3 = tempParams.Item6;
            string newKey = "";
            if (outputOrders == 0)
            {
                newKey = stringNumber + seperator1 + stringKey1 + seperator2 + stringKey2 + seperator3 + stringKey3 + seperator4 + stringKey4;
            }
            else if (outputOrders == 1)
            {
                newKey = stringKey1 + seperator1 + stringNumber + seperator2 + stringKey2 + seperator3 + stringKey3 + seperator4 + stringKey4;
            }
            else if (outputOrders == 2)
            {
                newKey = stringKey1 + seperator1 + stringKey2 + seperator2 + stringNumber + seperator3 + stringKey3 + seperator4 + stringKey4;
            }
            else if (outputOrders == 3)
            {
                newKey = stringKey1 + seperator1 + stringKey2 + seperator2 + stringKey3 + seperator3 + stringKey4 + seperator4 + stringNumber;
            }

            return newKey;
        }
        public int Length(string id)
        {
            return this.autoNumberRepository.Length(id);
        }
        public bool checkEnable(string id)
        {
            return this.autoNumberRepository.checkActive(id);
        }
        private string FindSxType(byte? nType, string s, int tranMonth, int tranYear, bool? enabled = false)
        {
            if (enabled == false)
            {
                return "";
            }
            switch (nType)
            {
                case 1://Theo Mã phân loại
                    return s;
                case 2://Theo mã đơn vị
                    return s;
                case 3://Theo hằng số
                    return s;
                case 4://Theo tháng (MM)
                    return tranMonth.ToString("00");
                case 5://Theo Năm (YY)
                    return tranYear.ToString().Substring(2, 2);
                case 6://Theo Năm (YYYY)
                    return tranYear.ToString();
                case 7://Theo Tháng Năm (MMYY)
                    return string.Format("{0}{1}", tranMonth.ToString("00"), tranYear.ToString().Substring(2, 2));
                case 8://Theo Tháng Năm (MMYYYY)
                    return string.Format("{0}{1}", tranMonth.ToString("00"), tranYear.ToString());
                default:
                    return "";
            }
        }
    }
}
