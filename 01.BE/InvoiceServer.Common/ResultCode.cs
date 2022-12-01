
namespace InvoiceServer.Common
{
    public enum ResultCode
    {
        #region Common error codes (000 ~ 099)

        NoError = 1,
        UnknownError = 2,
        TokenInvalid = 3,
        NotFoundResourceId = 4,
        IdNotMatch = 5,
        NotModified = 6,
        WaitNextRequest = 7,
        DataInvalid = 8,
        DataIsUsed = 9,
        FileLarge = 10,
        SendEmailFailed = 11,
        SendingEmail = 12,
        ReleaseDateInvalid = 13,
        NotFoundCompany = 14,
        FileUploadNotTheSameType = 15,
        #endregion Common error codes

        #region System error codes (100 ~ 199)

        SystemConfigNotFound = 100,
        SystemConfigInvalid = 101,
        FileNotFound = 102,
        ReadWriteFileError = 103,
        SendMailError = 104,

        #endregion System error codes

        #region Client error codes (200 ~ 299)

        RequestDataInvalid = 200,
        NotEnoughPermission = 201,

        #endregion Client error codes

        #region Noitice Invoice error codes (300 ~ 399)

        // Login, Session error codes
        UserIsDisabled = 300,
        UserNotFound = 301,
        LoginFailed = 302,
        SessionAlive = 303,
        SessionEnded = 304,

        // Insert, update data error codes
        ConflictResourceId = 305,

        #endregion Noitice Invoice error codes

        #region Common error codes (1000 ~ 1999)
        NotAuthorized = 1000,
        NotPermisionData = 1001,
        TimeOut = 1002,

        #endregion User operation error codes

        #region Login error codes (2000 ~ 2009)
        LoginUserIdIsEmpty = 2000,
        LoginUserIdNotExist = 2001,
        LoginEmailInvalid = 2002,
        LoginEmailNotExist = 2003,
        LoginPasswordIsEmpty = 2004,
        LoginOldPasswordIncorrect = 2005,
        OldPasswordIncorrect = 2007,
        AccountInfoIncorrect = 2008,
        #endregion

        #region Invoice error codes (2010 ~ 2019)
        InvoiceReleasedNotDelete = 2010,
        InvoiceReleasedNotUpdate = 2011,
        NumberInvoiceOverloadRegister = 2012,
        InvoiceIsConverted = 2013,
        InvoiceDateInvalid = 2014,
        InvoiceTypeChanged = 2015,
        InvoiceNotApproveSymbol = 2016,
        InvoiceNotSubstitute = 2017,
        InvoiceHadNumber = 2018,
        InvoiceNotInNewStatus = 2019,

        #endregion

        #region User Account Management error codes (2020 ~ 2039)
        UserAccountMgtUsernameExceedMaxLength = 2020,
        UserAccountMgtUsernameIsEmpty = 2021,
        UserAccountMgtEmailExceedMaxLength = 2022,
        UserAccountMgtPasswordExceedMaxLength = 2023,
        UserAccountMgtEmailIsEmpty = 2024,
        UserAccountMgtEmailInvalid = 2025,
        UserAccountMgtPasswordIsEmpty = 2026,
        UserAccountMgtConflictResourceUserId = 2027,
        UserAccountMgtUserIdIsEmpty = 2028,
        UserAccountMgtUserIdExceedMaxLength = 2029,
        UserAccountMgtConflictResourceEmail = 2030,
        UserAccountMgtCompanyIdNotFound = 2031,
        UserAccountMgtNotPermissionDelete = 2032,
        UserAccountMgtConflictResourceTaxCode = 2033,
        UserAccountMgtConflictResourceCustomerCode = 2034,
        UserAccountMgtIsReferrenced = 2035,

        #endregion Project management error codes

        #region Client error codes (2040~2059)

        // English name exceeds maximum length
        ClientEnglishNameExceedsMaxLength = 2040,
        // Local name exceeds maximum length
        ClientLocaNameExceedsMaxLength = 2041,

        // English address exceeds maximum length
        ClientEnglishAddressExceedsMaxLength = 2042,
        // Local address exceeds maximum length
        ClientLocalAddressExceedsMaxLength = 2043,

        // Phone number exceeds maximum length
        ClientPhoneNumberExceedsMaxLength = 2044,
        // Fax number exceeds maximum length
        ClientFaxNumberExceedsMaxLength = 2045, // 2045

        // Contact person's name exceeds maximum length
        ClientContactNameExceedsMaxLength = 2046,
        // Email address exceeds maximum length
        ClientContactEmailExceedsMaxLength = 2047,

