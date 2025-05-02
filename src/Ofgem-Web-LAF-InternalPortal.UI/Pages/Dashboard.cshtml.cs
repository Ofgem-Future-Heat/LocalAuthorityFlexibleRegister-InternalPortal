using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using System.Globalization;
using Ofgem_Web_LAF_InternalPortal.Services;
using System.Text;
using System.Text.Json;


namespace Ofgem_Web_LAF_InternalPortal.Pages
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]
    public class DashboardModel(
        IDeclarationManagementService declarationManagementService,
        IUserService userService,
        ILogger<DashboardModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty] public Models.DeclarationFilter Filter { get; set; } = new();

        [BindProperty] public List<Models.Declaration>? Declarations { get; set; }

        [BindProperty] public int DeclarationCount { get; set; }
        [BindProperty] public int DeclarationErrorCount { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public string UploadDetailedInformation { get; set; } = string.Empty;
        [BindProperty] public bool ShowNotification { get; set; }
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;
        [BindProperty] public bool HasProcessingDeclarationsMessage { get; set; }

        [BindProperty] public bool HasSoiCountMessage { get; set; }
        [BindProperty] public int SoiCount { get; set; }

        [BindProperty] public bool HasAwaitingDecisionDeclarationsMessage { get; set; }
        [BindProperty] public Extensions.Permissions.Dashboard Permissions { get; set; } = new();
        [BindProperty] public string DeclarationsDataDescription { get; set; } = string.Empty;
        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();
        [BindProperty] public int CurrentPage { get; set; }
        [BindProperty] public int TotalRecords { get; set; }

        // Date pickers
        [BindProperty] public List<SelectListItem> DateDays { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateMonths { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateFromYears { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateToYears { get; set; } = [];

        [BindProperty] public List<Models.AnnouncementView> PublishedAnnouncements { get; set; } = [];

        private const string ExcludedAnnouncementIdsKey = "ExcludedAnnouncementIds";

        public async Task OnGet()
        {
            logger.LogInformation("Dashboard - OnGet");

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            await GetSoiCountNotification();

            await GetUploadStatusNotification();

            await GetUploadAwaitingDecisionStatusNotification();

            await GetPublishedAnnouncements();

            SetupFilterData();

            await FilterData();
        }

        public async Task<IActionResult> OnGetResolveUpload()
        {
            logger.LogInformation("Dashboard - OnGetResolveUpload");

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            return await Task.FromResult<IActionResult>(RedirectToPage(LafPages.ResolveUploads.ROUTE));
        }

        public async Task OnGetSuccessfulUpload(int declarationCount, int declarationErrorCount,
            string detailedUploadMessage = "",
            string displayMessage = "")
        {
            logger.LogInformation("Dashboard - OnGetSuccessfulUpload");

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            await GetSoiCountNotification();

            await GetUploadStatusNotification();

            await GetUploadAwaitingDecisionStatusNotification();

            await GetPublishedAnnouncements();

            DeclarationCount = declarationCount;
            UploadDetailedInformation = detailedUploadMessage;
            DeclarationErrorCount = declarationErrorCount;

            ShowNotification = true;

            DisplayMessage = displayMessage;

            // clear any stored filter detail
            TempData.Remove(TempDataKeys.DashboardFilterData);

            SetupFilterData();

            await FilterData();
        }

        public async Task OnGetHideAnnouncement(Guid id)
        {
            var excludedAnnouncementIdsBytes = HttpContext.Session.Get(ExcludedAnnouncementIdsKey);
            var excludesAnnouncementIdsJson = excludedAnnouncementIdsBytes == null || excludedAnnouncementIdsBytes.Length == 0 ? null : Encoding.ASCII.GetString(excludedAnnouncementIdsBytes);
            var excludedAnnouncementIds = (excludesAnnouncementIdsJson == null ? [] : JsonSerializer.Deserialize<HashSet<Guid>>(excludesAnnouncementIdsJson)) ?? [];

            excludedAnnouncementIds.Add(id);

            HttpContext.Session.Set(ExcludedAnnouncementIdsKey, Encoding.ASCII.GetBytes(JsonSerializer.Serialize(excludedAnnouncementIds)));

            await OnGet();
        }

        public async Task OnPostApplySorting()
        {
            logger.LogInformation("Dashboard - OnPostApplySorting");

            InitialiseDateDropdowns();

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            CurrentPage = 1;
            Filter.PageIndex = 1;

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
                Filter.LAs =
                    Filter.LAs == null
                        ? await CreateLaList([])
                        : await CreateLaList(Filter.LAs);
            }
        }

        public async Task OnPostApplyFilters()
        {
            logger.LogInformation("Dashboard - OnPostApplyFilters");

            InitialiseDateDropdowns();

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            CurrentPage = 1;
            Filter.PageIndex = 1;

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
                Filter.LAs =
                    Filter.LAs == null
                        ? await CreateLaList([])
                        : await CreateLaList(Filter.LAs);
            }
        }

        public async Task OnPostApplyPagination(string id)
        {
            logger.LogInformation("Dashboard - OnPostApplyFilters");

            InitialiseDateDropdowns();

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            SetPageIndexes(id);

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
                Filter.LAs =
                    Filter.LAs == null
                        ? await CreateLaList([])
                        : await CreateLaList(Filter.LAs);
            }
        }

        public Task<IActionResult> OnPostClearFilters()
        {
            // clear any stored filter detail
            TempData.Remove(TempDataKeys.DashboardFilterData);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.Dashboard.ROUTE));
        }

        public async Task<IActionResult> OnPostDownloadSelectedAsync()
        {
            logger.LogInformation("Dashboard - OnPostApplyFilters");

            Permissions = new Extensions.Permissions.Dashboard(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = "File downloaded";
            ShowNotification = true;

            var results
                = await GetDownloadData(
                    (
                        from declaration in Declarations
                        where declaration.Selected
                        select declaration.Urn
                    )
                    .ToList());

            var ms = new MemoryStream();

            using (var parser = new ChoCSVWriter<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>(ms))
            {
                parser.Write(results);
            }

            ms.Position = 0; //reset stream

            return File(ms, "text/csv", "Dashboard_Download.csv");
        }



        private void SetupFilterData()
        {
            InitialiseDateDropdowns();

            var localFilter = TempData.Get<Models.DeclarationFilter>(TempDataKeys.DashboardFilterData);

            if (localFilter == null)
            {
                SetFilterDefaults();
                SetFromDateFieldValue();
                ResetFilterRoutes();
            }
            else
            {
                TempData.Put(TempDataKeys.DashboardFilterData, localFilter);
                Filter = localFilter;
                CurrentPage = Filter.PageIndex;
            }
        }

        private void ResetFilterRoutes()
        {
            // reset the filter routes as this is the 1st time on the page we want to show all routes
            // but the UI needs to have the shown as unchecked
            Filter.Route1 = false;
            Filter.Route2 = false;
            Filter.Route3 = false;
            Filter.Route4 = false;
        }

        private void SetFromDateFieldValue()
        {
            var minimumDateTime = DateTime.Parse(Filter.DateFromMin, new CultureInfo("en-GB"));

            var dateFrom = DateTime.Today.AddYears(-1).AddDays(1);
            var dateTo = DateTime.Today;

            Filter.DateFromDay = dateFrom.Day;
            Filter.DateFromMonth = dateFrom.Month;
            Filter.DateFromYear = dateFrom.Year;

            Filter.DateToDay = dateTo.Day;
            Filter.DateToMonth = dateTo.Month;
            Filter.DateToYear = dateTo.Year;

            if (dateFrom < minimumDateTime)
            {
                Filter.DateFromDay = minimumDateTime.Day;
                Filter.DateFromMonth = minimumDateTime.Month;
                Filter.DateFromYear = minimumDateTime.Year;
            }

            var dateDiff = dateTo.Subtract(dateFrom);

            if (dateDiff.TotalDays > Models.DeclarationFilter.DateDaysDifference)
            {
                dateFrom = dateTo.AddYears(-1).AddDays(1);
                Filter.DateFromDay = dateFrom.Day;
                Filter.DateFromMonth = dateFrom.Month;
                Filter.DateFromYear = dateFrom.Year;
            }
        }


        private async Task<List<SelectListItem>> CreateLaList(List<SelectListItem> selectedList)
        {
            var laList = new List<SelectListItem>();

            try
            {
                var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.Route);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<IEnumerable<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();

                    if (result != null)
                    {
                        foreach (var localAuthority in result)
                        {
                            laList.Add(new SelectListItem()
                            { Text = localAuthority.Name, Value = localAuthority.OnsCode });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Dashboard - CreateLaList");
                DisplayMessage = "An issue occurred retrieving data";
            }

            if (selectedList.Count == 0) return laList;

            for (int i = 0; i < selectedList.Count; i++)
            {
                laList[i].Selected = selectedList[i].Selected;
            }

            return laList;
        }

        private void SetPageIndexes(string id)
        {
            var isNumeric = int.TryParse(id, out int index);

            if (!isNumeric) return;

            switch (index)
            {
                case Models.PagePagination.PREVIOUS_PAGE_VALUE:
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex--;
                    CurrentPage = Filter.PageIndex;
                    break;

                case Models.PagePagination.NEXT_PAGE_VALUE:
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex++;
                    CurrentPage = Filter.PageIndex;
                    break;

                default:
                    Filter.PageIndex = index;
                    CurrentPage = index;
                    break;
            }
        }

        private void SetFilterDefaults()
        {
            Filter.RecordsPerPageSelected = Filter.RecordsPerPage[0].Text;
            Filter.SortOrderId = ((int)Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.DateUploadedZ2A).ToString();

            CurrentPage = 1;
            Filter.PageIndex = 1;
        }

        private async Task FilterData()
        {
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {

                // update the selected items in the LA's
                Filter.LAs =
                    Filter.LAs == null
                        ? await CreateLaList([])
                        : await CreateLaList(Filter.LAs);

                var laLookup = Filter.LAs
                    .ToDictionary(x => x.Value, t => t.Text);
                
                var declarationFilterRequest = new Ofgem.LAF.SharedLibrary.Models.DeclarationFilter
                {
                    Urn = Filter.Urn,
                    UploadId = Filter.UploadId ?? 0,
                    DateFrom =
                        new DateTime(Filter.DateFromYear, Filter.DateFromMonth, Filter.DateFromDay)
                            .ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT),
                    DateTo =
                        new DateTime(Filter.DateToYear, Filter.DateToMonth, Filter.DateToDay).AddDays(1)
                            .ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT),
                    LAs = GetSelectedLaList(),
                    Status = GetSelectedStatusCodes(),
                    Route1 = Filter.Route1,
                    Route2 = Filter.Route2,
                    Route3 = Filter.Route3,
                    Route4 = Filter.Route4,
                    RecordsPerPage = Convert.ToInt32(Filter.RecordsPerPageSelected),
                    PageIndex = Filter.PageIndex,
                    SortId =
                        Filter.SortOrderId == null
                            ? 0
                            : (Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder)Enum.Parse(
                                typeof(Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder), Filter.SortOrderId)
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteGetFiltered,
                        declarationFilterRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content
                        .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<
                            Ofgem.LAF.SharedLibrary.Models.Declaration>>();

                    if (result != null)
                    {
                        Declarations = [];

                        foreach (var item in result.Results)
                        {
                            if (item == null)
                            {
                                result.RowCount = 0;
                                continue;
                            }

                            laLookup.TryGetValue(item.LAAreaCode ?? string.Empty, out var actualLocalAuthorityName);

                            var newDeclaration = new Models.Declaration
                            {
                                DeclarationId = item.DeclarationId,
                                Urn = item.Urn,
                                LocalAuthority = actualLocalAuthorityName,
                                CreatedDate = item.CreatedDate,
                                ECO4orGreatBritishInsulationSchemeFlexReferralRoute = item.ECO4orGreatBritishInsulationSchemeFlexReferralRoute,
                                Route2Proxies =
                                    item.Route2Proxies,
                                AdditionalRoute2Proxies = item.AdditionalRoute2Proxies,
                                Route4ApplicationNumber = item.Route4ApplicationNumber,
                                LAAreaCode = item.LAAreaCode,
                                Status = item.Status,
                                RecordStatus = (Ofgem.LAF.SharedLibrary.Enums.RecordStatus)(int)item.RecordStatus,
                                Selected = false,
                                UploadId = item.UploadId
                            };

                            Declarations.Add(newDeclaration);
                        }

                        DeclarationsDataDescription = "Declarations shown in submitted date ascending order";

                        CreatePagination(result.RowCount, result.CurrentPage, result.PageSize, result.PageCount);

                        TotalRecords = result.RowCount;
                    }
                }

                // store these in case the next post has validation errors, we will need these values
                TempData.Put(TempDataKeys.DashboardDeclarationData, Declarations);
                TempData.Put(TempDataKeys.DashboardFilterData, Filter);
                TempData.Put(TempDataKeys.DashboardDeclarationCount, new Tuple<int>(TotalRecords));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Dashboard - FilterData");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus[] GetSelectedStatusCodes()
        {
            List<Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus> selectedStatus = [];

            if (Filter.Accepted)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Accepted);
            if (Filter.NotAccepted)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Rejected);
            if (Filter.AwaitingSignOff)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.AwaitingSignOff);
            if (Filter.FailedCoreChecks)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedCoreChecks);
            if (Filter.FailedSoiChecks)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedSoiChecks);
            if (Filter.OnHold)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.OnHold);
            if (Filter.Withdrawn)
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Withdrawn);

            return [.. selectedStatus];
        }

        private string[] GetSelectedLaList()
        {
            List<string> selectedLaItems = [];

            if (Filter.LAs == null) return [];

            foreach (var listItem in Filter.LAs)
            {
                if (listItem.Selected)
                {
                    selectedLaItems.Add(listItem.Value);
                }
            }

            return [.. selectedLaItems];
        }

        private async Task<List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>?> GetDownloadData(List<string> items)
        {
            List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>? result = [];

            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteGetDownloadData, items);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>>();

                    if (result == null)
                    {
                        DisplayMessage = "Failed to down load";
                    }
                }

                // store these in case the next post has validation errors, we will need these values
                TempData.Put(TempDataKeys.DashboardDeclarationData, Declarations);
                TempData.Put(TempDataKeys.DashboardFilterData, Filter);
                TempData.Put(TempDataKeys.DashboardDeclarationCount, new Tuple<int>(TotalRecords));

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Dashboard - GetDownloadData");
                DisplayMessage = "An issue occurred retrieving data";
            }

            return [];
        }

        private void CreatePagination(int rowCount, int currentPage, int pageSize, int resultPageCount)
        {
            Filter.PageIndex = currentPage;
            Filter.RecordsPerPageSelected = pageSize.ToString();

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }

        private async Task GetUploadStatusNotification()
        {
            HasProcessingDeclarationsMessage = false;
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);
            try
            {
                var httpResponseMessage =
                    await httpClient.GetAsync(Services.DeclarationApi.RouteUploadsProcessing);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.Upload>>();

                    if (result is { Count: > 0 })
                    {
                        HasProcessingDeclarationsMessage = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Dashboard - GetUploadStatusNotification");
                DisplayMessage = "An issue occurred retrieving uploads processing status.";
            }
        }


        private async Task GetSoiCountNotification()
        {
            HasSoiCountMessage = false;
            SoiCount = 0;
            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);
            try
            {
                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteGetSoiCount + "/" + (int)Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.ToBeAssessed);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var count = await httpResponseMessage.Content.ReadFromJsonAsync<int>();

                    if (count > 0)
                    {
                        HasSoiCountMessage = true;
                        SoiCount = count;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Dashboard - GetUploadStatusNotification");
                DisplayMessage = "An issue occurred retrieving uploads processing status.";
            }
        }

        private async Task GetUploadAwaitingDecisionStatusNotification()
        {
            try
            {
                HasAwaitingDecisionDeclarationsMessage = await declarationManagementService.ThereAreUploadsAwaitingADecision();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard - GetUploadAwaitingDecisionStatusNotification");
                DisplayMessage = "An issue occurred retrieving uploads awaiting decision status.";
            }
        }

        private async Task GetPublishedAnnouncements()
        {
            if (!HttpContext.Session.TryGetValue(ExcludedAnnouncementIdsKey, out var excludedAnnouncementIdsBytes))
            {
                excludedAnnouncementIdsBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new HashSet<Guid>()));
                HttpContext.Session.Set(ExcludedAnnouncementIdsKey, excludedAnnouncementIdsBytes);
            }

            var excludedAnnouncementIds = JsonSerializer.Deserialize<HashSet<Guid>>(Encoding.ASCII.GetString(excludedAnnouncementIdsBytes))!;

            PublishedAnnouncements = (await userService.GetPublishedAnnouncementsAsync())
                .Where(x => !excludedAnnouncementIds.Contains(x.AnnouncementId))
                .ToList() ?? throw new InvalidOperationException("Unable to load the announcements data");
        }
        
        private void InitialiseDateDropdowns()
        {
            DateDays = Enumerable.Range(1, 31).Select(day => new SelectListItem
            {
                Value = day.ToString(),
                Text = day.ToString()
            }).ToList();

            DateMonths = Enumerable.Range(1, 12).Select(month => new SelectListItem
            {
                Value = month.ToString(),
                Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)[..3]
            }).ToList();

            DateFromYears = Enumerable.Range(2023, 5).Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            }).ToList();

            DateToYears = Enumerable.Range(2024, 5).Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            }).ToList();

        }

        private bool FilterDatesAreValid()
        {
            var ukCulture = new CultureInfo("en-GB");

            var dateFromValid = DateTime.TryParse($"{Filter.DateFromDay}/{Filter.DateFromMonth}/{Filter.DateFromYear}", ukCulture, DateTimeStyles.None, out var dateFrom);

            if (!dateFromValid)
            {
                Filter.DateErrorText = "From date is invalid";
                Filter.FromDateHasError = true;
                return false;
            }

            var dateToValid = DateTime.TryParse($"{Filter.DateToDay}/{Filter.DateToMonth}/{Filter.DateToYear}", ukCulture, DateTimeStyles.None, out var dateTo);

            if (!dateToValid)
            {
                Filter.DateErrorText = "To date is invalid";
                Filter.ToDateHasError = true;
                return false;
            }

            if (dateFrom >= dateTo)
            {
                Filter.DateErrorText = "From date must be earlier than the To date";
                Filter.DateHasError = true;
                return false;
            }

            var dateDiff = dateTo.Subtract(dateFrom);
            if (dateDiff.TotalDays > Models.DeclarationFilter.DateDaysDifference)
            {
                Filter.DateErrorText = "Invalid date. Ensure search is within a 1 year window.";
                Filter.DateHasError = true;
                return false;
            }

            return true;
        }
    }
}