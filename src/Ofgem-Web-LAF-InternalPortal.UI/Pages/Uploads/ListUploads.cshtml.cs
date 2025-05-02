using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;

using Upload = Ofgem_Web_LAF_InternalPortal.Models.Upload;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Uploads
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard
        )]
    public class ListUploadsModel : PageModel
    {

        private readonly ILogger<ListUploadsModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty(SupportsGet = true)]
        public Models.ListUploadsFilter Filter { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<Upload> Uploads { get; set; }

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public string DisplayMessage { get; set; }

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.ListUploads Permissions { get; set; }

        [BindProperty] public Models.PagePagination Pagination { get; set; }
        [BindProperty] public int CurrentPage { get; set; }


        public ListUploadsModel(ILogger<ListUploadsModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            DisplayMessage = string.Empty;
            Filter = new Models.ListUploadsFilter();
            Permissions = new Extensions.Permissions.ListUploads();
            Pagination = new Models.PagePagination();
            Uploads = new List<Upload>();
        }

        public async Task OnGet()
        {
            _logger.LogInformation("ListUploads - OnGet");

            Permissions = new Extensions.Permissions.ListUploads(_httpContextAccessor, _httpClientFactory,TempData);

            await RefreshData();
        }

        public async Task OnGetSuccessfulCreate(string name)
        {
            _logger.LogInformation("ListUploads - OnPostSuccessfulCreate");
            Permissions = new Extensions.Permissions.ListUploads(_httpContextAccessor, _httpClientFactory, TempData);

            DisplayMessage = $"New profile for {name} successfully created.";

            ShowNotification = true;

            await RefreshData();
        }

        public async Task OnPostApplyPagination(string id)
        {
            Permissions = new Extensions.Permissions.ListUploads(_httpContextAccessor, _httpClientFactory, TempData);

            SetPageIndexes(id);
            await FilterData();
        }

        public async Task<IActionResult> OnGetDownloadErrors(int uploadId, string localAuthority)
        {
            _logger.LogInformation("Upload Log - OnGetDownloadErrors");

            var result = await GetDownloadData(uploadId);

            var ms = new MemoryStream();

            using (var parser = new ChoCSVWriter<RawDeclarationWithErrors>(ms))
            {
                parser.Write(result);
            }

            ms.Position = 0; //reset stream
            return File(ms, "text/csv", $"Upload_error_{localAuthority}_{DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_FORMAT_DOWNLOAD)}.csv");
        }

        private async Task<List<RawDeclarationWithErrors>> GetDownloadData(int uploadId)
        {
            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage =
                    await httpClient.GetAsync($"{Services.DeclarationApi.RouteUploadDownloadErrors}/{uploadId}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    List<RawDeclarationWithErrors>? result = await httpResponseMessage.Content.ReadFromJsonAsync<List<RawDeclarationWithErrors>>();

                    if (result == null)
                    {
                        DisplayMessage = "Failed to down load";
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "Upload Log - OnGetDownloadErrors");
                DisplayMessage = "An issue occurred retrieving data";
            }

            return new List<RawDeclarationWithErrors>();
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
            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var profilesFilterRequest = new UploadFilter()
                {
                    RecordsPerPage = Convert.ToInt32(Filter.RecordsPerPageSelected),
                    PageIndex = Filter.PageIndex,
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteUploadList, profilesFilterRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResult<Ofgem.LAF.SharedLibrary.Models.Upload>>();

                    if (result != null)
                    {
                        Uploads = new List<Upload>();
                        foreach (var item in result.Results)
                        {
                            var upload = new Upload
                            {
                                Comments = (item.Comments??"").Replace(@"\r\n",@"<\br>"),
                                LocalAuthority = item.LocalAuthority ?? "",
                                State = item.Status,
                                When = LafLocalTimezone.ToLocalTime(item.CreatedDate??DateTime.Today),
                                Who = item.CreatedByName ?? "",
                                UploadId = item.UploadId
                            };
                            if (item.FailedCoreRulesCount > 0 || item.FailedSoiRulesCount > 0)
                            {
                                upload.HasErrors = true;
                            }
                            else
                            {
                                upload.HasErrors = false;
                            }
                            Uploads.Add(upload);
                        }

                        CreatePagination(result.RowCount, result.CurrentPage, result.PageSize, result.PageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "ListUploads - FilterData, Error");
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