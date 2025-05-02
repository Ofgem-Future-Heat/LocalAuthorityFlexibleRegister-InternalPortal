using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
        )]
    public class ExternalUsersModel(
        ILogger<ExternalUsersModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Models.FilterExternalUsers Filter { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public List<Ofgem.LAF.SharedLibrary.Models.User> ExternalUsers { get; set; } = [];

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.ExternalUsers Permissions { get; set; } = new();

        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();
        [BindProperty] public int CurrentPage { get; set; }


        public async Task OnGet()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnGet");

            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();
        }

        public async Task OnGetCreateSuccess(string name)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "OnGetCreateSuccess");

            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();

            DisplayMessage = $"Created external user - {name}";
        }

        public async Task OnGetUpdateSuccess(string name)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "OnGetUpdateSuccess");

            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();

            DisplayMessage = $"Updated external user - {name}";
        }



        public async Task OnPostApplyPagination(string id)
        {
            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);

            SetPageIndexes(id);

            await FilterData();
        }

        private void SetPageIndexes(string id)
        {
            var isNumeric = int.TryParse(id, out int index);

            if (isNumeric)
            {
                if (index == Models.PagePagination.PREVIOUS_PAGE_VALUE)
                {
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex--;
                    CurrentPage = Filter.PageIndex;
                }
                else
                {
                    if (index == Models.PagePagination.NEXT_PAGE_VALUE)
                    {
                        Filter.PageIndex = CurrentPage;
                        Filter.PageIndex++;
                        CurrentPage = Filter.PageIndex;
                    }
                    else
                    {
                        Filter.PageIndex = index;
                        CurrentPage = index;
                    }
                }
            }
        }

        public async Task OnPostApplyFilters()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnPostApplyFilters");

            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();
        }

        public async Task OnPostClearFilters()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnPostClearFilters");

            Permissions = new Extensions.Permissions.ExternalUsers(httpContextAccessor, httpClientFactory, TempData);
            Filter.Filter = string.Empty;

            await RefreshData();
        }

        private async Task RefreshData()
        {
            CurrentPage = 1;
            Filter.PageIndex = 1;

            await FilterData();
        }

        private async Task FilterData()
        {
            var httpClient = httpClientFactory.CreateClient(Services.UserApi.ApiName);

            try
            {
                var externalUsersFilterRequest = new Models.FilterExternalUsers()
                {
                    RecordsPerPage = Filter.RecordsPerPage,
                    PageIndex = Filter.PageIndex,
                    Filter = Filter.Filter
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.UserApi.RouteGetFilteredExternalUser, externalUsersFilterRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.User>>();

                    if (result != null)
                    {
                        ExternalUsers = [.. result.Results];

                        CreatePagination(result.RowCount, result.CurrentPage, result.PageSize, result.PageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers,
                    "ExternalUsers - FilterData, Error {Filter}, Message {Message}", Filter, ex.Message);
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private void CreatePagination(int rowCount, int currentPage, int pageSize, int resultPageCount)
        {
            Filter.PageIndex = currentPage;
            Filter.RecordsPerPage = pageSize.ToString();

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }
    }
}