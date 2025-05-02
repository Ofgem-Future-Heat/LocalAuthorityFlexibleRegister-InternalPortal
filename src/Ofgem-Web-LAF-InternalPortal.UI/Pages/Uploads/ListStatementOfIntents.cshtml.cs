using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using System.Net.Http;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Uploads
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard
    )]
    public class ListStatementOfIntentsModel(
        ILogger<ListStatementOfIntentsModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty] public Extensions.Permissions.LogsStatementOfIntents Permissions { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();

        [BindProperty(SupportsGet = true)]

        public List<StatementOfIntentLog> StatementOfIntentLogs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Models.ListUploadsFilter Filter { get; set; } = new();

        [BindProperty] public int CurrentPage { get; set; }

        public async Task OnGet()
        {
            logger.LogInformation("ListStatementOfIntents - OnGet");

            Permissions = new Extensions.Permissions.LogsStatementOfIntents(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData();
        }

        public async Task OnPostApplyPagination(string id)
        {
            Permissions = new Extensions.Permissions.LogsStatementOfIntents(httpContextAccessor, httpClientFactory, TempData);

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

        private async Task RefreshData()
        {
            Filter.RecordsPerPageSelected = "25";
            CurrentPage = 1;
            Filter.PageIndex = 1;

            await FilterData();
        }

        private async Task FilterData()
        {
            var httpClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

            try
            {
                var logsFilterRequest = new UploadFilter()
                {
                    RecordsPerPage = Convert.ToInt32(Filter.RecordsPerPageSelected),
                    PageIndex = Filter.PageIndex,
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteGetFilteredSoiLog, logsFilterRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResult<Ofgem.LAF.SharedLibrary.Models.StatementOfIntentLog>>();

                    if (result != null)
                    {
                        StatementOfIntentLogs = new List<StatementOfIntentLog>();
                        foreach (var item in result.Results)
                        {
                            var soiLog = new StatementOfIntentLog
                            {
                                Comment = (item.Comment ?? "").Replace(@"\r\n", @"<\br>"),
                                LocalAuthorityName = item.LocalAuthorityName ?? "",
                                CreatedDate = LafLocalTimezone.ToLocalTime(item.CreatedDate ?? DateTime.Today),
                                CreatedByName = item.CreatedByName ?? "",
                            };

                            StatementOfIntentLogs.Add(soiLog);
                        }

                        CreatePagination(result.RowCount, result.CurrentPage, result.PageSize, result.PageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "ListStatementOfIntents - ListData, Error");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private void CreatePagination(int rowCount, int currentPage, int pageSize, int resultPageCount)
        {
            Filter.PageIndex = currentPage;
            Filter.RecordsPerPageSelected = pageSize.ToString();

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }
    }
}
