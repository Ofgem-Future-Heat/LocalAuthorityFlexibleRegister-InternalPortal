using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using System.Globalization;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class Add(
        ILogger<Add> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : SoiPage(logger, httpClientFactory, httpContextAccessor)
    {
        [BindProperty] public Extensions.Permissions.SoiV2.Add Permissions { get; set; } = new();

        protected override string PageName => "Add";

        public async Task OnGet(string onsCode)
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            DatePublishedNullableControl = new Models.ThreePartDateNullable(
                source: null,
                title: "Date of SOI publication",
                titleToBeUsedInErrorMessage: "Statement of Intent was published",
                firstHint: "Please ensure accurate date. After adding a SoI version, the date will not be editable once a declaration notification has been uploaded referencing this SoI.",
                secondHint: "For example, 25 2 2024",
                showTheHighlightBar: true);

            await RefreshData(onsCode);

            LocalAuthorities = await GetLocalAuthoritiesAsync();

            LocalAuthorities.RemoveAll(x => x.Name == Name); // my local authority

            TempData.Put(LOCAL_AUTHORITY_KEY, LocalAuthorities);

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }

            if (DatePublishedNullableControl.Day.HasValue && DatePublishedNullableControl.Month.HasValue && DatePublishedNullableControl.Year.HasValue)
            {
                DatePublished = new DateTime(DatePublishedNullableControl.Year.Value,
                                                DatePublishedNullableControl.Month.Value,
                                                    DatePublishedNullableControl.Day.Value, 0, 0, 0, DateTimeKind.Utc);
            }

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;
        }

        public async Task OnPostAddInLa()
        {
            Logger.LogInformation("{PageName} -  OnPostAddInLa", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            SoiStatus = SoiStatusV2.ToBeAssessed;
            SoiCategory = SoiCategory.Incomplete;

            await Task.Run(AddInLa);

            FocusControlId = "local-authority";
        }

        public async Task OnPostTakeOutLa(string id)
        {
            Logger.LogInformation("{PageName} -  OnPostTakeOutLa", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            SoiStatus = SoiStatusV2.ToBeAssessed;
            SoiCategory = SoiCategory.Incomplete;

            await Task.Run(() => { TakeOutLa(id); });

            FocusControlId = "local-authority";
        }

        public async Task OnPostCanSubmitOnBehalfOf()
        {
            Logger.LogInformation("{PageName} -  OnPostCanSubmitOnBehalfOf", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetCanSubmitOnBehalfOf);
            ValidateAddSoiDate();
        }

        public async Task OnPostCannotSubmitOnBehalfOf()
        {
            Logger.LogInformation("{PageName} -  OnPostCannotSubmitOnBehalfOf", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            await Task.Run(SetCannotSubmitOnBehalfOf);
            ValidateAddSoiDate();
        }

        public async Task<IActionResult> OnPostAdd()
        {
            Logger.LogInformation("{PageName} -  OnPostAdd", PageName);

            Permissions = new Extensions.Permissions.SoiV2.Add(HttpContextAccessor, HttpClientFactory, TempData);

            RefreshFromTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;
            SoiStatus = SoiStatusV2.ToBeAssessed;
            SoiCategory = SoiCategory.Incomplete;

            ValidBasePage();
            ValidateAddSoiDate();

            if (DisplayPageErrors.Count > 0) return Page();

            Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest request = new()
            {
                OnsCode = OnsCode,
                LocalAuthorityId = new Guid(LocalAuthorityId),
                PublishedDate = DatePublished,
                Status = SoiStatus,
                VersionNumber = SoiVersion,
                Category = SoiCategory,
                CanSubmit = false,
                StatementOfIntentLink = SoiLink,
                DesignatedLas = []
            };

            if (CanSubmitOnBehalfOfOtherAuthorities)
            {
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

            var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteCreateSoiV2, request);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

                if (result != null)
                {

                    var soiLogModel = new Ofgem.LAF.SharedLibrary.Models.StatementOfIntentLog
                    {
                        StatementOfIntentId = result.StatementOfIntentId,
                        LocalAuthorityName = Name,
                        CreatedDate = DateTime.Now,
                        Comment = $"New SOI {result.VersionNumber} added"
                    };

                    HttpResponseMessage httpSoiLogResponseMessage =
                        await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteAddSoiLog, soiLogModel);

                    if (!httpSoiLogResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.LogInformation("{PageName} -  Unable to save add soi log", PageName);
                        ProblemDetails? problem = await httpSoiLogResponseMessage.Content
                            .ReadFromJsonAsync<ProblemDetails>();

                        if (problem != null)
                        {
                            ShowNotification = true;

                            if (problem.Detail != null)
                            {
                                DisplayMessage = problem.Detail;
                            }
                        }
                    }

                    return RedirectToPage(LafPages.SoiV2.AddConfirmation.ROUTE,
                        LafPages.SoiV2.AddConfirmation.METHOD_GET_BY_ID,
                        new
                        {
                            statementOfIntentId = result.StatementOfIntentId,
                        });
                }
            }
            else
            {
                var problem = await httpResponseMessage.Content
                    .ReadFromJsonAsync<ProblemDetails>();

                if (problem != null)
                {
                    ShowNotification = true;

                    if (problem.Detail != null)
                    {
                        DisplayMessage = problem.Detail;

                        DisplayErrors(DisplayMessage, "");
                    }
                }

                return Page();
            }


            return RedirectToPage(
                LafPages.ProfileV2.ROUTE,
                LafPages.ProfileV2.METHOD_FAILED_SOI_CREATE,
                new { onsCode = OnsCode });
        }

        private async Task RefreshData(string onsCode)
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteGetByOnsCode + $"{onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                    if (result != null)
                    {
                        LocalAuthorityId = result.LocalAuthorityId.ToString();
                        OnsCode = result.OnsCode;
                        Name = result.Name!;
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

        public bool ValidateAddSoiDate()
        {
            if (!ValidateNewDatePublishedControl())
            {
                return false;
            }
            return true;
        }

        internal bool ValidateNewDatePublishedControl()
        {
            if (DatePublishedNullableControl != null && DatePublishedNullableControl.HasErrors())
            {
                DisplayErrors(DatePublishedNullableControl.ErrorMessage, "day");
                StoreInTempData();
                return false;
            }

            if (DatePublishedNullableControl != null)
            {
                if (DatePublishedNullableControl.Year.HasValue && DatePublishedNullableControl.Month.HasValue && DatePublishedNullableControl.Day.HasValue)
                {
                    DatePublished = new DateTime(DatePublishedNullableControl.Year.Value,
                                                    DatePublishedNullableControl.Month.Value,
                                                        DatePublishedNullableControl.Day.Value, 0, 0, 0, DateTimeKind.Utc);

                    if (DatePublished < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
                    {
                        var message = "Enter a date from April 1 2022 up to the present date";
                        DisplayErrors(message, "day");
                        DatePublishedNullableControl.HasError = true;
                        DatePublishedNullableControl.HasYearError = true;
                        DatePublishedNullableControl.ErrorMessage = message;
                        StoreInTempData();
                        return false;
                    }

                    if (DatePublished > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
                    {
                        var message = $"Date published field cannot be after {DateTime.Now:dd/MM/yyyy}";
                        DisplayErrors(message, "day");
                        DatePublishedNullableControl.HasError = true;
                        DatePublishedNullableControl.ErrorMessage = message;
                        StoreInTempData();
                        return false;
                    }
                }
                else
                {
                    // Handle the case where any of the date parts are null
                    var message = "Date published field is incomplete";
                    DisplayErrors(message, "day");
                    DatePublishedNullableControl.HasError = true;
                    DatePublishedNullableControl.ErrorMessage = message;
                    StoreInTempData();
                    return false;
                }
            }

            return true;
        }
    }
}
