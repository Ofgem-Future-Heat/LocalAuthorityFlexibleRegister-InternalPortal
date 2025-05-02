using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class Edit(
        ILogger<Edit> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        Services.ILaManagementService laManagementService)
        : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {

        [BindProperty] public Extensions.Permissions.SoiV2.Edit Permissions { get; set; } = new();

        protected override string PageName => "EditSoiV2";

        public async Task OnGet(string onsCode, string version, string publishedDate)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            await RefreshData(onsCode, version, publishedDate);

            LocalAuthorities = await GetLocalAuthoritiesAsync();

            LocalAuthorities.RemoveAll(x => x.Name == Name); // my local authority

            TempData.Put(LOCAL_AUTHORITY_KEY, LocalAuthorities);

            if (SelectedLocalAuthorities.Count > 0)
            {
                foreach (var authority in SelectedLocalAuthorities)
                {
                    if (authority.Name != null)
                        LocalAuthorities.RemoveAll(x => x.Name == authority.Name); // delegate local authorities
                }
            }

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;
        }

        public async Task OnPostAddInLa()
        {
            Logger.LogInformation("{PageName} -  OnPostAddInLa", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(AddInLa);

            FocusControlId = "local-authority";
        }

        public async Task OnPostTakeOutLa(string id)
        {
            Logger.LogInformation("{PageName} -  OnPostTakeOutLa", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(() => { TakeOutLa(id); });

            FocusControlId = "local-authority";
        }

        public async Task OnPostCanSubmitOnBehalfOf()
        {
            Logger.LogInformation("{PageName} -  OnPostCanSubmitOnBehalfOf", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetCanSubmitOnBehalfOf);
            ValidateSoiDate();
        }

        public async Task OnPostCannotSubmitOnBehalfOf()
        {
            Logger.LogInformation("{PageName} -  OnPostCannotSubmitOnBehalfOf", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetCannotSubmitOnBehalfOf);
            ValidateSoiDate();
        }

        public async Task<IActionResult> OnPostSaveContinue(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveContinue", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            RefreshFromTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            ValidBasePage();
            ValidateSoiDate();

            if (DisplayPageErrors.Count > 0) return Page();

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            try
            {
                var soi = await laManagementService.GetSoiById(statementOfIntentId)
                                           ?? throw new ArgumentNullException($"{statementOfIntentId} does not exist");

                if (!soi.IsInitialCheckListComplete)
                {
                    return RedirectToPage(
                        LafPages.SoiV2.InitialAssessmentChecklist.ROUTE,
                        LafPages.SoiV2.InitialAssessmentChecklist.METHOD_GET_BY_ID,
                        new { StatementOfIntentId = statementOfIntentId });
                }

                if (!soi.IsSchemeDetailComplete)
                {
                    return RedirectToPage(
                        LafPages.SoiV2.SchemeChecklist.ROUTE,
                        LafPages.SoiV2.SchemeChecklist.METHOD_GET_BY_ID,
                        new { StatementOfIntentId = statementOfIntentId });
                }

                if (!soi.IsEligibilityForRoute2Proxy5Complete)
                {
                    return RedirectToPage(
                        LafPages.SoiV2.EligibilityRoute2Proxy5.ROUTE,
                        LafPages.SoiV2.EligibilityRoute2Proxy5.METHOD_GET_BY_ID,
                        new { StatementOfIntentId = statementOfIntentId });
                }


            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving SOI data, {ex.Message}";
                logger.LogLafError(LogEvents.GetSoi, DisplayMessage);
                return Page();
            }




            return RedirectToPage(
                LafPages.SoiV2.InitialAssessmentChecklist.ROUTE,
                LafPages.SoiV2.InitialAssessmentChecklist.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        public async Task<IActionResult> OnPostSaveExit(Guid statementOfIntentId)
        {
            Logger.LogInformation("{PageName} -  OnPostSaveExit", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Edit(HttpContextAccessor, HttpClientFactory, TempData);

            RefreshFromTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;

            ValidBasePage();
            ValidateSoiDate();

            if (DisplayPageErrors.Count > 0) return Page();

            var success = await this.SaveData(statementOfIntentId);

            if (!success)
            {
                return Page();
            }

            return RedirectToPage(
                LafPages.SoiV2.SoiDetails.ROUTE,
                LafPages.SoiV2.SoiDetails.METHOD_GET_BY_ID,
                new { StatementOfIntentId = statementOfIntentId });
        }

        private async Task RefreshData(string onsCode, string version, string publishedDate)
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteOnsExists + $"{onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                    if (result != null)
                    {
                        LocalAuthorityId = result.LocalAuthorityId.ToString();
                        OnsCode = result.OnsCode;
                        Name = result.Name ?? "";

                        var selectedStatementOfIntent = result.StatementOfIntents!.First(s =>
                            s.VersionNumber == version &&
                            s.PublishedDate.ToString("yyyyMMdd") == publishedDate);

                        if (selectedStatementOfIntent == null)
                        {
                            DisplayMessage = $"Unable to locate the version: {version}, published date: {publishedDate}";
                            Permissions = new Extensions.Permissions.SoiV2.Edit { CanContinue = false };
                        }
                        else
                        {
                            StatementOfIntentId = selectedStatementOfIntent.StatementOfIntentId.ToString();
                            SoiVersion = selectedStatementOfIntent.VersionNumber;
                            SoiStatus = selectedStatementOfIntent.Status;
                            SoiCategory = selectedStatementOfIntent.Category;
                            CanSubmitOnBehalfOfOtherAuthorities = selectedStatementOfIntent.CanSubmit;
                            SoiLink = selectedStatementOfIntent.StatementOfIntentLink;
                            DatePublished = selectedStatementOfIntent.PublishedDate;

                            DatePublishedControl = new Models.ThreePartDate(
                                source: DatePublished,
                                title: "Date of SOI publication",
                                titleToBeUsedInErrorMessage: "Date of SOI publication",
                                firstHint: "",
                                secondHint: "For example, 25 2 2024",
                                showTheHighlightBar: true);

                            if (selectedStatementOfIntent.DesignatedLas != null)
                            {
                                foreach (var designatedLa in selectedStatementOfIntent.DesignatedLas)
                                {
                                    SelectedLocalAuthorities.Add(designatedLa.LocalAuthority!);
                                }
                            }
                        }
                    }
                }

                TempData.Keep();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} -  RefreshData, Error", PageName);
                Message = $"An issue occurred retrieving the data local authority by onsCode {onsCode}.";
                throw;
            }
        }

        private async Task<bool> SaveData(Guid statementOfIntentId)
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpGetResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteSoiById + $"{statementOfIntentId}");

                if (!httpGetResponseMessage.IsSuccessStatusCode) return false;

                var result = await httpGetResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

                if (result == null) return false;

                Ofgem.LAF.SharedLibrary.Models.StatementOfIntentUpdateRequest request = new()
                {
                    StatementOfIntentId = result.StatementOfIntentId,
                    OnsCode = result.OnsCode,
                    LocalAuthorityId = result.LocalAuthorityId,
                    Status = result.Status,
                    Category = result.Category,

                    // updating with new values of initial assessment page
                    VersionNumber = SoiVersion,
                    PublishedDate = DatePublished,
                    StatementOfIntentLink = SoiLink,
                    CanSubmit = result.CanSubmit,
                    DesignatedLas = []
                };

                if (CanSubmitOnBehalfOfOtherAuthorities)
                {
                    // updating already added LAs
                    result.DesignatedLas = [];

                    foreach (var la in SelectedLocalAuthorities)
                    {
                        request.DesignatedLas.Add(la.LocalAuthorityId);
                    }

                    request.CanSubmit = request.DesignatedLas.Count > 0;
                }
                else
                {
                    request.CanSubmit = false;
                }

                var httpResponseMessage =
                    await httpClient.PutAsJsonAsync(Services.LocalAuthorityApi.RouteUpdateSoiV2, request);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return true;
                }

                var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem is not { Detail: not null }) return false;

                DisplayMessage = problem.Detail;
                ShowNotification = true;
                Logger.LogInformation(message: "{PageName} -  OnPostSave - {DisplayMessage}", PageName, DisplayMessage);

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{PageName} -  SaveData, Error", PageName);
                Message = $"An issue occurred retrieving the data local authority by onsCode {statementOfIntentId}.";
                throw;
            }
        }
    }
}
