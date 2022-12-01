using InvoiceServer.Business.Models;
using InvoiceServer.GateWay.Models.MVan;

namespace InvoiceServer.GateWay.Services.MVan
{
    public interface IMVanService
    {
        string MVanResponse(MVanResponse mVanResponse);
        string MVanReceivedata(MVanResponse mVanResponse);
        VanSignUpResult VanSignUp(VanSignUpRequest vanSignUpRequest);
        VanLoginResult VanLogin(VanLoginRequest vanLoginRequest);
        VanDefaulResult VanRegister(VanDefaulRequest vanDefaulRequest);
        VanDefaulResult SendInvoiceWithCode(VanDefaulRequest vanDefaulRequest);
        VanDefaulResult SendInvoiceNotCode(VanDefaulRequest vanDefaulRequest);
        VanDefaulResult VanCancel(VanDefaulRequest vanDefaulRequest);
        VanDefaulResult VanSynthesis(VanDefaulRequest vanDefaulRequest);
        VanResponseResult MVanReceived(VanResponseRequest vanResponseRequest);
        VanResponseResult InsertMinvoiceRecieved(VanDefaulResult result, bool sleep, int type = 0);
        InvoiceSystemSettingInfo UpdateInvoiceSystemSettingDetail(InvoiceSystemSettingInfo info);
        InvoiceSystemSettingInfo GetInvoiceSystemSettingDetail();
    }
}
