using Dapper;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.GateWay.Models.MVan;
using InvoiceServer.GateWay.Services.GateWayLog;
using InvoiceServer.GateWay.Services.MinVoice;
using InvoiceServer.GateWay.Services.NotificationMinvoice;
using InvoiceServer.GateWay.Services.ServiceFactorys;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace InvoiceServer.GateWay.Services.MVan
{
    public class MVanService : IMVanService
    {
        private static readonly Logger logger = new Logger();
        protected readonly IDbContext _context;
        protected readonly IDbSet<GATEWAY_LOG> dbSet_gateWayLog;
        protected readonly IDbSet<MINVOICE_DATA> dbSet_minVoice;
        protected readonly IDbSet<INVOICESYSTEMSETTING> dbSet_invoiceSys;
        protected readonly IDbSet<NOTIFICATIONMINVOICE> dbSet_notificationMinvoice;
        protected readonly IGateWayLogService _gateWayLogService;
        protected readonly IMinvoiceService _minvoiceService;
        protected readonly INotificationMinvoiceService _notificationMinvoiceService;
        private static readonly object lockObject = new object();

        public MVanService(IDbContext context)
        {
            _context = context;
            var serviceFactory = GetServiceFactory(_context);
            dbSet_gateWayLog = context.Set<GATEWAY_LOG>();
            dbSet_minVoice = context.Set<MINVOICE_DATA>();
            dbSet_invoiceSys = context.Set<INVOICESYSTEMSETTING>();
            dbSet_notificationMinvoice = context.Set<NOTIFICATIONMINVOICE>();
            _gateWayLogService = serviceFactory.GetService<IGateWayLogService>(_context);
            _minvoiceService = serviceFactory.GetService<IMinvoiceService>(_context);
            _notificationMinvoiceService = serviceFactory.GetService<INotificationMinvoiceService>(_context);
        }

        public IServiceFactory GetServiceFactory(IDbContext _context)
        {
            return new ServiceFactory(_context);
        }

        public VanDefaulResult SendInvoiceNotCode(VanDefaulRequest vanDefaulRequest)
        {
            VanDefaulResult result = new VanDefaulResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_INVOICENOTCODE))?.SETTINGVALUE;
                //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                VanLoginRequest loginInfo = GetMVanSignUp();
                var token = VanLogin(loginInfo).Token;
                request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                            " + "\n" +
                            @"""XmlData"": ""##1"",
                            " + "\n" +
                            @"""MstNnt"": ""##2"",
                            " + "\n" +
                            @"""mstTcgp"":""##3""
                            " + "\n" +
                            @"}
                            " + "\n" +
                            @"";

                body = body.Replace("##1", vanDefaulRequest.XmlData);
                body = body.Replace("##2", vanDefaulRequest.MstNnt);
                body = body.Replace("##3", mstTcgp);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                    if(result.Code == "00")
                    {
                        UpdateInvoiceMessageCode(vanDefaulRequest.InvoiceId, result.Data.MaThongdiep);
                        //minvoice data
                        //InsertMinvoiceRecieved(result, true, Common.Constants.GateWayType.SendInvoiceNotCode);
                    }
                }
                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.SendInvoiceNotCode,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {

                logger.Error("SendInvoiceNotCode", ex);
            }

            return result;
        }
        public VanDefaulResult SendInvoiceWithCode(VanDefaulRequest vanDefaulRequest)
        {
            VanDefaulResult result = new VanDefaulResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_INVOICEWITHCODE))?.SETTINGVALUE;
                //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                VanLoginRequest loginInfo = GetMVanSignUp();
                var token = VanLogin(loginInfo).Token;
                request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                            " + "\n" +
                            @"""XmlData"": ""##1"",
                            " + "\n" +
                            @"""MstNnt"": ""##2"",
                            " + "\n" +
                            @"""mstTcgp"":""##3""
                            " + "\n" +
                            @"}
                            " + "\n" +
                            @"";

                body = body.Replace("##1", vanDefaulRequest.XmlData);
                body = body.Replace("##2", vanDefaulRequest.MstNnt);
                body = body.Replace("##3", mstTcgp);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                    if (result.Code == "00")
                    {
                        UpdateInvoiceMessageCode(vanDefaulRequest.InvoiceId, result.Data.MaThongdiep);

                        //minvoice data
                        //InsertMinvoiceRecieved(result,true, Common.Constants.GateWayType.SendInvoiceWithCode);
                    }
                }
                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.SendInvoiceWithCode,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {
                logger.Error("SendInvoiceWithCode", ex);
            }

            return result;
        }

        public VanDefaulResult VanCancel(VanDefaulRequest vanDefaulRequest)
        {
            VanDefaulResult result = new VanDefaulResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_CANCEL))?.SETTINGVALUE;
                //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                VanLoginRequest loginInfo = GetMVanSignUp();
                var token = VanLogin(loginInfo).Token;
                request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                            " + "\n" +
                            @"""XmlData"": ""##1"",
                            " + "\n" +
                            @"""MstNnt"": ""##2"",
                            " + "\n" +
                            @"""mstTcgp"":""##3""
                            " + "\n" +
                            @"}
                            " + "\n" +
                            @"";

                body = body.Replace("##1", vanDefaulRequest.XmlData);
                body = body.Replace("##2", vanDefaulRequest.MstNnt);
                body = body.Replace("##3", mstTcgp);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                    if (result.Code == "00")
                    {
                        UpdateNotification(vanDefaulRequest, result, 0);
                        result.Message = "Gửi thành công, chờ phản hồi từ CQT";
                    }
                }

                //notification

                // gateway log
                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanCancel 
                                                             ? Common.Constants.GateWayLogName.VanCancel 
                                                             : vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanDelete 
                                                             ? Common.Constants.GateWayLogName.VanDelete
                                                             : Common.Constants.GateWayLogName.VanAdjustment,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {
                logger.Error("VanCancel", ex);
            }

            return result;
        }

        private void UpdateNotification(VanDefaulRequest vanDefaulRequest, VanDefaulResult result, int? status)
        {
            var notiExisted = vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanCancel 
                                                                ? dbSet_notificationMinvoice.FirstOrDefault(x => x.INVOICEID == vanDefaulRequest.InvoiceId && x.NAME.Equals(Common.Constants.GateWayLogName.VanCancel))
                                                                : vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanDelete
                                                                ? dbSet_notificationMinvoice.FirstOrDefault(x => x.INVOICEID == vanDefaulRequest.InvoiceId && x.NAME.Equals(Common.Constants.GateWayLogName.VanDelete))
                                                                : dbSet_notificationMinvoice.FirstOrDefault(x => x.INVOICEID == vanDefaulRequest.InvoiceId && x.NAME.Equals(Common.Constants.GateWayLogName.VanAdjustment));
            if (notiExisted.IsNullOrEmpty())
            {
                NOTIFICATIONMINVOICE noti = new NOTIFICATIONMINVOICE()
                {
                    NAME = vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanCancel 
                                                             ? Common.Constants.GateWayLogName.VanCancel 
                                                             : vanDefaulRequest.InvoiceNotiType == Common.Constants.InvoiceNotiType.VanDelete 
                                                             ? Common.Constants.GateWayLogName.VanDelete
                                                             : Common.Constants.GateWayLogName.VanAdjustment,
                    INVOICEID = vanDefaulRequest.InvoiceId,
                    URL = vanDefaulRequest.XmlData,
                    STATUS = status,
                    MESSAGECODE = result.Data.MaThongdiep,
                    CREATEDDATE = DateTime.Now
                };
                _notificationMinvoiceService.Insert(noti);
            }
            else
            {
                notiExisted.STATUS = status;
                notiExisted.URL = vanDefaulRequest.XmlData;
                notiExisted.MESSAGECODE = result.Data.MaThongdiep;
                notiExisted.CREATEDDATE = DateTime.Now;
                _notificationMinvoiceService.Update(notiExisted);
            }
        }

        public VanResponseResult InsertMinvoiceRecieved(VanDefaulResult result, bool sleep, int type = 0)
        {
            VanResponseResult finalResult = new VanResponseResult();
            try
            {
                if (sleep) System.Threading.Thread.Sleep(4000);
                VanResponseRequest vanResponse = new VanResponseRequest()
                {
                    Mtdtchieu = result.Data?.MaThongdiep,
                    Skip = 0,
                    Take = 10
                };
                var vanResponResult = MVanReceived(vanResponse);
                if (!vanResponResult.Data.IsNullOrEmpty())
                {
                    foreach (var item in vanResponResult.Data.Where(x => x.Mltdiep != "999"))
                    {
                        MVanResponse res = new MVanResponse()
                        {
                            Xml = item.Xml.IsNullOrWhitespace() ? null : Encoding.UTF8.GetString(Convert.FromBase64String(item.Xml)),
                            Mst = item.Mst,
                            MstNnt = item.Msttcgp
                        };
                        switch (type)
                        {
                            case Common.Constants.GateWayType.SendInvoiceWithCode:
                                MVanResponseForInvoiceWithCode(res);
                                break;
                            case Common.Constants.GateWayType.VanRegister:
                                MVanResponseForRegister(res);
                                break;
                            case Common.Constants.GateWayType.VanCancel:
                                MVanResponseForCancel(res);
                                break;
                            case Common.Constants.GateWayType.VanDelete:
                                MVanResponseForDelete(res);
                                break;
                            case Common.Constants.GateWayType.VanAdjustment:
                                MVanResponseForAdjustment(res);
                                break;
                            case Common.Constants.GateWayType.SendInvoiceNotCode:
                                MVanResponseForInvoiceNotCode(res);
                                break;
                            case Common.Constants.GateWayType.VanSynthesis:
                                MVanResponseForSynthesis(res);
                                break;
                            default:
                                MVanResponse(res);
                                break;
                        }
                    }
                }
                else
                {
                    InsertMinvoiceRecievedByMtd(result, type);
                }
            }
            catch (Exception ex)
            {
                logger.Error("InsertMinvoiceRecieved", ex);
            }
            finalResult.Code = "00";
            return finalResult;
        }

        private void InsertMinvoiceRecievedByMtd(VanDefaulResult result, int type = 0)
        {
            VanResponseRequest vanResponse = new VanResponseRequest()
            {
                Mtdiep = result.Data?.MaThongdiep,
                Skip = 0,
                Take = 10
            };
            var vanResponResult = MVanReceived(vanResponse);
            if (!vanResponResult.Data.IsNullOrEmpty())
            {
                foreach (var item in vanResponResult.Data.Where(x => x.Type == "OUT"))
                {
                    MVanResponse res = new MVanResponse()
                    {
                        Xml = item.Xml.IsNullOrWhitespace() ? null : Encoding.UTF8.GetString(Convert.FromBase64String(item.Xml)),
                        Mst = item.Mst,
                        MstNnt = item.Msttcgp
                    };
                    switch (type)
                    {
                        case Common.Constants.GateWayType.SendInvoiceWithCode:
                            MVanResponseForInvoiceWithCode(res);
                            break;
                        case Common.Constants.GateWayType.VanRegister:
                            MVanResponseForRegister(res);
                            break;
                        case Common.Constants.GateWayType.VanCancel:
                            MVanResponseForCancel(res);
                            break;
                        case Common.Constants.GateWayType.VanDelete:
                            MVanResponseForDelete(res);
                            break;
                        case Common.Constants.GateWayType.VanAdjustment:
                            MVanResponseForAdjustment(res);
                            break;
                        case Common.Constants.GateWayType.SendInvoiceNotCode:
                            MVanResponseForInvoiceNotCode(res);
                            break;
                        case Common.Constants.GateWayType.VanSynthesis:
                            MVanResponseForSynthesis(res);
                            break;
                        default:
                            MVanResponse(res);
                            break;
                    }
                }
            }
        }

        public VanLoginResult VanLogin(VanLoginRequest vanLoginRequest)
        {
            VanLoginResult result = new VanLoginResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_LOGIN))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                        " + "\n" +
                                        @"    ""username"":""##1"",
                        " + "\n" +
                                        @"    ""password"": ""##2"",
                        " + "\n" +
                                        @"    ""ma_dvcs"":""##3""
                        " + "\n" +
                                        @"}
                        " + "\n" +
                                        @"";
                body = body.Replace("##1", vanLoginRequest.Username);
                body = body.Replace("##2", vanLoginRequest.Password);
                body = body.Replace("##3", vanLoginRequest.Ma_dvcs);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanLoginResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                }
                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanLogin,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);
            }
            catch (Exception ex)
            {
                logger.Error("VanLogin", ex);
            }
            return result;
        }

        public VanDefaulResult VanRegister(VanDefaulRequest vanDefaulRequest)
        {
            VanDefaulResult result = new VanDefaulResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_REGISTER))?.SETTINGVALUE;
                //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                VanLoginRequest loginInfo = GetMVanSignUp();
                var token = VanLogin(loginInfo).Token;
                request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                            " + "\n" +
                            @"""XmlData"": ""##1"",
                            " + "\n" +
                            @"""MstNnt"": ""##2"",
                            " + "\n" +
                            @"""mstTcgp"":""##3""
                            " + "\n" +
                            @"}
                            " + "\n" +
                            @"";

                body = body.Replace("##1", vanDefaulRequest.XmlData);
                body = body.Replace("##2", vanDefaulRequest.MstNnt);
                body = body.Replace("##3", mstTcgp);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                    if (result.Code == "00")
                    {
                        UpdateDeclarationMessageCode(vanDefaulRequest.DeclarationId, result.Data.MaThongdiep);

                        //minvoice data
                        //InsertMinvoiceRecieved(result, true, Common.Constants.GateWayType.VanRegister);
                    }
                }

                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanRegister,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {
                logger.Error("VanRegister", ex);
            }

            return result;
        }

        public string MVanResponse(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var ltBao = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<LTBao>") && mVanResponse.Xml.Contains("</LTBao>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<LTBao>") + 7, mVanResponse.Xml.IndexOf("</LTBao>") - mVanResponse.Xml.IndexOf("<LTBao>") - 7) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>")) 
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7) 
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.LTBAO = ltBao;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            LTBAO = ltBao,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanReceivedata(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                XDocument xdoc = mVanResponse.Xml.IsNullOrWhitespace() ? new XDocument() : XDocument.Parse(mVanResponse.Xml);
                var listBTHFailed = xdoc.Descendants().Where(x => x.Name.LocalName == "BTHop").ToList();

                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var ltBao = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<LTBao>") && mVanResponse.Xml.Contains("</LTBao>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<LTBao>") + 7, mVanResponse.Xml.IndexOf("</LTBao>") - mVanResponse.Xml.IndexOf("<LTBao>") - 7) : null;
                var ntBao = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<NTBao>") && mVanResponse.Xml.Contains("</NTBao>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<NTBao>") + 7, mVanResponse.Xml.IndexOf("</NTBao>") - mVanResponse.Xml.IndexOf("<NTBao>") - 7) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var ttxncqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<TTXNCQT>") && mVanResponse.Xml.Contains("</TTXNCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<TTXNCQT>") + 9, mVanResponse.Xml.IndexOf("</TTXNCQT>") - mVanResponse.Xml.IndexOf("<TTXNCQT>") - 9) : null;
                var thop = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<THop>") && mVanResponse.Xml.Contains("</THop>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<THop>") + 6, mVanResponse.Xml.IndexOf("</THop>") - mVanResponse.Xml.IndexOf("<THop>") - 6) : null;

                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));

                var errorcode = xdoc.Descendants().Where(x => x.Name.LocalName == "MLoi").FirstOrDefault()?.ToString(); //Mã lỗi 
                var error = xdoc.Descendants().Where(x => x.Name.LocalName == "MTLoi").FirstOrDefault()?.ToString(); //Mô tả lỗi 

                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    var messageCode = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                    UpdateForDeclaration(mltdiep, mlloi, messageCode, mVanResponse);
                    if (ltBao == "4")
                    {
                        logger.Error("messagecode" + messageCode, new Exception(""));
                        logger.Error("errorcode : " + errorcode, new Exception(""));
                        logger.Error("error" + error, new Exception(""));
                        try
                        {
                            UpdateForErrorBTH(messageCode, errorcode, error);
                        }
                        catch(Exception ex)
                        {
                            logger.Error("Error" + ex.ToString(), new Exception(ex.Message));
                        }
                        
                    }
                    else
                    {
                        UpdateForSynthesis(listBTHFailed);
                    }
                    UpdateForInvoiceNotification(mlloi, messageCode);

                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = messageCode;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.TTXNCQT = ttxncqt;
                        minvoiceData.THOP = thop;
                        minvoiceData.LTBAO = ltBao;
                        minvoiceData.APPROVEDDATE = ntBao.IsNullOrWhitespace() ? DateTime.Now : DateTime.Parse(ntBao);
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = messageCode,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            TTXNCQT = ttxncqt,
                            THOP = thop,
                            LTBAO = ltBao,
                            APPROVEDDATE = ntBao.IsNullOrWhitespace() ? DateTime.Now : DateTime.Parse(ntBao),
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        private void UpdateForSynthesis(List<XElement> listBTHFailed)
        {
            var BTHFailedInvoices = new List<MVanSynthesisResult>();
            foreach (var bth in listBTHFailed)
            {
                var KHHDon = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "KHHDon")?.Value;
                var KHMSHDon = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "KHMSHDon")?.Value;
                var SHDon = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "SHDon")?.Value;
                var error = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "MTLoi")?.Value;
                var errorCode = bth.Descendants().FirstOrDefault(x => x.Name.LocalName == "MLoi")?.Value;

                BTHFailedInvoices.Add(new MVanSynthesisResult()
                {
                    Error = error,
                    KHHDon = KHHDon,
                    KHMSHDon = KHMSHDon,
                    SHDon = SHDon,
                    ErrorCode = errorCode
                });
            };
            UpdateInvoicesSynthesisStatus(BTHFailedInvoices);
        }

        public void UpdateForErrorBTH(string messagecode,string errorcode , string error)
        {
            logger.Error("messagecode for UpdateForErrorBTH : " + messagecode, new Exception(""));
            logger.Error("errorcode  for UpdateForErrorBTH  : " + errorcode, new Exception(""));
            logger.Error("error  for UpdateForErrorBTH : " + error, new Exception(""));
            string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
            string sqlResult = @"UPDATE INVOICE SET BTHERROR = '{error}', BTHERRORSTATUS = {errorcode} WHERE MESSAGECODE = '{messagecode}'";
            //sqlResult = sqlResult.Replace("{errorcode}", errorcode).Replace("{error}", error).Replace("{messagecode}", messagecode);
            sqlResult = sqlResult.Replace("{errorcode}", errorcode.Replace("<MLoi>", "").Replace("</MLoi>", "").Trim()).Replace("{error}", error.Replace("<MTLoi>", "").Replace("</MTLoi>", "").Trim()).Replace("{messagecode}", messagecode);
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                var result = connection.Execute(sqlResult);
                connection.Close();
            }
        }

        private void UpdateInvoicesSynthesisStatus(List<MVanSynthesisResult> listInvoiceFaileds)
        {
            List<INVOICE> lstInvoice = new List<INVOICE>();

            foreach (var item in listInvoiceFaileds)
            {
                var invoice = _context.Set<INVOICE>().FirstOrDefault(x => x.SYMBOL == (item.KHMSHDon + item.KHHDon) && x.NO == item.SHDon);
                if (!invoice.IsNullOrEmpty())
                {
                    invoice.BTHERRORSTATUS = decimal.Parse(item.ErrorCode);
                    invoice.BTHERROR = item.Error;
                    //invoice.MESSAGECODE = null;
                    //lstInvoice.Add(invoice);
                    Update(invoice);
                }
            }
            //if (lstInvoice.IsNotNullOrEmpty())
            //    Update(lstInvoice[0]);
        }
        private void UpdateForDeclaration(string mltdiep, string mlloi, string messageCode, MVanResponse mVanResponse)
        {
            var declaration = _context.Set<INVOICEDECLARATION>().FirstOrDefault(x => x.MESSAGECODE == messageCode);
            if (declaration.IsNullOrEmpty()) return;

            if (!mlloi.IsNullOrWhitespace())
            {
                UpdateDeclarationStatus(declaration, DeclarationStatus.New);
                mVanResponse.CompanyId = (long)declaration.COMPANYID;
                DeleteDeclarationXmlFile(declaration.ID, mVanResponse);
            }
            if (mltdiep == MinvoiceStatus.DeclarationApproved && mlloi.IsNullOrWhitespace())
            {
                UpdateDeclarationStatus(declaration, DeclarationStatus.Completed);
            }
        }

        private void UpdateForInvoice(string mlloi, string messageCode, MVanResponse mVanResponse)
        {
            var invoice = _context.Set<INVOICE>().FirstOrDefault(x => x.MESSAGECODE == messageCode);
            if (invoice.IsNullOrEmpty()) return;

            if (!mlloi.IsNullOrWhitespace())
            {
                UpdateInvoiceStatus(invoice, (int)InvoiceStatus.New);
                mVanResponse.CompanyId = invoice.COMPANYID;
                DeleteInvoiceXmlName(invoice, mVanResponse);
            }
        }

        private void UpdateForInvoiceNotification(string mlloi, string messageCode)
        {
            var invoiceNoti = _context.Set<NOTIFICATIONMINVOICE>().FirstOrDefault(x => x.MESSAGECODE == messageCode);
            if (invoiceNoti.IsNullOrEmpty()) return;

            if (!mlloi.IsNullOrWhitespace())
            {
                UpdateInvoiceNotificationStatus(invoiceNoti, (int)InvoiceNotificationStatus.Successfull);
            }
        }

        public void DeleteInvoiceXmlName(INVOICE invoice, MVanResponse mVanResponse)
        {
            string folderTree = DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MM") + "\\" + DateTime.Now.ToString("dd");
            if (invoice.RELEASEDDATE != null)
            {
                folderTree = invoice.RELEASEDDATE.Value.Year.ToString() + "\\" + invoice.RELEASEDDATE.Value.ToString("MM") + "\\" + invoice.RELEASEDDATE.Value.ToString("dd");
            }

            string fileNameSymbol = invoice.ID.ToString();

            string pathFile = Path.Combine(mVanResponse.FolderStore, mVanResponse.CompanyId.ToString(), AssetSignInvoice.Release, AssetSignInvoice.SignFile, folderTree);

            string pathXml = Path.Combine(pathFile, fileNameSymbol + "_sign" + ".xml");

            if (File.Exists(pathXml))
            {
                File.Delete(pathXml);
            }
        }

        public void DeleteDeclarationXmlFile(long id, MVanResponse mVanResponse)
        {
            string fileName = string.Format("Declaration_{0}.xml", id);
            string fileSign = Path.Combine(mVanResponse.FolderStore, mVanResponse.CompanyId.ToString(), "Declaration", "Sign", fileName);
            string fileGenerate = Path.Combine(mVanResponse.FolderStore, mVanResponse.CompanyId.ToString(), "Declaration", fileName);

            if (File.Exists(fileSign))
            {
                File.Delete(fileSign);
            }
            if (File.Exists(fileGenerate))
            {
                File.Delete(fileGenerate);
            }

        }

        public string MVanResponseForInvoiceNotCode(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {

                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForSynthesis(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {

                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForCancel(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForDelete(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForAdjustment(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForRegister(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : "Đăng kí sử dụng hóa đơn thành công";
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace()
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.Equals("Đăng kí sử dụng hóa đơn thành công") ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.Equals("Đăng kí sử dụng hóa đơn thành công") ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public string MVanResponseForInvoiceWithCode(MVanResponse mVanResponse)
        {
            string result = string.Empty;
            try
            {
                var mltdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MLTDiep>") && mVanResponse.Xml.Contains("</MLTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MLTDiep>") + 9, mVanResponse.Xml.IndexOf("</MLTDiep>") - mVanResponse.Xml.IndexOf("<MLTDiep>") - 9) : null;
                if (!mltdiep.IsNullOrWhitespace() && mltdiep == "999") return result;
                var mltchieu = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("MTDTChieu") && mVanResponse.Xml.Contains("</MTDTChieu>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDTChieu>") + 11, mVanResponse.Xml.IndexOf("</MTDTChieu>") - mVanResponse.Xml.IndexOf("<MTDTChieu>") - 11) : null;
                var mtdiep = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTDiep>") && mVanResponse.Xml.Contains("</MTDiep>")) ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTDiep>") + 8, mVanResponse.Xml.IndexOf("</MTDiep>") - mVanResponse.Xml.IndexOf("<MTDiep>") - 8) : null;
                var mlloi = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTLoi>") && mVanResponse.Xml.Contains("</MTLoi>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTLoi>") + 7, mVanResponse.Xml.IndexOf("</MTLoi>") - mVanResponse.Xml.IndexOf("<MTLoi>") - 7)
                    : (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MTa>") && mVanResponse.Xml.Contains("</MTa>"))
                    ? mVanResponse.Xml.Substring(mVanResponse.Xml.IndexOf("<MTa>") + 5, mVanResponse.Xml.IndexOf("</MTa>") - mVanResponse.Xml.IndexOf("<MTa>") - 5)
                    : null;
                var mccqt = (mVanResponse.Xml.IsNotNullOrEmpty() && mVanResponse.Xml.Contains("<MCCQT") && mVanResponse.Xml.Contains("</MCCQT>")) ? mVanResponse.Xml?.Substring(mVanResponse.Xml.IndexOf("<MCCQT") + 48, mVanResponse.Xml.IndexOf("</MCCQT>") - mVanResponse.Xml.IndexOf("<MCCQT") - 48) : null;
                var minvoiceData = mltchieu.IsNullOrWhitespace() 
                                   ? dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mtdiep))
                                   : dbSet_minVoice.FirstOrDefault(x => x.MESSAGECODE.Equals(mltchieu));
                if ((!mtdiep.IsNullOrWhitespace() || !mltchieu.IsNullOrWhitespace()))
                {
                    if (minvoiceData != null)
                    {
                        minvoiceData.MST = mVanResponse.Mst;
                        minvoiceData.MSTNT = mVanResponse.MstNnt;
                        minvoiceData.XML = mVanResponse.Xml;
                        minvoiceData.MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu;
                        minvoiceData.STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0;
                        minvoiceData.MLTDIEP = mltdiep;
                        minvoiceData.ERROR = mlloi;
                        minvoiceData.MCQT = mccqt;
                        minvoiceData.CREATEDDATE = DateTime.Now;
                        _minvoiceService.Update(minvoiceData);
                    }
                    else
                    {
                        MINVOICE_DATA data = new MINVOICE_DATA()
                        {
                            MST = mVanResponse.Mst,
                            MSTNT = mVanResponse.MstNnt,
                            XML = mVanResponse.Xml,
                            MESSAGECODE = mltchieu.IsNullOrWhitespace() ? mtdiep : mltchieu,
                            STATUS = mlloi.IsNullOrWhitespace() ? 1 : 0,
                            MLTDIEP = mltdiep,
                            ERROR = mlloi,
                            MCQT = mccqt,
                            CREATEDDATE = DateTime.Now
                        };
                        _minvoiceService.Insert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("MVanResponse", ex);
            }

            return result;
        }

        public VanSignUpResult VanSignUp(VanSignUpRequest vanSignUpRequest)
        {
            VanSignUpResult result = new VanSignUpResult();
            try
            {
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SIGNUP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                        " + "\n" +
                                        @"    ""username"":""##1"",
                        " + "\n" +
                                        @"    ""password"": ""##2"",
                        " + "\n" +
                                        @"    ""email"":""##3"",
                        " + "\n" +
                                        @"    ""sdt"":""##4"",
                        " + "\n" +
                                        @"    ""ma_dvcs"":""##5"",
                        " + "\n" +
                                        @"    ""ten_nguoi_sd"":""##6"",
                        " + "\n" +
                                        @"    ""dien_giai"":""##7""
                        " + "\n" +
                                        @"}
                        " + "\n" +
                                        @"";

                body = body.Replace("##1", vanSignUpRequest.Username);
                body = body.Replace("##2", vanSignUpRequest.Password);
                body = body.Replace("##3", vanSignUpRequest.Email);
                body = body.Replace("##4", vanSignUpRequest.Sdt);
                body = body.Replace("##5", vanSignUpRequest.Ma_dvcs);
                body = body.Replace("##6", vanSignUpRequest.Ten_nguoi_sd);
                body = body.Replace("##7", vanSignUpRequest.Dien_giai);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanSignUpResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;

                    if (!result.Data.IsNullOrEmpty()
                    && result.Data.Username.IsNotNullOrEmpty()
                    && result.Data.Password.IsNotNullOrEmpty()
                    && result.Data.Ma_dvcs.IsNotNullOrEmpty())
                    {
                        var loginInfo = JsonConvert.SerializeObject(vanSignUpRequest);
                        GATEWAY_LOG log = new GATEWAY_LOG()
                        {
                            IP = client.BaseUrl.AbsoluteUri,
                            CREATEDDATE = DateTime.Now,
                            NAME = Common.Constants.GateWayLogName.VanSignUpAC,
                            OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                            BODY = loginInfo
                        };
                        _gateWayLogService.Insert(log);
                        UpdateMVanSignUp(vanSignUpRequest);
                    }
                }

                GATEWAY_LOG logb = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanSignUpAC,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(logb);
                
            }
            catch (Exception ex)
            {
                logger.Error("VanSignUp", ex);
            }

            return result;
        }

        //public VanDefaulResult VanSynthesis(VanDefaulRequest vanDefaulRequest)
        //{
        //    VanDefaulResult result = new VanDefaulResult();
        //    try
        //    {
        //        string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
        //        string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SYNTHESIS))?.SETTINGVALUE;
        //        //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
        //        string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
        //        var client = new RestSharp.RestClient(mVanUrl + mVanApi);
        //        var request = new RestRequest(Method.POST);
        //        VanLoginRequest loginInfo = GetMVanSignUp();
        //        var token = VanLogin(loginInfo).Token;
        //        request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
        //        request.AddHeader("accept", "*/*");
        //        request.AddHeader("Content-Type", "application/json");
        //        var body = @"{
        //                    " + "\n" +
        //                    @"""XmlData"": ""##1"",
        //                    " + "\n" +
        //                    @"""MstNnt"": ""##2"",
        //                    " + "\n" +
        //                    @"""mstTcgp"":""##3""
        //                    " + "\n" +
        //                    @"}
        //                    " + "\n" +
        //                    @"";

        //        body = body.Replace("##1", vanDefaulRequest.XmlData);
        //        body = body.Replace("##2", vanDefaulRequest.MstNnt);
        //        body = body.Replace("##3", mstTcgp);
        //        request.AddParameter("application/json", body, ParameterType.RequestBody);
        //        IRestResponse response = client.Execute(request);
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
        //            Console.WriteLine(response.Content);
        //            result = responseBody;
        //            if (result.Code == "00")
        //            {
        //                UpdateHistoryReportMessageCode(vanDefaulRequest.HistoryReportId, result.Data.MaThongdiep);
        //                UpdateInvoicesMessageCode(result.Data.MaThongdiep, vanDefaulRequest.InvoiceIds);
        //            }
        //        }
        //        GATEWAY_LOG log = new GATEWAY_LOG()
        //        {
        //            IP = client.BaseUrl.AbsoluteUri,
        //            CREATEDDATE = DateTime.Now,
        //            NAME = Common.Constants.GateWayLogName.VanSynthesis,
        //            OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
        //            BODY = response.Content
        //        };
        //        _gateWayLogService.Insert(log);

        //    }
        //    catch (Exception ex)
        //    {

        //        logger.Error("VanSynthesis", ex);
        //    }

        //    return result;
        //}

        public VanDefaulResult VanSynthesis(VanDefaulRequest vanDefaulRequest)
        {
            VanDefaulResult result = new VanDefaulResult();
            try
            {
                logger.Error("start process data send: " + DateTime.Now.ToString("dd/MM/YYYY HH:mm:ss.fff"), new Exception("VanSynthesis"));
                string mVanUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE;
                string mVanApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SYNTHESIS))?.SETTINGVALUE;
                //string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                string mstTcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE;
                var client = new RestSharp.RestClient(mVanUrl + mVanApi);
                var request = new RestRequest(Method.POST);
                logger.Error("start login: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
                VanLoginRequest loginInfo = GetMVanSignUp();
                var token = VanLogin(loginInfo).Token;
                request.AddHeader("Authorization", "Bear " + token + ";" + loginInfo.Ma_dvcs);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
                            " + "\n" +
                            @"""XmlData"": ""##1"",
                            " + "\n" +
                            @"""MstNnt"": ""##2"",
                            " + "\n" +
                            @"""mstTcgp"":""##3""
                            " + "\n" +
                            @"}
                            " + "\n" +
                            @"";

                body = body.Replace("##1", vanDefaulRequest.XmlData);
                body = body.Replace("##2", vanDefaulRequest.MstNnt);
                body = body.Replace("##3", mstTcgp);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                logger.Error("start send request: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
                IRestResponse response = client.Execute(request);
                logger.Error("Send done va nhan respone: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanDefaulResult>(response.Content);
                    //Console.WriteLine(response.Content);
                    result = responseBody;
                    if (result.Code == "00")
                    {
                        UpdateHistoryReportMessageCode(vanDefaulRequest.HistoryReportId, result.Data.MaThongdiep);
                        //UpdateInvoicesMessageCode(result.Data.MaThongdiep, vanDefaulRequest.InvoiceIds);
                        UpdateMessageCode(result.Data.MaThongdiep, vanDefaulRequest.InvoiceIds);//Comment: 20/08/2022
                    }
                }
                logger.Error("Insert gateWayLog: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanSynthesis,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {

                logger.Error("VanSynthesis", ex);
            }
            logger.Error("DONE: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
            return result;
        }

        private void UpdateMessageCode(string messageCode, List<long> listInvoiceIds)
        {
            logger.Error("Start update messageCode: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
            var take = (listInvoiceIds.Count() / 4) + 1;
            var skip = 0;
            for (int i = 1; i <= 4; i++)
            {
                var listProcess = new List<long>(listInvoiceIds.Skip(skip).Take(take));
                if(listProcess.Count > 0)
                {
                    string listId = string.Join(",", listProcess);
                    string connectionString = GetConnectionString.GetByName("DataClassesDataContext");
                    string sqlResult = @"UPDATE INVOICE SET MESSAGECODE = '{messageCode}' WHERE ID IN({ids})";
                    sqlResult = sqlResult.Replace("{messageCode}", messageCode).Replace("{ids}", listId);
                    using (var connection = new OracleConnection(connectionString))
                    {
                        connection.Open();
                        var result = connection.Execute(sqlResult);
                        connection.Close();
                    }
                    skip = take * i;
                }
                
            }

            logger.Error("Done update messageCode: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), new Exception("VanSynthesis"));
        }

        private void UpdateInvoicesMessageCode(string messageCode, List<long> listInvoiceIds)
        {
            List<INVOICE> lstInvoice = new List<INVOICE>();

            foreach (var item in listInvoiceIds)
            {
                var invoice = _context.Set<INVOICE>().Find(item);
                invoice.MESSAGECODE = messageCode;
                lstInvoice.Add(invoice);
            }

            Update(lstInvoice[0]);
        }

        public virtual bool Update(INVOICE entity)
        {
            int result = 0;
            try
            {
                lock (lockObject)
                {
                    result = _context.SaveChanges();
                }
            }
            catch (Exception dbEx)
            {

            }

            return result > 0;
        }

        public VanLoginResult MVanAdminLogin(VanLoginRequest vanLoginRequest)
        {
            VanLoginResult result = new VanLoginResult();
            try
            {
                string mVanAdminUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINURL))?.SETTINGVALUE;
                string mVanAdminApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINLOGIN))?.SETTINGVALUE;
                string restUrl = mVanAdminUrl + mVanAdminApi;
                var client = new RestClient(restUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{""userId"":""##1"",""password"":""##2""}";
                body = body.Replace("##1",vanLoginRequest.UserId);
                body = body.Replace("##2", vanLoginRequest.Password);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanAdminLogin,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);
            }
            catch (Exception ex)
            {

                logger.Error("MVanAdminLogin", ex);
            }

            return result;
        }
        public VanResponseResult MVanReceived(VanResponseRequest vanResponseRequest)
        {
            VanResponseResult result = new VanResponseResult();
            try
            {
                string mVanAdminUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINURL))?.SETTINGVALUE;
                string mVanAdminApi = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SEARCH))?.SETTINGVALUE;
                string mVanAdminUserId = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINUSERID))?.SETTINGVALUE;
                string mVanAdminPassword = Decrypt(dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINPASSWORD))?.SETTINGVALUE, AES.Key, AES.IV);
                string mstNnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE;
                VanLoginRequest login = new VanLoginRequest()
                {
                    UserId = mVanAdminUserId,
                    Password = mVanAdminPassword
                };
                MVanAdminLogin(login);
                string restUrl = mVanAdminUrl + mVanAdminApi;
                restUrl = restUrl.Replace("##1", vanResponseRequest.DateFrom.IsNullOrWhitespace() ? "" : vanResponseRequest.DateFrom.ToString());
                restUrl = restUrl.Replace("##2", vanResponseRequest.Mltdiep.IsNullOrWhitespace() ? "" : vanResponseRequest.Mtdiep);
                restUrl = restUrl.Replace("##3", mstNnt.IsNullOrWhitespace() ? "" : mstNnt);
                restUrl = restUrl.Replace("##4", vanResponseRequest.Thop.IsNullOrWhitespace() ? "" : vanResponseRequest.Thop);
                restUrl = restUrl.Replace("##5", vanResponseRequest.DateTo.IsNullOrWhitespace() ? "" : vanResponseRequest.DateTo.ToString());
                restUrl = restUrl.Replace("##6", vanResponseRequest.Mtdiep.IsNullOrWhitespace() ? "" : vanResponseRequest.Mtdiep);
                restUrl = restUrl.Replace("##7", vanResponseRequest.Mngui.IsNullOrWhitespace() ? "" : vanResponseRequest.Mngui);
                restUrl = restUrl.Replace("##8", vanResponseRequest.Ttxncqt.IsNullOrWhitespace() ? "" : vanResponseRequest.Ttxncqt);
                restUrl = restUrl.Replace("##9", vanResponseRequest.Type.IsNullOrWhitespace() ? "" : vanResponseRequest.Type);
                restUrl = restUrl.Replace("$$1", vanResponseRequest.Mtdtchieu.IsNullOrWhitespace() ? "" : vanResponseRequest.Mtdtchieu);
                restUrl = restUrl.Replace("$$2", vanResponseRequest.Mnnhan.IsNullOrWhitespace() ? "" : vanResponseRequest.Mnnhan);
                restUrl = restUrl.Replace("$$3", vanResponseRequest.Ltbao.IsNullOrWhitespace() ? "" : vanResponseRequest.Ltbao);
                restUrl = restUrl.Replace("$$4", vanResponseRequest.Skip.ToString());
                restUrl = restUrl.Replace("$$5", vanResponseRequest.Take.ToString());
                var client = new RestClient(restUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("X-Authorization-Token", "uat");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = JsonConvert.DeserializeObject<VanResponseResult>(response.Content);
                    Console.WriteLine(response.Content);
                    result = responseBody;
                }

                GATEWAY_LOG log = new GATEWAY_LOG()
                {
                    IP = client.BaseUrl.AbsoluteUri,
                    CREATEDDATE = DateTime.Now,
                    NAME = Common.Constants.GateWayLogName.VanReceived,
                    OBJECTNAME = Common.Constants.GateWayLogName.ObjDefaultName,
                    BODY = response.Content
                };
                _gateWayLogService.Insert(log);

            }
            catch (Exception ex)
            {

                logger.Error("MVanReceived", ex);
            }

            return result;
        }

        public void UpdateInvoiceMessageCode(long invoiceId, string messageCode)
        {
            var invoice = _context.Set<INVOICE>().Find(invoiceId);
            if (!invoice.IsNullOrEmpty())
            {
                invoice.MESSAGECODE = messageCode;
                _context.Set<INVOICE>().Attach(invoice);
                _context.Entry(invoice).State = EntityState.Unchanged;
                _context.Entry(invoice).Property(x => x.MESSAGECODE).IsModified = true;
                _context.SaveChanges();
            }
        }

        public void UpdateHistoryReportMessageCode(long HistoryReportId, string messageCode)
        {
            var history = _context.Set<HISTORYREPORTGENERAL>().Find(HistoryReportId);
            if (!history.IsNullOrEmpty())
            {
                history.MESSAGECODE = messageCode;
                _context.Set<HISTORYREPORTGENERAL>().Attach(history);
                _context.Entry(history).State = EntityState.Unchanged;
                _context.Entry(history).Property(x => x.MESSAGECODE).IsModified = true;
                _context.SaveChanges();
            }
        }

        public void UpdateDeclarationMessageCode(long DeclarationId, string messageCode)
        {
            var declaration = _context.Set<INVOICEDECLARATION>().Find(DeclarationId);
            if (!declaration.IsNullOrEmpty())
            {
                declaration.MESSAGECODE = messageCode;
                _context.Set<INVOICEDECLARATION>().Attach(declaration);
                _context.Entry(declaration).State = EntityState.Unchanged;
                _context.Entry(declaration).Property(x => x.MESSAGECODE).IsModified = true;
                _context.SaveChanges();
            }
        }

        public void UpdateDeclarationStatus(INVOICEDECLARATION declaration, int status)
        {
            if (!declaration.IsNullOrEmpty())
            {
                declaration.STATUS = status;
                _context.Set<INVOICEDECLARATION>().Attach(declaration);
                _context.Entry(declaration).State = EntityState.Unchanged;
                _context.Entry(declaration).Property(x => x.STATUS).IsModified = true;
                _context.SaveChanges();
            }
        }

        public void UpdateInvoiceStatus(INVOICE invoice, int status)
        {
            if (!invoice.IsNullOrEmpty())
            {
                invoice.INVOICESTATUS = status;
                _context.Set<INVOICE>().Attach(invoice);
                _context.Entry(invoice).State = EntityState.Unchanged;
                _context.Entry(invoice).Property(x => x.INVOICESTATUS).IsModified = true;
                _context.SaveChanges();
            }
        }
        public void UpdateInvoiceNotificationStatus(NOTIFICATIONMINVOICE invoiceNoti, int status)
        {
            if (!invoiceNoti.IsNullOrEmpty())
            {
                invoiceNoti.STATUS = status;
                _context.Set<NOTIFICATIONMINVOICE>().Attach(invoiceNoti);
                _context.Entry(invoiceNoti).State = EntityState.Unchanged;
                _context.Entry(invoiceNoti).Property(x => x.STATUS).IsModified = true;
                _context.SaveChanges();
            }
        }

        public void UpdateInvoiceSystemsetting(string type, string key, string value)
        {
            var setting = _context.Set<INVOICESYSTEMSETTING>().FirstOrDefault(x => x.SETTINGTYPE.Equals(type) && x.SETTINGKEY.Equals(key));
            if (!setting.IsNullOrEmpty())
            {
                setting.SETTINGVALUE = value;
                _context.Set<INVOICESYSTEMSETTING>().Attach(setting);
                _context.Entry(setting).State = EntityState.Unchanged;
                _context.Entry(setting).Property(x => x.SETTINGVALUE).IsModified = true;
                _context.SaveChanges();
            }
        }
        public VanLoginRequest UpdateMVanSignUp(VanSignUpRequest info)
        {
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_USERNAME, info.Username);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_PASSWORD, EncryptAesManaged(info.Password));
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_MSTNNT, info.Ma_dvcs);
            return GetMVanSignUp();
        }
        public VanLoginRequest GetMVanSignUp()
        {
            return new VanLoginRequest()
            {
                Username = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_USERNAME))?.SETTINGVALUE,
                Password = Decrypt(dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_PASSWORD))?.SETTINGVALUE, AES.Key, AES.IV),
                Ma_dvcs = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MDVCS))?.SETTINGVALUE,
            };
        }

        static string EncryptAesManaged(string raw)
        {
            try
            {
                if (raw.IsNullOrWhitespace())
                    return raw;
                string encryptedText = null;
                using (AesManaged aes = new AesManaged())
                {
                    byte[] encrypted = Encrypt(raw, AES.Key, AES.IV);
                    encryptedText = Convert.ToBase64String(encrypted);
                    return encryptedText;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return null;
        }

        static byte[] Encrypt(string plainText, string Key, string IV)
        {
            byte[] encrypted;
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.ASCII.GetBytes(Key), Encoding.ASCII.GetBytes(IV));
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            return encrypted;
        }

        static string Decrypt(string base64cipherText, string Key, string IV)
        {
            string plaintext = null;

            if (base64cipherText.IsNullOrWhitespace())
            {
                return plaintext;
            }
            else
            {
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.ASCII.GetBytes(Key), Encoding.ASCII.GetBytes(IV));
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64cipherText)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                plaintext = reader.ReadToEnd();
                            }

                        }
                    }
                }
            }
            
            
            return plaintext;
        }

        public InvoiceSystemSettingInfo GetInvoiceSystemSettingDetail()
        {

            return new InvoiceSystemSettingInfo()
            {
                Url = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_URL))?.SETTINGVALUE,
                InvoiceWithCode = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_INVOICEWITHCODE))?.SETTINGVALUE,
                InvoiceNotCode = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_INVOICENOTCODE))?.SETTINGVALUE,
                Username = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_USERNAME))?.SETTINGVALUE,
                Password = Decrypt(dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_PASSWORD))?.SETTINGVALUE, AES.Key, AES.IV),
                Cancel = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_CANCEL))?.SETTINGVALUE,
                Login = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_LOGIN))?.SETTINGVALUE,
                Register = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_REGISTER))?.SETTINGVALUE,
                SignUp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SIGNUP))?.SETTINGVALUE,
                Synthesis = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SYNTHESIS))?.SETTINGVALUE,
                Mstnnt = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTNNT))?.SETTINGVALUE,
                Mdvcs = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MDVCS))?.SETTINGVALUE,
                Mstcgp = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_MSTTCGP))?.SETTINGVALUE,
                AdminUrl = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINURL))?.SETTINGVALUE,
                Search = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_SEARCH))?.SETTINGVALUE,
                AdminUserId = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINUSERID))?.SETTINGVALUE,
                AdminPassword = Decrypt(dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINPASSWORD))?.SETTINGVALUE, AES.Key, AES.IV),
                AdminLogin = dbSet_invoiceSys.FirstOrDefault(x => x.SETTINGTYPE.Equals(InvoiceSystemsettingKey.MVAN) && x.SETTINGKEY.Equals(InvoiceSystemsettingKey.MVAN_ADMINLOGIN))?.SETTINGVALUE,
            };
        }

        public InvoiceSystemSettingInfo UpdateInvoiceSystemSettingDetail(InvoiceSystemSettingInfo info)
        {
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_URL, info.Url);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_INVOICENOTCODE, info.InvoiceNotCode);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_INVOICEWITHCODE, info.InvoiceWithCode);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_CANCEL, info.Cancel);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_LOGIN, info.Login);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_REGISTER, info.Register);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_SIGNUP, info.SignUp);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_SYNTHESIS, info.Synthesis);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_MSTNNT, info.Mstnnt);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_MDVCS, info.Mdvcs);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_MSTTCGP, info.Mstcgp);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_ADMINURL, info.AdminUrl);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_SEARCH, info.Search);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_ADMINUSERID, info.AdminUserId);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_ADMINPASSWORD, EncryptAesManaged(info.AdminPassword));
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_USERNAME, info.Username);
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_PASSWORD, EncryptAesManaged(info.Password));
            UpdateInvoiceSystemsetting(InvoiceSystemsettingKey.MVAN, InvoiceSystemsettingKey.MVAN_ADMINLOGIN, info.AdminLogin);
            return GetInvoiceSystemSettingDetail();
        }
    }
}