        // You have to enter at least a name of client
        ClientClientNameIsEmpty = 2048,
        // You have to enter at least a address of client
        ClientAddressIsEmpty = 2049,
        // Format of email is invalid
        ClientEmailInvalidFormat = 2050, // 2050

        // Upload file excel to import empty
        FileUploadImportEmpty = 2051,

        #endregion

        #region Country admin account management error codes(2080 ~ 2999)

        CountryAdminMgtSearchTextExceedMaxLength = 2080,
        CountryAdminMgtUsernameExceedMaxLength = 2081,
        CountryAdminMgtFormatUserNameInvalid = 2082,
        CountryAdminMgtEmailExceedMaxLength = 2083,
        CountryAdminMgtEmailInvalid = 2084,
        CountryAdminMgtPasswordInvalid = 2085,
        CountryAdminMgtPasswordExceedMaxLength = 2086,
        CountryAdminMgtEmailIsEmpty = 2087,
        CountryAdminMgtPasswordIsEmpty = 2088,

        #endregion Country admin account management error codes

        #region Company error Code (3000 ~ 3029)

        CompanyNameBlank = 3000,
        CompanyAddressBlank = 3001,
        CompanyAdminNameBlank = 3002,
        CompanyAdminEmailBlank = 3003,
        CompanyTaxInvalid = 3004,
        CompanyEnglishCompanyNameMaxLength = 3005,
        CompanyLocalCompanyNameMaxLength = 3006,
        CompanyEnglishAddressMaxLength = 3007,
        CompanyLocalAddressMaxLength = 3008,
        CompanyFirstPhoneNumberMaxLength = 3009,
        CompanyPhoneNumberMaxLength = 3010,
        CompanyFaxNumberMaxLength = 3011,
        CompanyHomepageMaxLength = 3012,
        CompanyAdminNameMaxLength = 3013,
        CompanyAdminEmailMaxLength = 3014,
        CompanyAdminEmailInvalid = 3015,
        CompanyNameMaxLength = 3016,
        CompanyNameIsExitsTaxCode = 3017,
        CompanyNameIsExitsEmail = 3018,
        CompanyNameCannotCreated = 3019,
        CompanyHONotExists = 3020,
        CompanyIsUsing = 3021,
        CompanyBranchExists = 3022,

        #endregion

        #region Register Template error Code (3030 ~ 3060)
        TemplateInvoiceBeingused = 3030,
        TemplateCodeIdIsExisted = 3031,
        ExistedUnit = 3032,
        ExistedUnitName = 3033,
        ExistedProductName = 3034,
        ExistedInvoiceType = 3035,
        ExistedInvoiceSample = 3036,
        ExistedProductCode = 3037,
        ExistedNameUnit = 3038,
        ExistedDifferTax = 3039,
        ProductNotExisted = 25225,
        #endregion

        #region Announcement error codes (3061 ~ 3100)
        AnnouncementReleasedNotDelete = 3061,
        AnnouncementNotExist = 3062,
        AnnouncementInvoiceCancel = 3063,
        AnnouncementMinuIsExist = 3064,
        InvoiceExistAnnouncementAdjustmentOrCancel = 3065,
        InvoiceApproveInvalidDay = 3066,
        #endregion

        #region Noitice Invoice error codes (4000 ~ 4050)
        InvoiceReleaseNotApproved = 40123,
        NoiticeUseInvoiceApprovedNotUpdate = 4000,
        NoiticeUseInvoiceHasBeenUsedNotCancel = 4001,
        NoticeDetailSymbolIsExisted = 4002,
        InvoiceApprovedNotHasCustomer = 4003,
        InvoiceApprovedNotHasItem = 4004,
        NumberNoiticeUseInvoiceExceedNumberOfContract = 4005,
        InvoiceHaveNoPermissionData = 4006,
        InvoiceHaveNoPermissionDataSign = 4007,
        InvoiceApprovedInvalid = 4008,
        JobApprovedProcess = 4010,
        JobSignProcess = 4011,
        NumberNoiticeUseExceedNumberOfPackage = 4009,
        CaNotExisted = 1508,
        
        #endregion Noitice Invoice error codes

        #region Release Invoice error codes (4051 ~ 4100)
        ReleaseInvoiceApprovedNotUpdate = 4051,
        ReleaseUseInvoiceApprovedNotDelete = 4052,
        ReleaseDetailSymbolIsExisted = 4053,
        ReleaseInvoiceApprovedNotUnUpdate = 4143,
        ExpireDateToken = 4139,
        #endregion Noitice Invoice error codes

        #region Cancelling Invoice error codes (4101 ~ 4140)
        InvoiceIssuedNotUpdate = 4101,
        InvoiceIssuedNotCreate = 4102,
        InvoiceIssuedNotDelete = 4103,
        CannotCancel = 1580,
        UnitIssuedNotUpdate = 4104,
        #endregion Cancelling Invoice error codes

