using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using Ofgem_Web_LAF_InternalPortal.Services;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Uploads;

[AutoValidateAntiforgeryToken]
[AuthorizeRoles(
    UserRoles.Advanced,
    UserRoles.Admin,
    UserRoles.Expert,
    UserRoles.Standard,
    UserRoles.Basic
)]
public class ResolveUploadsModel : PageModel
{
    private readonly IDeclarationManagementService _declarationManagementService;
    private readonly ILogger<ResolveUploadsModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;



    [BindProperty] public List<Upload>? UploadsAwaitingDecision { get; set; }

    [BindProperty] public bool ShowNotification { get; set; }

    [BindProperty] public string DisplayMessage { get; set; }

    [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

    [BindProperty] public Extensions.Permissions.ResolveUploads Permissions { get; set; }

    public ResolveUploadsModel(
        IDeclarationManagementService declarationManagementService,
        ILogger<ResolveUploadsModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _declarationManagementService = declarationManagementService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;

        DisplayMessage = string.Empty;
        Permissions = new Extensions.Permissions.ResolveUploads();
    }

    public async Task OnGet()
    {
        _logger.LogInformation("ResolveUploads - OnGet");

        Permissions = new Extensions.Permissions.ResolveUploads(_httpContextAccessor, _httpClientFactory,TempData);

        await GetUploadAwaitingDecisionStatusNotification();
    }

    public async Task<IActionResult> OnGetResolveUpload(int uploadId)
    {
        _logger.LogInformation("ResolveUploads - OnGetResolveUpload");

        Permissions = new Extensions.Permissions.ResolveUploads(_httpContextAccessor, _httpClientFactory, TempData);

        // delete the original upload 
        await DeleteUpload(uploadId);

        List<Upload> uploadsAwaiting =TempData.Get<List<Upload>>(TempDataKeys.UploadsAwaitingDecision);
        Upload upload = uploadsAwaiting.Single(x => x.UploadId == uploadId);
        // go back to the file upload page and use the validate method there to call the validation on the api
        return RedirectToPage(LafPages.UploadTemplate.ROUTE, LafPages.UploadTemplate.METHOD_VALIDATE, new
        {
            fileContainer = upload.DocumentContainerId,
            fileName = upload.DocumentId
        });
    }

    public Task<IActionResult> OnGetDeleteUpload(int uploadId)
    {
        _logger.LogInformation("ResolveUploads - OnPostDeleteUpload");

        Permissions = new Extensions.Permissions.ResolveUploads(_httpContextAccessor, _httpClientFactory, TempData);
        List<Upload> uploadsAwaiting = TempData.Get<List<Upload>>(TempDataKeys.UploadsAwaitingDecision);
        Upload upload = uploadsAwaiting.Single(x => x.UploadId == uploadId);
        TempData.Put(TempDataKeys.UploadToDelete, upload);

        return Task.FromResult<IActionResult>(RedirectToPage(LafPages.DeleteUpload.ROUTE));
    }

    private async Task<IActionResult> DeleteUpload(int uploadId)
    {
        var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);
        try
        {
            _ = await httpClient.DeleteAsync($"{Services.DeclarationApi.RouteUploadDelete}/{uploadId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            _logger.LogError(ex, "ResolveUploads - DeleteUpload");
            DisplayMessage = "An issue occurred deleting an upload.";
        }

        return RedirectToPage(LafPages.ResolveUploads.ROUTE);
    }


    private async Task GetUploadAwaitingDecisionStatusNotification()
    {
        try
        {
                var result = await _declarationManagementService.GetUploadsAwaitingADecision();
                if (result is { Count: > 0 })
                {
                    UploadsAwaitingDecision = result;
                    TempData.Put(TempDataKeys.UploadsAwaitingDecision, UploadsAwaitingDecision);
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            _logger.LogError(ex, "ResolveUploadsModel - GetUploadAwaitingDecisionStatusNotification");
            DisplayMessage = "An issue occurred retrieving uploads awaiting decision status.";
        }
    }
}