using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
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
    public class LogsModel(ILogger<LogsModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : PageModel
    {

        [BindProperty] public Extensions.Permissions.Logs Permissions { get; set; } = new Extensions.Permissions.Logs();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public List<Upload> Uploads { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<StatementOfIntentLog> StatementOfIntentLogs { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.ListUploadsFilter Filter { get; set; }

        public async Task OnGet()
        {
            logger.LogInformation("Logs - OnGet");

            Permissions = new Extensions.Permissions.Logs(httpContextAccessor, httpClientFactory, TempData);

            await FilterData();
        }

        private async Task FilterData()
        {
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var filterRequest = new UploadFilter()
                {
                    RecordsPerPage = 3,
                    PageIndex = 1,
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteUploadList, filterRequest);

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
                                Comments = (item.Comments ?? "").Replace(@"\r\n", @"<\br>"),
                                LocalAuthority = item.LocalAuthority ?? "",
                                State = item.Status,
                                When = LafLocalTimezone.ToLocalTime(item.CreatedDate ?? DateTime.Today),
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

                    }
                }


                var httpSoiLogClient = httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpSoiLogResponseMessage =
                    await httpSoiLogClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteGetFilteredSoiLog, filterRequest);

                if (httpSoiLogResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpSoiLogResponseMessage.Content.ReadFromJsonAsync<PagedResult<Ofgem.LAF.SharedLibrary.Models.StatementOfIntentLog>>();

                    if (result != null)
                    {
                        StatementOfIntentLogs = new List<StatementOfIntentLog>();
                        foreach (var item in result.Results)
                        {
                            var soiLog = new StatementOfIntentLog
                            {
                                CreatedDate = LafLocalTimezone.ToLocalTime(item.CreatedDate ?? DateTime.Today),
                                CreatedByName = item.CreatedByName ?? "",
                                LocalAuthorityName = item.LocalAuthorityName ?? "",
                                Comment = (item.Comment ?? "").Replace(@"\r\n", @"<\br>"),
                            };

                            StatementOfIntentLogs.Add(soiLog);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "ListUploads - FilterData, Error");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}