        #region Server Sign Error
        DataSignInvalid = 5110,
        CheckSignInvalid = 7254,
        PasswordCaInvalid = 7583,
        NotSlotHSM = 75855,
        #endregion

        #region ImportData
        ImportDataSizeOfFileTooLarge = 5050,
        ImportFileFormatInvalid = 5051,
        ImportColumnIsNotExist = 5052,
        ImportDataIsEmpty = 5053,
        ImportDataExceedMaxLength = 5054,
        ImportDataFormatInvalid = 5055,
        ImportDataNotSuccess = 5056,
        ImportDataIsExisted = 5057,
        ImportDataIsNotExisted = 5074,
        ImportDataIsNotNumberic = 5058,
        ImportDataIsNotDateTime = 5059,
        ImportDataDaeteOfInvoiceInvalid = 5060,
        ImportDataUnitNameDuplicate = 5061,
        ImportDataUnitNameDuplicateInFile = 5062,
        ImportDataOneCodeWithManyNameInFile = 5063,
        ImportInvoiceTaxNotIsTheSame = 5064,
        ImportInvoiceTaxEmpty = 5065,
        ImportDiscountPercentNotValid = 5066,
        ImportDiscountGreaterTotal = 5067,
        ImportSumLessThanZero = 5068,
        ImportCurrencyNameNotExists = 5069,
        ImportCurrencyExchangeRateNotExists = 5070,
        ImportInvoiceCurrencyCodeNotEqual = 5071,
        ImportInvoiceCurrencyNameNotEqual = 5072,
        ImportInvoiceCurrencyExchangeRateNotEqual = 5073,
        ImportInvoiceFileError = 5075,
        ImportInvoiceFileSuccess = 5076,
        ImportInvoiceFileRunning = 111,
        ImportClientFileError = 5077,
        ImportClientFileSuccess = 5078,
        ImportFileNotExist = 5079,
        ImportParseDataClientError = 5080,
        ImportParseDataInvoiceError = 5081,
        ImportParseDataClientAndInvoiceError = 5082,
        ImportAPIInitConnectionError = 5083,
        ImportAPIStatusReturnErrorNoDataWasFound = 5084,
        ImportAPIStatusReturnErrorInStringIsInValid = 5084,
        ImportAPIStatusReturnErrorBIDCSystemError = 5084,
        ImportAPIBankRefExisted = 5085,
        ImportPGDNotEmpty = 5086,
        ImportDataIsNotTemplate = 5087,
        ImportInvoiceCancelFileSuccess = 5089,
        ImportInvoiceCancelFileError = 5090,
        ImportInvoiceReplaceFileSuccess = 5901,
        ImportInvoiceReplaceFileError = 5902,
        ImportInvoiceAdjustedFileSuccess = 5903,
        ImportInvoiceAdjustedFileError = 5904,
        #endregion

        #region PayMent
        ExceedAmountOfInvoice = 6000,
        #endregion

        #region Contract
        ContractLoginUserIsEmpty = 8004,
        #endregion

        #region Agencies
        AgenciesHasContract = 9000,
        #endregion

        #region Customer
        CustomerHasContract = 9050,
        #endregion

        #region Role
        CreateRoleNameDuplicate = 10000,
        EditRoleNameDuplicate = 10001,
        RoleCanNotDelete = 10002,
        RoleNameIsNotEmptyOrWhitespace = 10003,
        #endregion

        #region Currency error Code (10101 ~ 10200)
        ExistedCurrencyName = 10101,
        ExistedCurrencyCode = 10102,
        CannotCreateCurrency = 10103,
        CurrencyIsUsedNotDelete = 10104,
        #endregion

        # region InvoiceTemplate (10201 ~ 10300)
        InvoiceTemplateIdCannotNull = 10201,
        #endregion

        #region Invoice error codes (2) (10301 ~ 10400)
        CanNotFindInvoiceWithCondition = 10301,
        InvoiceNotInCanceledStatus = 10302,
        InvoiceDeleted = 10303,
        InvoiceSentmail = 10304,
        #endregion

        #region ReportCancelling error codes (2) (10401 ~ 10500)
        ReportCancellingNotApprove = 10401,
        SendMailCQT = 10402,
        #endregion


        #region Invoice declaration (10501 ~ 10600)
        InvoiceDeclarationStatusIsNotAddNew = 10501,
        InvoiceDeclarationStatusIsNotApprove = 10502,
        #endregion

        #region Report history general (10601 ~ 10700)
        NotExistFirstTimeSuccess = 10601,
        CanNotInsertFirstTimeTwice = 10602,
        #endregion
    }
}
