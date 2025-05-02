using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions.Permissions.SoiV2;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;
using Ofgem.LAF.SharedLibrary.Enums;
using LocalAuthority = Ofgem.LAF.SharedLibrary.Models.LocalAuthority;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [BindProperties]
    public class EditDesignatedLasModel(
        ILogger<EditDesignatedLasModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        Services.ISoiService soiService) : PageModel
    {

        [BindProperty] public EditDesignatedLas Permissions { get; set; } = new();

        protected string PageName => "EditDesignatedLasModel";

        internal const string LocalAuthorityKey = "EditDesignatedLas";

        internal const string SelectedLocalAuthorityKey = "DesignatedSelectedlas";

        [BindProperty(SupportsGet = true)] public string Name { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string OnsCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string LocalAuthorityId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public string StatementOfIntentId { get; set; } = string.Empty;

        [BindProperty] public string SoiVersion { get; set; } = string.Empty;

        [BindProperty] public DateTime DatePublished { get; set; }

        [BindProperty] public string? SoiLink { get; set; } = string.Empty;

        [BindProperty] public ThreePartDate? DatePublishedControl { get; set; }

        [BindProperty] public SoiStatusV2 SoiStatus { get; set; }

        [BindProperty] public SoiCategory SoiCategory { get; set; }

        [BindProperty] public required string SelectedLa { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)] public bool CanSubmitOnBehalfOfOtherAuthorities { get; set; }

        public const string ErrorMessage =
            "An issue occurred retrieving the data for local authority by onsCode";

        [BindProperty] public required string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public bool ShowNotification => HasDisplayMessage;

        [BindProperty] public required string SelectedLocalAuthorityId { get; set; } = string.Empty;

        [BindProperty]
        public required List<LocalAuthority> LocalAuthorities { get; set; } = [];

        [BindProperty]
        public List<SelectedLocalAuthority> SelectedLocalAuthorities { get; set; } = [];

        [BindProperty] public required List<SelectListItem> LaSelectList { get; set; } = [];


        public async Task OnGet(string onsCode, string version, string publishedDate)
        {
            logger.LogInformation("{PageName} -  OnGet", PageName);

            Permissions = new EditDesignatedLas(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(onsCode, version, publishedDate);

            LocalAuthorities = await GetLocalAuthoritiesAsync();

            LocalAuthorities.RemoveAll(x => x.Name == Name); // my local authority

            TempData.Put(LocalAuthorityKey, LocalAuthorities);

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

        private async Task RefreshData(string onsCode, string version, string publishedDate)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(LocalAuthorityApi.RouteOnsExists + $"{onsCode}")
                    ?? throw new ArgumentNullException(ErrorMessage + $" {onsCode}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<LocalAuthority>();

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
                            DisplayMessage =
                                $"Unable to locate the version: {version}, published date: {publishedDate}";
                            Permissions = new EditDesignatedLas { CanUpdate = false };
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

                            DatePublishedControl = new ThreePartDate(
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
                                    if (designatedLa.LocalAuthority != null)
                                    {
                                        var localAuthorityModel = new SelectedLocalAuthority
                                        {
                                            Name = designatedLa.LocalAuthority.Name,
                                            OnsCode = designatedLa.LocalAuthority.OnsCode,
                                            LocalAuthorityId = designatedLa.LocalAuthority.LocalAuthorityId,
                                            Email = designatedLa.LocalAuthority.Email,
                                            DesignatedLas = designatedLa.LocalAuthority.DesignatedLas,
                                            StatementOfIntents = designatedLa.LocalAuthority.StatementOfIntents,
                                            IsEditable = false,
                                        };

                                        SelectedLocalAuthorities.Add(localAuthorityModel);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("{PageName} -  Unable to get statement of intent", PageName);

                        DisplayMessage = ErrorMessage + $" {onsCode}.";
                    }

                    TempData.Keep();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{PageName} -  RefreshData, Error", PageName);
                DisplayMessage = ErrorMessage + $" {onsCode}.";
                throw;
            }
        }

        public async Task<IActionResult> OnPostUpdateLa()
        {
            logger.LogInformation("{PageName} -  OnPostUpdateLA", PageName);

            Permissions = new EditDesignatedLas(httpContextAccessor, httpClientFactory, TempData);

            RefreshFromTempData();

            if (!ValidPage())
            {
                return Page();
            }

            try
            {
                var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpGetResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteSoiById + $"{StatementOfIntentId}");

                if (!httpGetResponseMessage.IsSuccessStatusCode)
                {
                    logger.LogInformation("{PageName} -  Unable to update Statement of Intent", PageName);
                    ProblemDetails? getSoiProblem = await httpGetResponseMessage.Content
                        .ReadFromJsonAsync<ProblemDetails>();

                    if (getSoiProblem is { Detail: not null })
                    {
                        DisplayMessage = getSoiProblem.Detail;
                    }

                    return Page();
                }


                var result = await httpGetResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

                if (result != null)
                {
                    Ofgem.LAF.SharedLibrary.Models.StatementOfIntentUpdateRequest request = new()
                    {
                        StatementOfIntentId = result.StatementOfIntentId,
                        OnsCode = result.OnsCode,
                        LocalAuthorityId = result.LocalAuthorityId,
                        Status = result.Status,
                        Category = result.Category,

                        // updating with new values of initial assessment page
                        VersionNumber = result.VersionNumber,
                        PublishedDate = result.PublishedDate,
                        StatementOfIntentLink = result.StatementOfIntentLink,
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
                        DisplayMessage = "Local authorities are updated successfully.";

                        if (SoiStatus is
                            SoiStatusV2.PassedAssessment or
                            SoiStatusV2.Withdrawn or
                            SoiStatusV2.FailedAssessment)
                        {
                            await soiService.RerunRules(
                                OnsCode,
                                Name,
                                SoiVersion,
                                SoiStatus,
                                DatePublished.Year,
                                DatePublished.Month,
                                DatePublished.Day);
                        }

                        return RedirectToPage(LafPages.SoiV2.SoiDetails.ROUTE,
                            LafPages.SoiV2.SoiDetails.METHOD_GET_BY_ID,
                            new
                            {
                                statementOfIntentId = new Guid(StatementOfIntentId)
                            });
                    }

                    var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
                    if (problem is not null && problem.Detail is not null)
                    {
                        DisplayMessage = problem.Detail;
                    }

                    logger.LogInformation(message: "{PageName} -  OnPostSave - {DisplayMessage}", PageName,
                        DisplayMessage);

                    return Page();
                }

                logger.LogInformation("{PageName} -  Unable to get statement of intent", PageName);

                DisplayMessage = ErrorMessage + $" {OnsCode}.";

                return Page();
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "{PageName} -  SaveData, Error", PageName);
                DisplayMessage = ErrorMessage + $" {OnsCode}.";
                throw;
            }
        }

        public Task OnPostAddInLa()
        {
            logger.LogInformation("{PageName} -  OnPostAddInLa", PageName);

            Permissions = new EditDesignatedLas(httpContextAccessor, httpClientFactory, TempData);

            RefreshFromTempData();

            LocalAuthority? la =
                LocalAuthorities.SingleOrDefault(x => x.LocalAuthorityId == new Guid(SelectedLa));


            if (la != null)
            {
                var addNewLa = new SelectedLocalAuthority
                {
                    Name = la.Name,
                    OnsCode = la.OnsCode,
                    LocalAuthorityId = la.LocalAuthorityId,
                    Email = la.Email,
                    DesignatedLas = la.DesignatedLas,
                    StatementOfIntents = la.StatementOfIntents,
                    IsEditable = true,
                };

                SelectedLocalAuthorities.Add(addNewLa);

                if (!string.IsNullOrEmpty(la.Name)) LocalAuthorities.RemoveAll(x => x.Name == la.Name);
            }

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }

            StoreInTempData();

            CreateLocalAuthoritiesSelectList();

            SelectedLa = string.Empty;
            return Task.CompletedTask;
        }

        public Task OnPostTakeOutLa(string id)
        {
            logger.LogInformation("{PageName} -  OnPostTakeOutLa", PageName);

            Permissions = new EditDesignatedLas(httpContextAccessor, httpClientFactory, TempData);

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
            return Task.CompletedTask;
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
            LocalAuthorities = TempData.Get<List<LocalAuthority>>(LocalAuthorityKey);

            SelectedLocalAuthorities =
                TempData.Get<List<SelectedLocalAuthority>>(SelectedLocalAuthorityKey);

            if (LocalAuthorities.Count > 0)
            {
                SelectedLocalAuthorityId = LocalAuthorities[0].LocalAuthorityId.ToString();
            }
        }

        internal void StoreInTempData()
        {
            TempData.Put(LocalAuthorityKey, LocalAuthorities);
            TempData.Put(SelectedLocalAuthorityKey, SelectedLocalAuthorities);
        }


        internal async Task<List<LocalAuthority>> GetLocalAuthoritiesAsync()
        {
            try
            {
                List<LocalAuthority>? result = [];

                HttpClient httpClient = httpClientFactory.CreateClient(LocalAuthorityApi.ApiName);

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(LocalAuthorityApi.Route);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    result =
                        await httpResponseMessage.Content.ReadFromJsonAsync<List<LocalAuthority>>();
                }

                return result ?? [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{PageName} - GetLocalAuthoritiesAsync, Error", "EditDesignatedLAPage");
                DisplayMessage = ErrorMessage + $" {OnsCode}.";
                throw;
            }
        }
        public bool ValidPage()
        {
            if (LocalAuthorities.Count == 0)
            {
                DisplayMessage = "An issue occurred retrieving the local authority data";
                logger.LogLafError(LogEvents.GetLocalAuthorities, $"GetLocalAuthoritiesAsync, Error {DisplayMessage}");
                return false;
            }

            if (CanSubmitOnBehalfOfOtherAuthorities && SelectedLocalAuthorities.Count == 0)
            {
                DisplayMessage = "Select a local authority.";
                return false;
            }

            return true;
        }


        public class SelectedLocalAuthority : LocalAuthority
        {
            public bool IsEditable { get; set; }
        }
    }
}
