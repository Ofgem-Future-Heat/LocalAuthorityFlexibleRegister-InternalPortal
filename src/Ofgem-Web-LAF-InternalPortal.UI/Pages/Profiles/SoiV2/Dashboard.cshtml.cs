using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem_Web_LAF_InternalPortal.Services;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles.SoiV2
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]

    public class Dashboard(
        ILogger<Dashboard> logger,
        IHttpContextAccessor httpContextAccessor,
         IHttpClientFactory httpClientFactory,
        ILaManagementService laManagementService) : PageModel
    {

        [BindProperty] public Ofgem.LAF.SharedLibrary.Enums.ExternalUserType ExternalUserType { get; set; }

        [BindProperty] public List<StatementOfIntentV2>? StatementOfIntents { get; set; }

        [BindProperty] public Models.SoiFilter Filter { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public int StatementOfIntentsCount { get; set; }

        [BindProperty] public string StatementOfIntentsDataDescription { get; set; } = string.Empty;

        [BindProperty] public Extensions.Permissions.SoiDashboard Permissions { get; set; } = new();

        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();

        [BindProperty] public int CurrentPage { get; set; }
        [BindProperty] public int TotalRecords { get; set; }

        public async Task OnGet()
        {
            logger.LogLafInformation(LogEvents.GetSoi);

            Permissions = new Extensions.Permissions.SoiDashboard(httpContextAccessor, httpClientFactory, TempData);

            TempData.Remove(TempDataKeys.StatementOfIntentFilterData);

            SetupFilterData();

            await FilterData();
        }

        public async Task OnPostApplyFilters()
        {
            logger.LogLafInformation(LogEvents.GetSoi, "StatementOfIntentList - OnPostApplyFilters");

            Permissions = new Extensions.Permissions.SoiDashboard(httpContextAccessor, httpClientFactory, TempData);

            SetFilterDefaults();

            await FilterData();
        }

        public Task<IActionResult> OnPostClearFilters()
        {
            logger.LogLafInformation(LogEvents.GetSoi, "StatementOfIntentList - OnPostClearFilters");

            // clear any stored filter detail
            TempData.Remove(TempDataKeys.StatementOfIntentFilterData);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SoiV2.Dashboard.ROUTE));
        }

        private void SetupFilterData()
        {
            var localFilter = TempData.Get<Models.SoiFilter>(TempDataKeys.StatementOfIntentFilterData);

            if (localFilter is null)
            {
                SetFilterDefaults();
                ResetFilterStatus();
            }
            else
            {
                TempData.Put(TempDataKeys.StatementOfIntentFilterData, localFilter);
                Filter = localFilter;
                CurrentPage = Filter.PageIndex;
            }
        }

        private void SetFilterDefaults()
        {
            CurrentPage = 1;
            Filter.PageIndex = 1;
        }

        private void ResetFilterStatus()
        {
            Filter.LocalAuthority = string.Empty;

            // reset the filter status as this is the 1st time on the page we want to show all status
            // but the UI needs to have the shown as checked
            Filter.IsPassedAssessment = false;
            Filter.IsToBeAssessed = false;
            Filter.IsFailedAssessment = false;
            Filter.IsAwaitingSignOff = false;
        }

        private async Task FilterData()
        {
            try
            {
                List<Models.LocalAuthority> laProfileList = [];

                if (!string.IsNullOrWhiteSpace(Filter.LocalAuthority))
                {

                    var laProfileFilterRequest = new Ofgem.LAF.SharedLibrary.Models.ProfilesFilter()
                    {
                        IncludeSoi = true,
                        RecordsPerPage = 25,
                        PageIndex = Filter.PageIndex,
                        Filter = Filter.LocalAuthority!
                    };

                    laProfileList = await laManagementService.GetSoiLaAsync(laProfileFilterRequest);
                }


                var soiFilterRequest = new Ofgem.LAF.SharedLibrary.Models.SoiFilter
                {
                    LAs = GetSelectedLaList(laProfileList).ToArray(),
                    Status = GetSelectedStatusCodes().ToArray(),
                    RecordsPerPage = Constants.SOI_PAGE_SIZE,
                    PageIndex = Filter.PageIndex,
                };

                var result = await laManagementService.GetPagedSoiListAsync(soiFilterRequest);

                StatementOfIntents = result.StatementOfIntents.ToList();

                StatementOfIntentsCount = StatementOfIntents.Count;

                StatementOfIntentsDataDescription = "Statement of Intents shown in SOI status order";

                CreatePagination(result.RowCount, result.CurrentPage, result.PageCount);

                TotalRecords = result.RowCount;

                // store these in case the next post has validation errors, we will need these values
                TempData.Put(TempDataKeys.StatementOfIntentFilterData, Filter);
                TempData.Put(TempDataKeys.StatementOfIntentCount, new Tuple<int>(TotalRecords));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SOI Dashboard - FilterData");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private void CreatePagination(int rowCount, int currentPage, int resultPageCount)
        {
            Filter.PageIndex = currentPage;

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }

        private List<Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2> GetSelectedStatusCodes()
        {
            List<Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2> selectedStatus = [];

            if (Filter is { IsPassedAssessment: false, IsToBeAssessed: false, IsAwaitingSignOff: false, IsFailedAssessment: false })
            {
                // none are selected.
                // so we want to get ALL the Statuses
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.ToBeAssessed);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment);
            }
            else
            {
                if (Filter.IsToBeAssessed)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.ToBeAssessed);
                }
                if (Filter.IsFailedAssessment)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment);
                }
                if (Filter.IsAwaitingSignOff)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff);
                }
                if (Filter.IsPassedAssessment)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment);
                }
            }

            return selectedStatus;
        }

        private static List<string> GetSelectedLaList(List<LocalAuthority>? localAuthorities)
        {
            List<string> selectedLaItems = [];

            if (localAuthorities?.Count == 0) return [];

            selectedLaItems.AddRange(from listItem in localAuthorities ?? []
                                     where listItem.OnsCode != null
                                     select listItem.OnsCode);
            return selectedLaItems;
        }

        public async Task OnPostApplyPagination(string id)
        {
            logger.LogLafInformation(LogEvents.GetSoi);

            Permissions = new Extensions.Permissions.SoiDashboard(httpContextAccessor, httpClientFactory, TempData);

            SetPageIndexes(id);

            await FilterData();
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
    }
}
