namespace InvoiceServer.Common.Enums
{
    public enum RecordStatus
    {
        Created = 1,
        Approved,
        Cancelled,
    }

    public enum InvoiceStatus
    {
        New = 1,
        Approved,
        Releaseding,
        Released,
        Cancel,
        Draft,
        Delete,
        TwanSccess
    }

    public enum InvoiceNotificationStatus
    {
        failed = 0,
        Successfull = 1,
    }

    public enum ReportCancellingStatus
    {
        New = 1,
        Approved,

    }
    public enum AnnouncementStatus
    {
        New = 1,
        Approved = 3,
        Releaseding = 4,
        Released = 6,
        Successfull = 99,
    }

    public enum TypeSendInvoice
    {
        Email = 1,
        SMS,
    }
    public enum SendEmailStatus
    {
        Success = 1,
        Error = 0,
    }
    public enum CustomerType
    {
        IsAccounting = 1,
        NoneAccounting,
    }

    public enum InvoiceType
    {
        Substitute = 1,
        AdjustmentUpDown = 2,
        AdjustmentInfomation = 3,
        AdjustmentTax = 4,
        AdjustmentTaxCode = 5,
        AdjustmentElse = 6,
    }

    public enum ReleaseInvoiceStatus
    {
        New = 0,
        SignError = 1,
        ProcessSendEmail = 2,
        SenddingEmail,
        SendEmailError,
        SendEmailSuccess,
    }

    public enum ReleaseAnnouncementStatus
    {
        New = 0,
        SignError = 1,
        ProcessSendEmail = 2,
        SenddingEmail,
        SendEmailError,
        SendEmailSuccess,
    }

    public enum ReleaseInvoiceDetailStatus
    {
        New = 0,
        SendEmailSuccess,
        SendEmailError,
    }

    public enum ContractType
    {
        Customer = 1,
        Agencies,
        Client,
    }

    public enum ContractStatus
    {
        Active = 1,
        NotActivated,
    }
    //template html email
    public enum SendEmailType
    {
        NoticeAdjustSubstitute = 1,
        NoticeAdjustUp,
        NoticeAdjustDown,
        NoticeAdjustInfomation,
        NoticeAccountSeller,
        NoticeAccountCustomer,
        SendVerificationCode,
        SendVerificationCodeAnnouncementAdjust,
        SendVerificationCodeAnnouncementReplace,
        SendVerificationCodeAnnouncementCancel,
        SendVerificationCodeSendMonth,
        SendVerificationCodeSino
    }
    //email type insert
    public enum TypeEmailActive
    {
        ProvideAccountEmail = 1,
        DailyEmail,
        AnnouncementEmail,
        MonthlyEmail

    }
    public enum QueueCreateFileStatus
    {
        New = 1,
        Creating,
        Error,
        Successful,
    }

    public enum StatusSendEmail
    {
        New = 0,
        Successfull,
        Error,
    }

    enum EnumActionType
    {
        ADDNEW,
        ADJUSTMENT,// DIEUCHINH,
        SUBSTITUTE, //THAYTHE,
        CANCEL,//HUY,
        DUYET,
        UPADTE
    }

    public enum SignDetailTypeSign
    {
        Invoice = 1,
        Announcement = 2,
    }

    public enum PackageType
    {
        UAA = 50000,
        UAB = 100000,
        UAC = 150000,
        UAD = 200000,
        UAE = 250000,
        UAF = 300000,
        UAG = 350000,
        UAH = 400000,
        UAI = 450000,
        UAJ = 500000,
        UBA = 550000,
        UBB = 600000,
        UBC = 650000,
        UBD = 700000,
        UBE = 750000,
        UBF = 800000,
        UBG = 850000,
        UBH = 900000,
        UBI = 950000,
        UBJ = 1000000,
        UAX = -1 // k gioi han
    }

    public enum SystemLog_LogType
    {
        Success = 0,
        Error,
        Warning,
    }

    public enum PasswordStatus
    {
        NotEncript = 1,
        Encripted = 2,
        Changed = 3,
    }

    public enum AdjustmentType
    {
        Up = 1,
        Down = 2,
    }
}