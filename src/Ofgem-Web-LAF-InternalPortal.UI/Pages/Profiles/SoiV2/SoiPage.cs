using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    public abstract class SoiPage(
        ILogger<SoiPage> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        internal ILogger<SoiPage> Logger = logger;
        internal IHttpClientFactory HttpClientFactory = httpClientFactory;
        internal IHttpContextAccessor HttpContextAccessor = httpContextAccessor;

        internal const string LOCAL_AUTHORITY_KEY = "las";
        internal const string SELECTED_LOCAL_AUTHORITY_KEY = "selectedlas";

        protected abstract string PageName { get; }

        [BindProperty(SupportsGet = true)] public string Name { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string OnsCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string LocalAuthorityId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string StatementOfIntentId { get; set; } = string.Empty;

        [BindProperty]
        public List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority> SelectedLocalAuthorities { get; set; } = [];

        [BindProperty] public required List<SelectListItem> LaSelectList { get; set; } = [];

        [BindProperty] public string SoiVersion { get; set; } = string.Empty;

        [BindProperty] public DateTime DatePublished { get; set; }

        [BindProperty] public string? SoiLink { get; set; } = string.Empty;

        [BindProperty] public Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2 SoiStatus { get; set; }

        [BindProperty] public Ofgem.LAF.SharedLibrary.Enums.SoiCategory SoiCategory { get; set; }
        [BindProperty] public string Message { get; set; } = string.Empty;

        [BindProperty]
        public required List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority> LocalAuthorities { get; set; } = [];

        [BindProperty] public required string SelectedLocalAuthorityId { get; set; } = string.Empty;

        public string DateFromMin { get; set; } = Constants.SOI_MINIMUM_DATE;

        public string DateFromMax { get; set; } =
            DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public bool SoiVersionHasError { get; set; }

        [BindProperty] public bool SoiStatusHasError { get; set; }

        [BindProperty] public bool SoiLinkHasError { get; set; }

        [BindProperty] public bool SelectLaHasError { get; set; }

        [BindProperty] public required string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public required string SelectedLa { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public bool CanSubmitOnBehalfOfOtherAuthorities { get; set; }

        [BindProperty(SupportsGet = true)] public bool IsSignOffResponsiblePersonSigned { get; set; }

        [BindProperty(SupportsGet = true)] public bool IsSignOffLaOfficerResponsibleStatement { get; set; }

        [BindProperty] public Models.ThreePartDate? DatePublishedControl { get; set; }

        [BindProperty] public Models.ThreePartDateNullable? DatePublishedNullableControl { get; set; }

        [BindProperty] public bool? IsLocalAuthorityNamed { get; set; }

        [BindProperty] public bool? IsDelegatedAuthorityNamed { get; set; }

        [BindProperty] public bool? IsSoiSameAsWebsite { get; set; }

        [BindProperty] public bool? HasMostRecentTemplate { get; set; }

        [BindProperty] public bool? IsPreviousVersionStatusClear { get; set; }

        [BindProperty] public bool LocalAuthorityNamedHasError { get; set; }

        [BindProperty] public bool DelegatedAuthorityNamedHasError { get; set; }

        [BindProperty] public bool SoiSameAsWebsiteHasError { get; set; }

        [BindProperty] public bool MostRecentTemplateHasError { get; set; }

        [BindProperty] public bool PreviousVersionStatusHasError { get; set; }

        [BindProperty] public bool? IsProxy5SchemePresent { get; set; }

        [BindProperty] public bool Proxy5SchemePresentError { get; set; }

        [BindProperty] public bool? IsDescriptionNiceNg6Recommendation2 { get; set; }

        [BindProperty] public bool DescriptionNiceNg6Recommendation2Error { get; set; }

        [BindProperty] public bool? IsPublishedDateCorrect { get; set; }

        [BindProperty] public bool? IsProxy5PartOfRoute2 { get; set; }

        [BindProperty] public Ofgem.LAF.SharedLibrary.Enums.ForSchemeEnum ForScheme { get; set; }

        [BindProperty] public Ofgem.LAF.SharedLibrary.Models.LocalAuthority? LocalAuthority { get; set; }

        [BindProperty] public bool? IsRoute4SapBandsCorrect { get; set; }

        [BindProperty] public bool HasDeclarations { get; set; }

        [BindProperty] public List<Ofgem.LAF.SharedLibrary.Models.DesignatedLA> DesignatedLas { get; set; } = [];

        [BindProperty] public string StatementOfIntentLink { get; set; } = string.Empty;

        [BindProperty] public bool SchemeNameHasError { get; set; }

        [BindProperty] public bool PublishDateHasError { get; set; }

        [BindProperty] public bool Proxy5PartOfRoute2HasError { get; set; }

        [BindProperty] public string? DescriptionNiceNg6ErrorMessage { get; set; }

        [BindProperty] public string? AdditionalInformationText { get; set; }

        [BindProperty]
        public bool IsAutomaticFail => IsLocalAuthorityNamed == false || IsSoiSameAsWebsite == false ||
                                        HasMostRecentTemplate == false ||
                                        ForScheme == ForSchemeEnum.NoScheme;

        [BindProperty] public string? LocalAuthorityName { get; set; }

        [BindProperty] public bool ShowForm { get; set; } = true;

        public List<PageErrorsModel> DisplayPageErrors { get; set; } = [];

        [BindProperty] public bool HasMessage => DisplayPageErrors.Any(x => x.DisplayMessage?.Length > 0);

        [BindProperty]
        public string? GetSoiVersionErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == SoiVersionErrorId)?.DisplayMessage;

        [BindProperty]
        public string? GetSoiLinkErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == SoiLinkErrorId)?.DisplayMessage;

        [BindProperty]
        public string? GetSelectLaErrorMessage =>
            DisplayPageErrors.FirstOrDefault(x => x.ErrorId == SelectLaErrorId)?.DisplayMessage;

        [BindProperty] public string FocusControlId { get; set; } = string.Empty;


        internal void AddInLa()
        {
            RefreshFromTempData();

            Ofgem.LAF.SharedLibrary.Models.LocalAuthority? la =
                LocalAuthorities.SingleOrDefault(x => x.LocalAuthorityId == new Guid(SelectedLa));

            if (la != null)
            {
                SelectedLocalAuthorities.Add(la);

                if (!string.IsNullOrEmpty(la.Name)) LocalAuthorities.RemoveAll(x => x.Name == la.Name);
            }

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            _ = ValidBasePage();
        }

        internal void TakeOutLa(string id)
        {
            RefreshFromTempData();

            var la = SelectedLocalAuthorities.FindAll(x => x.LocalAuthorityId == new Guid(id));

            if (la is not null)
            {
                LocalAuthorities.AddRange(la.Distinct());

                SelectedLocalAuthorities.RemoveAll(x => x.LocalAuthorityId == new Guid(id));
            }

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            _ = ValidBasePage();
        }

        internal void SetCanSubmitOnBehalfOf()
        {
            RefreshFromTempData();

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            CanSubmitOnBehalfOfOtherAuthorities = true;

            _ = ValidBasePage();
        }

        internal void SetCannotSubmitOnBehalfOf()
        {
            RefreshFromTempData();

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            CanSubmitOnBehalfOfOtherAuthorities = false;

            _ = ValidBasePage();
        }

        internal void CreateLocalAuthoritiesSelectList()
        {
            List<SelectListItem> list =
            [
                .. LocalAuthorities
                    .Select(a => new SelectListItem { Value = a.LocalAuthorityId.ToString(), Text = a.Name })
                    .OrderBy(x => x.Text),
            ];

            LaSelectList = list;
        }

        internal void RefreshFromTempData()
        {
            LocalAuthorities = TempData.Get<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>(LOCAL_AUTHORITY_KEY);
            SelectedLocalAuthorities =
                TempData.Get<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>(SELECTED_LOCAL_AUTHORITY_KEY);

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }
        }

        internal void StoreInTempData()
        {
            TempData.Put(LOCAL_AUTHORITY_KEY, LocalAuthorities);
            TempData.Put(SELECTED_LOCAL_AUTHORITY_KEY, SelectedLocalAuthorities);
        }

        internal async Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>> GetLocalAuthoritiesAsync()
        {
            try
            {
                List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? result = [];

                HttpClient httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(Services.LocalAuthorityApi.Route);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    result =
                        await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();
                }

                return result ?? [];
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} - GetLocalAuthoritiesAsync, Error", "SoiPage");
                Message = "An issue occurred retrieving the data.";
                throw;
            }
        }

        internal void RemoveLocalAuthorityFromLisxxt(string laName)
        {
            _ = LocalAuthorities.RemoveAll(x => x.Name == laName);
        }

        internal static bool SoiVersionIsInvalid(string soiVersion)
        {
            foreach (char c in soiVersion)
            {
                if ((!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_') || char.IsWhiteSpace(c))
                {
                    return true;
                }
            }

            return false;
        }


#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        internal bool ValidBasePage()
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            ValidateSoiVersion();
            ValidateSoiLink();
            ValidateSelectedLocalAuthorities();

            if ((!(IsSignOffResponsiblePersonSigned) || !(IsSignOffLaOfficerResponsibleStatement)) &&
                SoiStatus is
                    Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff or
                    Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment)
            {
                DisplayErrors(Constants.SOI_EDIT_MANDATORY_FIELDS_SIGN_OFF, string.Empty);
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            return true;
        }

        public string SoiVersionErrorId = "SoiVersion";
        public string SoiLinkErrorId = "SoiLink";
        public string SelectLaErrorId = "canSubmitOnBehalfOff";

        public bool ValidateSoiVersion()
        {
            if (string.IsNullOrEmpty(SoiVersion))
            {
                DisplayErrors(Constants.VERSION_NUMBER_IS_MANDATORY, SoiVersionErrorId);

                SoiVersionHasError = true;
                StoreInTempData();
                return false;
            }

            if (SoiVersion.Contains(' '))
            {
                DisplayErrors("Version must not contain spaces", SoiVersionErrorId);
                SoiVersionHasError = true;
                StoreInTempData();
                return false;
            }

            if (SoiVersionIsInvalid(SoiVersion))
            {
                DisplayErrors(Constants.VERSION_NUMBER_IS_INVALID_MESSAGE, SoiVersionErrorId);
                SoiVersionHasError = true;
                StoreInTempData();
                return false;
            }

            return true;
        }

        public bool ValidateSoiLink()
        {
            if (!UrlValidation.IsLinkValid(SoiLink))
            {
                DisplayErrors("Enter a URL in the format https://example.gov.uk", SoiLinkErrorId);
                SoiLinkHasError = true;
                StoreInTempData();
                return false;
            }
            return true;
        }

        public bool ValidateSelectedLocalAuthorities()
        {
            if (CanSubmitOnBehalfOfOtherAuthorities && SelectedLocalAuthorities.Count == 0)
            {
                DisplayErrors("Select a local authority", SelectLaErrorId);
                SelectLaHasError = true;
                StoreInTempData();
                return false;
            }
            return true;
        }

        public bool ValidateSoiDate()
        {
            if (!ValidateDatePublishedControl())
            {
                return false;
            }
            return true;
        }


#pragma warning disable S4144 // Cognitive Complexity of methods should not be too high
        public bool ValidInitialAssessmentChecklistPage()
#pragma warning restore S4144 // Cognitive Complexity of methods should not be too high
        {
            if (IsLocalAuthorityNamed == null)
            {
                LocalAuthorityNamedHasError = true;
                DisplayMessage = "Select an option to indicate the relevant local authority is named";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (IsDelegatedAuthorityNamed == null)
            {
                DelegatedAuthorityNamedHasError = true;
                DisplayMessage = "Select an option to indicate if a delegate authority is named";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (IsSoiSameAsWebsite == null)
            {
                SoiSameAsWebsiteHasError = true;
                DisplayMessage =
                    "Select an option to indicate if the Statement of Intent is published on the local authority website";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (HasMostRecentTemplate == null)
            {
                MostRecentTemplateHasError = true;
                DisplayMessage =
                    "Select an option to indicate if the latest Statement of Intent template has been used";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (IsPreviousVersionStatusClear == null)
            {
                PreviousVersionStatusHasError = true;
                DisplayMessage =
                    "Select an option to indicate if previous versions of the statement of intent have been clearly displayed as withdrawn";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }


            return true;
        }

#pragma warning disable S4144 // Cognitive Complexity of methods should not be too high
        public bool ValidRoute2Proxy5Page()
#pragma warning restore S4144 // Cognitive Complexity of methods should not be too high
        {
            if (IsProxy5PartOfRoute2 == true)
            {
                if (IsProxy5SchemePresent == null)
                {
                    Proxy5SchemePresentError = true;
                    DisplayMessage =
                        "Select the option to indicate if the local authority has named and described the scheme";

                    DescriptionNiceNg6Recommendation2Error = true;
                    DescriptionNiceNg6ErrorMessage = "Select the option to indicate if the description adheres to the guidelines";

                    ShowNotification = true;
                    StoreInTempData();
                    return false;
                }

                if (IsProxy5SchemePresent == true && IsDescriptionNiceNg6Recommendation2 == null)
                {
                    DescriptionNiceNg6Recommendation2Error = true;
                    DisplayMessage = "Select the option to indicate if the description adheres to the guidelines";
                    DescriptionNiceNg6ErrorMessage = DisplayMessage;
                    ShowNotification = true;
                    StoreInTempData();
                    return false;
                }

                if (IsProxy5SchemePresent == false && IsDescriptionNiceNg6Recommendation2 == true)
                {
                    DescriptionNiceNg6Recommendation2Error = true;
                    DisplayMessage = "As you have stated that proxy 5 is not being used, this question must be left blank";
                    DescriptionNiceNg6ErrorMessage = DisplayMessage;
                    ShowNotification = true;
                    StoreInTempData();
                    return false;
                }
            }


            return true;
        }

#pragma warning disable S4144 // Cognitive Complexity of methods should not be too high
        public bool ValidStatusSettingPage()
#pragma warning restore S4144 // Cognitive Complexity of methods should not be too high
        {
            if (SoiStatus is not (Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment or
                    Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff or
                    Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment))
            {
                SoiStatusHasError = true;
                DisplayMessage = "SoI status is mandatory.";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            return true;
        }

        public bool ValidDateSchemeDetailsPage()
        {
            if (ForScheme != ForSchemeEnum.ECO4 && ForScheme != ForSchemeEnum.GBIS && ForScheme != ForSchemeEnum.ECO4andGBIS && ForScheme != ForSchemeEnum.NoScheme)
            {
                SchemeNameHasError = true;
                DisplayMessage = "Select an option to indicate the scheme the local authority will administer";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (IsPublishedDateCorrect == null)
            {
                PublishDateHasError = true;
                DisplayMessage = "Select an option to indicate if the publish date is correct.";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            if (IsProxy5PartOfRoute2 == null)
            {
                Proxy5PartOfRoute2HasError = true;
                DisplayMessage = "Select an option to indicate if the scheme is part of Route 2.";
                ShowNotification = true;
                StoreInTempData();
                return false;
            }

            return true;
        }

        internal bool ValidateDatePublishedControl()
        {
            if (DatePublishedControl != null && DatePublishedControl.HasErrors())
            {
                DisplayErrors(DatePublishedControl.ErrorMessage, "day");
                StoreInTempData();
                return false;
            }

            DatePublished = new DateTime(DatePublishedControl!.Year, DatePublishedControl!.Month,
                DatePublishedControl!.Day);

            if (DatePublished < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
            {
                var message = "Enter a date from April 1 2022 up to the present date";
                DisplayErrors(message, "day");
                DatePublishedControl.HasError = true;
                DatePublishedControl.HasYearError = true;
                DatePublishedControl.ErrorMessage = message;
                StoreInTempData();
                return false;
            }

            if (DatePublished > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
            {
                var message = $"Date published field cannot be after {DateTime.Now:dd/MM/yyyy}";
                DisplayErrors(message, "day");
                DatePublishedControl.HasError = true;
                DatePublishedControl.ErrorMessage = message;
                StoreInTempData();
                return false;
            }

            return true;
        }

        public async Task SetPageData(Guid statementOfIntentId)
        {
            try
            {
                var statementOfIntent = await GetSoi(statementOfIntentId);

                if (statementOfIntent != null)
                {
                    LocalAuthorityId = statementOfIntent.LocalAuthorityId.ToString();
                    LocalAuthority = statementOfIntent.LocalAuthority;
                    Name = statementOfIntent.LocalAuthority?.Name ?? "";
                    OnsCode = statementOfIntent.OnsCode;
                    DatePublished = statementOfIntent.PublishedDate;
                    SoiVersion = statementOfIntent.VersionNumber;
                    StatementOfIntentId = statementOfIntent.StatementOfIntentId.ToString();
                    StatementOfIntentLink = statementOfIntent.StatementOfIntentLink;
                    SoiCategory = statementOfIntent.Category;

                    LocalAuthority = statementOfIntent.LocalAuthority;
                    LocalAuthorityName = statementOfIntent.LocalAuthority?.Name ?? "";

                    HasDeclarations = statementOfIntent.HasDeclarations;
                    IsRoute4SapBandsCorrect = statementOfIntent.IsRoute4SapBandsCorrect;
                    DatePublished = statementOfIntent.PublishedDate;
                    CanSubmitOnBehalfOfOtherAuthorities = statementOfIntent.CanSubmit;

                    DesignatedLas = statementOfIntent.DesignatedLas is null
                        ? []
                        : statementOfIntent.DesignatedLas.Select(t =>
                            new Ofgem.LAF.SharedLibrary.Models.DesignatedLA
                            {
                                StatementOfIntentId = t.StatementOfIntentId,
                                LocalAuthorityId = t.LocalAuthorityId,
                                DesignatedLAId = t.DesignatedLAId,
                                LocalAuthority = t.LocalAuthority
                            }).ToList();

                    // Initial Assessment checklist page details set
                    IsSoiSameAsWebsite = statementOfIntent.IsSoiSameAsWebsite;
                    HasMostRecentTemplate = statementOfIntent.HasMostRecentTemplate;
                    IsPreviousVersionStatusClear = statementOfIntent.IsPreviousVersionStatusClear;
                    IsLocalAuthorityNamed = statementOfIntent.IsLocalAuthorityNamed;
                    IsDelegatedAuthorityNamed = statementOfIntent.IsDelegatedAuthorityNamed;

                    // Scheme details page set
                    ForScheme = statementOfIntent.ForScheme;
                    IsPublishedDateCorrect = statementOfIntent.IsPublishedDateCorrect;
                    IsProxy5PartOfRoute2 = statementOfIntent.IsProxy5PartOfRoute2;

                    // eligibility details page set
                    IsProxy5SchemePresent = statementOfIntent.IsProxy5SchemePresent;
                    IsDescriptionNiceNg6Recommendation2 = statementOfIntent.IsDescriptionNiceNg6Recomendation2;

                    // todo to added one more condition from scheme details waiting on Nadeem and Shakir

                    // soi status setting page set
                    if (statementOfIntent.IsLocalAuthorityNamed == false || statementOfIntent.IsSoiSameAsWebsite == false ||
                        statementOfIntent.HasMostRecentTemplate == false || statementOfIntent.ForScheme == ForSchemeEnum.NoScheme)
                    {
                        SoiStatus = SoiStatusV2.FailedAssessment;
                    }
                    else
                    {
                        SoiStatus = statementOfIntent.Status;
                    }

                    if (statementOfIntent.AssessmentNotes?.Count != 0)
                    {
                        var note = statementOfIntent.AssessmentNotes?.FirstOrDefault();
                        AdditionalInformationText = note?.Text;
                    }
                }

                TempData.Keep();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} -  RefreshData, Error", PageName);
                Message = $"An issue occurred retrieving the data by Statement of Intent Id {statementOfIntentId}.";
                throw;
            }
        }

        public async Task<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent?> GetSoi(Guid statementOfIntentId)
        {
            HttpClient httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            HttpResponseMessage httpGetResponseMessage =
                await httpClient.GetAsync(Services.LocalAuthorityApi.RouteGetSoiV2 + $"{statementOfIntentId}");

            if (httpGetResponseMessage.IsSuccessStatusCode)
            {
                Ofgem.LAF.SharedLibrary.Models.StatementOfIntent? statementOfIntentModel = await httpGetResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

                if (statementOfIntentModel != null)
                {
                    return statementOfIntentModel;
                }
            }

            var problem = await httpGetResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem is { Detail: not null })
            {
                DisplayMessage = problem.Detail;
                ShowNotification = true;
                ShowForm = false;
                Logger.LogInformation(message: "SoiPageV2 -  OnGetSoi - {DisplayMessage}", DisplayMessage);
            }

            return null;
        }

        public void DisplayErrors(string? displayMessage, string? errorId)
        {
            var error = new PageErrorsModel
            {
                DisplayMessage = displayMessage,
                ErrorId = errorId
            };

            DisplayPageErrors.Add(error);
        }
    }
}
