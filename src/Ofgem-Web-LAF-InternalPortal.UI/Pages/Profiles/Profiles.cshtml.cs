using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Profiles
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard,
        UserRoles.Basic
        )]
    public class ProfilesModel(
        ILogger<ProfilesModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Models.ProfilesFilter Filter { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public List<Models.LocalAuthority> LocalAuthorities { get; set; } = [];

        public List<string> LocalAuthorityNames { get; set; } = [];

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.Profiles Permissions { get; set; } = new();

        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();
        [BindProperty] public int CurrentPage { get; set; }


        public async Task OnGet()
        {
            logger.LogInformation("Profiles - OnGet");

            Permissions = new Extensions.Permissions.Profiles(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();
        }

        public async Task OnGetSuccessfulCreate(string name)
        {
            logger.LogInformation("Profiles - OnPostSuccessfulCreate");
            Permissions = new Extensions.Permissions.Profiles(httpContextAccessor, httpClientFactory, TempData);

            DisplayMessage = $"New profile for {name} successfully created.";

            ShowNotification = true;

            await RefreshData();
        }

        public async Task OnPostApplyPagination(string id)
        {
            Permissions = new Extensions.Permissions.Profiles(httpContextAccessor, httpClientFactory, TempData);

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
            logger.LogInformation("Profiles - OnPostApplyFilters");

            Permissions = new Extensions.Permissions.Profiles(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();

        }

        public async Task GetAutocompleteAsync()
        {
            logger.LogInformation("Profiles - OnGetAutocompleteAsync");

            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            try
            {
                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.Route);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = httpResponseMessage.Content
                        .ReadFromJsonAsync<IEnumerable<LocalAuthority>>();

                    if (result != null)
                    {
                        foreach (var localAuthority in result.Result)
                        {
                            LocalAuthorityNames.Add(localAuthority.Name);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting autocomplete suggestions");
            }
        }

        public async Task OnPostClearFilters()
        {
            logger.LogInformation("Profiles - OnPostClearFilters");

            Permissions = new Extensions.Permissions.Profiles(httpContextAccessor, httpClientFactory, TempData);
            this.Filter.Filter = string.Empty;
            await RefreshData();
        }

        private async Task RefreshData()
        {
            Filter.RecordsPerPage = "25";
            CurrentPage = 1;
            Filter.PageIndex = 1;

            await FilterData();
            await GetAutocompleteAsync();
        }

        private async Task FilterData()
        {
            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            try
            {
                var profilesFilterRequest = new ProfilesFilter
                {
                    IncludeSoi = true,
                    RecordsPerPage = Convert.ToInt32(Filter.RecordsPerPage),
                    PageIndex = Filter.PageIndex,
                    Filter = Filter.Filter!
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteGetFiltered, profilesFilterRequest);
                
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResult<LocalAuthority>>();

                    if (result != null)
                    {
                        LocalAuthorities = [];
                        foreach (var item in result.Results)
                        {
                            var localAuthority = new Models.LocalAuthority
                            {
                                Email = item.Email,
                                LocalAuthorityId = item.LocalAuthorityId,
                                Name = item.Name,
                                OnsCode = item.OnsCode,
                                Status = item.StatementOfIntents!.MaxBy(x => x.PublishedDate)?.Status ?? 0
                            };

                            LocalAuthorities.Add(localAuthority);
                        }

                        CreatePagination(result.RowCount, result.CurrentPage, result.PageSize, result.PageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "Profiles - FilterData, Error");
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