using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.GateWay.Models.MVan;
using InvoiceServer.GateWay.Services.MVan;
using InvoiceServer.GateWay.Services.ServiceFactorys;
using System.Configuration;
using System.Web;

namespace InvoiceServer.API.Business.GateWay
{
    public class MVanBusiness : BaseBusiness
    {
        private readonly IMVanService vanService;
        private readonly IDbContext dbContext;

        public MVanBusiness(IServiceFactory serviceFactory)
        {
            this.vanService = serviceFactory.GetService<IMVanService>(dbContext);

        }
        public string MVanResponse(MVanResponse mVanResponse)
        {
            return this.vanService.MVanResponse(mVanResponse);
        }
        public string MVanReceivedata(MVanResponse mVanResponse)
        {
            mVanResponse.FolderStore = ConfigurationManager.AppSettings["FolderInvoiceFile"];
            return this.vanService.MVanReceivedata(mVanResponse);
        }
        public VanSignUpResult VanSignUp(VanSignUpRequest vanSignUpRequest)
        {
            return this.vanService.VanSignUp(vanSignUpRequest);
        }

        public VanLoginResult VanLogin(VanLoginRequest vanLoginRequest)

        {
            return this.vanService.VanLogin(vanLoginRequest);
        }
        public VanDefaulResult VanRegister(VanDefaulRequest vanDefaulRequest)
        {
            vanDefaulRequest.MstNnt = this.CurrentUser?.Company?.TaxCode;
            return this.vanService.VanRegister(vanDefaulRequest);
        }

        public VanDefaulResult SendInvoiceWithCode(VanDefaulRequest vanDefaulRequest)
        {
            vanDefaulRequest.MstNnt = this.CurrentUser?.Company?.TaxCode;
            return this.vanService.SendInvoiceWithCode(vanDefaulRequest);
        }

        public VanDefaulResult SendInvoiceNotCode(VanDefaulRequest vanDefaulRequest)
        {
            vanDefaulRequest.MstNnt = this.CurrentUser?.Company?.TaxCode;
            return this.vanService.SendInvoiceNotCode(vanDefaulRequest);
        }

        public VanDefaulResult VanCancel(VanDefaulRequest vanDefaulRequest)
        {
            vanDefaulRequest.MstNnt = this.CurrentUser?.Company?.TaxCode;
            return this.vanService.VanCancel(vanDefaulRequest);
        }

        public VanDefaulResult VanSynthesis(VanDefaulRequest vanDefaulRequest)
        {
            //vanDefaulRequest.MstNnt = ;
            return this.vanService.VanSynthesis(vanDefaulRequest);
        }

        public VanResponseResult MVanReceived(string maThongDiep, bool sleep, int type)
        {
            VanDefaulResult van = new VanDefaulResult()
            {
                Data = new DefaulData()
                {
                    MaThongdiep = maThongDiep
                }
            };
            return this.vanService.InsertMinvoiceRecieved(van, sleep, type);
        }

        public InvoiceSystemSettingInfo UpdateInvoiceSystemSettingDetail(InvoiceSystemSettingInfo info)
        {
            return this.vanService.UpdateInvoiceSystemSettingDetail(info);
        }

        public InvoiceSystemSettingInfo GetInvoiceSystemSettingDetail()
        {
            return this.vanService.GetInvoiceSystemSettingDetail();
        }
    }
}